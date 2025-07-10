using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing;

[TestedType(typeof(CC044CGoodsMeasureWrapper))]
sealed class CC044CGoodsMeasureWrapperTest : Customs.Business.Testing.DataProviderTestCase<CC044CGoodsMeasureWrapper>
{
	public void TestGrossMass() => CombineAssertions(() =>
	{
		item.BY_GrossWeight = 5m;
		item.BY_GrossWeightUnit = "KG";
		AssertEquals("GrossMass should be item gross weight because unloaded state is 'New'", 5m, Provider.GrossMass);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		var unloadedItem = item.UnloadedGoodsItem;
		unloadedItem.BY_GrossWeight = 10m;
		provider = CC044CGoodsMeasureWrapper.New(unloadedItem);
		AssertEquals("GrossMass should be item.UnloadedGoodsItem's gross weight because unloaded state is 'DIF'", 10m, provider.GrossMass);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		provider = CC044CGoodsMeasureWrapper.New(item);
		AssertEquals("GrossMass should be 0 because unloaded state is 'MIS'", 0m, provider.GrossMass);
	});

	public void TestNetMass() => CombineAssertions(() =>
	{
		item.BY_NetWeight = 5m;
		item.BY_NetWeightUnit = "KG";
		AssertEquals("NetMass should be item net weight because unloaded state is 'New'", 5m, Provider.NetMass);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		var unloadedItem = item.UnloadedGoodsItem;
		unloadedItem.BY_NetWeight = 10m;
		provider = CC044CGoodsMeasureWrapper.New(unloadedItem);
		AssertEquals("NetMass should be item.UnloadedGoodsItem's net weight because unloaded state is 'DIF'", 10m, provider.NetMass);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		provider = CC044CGoodsMeasureWrapper.New(item);
		AssertEquals("NetMass should be 0 because unloaded state is 'MIS'", 0m, provider.NetMass);
	});

	protected override CC044CGoodsMeasureWrapper GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		item = header.Bills.AddNew().ArrivalGoodsItems.AddNew();

		provider = CC044CGoodsMeasureWrapper.New(item);
	}

	NctsArrivalCargoDesc item;
	CC044CGoodsMeasureWrapper provider;
}
