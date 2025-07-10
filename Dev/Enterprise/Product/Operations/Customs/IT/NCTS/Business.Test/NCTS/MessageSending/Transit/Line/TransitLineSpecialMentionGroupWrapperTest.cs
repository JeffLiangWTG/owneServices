using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class TransitLineSpecialMentionGroupWrapperTest : NctsSADLineSpecialMentionGroupWrapperTest
{
	public void TestUnloadingDataForSpecificClass()
	{
		CombineAssertions("When there are no previous documents", () => AssertEmptyUnloadingData(wrapper.UnloadingData));

		var previousDocument1 = goodsItem.PreviousDocuments.AddNew();
		previousDocument1.CSI_Tariff = "123456";
		previousDocument1.CSI_Quantity = 1m;
		previousDocument1.CSI_Quantity2 = 2m;
		CombineAssertions("When there is only one previous document", () =>
		{
			var unloadingData = wrapper.UnloadingData;
			AssertEquals(nameof(unloadingData.CommodityCode), "123456", unloadingData.CommodityCode);
			AssertEquals(nameof(unloadingData.Quantity), 1m, unloadingData.Quantity);
			AssertEquals(nameof(unloadingData.SupplementaryUnit), 2m, unloadingData.SupplementaryUnit);
		});

		goodsItem.PreviousDocuments.AddNew();
		CombineAssertions("When there are more than one previous document", () => AssertEmptyUnloadingData(wrapper.UnloadingData));
	}

	#region Implementation

	protected override IETLineSpecialMentionGroup GetNewLineSpecialMentionGroup(NctsDepartureCargoDesc nctsDepartureCargoDesc) => new TransitLineSpecialMentionGroupWrapper(nctsDepartureCargoDesc);

	void AssertEmptyUnloadingData(ISpecialMentionUnloadingDataInfo unloadingData)
	{
		AssertEquals(nameof(unloadingData.CommodityCode), ZString.Empty, unloadingData.CommodityCode);
		AssertNull(nameof(unloadingData.Quantity), unloadingData.Quantity);
		AssertNull(nameof(unloadingData.SupplementaryUnit), unloadingData.SupplementaryUnit);
	}

	#endregion Implementation
}
