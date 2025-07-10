using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusPartShipAIRCRManager : CMRMessageManager
	{
		public CusPartShipAIRCRManager(CusPartShip partShip)
		{
			this.partShip = partShip;
		}

		internal override CMRAmendmentGenerator GetAmendmentManager(BusinessObject bizo) => new CusPartShipAIRCRAmendmentGenerator(bizo as CusPartShip);

		internal override CMRMessageBuilder[] GetBuilder(BusinessObject bizo)
		{
			var partShip = bizo as CusPartShip;
			return [new AIRCRMessageBuilder(partShip, partShip.HouseBill as CTOCusHAWB)];
		}

		public override string MessageFriendlyName => "Air Cargo Report for MAWB: " + ((CusPartShip)BusinessObject).HouseBill.CS_HAWB + " (part shipment for flight: " + partShip.CG_FlightNo + ", " + partShip.CG_ArrivalDate.ToString("d") + ")";

		internal override ICalculatedCusStatusCalculator[] StatusCalculators => [partShip.Calculator];

		internal override string GetStatus() => partShip.CG_CustomsStatus;

		public override BusinessObject BusinessObject => partShip;

		internal override EDIMessageCollection GetMessages(BusinessObject bizo) => (bizo as CusPartShip).Messages;

		readonly CusPartShip partShip;
	}
}
