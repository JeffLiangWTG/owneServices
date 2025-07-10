using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing
{
	[TestedType(typeof(RefContainerMapProvider))]
	sealed class RefContainerMapProviderTest : TestCaseWithFactory
	{
		public void TestIsUsageNeeded()
		{
			AssertEquals(UsageRequirement.NotRequire, aeRefContainerCodeMap.ContainerMapProvider?.IsUsageNeeded);
		}

		public  void TestUsageList()
		{
			AssertEquals(0, aeRefContainerCodeMap.Lookups.UsageList.Count);
		}

		public void TestCodeList()
		{
			AssertSame(Factory.GetCachedValue<AEContainerTypeCodeList>(), aeRefContainerCodeMap.Lookups.CodeList);
		}

		public void TestGetDefaultCustomsCode()
		{
			AssertEquals(ZString.Empty, aeRefContainerCodeMap.ContainerMapProvider?.GetDefaultCustomsCode(Factory, "DRY"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			aeRefContainerCodeMap = Factory.NewWithValidTestData<RefContainerCodeMap>();
			aeRefContainerCodeMap.RCM_RN_NKCountry = Core.Constants.CountryCodes.UnitedArabEmirates;
		}
		RefContainerCodeMap aeRefContainerCodeMap;
	}
}
