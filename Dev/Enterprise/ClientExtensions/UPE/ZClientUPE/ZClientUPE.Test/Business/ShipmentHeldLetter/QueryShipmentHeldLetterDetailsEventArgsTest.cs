using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.UPE.Business.Testing
{
	class QueryShipmentHeldLetterDetailsEventArgsTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		public void TestConstructor()
		{
			UPECusHAWB cusHAWB = Factory.NewWithValidTestData<UPECusHAWB>();
			ShipmentHeldLetterBusinessObject bizObj = cusHAWB.ShipmentHeldLetterDetails;
			QueryShipmentHeldLetterDetailsEventArgs e = new QueryShipmentHeldLetterDetailsEventArgs(bizObj, ShipmentHeldLetterRecipient.Consignee);
			AssertEquals("BizObj", bizObj, e.BizObj);
			AssertEquals("Recipient", ShipmentHeldLetterRecipient.Consignee, e.Recipient);
		}
	}
}
