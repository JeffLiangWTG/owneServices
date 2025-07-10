using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.MX.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	class RefContainerMapProviderTest : TestCaseWithFactory
	{
		public void TestIsUsageNeeded()
		{
			AssertEquals(UsageRequirement.MayRequire, containerCodeMap.ContainerMapProvider.IsUsageNeeded);
		}

		public void TestUsageList()
		{
			var usageList = containerCodeMap.Lookups.UsageList;

			Assert(usageList.Count == 1);
			AssertEquals(true, usageList.ContainsCode("CUS"));
			AssertEquals("Customs", usageList.GetDescriptionFromCode("CUS"));
			AssertSame(usageList, containerCodeMap.Lookups.UsageList);
		}

		public void TestCodeList()
		{
			var codeList = containerCodeMap.Lookups.CodeList;
			AssertType<MXContainerCodeList>(codeList);
			AssertSame(codeList, containerCodeMap.Lookups.CodeList);

			containerCodeMap.RCM_Usage = "CUS";
			codeList = containerCodeMap.Lookups.CodeList;
			AssertType<MXCustomsContainerCodeList>(codeList);
			AssertSame(codeList, containerCodeMap.Lookups.CodeList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			containerCodeMap = Factory.NewWithValidTestData<RefContainerCodeMap>();
			containerCodeMap.RCM_RN_NKCountry = Core.Constants.CountryCodes.Mexico;
		}

		RefContainerCodeMap containerCodeMap;
	}
}
