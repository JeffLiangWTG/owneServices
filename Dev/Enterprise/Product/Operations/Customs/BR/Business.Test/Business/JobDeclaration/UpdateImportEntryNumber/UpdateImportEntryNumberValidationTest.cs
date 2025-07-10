using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class UpdateImportEntryNumberValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckEntryNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var entryheader = declaration.ActiveEntryHeaders.AddNew();
			entryheader.MovementReferenceNumberSetter("1111111111", ZDateTime.Today);

			var updateImportEntryNumber = new UpdateImportEntryNumberObject(declaration);
			updateImportEntryNumber.EntryNumber = "A2222222-2";
			updateImportEntryNumber.Validation.ValidateEntryNumber();
			AssertHasError(updateImportEntryNumber.EntryNumberInfo, "Entry Number allows numeric characters.");
		}

		public void TestCheckRegistrationDate()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var entryheader = declaration.ActiveEntryHeaders.AddNew();
			entryheader.MovementReferenceNumberSetter("1111111111");

			var updateImportEntryNumber = new UpdateImportEntryNumberObject(declaration);
			updateImportEntryNumber.RegistrationDate = ZDateTime.Invalid;
			updateImportEntryNumber.Validation.ValidateRegistrationDate();
			AssertHasError(updateImportEntryNumber.RegistrationDateInfo, "Enter a valid Registration Date.");
		}

		public void TestGetDuplicateEntryNumberQuery()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration1.JE_DeclarationReference = "JOB000001";

			var entryheader1 = declaration1.ActiveEntryHeaders.AddNew();
			entryheader1.MovementReferenceNumberSetter("1111111111", ZDateTime.Today);

			Factory.Save();

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var entryheader2 = declaration2.ActiveEntryHeaders.AddNew();
			entryheader2.MovementReferenceNumberSetter("2222222222", ZDateTime.Today);
			var updateImportEntryNumber = new UpdateImportEntryNumberObject(declaration2);
			updateImportEntryNumber.EntryNumber = "1111111111";
			updateImportEntryNumber.Validation.ValidateEntryNumber();
			AssertHasWarningContaining(updateImportEntryNumber.EntryNumberInfo, "The Entry Number 1111111111 is already contained in the job 'JOB000001'.");

			entryheader1.MovementReferenceNumberSetter("1111111112", ZDateTime.Today);
			Factory.Save();

			updateImportEntryNumber.Validation.ValidateEntryNumber();
			AssertNoWarningContaining(updateImportEntryNumber.EntryNumberInfo, "The Entry Number 1111111111 is already contained in the job 'JOB000001'.");
		}
	}
}

