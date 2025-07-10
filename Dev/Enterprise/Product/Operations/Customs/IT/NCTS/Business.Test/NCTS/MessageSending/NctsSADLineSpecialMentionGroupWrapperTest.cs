using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

abstract class NctsSADLineSpecialMentionGroupWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When NctsDepartureCargoDesc parameter is null", () => GetNewLineSpecialMentionGroup(null));
		AssertNoExceptionThrown(() => GetNewLineSpecialMentionGroup(goodsItem));
	}

	public void TestAdditionalInformation()
	{
		AssertType<NctsSADLineSpecialMentionInfoAdditionalInformationWrapper>($"{nameof(wrapper.AdditionalInformation)} type", wrapper.AdditionalInformation);
	}

	public void TestEori()
	{
		AssertType<NctsSADLineSpecialMentionEoriInfoWrapper>($"{nameof(wrapper.Eori)} type", wrapper.Eori);
	}

	public void TestUnloadingData()
	{
		CombineAssertions("When there are no previous documents", () =>
		{
			var unloadingData = wrapper.UnloadingData;
			AssertEquals(nameof(unloadingData.CommodityCode), ZString.Empty, unloadingData.CommodityCode);
			AssertNull(nameof(unloadingData.Quantity), unloadingData.Quantity);
			AssertNull(nameof(unloadingData.SupplementaryUnit), unloadingData.SupplementaryUnit);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var nctsHeader = Factory.NewDepartureNctsHeader();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		wrapper = GetNewLineSpecialMentionGroup(goodsItem);
	}

	protected NctsDepartureCargoDesc goodsItem;
	protected IETLineSpecialMentionGroup wrapper;

	protected virtual IETLineSpecialMentionGroup GetNewLineSpecialMentionGroup(NctsDepartureCargoDesc nctsDepartureCargoDesc) => new NctsSADLineSpecialMentionGroupWrapper(nctsDepartureCargoDesc);
}
