using Enterprise.MasterFiles.Business;
using UNDGDataItem = Enterprise.MasterFiles.Business.UNDGDataItem;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class DangerousGoodsProviderTest : Customs.Business.Testing.DataProviderTestCase<DangerousGoodsProvider>
{
	public void TestSequenceNumber()
	{
		AssertEquals(999, provider.SequenceNumber);
	}

	public void TestUNNumber()
	{
		var undgSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
		undgDataItem.DI_DG = undgSubstance.PK;

		undgDataItem.UNDGSubstance.DG_Code = "UNNR";
		AssertEquals("UNNR", provider.UNNumber);
	}

	protected override DangerousGoodsProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		undgDataItem = Factory.NewWithValidTestData<UNDGDataItem>();
		provider = new DangerousGoodsProvider(undgDataItem, 999);
	}
	UNDGDataItem undgDataItem;
	DangerousGoodsProvider provider;
}
