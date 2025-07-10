using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	class RefContainerMapProviderTest : TestCaseWithFactory
	{
		public void TestIsUsageNeeded()
		{
			var testParent = Factory.NewWithValidTestData<RefContainerCodeMap>();
			testParent.RCM_RN_NKCountry = Core.Constants.CountryCodes.Colombia;
			AssertEquals(UsageRequirement.NotRequire, testParent.ContainerMapProvider?.IsUsageNeeded);
		}

		public void TestUsageList()
		{
			var testParent = Factory.NewWithValidTestData<RefContainerCodeMap>();
			Assert(testParent.Lookups.UsageList.Count == 0);
		}

		public void TestCodeList()
		{
			var testParent = Factory.NewWithValidTestData<RefContainerCodeMap>();
			testParent.RCM_RN_NKCountry = Core.Constants.CountryCodes.Colombia;
			Assert(testParent.Lookups.CodeList.Count == 6);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Colombia))
			{
				var testList = testParent.Lookups.CodeList;

				AssertEquals(true, testList.ContainsCode("1"));
				AssertEquals(true, testList.ContainsCode("2"));
				AssertEquals(true, testList.ContainsCode("3"));
				AssertEquals(true, testList.ContainsCode("4"));
				AssertEquals(true, testList.ContainsCode("5"));
				AssertEquals(true, testList.ContainsCode("6"));
			}
		}

		public void TestDefaultCustomsCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Colombia);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.CTYPE, "OUT", "Container Type Mapping", true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.CTYPE, "20G0", "1", ZDateTime.Today, ZDateTime.Today, Core.Constants.CountryCodes.Colombia);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Colombia))
			{
				var testParent = Factory.NewWithValidTestData<RefContainerCodeMap>();
				testParent.RCM_RN_NKCountry = Core.Constants.CountryCodes.Colombia;
				var customsCode = testParent.ContainerMapProvider?.GetDefaultCustomsCode(Factory, "20G0");
				AssertEquals("1", customsCode);
			}
		}
	}
}
