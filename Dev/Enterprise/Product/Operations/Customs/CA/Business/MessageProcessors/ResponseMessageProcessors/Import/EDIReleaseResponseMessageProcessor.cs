
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.BatchProcessor;
	using Enterprise.Customs.Business.MessageProcessors;
	using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
	using Enterprise.Customs.CA.Business.MessageBuilders;
	using Enterprise.Customs.CA.Business.MessageManagers;
	using Enterprise.Customs.CA.Messaging;
	using Enterprise.Customs.Common.Shared;
	using Enterprise.Edifact.Auto;
	using Enterprise.Edifact.D96A.Elements;
	using Enterprise.Edifact.D96A.Messages.CUSREP;
	using Enterprise.Edifact.D96A.Messages.CUSRES;
	using Enterprise.Edifact.D96A.Segments;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business.CustomValues;
	using Enterprise.Messaging.MessageProcessors;
	using Enterprise.ZArchitecture.Environment;
	using static Enterprise.Customs.CA.Business.MessageProcessors.ImportLinkedObjectManager;
	using MessageTypeList = MessageTypeList;
	using SegmentGroup1 = Enterprise.Edifact.D96A.Messages.CUSRES.SegmentGroup1;

	/// <summary>
	/// Inbound release messages may be received as a result of several reasons:
	/// Error reports in response to sent ACROSS declarations or RNS requests (Status Enquiry or Arrival Report),
	/// Or release status updates that may be responses to sent ACROSS declarations or RNS requests, or unscolicited status updates.
	/// Or cancellation accepted messages in response to sent ACROSS declaration cancellation massages.
	///
	/// The following logic is used to process these messages:
	/// 
	/// Error messages should only be attached to the BO from where the message was sent. So:
	/// 1.	If first sent RNS requests which is awaiting reply (or last which is not awaiting reply) specified is matched, then:
	///		-	the received message is attached.
	///		-	message and job statuses are updated (if applicable).
	///		-	the group specified in the registry to receive error notifications is notified for a branch and sub-type associated with the sent message from a job 
	/// 
	/// 2.	If matched request doesn't have a linked job specified,
	///		then this error message is a response to an RNS request submitted from the Release Notifications Module grid, 
	///		and probably relates to a shipment not lodged by this site, but this site is the associated sub-location. 
	///		The group specified in the registry to receive error notifications is notified, for a branch and sub-type associated with the sent message
	/// 
	/// 3.	If RNS requests are NOT found, then it must be response to sent ACROSS message from declaration.
	///		So the job is loaded by transaction number and message processed as option 1.
	/// 
	/// 4.	If NO declarations are found too, then this is an unexpected situation (job is deleted or something else) and so a ‘CouldNotFindLinkedObjectException’ exception is generated.
	/// 
	/// 
	/// Release status update messages should be attached to all related BOs:
	/// 1.	If existing BOs are matched by transaction number or cargo control number then for each job:
	///		-	the inbound message is attached;
	///		-	the status and the release date are updated (if applicable);
	///		-	the user and group associated with the BO are notified (by email).
	/// 
	/// 2.	If there are NO matched jobs, but RNS requests with linked objects are matched, 
	///		then it would appear that the reference data used to match against the BO is not consistent. 
	///		This could be a result of the CCN being changed on the job after the request was sent, or some other inconsistency.
	///		So for each BO:	
	///		-	the inbound messages is attached to the linked BO;
	///		-	since there is some inconsistence the status and release date are not updated on the BO;
	///		-	the user and group associated with the BO are notified with an appropriate comment.
	/// 
	/// 3.	If RNS requests are matched but any of them doesn't have a linked job specified,
	///		then this status update message is a response to an RNS request submitted from the Release Notifications Module grid,
	///		and probably relates to a shipment not lodged by this site, but this site is the associated sub-location.
	///		The group specified in the registry to receive acknowledgement notifications is notified with an appropriate comment, for a branch associated with the sent message.
	///	
	/// 4.	If RNS requests are NOT found, then this message probably relates to a shipment lodged by some other party 
	///		and is an unscoliced status update sent to this site because the sub-location (warehouse) specified by the external broker relates to this site.
	///		The group specified in the registry to receive release notifications is notified with an appropriate comment for current branch.
	///	
	/// 	
	/// Cancellation accepted message is exactly like status update but related to declaration cancelation message and includes transaction number only.
	/// The message can only be determined if declaration status is cancelled.
	/// Since other related jobs should be notified as well, cargo control number is gotten from cancelled declaration to load them.
	/// And all objects are processed as option 1 of status updates but with cancellation notification.
	/// </summary>

	[ApplicationIdentifier(ServiceOptions.Codes.Aerospace)]
	[ApplicationIdentifier(ServiceOptions.Codes.PaperEnterToArrive)]
	[ApplicationIdentifier(ServiceOptions.Codes.PaperFIRST)]
	[ApplicationIdentifier(ServiceOptions.Codes.PaperPARS)]
	[ApplicationIdentifier(ServiceOptions.Codes.IID)]
	[ApplicationIdentifier(ServiceOptions.Codes.PARS)]
	[ApplicationIdentifier(ServiceOptions.Codes.PaperRMD)]
	[ApplicationIdentifier(ServiceOptions.Codes.PaperValueIncluded)]
	[ApplicationIdentifier(ServiceOptions.Codes.RMD)]
	[ApplicationIdentifier(ServiceOptions.Codes.PaperCash)]
	[ApplicationIdentifier(ServiceOptions.Codes.TemporaryRelease)]
	[ApplicationIdentifier(ServiceOptions.Codes.SpecialRelease)]
	[ApplicationIdentifier(ServiceOptions.Codes.ReplaceRMDwithAQ)]
	[ApplicationIdentifier(ServiceOptions.Codes.PARSOGD)]
	[ApplicationIdentifier(ServiceOptions.Codes.RMDOGD)]
	[ApplicationIdentifier(ServiceOptions.Codes.RNSGenericResponse)]
	[ApplicationIdentifier(ServiceOptions.Codes.CSAEDIHighway513)]
	[ApplicationIdentifier(ServiceOptions.Codes.CSAEDIRail)]
	[ApplicationIdentifier(ServiceOptions.Codes.CSAHighwayPaper)]
	[ApplicationIdentifier(ServiceOptions.Codes.CSANonHighwayPpaper)]
	[ApplicationIdentifier(ServiceOptions.Codes.CSAEDIHighway612)]
	[WTG.StaticAnalysis.Annotation.CodeAlive("Is used by CA custom")]
	class EDIReleaseResponseMessageProcessor : ImportResponseMessageProcessor
	{
		public EDIReleaseResponseMessageProcessor(LoggingInformation logger)
			: base(logger, new EDIReleaseImportStatusCalculator(), MessageTypeList.Codes.EDIRelease, Res.GetString("dae17117-8447-4abb-a461-42f0a429e521", "EDI Release Response"))
		{
		}

		protected override string DoProcessingReturningStatus(Enterprise.Messaging.Business.EDIMessage ediMessage)
		{
			base.DoPreProcessingReturningStatus(ediMessage);
			var resultStatus = ZString.Empty;
			var cusresMessage = (CUSRESMessage)ediMessage.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet());
			if (cusresMessage != null && cusresMessage.GIS.Count > 0)
			{
				var message = (EDIReleaseMessage)ediMessage;
				var serviceOption = cusresMessage.BGM[0].DocumentMessageName.DocumentMessageName;
				var processingIndicator = cusresMessage.GIS[0].ProcessingIndicator.ProcessingIndicatorCoded;
				var tranNumber = new ZString(cusresMessage.BGM[0].DocumentMessageNumber).Replace(" ", "");
				var ccn = cusresMessage.Group5.Count > 0 && cusresMessage.Group5[0].RFF.Count > 0
							? new ZString(cusresMessage.Group5[0].RFF[0].Reference.ReferenceNumber).Replace(" ", "") : ZString.Empty;
				ediMessage.EM_ApplicationReference = ccn;
				if (Regex.IsMatch(tranNumber, $"^\\d{{{TransactionNumber.Schema.FormattedTransactionNumberMaxLength}}}$"))
				{
					message.SetSystemDefinedValue(EDIReleaseMessage.Schema.TransactionNumber, tranNumber);
				}
				if (IsErrorResponse(processingIndicator))
				{
					resultStatus = ProcessErrorMessage(cusresMessage, message, serviceOption, processingIndicator);
				}
				else if (IsCorrectIndicator(processingIndicator))
				{
					resultStatus = ProcessAcknowledgementMessage(cusresMessage, message, serviceOption, processingIndicator);
				}
			}

			if (resultStatus.IsEmpty)
			{
				throw new UnableToInterpretMessageException(ediMessage, this);
			}
			return resultStatus;
		}

		#region ProcessErrorMessage

		ZString ProcessErrorMessage(CUSRESMessage cusresMessage, EDIReleaseMessage message, string serviceOption, ProcessingIndicatorCodedList processingIndicator)
		{
			LinkedObjectInfo linkedObjectInfo = null;
			if (serviceOption == ServiceOptions.Codes.RNSGenericResponse)
			{
				var originalMessage = D96AMessageUtilities.GetOriginalRNSMessage(message.Factory, cusresMessage);
				if (originalMessage != null)
				{
					var attachee = GetAttacheeFromJob(originalMessage.EM_LinkedObject);
					if (!message.TransactionNumber.IsEmpty && attachee is CusEntryHeader entryHeader && entryHeader.CH_BGMReference != message.TransactionNumber)
					{
						throw new CouldNotFindLinkedObjectException(message.TransactionNumber, message, this);
					}
					else
					{
						var reference = GetLinkedObjectReferenceFromMessage(originalMessage);
						linkedObjectInfo = new LinkedObjectInfo(GetAttacheeFromJob(originalMessage.EM_LinkedObject), reference, originalMessage);
					}
				}
				else
				{
					linkedObjectInfo = LoadLinkedObjectInfosByDirectReference(message, false).FirstOrDefault();
				}
			}
			else
			{
				linkedObjectInfo = LoadLinkedObjectInfosByDirectReference(message, false).FirstOrDefault() ?? LoadObjectBySentRNSMessage(message);
			}

			if (linkedObjectInfo != null)
			{
				linkedObject = linkedObjectInfo.LinkedObject;
				linkedObjectReference = linkedObjectInfo.LinkedObjectReference;

				message.EM_GB = linkedObjectInfo.NotificationBranch.PK;
				message.EM_MessageSubType = EDIReleaseImportEntryStatusList.GetEntryStatusByProcessingIndicatorCoded(processingIndicator);

				if (linkedObject != null) //RNS request is submitted from any job (not the Release Notifications Module)
				{
					linkedObject.Messages.Add(message);

					if (StatusCalculator.IsAwaitingReply(linkedObject.MessageStatus))
					{
						var messageSubType = StatusCalculator.GetMessageSubType(linkedObject.MessageStatus);
						linkedObject.MessageStatus = StatusCalculator.GetMessageRejectedStatus(messageSubType);
					}
					SetJobStatus(serviceOption);
				}
				if (serviceOption == ServiceOptions.Codes.RNSGenericResponse)
				{
					var sentRNSMessage = linkedObjectInfo.MatchedSentRNSMessages.FirstOrDefault();
					if (sentRNSMessage != null)
					{
						sentRNSMessage.EM_Status = EDIMessage.Status.Rejected;
					}
				}
				using (DisposableEnvironment.ForBranch(message.Branch.PK.ToGuid()))
				{
					var matchedSentMessage = linkedObjectInfo.MatchedSentRNSMessages;
					var email = GetErrorResponseEmailAndSetOnMessage(cusresMessage, message, GetMessageTypeDescription(matchedSentMessage));
					AddWarehouseRNSNotificationEmailGroups(message, EmailResponseLinkedObject, email);
					SendErrorReport(EmailResponseLinkedObject, email); //TODO: Enhance SendReport to take query to find notification user if linkedObject == null
				}
			}
			else
			{
				throw new CouldNotFindLinkedObjectException(message.TransactionNumber, message, this);
			}
			return EDIMessage.Status.Received;
		}

		ZString GetLinkedObjectReferenceFromMessage(EDIMessage originalMessage)
		{
			var result = ZString.Empty;
			if (originalMessage != null)
			{
				result = originalMessage.EM_MessageOwner.IsEmpty ? originalMessage.EM_ApplicationReference : originalMessage.EM_MessageOwner;
				if (result.IsEmpty)
				{
					var cusrep = (CUSREPMessage)originalMessage.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet());
					if (cusrep != null)
					{
						var group1 = cusrep.Group1;
						result = D96AMessageUtilities.GetReferenceCode(group1, ReferenceQualifierList.CustomsDeclarationNumber);
						if (result.IsEmpty)
						{
							result = D96AMessageUtilities.GetReferenceCode(group1, ReferenceQualifierList.TransactionReferenceNumber);
						}
					}
				}
				if (result.IsEmpty)
				{
					result = GetLinkedObjectReference(originalMessage.EM_LinkedObject);
				}
			}
			return result;
		}

		static string GetMessageTypeDescription(IEnumerable<EDIMessage> messages)
		{
			if (messages.Any())
			{
				return messages.Take(2).Count() == 1 || messages.All(m => m.EM_MessageSubType == messages.First().EM_MessageSubType)
								? new RNSMessageTypes().GetDescriptionFromCode(messages.First().EM_MessageSubType) : MessageTypeList.Descriptions.RNSRequest;
			}
			return string.Empty;
		}

		#endregion

		#region ProcessAcknowledgementMessage

		ZString ProcessAcknowledgementMessage(CUSRESMessage cusresMessage, EDIReleaseMessage message, string serviceOption, ProcessingIndicatorCodedList processingIndicator)
		{
			SetSystemDefinedValues(cusresMessage, message, message.EM_ApplicationReference);
			var linkedObjectInfos = LoadLinkedObjectInfosByDirectReference(message, true);
			var invalidReferences = !linkedObjectInfos.Any();
			if (invalidReferences)
			{
				linkedObjectInfos = LoadObjectsBySentRNSMessage(message);
			}

			if (linkedObjectInfos.Any())
			{
				var firstInfo = linkedObjectInfos.First();
				if (firstInfo.LinkedObject == null) //RNS request submitted from the Release Notifications Module
				{
					linkedObjectReference = firstInfo.LinkedObjectReference;
					message.EM_GB = firstInfo.NotificationBranch.PK;
					message.EM_MessageSubType = EDIReleaseImportEntryStatusList.GetEntryStatusByProcessingIndicatorCoded(processingIndicator);

					using (DisposableEnvironment.ForBranch(message.Branch.PK.ToGuid()))
					{
						var email = GetStatusUpdateEmailAndSetOnMessage(cusresMessage, message, NoJobFoundComment);
						AddWarehouseRNSNotificationEmailGroups(message, null, email);
						SendAcknowledgementReport(null, email); //TODO: Enhance SendReport to take query to find notification user
					}
				}
				else
				{
					var messageSubType = ZString.Empty;
					foreach (var info in linkedObjectInfos) //Cases where linked objects are found
					{
						linkedObject = info.LinkedObject;
						linkedObjectReference = info.LinkedObjectReference;

						message = messageSubType.IsEmpty ? message : (EDIReleaseMessage)message.Clone();
						message.EM_GB = info.NotificationBranch.PK;
						if (messageSubType.IsEmpty) //This is to handle job cancelation, so if first job (declaration) is cancelled then all attached messages should be cancelled
						{
							messageSubType = StatusCalculator.GetMessageSubType(linkedObject.MessageStatus, processingIndicator);
						}
						message.EM_MessageSubType = EDIReleaseImportEntryStatusList.GetEntryStatusByProcessingIndicatorCoded(processingIndicator, messageSubType);

						linkedObject.Messages.Add(message);

						using (DisposableEnvironment.ForBranch(message.Branch.PK.ToGuid()))
						{
							EmailDef email;
							if (!invalidReferences && StatusCalculator.IsAwaitingReply(linkedObject, message)
								&& (StatusCalculator.GetMessageSubType(linkedObject.MessageStatus) != MessageSubTypeCodes.Codes.Cancellation
								|| messageSubType == MessageSubTypeCodes.Codes.Cancellation))
							{
								email = GetAcceptedResponseEmailAndSetOnMessage(cusresMessage, message);
								var header = linkedObject as CusEntryHeader;
								if (!(header?.Declaration?.IsIID ?? ZBool.True))
								{
									linkedObject.MessageStatus = StatusCalculator.GetMessageClearedStatus(messageSubType);
								}
							}
							else
							{
								var additionalComments = invalidReferences ? InvalidReferencesDetails : string.Empty;
								email = GetStatusUpdateEmailAndSetOnMessage(cusresMessage, message, additionalComments);
							}
							AddWarehouseRNSNotificationEmailGroups(message, EmailResponseLinkedObject, email);
							SendAcknowledgementReport(EmailResponseLinkedObject, email);
						}

						if (!invalidReferences)
						{
							SetReleaseDate(cusresMessage);
							SetJobStatus(serviceOption);
						}
						message.EM_Status = EDIMessage.Status.Received;
					}
				}
			}
			else //An unscoliced status update
			{
				linkedObjectReference = message.TransactionNumber.IsEmpty ? message.EM_ApplicationReference : message.TransactionNumber;
				message.EM_MessageSubType = EDIReleaseImportEntryStatusList.GetEntryStatusByProcessingIndicatorCoded(processingIndicator);
				var email = GetStatusUpdateEmailAndSetOnMessage(cusresMessage, message, NoJobFoundComment);
				AddWarehouseRNSNotificationEmailGroups(message, null, email, true);
				SendAcknowledgementReport(null, email);
			}
			return EDIMessage.Status.Received;
		}

		void SetSystemDefinedValues(CUSRESMessage cusresMessage, EDIReleaseMessage message, ZString ccn)
		{
			var processingDate = ZDateTime.Empty;
			var releaseDate = ZDateTime.Empty;
			foreach (DTMSegment dtm in cusresMessage.DTM)
			{
				ZDateTime dateTime;
				if (ZDateTime.TryParseExact(dtm.DateTimePeriod.DateTimePeriod, out dateTime, "yyyyMMddHHmm"))
				{
					if (dtm.DateTimePeriod.DateTimePeriodQualifier == "9")
					{
						processingDate = dateTime;
					}
					else
					{
						releaseDate = dateTime;
					}
				}
			}
			if (!releaseDate.IsEmpty)
			{
				message.RNSReleaseDate = releaseDate;
			}

			if (processingDate.IsEmpty)
			{
				processingDate = releaseDate;
			}

			if (!processingDate.IsEmpty)
			{
				message.RNSProcessingDate = processingDate;
			}

			var warehouseCode = D96AMessageUtilities.GetRelatedLocation(cusresMessage.LOC, PlaceLocationQualifierList.CustomsOfficeOfClearance);
			if (!warehouseCode.IsEmpty)
			{
				message.SetSystemDefinedValue(ReleaseStatus.Schema.RL_WarehouseCode, warehouseCode);
			}

			var releaseOfficeCode = D96AMessageUtilities.GetLocation(cusresMessage.LOC, PlaceLocationQualifierList.CustomsOfficeOfClearance);
			if (!releaseOfficeCode.IsEmpty)
			{
				message.SetSystemDefinedValue(ReleaseStatus.Schema.RL_ReleaseOffice, releaseOfficeCode);
			}

			if (!ccn.IsEmpty)
			{
				message.SetSystemDefinedValue(EDIMessage.Schema.CargoControlNumber, ccn);
			}
		}
		#endregion
		#region Implementation

		#region GetAcceptedResponseEmailAndSetOnMessage

		protected override string GetAcceptedMessageText(SegmentGroup cusresMessage, EDIMessage message)
		{
			var cusres = (CUSRESMessage)cusresMessage;
			var table = new HtmlTableCreator(new[] { Res.GetString("0ef2d8bc-c505-4e14-869a-7d8dbd548d94", "Field"), Res.GetString("3c3e7fbd-b5cf-4032-9cf5-b41318f901fd", "Value") });

			#region Processing Indicator

			if (cusres.GIS.Count > 0)
			{
				table.WriteRow(Res.GetString("9c829a6d-eba6-4a67-94e3-37b54ab366ce", "Processing Indicator"),
									string.Format("{0} - {1}", cusres.GIS[0].ProcessingIndicator.ProcessingIndicatorCoded, message.EM_MessageSubTypeDescription));
			}

			#endregion

			#region  Service Option & Document Reference

			if (cusres.BGM.Count > 0)
			{
				var serviceOption = cusres.BGM[0].DocumentMessageName.DocumentMessageName;
				table.WriteRow(Res.GetString("5711686d-025a-4d15-a72b-0aaf3f93a4a5", "Service Option"),
													string.Format("{0} - {1}", serviceOption, new ServiceOptions().GetDescriptionFromCode(serviceOption)));

				table.WriteRow(Res.GetString("01aafd33-4e8d-42c9-9693-ca476f964bf5", "Document Reference"), new ZString(cusres.BGM[0].DocumentMessageNumber).Replace(" ", ""));
			}

			#endregion

			#region Release Office & Warehouse Codes

			if (cusres.LOC.Count > 0)
			{
				table.WriteRow(Res.GetString("71654f41-7226-409a-bc61-9cc578f88c6f", "Release Office Code"), cusres.LOC[0].LocationIdentification.PlaceLocationIdentification);
				table.WriteRow(Res.GetString("ecbc2a18-63dd-49c7-b1d4-d7dc798b7428", "Warehouse Code"), cusres.LOC[0].LocationIdentification.PlaceLocation);
			}

			#endregion

			#region Cargo Control Number (CCN)

			if (cusres.Group5.Count > 0 && cusres.Group5[0].RFF.Count > 0)
			{
				table.WriteRow(Res.GetString("87fd8616-e2f9-42f4-803b-34e1748f6e8e", "Cargo Control Number"), new ZString(cusres.Group5[0].RFF[0].Reference.ReferenceNumber).Replace(" ", ""));
			}

			#endregion

			#region Processing & Clearance Dates

			foreach (DTMSegment dtm in cusres.DTM)
			{
				ZDateTime processingDate;
				ZDateTime.TryParseExact(dtm.DateTimePeriod.DateTimePeriod, out processingDate, "yyyyMMddHHmm");
				var dateCaption = dtm.DateTimePeriod.DateTimePeriodQualifier == "9"
														? Res.GetString("68a0d711-fca6-4432-99c0-c1d493cc7b0f", "Processing Date")
														: Res.GetString("e2a93dfc-634e-4aa6-8b92-1abc824a980f", "Clearance Date");
				table.WriteRow(dateCaption, processingDate.ToLongTimeString());
			}

			#endregion

			#region Container Numbers

			var containerNumber = message.ContainerNumbers;
			if (!containerNumber.IsEmpty)
			{
				table.WriteRow(Res.GetString("464d1148-5708-4263-9887-5dd1b105f0fc", "Container Numbers"), containerNumber);
			}
			#endregion

			#region Delivery Instructions

			if (cusres.FTX.Count > 0)
			{
				var instructions = from FTXSegment ftx in cusres.FTX
								   where ftx.TextSubjectQualifier == TextSubjectQualifierList.PartyInstructions
								   select ftx.TextLiteral.FreeText1 + " " + ftx.TextLiteral.FreeText2;

				var deliveryInstructions = instructions.Aggregate(new ZStringBuilder(), (b, i) => b.Append(i)).ToStringWithNewLineBetweenAppends();
				table.WriteRow(Res.GetString("a9da7d13-9d8b-4023-89dd-9c984cf56d6f", "Delivery Instructions"), deliveryInstructions);
			}

			#endregion

			return table.ToHtml();
		}

		#endregion

		#region GetErrorResponseEmailAndSetOnMessage

		protected override string GetErrorMessageText1(SegmentGroup cusresMessage, EDIMessage message)
		{
			var builder = new StringBuilder();
			if (((CUSRESMessage)cusresMessage).FTX.Count > 0)
			{
				var rejectCommentsCaption = Res.GetString("86419a43-db91-4db1-a43c-c0e8b75a0bdf", "Reject Comments");
				builder.AppendFormat("<strong>{0}:</strong><br />", rejectCommentsCaption);
				foreach (FTXSegment ftx in ((CUSRESMessage)cusresMessage).FTX)
				{
					if (ftx.TextSubjectQualifier == TextSubjectQualifierList.ErrorDescriptionFreeText)
					{
						builder.AppendFormat("\r\n<p>{0} {1}</p>", ftx.TextLiteral.FreeText1, ftx.TextLiteral.FreeText2);
					}
				}
			}

			return builder.ToString();
		}

		protected override string GetErrorMessageText2(SegmentGroup cusresMessage, EDIMessage message)
		{
			HtmlTableCreator errorTable = null;
			if (((CUSRESMessage)cusresMessage).Group1.Count > 0)
			{
				errorTable = new HtmlTableCreator(new[] { Res.GetString("8ed0ac48-785e-42b9-8ffd-75a45c02a761", "Code"), Res.GetString("5a6c741b-c420-4874-a4de-81854583f54d", "Error Text") });

				foreach (SegmentGroup1 group1 in ((CUSRESMessage)cusresMessage).Group1)
				{
					foreach (ERCSegment erc in group1.ERC)
					{
						var errorCode = erc.ApplicationErrorDetail.ApplicationErrorIdentification;
						errorTable.WriteRow(errorCode, message.GetErrorDescription(errorCode));
					}
				}
			}

			return errorTable != null ? errorTable.ToHtml() : string.Empty;
		}

		#endregion

		#region GetStatusUpdateEmailAndSetOnMessage

		protected EmailDef GetStatusUpdateEmailAndSetOnMessage(SegmentGroup cusresMessage, EDIMessage message, string additionalComments = "")
		{
			var subject = EDIReleaseImportEntryStatusList.IsCancellation(message.EM_MessageSubType)
				? Res.GetString("35918EB4-5E74-4C22-BBE1-B3F121CF68FB", "Cancellation accepted {0} Response for {1}", StatusCalculator.MessageTypeDescription, linkedObjectReference)
				: Res.GetString("2BF97EC7-E842-42A2-8AD2-E64B7024EE25", "{0} Status Update Message for {1}", StatusCalculator.MessageTypeDescription, linkedObjectReference);
			var emailBuilder = new EmailDefBuilder(subject, message.EM_FormattedMessageText, EmailDefBuilder.HtmlTemplates.StatusUpdate);
			emailBuilder.AddArgReplacementRange(LinkProvider.GetLink(linkedObject), linkedObjectReference, StatusCalculator.MessageTypeDescription);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.FromMessageSender, MessageSender);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, GetAcceptedMessageText(cusresMessage, message), true);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.AdditionalComments, additionalComments);
			message.EM_MessageInterpretation = emailBuilder.ToString();
			return emailBuilder.ToEmail();
		}

		#endregion

		#region Update Linked Object

		void SetReleaseDate(CUSRESMessage cusresMessage)
		{
			var ediReleaseAttachee = linkedObject as IEDIReleaseMessageAttachee;
			if (ediReleaseAttachee != null)
			{
				if (cusresMessage.DTM.Count > 0 && cusresMessage.DTM[0].DateTimePeriod.DateTimePeriodQualifier == DateTimePeriodQualifierList.ClearanceDateCustoms)
				{
					ZDateTime processingDate;
					if (ZDateTime.TryParseExact(cusresMessage.DTM[0].DateTimePeriod.DateTimePeriod, out processingDate, "yyyyMMddHHmm"))
					{
						try
						{
							ediReleaseAttachee.SettingReleaseDateWithStatusUpdate = true;
							ediReleaseAttachee.ReleaseDate = processingDate;
							if (cusresMessage.LOC.Count > 0)
							{
								ediReleaseAttachee.ReleaseOffice = cusresMessage.LOC[0].LocationIdentification.PlaceLocationIdentification;
							}
						}
						finally
						{
							ediReleaseAttachee.SettingReleaseDateWithStatusUpdate = false;
						}
					}
				}
			}
		}

		void SetJobStatus(ZString serviceOption)
		{
			if (serviceOption != ServiceOptions.Codes.RNSGenericResponse)
			{
				string jobStatus = StatusCalculator.CalculatedJobStatus(linkedObject);
				if (jobStatus != EDIReleaseImportEntryStatusList.Codes.SyntaxError)
				{
					if (jobStatus == EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted
						&& linkedObject.JobStatus == EDIReleaseImportEntryStatusList.Codes.GoodsReleased
						&& ((linkedObject as IEDIReleaseMessageAttachee)?.ReleaseDate.IsValid ?? false))
					{
						return;
					}
					linkedObject.JobStatus = jobStatus;
				}
			}
		}

		#endregion

		protected override ZString ApplicationCodeForGetUserToNotify
		{
			get { return EDIMessage.ApplicationCodes.CAIMP; }
		}

		static bool IsErrorResponse(ProcessingIndicatorCodedList processingIndicator)
		{
			return processingIndicator == ProcessingIndicatorCodedList.MessageContentRejectedWithComment || processingIndicator == ProcessingIndicatorCodedList.ErrorMessage;
		}

		static bool IsCorrectIndicator(ProcessingIndicatorCodedList processingIndicator)
		{
			return !string.IsNullOrEmpty(EDIReleaseImportEntryStatusList.GetEntryStatusByProcessingIndicatorCoded(processingIndicator));
		}

		static string InvalidReferencesDetails
		{
			get { return Res.GetString("bb83d2f6-3059-4c8a-b39f-bd3fb76e5623", "NOTE: The reference details in this received message do not match the job. Please review this job."); }
		}

		static string NoJobFoundComment
		{
			get { return Res.GetString("5b67e4fd-ce1a-4257-a039-47808b53394a", "NOTE: No job has been found that matches the reference details in this message."); }
		}

		new EDIReleaseImportStatusCalculator StatusCalculator
		{
			get { return ((EDIReleaseImportStatusCalculator)base.StatusCalculator); }
		}

		#endregion

		#region Send Warehouse RNS Status Messages Notification

		void AddWarehouseRNSNotificationEmailGroups(EDIReleaseMessage message, BusinessObject parent, EmailDef email, bool setBranch = false)
		{
			if (!message.SubLocation.IsEmpty)
			{
				var notificationHelper = new ResponseNotificationHelper(message);
				if (setBranch && notificationHelper.NotificationBranch != null)
				{
					message.EM_GB = notificationHelper.NotificationBranch.PK;
				}
				var groups = notificationHelper.NotificationEmailGroups;
				if (groups != null && groups.Any())
				{
					foreach (var group in groups)
					{
						CopyGroupToEmails(parent, email, group);
					}
				}
			}
		}

		#endregion
	}
}
