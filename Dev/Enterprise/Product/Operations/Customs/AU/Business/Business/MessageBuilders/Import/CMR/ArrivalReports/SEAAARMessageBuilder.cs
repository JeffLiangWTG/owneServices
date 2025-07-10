using System;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SEAAARMessageBuilder : ActualArrivalReportBuilder
	{
		public SEAAARMessageBuilder(CusSeaManArrivalPort arrival)
			: this(new CusSeaManArrivalPortSeaActualArrivalReportInformation(arrival))
		{
		}

		public SEAAARMessageBuilder(CusSCAOceanBill oceanBill)
			: this(new CusSCAOceanBillActualArrivalReportInformation(oceanBill))
		{
		}

		SEAAARMessageBuilder(ISeaActualArrivalReportInformation reportInfo)
			: base(reportInfo)
		{
			this.reportInfo = reportInfo;
		}

		protected override void PopulateGroup2LOCs()
		{
			base.PopulateGroup2LOCs();
			if (!IsWithdrawal && !reportInfo.BerthCode.IsEmpty)
			{
				MessageUtilities.PopulateLOC(cUSREP.Group2.InstantiateAChildAndAddItToChildrenCollection().LOC[0], LocationFunctionCodeQualifierList.Berth, reportInfo.BerthCode, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
		}

		protected override void PopulateGroup5()
		{
			base.PopulateGroup5();

			if (!reportInfo.DischargeCTOID.IsEmpty)
			{
				MessageUtilities.PopulateNAD(cUSREP.Group5.InstantiateAChildAndAddItToChildrenCollection().NAD[0], PartyFunctionCodeQualifierList.TerminalOperator, reportInfo.DischargeCTOID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
			}
			if (!IsWithdrawal && !reportInfo.StevedoreID.IsEmpty)
			{
				MessageUtilities.PopulateNAD(cUSREP.Group5.InstantiateAChildAndAddItToChildrenCollection().NAD[0], PartyFunctionCodeQualifierList.UnloadingParty, reportInfo.StevedoreID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
			}
		}

		protected override bool ShouldPopulatePortOfArrival
		{
			get { return !IsWithdrawal; }
		}

		//		protected override void PopulateGroup2LOCs()
		//		{
		//			base.PopulateGroup2LOCs();
		//			if (!MAWB.CM_RL_NKLoadPort.IsEmpty)
		//			{
		//				MessageUtilities.PopulateLOC(CUSREP.Group2.InstantiateAChildAndAddItToChildrenCollection().LOC[0], LocationFunctionCodeQualifierList.LastPlacePortOfCallOfConveyance, MAWB.CM_RL_NKLoadPort, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
		//			}
		//		}
		//
		//		protected override void PopulateGroup2DTMSegments()
		//		{
		//			base.PopulateGroup2DTMSegments();
		//			int Group2Count = CUSREP.Group2.Count;
		//			if (Group2Count > 0)
		//			{
		//				if (MessageSubType != Enterprise.Customs.Common.MessageBuilders.MessageSubTypes.Withdraw)
		//				{
		//					if (!MAWB.CM_ArrivalDate.IsEmpty)
		//					{
		//						MessageUtilities.PopulateDTM(CUSREP.Group2[Group2Count - 1].DTM.InstantiateAChildAndAddItToChildrenCollection(), DateTimePeriodFunctionCodeQualifierList.ArrivalDateTimeEstimated, MAWB.CM_ArrivalDate.ToString("yyyyMMdd"), DateTimePeriodFormatCodeList.Ccyymmdd);
		//					}
		//				}
		//				ZDateTime DateTimeOfDeparture = new CusMAWBImpendingArrivalReportInformation(MAWB).DateTimeOfDeparture;
		//				if (!DateTimeOfDeparture.IsEmpty)
		//				{
		//					MessageUtilities.PopulateDTM(CUSREP.Group2[Group2Count - 1].DTM.InstantiateAChildAndAddItToChildrenCollection(), DateTimePeriodFunctionCodeQualifierList.DepartureDateTimeFromLastPortOfCall, DateTimeOfDeparture.ToString("yyyyMMdd"), DateTimePeriodFormatCodeList.Ccyymmdd);
		//				}
		//			}
		//		}
		//
		protected override void PopulateTDTSegment()
		{
			if (!reportInfo.Voyage.IsEmpty && !reportInfo.LloydsNumber.IsEmpty)
			{
				MessageUtilities.PopulateTDT(cUSREP.Group8[0].TDT[0], TransportStageCodeQualifierList.MainCarriageTransport, reportInfo.Voyage, TransportMeansDescriptionCodeList.Ship, null, null, reportInfo.LloydsNumber);
			}
		}

		protected internal override Type TypeOfMessage => typeof(CMRSEAAARMessage);

		protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.SEAAAR;

		protected internal override ZString DocumentName => "SEAAAR";

		readonly ISeaActualArrivalReportInformation reportInfo;
	}
}
