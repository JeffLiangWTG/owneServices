using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.Testing;
class CsvCodeInfoValidationTest : TestCaseWithFactory
{
	public void TestCheckCsvCodeFromUser()
	{
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		var csvCodeInfo = CsvCodeInfo.LoadNew(temporaryStorageHeader);
		CombineAssertions(() =>
		{
			AssertNoErrors("No errors when CsvCodeFromUser is empty", csvCodeInfo.CsvCodeFromUserInfo);

			csvCodeInfo.CsvCodeFromUser = "+--**__aa/#€&@";
			AssertHasErrorContaining("There is an error when CsvCodeFromUser has an incorrect format", csvCodeInfo.CsvCodeFromUserInfo, "+--**__aa/#€&@ format is incorrect. Please fill with a correct CSV Clearance Code");

			csvCodeInfo.CsvCodeFromUser = "ABCDEFGHIJKLMNOP";
			AssertNoErrors("No errors when CsvCodeFromUser is filled with the correct format", csvCodeInfo.CsvCodeFromUserInfo);
		});
	}

	public void TestCheckClearanceDateFromUser()
	{
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		var csvCodeInfo = CsvCodeInfo.LoadNew(temporaryStorageHeader);
		CombineAssertions(() =>
		{
			AssertNoErrors("No errors when ClearanceDateFromUser is empty and CsvCodeFromUser is also empty", csvCodeInfo.ClearanceDateFromUserInfo);

			csvCodeInfo.ClearanceDateFromUser = ZDateTime.Invalid;
			AssertHasErrorContaining("There is an error when ClearanceDateFromUser has an invalid value", csvCodeInfo.ClearanceDateFromUserInfo, "Clearance Date is invalid. Please fill with a correct date");

			csvCodeInfo.CsvCodeFromUser = "ABCDEFGHIJKLMNOP";
			csvCodeInfo.ClearanceDateFromUser = ZDateTime.Now;
			AssertNoErrors("No errors when ClearanceDateFromUser is filled with the correct value", csvCodeInfo.ClearanceDateFromUserInfo);

			csvCodeInfo.ClearanceDateFromUser = ZDateTime.Empty;
			AssertHasErrorContaining("There is an error when ClearanceDateFromUser is empty and CsvCodeFromUser is not empty", csvCodeInfo.ClearanceDateFromUserInfo, "Clearance Date is mandatory when CSV Clearance is entered");
		});
	}
}
