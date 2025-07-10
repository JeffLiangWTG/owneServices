//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	using CargoWise.Common;
	using CargoWise.Types;
	using Enterprise.BatchProcessor;
	using Enterprise.Customs.Business.MessageInterpretation;
	using Enterprise.Customs.Business.MessageProcessors;
	using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
	using Enterprise.Customs.CA.Messaging;
	using Enterprise.Customs.CA.Registry;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Customs.Common.Shared;
	using Enterprise.Edifact.D00A.Elements;
	using Enterprise.Edifact.D00A.Messages.CUSRES;
	using Enterprise.Environment;
	using Enterprise.ZArchitecture.Environment;

	public abstract class ACIResponseMessageProcessorBase : ResponseMessageProcessor
	{
		protected ACIResponseMessageProcessorBase(LoggingInformation logger, ZString messageTypeCode, ZString messageTypeDescription)
			: base(logger, messageTypeCode, messageTypeDescription)
		{
		}

		internal string DoProcessingReturningStatusForGenericSyntaxError(Enterprise.Messaging.Business.EDIMessage ediMessage)
		{
			return DoProcessingReturningStatus(ediMessage);
		}

		protected override string DoProcessingReturningStatus(Enterprise.Messaging.Business.EDIMessage ediMessage)
		{
			var resultStatus = ZString.Empty;
			message = (ACIEDIMessage)ediMessage;
			cusresMessage = (CUSRESMessage)ediMessage.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet());
			if (cusresMessage != null)
			{
				ZString objectReference = cusresMessage.BGM[0].DocumentMessageIdentification.DocumentIdentifier;
				objectReferenceFormatted = objectReference.SubstringSafe(0, 4) + " " + objectReference.SubstringSafe(4);

				linkedObject = GetLinkedObjectAndSetOnMessage(objectReference);
				if (linkedObject != null)
				{
					message.EM_MessageSubType = StatusCalculator.GetMessageSubType(linkedObject.MessageStatus);
					if (cusresMessage.GIS.Count > 0)
					{
						using (DisposableEnvironment.ForBranch(GetMessageBranchAndSetOnMessage(linkedObject, message).PK.ToGuid()))
						{
							var processingIndicator = cusresMessage.GIS[0].ProcessingIndicator_X.ProcessingIndicatorDescriptionCode;
							if (processingIndicator == ProcessingIndicatorDescriptionCodeList.MessageReceived) // 17
							{
								if (StatusCalculator.IsAwaitingReply(linkedObject.MessageStatus))
								{
									linkedObject.MessageStatus = StatusCalculator.GetMessageAcknowledgedStatus(message);
								}
								resultStatus = EDIMessage.Status.Acknowledged;
							}
							else if (processingIndicator == ProcessingIndicatorDescriptionCodeList.MessageContentAccepted) // 1
							{
								linkedObject.MessageStatus = StatusCalculator.GetMessageClearedStatus(message);
								SendAcknowledgementReport(EmailResponseLinkedObject, GetAcceptedResponseEmailAndSetOnMessage());
								linkedObject.JobStatus = StatusCalculator.CalculatedJobStatus(linkedObject);
								resultStatus = EDIMessage.Status.Received;
							}
							else if (processingIndicator == ProcessingIndicatorDescriptionCodeList.TransactionAccepted) // 32
							{
								linkedObject.MessageStatus = StatusCalculator.GetMessageClearedStatus(message);
								SendAcknowledgementReport(EmailResponseLinkedObject, GetMatchedResponseEmailAndSetOnMessage());
								linkedObject.JobStatus = StatusCalculator.CalculatedJobStatus(linkedObject);
								resultStatus = EDIMessage.Status.Received;
							}
							else if (processingIndicator == ProcessingIndicatorDescriptionCodeList.TransactionRejected) // 33
							{
								SendErrorReport(EmailResponseLinkedObject, GetNotMatchedResponseEmailAndSetOnMessage());
								linkedObject.MessageStatus = StatusCalculator.GetMessageClearedStatus(message);
								linkedObject.JobStatus = StatusCalculator.CalculatedJobStatus(linkedObject);
								resultStatus = EDIMessage.Status.Received;
							}
							else if (processingIndicator == ProcessingIndicatorDescriptionCodeList.ErrorMessage) // 14
							{
								SendErrorReport(EmailResponseLinkedObject, GetErrorResponseEmailAndSetOnMessage());
								linkedObject.MessageStatus = StatusCalculator.GetMessageRejectedStatus(message);
								linkedObject.JobStatus = StatusCalculator.CalculatedJobStatus(linkedObject);
								resultStatus = EDIMessage.Status.Received;
							}
							else if (processingIndicator == ProcessingIndicatorDescriptionCodeList.ProhibitedRestrictedGoods) //25
							{
								linkedObject.MessageStatus = StatusCalculator.GetMessageClearedStatus(message);
								SendImpedimentReport(EmailResponseLinkedObject, GetRiskAssessmentResponseEmailAndSetOnMessage());
								linkedObject.JobStatus = StatusCalculator.CalculatedJobStatus(linkedObject);
								resultStatus = EDIMessage.Status.Received;
							}
						}
					}
				}
				else
				{
					throw new CouldNotFindLinkedObjectException(objectReference, ediMessage, this);
				}
			}

			if (resultStatus.IsEmpty)
			{
				throw new UnableToInterpretMessageException(ediMessage, this);
			}

			return resultStatus;
		}

		#region Implementation

		#region Response Emails

		EmailDef GetAcceptedResponseEmailAndSetOnMessage()
		{
			var subject = message.EM_MessageSubType == MessageSubTypeCodes.Codes.Cancellation
							? Res.GetString("c0dfa1a9-1610-46c5-abc2-0465ca65670f", "Cancellation accepted {0} Response for {1}",
											StatusCalculator.MessageTypeDescription, objectReferenceFormatted)
							: Res.GetString("761838a1-982c-4723-a26b-f3426838ed73", "Validated {0} Response for {1}",
											StatusCalculator.MessageTypeDescription, objectReferenceFormatted);

			return GetResponseEmailAndSetOnMessage(subject, EmailDefBuilder.HtmlTemplates.ValidatedResponse);
		}

		EmailDef GetMatchedResponseEmailAndSetOnMessage()
		{
			var subject = Res.GetString("8c453af8-5ad1-4abd-8cea-5b85d5764264", "Matched {0} Response for {1}",
										StatusCalculator.MessageTypeDescription, objectReferenceFormatted);
			return GetResponseEmailAndSetOnMessage(subject, EmailDefBuilder.HtmlTemplates.MatchedResponse);
		}

		EmailDef GetErrorResponseEmailAndSetOnMessage()
		{
			var subject = Res.GetString("76b3fefd-ccd9-41bb-a409-71f7dd3f303b", "Error {0} Response for {1}",
				StatusCalculator.MessageTypeDescription, objectReferenceFormatted);
			return GetResponseEmailAndSetOnMessage(subject, EmailDefBuilder.HtmlTemplates.ErrorResponse);
		}

		EmailDef GetNotMatchedResponseEmailAndSetOnMessage()
		{
			var subject = Res.GetString("ff601948-3c09-4b9a-a521-eb66f6a5a6fd", "NOT Matched {0} Response for {1}",
										StatusCalculator.MessageTypeDescription, objectReferenceFormatted);
			return GetResponseEmailAndSetOnMessage(subject, EmailDefBuilder.HtmlTemplates.NotMatchedResponse);
		}

		EmailDef GetRiskAssessmentResponseEmailAndSetOnMessage()
		{
			var subject = Res.GetString("ed61bb1a-72c4-4d44-a054-556ed088abe3", "Risk Assessment {0} Response for {1}",
										StatusCalculator.MessageTypeDescription, objectReferenceFormatted);
			return GetResponseEmailAndSetOnMessage(subject, EmailDefBuilder.HtmlTemplates.RiskAssessmentACIResponse);
		}

		#region Response Email Implementation

		EmailDef GetResponseEmailAndSetOnMessage(string subject, string templateName)
		{
			string sourceMessage;
			var wrapper = new ACIResponseMessageWrapper(message);
			var emailBuilder = new EmailDefBuilder(subject, message.EM_FormattedMessageText, templateName);
			emailBuilder.AddArgReplacementRange(JobNumberLink, objectReferenceFormatted, StatusCalculator.MessageTypeDescription);
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

		static string GetHeaderHtmlText(ACIResponseMessageWrapper wrapper)
		{
			var dailyTotalTable = new FieldValueTableInterpretation();
			dailyTotalTable.AddIfNotEmpty(() => wrapper.ProcessingDate);
			dailyTotalTable.AddIfNotEmpty(() => wrapper.RelatedCargoControlNumber);
			dailyTotalTable.AddIfNotEmpty(() => wrapper.RiskAssessmentType);
			dailyTotalTable.AddIfNotEmpty(() => wrapper.ContainerNumbers);
			return dailyTotalTable.ToHtml();
		}

		string GetNotificationsHtmlText(ACIResponseMessageWrapper wrapper, out string sourceMessageWithErrorMarks)
		{
			if (wrapper.IsSyntaxError)
			{
				message.EM_MessageType = MessageTypeList.Codes.SyntaxError;
				message.EM_MessageSubType = MessageTypeList.Codes.SupplementaryCargoReport;
				var originalMessage = SyntaxErrorMessage.GetOriginalMessage(message);
				var requestMessageText = originalMessage?.EM_MessageText ?? ZString.Empty;
				var hasLinkedObject = new ZBool(originalMessage != null);
				var hasSentMessage = new ZBool(message.EM_LinkedObject != null);
				var isSentWithMessageErrors = originalMessage?.EM_SendWithMessageErrors ?? ZBool.False;
				if (!hasLinkedObject || !hasSentMessage || !isSentWithMessageErrors)
				{
					var msg = cusresMessage.ToString(new CACharSet());
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
					ErrorReporter.ReportOnce("ACI message syntax error", msg);
				}

				var data = SyntaxErrorMessage.GetSyntaxErrorData(message, requestMessageText);
				sourceMessageWithErrorMarks = data.sourceMessageTextWithErrorMarks;
				return TableInterpretation.GetTableInterpretation(data.syntaxErrors, null, TableInterpretation.Attributes.AlignLeft);
			}
			sourceMessageWithErrorMarks = null;
			return TableInterpretation.GetTableInterpretation(wrapper.Notifications, null, TableInterpretation.Attributes.AlignLeft);
		}

		#endregion

		#endregion

		#region Overridden Properties

		protected override ZGuid AcknowledgementEmailGroup
		{
			get { return CACustomsDataRegistry.Instance.SendACIAcknowledgementsToGroupAppliesAllCountries.Value; }
		}

		protected override ZString AcknowledgementEmailMode
		{
			get { return CACustomsDataRegistry.Instance.SendACIAcknowledgementsAppliesAllCountries.Value; }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get { return CACustomsDataRegistry.Instance.SendACIErrorsToGroupAppliesAllCountries.Value; }
		}

		protected override ZString ImpedimentEmailMode
		{
			get { return CACustomsDataRegistry.Instance.SendACIErrorsAppliesAllCountries.Value; }
		}

		protected override ZGuid ErrorEmailGroup
		{
			get { return CACustomsDataRegistry.Instance.SendACIErrorsToGroupAppliesAllCountries.Value; }
		}

		protected override ZString ErrorEmailMode
		{
			get { return CACustomsDataRegistry.Instance.SendACIErrorsAppliesAllCountries.Value; }
		}

		#endregion

		protected abstract IEDIFACTMessageAttachee GetLinkedObjectAndSetOnMessage(string objectReference);
		protected abstract EDIFACTMessageStatusCalculator StatusCalculator { get; }
		protected abstract ZString JobNumberLink { get; }

		protected ACIEDIMessage message;
		protected IEDIFACTMessageAttachee linkedObject;
		CUSRESMessage cusresMessage;
		ZString objectReferenceFormatted;

		#endregion
	}
}
