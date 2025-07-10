using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(ClientBISIShipmentChargeCollection))]
	public class ClientBISIShipmentChargeCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		public void TestFindByChargeType()
		{
			ClientBISIShipmentChargeCollection collection = (ClientBISIShipmentChargeCollection)GetCollectionToTest();
			ClientBISIShipmentCharge charge1 = collection.AddNew();
			ClientBISIShipmentCharge charge2 = collection.AddNew();
			charge1.T9_ChargeType = "CG1";
			charge2.T9_ChargeType = "CG2";
			ClientBISIShipmentCharge foundCharge = collection.FindByChargeType("CG2");
			AssertEquals("Should find the correct charge", "CG2", foundCharge.T9_ChargeType);
			ClientBISIShipmentCharge noCharge = collection.FindByChargeType("XXX");
			AssertNull("Should return null if there is no charge", noCharge);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			ClientBISIShipmentHeader shipment = Factory.New<ClientBISIShipmentHeader>();
			return shipment.Charges;
		}
	}
}
