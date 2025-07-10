using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCUSRESMessage : CMRIncomingMessage
	{
		public CMRCUSRESMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly new TypeDecider TypeDecider = new CMRCUSRESMessageTypeDecider();

		public bool IsAir
		{
			get
			{
				var result = false;
				var tDT = GetTDTSegment(CUSRES.TDT, TransportStageCodeQualifierList.MainCarriageTransport);
				if (tDT != null)
				{
					result = tDT.TransportMeans.TransportMeansDescriptionCode == TransportMeansDescriptionCodeList.Aircraft;
				}
				return result;
			}
		}

		public bool IsSea
		{
			get
			{
				var result = false;
				var tDT = GetTDTSegment(CUSRES.TDT, TransportStageCodeQualifierList.MainCarriageTransport);
				if (tDT != null)
				{
					result = tDT.TransportMeans.TransportMeansDescriptionCode == TransportMeansDescriptionCodeList.Ship;
				}
				return result;
			}
		}

		bool IsERM
		{
			get
			{
				return BGMMessageType == CMRMessage.CMRMessageTypes.ERM;
			}
		}

		public ZString SendersReference
		{
			get
			{
				if (CUSRES != null)
				{
					var result = SendersReferenceCore;
					if (result.IsEmpty && IsERM)
					{
						result = GetReference(CUSRES.Group3, ReferenceFunctionCodeQualifierList.CarriersAgentReferenceNumber);
					}

					return result;
				}
				return ZString.Empty;
			}
		}

		protected virtual ZString SendersReferenceCore
		{
			get
			{
				return GetReference(CUSRES.Group3, ReferenceFunctionCodeQualifierList.OriginatorsReference);
			}
		}

		public ZInt SendersReferenceVersion
		{
			get
			{
				var result = ZInt.Zero;
				if (CUSRES != null)
				{
					foreach (SegmentGroup3 group3 in CUSRES.Group3)
					{
						foreach (RFFSegment rFF in group3.RFF)
						{
							if (rFF.Reference != null)
							{
								if (rFF.Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.OriginatorsReference.ToString())
								{
									ZInt.TryParse(rFF.Reference.ReferenceVersionIdentifier, out result);
									return result;
								}
							}
						}
					}
				}
				return result;
			}
		}

		public ZString EntryNumber
		{
			get
			{
				if (CUSRES != null)
				{
					return GetReference(CUSRES.Group3, ReferenceFunctionCodeQualifierList.CustomsDeclarationNumber);
				}
				else
				{
					return "";
				}
			}
		}

		public ZString DrawbackClaimID
		{
			get
			{
				if (CUSRES != null)
				{
					return GetReference(CUSRES.Group3, ReferenceFunctionCodeQualifierList.ExportReferenceNumber);
				}
				else
				{
					return "";
				}
			}
		}

		public ZString GetStatus()
		{
			return GetStatusCore();
		}

		protected virtual ZString GetStatusCore()
		{
			if (CUSRES != null)
			{
				foreach (FTXSegment fTX in CUSRES.FTX)
				{
					if (fTX.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.StatusDetails.ToString())
					{
						return fTX.TextLiteral.FreeTextValue1;
					}
				}
			}
			var statusDescription = GetStatusDescription();
			if (statusDescription.IndexOf("TRANSACTION WAS ACCEPTED") != -1)
			{
				return MessageBeingRespondedTo + " " + CMRMessageStatusDescription.ACCEPTED;
			}
			else
			{
				return MessageBeingRespondedTo + " " + CMRMessageStatusDescription.REJECTED;
			}
		}

		public ZString GetStatusDescription()
		{
			return GetStatusDescriptionCore();
		}

		protected virtual ZString GetStatusDescriptionCore()
		{
			var result = ZString.Empty;
			if (CUSRES != null)
			{
				var firstStatus = true;
				foreach (FTXSegment fTX in CUSRES.FTX)
				{
					if (fTX.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.StatusDetails.ToString())
					{
						if (ShowExtendedStatus)
						{
							if (firstStatus)
							{
								result += "\r\n";
							}

							result += "\t" + fTX.TextLiteral.FreeTextValue1 + ": " + fTX.TextLiteral.FreeTextValue2 + "\r\n";
						}
						else
						{
							result += fTX.TextLiteral.FreeTextValue2;
						}
						firstStatus = false;
					}
				}
				var advices = ZString.Empty;
				foreach (SegmentGroup4 group4 in CUSRES.Group4)
				{
					if (IsAdvice(group4.ERC))
					{
						foreach (FTXSegment fTX in group4.FTX)
						{
							advices += fTX.TextLiteral.FreeTextValue1 + "\r\n";
						}
					}
				}
				if (!advices.IsEmpty)
				{
					result += advices.TrimEnd('\n').TrimEnd('\r');
				}
			}
			return result;
		}

		public ZString GetCargoStatusForTransportLine(ZShort cargoLineNumber)
		{
			return GetCargoStatusForTransportLineCore(cargoLineNumber);
		}

		protected virtual ZString GetCargoStatusForTransportLineCore(ZShort cargoLineNumber)
		{
			return GetStatusFromFTXSegment();
		}

		public ZString AbbreviatedStatusDescriptionForTransportLine(ZShort cargoLineNumber)
		{
			return AbbreviatedStatusDescriptionForTransportLineCore(cargoLineNumber);
		}

		protected virtual ZString AbbreviatedStatusDescriptionForTransportLineCore(ZShort cargoLineNumber)
		{
			return ZString.Empty;
		}

		public ZString GetCustomsStatusFromMessage()
		{
			return GetCustomsStatusFromMessageCore();
		}

		protected virtual ZString GetCustomsStatusFromMessageCore()
		{
			return GetStatusFromFTXSegment();
		}

		ZString GetStatusFromFTXSegment()
		{
			var result = ZString.Empty;
			if (CUSRES != null)
			{
				foreach (FTXSegment fTX in CUSRES.FTX)
				{
					if (fTX.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.StatusDetails)
					{
						result = fTX.TextLiteral.FreeTextValue2;
						break;
					}
				}
			}
			return result;
		}

		protected virtual bool ShowExtendedStatus
		{
			get { return false; }
		}

		public ZString MessageBeingRespondedTo
		{
			get
			{
				var result = ZString.Empty;
				if (CUSRES != null)
				{
					result = GetMessageNameFromCode(GetReference(CUSRES.Group3, ReferenceFunctionCodeQualifierList.SecondaryCustomsReference));
				}
				if (result.IsEmpty)
				{
					result = "TRANSACTION";
				}

				return result;
			}
		}

		public ZString GetMessageNameFromCode(ZString messageFunctionCode)
		{
			switch (messageFunctionCode)
			{
				case "4":
					return "CHANGE";
				case "5":
					return "REPLACE";
				case "9":
					return "ORIGINAL";
				case "20":
					return "REPLACE-HEADING-SECTION-ONLY";
				case "21":
					return "REPLACE-ITEM-DETAIL-AND-SUMMARY-ONLY";
				case "50":
					return "WITHDRAW";
			}
			return ZString.Empty;
		}

		protected bool IsAdvice(ERCSegmentMessageSection eRCSection)
		{
			var result = false;
			foreach (ERCSegment eRC in eRCSection)
			{
				if (eRC.ApplicationErrorDetail.ApplicationErrorIdentification == "ADVICE")
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public void SetEM_LinkedObject()
		{
			var bizo = GetWrappedObject();
			if (bizo != null)
			{
				LinkOrCloneMessage(bizo);
			}
		}

		#region Link or Clone

		public CMRCUSRESMessage LinkOrCloneMessage(BusinessObject linkedBizO)
		{
			return LinkOrCloneMessageCore(CMRRespondeeWrapper.GetWrapper(linkedBizO).Messages);
		}

		protected virtual CMRCUSRESMessage LinkOrCloneMessageCore(EDIMessageCollection messages)
		{
			var messageToAdd = this;
			if (messages != null && !messages.ContainsByKeyFields(this))
			{
				if (messageToAdd.EM_LinkedObject != null)
				{
					var args = new BusinessObjectCloneArgs(EM_MessageType == CMRMessageTypes.SEI ? new string[] { EDIMessageSchema.EM_MessageText.Name } : System.Array.Empty<string>());
					messageToAdd = (CMRCUSRESMessage)messageToAdd.Clone(args);
					// Perhaps we should be keeping track of all the clones and then failing them all if the main one fails?
					// What we _really_ need is a many-to-many, but that's a huge change.
					messageToAdd.EM_Status = EDIMessage.Status.Received;
				}
				messages.Add(messageToAdd);
			}
			return messageToAdd;
		}

		#endregion

		public ZString GetEM_LinkedObjectDetails(bool isForHTMLReport)
		{
			var linkedObject = EM_LinkedObject;
			var result = CMRRespondeeWrapper.GetWrapper(linkedObject).Details;
			var responseReference = linkedObject as ICMRMessageRespondeeReference;
			return responseReference != null ? responseReference.GetHTMLFormatDetailsIfNeeded(result, isForHTMLReport) : result;
		}

		#region Report

		public override ZString GetReport()
		{
			return GetReport(false);
		}

		ZString statusType;
		protected ZString GetReport(bool isForHTMLReport)
		{
			var builder = new StringBuilder();

			if (CUSRES != null)
			{
				statusType = GetStatus();
				var statusDescription = GetStatusDescription();

				if (EM_LinkedObject != null)
				{
					builder.Append(GetEM_LinkedObjectDetails(isForHTMLReport) + "\r\n");
				}

				builder.Append(AdditionalInfoForStatusSectionOfReport());

				builder.Append("Status: " + statusType + "\r\n");

				if (!statusDescription.IsEmpty)
				{
					builder.Append("Status Description: " + statusDescription + "\r\n");
				}

				builder.Append(AdditionalInfoForHeaderSectionOfReport());

				if (!isForHTMLReport)
				{
					builder.Append(GetErrorsSection(statusType));
					if (EM_LinkedObject != null)
					{
						builder.Append(StatusOfLinesReport);
					}
				}
			}

			return builder.ToString();
		}

		protected virtual ZString AdditionalInfoForHeaderSectionOfReport()
		{
			return ZString.Empty;
		}

		public virtual ZString AdditionalInfoForStatusSectionOfReport()
		{
			return ZString.Empty;
		}

		public override ZString ErrorNotificationsCaption
		{
			get
			{
				return GetErrorSectionDescription(statusType);
			}
		}

		protected override ZString GetReportForHTMLHeaderSection()
		{
			var reportBuilder = new StringBuilder();
			reportBuilder.Append(base.GetReportForHTMLHeaderSection());
			reportBuilder.Append(GetReport(true));
			return reportBuilder.ToString();
		}

		protected override List<MessageNotification> GetErrorNotifications()
		{
			var result = base.GetErrorNotifications();
			if (CUSRES != null && CUSRES.Group4.Count > 0)
			{
				foreach (SegmentGroup4 group4 in CUSRES.Group4)
				{
					if (!IsAdvice(group4.ERC))
					{
						var errorLocation = AdditionalLineReference(group4.ERP[0].ErrorPointDetails.MessageSubItemNumber);
						var errorCode = ZString.Empty;
						var additionalMessageAdviceForThisError = ZString.Empty;
						if (group4.ERC.Count > 0)
						{
							errorCode = group4.ERC[0].ApplicationErrorDetail.ApplicationErrorIdentification;
							additionalMessageAdviceForThisError = GetAdditionalMessageAdviceForThisError(errorCode);
						}
						foreach (FTXSegment currentFTX in group4.FTX)
						{
							if (currentFTX.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.ErrorDescriptionFreeText)
							{
								result.Add(new MessageNotification(errorLocation, errorCode, currentFTX.TextLiteral.FreeTextValue1));
							}
						}
						if (additionalMessageAdviceForThisError != "")
						{
							result.Add(new MessageNotification(ZString.Empty, ZString.Empty, additionalMessageAdviceForThisError));
						}
					}
				}
			}
			return result;
		}

		protected override List<MessageNotification> GetStatusNotifications()
		{
			var result = base.GetStatusNotifications();

			if (CUSRES != null)
			{
				foreach (SegmentGroup6 group6 in CUSRES.Group6)
				{
					if (group6.Group11.Count > 0)
					{
						ZString lineNumber = group6.Group11[0].CST.Count > 0 ? group6.Group11[0].CST[0].GoodsItemNumber : string.Empty;
						ZString cAN = group6.RFF.Count > 0 ? group6.RFF[0].Reference.ReferenceIdentifier : string.Empty;
						ZString lineStatusType = group6.Group11[0].FTX.Count > 0 ? group6.Group11[0].FTX[0].TextLiteral.FreeTextValue1 : string.Empty;
						ZString lineStatusDescription = group6.Group11[0].FTX.Count > 0 ? group6.Group11[0].FTX[0].TextLiteral.FreeTextValue2 : string.Empty;
						var location = ZString.Empty;
						if (!lineNumber.IsEmpty)
						{
							location += "Line: " + lineNumber + " ";
						}

						location += AdditionalLineReference(lineNumber) + " ";
						if (!cAN.IsEmpty)
						{
							location += "CAN: " + cAN;
						}

						result.Add(new MessageNotification(location, lineStatusType, lineStatusDescription));
					}
				}
			}
			return result;
		}

		public override ZString LineStatusNotificationsCaption
		{
			get { return "Status of Lines:"; }
		}

		protected override List<MessageNotification> GetAdviceNotifications()
		{
			var result = base.GetAdviceNotifications();
			if (CUSRES != null && CUSRES.Group6.Count > 0)
			{
				foreach (SegmentGroup6 group6 in CUSRES.Group6)
				{
					if (group6.Group13.Count > 0)
					{
						foreach (SegmentGroup13 group13 in group6.Group13)
						{
							var lineNumber = ZString.Empty;
							ZString adviceCode = group13.ERP.Count > 0 ? group13.ERP[0].ErrorPointDetails.MessageSubItemNumber : string.Empty;
							foreach (ERCSegment currentERC in group13.ERC)
							{
								lineNumber = AdditionalLineReference(currentERC.ApplicationErrorDetail.ApplicationErrorIdentification);
								if (group13.FTX.Count > 0)
								{
									foreach (FTXSegment currentFTX in group13.FTX)
									{
										if (currentFTX.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.AdditionalConditions)
										{
											result.Add(new MessageNotification(lineNumber, adviceCode, currentFTX.TextLiteral.FreeTextValue1));
										}
									}
								}
							}
						}
					}
				}
			}
			return result;
		}

		public ZString GetErrorsSection(ZString status)
		{
			var builder = new StringBuilder();

			var errorList = GetErrorsArrayList();
			if (errorList.Count > 0)
			{
				builder.Append(GetErrorSectionDescription(status));
				foreach (string error in errorList)
				{
					builder.Append("\t" + error + "\r\n");
				}
			}
			return builder.ToString();
		}

		protected virtual ZString GetErrorSectionDescription(ZString status)
		{
			return "\r\n\r\nErrors:\r\n";
		}

		#endregion

		public EDIMessage LastSentOrPendingOutgoingCMRMessage
		{
			get
			{
				var messages = CMRRespondeeWrapper.GetWrapper(EM_LinkedObject).Messages;
				return messages?.GetLastMessage(EDIMessage.ApplicationCodes.CMR, ZString.Empty, EDIMessage.Direction.Transmit, new ZString[] { EDIMessage.Status.Sent, EDIMessage.Status.Pending });
			}
		}

		public EDIMessage GetOutgoingMessageByBGMRefAndVersion(EDIMessageCollection messages)
		{
			return GetOutgoingMessageByBGMRefAndVersion(messages, ZString.Empty);
		}

		public EDIMessage GetOutgoingMessageByBGMRefAndVersion(EDIMessageCollection messages, ZString messageType)
		{
			var sendersReferenceCached = SendersReference;
			var sendersReferenceVersionCached = SendersReferenceVersion;
			foreach (var message in messages.Cast<EDIMessage>())
			{
				var cMRMessage = message as CMRMessage;

				if (cMRMessage != null
					&& cMRMessage.EM_ReceiveTransmit == EDIMessage.Direction.Transmit
					&& cMRMessage.BGMReference == sendersReferenceCached
					&& cMRMessage.BGMReferenceVersion == sendersReferenceVersionCached
					&& (messageType.IsEmpty || cMRMessage.EM_MessageType == messageType))
				{
					return message;
				}
			}
			return null;
		}

		public void ReleasePendingMessages()
		{
			SetAllPendingMessagesStatus(EDIMessage.Status.Queued);
		}

		public void CancelPendingMessages(CMRCUSRESMessage incomingMessage)
		{
			SetAllPendingMessagesStatus(EDIMessage.Status.Cancelled);
			if (IsOutturnResponse(incomingMessage.EM_MessageType))
			{
				AddSplitMessageLog(incomingMessage.LastSentOrPendingOutgoingCMRMessage);
			}
		}
		internal const string splitMessageIdentifier = "SMI";
		internal const string splitMessageOriginalCancelled = "Split Outturn message has been rejected. Correct the Outturn error and resend as an Original.";
		internal const string splitMessageCancelledReference = "Split Outturn message not wholly processed. Outturn in an inconsistent state. Withdraw the Outturn and resend as an Original.";

		bool IsOutturnResponse(string messageType)
		{
			return messageType == CMRMessageTypes.AIROUT || messageType == CMRMessageTypes.SEAOUT;
		}

		internal static void AddSplitMessageLog(EDIMessage lastSentMessage)
		{
			if (lastSentMessage != null)
			{
				if (lastSentMessage.EM_ApplicationReference.StartsWith(splitMessageIdentifier, System.StringComparison.OrdinalIgnoreCase))
				{
					var underbond = lastSentMessage.EM_LinkedObject as CusUnderbond;
					if (underbond != null)
					{
						AddUnderbondSplitMessageLog(underbond, lastSentMessage);
					}
					else
					{
						var outturnHeader = lastSentMessage.EM_LinkedObject as CusOutturnHeader;
						if (outturnHeader != null)
						{
							AddOutturnHeaderSplitMessageLog(outturnHeader, lastSentMessage);
						}
					}
				}
			}
		}

		static void AddUnderbondSplitMessageLog(CusUnderbond underbond, EDIMessage lastSentMessage)
		{
			var outMsg = lastSentMessage as CMRMessage;
			if (outMsg != null && outMsg.BGMMessageFunctionCode == MessageFunctionCodeList.Original)
			{
				underbond.CusUnderbondOutturnLogManager.AddANewSplitMessageOriginalRejectedLog(splitMessageOriginalCancelled);
				underbond.OutturnStatus.Code = CMRBaseStatuses.Codes.OriginalRejected;
			}
			else
			{
				underbond.CusUnderbondOutturnLogManager.AddANewSplitMessageFailedLog(splitMessageCancelledReference);
				underbond.OutturnStatus.Code = CMRBaseStatuses.Codes.AmendmentRejected;
			}
		}

		static void AddOutturnHeaderSplitMessageLog(CusOutturnHeader outturnHeader, EDIMessage lastSentMessage)
		{
			var outMsg = lastSentMessage as CMRMessage;
			if (outMsg != null && outMsg.BGMMessageFunctionCode == MessageFunctionCodeList.Original)
			{
				outturnHeader.CusUnderbondOutturnLogManager.AddANewSplitMessageOriginalRejectedLog(splitMessageOriginalCancelled);
				outturnHeader.OutturnStatus.Code = CMRBaseStatuses.Codes.OriginalRejected;
			}
			else
			{
				outturnHeader.CusUnderbondOutturnLogManager.AddANewSplitMessageFailedLog(splitMessageCancelledReference);
				outturnHeader.OutturnStatus.Code = CMRBaseStatuses.Codes.AmendmentRejected;
			}
		}

		void SetAllPendingMessagesStatus(ZString newStatus)
		{
			var messages = CMRRespondeeWrapper.GetWrapper(EM_LinkedObject).Messages;
			if (messages != null)
			{
				messages.Sort(new SortInfo(EDIMessage.Schema.EM_ApplicationReference, ListSortDirection.Ascending));
				foreach (var message in messages.Cast<EDIMessage>())
				{
					if (message.EM_Status == EDIMessage.Status.Pending)
					{
						message.EM_Status = newStatus;
						if ((message.EM_MessageType == CMRMessageTypes.AIROUT || message.EM_MessageType == CMRMessageTypes.SEAOUT) && newStatus == EDIMessage.Status.Queued)
						{
							break;
						}
					}
				}
			}
		}

		protected virtual internal string GetReferenceFromSendersReference()
		{
			return GetReferenceFromSendersReference(SendersReference);
		}

		protected internal string GetReferenceFromSendersReference(string sendersReference)
		{
			var result = sendersReference;
			var endOfReference = result.LastIndexOf("/");
			if (endOfReference != -1)
			{
				result = result.Substring(0, endOfReference);
			}

			return result;
		}

		protected virtual internal BusinessObject GetWrappedObject()
		{
			BusinessObject result = null;
			ZString reference = GetReferenceFromSendersReference();

			if (reference.StartsWith("C"))
			{
				if (reference.StartsWith("CE"))
				{
					result = ConsolidatedDeclaration.LoadFromRef(Factory, reference);
				}
				else
				{
					result = ForwardingConsol.LoadFromRef(Factory, reference);
				}
			}
			else if (reference.StartsWith("S") || reference.StartsWith("B"))
			{
				if (reference.Contains(CusEntryHeader.ReferenceNumberSeparator))// Imports
				{
					result = CusEntryHeader.LoadForBGMReferenceAndEntryNumber(Factory, reference, EntryNumber);
				}
				else// Exports
				{
					var declaration = JobDeclaration.LoadFirstMatchingInCurrentCompanyIncludingInActive(Factory, reference);
					result = declaration?.ActiveEntryHeaders.FirstOrDefault() ?? declaration;
				}
			}
			else if (reference.StartsWith("K"))
			{
				if (reference.StartsWith("KL"))
				{
					result = ExportCustomsManifestLines.Load(Factory, reference);
				}
				else
				{
					result = ExportCustomsManifestHeader.Load(Factory, reference);
				}
			}
			else if (reference.StartsWith("U"))
			{
				result = CusUnderbond.LoadFromSendersReference(Factory, reference);
			}
			else if (reference.StartsWith("T"))
			{
				result = CusSeaManTranHead.LoadFromSendersReference(Factory, reference);
			}
			else if (reference.StartsWith("A"))
			{
				result = CusSeaManArrivalPort.LoadFromSendersReference(Factory, reference);
			}
			else if (reference.StartsWith("H"))
			{
				result = CusSeaManOBLHeader.LoadFromSendersReference(Factory, reference);
			}
			else if (reference.StartsWith("L"))
			{
				result = CusSCAHouse.Load(Factory, reference);
			}
			else if (reference.StartsWith("J"))
			{
				result = Factory.LoadTop1<JobVoyage>(new ZQuery(JobVoyageSchema.JV_SendersMessageReference, reference));
				if (result != null)
				{
					result = new CustomsJobVoyageWrapper(result as JobVoyage);
				}
			}
			else if (reference.StartsWith("D"))
			{
				result = Factory.LoadTop1<VoyageDestination>(new ZQuery(JobVoyDestinationSchema.JB_SendersMessageReference, reference));
				if (result != null)
				{
					var destination = result as VoyageDestination;
					var voyageWrapper = new CustomsJobVoyageWrapper(destination.Voyage);
					result = new CustomsVoyageDestinationWrapper(voyageWrapper, destination);
				}
			}
			else if (reference.StartsWith("M"))
			{
				result = CTOCusHAWB.LoadFromSendersReference(Factory, reference);
			}
			else if (reference.StartsWith("P"))
			{
				result = CusPartShip.LoadFromSendersReference(Factory, reference);
			}
			//if (Result != null)
			//{
			//  ErrorReporter.ReportOnce("New BASE CMRDeclaration GetWrappedObject hit, Dept:" + Reference.Left(1),
			//    " Message Type:" + EM_MessageType +
			//    " Message Sub-Type:" + EM_MessageSubType +
			//    " BGMRef:" + Reference +
			//    " EntryNo: " + EntryNumber +
			//    " Message Text: " + EM_MessageText);
			//}
			return result;
		}

		CUSRESMessage fCUSRES;
		protected CUSRESMessage CUSRES
		{
			get
			{
				if (fCUSRES == null)
				{
					fCUSRES = (CUSRESMessage)GetAutoEdifactMessageUsingNamedFactory(AuEdifactMessageFactory.AUCMessageFactory, new Edifact.UNOCCMRCharacterSet());
				}
				return fCUSRES;
			}
		}

		public void ResetCUSRESCache()
		{
			fCUSRES = null;
		}

#if DEBUG
		public CUSRESMessage CUSRESForTesting
		{
			get
			{
				return CUSRES;
			}
		}

		public CUSRESMessage CUSRESCacheForTesting
		{
			get
			{
				return fCUSRES;
			}
		}
#endif

		protected ZString GetReference(SegmentGroup3MessageSection group3Section, ReferenceFunctionCodeQualifierList referenceCode)
		{
			foreach (SegmentGroup3 group3 in group3Section)
			{
				foreach (RFFSegment rFF in group3.RFF)
				{
					if (rFF.Reference != null)
					{
						string refCode = rFF.Reference.ReferenceFunctionCodeQualifier;
						if (refCode == referenceCode.ToString())
						{
							return rFF.Reference.ReferenceIdentifier;
						}
					}
				}
			}
			return ZString.Empty;
		}

		protected ZString GetReference(SegmentGroup3MessageSection group3Section, string referenceCode)
		{
			foreach (SegmentGroup3 group3 in group3Section)
			{
				foreach (RFFSegment rFF in group3.RFF)
				{
					if (rFF.Reference != null)
					{
						string refCode = rFF.Reference.ReferenceFunctionCodeQualifier;
						if (refCode == referenceCode)
						{
							return rFF.Reference.ReferenceIdentifier;
						}
					}
				}
			}
			return ZString.Empty;
		}

		protected ZString GetReference(SegmentGroup6MessageSection group6Section, ReferenceFunctionCodeQualifierList referenceCode)
		{
			foreach (SegmentGroup6 group6 in group6Section)
			{
				foreach (RFFSegment rFF in group6.RFF)
				{
					if (rFF.Reference != null)
					{
						string refCode = rFF.Reference.ReferenceFunctionCodeQualifier;
						if (refCode == referenceCode.ToString())
						{
							return rFF.Reference.ReferenceIdentifier;
						}
					}
				}
			}
			return ZString.Empty;
		}

		public virtual ZString StatusOfLinesReport
		{
			get
			{
				var result = ZString.Empty;

				if (CUSRES.Group6.Count > 0)
				{
					result += "\r\nStatus of Lines:\r\n";
				}

				foreach (SegmentGroup6 group6 in CUSRES.Group6)
				{
					ZString lineNumber = group6.Group11[0].CST[0].GoodsItemNumber;
					ZString cAN = group6.RFF[0].Reference.ReferenceIdentifier;
					ZString lineStatusType = group6.Group11[0].FTX[0].TextLiteral.FreeTextValue1;
					ZString lineStatusDescription = group6.Group11[0].FTX[0].TextLiteral.FreeTextValue2;

					if (!lineNumber.IsEmpty)
					{
						result += "Line: " + lineNumber + "\r\n";
					}

					result += AdditionalLineReference(lineNumber);

					if (!cAN.IsEmpty)
					{
						result += "\tCAN: " + cAN + "\r\n";
					}

					if (!lineStatusType.IsEmpty)
					{
						result += "\tStatus: " + lineStatusType + "\r\n";
					}

					if (!lineStatusDescription.IsEmpty)
					{
						result += "\tStatus Description: " + lineStatusDescription + "\r\n";
					}

					result += "\r\n";
				}
				return result;
			}
		}

		protected internal virtual ArrayList GetErrorsArrayList()
		{
			var errorList = new ArrayList();
			foreach (SegmentGroup4 group4 in CUSRES.Group4)
			{
				if (!IsAdvice(group4.ERC))
				{
					var errorLocation = AdditionalLineReference(group4.ERP[0].ErrorPointDetails.MessageSubItemNumber);
					if (!errorLocation.IsEmpty)
					{
						errorLocation = " (" + errorLocation + ")";
					}

					foreach (FTXSegment fTX in group4.FTX)
					{
						errorList.Add(fTX.TextLiteral.FreeTextValue1 + errorLocation);
					}

					if (group4.ERC.Count > 0)
					{
						ZString additionalMessageAdviceForThisError = GetAdditionalMessageAdviceForThisError(group4.ERC[0].ApplicationErrorDetail.ApplicationErrorIdentification);
						if (additionalMessageAdviceForThisError != "")
						{
							errorList.Add(additionalMessageAdviceForThisError + "\r\n\r\n");
						}
					}
				}
			}
			return errorList;
		}

		protected virtual string GetAdditionalMessageAdviceForThisError(string errorCode)
		{
			return "";
		}

		protected virtual ZString AdditionalLineReference(ZString lineNumber)
		{
			var consol = EM_LinkedObject as ForwardingConsol;
			if (consol != null)
			{
				var line = LineKeys().FirstOrDefault(x => x.LineNumber == lineNumber);
				var shipment = consol.Shipments.Cast<ForwardingShipment>().FirstOrDefault(x => x.JS_HouseBill.EqualsIgnoringCase(line.houseBill));
				return shipment != null ? "\tReference: " + shipment.JS_UniqueConsignRef + "\r\n" : string.Empty;
			}
			else
			{
				var declaration = EM_LinkedObject as JobDeclaration;
				if (declaration != null)
				{
					ZInt lineNum;
					if (ZInt.TryParse(lineNumber, out lineNum))
					{
						lineNum--;
						var sortedLines = declaration.SortedInvoiceLines;
						if (lineNum >= 0 && lineNum < sortedLines.Length)
						{
							return sortedLines[lineNum].InvoiceAndLineReference;
						}
					}
				}
			}
			return ZString.Empty;
		}

		protected TDTSegment GetTDTSegment(TDTSegmentMessageSection tDTSegments, TransportStageCodeQualifierList code)
		{
			foreach (TDTSegment tDT in tDTSegments)
			{
				if (tDT.TransportStageCodeQualifier == code)
				{
					return tDT;
				}
			}
			return null;
		}

		protected LOCSegment GetLOCSegment(LOCSegmentMessageSection lOCSegments, LocationFunctionCodeQualifierList code)
		{
			foreach (LOCSegment lOC in lOCSegments)
			{
				if (lOC.LocationFunctionCodeQualifier == code)
				{
					return lOC;
				}
			}
			return null;
		}

		public ZDateTime ProcessingDate
		{
			get
			{
				if (fProcessingDate == null)
				{
					fProcessingDate = ZDateTime.Empty;
					foreach (DTMSegment dTM in CUSRES.DTM)
					{
						if (dTM.DateTimePeriod.DateTimePeriodFunctionCodeQualifier == DateTimePeriodFunctionCodeQualifierList.ProcessingDateTime)
						{
							ZDateTime result;
							if (ZDateTime.TryParseExact(new ZString(dTM.DateTimePeriod.DateTimePeriodValue).SubstringSafe(0, 14), out result, "yyyyMMddHHmmss"))
							{
								fProcessingDate = result;
								break;
							}
						}
					}
				}
				return fProcessingDate.Value;
			}
		}
		ZDateTime? fProcessingDate;

		#region Message Advice Errors

		public ZString GetMessageAdviceErrors()
		{
			var messageAdvice = ZString.Empty;

			if (CUSRES != null && CUSRES.Group6.Count > 0)
			{
				foreach (SegmentGroup6 group6 in CUSRES.Group6)
				{
					if (group6.Group13.Count > 0)
					{
						if (!fAddedMessageAdviceHeading)
						{
							messageAdvice += "\r\nMessage Advice:\r\n";
							fAddedMessageAdviceHeading = true;
						}

						messageAdvice += GetMessageAdviceErrorsForGroup6(group6);
					}
				}
			}

			return messageAdvice;
		}
		bool fAddedMessageAdviceHeading;

		ZString GetMessageAdviceErrorsForGroup6(SegmentGroup6 group6)
		{
			var result = new ZStringBuilder();

			foreach (SegmentGroup13 group13 in group6.Group13)
			{
				var lineNumber = ZString.Empty;

				foreach (ERCSegment currentERC in group13.ERC)
				{
					lineNumber = AdditionalLineReference(currentERC.ApplicationErrorDetail.ApplicationErrorIdentification);

					var adviceMessage = ZString.Empty;

					if (group13.FTX.Count > 0)
					{
						adviceMessage = group13.FTX[0].TextLiteral.FreeTextValue1;
					}

					if (!adviceMessage.IsEmpty)
					{
						result.Append("\t" + adviceMessage);

						if (!lineNumber.IsEmpty)
						{
							result.Append(" (" + lineNumber + ")\r\n");
						}
					}
				}
			}

			return result.ToString();
		}

		#endregion
	}
}
