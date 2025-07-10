namespace Enterprise.Client.FSH.TsManifest.Lines.Testing
{
	public class HouseBillLineTest : BaseLineTest
	{
		public void TestProperties()
		{
			AssertEquals("HouseBillNumber", "HOUSE110", HouseBill.HouseBillNumber);
			AssertEquals("ColoadMasterNumber", "BL2994", HouseBill.ColoadMasterNumber);
			AssertEquals("NumberOfPackages", 5, HouseBill.NumberOfPackages);
			AssertNotNull(HouseBill.OceanBill);
		}
	}
}
