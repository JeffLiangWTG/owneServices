using System.Runtime.CompilerServices;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using UnloadedState = Enterprise.Customs.EU.NCTS.Business.NctsUnloadedStateList.Codes;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class NctsArrivalCargoDescValidationTest : BusinessObjectValidationTestCase
{
	public void TestBY_HarmonisedTariffCharacterCheck() => CargoDescTestHelper.AssertHarmonisedTariffCharacterCheck(Factory, ArrivalCargoDesc);

	public void TestCheckBY_Cus4Number()
	{
		ArrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		CargoDescTestHelper.AssertCus4NumberListValidation(ArrivalCargoDesc);
	}

	public void TestBY_UnloadedState_NP70237() => CombineAssertions(() =>
	{
		const string message1 = "[NP70237] Please enter a difference in the unloaded value of the goods item.";
		const string message2 = "[NP70237] Please enter a difference in the unloaded value of the goods item or its packaging.";

		AssertMessage(null, UnloadedState.DIF, true, null, false);
		AssertMessage(message1, UnloadedState.DIF, false, null, false);
		AssertMessage(message1, UnloadedState.DIF, false, UnloadedState.DEC, false);
		AssertMessage(null, UnloadedState.DIF, false, UnloadedState.MIS, false);
		AssertMessage(null, UnloadedState.DIF, false, UnloadedState.NEW, false);
		AssertMessage(message2, UnloadedState.DIF, false, UnloadedState.DIF, false);
		AssertMessage(null, UnloadedState.DIF, true, UnloadedState.DIF, false);
		AssertMessage(null, UnloadedState.DIF, false, UnloadedState.DIF, true);

		AssertMessage(null, UnloadedState.DEC, false, UnloadedState.DIF, false);
		AssertMessage(null, UnloadedState.MIS, false, UnloadedState.DIF, false);
		AssertMessage(null, UnloadedState.NEW, false, UnloadedState.DIF, false);

		void AssertMessage(string expectedMessage, string goodsItemUnloadedState, bool goodsItemDifference, string packageUnloadedState, bool packageDifference, [CallerLineNumber] int line = 0)
		{
			var assertionMessage = $"[{line}] GoodsItem: {goodsItemUnloadedState} {(goodsItemDifference ? "with" : "without")} differences";

			ArrivalCargoDesc.BY_UnloadedState = goodsItemUnloadedState;
			if (ArrivalCargoDesc.UnloadedGoodsItem != null)
			{
				ArrivalCargoDesc.UnloadedGoodsItem.BY_Description = goodsItemDifference ? "xxx" : ArrivalCargoDesc.BY_Description;
			}
			AssertEquals($"[{line}] Pre-condition: ArrivalCargoDesc.IsDIFWithDifferences", goodsItemUnloadedState == UnloadedState.DIF && goodsItemDifference, ArrivalCargoDesc.IsDIFWithDifferencesIncludingPackages);

			ArrivalCargoDesc.Packages.RemoveAll();
			if (packageUnloadedState == null)
			{
				assertionMessage += ", no packages";
			}
			else
			{
				assertionMessage += $", packages DEC and {packageUnloadedState} {(packageDifference ? "with" : "without")} differences";

				var package1 = ArrivalCargoDesc.Packages.AddNew();
				package1.B5_TypeOfDifference = UnloadedState.DEC;

				var package2 = ArrivalCargoDesc.Packages.AddNew();
				package2.B5_TypeOfDifference = packageUnloadedState;
				package2.B5_UnitType = "CT";
				if (package2.PackDifference != null)
				{
					package2.PackDifference.B5_UnitType = packageDifference ? "XX" : package2.B5_UnitType;
				}
				AssertEquals($"[{line}] Pre-condition: NctsPackage.IsDIFWithDifferences", packageUnloadedState == UnloadedState.DIF && packageDifference, package2.IsDIFWithDifferences);
			}

			ArrivalCargoDesc.Validation.ValidateBY_UnloadedState();
			if (expectedMessage != null)
			{
				AssertHasMessageError(assertionMessage, ArrivalCargoDesc.BY_UnloadedStateInfo, expectedMessage);
			}
			if (expectedMessage != message1)
			{
				AssertNoMessageError(assertionMessage, ArrivalCargoDesc.BY_UnloadedStateInfo, message1);
			}
			if (expectedMessage != message2)
			{
				AssertNoMessageError(assertionMessage, ArrivalCargoDesc.BY_UnloadedStateInfo, message2);
			}
		}
	});

	NctsArrivalCargoDesc CreateNctsArrivalCargoDesc()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var bill = nctsHeader.Bills.AddNew();
		arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
		arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		return arrivalCargoDesc;
	}

	NctsArrivalCargoDesc ArrivalCargoDesc => arrivalCargoDesc ?? (arrivalCargoDesc = CreateNctsArrivalCargoDesc());
	NctsArrivalCargoDesc arrivalCargoDesc;
}
