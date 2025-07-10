namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	class ReportColumnSettingRegistryItemPrefixManagerTest : NUnit.Framework.TestCase
	{
		public void TestPrefixHelper()
		{
			ReportColumnSettingRegistryPrefixHelper test = new ReportColumnSettingRegistryPrefixHelper();
			AssertEquals("Get Description from Get Key should return description", "Test Description", test.GetDescriptionFromKey(test.GetKey("Test Description")));
			AssertEquals("Get Descriptiohn", "Test Description", test.GetDescriptionFromKey("RPT_CFG$COMPCODE$Test Description"));
			AssertEquals("Get Company Code", "COMPCODE", test.GetCompanyCodeFromKey("RPT_CFG$COMPCODE$Test Description"));
		}
	}
}
