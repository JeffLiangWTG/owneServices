using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class PackLineStatusTest : TestCaseWithFactory
	{
		public void TestGetCustomsStatusDescription()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "OBL";
			CFSContainer testContainer = consol.Containers.AddNew();
			testContainer.JC_ContainerNum = "C1";
			CFSShipment shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HBL";
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			CFSPackLine packLine = shipment.OuterPackLines.AddNew();
			testContainer.AddPackLine(shipment.OuterPackLines[0]);

			AssertEquals("", packLine.CustomsStatusDescription);
		}
	}
}
