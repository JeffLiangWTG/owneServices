using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportLicenseAttachingObjectValidationTest : TestCaseWithFactory
	{
		public void TestCheckShouldAttach()
		{
			var date = ZDateTime.Now;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "Inst-1";

			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = entryInstruction.PK;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invLine.JI_CL = entryLine.PK;

			entryInstruction.EntryHeader.MovementReferenceNumberSetter("0123456789", date);
			var importLicenseAttaching = new ImportLicenseAttachingObject((CusEntryInstruction)entryInstruction);
			importLicenseAttaching.ShouldAttach = true;

			AssertNoErrorContaining(importLicenseAttaching.ShouldAttachInfo, "The Import License Number cannot exceed the length of 10.");

			entryInstruction.EntryHeader.MovementReferenceNumberSetter("01234567891", date);
			importLicenseAttaching.Validation.ValidateShouldAttach();
			AssertHasErrorContaining(importLicenseAttaching.ShouldAttachInfo, "The Import License Number cannot exceed the length of 10.");

			importLicenseAttaching.ShouldAttach = false;
			AssertNoErrorContaining(importLicenseAttaching.ShouldAttachInfo, "The Import License Number cannot exceed the length of 10.");
		}
	}
}
