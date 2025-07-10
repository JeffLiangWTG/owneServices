using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business
{
	[TestedType(typeof(ClientPWSChargeCollection))]
	public class ClientPWSChargeCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		public void TestFindByChargeType()
		{
			ClientPWSChargeCollection collection = (ClientPWSChargeCollection)GetCollectionToTest();
			ClientPWSCharge charge1 = collection.AddNew();
			ClientPWSCharge charge2 = collection.AddNew();
			charge1.U2_ChargeDescription = "CG1";
			charge2.U2_ChargeDescription = "CG2";
			ClientPWSCharge foundCharge = collection.FindByChargeDescription("CG2");
			AssertEquals("Should find the correct charge", "CG2", foundCharge.U2_ChargeDescription);
			ClientPWSCharge noCharge = collection.FindByChargeDescription("XXX");
			AssertNull("Should return null if there is no charge", noCharge);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			ClientPWSHeader shipment = Factory.New<ClientPWSHeader>();
			return shipment.Charges;
		}
	}
}
