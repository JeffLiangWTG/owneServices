using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ImportLicenseAttachingObjectCollection))]
	class ImportLicenseAttachingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ImportLicenseAttachingObjectCollection>
	{
		public void TestLoad()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "Inst-1";
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = entryInstruction.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invLine.JI_CL = entryLine.PK;

			entryInstruction.EntryHeader.MovementReferenceNumberSetter("TST1", ZDateTime.Now);

			Factory.Save();
			var importLicenseAttaching = new ImportLicenseAttachingObjectCollection(declaration);
			importLicenseAttaching.Load();
			AssertEquals(1, importLicenseAttaching.Count);

			entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "Inst-2";
			invHeader = declaration.Invoices.AddNew();
			invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = entryInstruction.PK;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryLine = entryHeader.AllEntryLines.AddNew();
			invLine.JI_CL = entryLine.PK;

			Factory.Save();
			importLicenseAttaching = new ImportLicenseAttachingObjectCollection(declaration);
			importLicenseAttaching.Load();
			AssertEquals(1, importLicenseAttaching.Count);

			entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "Inst-3";
			invHeader = declaration.Invoices.AddNew();
			invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = entryInstruction.PK;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryLine = entryHeader.AllEntryLines.AddNew();
			invLine.JI_CL = entryLine.PK;

			entryInstruction.EntryHeader.MovementReferenceNumberSetter("TST3", ZDateTime.Now);

			Factory.Save();
			importLicenseAttaching = new ImportLicenseAttachingObjectCollection(declaration);
			importLicenseAttaching.Load();
			AssertEquals(2, importLicenseAttaching.Count);

			var genPivot = Factory.New<GenPivot>();
			genPivot.XX_RelationType = GenPivotTypeDecider.Types.JobDecRelatedImportLicenseEntryGenPivot;
			genPivot.Relation1ID = declaration.PK;
			genPivot.Relation2ID = entryInstruction.PK;

			Factory.Save();
			importLicenseAttaching = new ImportLicenseAttachingObjectCollection(declaration);
			importLicenseAttaching.Load();
			AssertEquals(1, importLicenseAttaching.Count);
		}

		public void TestAllowNew()
		{
			AssertEquals("AllowNew", false, GetCollectionToTest().AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals("AllowRemove", false, GetCollectionToTest().AllowRemove);
		}

		protected override ImportLicenseAttachingObjectCollection GetCollectionToTest()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var importLicenseCollection = new ImportLicenseAttachingObjectCollection(jobDeclaration);
			return importLicenseCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => new ImportLicenseAttachingObject(Factory.New<CusEntryInstruction>());
	}
}
