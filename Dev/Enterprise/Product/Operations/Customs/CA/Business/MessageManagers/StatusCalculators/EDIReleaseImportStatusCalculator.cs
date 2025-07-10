namespace Enterprise.Customs.CA.Business.MessageManagers
{
	using System.ComponentModel;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.CA.Business.MessageProcessors;
	using Enterprise.Customs.CA.Messaging;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Customs.Common.Shared;
	using Enterprise.Edifact.D96A.Elements;
	using Enterprise.Edifact.D96A.Messages.CUSRES;
	using Enterprise.Edifact.D96A.Segments;
	using MessageTypeList = MessageTypeList;

	public class EDIReleaseImportStatusCalculator : EDIFACTMessageStatusCalculator
	{
		public override ZString MessageTypeDescription
		{
			get { return MessageTypeList.Descriptions.EDIRelease; }
		}

		internal bool IsAwaitingReply(IEDIFACTMessageAttachee linkedObject, EDIReleaseMessage message)
		{
			var releaseAttachee = linkedObject as IEDIReleaseMessageAttachee;
			return IsAwaitingReply(linkedObject.MessageStatus)
				   || (!message.CargoControlNumber.IsEmpty
					   && releaseAttachee != null && releaseAttachee.CargoControlNumbersAwaitingReply);
		}

		internal static bool IsShipmentCleared(BusinessObject jobDeclaration)
		{
			var result = false;
			var declaration = jobDeclaration as JobDeclaration;
			if (declaration != null)
			{
				result = declaration.IsLVS;
				if (!result)
				{
					var ediReleaseEntryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.EDIRelease);
					if (ediReleaseEntryHeader != null)
					{
						result = IsClearedStatus(ediReleaseEntryHeader.CH_EntryStatus) ||
							ediReleaseEntryHeader.CH_EntryStatus == EDIReleaseImportEntryStatusList.Codes.MultipleCargoControlNumber &&
							declaration.ReleaseStatuses.Cast<ReleaseStatus>().All(o => !o.IsPersistent || IsClearedStatus(o.RL_ReleaseStatus));
					}
				}
			}
			return result;
		}

		static bool IsClearedStatus(ZString entryStatus)
		{
			return entryStatus == EDIReleaseImportEntryStatusList.Codes.AuthorisedToDeliver ||
				entryStatus == EDIReleaseImportEntryStatusList.Codes.GoodsReleased ||
				entryStatus == EDIReleaseImportEntryStatusList.Codes.Y51ReleaseDocumentsRequired ||
				entryStatus == EDIReleaseImportEntryStatusList.Codes.ManualRelease;
		}

		internal ZString GetMessageSubType(ZString currentMessageStatus, ProcessingIndicatorCodedList processingIndicator)
		{
			var subType = ZString.Empty;

			if (processingIndicator == ProcessingIndicatorCodedList.DeclarationAcceptedAwaitingGoodsArrival)
			{
				if (currentMessageStatus == MessageStatusList.Codes.ErrorDelete)
				{
					subType = MessageSubTypeCodes.Codes.Original;
				}
				else
				{
					subType = GetMessageSubType(currentMessageStatus);
				}
			}
			else
			{
				subType = GetMessageSubType(currentMessageStatus);
				if (subType == MessageSubTypeCodes.Codes.Cancellation)
				{
					subType = MessageSubTypeCodes.Codes.Original;
				}
			}
			return subType;
		}

		#region CalculatedJobStatus

		public override ZString CalculatedJobStatus(IEDIFACTMessageAttachee linkedObject)
		{
			var releaseAttachee = linkedObject as IEDIReleaseMessageAttachee;
			var releaseStatus = releaseAttachee != null ? releaseAttachee.CargoControlNumbersReleaseStatus : ZString.Empty;
			return releaseStatus.IsEmpty ? CalculatedJobStatusCore(linkedObject) : releaseStatus;
		}

		static ZString CalculatedJobStatusCore(IEDIFACTMessageAttachee linkedObject)
		{
			var resultStatus = ZString.Empty;
			var acceptedReplyFound = false;
			var syntaxErrorFound = false;

			foreach (var inMessage in linkedObject.Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.CAIMP, new ZString[] { MessageTypeList.Codes.EDIRelease }, EDIMessage.Direction.Receive, true, ListSortDirection.Descending))
			{
				var cusresMessage = (CUSRESMessage)inMessage.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet());
				if (cusresMessage != null && cusresMessage.GIS.Count > 0)
				{
					ZString status = EDIReleaseImportEntryStatusList.GetEntryStatusByProcessingIndicatorCoded(cusresMessage.GIS[0].ProcessingIndicator.ProcessingIndicatorCoded, inMessage.EM_MessageSubType);
					if (!status.IsEmpty)
					{
						switch (status)
						{
							case EDIReleaseImportEntryStatusList.Codes.MessageContentAccepted:
								if (inMessage.EM_MessageSubType == MessageSubTypeCodes.Codes.Cancellation)
								{
									resultStatus = EDIReleaseImportEntryStatusList.Codes.Cancelled;
								}
								acceptedReplyFound = true;
								break;
							case EDIReleaseImportEntryStatusList.Codes.MessageContentRejected:
							case EDIReleaseImportEntryStatusList.Codes.Error:
								if (!acceptedReplyFound)
								{
									if (!IsSyntaxError(cusresMessage))
									{
										resultStatus = EDIReleaseImportEntryStatusList.Codes.Error;
									}
									else
									{
										syntaxErrorFound = true;
									}
								}
								break;
							default:
								resultStatus = status;
								break;
						}
					}

					if (!resultStatus.IsEmpty)
					{
						break;
					}
				}
			}

			if (resultStatus.IsEmpty)
			{
				if (acceptedReplyFound)
				{
					resultStatus = EDIReleaseImportEntryStatusList.Codes.MessageContentAccepted;
				}
				else if (syntaxErrorFound)
				{
					resultStatus = EDIReleaseImportEntryStatusList.Codes.SyntaxError;
				}
				else
				{
					resultStatus = linkedObject.JobStatus;
				}
			}

			return resultStatus;
		}

		static bool IsSyntaxError(CUSRESMessage cusresMessage)
		{
			return (from SegmentGroup1 group1 in cusresMessage.Group1
					from ERPSegment erp in group1.ERP
					let number = erp.ErrorPointDetails.MessageSubItemNumber
					where number == "28" || number == "29"
					select erp).Any();
		}

		#endregion

		public ZString OverrideMessageSubType { get; set; }
	}
}
