using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsPackageValidation))]
sealed class NctsPackageValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckB5_TypeOfDifference() => CombineAssertions(() =>
	{
		var notEnteredMessage = MandatoryValidation.MustBeEnteredMessage("State of unloading");

		ArrivalNctsPackage.B5_TypeOfDifference = ZString.Empty;
		AssertHasError("Default - Has Error if empty", ArrivalNctsPackage.B5_TypeOfDifferenceInfo, notEnteredMessage);

		var nctsPackDifference = GetNewArrivalNctsPackage();
		nctsPackDifference.B5_B5_ParentPackage = ArrivalNctsPackage.PK;
		nctsPackDifference.Validation.ValidateB5_TypeOfDifference();
		AssertNoError("Pack Difference - No Error if empty", nctsPackDifference.B5_TypeOfDifferenceInfo, notEnteredMessage);
	});

	public void TestCheckB5_MarksAndNumber() => CombineAssertions(() =>
	{
		AssertNoError(NctsUnloadedStateList.Codes.DEC);
		AssertNoError(NctsUnloadedStateList.Codes.DIF);
		AssertNoError(NctsUnloadedStateList.Codes.MIS);
		AssertNoError(NctsUnloadedStateList.Codes.NEW);

		void AssertNoError(string typeOfDifference)
		{
			ArrivalNctsPackage.B5_TypeOfDifference = typeOfDifference;
			ArrivalNctsPackage.B5_MarksAndNumbers = ZString.Empty;
			AssertNoErrors($"TypeOfDifference={typeOfDifference}", ArrivalNctsPackage.B5_MarksAndNumbersInfo);
			var packDifference = arrivalNctsPackage.PackDifference;
			if (packDifference != null)
			{
				AssertNoErrors($"TypeOfDifference={typeOfDifference} - PackDifference", packDifference.B5_MarksAndNumbersInfo);
			}
		}
	});

	NctsPackage ArrivalNctsPackage => arrivalNctsPackage ??= GetNewArrivalNctsPackage();
	NctsPackage arrivalNctsPackage;

	NctsPackage GetNewArrivalNctsPackage()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		var goodsItem = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
		return goodsItem.Packages.AddNew();
	}
}
