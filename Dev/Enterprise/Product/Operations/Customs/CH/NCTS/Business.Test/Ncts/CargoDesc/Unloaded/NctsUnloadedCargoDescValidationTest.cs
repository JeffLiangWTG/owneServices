using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsUnloadedCargoDescValidation))]
sealed class NctsUnloadedCargoDescValidationTest : BusinessObjectValidationTestCase
{
	public void TestBY_HarmonisedTariffCharacterCheck() => CargoDescTestHelper.AssertHarmonisedTariffCharacterCheck(Factory, UnloadedCargoDesc);

	public void TestCheckBY_Cus4Number()
	{
		CargoDescTestHelper.AssertCus4NumberListValidation(UnloadedCargoDesc);
	}

	NctsUnloadedCargoDesc CreateNctsUnloadedCargoDesc()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var bill = nctsHeader.Bills.AddNew();
		var arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
		arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		return arrivalCargoDesc.UnloadedGoodsItem;
	}

	NctsUnloadedCargoDesc UnloadedCargoDesc => unloadedCargoDesc ?? (unloadedCargoDesc = CreateNctsUnloadedCargoDesc());
	NctsUnloadedCargoDesc unloadedCargoDesc;
}
