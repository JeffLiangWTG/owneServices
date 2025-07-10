using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing
{
	[TestedType(typeof(RefContainerMapProvider))]
	sealed class RefContainerMapProviderTest : TestCaseWithFactory
	{
		public void TestIsUsageNeeded()
		{
			AssertEquals(UsageRequirement.NotRequire, refContainerCodeMap.ContainerMapProvider?.IsUsageNeeded);
		}

		public void TestUsageList()
		{
			AssertEquals(0, refContainerCodeMap.Lookups.UsageList.Count);
		}

		public void TestCodeList()
		{
			AssertSame(Factory.GetCachedValue<ISOContainerCodeList>(), refContainerCodeMap.Lookups.CodeList);
		}

		public void TestGetDefaultCustomsCode()
		{
			AssertEquals(ZString.Empty, refContainerCodeMap.ContainerMapProvider?.GetDefaultCustomsCode(Factory, "2000"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			refContainerCodeMap = Factory.NewWithValidTestData<RefContainerCodeMap>();
			refContainerCodeMap.RCM_RN_NKCountry = Core.Constants.CountryCodes.India;
		}

		RefContainerCodeMap refContainerCodeMap;
	}
}
