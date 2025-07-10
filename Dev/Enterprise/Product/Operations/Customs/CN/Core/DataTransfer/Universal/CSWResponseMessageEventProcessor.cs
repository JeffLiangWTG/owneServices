using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CN.DataTransfer.Universal
{
	public class CSWResponseMessageEventProcessor : IJobDeclarationEventProcessor
	{
		public static bool IsCSWResponseMessage(UniversalEvent eventDataObject)
		{
			var result = false;
			if (eventDataObject != null)
			{
				var eventReference = eventDataObject.EventReference.GetValueOrDefault();
				if (eventReference.EqualsIgnoringCase($"{CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType}={CN.Business.Constants.JobDeclarationUniversalMessagingEventTypes.SingleWindow}"))
				{
					result = true;
				}
			}
			return result;
		}

		public static BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent eventDataObject, BusinessObjectFactory factory)
		{
			Customs.Business.CusEntryHeader entryHeader = null;

			var localReferenceNumber = eventDataObject.GetContextValueByType(Constants.Universal.ContextType.LocalReferenceNumber);
			if (!localReferenceNumber.IsEmpty)
			{
				entryHeader = Customs.Business.CusEntryHeader.LoadForBGMReference(factory, localReferenceNumber);
			}

			if (entryHeader == null)
			{
				var declarationUnifiedNumber = eventDataObject.GetContextValueByType(Constants.Universal.ContextType.DeclarationUnifiedNumber);
				if (!declarationUnifiedNumber.IsEmpty)
				{
					entryHeader = UniversalEventHelper.GetEntryHeaderFromUNINumber(declarationUnifiedNumber, factory);
				}
			}

			return entryHeader != null ? new BusinessObject[] { entryHeader } : System.Array.Empty<BusinessObject>();
		}

		public bool ProcessMessage(IXmlSessionTracker logger, IXmlEventValueObject eventDataObject, IEDIMessage message, BusinessObject businessObject)
		{
			var result = false;
			var entryHeader = businessObject as CusEntryHeader;
			var ediMessage = message as EDIMessage;

			if (eventDataObject is UniversalEvent eventData && entryHeader != null && ediMessage != null && IsCSWResponseMessage(eventData))
			{
				var messageStatus = UpdateMessageStatus(eventData, entryHeader);
				var entryStatus = UpdateEntryStatus(eventData, entryHeader);
				UpdateEntryNumbers(logger, eventData, entryHeader);
				UpdateEDIMessage(eventData, entryHeader, ediMessage);

				if (!messageStatus.IsEmpty || !entryStatus.IsEmpty)
				{
					SendEmail(entryHeader, ediMessage, messageStatus, entryStatus);
				}
				result = true;
			}

			return result;
		}

		ZString UpdateMessageStatus(UniversalEvent eventData, CusEntryHeader entryHeader)
		{
			var messageStatus = ZString.Empty;
			var currentStatus = entryHeader.CH_Status;

			var eventType = eventData.EventType.GetValueOrDefault();
			if (eventType == AutoEvents.MessageDeliveredCode)
			{
				messageStatus = JobMessageStatusList.GetAcknowledgedStatus(currentStatus);
			}
			else if (eventType == AutoEvents.MessageRejectedCode)
			{
				messageStatus = JobMessageStatusList.GetErrorStatus(currentStatus);
			}

			if (!messageStatus.IsEmpty)
			{
				var lastIncomingMsg = entryHeader.GetLastDeliverResponseMessage();
				if (lastIncomingMsg == null || eventData.EventTime > lastIncomingMsg.GetUniversalEvent().EventTime)
				{
					entryHeader.CH_Status = messageStatus;
				}
			}

			return messageStatus;
		}

		ZString UpdateEntryStatus(UniversalEvent eventData, CusEntryHeader entryHeader)
		{
			var entryStatus = eventData.GetContextValueByType(Constants.Universal.ContextType.EntryStatus);
			var eventTimeOffset = eventData.EventTime.GetValueOrDefault();
			var eventTime = eventTimeOffset.IsEmpty ? ZDateTime.Now : eventTimeOffset.ToZDateTime();

			if (!entryStatus.IsEmpty)
			{
				var factory = entryHeader.Factory;

				if (CNRefCusCodeListLoader.GetCustomsStatus(factory, entryStatus, AssessmentDate) == null)
				{
					var declarationUnifiedNumber = eventData.GetContextValueByType(Constants.Universal.ContextType.DeclarationUnifiedNumber);
					var movementReferenceNumber = eventData.GetContextValueByType(Constants.Universal.ContextType.CustomsDeclarationNumber);
					var messageDUN = declarationUnifiedNumber.IsEmpty ? string.Empty : string.Format(CultureInfo.InvariantCulture, (NoResString)"\r\nDeclaration Unified Number = {0}", declarationUnifiedNumber);
					var messageMRN = movementReferenceNumber.IsEmpty ? string.Empty : string.Format(CultureInfo.InvariantCulture, (NoResString)"\r\nEntry Number = {0}", movementReferenceNumber);
					ErrorReporter.ReportOnce("CN CSWResponseMessageEventProcessor: Unknown Entry Status On Response Message", string.Format(CultureInfo.InvariantCulture, "The Entry Status '{0}' from the response message does not defined in RefCusCodeList.{1}{2}", entryStatus, messageDUN, messageMRN));
				}
				else
				{
					if (entryHeader.CH_EntryStatus != entryStatus && CustomsStatusAttributeHelper.ShouldUpdateCustomsStatus(factory, entryStatus, CountryCode, AssessmentDate))
					{
						var lastIncomingMsg = entryHeader.GetLastIncomingdMessageUpdatedCustomsStatus();
						if (lastIncomingMsg == null || eventData.EventTime > lastIncomingMsg.GetUniversalEvent().EventTime)
						{
							entryHeader.CH_EntryStatus = entryStatus;
						}
					}
					if (CustomsStatusAttributeHelper.IsStatusCommenced(factory, entryStatus, CountryCode, AssessmentDate))
					{
						entryHeader.Declaration?.LogCustomsCommencedIfNeeded();
						entryHeader.PopulateEntrySubmittedDateIfRequired(eventTime);
					}
					if (CustomsStatusAttributeHelper.IsStatusCleared(factory, entryStatus, CountryCode, AssessmentDate))
					{
						entryHeader.CH_Status = JobMessageStatusList.GetClearedStatus(entryHeader.CH_Status);
						if (entryHeader.CH_Status == JobMessageStatusList.Codes.ClearedPreliminaryDeclaration)
						{
							if (entryHeader.Declaration != null && (entryHeader.Declaration.JE_ClearanceMode == ClearanceModeList.Codes.TwoStepManual || entryHeader.Declaration.JE_ClearanceMode == ClearanceModeList.Codes.TwoStepAuto))
							{
								entryHeader.CH_Status = JobMessageStatusList.Codes.AwaitingResponseCompletedDeclaration;
							}
						}
					}
					if (CustomsStatusAttributeHelper.ShouldUpdateReleaseDate(factory, entryStatus, CountryCode, AssessmentDate) && entryHeader.CH_EntryReleaseDate.IsEmpty)
					{
						entryHeader.CH_EntryReleaseDate = eventTime;
					}
					if (CustomsStatusAttributeHelper.IsStatusRejected(factory, entryStatus, CountryCode, AssessmentDate))
					{
						entryHeader.CH_Status = JobMessageStatusList.GetErrorStatus(entryHeader.CH_Status);
					}
					if (CustomsStatusAttributeHelper.IsStatusCancelled(factory, entryStatus, CountryCode, AssessmentDate))
					{
						entryHeader.CH_Status = MessageStatusList.Codes.ClearDelete;
					}
				}
			}

			return entryStatus;
		}

		void UpdateEntryNumbers(IXmlSessionTracker logger, UniversalEvent eventData, CusEntryHeader entryHeader)
		{
			var declarationUnifiedNumber = eventData.GetContextValueByType(Constants.Universal.ContextType.DeclarationUnifiedNumber);
			if (!declarationUnifiedNumber.IsEmpty)
			{
				var uni = entryHeader.DeclarationUnifiedNumber;
				if (!uni.IsEmpty && uni != declarationUnifiedNumber && entryHeader.IsEntryNumberSystemGernerated(CusEntryNumberTypes.China.DeclarationUnifiedNumber))
				{
					logger.Log(Integration.LogType.Warning, string.Format(CultureInfo.InvariantCulture, "For the Declaration Unified Number {0} already exists, so it will not be updated.", uni));
				}
				else
				{
					entryHeader.DeclarationUnifiedNumber = declarationUnifiedNumber;
				}
			}

			var newMovementReferenceNumber = eventData.GetContextValueByType(Constants.Universal.ContextType.CustomsDeclarationNumber);
			if (!newMovementReferenceNumber.IsEmpty)
			{
				var movementReferenceNumber = entryHeader.MovementReferenceNumber;
				var isMRNSystemGernerated = entryHeader.IsEntryNumberSystemGernerated(CusEntryNumberTypes.Standard.MovementReferenceNumber);
				if (!movementReferenceNumber.IsEmpty && newMovementReferenceNumber != movementReferenceNumber && isMRNSystemGernerated)
				{
					logger.Log(Integration.LogType.Warning, string.Format(CultureInfo.InvariantCulture, "For the Movement Reference Number {0} already exists, so it will not be updated.", movementReferenceNumber));
				}
				else
				{
					var issueDate = entryHeader.MovementReferenceNumberIssueDate;
					var newIssueDate = eventData.GetContextValueByTypeAsDateTime(Constants.Universal.ContextType.CustomsDeclarationDate, issueDate);
					if (newIssueDate.IsEmpty || (isMRNSystemGernerated && newIssueDate.Date < issueDate))
					{
						newIssueDate = issueDate;
					}

					if (movementReferenceNumber != newMovementReferenceNumber || issueDate != newIssueDate || !isMRNSystemGernerated)
					{
						entryHeader.SetMovementReferenceNumber(newMovementReferenceNumber, newIssueDate);
					}

					if (newMovementReferenceNumber != entryHeader.PreEntryNumber || entryHeader.PreEntryNumberIssueDate != newIssueDate)
					{
						entryHeader.SetPreEntryNumber(newMovementReferenceNumber, newIssueDate);
					}
				}
			}

			UpdateCIQEntryNumber(logger, eventData, entryHeader);
		}

		void UpdateCIQEntryNumber(IXmlSessionTracker logger, UniversalEvent eventData, CusEntryHeader entryHeader)
		{
			var newCIQNumber = eventData.ParseCIQNumberFromNoteText();
			var ciqNumber = entryHeader.CIQNumber;
			var isCIQSystemGernerated = entryHeader.IsEntryNumberSystemGernerated(CusEntryNumberTypes.China.CIQNumber);
			if (!newCIQNumber.IsEmpty && !ciqNumber.IsEmpty && ciqNumber != newCIQNumber && isCIQSystemGernerated)
			{
				logger.Log(Integration.LogType.Warning, string.Format(CultureInfo.InvariantCulture, "For the CIQ Number {0} already exists, so it will not be updated.", ciqNumber));
			}
			else
			{
				if (newCIQNumber.IsEmpty)
				{
					newCIQNumber = ciqNumber;
				}

				if (!newCIQNumber.IsEmpty)
				{
					var issueDate = entryHeader.CIQIssueDate;
					var newIssueDate = eventData.ParseCIQIssueDateFromNoteText();
					if (newIssueDate.IsEmpty || (isCIQSystemGernerated && newIssueDate.Date < issueDate))
					{
						newIssueDate = issueDate;
					}

					if (ciqNumber != newCIQNumber || issueDate != newIssueDate || !isCIQSystemGernerated)
					{
						entryHeader.SetCIQNumber(newCIQNumber, newIssueDate);
					}

					var ciqStatus = entryHeader.CIQStatus;
					var newCIQStatus = eventData.GetContextValueByType(Constants.Universal.ContextType.EntryStatus);
					if (newCIQStatus != ciqStatus && CustomsStatusAttributeHelper.ShouldUpdateCIQStatus(entryHeader.Factory, newCIQStatus, CountryCode, AssessmentDate))
					{
						var lastIncomingMsg = entryHeader.GetLastIncomingMessageUpdatedCIQStatus();
						if (lastIncomingMsg == null || eventData.EventTime > lastIncomingMsg.GetUniversalEvent().EventTime)
						{
							entryHeader.SetCIQStatus(newCIQStatus);
							entryHeader.Logs.AddNew(Events.StatusChange, ZString.Format("CIQ Status Changed|NEW={0}|OLD={1}", newCIQStatus, ciqStatus), ZDateTimeOffset.Now);
						}
					}
				}
			}
		}

		#region HTML Interpretation

		void UpdateEDIMessage(UniversalEvent eventData, CusEntryHeader entryHeader, EDIMessage message)
		{
			if (message != null)
			{
				message.EM_LinkedObject = entryHeader;
				message.EM_MessageInterpretation = eventData.GetHtmlInterpretation();
			}
		}

		#endregion

		#region Email Notification

		void SendEmail(CusEntryHeader entryHeader, EDIMessage message, ZString messageStatus, ZString entryStatus)
		{
			var declaration = entryHeader.Declaration;
			if (declaration != null)
			{
				var emailAddresses = ResponseMessageProcessHelper.GetEmailAddressesToNotify(entryHeader.GetLastOutgoingMessage(), entryHeader);
				if (emailAddresses.Any())
				{
					ZString emailSubject = ZString.Empty;

					if (!entryStatus.IsEmpty && CustomsStatusAttributeHelper.ShouldNotify(entryHeader.Factory, entryStatus, CountryCode, AssessmentDate))
					{
						emailSubject = GetSubjectForEntryStatusUpdate(entryHeader, entryStatus);
					}

					if (!messageStatus.IsEmpty)
					{
						emailSubject = ResponseMessageProcessHelper.GetEmailSubjectWithReferenceNumbers(entryHeader, messageStatus, Business.Constants.MessageDescriptions.CSWResponseMessage);
					}

					if (!emailSubject.IsEmpty)
					{
						ResponseMessageProcessHelper.CreateMail(emailSubject, message.EM_MessageInterpretation, emailAddresses, declaration);
					}
				}
			}
		}

		ZString GetSubjectForEntryStatusUpdate(CusEntryHeader entryHeader, ZString entryStatus)
		{
			var statusDescription = entryHeader.Lookups.CH_EntryStatusList.GetDescriptionFromCode(entryStatus);
			return ZString.Format((NoResString)"单一窗口报关状态回执: {0} {1}, {2}",
				entryHeader.MovementReferenceNumber.IsEmpty ? entryHeader.DeclarationUnifiedNumber : entryHeader.MovementReferenceNumber,
				statusDescription, entryHeader.Declaration?.JE_DeclarationReference ?? ZString.Empty);
		}

		#endregion

		ZString CountryCode => Core.Constants.CountryCodes.China;

		ZDateTime AssessmentDate => ZDateTime.Today;
	}
}
