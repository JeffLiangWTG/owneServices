using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsUnloadedCargoDesc))]
sealed class NctsUnloadedCargoDescTest : NctsCommonCargoDescAbstractTest<NctsHeader>
{
	public new void TestCorrectTypeDecideForLoad()
	{
		var goodsItem = GetNewBusinessObject();
		Factory.Save();

		AssertType("GoodsItem", goodsItem.GetType(), new BusinessObjectFactory().Load<NctsUnloadedCargoDesc>(goodsItem.PK));
	}

	public void TestValidation()
	{
		AssertType<NctsUnloadedCargoDescValidation>(UnloadedCargoDesc.Validation);
	}

	public void TestLookups()
	{
		AssertType<NctsUnloadedCargoDescLookups>(UnloadedCargoDesc.Lookups);
	}

	public void TestGetNewTariffFormatter()
	{
		AssertType<TariffFormatterCH>(((ITariffFormatProvider)UnloadedCargoDesc).TariffFormatter);
	}

	public void TestNctsAdditionalInfoCollection()
	{
		AssertType<EU.NCTS.Business.NctsAdditionalInfoCollection<NctsAdditionalInfo>>(UnloadedCargoDesc.AdditionalInfos);
	}

	protected override BusinessObject GetNewBusinessObject() => UnloadedCargoDesc;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => UnloadedCargoDesc;

	protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => UnloadedCargoDesc;

	BusinessObject GetNewNctsUnloadedCargoDesc(BusinessObjectFactory factory)
	{
		var header = factory.New<NctsHeader>();
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		var bill = header.Bills.AddNew();
		var arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
		arrivalCargoDesc.BY_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF;
		return arrivalCargoDesc.UnloadedGoodsItem;
	}

	NctsUnloadedCargoDesc UnloadedCargoDesc => unloadedCargoDesc ?? (unloadedCargoDesc = (NctsUnloadedCargoDesc)GetNewNctsUnloadedCargoDesc(Factory));
	NctsUnloadedCargoDesc unloadedCargoDesc;

	protected override ZString CountryCode => Core.Constants.CountryCodes.Switzerland;
}
