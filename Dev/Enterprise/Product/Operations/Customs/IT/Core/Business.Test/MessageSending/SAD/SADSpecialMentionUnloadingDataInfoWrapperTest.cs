using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SADSpecialMentionUnloadingDataInfoWrapperTest : TestCaseWithFactory
{
	public void TestSADSpecialMentionUnloadingDataInfo()
	{
		var wrapper = new SADSpecialMentionUnloadingDataInfoWrapper("1234657890", 100m, 1m);
		CombineAssertions(() =>
		{
			AssertEquals(nameof(wrapper.CommodityCode), "1234657890", wrapper.CommodityCode);
			AssertEquals(nameof(wrapper.Quantity), 100m, wrapper.Quantity);
			AssertEquals(nameof(wrapper.SupplementaryUnit), 1m, wrapper.SupplementaryUnit);
		});
	}

	public void TestNullQuantitiesIfZero()
	{
		var wrapper = new SADSpecialMentionUnloadingDataInfoWrapper(ZString.Empty, 0m, 0m);
		CombineAssertions(() =>
		{
			AssertNull(nameof(wrapper.Quantity), wrapper.Quantity);
			AssertNull(nameof(wrapper.SupplementaryUnit), wrapper.SupplementaryUnit);
		});
	}

	public void TestEmptyWrapper()
	{
		var wrapper = SADSpecialMentionUnloadingDataInfoWrapper.Empty();
		CombineAssertions(() =>
		{
			AssertEquals(nameof(wrapper.CommodityCode), "", wrapper.CommodityCode);
			AssertNull(nameof(wrapper.Quantity), wrapper.Quantity);
			AssertNull(nameof(wrapper.SupplementaryUnit), wrapper.SupplementaryUnit);
		});
	}
}
