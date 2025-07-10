using System;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Summary description for AIRIARMessageBuilder.
	/// </summary>
	public class AIRIARMessageBuilder : ImpendingArrivalReportBuilder
	{
		public AIRIARMessageBuilder(CusMAWB mAWB)
			: this(new CusMAWBImpendingArrivalReportInformation(mAWB))
		{
		}

		public AIRIARMessageBuilder(IAirImpendingArrivalReportInformation reportInfo)
			: base(reportInfo)
		{
			this.reportInfo = reportInfo;
		}

		readonly IAirImpendingArrivalReportInformation reportInfo;

		protected internal override ZString DocumentName => "AIRIAR";

		protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.AIRIAR;

		protected internal override Type TypeOfMessage => typeof(CMRAIRIARMessage);

		protected override DateTimePeriodFunctionCodeQualifierList DepartureDateTimeCode => DateTimePeriodFunctionCodeQualifierList.DepartureDateTimeFromLastPortOfCall;

		protected override void PopulateTDTSegment()
		{
			ZString flightNumber = reportInfo.FlightNo.KeepChars("ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890");
			if (!flightNumber.IsEmpty)
			{
				MessageUtilities.PopulateTDT(cUSREP.Group8[0].TDT[0], TransportStageCodeQualifierList.MainCarriageTransport, flightNumber.SubstringSafe(2), TransportMeansDescriptionCodeList.Aircraft, flightNumber.Left(2), CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation, null);
			}
		}

		protected override bool ShouldPopulatePortOfDeparture
		{
			get { return true; }
		}

		protected override bool ShouldPopulateDepartureDate
		{
			get { return true; }
		}

		protected override bool ShouldPopulatePortOfArrival
		{
			get { return true; }
		}
	}
}
