using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(UpdateImportEntryNumberObject))]
	class UpdateImportEntryNumberObjectTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2023, 1, 1)]
		public void TestProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var entryheader = declaration.ActiveEntryHeaders.AddNew();
			entryheader.MovementReferenceNumberSetter("1111111111", ZDateTime.Today);

			var updateImportEntryNumber = new UpdateImportEntryNumberObject(declaration);
			AssertEquals("EntryNumber should be equal", "1111111111", updateImportEntryNumber.EntryNumber);
			AssertEquals("RegistrationDate should be equal", ZDateTime.Today, updateImportEntryNumber.RegistrationDate);

			updateImportEntryNumber.EntryNumber = "2222222222";
			updateImportEntryNumber.RegistrationDate = ZDateTime.Today.AddDays(-1);
			AssertEquals("EntryNumber should be equal", "2222222222", updateImportEntryNumber.EntryNumber);
			AssertEquals("RegistrationDate should be equal", ZDateTime.Today.AddDays(-1), updateImportEntryNumber.RegistrationDate);
		}

		[TestDate(2023, 1, 1)]
		public void TestUpdateEntryNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			declaration.ActiveEntryHeaders.AddNew();

			var updateImportEntryNumber = new UpdateImportEntryNumberObject(declaration);
			updateImportEntryNumber.RegistrationDate = ZDateTime.Today.AddDays(-1);
			updateImportEntryNumber.EntryNumber = "2222222222";
			updateImportEntryNumber.UpdateEntryNumber();

			declaration.Reload();
			var entryheader = declaration.ActiveEntryHeaders[0];

			AssertEquals("MovementReferenceNumber should be equal", updateImportEntryNumber.EntryNumber, entryheader.MovementReferenceNumber);
			AssertEquals("MovementReferenceNumberIssueDate should be equal", updateImportEntryNumber.RegistrationDate, entryheader.MovementReferenceNumberIssueDate);

			updateImportEntryNumber.RegistrationDate = ZDateTime.Empty;
			updateImportEntryNumber.EntryNumber = ZString.Empty;
			updateImportEntryNumber.UpdateEntryNumber();

			declaration.Reload();
			entryheader = declaration.ActiveEntryHeaders[0];

			AssertEquals("MovementReferenceNumber should be equal", ZString.Empty, entryheader.MovementReferenceNumber);
			AssertEquals("MovementReferenceNumberIssueDate should be equal", ZDateTime.Empty, entryheader.MovementReferenceNumberIssueDate);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.ActiveEntryHeaders.AddNew();
			return new UpdateImportEntryNumberObject(declaration);
		}
	}
}

