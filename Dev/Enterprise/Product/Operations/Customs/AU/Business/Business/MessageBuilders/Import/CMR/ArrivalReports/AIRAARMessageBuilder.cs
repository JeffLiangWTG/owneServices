using System;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AIRAARMessageBuilder : ActualArrivalReportBuilder
	{
		public AIRAARMessageBuilder(CusMAWB mAWB)
			: this(new CusMAWBActualArrivalReportInformation(mAWB))
		{
		}

		public AIRAARMessageBuilder(IAirActualArrivalReportInformation reportInfo)
			: base(reportInfo)
		{
			this.reportInfo = reportInfo;
		}

		protected override void PopulateGroup2LOCs()
		{
			base.PopulateGroup2LOCs();
			if (!reportInfo.LastOverseasPortOfDeparture.IsEmpty)
			{
				MessageUtilities.PopulateLOC(cUSREP.Group2.InstantiateAChildAndAddItToChildrenCollection().LOC[0], LocationFunctionCodeQualifierList.LastPlacePortOfCallOfConveyance, reportInfo.LastOverseasPortOfDeparture, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
			}
		}

		protected override void PopulateGroup2DTMSegments()
		{
			base.PopulateGroup2DTMSegments();
			int group2Count = cUSREP.Group2.Count;
			if (group2Count > 0)
			{
				if (MessageSubType != Enterprise.Customs.Common.MessageBuilders.MessageSubTypes.Withdraw)
				{
					if (!reportInfo.EstimatedArrivalDate.IsEmpty)
					{
						MessageUtilities.PopulateDTM(cUSREP.Group2[group2Count - 1].DTM.InstantiateAChildAndAddItToChildrenCollection(), DateTimePeriodFunctionCodeQualifierList.ArrivalDateTimeEstimated, reportInfo.EstimatedArrivalDate.ToString("yyyyMMdd"), DateTimePeriodFormatCodeList.Ccyymmdd);
					}
				}
				ZDateTime dateTimeOfDeparture = reportInfo.DateTimeOfDepartureUTC;
				if (!dateTimeOfDeparture.IsEmpty)
				{
					MessageUtilities.PopulateDTM(cUSREP.Group2[group2Count - 1].DTM.InstantiateAChildAndAddItToChildrenCollection(), DateTimePeriodFunctionCodeQualifierList.DepartureDateTimeFromLastPortOfCall, dateTimeOfDeparture.ToString("yyyyMMdd"), DateTimePeriodFormatCodeList.Ccyymmdd);
				}
			}
		}

		protected override void PopulateTDTSegment()
		{
			ZString flightNumber = reportInfo.FlightNo.KeepChars("ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890");
			if (!flightNumber.IsEmpty)
			{
				MessageUtilities.PopulateTDT(cUSREP.Group8[0].TDT[0], TransportStageCodeQualifierList.MainCarriageTransport, flightNumber.SubstringSafe(2), TransportMeansDescriptionCodeList.Aircraft, flightNumber.Left(2), CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation, null);
			}
		}

		protected internal override Type TypeOfMessage => typeof(CMRAIRAARMessage);

		protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.AIRAAR;

		protected internal override ZString DocumentName => "AIRAAR";

		readonly IAirActualArrivalReportInformation reportInfo;
	}
}
