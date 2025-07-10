using System;
using System.Collections;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AIROUTMessageBuilder : CMRCUSCARMessageBuilder
	{
		public AIROUTMessageBuilder(IAirOutturnReportHeaderInformation reportHeader, IAirOutturnReportLineInformation[] lines, bool splitMessage, bool subsequentSplitMessages, bool shouldDelaySending = false)
		{
			this.reportHeader = reportHeader;
			this.lines = lines;
			this.isSplitMessage = splitMessage;
			this.isSubsequentSplitMessage = subsequentSplitMessages;
			this.shouldDelaySending = shouldDelaySending;
		}

		readonly IAirOutturnReportHeaderInformation reportHeader;
		readonly IAirOutturnReportLineInformation[] lines;
		readonly bool isSplitMessage;
		readonly bool isSubsequentSplitMessage;
		readonly bool shouldDelaySending;

		protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.AIROUT;

		protected internal override Type TypeOfMessage => typeof(CMRAIROUTMessage);

		protected internal override ZString DocumentName => "AIROUT";

		protected internal override DocumentNameCodeList DocumentNameCode => DocumentNameCodeList.InventoryAdjustmentStatusReport;

		protected internal override void GenerateMessageText()
		{
			if (CUSCAR == null)
			{
				CUSCAR = new Edifact.D99B.Messages.CUSCAR.CUSCARMessage();
				PopulateUNH();
				if (isSubsequentSplitMessage)
				{
					PopulateSplitMessageBGM();
				}
				else
				{
					PopulateBGM();
				}

				PopulateDTM();
				PopulateNAD();
				PopulateTDT();
				if (CUSCAR.Group4.Count > 0)
				{
					PopulateEstimatedDateOfArrivalDTM();
					PopulateLOC();
				}

				PopulateGroup7(lines);
				PopulateUNT();
			}
		}

		protected void PopulateSplitMessageBGM()
		{
			MessageUtilities.PopulateBGM(BGM, DocumentNameCode, DocumentName, BGMReference, Version.ToString(), MessageFunctionCodeList.Change);
		}

		protected override void SetAdditionalEDIMessageDetails(EDIMessage message)
		{
			base.SetAdditionalEDIMessageDetails(message);
			if (SetStatusToPending)
			{
				message.EM_Status = EDIMessage.Status.Pending;
			}

			if (SetSplitMessageIdentifier)
			{
				message.EM_ApplicationReference = CMRCUSRESMessage.splitMessageIdentifier + CurrentVersion.ToString().PadLeft(7, '0');
				message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMilliseconds(CurrentVersion);  // Cusres processing cannot find 'Last' outgoing message when split messages are all created with the same time.
				if (isSubsequentSplitMessage)
				{
					message.EM_MessageSubType = CMRMessage.MessageSubTypes.Amendment;
				}
			}

			if (shouldDelaySending)
			{
				message.EM_HeldUntilDate = ZDateTime.UtcNow;
			}
		}

		protected void PopulateDTM()
		{
			if (!reportHeader.DateTimeOfOutturn.IsEmpty)
			{
				MessageUtilities.PopulateDTM(CUSCAR.DTM.InstantiateAChildAndAddItToChildrenCollection(), DateTimePeriodFunctionCodeQualifierList.ReportedDate, reportHeader.DateTimeOfOutturn.ToString("yyyyMMdd"), DateTimePeriodFormatCodeList.Ccyymmdd);
				MessageUtilities.PopulateDTM(CUSCAR.DTM.InstantiateAChildAndAddItToChildrenCollection(), DateTimePeriodFunctionCodeQualifierList.ReportedDate, reportHeader.DateTimeOfOutturn.ToString("HHmm"), DateTimePeriodFormatCodeList.Hhmm);
			}
		}

		protected void PopulateNAD()
		{
			if (!reportHeader.ResponsiblePartyID.IsEmpty)
			{
				MessageUtilities.PopulateNAD(CUSCAR.Group2[0].NAD[0], PartyFunctionCodeQualifierList.ResponsibleParty, reportHeader.ResponsiblePartyID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
			}
		}

		protected void PopulateTDT()
		{
			ZString flightNumber = reportHeader.FlightNumber.KeepChars("ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890");
			if (!flightNumber.IsEmpty)
			{
				MessageUtilities.PopulateTDT(CUSCAR.Group4[0].TDT[0], TransportStageCodeQualifierList.MainCarriageTransport, flightNumber.SubstringSafe(2), TransportMeansDescriptionCodeList.Aircraft, flightNumber.Left(2), CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation, null);
			}
		}

		protected void PopulateLOC()
		{
			if (!reportHeader.EstablishmentID.IsEmpty)
			{
				MessageUtilities.PopulateLOC(CUSCAR.Group4[0].LOC[0], LocationFunctionCodeQualifierList.PlaceOfLoss, reportHeader.EstablishmentID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
		}

		protected void PopulateEstimatedDateOfArrivalDTM()
		{
			if (!reportHeader.EstimatedDateOfArrival.IsEmpty)
			{
				MessageUtilities.PopulateDTM(CUSCAR.Group4[0].DTM.InstantiateAChildAndAddItToChildrenCollection(), DateTimePeriodFunctionCodeQualifierList.ArrivalDateTimeEstimated, reportHeader.EstimatedDateOfArrival.ToString("yyyyMMdd"), DateTimePeriodFormatCodeList.Ccyymmdd);
			}
		}

		void PopulateGroup7(IAirOutturnReportLineInformation[] lines)
		{
			if (MessageSubType == Common.MessageBuilders.MessageSubTypes.Change)
			{
				if (isSplitMessage)
				{
					new UniqueIdentifierOutturnMessageLinePopulator().PopulateSplitMsgLines(CUSCAR, GetLineBuilders(lines), GetLineBuilders(reportHeader.Lines), GetLineBuilders(reportHeader.DatabaseLines));
				}
				else
				{
					new UniqueIdentifierOutturnMessageLinePopulator().Populate(CUSCAR, GetLineBuilders(reportHeader.Lines), GetLineBuilders(reportHeader.DatabaseLines));
				}
			}
			else if (MessageSubType == Common.MessageBuilders.MessageSubTypes.Create || MessageSubType == Common.MessageBuilders.MessageSubTypes.Replace)
			{
				new UniqueIdentifierOutturnMessageLinePopulator().Populate(CUSCAR, GetLineBuilders(lines));
			}
		}

		AIROUTMessageLine[] GetLineBuilders(IAirOutturnReportLineInformation[] reportLines)
		{
			var result = new ArrayList();

			foreach (IAirOutturnReportLineInformation reportLine in reportLines)
			{
				result.Add(new AIROUTMessageLine(reportLine));
			}
			return (AIROUTMessageLine[])result.ToArray(typeof(AIROUTMessageLine));
		}
	}
}
