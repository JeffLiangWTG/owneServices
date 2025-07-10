using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusPartShipAIRCRAmendmentGenerator : CMRAmendmentGenerator
	{
		public CusPartShipAIRCRAmendmentGenerator(CusPartShip partShip)
			: base(partShip)
		{
			this.partShip = partShip;
		}

		protected internal override CMRMessageBuilder GetBuilder(BusinessObject bizo)
		{
			var partShip = bizo as CusPartShip;
			return new AIRCRMessageBuilder(partShip, partShip.HouseBill as CTOCusHAWB);
		}

		protected internal override EDIMessageCollection GetMesssageCollection(BusinessObject bizo)
		{
			var partShip = bizo as CusPartShip;
			return partShip.Messages;
		}

		protected override ZPropertyInfo[] UniqueIdentifierInfos => new ZPropertyInfo[] { partShip.HouseBill.MAWB.CM_FlightNoInfo, partShip.CG_ArrivalDateInfo, partShip.HouseBill.CS_HAWBInfo };

		readonly CusPartShip partShip;
	}
}
