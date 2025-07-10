using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ImportLicenseAttachingObjectParent))]
	class ImportLicenseAttachingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestImportLicenses()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "TEST1";
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = entryInstruction.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invLine.JI_CL = entryLine.PK;
			entryInstruction.EntryHeader.MovementReferenceNumberSetter("TST1", ZDateTime.Now);
			Factory.Save();
			var importLicense = new ImportLicenseAttachingObjectParent(declaration);
			AssertEquals(1, importLicense.ImportLicenses.Count);
		}

		protected override BusinessObject GetNewBusinessObject() => new ImportLicenseAttachingObjectParent(Factory.New<JobDeclaration>());
	}
}
