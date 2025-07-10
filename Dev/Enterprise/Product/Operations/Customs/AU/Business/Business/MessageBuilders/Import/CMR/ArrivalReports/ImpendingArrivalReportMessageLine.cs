using System;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSREP;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ImpendingArrivalReportMessageLine : UniqueIdentifierMessageLine
	{
		public ImpendingArrivalReportMessageLine(IImpendingArrivalReportLineInformation line)
		{
			this.line = line;
		}

		public override string UniqueIdentifier
		{
			get
			{
				return "ARRIVALPORT=" + line.PortOfArrival + "ETA=" + line.EstimatedDateTimeOfArrivalUTC.ToString("yyyyMMdd") + "DISCHARGE=" + line.DischargeIndicator + "DISCHARGECTO=" + line.DischargeCTOEstablishmentID;
			}
		}

		public override SegmentGroup GetNewSegmentGroup(SegmentGroup message)
		{
			return ((CUSREPMessage)message).Group8[0].Group9.InstantiateAChildAndAddItToChildrenCollection();
		}

		public override void Populate(SegmentGroup segmentGroup, string lineActionCode)
		{
			SegmentGroup9 group9 = segmentGroup as SegmentGroup9;

			if (!line.PortOfArrival.IsEmpty)
			{
				MessageUtilities.PopulateLOC(group9.LOC[0], LocationFunctionCodeQualifierList.PlaceOfArrival, line.PortOfArrival, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);

				if (!line.EstimatedDateTimeOfArrivalUTC.IsEmpty)
				{
					MessageUtilities.PopulateDTM(group9.DTM[0], DateTimePeriodFunctionCodeQualifierList.ArrivalDateTimeEstimated, line.EstimatedDateTimeOfArrivalUTC.ToString("yyyyMMdd"), DateTimePeriodFormatCodeList.Ccyymmdd);
					MessageUtilities.PopulateDTM(group9.DTM[1], DateTimePeriodFunctionCodeQualifierList.ArrivalDateTimeEstimated, line.EstimatedDateTimeOfArrivalUTC.ToString("HHmm"), DateTimePeriodFormatCodeList.Hhmm);
				}

				if (!line.DischargeCTOEstablishmentID.IsEmpty)
				{
					MessageUtilities.PopulateNAD(group9.NAD.InstantiateAChildAndAddItToChildrenCollection(), PartyFunctionCodeQualifierList.TerminalOperator, line.DischargeCTOEstablishmentID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
				}

				if (!line.StevedoreID.IsEmpty)
				{
					MessageUtilities.PopulateNAD(group9.NAD.InstantiateAChildAndAddItToChildrenCollection(), PartyFunctionCodeQualifierList.UnloadingParty, line.StevedoreID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
				}

				MessageUtilities.PopulateSTS(group9.STS[0], line.DischargeIndicator ? StatusDescriptionCodeList.GetFromString("Y") : StatusDescriptionCodeList.GetFromString("N"));

				MessageUtilities.PopulateFTX(group9.FTX[0], TextSubjectCodeQualifierList.LineItem, TextFunctionCodedList.GetFromString(lineActionCode));
			}
		}

		readonly IImpendingArrivalReportLineInformation line;

		protected internal override Type SegmentGroupType => typeof(SegmentGroup9);
	}
}
