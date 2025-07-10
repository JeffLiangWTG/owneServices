//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.BatchProcessor;
	using Enterprise.Customs.Business;
	using Enterprise.Customs.Business.MessageInterpretation;
	using Enterprise.Customs.Business.MessageProcessors;
	using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
	using Enterprise.Customs.CA.Business.MessageManagers;
	using Enterprise.Customs.CA.Messaging;
	using Enterprise.Customs.CA.Registry;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Customs.Common.Shared;
	using Enterprise.Edifact.D11B.Messages.GOVCBR;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Messaging.Integration;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.Schema;

	[ApplicationIdentifier(ServiceOptions.Codes.ACKResponse)]
	[ApplicationIdentifier(ServiceOptions.Codes.AppError)]
	[ApplicationIdentifier(ServiceOptions.Codes.GenResponse)]
	[ApplicationIdentifier(ServiceOptions.Codes.DocResponse)]
	[ApplicationIdentifier(ServiceOptions.Codes.StatusInfo)]
	public class EManifestResponse : ResponseMessageProcessor
	{
		public EManifestResponse(LoggingInformation logger)
			: base(logger, x => GetMessageType(x), Res.GetString("1f7c088a-d544-4513-b3e8-4aa8ce3e9883", "eManifest Forwarding Response"))
		{
		}

		static string GetMessageType(Enterprise.Messaging.Business.EDIMessage message)
		{
			if (message.EM_MessageText.Contains("RFF+AGO:CLS-"))
			{
				return MessageTypeList.Codes.ACIForwarderClose;
			}

			return MessageTypeList.Codes.ACIHouseBill;
		}

		protected override string DoProcessingReturningStatus(Enterprise.Messaging.Business.EDIMessage ediMessage)
		{
			var resultStatus = ZString.Empty;
			message = ediMessage.Factory.Load<ACIForwarderMessage>(ediMessage.PK);
			if (message != null)
			{
				wrapper = new EManifestResponseWrapper(message);
				govcbrMessage = message.GOVCBR;
				if (govcbrMessage != null)
				{
					docReference = wrapper.DocumentReference;
					ZString messageReference = GetMessageReferenceLinkedObjectAndSetOnMessage(wrapper);
					if (linkedObject != null)
					{
						using (DisposableEnvironment.ForBranch(GetMessageBranchAndSetOnMessage(linkedObject, message).PK.ToGuid()))
						{
							var house = linkedObject as CusCAeMHHouse;
							var master = house != null ? house.MasterBill : linkedObject as CusCAeMHMaster;
							var isAllHouseBillsAcceptedBefore = master?.ReadyToClose ?? ZBool.False;
							var oldStatus = linkedObject.JobStatus;

							message.EM_MessageSubType = StatusCalculator.GetMessageSubType(linkedObject.MessageStatus);
							if (wrapper.IsMessageReceived) // 17
							{
								if (StatusCalculator.IsAwaitingReply(linkedObject.MessageStatus))
								{
									linkedObject.MessageStatus = StatusCalculator.GetMessageAcknowledgedStatus(message);
								}
								resultStatus = EDIMessage.Status.Acknowledged;
							}
							else if (wrapper.IsAccepted) // 1 or 66
							{
								linkedObject.MessageStatus = StatusCalculator.GetMessageClearedStatus(message);
								SendAcknowledgementReport(EmailResponseLinkedObject, GetAcceptedResponseEmailAndSetOnMessage());
								linkedObject.JobStatus = StatusCalculator.CalculatedJobStatus(linkedObject);
								UpdateHouseBillIsCloseReportedIfNeeded();
								resultStatus = EDIMessage.Status.Received;
							}
							else if (wrapper.IsRejected) // 14 or 2
							{
								SendErrorReport(EmailResponseLinkedObject, GetErrorResponseEmailAndSetOnMessage());
								linkedObject.MessageStatus = StatusCalculator.GetMessageRejectedStatus(message);
								linkedObject.JobStatus = StatusCalculator.CalculatedJobStatus(linkedObject);
								resultStatus = EDIMessage.Status.Received;
							}
							else if (wrapper.IsMatchedNotice) // completeness matched
							{
								SendAcknowledgementReport(EmailResponseLinkedObject, GetMatchedResponseEmailAndSetOnMessage());
								if (!(linkedObject is CusEntryHeader))
								{
									linkedObject.MessageStatus = StatusCalculator.GetMessageClearedStatus(message);
									linkedObject.JobStatus = StatusCalculator.CalculatedJobStatus(linkedObject);
								}
								resultStatus = EDIMessage.Status.Received;
							}
							else if (wrapper.IsNOTMatchedNotice) // completeness NOT matched
							{
								SendErrorReport(EmailResponseLinkedObject, GetNotMatchedResponseEmailAndSetOnMessage());
								if (!(linkedObject is CusEntryHeader))
								{
									linkedObject.MessageStatus = StatusCalculator.GetMessageClearedStatus(message);
									linkedObject.JobStatus = StatusCalculator.CalculatedJobStatus(linkedObject);
								}
								resultStatus = EDIMessage.Status.Received;
							}

							if (ShouldGenerateAutoCloseReport(master, linkedObject, isAllHouseBillsAcceptedBefore, oldStatus))
							{
								var (isError, logText) = GenerateAutoCloseReport(master);

								if (isError)
								{
									Logger.LogError(logText);
								}
								else
								{
									Logger.Log(logText);
								}
							}
						}
					}
					else
					{
						throw new CouldNotFindLinkedObjectException(messageReference, ediMessage, this);
					}
				}
			}

			if (resultStatus.IsEmpty)
			{
				throw new UnableToInterpretMessageException(ediMessage, this);
			}

			return resultStatus;
		}

		ZBool IsNeedAutoCloseReport(CusCAeMHMaster master)
		{
			return message != null && message.EM_MessageSubType == MessageSubTypeCodes.Codes.Cancellation &&
				master != null && master.Messages.OfType<ACIForwarderCloseMessage>()
				.Any(closeMessage => closeMessage.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit && closeMessage.EM_MessageSubType == MessageSubTypeCodes.Codes.Cancellation
						&& closeMessage.NeedAutoCloseReport);
		}

		internal bool ShouldGenerateAutoCloseReport(CusCAeMHMaster master, IEDIFACTMessageAttachee linkedObject, ZBool isAllHouseBillsAcceptedBefore, ZString oldStatus)
		{
			var result = false;

			if (CACustomsDataRegistry.Instance.AutoSendCloseMessage.Value)
			{
				var isAllHouseBillsAcceptedAfter = master?.ReadyToClose ?? ZBool.False;
				var newStatus = linkedObject.JobStatus;
				if (!isAllHouseBillsAcceptedBefore && newStatus != oldStatus && isAllHouseBillsAcceptedAfter || newStatus == EManifestForwarderJobStatusList.Codes.Cancelled && IsNeedAutoCloseReport(master))
				{
					result = true;
				}
			}

			return result;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We accept that may fail and we do nothing but reporting the failure")]
		internal (ZBool isError, ZString logText) GenerateAutoCloseReport(CusCAeMHMaster master)
		{
			try
			{
				var closeWrapper = new CusCAeMHMasterCloseWrapper(master);
				closeWrapper.UpdateIsShouldSend(house => house.IsAccepted);
				var manager = new ACIForwarderCloseMessageManager(closeWrapper, null, true);
				manager.GenerateOriginalMessages(manager.BusinessObject);
				manager.OnOriginalSent(true);
				var logText = ZString.Format("The auto close report messages have been generated for {0}", master.BP_MessageReference);
				return (false, logText);
			}
			catch (Exception e)
			{
				var logText = ZString.Format("The auto close report messages have failed to be generated for {0} due to the following error : {1}", master.BP_MessageReference, e.Message);
				return (true, logText);
			}
		}

		ZString GetMessageReferenceLinkedObjectAndSetOnMessage(EManifestResponseWrapper wrapper)
		{
			linkedObject = wrapper.LinkedObject;
			if (linkedObject != null)
			{
				linkedObject.AddMessage(message);
			}
			return wrapper.OriginalMessageReference;
		}

		EDIFACTMessageStatusCalculator StatusCalculator
		{
			get { return statusCalculator ?? (statusCalculator = new ACIEManifestForwaderStatusCalculator(wrapper, MessageTypeDescription)); }
		}
		EDIFACTMessageStatusCalculator statusCalculator;

		ZString MessageTypeDescription
		{
			get { return wrapper.OriginalMessageReference.StartsWith(CusCAeMHMaster.JobIdentificationPrefix) ? MessageTypeList.Descriptions.ACIForwarderClose : MessageTypeList.Descriptions.ACIHouseBill; }
		}

		void UpdateHouseBillIsCloseReportedIfNeeded()
		{
			if (message.EM_MessageType == MessageTypeList.Codes.ACIForwarderClose && wrapper.DocumentName == ServiceOptions.Codes.ACKResponse)
			{
				var linkedObjectMessages = linkedObject.Messages;
				var messageSubType = message.EM_MessageSubType;
				IEnumerable<string> relatedCCNs = null;
				if (linkedObjectMessages.Count > 0)
				{
					ACIForwarderCloseMessage latestOutgoingMessage = null;
					latestOutgoingMessage = linkedObjectMessages.OfType<ACIForwarderCloseMessage>()
						.Where(linkedMessage => linkedMessage.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit && linkedMessage.EM_MessageSubType == messageSubType)
						.OrderByDescending(closeMessage => closeMessage.EM_SystemCreateTimeUtc).FirstOrDefault();
					if (latestOutgoingMessage != null)
					{
						relatedCCNs = latestOutgoingMessage.GOVCBR.Group9.Cast<SegmentGroup9>().Select(seg => seg.DOC.Count > 0 ? seg.DOC[0].DocumentMessageDetails.DocumentIdentifier : string.Empty).Where(docCCN => !string.IsNullOrEmpty(docCCN));
					}
				}
				switch (messageSubType)
				{
					case MessageSubTypeCodes.Codes.Original:
					case MessageSubTypeCodes.Codes.Change:
						UpdateHouseBillIsClosedReportedStatus(relatedCCNs, false);
						break;
					case MessageSubTypeCodes.Codes.Cancellation:
						UpdateHouseBillIsClosedReportedStatus(null, true);
						break;
				}
			}
		}

		void UpdateHouseBillIsClosedReportedStatus(IEnumerable<string> relatedCCNs, bool cancelAllIfEmptyList)
		{
			var isRelatedCCNSEmpty = relatedCCNs == null || !relatedCCNs.Any();
			var masterBill = linkedObject.TopLevelBusinessObject as CusCAeMHMaster;
			if (masterBill != null)
			{
				if (!isRelatedCCNSEmpty)
				{
					masterBill.HouseBills.ForEach(houseBill =>
					{
						var houseCCNInMessage = houseBill.BW_HouseCCN.Replace(" ", "").ToString();
						houseBill.BW_IsCloseReported = relatedCCNs.Contains(houseCCNInMessage);
					});
				}
				else if (cancelAllIfEmptyList)
				{
					masterBill.HouseBills.ForEach(houseBill =>
					{
						houseBill.BW_IsCloseReported = false;
					});
				}
			}
		}

		#region Implementation

		#region Response Emails

		EmailDef GetAcceptedResponseEmailAndSetOnMessage()
		{
			var subject = message.EM_MessageSubType == MessageSubTypeCodes.Codes.Cancellation
											? Res.GetString("DABDCD87-517D-458F-93A2-AE5B1B19E193", "Cancellation accepted {0} Response for {1}",
																			StatusCalculator.MessageTypeDescription, docReference)
											: Res.GetString("54CAE41D-30D7-4446-9729-E4CCB3F88AFE", "Message content accepted {0} Response for {1}",
																			StatusCalculator.MessageTypeDescription, docReference);
			return GetResponseEmailAndSetOnMessage(subject, EmailDefBuilder.HtmlTemplates.ValidatedResponse);
		}

		EmailDef GetMatchedResponseEmailAndSetOnMessage()
		{
			var subject = Res.GetString("D8746D97-9CBC-4A85-B20A-FD63AC9D0663", "Matched {0} Response for {1}",
																	StatusCalculator.MessageTypeDescription, docReference);
			return GetResponseEmailAndSetOnMessage(subject, EmailDefBuilder.HtmlTemplates.MatchedResponse);
		}

		EmailDef GetErrorResponseEmailAndSetOnMessage()
		{
			var subject = Res.GetString("807CF97C-AA21-4C0D-90E9-28CB9943F935", "Error {0} Response for {1}",
				StatusCalculator.MessageTypeDescription, docReference);
			return GetResponseEmailAndSetOnMessage(subject, EmailDefBuilder.HtmlTemplates.ErrorResponse);
		}

		EmailDef GetNotMatchedResponseEmailAndSetOnMessage()
		{
			var subject = Res.GetString("6B2A73D9-B180-4C52-A81B-2DB9FDE8E2BA", "NOT Matched {0} Response for {1}",
																	StatusCalculator.MessageTypeDescription, docReference);
			return GetResponseEmailAndSetOnMessage(subject, EmailDefBuilder.HtmlTemplates.NotMatchedResponse);
		}

		#region Response Email Implementation

		EmailDef GetResponseEmailAndSetOnMessage(string subject, string templateName)
		{
			string sourceMessage;
			var emailBuilder = new EmailDefBuilder(subject, message.EM_FormattedMessageText, templateName);
			emailBuilder.AddArgReplacementRange(JobNumberLink, docReference, StatusCalculator.MessageTypeDescription);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.FromMessageSender, MessageSender);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, GetHeaderHtmlText(wrapper));
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml2, GetNotificationsHtmlText(wrapper, out sourceMessage));
			if (!string.IsNullOrEmpty(sourceMessage))
			{
				emailBuilder.AddAttachment("SourceMessageWithErrorMarks.txt", sourceMessage);
			}

			message.EM_MessageInterpretation = emailBuilder.ToString();
			return emailBuilder.ToEmail();
		}

		static string GetHeaderHtmlText(EManifestResponseWrapper wrapper)
		{
			var commentsTable = new FieldValueTableInterpretation();
			commentsTable.AddIfNotEmpty(() => wrapper.ProcessingDate);
			foreach (var comment in wrapper.ErrorComments)
			{
				commentsTable.AddIfNotEmpty(() => (ZString)comment);
			}
			return commentsTable.ToHtml();
		}

		string GetNotificationsHtmlText(EManifestResponseWrapper wrapper, out string sourceMessageWithErrorMarks)
		{
			if (wrapper.IsSyntaxError)
			{
				message.EM_MessageSubType = message.EM_MessageType;
				message.EM_MessageType = MessageTypeList.Codes.SyntaxError;
				var hasLinkedObject = new ZBool(linkedObject != null);
				var originalMessage = SyntaxErrorMessage.GetOriginalMessage(message);
				var requestMessageText = originalMessage?.EM_MessageText ?? ZString.Empty;
				var hasSentMessage = new ZBool(originalMessage != null);
				var isSentWithMessageErrors = originalMessage?.EM_SendWithMessageErrors ?? ZBool.False;
				if (!hasLinkedObject || !hasSentMessage || !isSentWithMessageErrors)
				{
					var msg = govcbrMessage.ToString(new CACharSet());
					if (!requestMessageText.IsEmpty)
					{
						var requestInterChange = originalMessage.Interchange;
						var responseInterChange = message.Interchange;
						msg = ZString.Format(@"Can response message's linked object be found? '{0}';
Can last request message be found? '{1}';
Did last request message sent with message errors? '{2}';
Request message eHub Tracking ID: '{3}';
Response message eHub Tracking ID: '{4}';
<Request>{5}</Request><Response>{6}</Response>;
<RequestInterChange>{7}</RequestInterChange><ResponseInterChange>{8}</ResponseInterChange>",
hasLinkedObject, hasSentMessage, isSentWithMessageErrors, requestInterChange?.eHubID ?? ZString.Empty, responseInterChange?.eHubID ?? ZString.Empty, requestMessageText, msg,
requestInterChange?.EI_HeaderText ?? ZString.Empty, responseInterChange?.EI_HeaderText ?? ZString.Empty);
					}
					ErrorReporter.ReportOnce("ACI eManifest Forwarding message syntax error", msg);
				}
				var data = SyntaxErrorMessage.GetSyntaxErrorData(message, requestMessageText);
				sourceMessageWithErrorMarks = data.sourceMessageTextWithErrorMarks;
				return TableInterpretation.GetTableInterpretation(data.syntaxErrors, null, TableInterpretation.Attributes.AlignLeft);
			}
			sourceMessageWithErrorMarks = null;
			return TableInterpretation.GetTableInterpretation(wrapper.Notifications, null, TableInterpretation.Attributes.AlignLeft);
		}

		protected override GlbStaff GetUserToNotify(IBusiness parent)
		{
			var result = base.GetUserToNotify(parent);

			if (result == null && parent is CusCAeMHMaster master)
			{
				var latestHBMessage = master.HouseBills.SelectMany(h => h.Messages).Cast<EDIMessage>().Where(m => m.EM_SystemCreateUser != User.ServiceUserCode && m.EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
				.OrderByDescending(m => m.EM_SystemCreateTimeUtc).FirstOrDefault();

				if (latestHBMessage != null)
				{
					result = parent.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, latestHBMessage.EM_SystemCreateUser);
				}
			}

			return result;
		}

		#endregion

		#endregion

		#region Overridden Properties

		protected override ZGuid AcknowledgementEmailGroup
		{
			get
			{
				return linkedObject is CusEntryHeader ?
					CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgementsToGroup.Value :
					CACustomsDataRegistry.Instance.SendeManifestForwarderMessageAcknowledgementsToGroupAppliesAllCountries.Value;
			}
		}

		protected override ZString AcknowledgementEmailMode
		{
			get
			{
				return linkedObject is CusEntryHeader ?
					CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgements.Value :
					CACustomsDataRegistry.Instance.SendeManifestForwarderMessageAcknowledgementsAppliesAllCountries.Value;
			}
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get
			{
				return linkedObject is CusEntryHeader ?
					CACustomsDataRegistry.Instance.SendDeclarationMessageErrorsToGroup.Value :
					CACustomsDataRegistry.Instance.SendeManifestForwarderMessageErrorsToGroupAppliesAllCountries.Value;
			}
		}

		protected override ZString ImpedimentEmailMode
		{
			get
			{
				return linkedObject is CusEntryHeader ?
					CACustomsDataRegistry.Instance.SendDeclarationMessageErrors.Value :
					CACustomsDataRegistry.Instance.SendeManifestForwarderMessageErrorsAppliesAllCountries.Value;
			}
		}

		protected override ZGuid ErrorEmailGroup
		{
			get
			{
				return linkedObject is CusEntryHeader ?
					CACustomsDataRegistry.Instance.SendDeclarationMessageErrorsToGroup.Value :
					CACustomsDataRegistry.Instance.SendeManifestForwarderMessageErrorsToGroupAppliesAllCountries.Value;
			}
		}

		protected override ZString ErrorEmailMode
		{
			get
			{
				return linkedObject is CusEntryHeader ?
					CACustomsDataRegistry.Instance.SendDeclarationMessageErrors.Value :
					CACustomsDataRegistry.Instance.SendeManifestForwarderMessageErrorsAppliesAllCountries.Value;
			}
		}

		#endregion

		ZString JobNumberLink
		{
			get
			{
				if (linkedObject is CusCAeMHHouse)
				{
					var obj = ((CusCAeMHHouse)linkedObject).MasterBill;
					if (obj != null)
					{
						return EmailDefBuilder.GetJobLink(obj, obj.BP_MessageReference);
					}
				}
				else if (linkedObject is CusCAeMHMaster)
				{
					var obj = linkedObject as CusCAeMHMaster;
					return EmailDefBuilder.GetJobLink(obj, obj.BP_MessageReference);
				}
				else if (linkedObject is CusEntryHeader)
				{
					var obj = ((CusEntryHeader)linkedObject).Declaration;
					if (obj != null)
					{
						return EmailDefBuilder.GetJobLink(obj, obj.JE_DeclarationReference);
					}
				}
				return ZString.Empty;
			}
		}

		protected override BusinessObject EmailResponseLinkedObject
		{
			get { return linkedObject != null ? linkedObject.Messages.Master : null; }
		}

		EManifestResponseWrapper wrapper;
		ACIForwarderMessage message;
		IEDIFACTMessageAttachee linkedObject;
		GOVCBRMessage govcbrMessage;
		ZString docReference;

		#endregion
	}
}
