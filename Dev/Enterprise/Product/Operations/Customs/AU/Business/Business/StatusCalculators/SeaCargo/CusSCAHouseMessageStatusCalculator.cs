
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAHouseMessageStatusCalculator : CMRStatusCalculator<CusSCAHouse>, ICMRCargoReportEventsLogger
	{
		public CusSCAHouseMessageStatusCalculator(CusSCAHouse house) : base(house)
		{
			this.house = house;
		}
		readonly CusSCAHouse house;

		protected internal override ZPropertyInfo StatusInfo => Parent.CA_MessageStatusInfo;

		protected internal override ZString[] InterestedMessageTypes => new ZString[] { CMRMessage.CMRMessageTypes.SEACR };

		IParentForCargoReporter ICMRCargoReportEventsLogger.ParentForCargoReportingEvents => house.Shipment;
	}
}
