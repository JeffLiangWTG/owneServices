using System;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Summary description for SEAIARMessageBuilder.
	/// </summary>
	public class SEAIARMessageBuilder : ImpendingArrivalReportBuilder
	{
		public SEAIARMessageBuilder(CusSeaManTranHead transportHeader)
			: this(new CusSeaManTranHeadSeaImpendingArrivalReportInformation(transportHeader))
		{
		}

		public SEAIARMessageBuilder(CusSCAOceanBill oceanBill)
			: this(new CusSCAOceanBillImpendingArrivalReportInformation(oceanBill))
		{
		}

		SEAIARMessageBuilder(ISeaImpendingArrivalReportInformation reportInfo)
			: base(reportInfo)
		{
			this.reportInfo = reportInfo;
		}

		readonly ISeaImpendingArrivalReportInformation reportInfo;

		protected internal override ZString DocumentName => "SEAIAR";

		protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.SEAIAR;

		protected internal override Type TypeOfMessage => typeof(CMRSEAIARMessage);

		protected override DateTimePeriodFunctionCodeQualifierList DepartureDateTimeCode => DateTimePeriodFunctionCodeQualifierList.DepartureDateTime;

		protected override void PopulateGroup2LOCs()
		{
			base.PopulateGroup2LOCs();
			if (MessageSubType != Enterprise.Customs.Common.MessageBuilders.MessageSubTypes.Withdraw)
			{
				if (!reportInfo.PortOfFirstArrival.IsEmpty)
				{
					MessageUtilities.PopulateLOC(cUSREP.Group2.InstantiateAChildAndAddItToChildrenCollection().LOC[0], LocationFunctionCodeQualifierList.PlacePortOfFirstEntry, reportInfo.PortOfFirstArrival, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
				}
			}
		}

		protected override void PopulateTDTSegment()
		{
			if (!reportInfo.Voyage.IsEmpty && !reportInfo.LloydsNumber.IsEmpty)
			{
				MessageUtilities.PopulateTDT(cUSREP.Group8[0].TDT[0], TransportStageCodeQualifierList.MainCarriageTransport, reportInfo.Voyage, TransportMeansDescriptionCodeList.Ship, null, null, reportInfo.LloydsNumber);
			}
		}

		protected override bool ShouldPopulatePortOfDeparture
		{
			get
			{
				return MessageSubType != Enterprise.Customs.Common.MessageBuilders.MessageSubTypes.Withdraw;
			}
		}

		protected override bool ShouldPopulateDepartureDate
		{
			get
			{
				return MessageSubType != Enterprise.Customs.Common.MessageBuilders.MessageSubTypes.Withdraw;
			}
		}

		protected override bool ShouldPopulatePortOfArrival
		{
			get
			{
				return MessageSubType != Enterprise.Customs.Common.MessageBuilders.MessageSubTypes.Withdraw;
			}
		}
	}
}
