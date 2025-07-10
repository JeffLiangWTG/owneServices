using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.UPE.Business.Testing
{
	public class RTSTranshipmentDetailsLookupsTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		public void TestLists()
		{
			UPECusHAWB houseBill = Factory.New<UPECusHAWB>();
			RTSTranshipmentDetailsLookups lookups = new RTSTranshipmentDetailsLookups(new RTSDetails(houseBill));
			AssertNotNull(lookups.AuthReceivedByList);
			AssertNotNull(lookups.CountryList);
			AssertNotNull(lookups.DestinationPortList);
			AssertNotNull(lookups.OriginPortList);
		}
	}
}
