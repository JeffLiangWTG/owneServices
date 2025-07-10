using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class TemporaryStoragePackValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckAPA_MarksAndNumbers()
	{
		const string expectedErrorMessage = "You have entered more than 512 digits in the field Marks";
		var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		var storageBill = storageHeader.Bills.AddNew();
		var temporaryStoragePack = storageBill.Packs.AddNew();

		temporaryStoragePack.APA_MarksAndNumbers = new ZString('A', 513);
		temporaryStoragePack.Validation.ValidateAPA_MarksAndNumbers();
		AssertHasMessageError("APA_MarksAndNumbers has length more than 512", temporaryStoragePack.APA_MarksAndNumbersInfo, expectedErrorMessage);
		temporaryStoragePack.APA_MarksAndNumbers = "12345";
		temporaryStoragePack.Validation.ValidateAPA_MarksAndNumbers();
		AssertNoMessageError("APA_MarksAndNumbers has length less than 512", temporaryStoragePack.APA_MarksAndNumbersInfo, expectedErrorMessage);
	}

	public void TestCheckAPA_PackQty()
	{
		const string expectedErrorMessage = "You have entered more than 8 digits in the field Quantity";
		var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		var storageBill = storageHeader.Bills.AddNew();
		var temporaryStoragePack = storageBill.Packs.AddNew();

		temporaryStoragePack.APA_PackQty = 1000000000;
		temporaryStoragePack.Validation.ValidateAPA_PackQty();
		AssertHasMessageError("APA_PackQty has more than 8 digits", temporaryStoragePack.APA_PackQtyInfo, expectedErrorMessage);
		temporaryStoragePack.APA_PackQty = 12345;
		temporaryStoragePack.Validation.ValidateAPA_PackQty();
		AssertNoMessageError("APA_PackQty has less than 8 digits", temporaryStoragePack.APA_PackQtyInfo, expectedErrorMessage);
	}
}
