using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(DeclarationRelatedImportLicenseEntryGenPivot))]
	class DeclarationRelatedImportLicenseEntryGenPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			AssertEquals(GenPivotTypeDecider.Types.JobDecRelatedImportLicenseEntryGenPivot, DeclarationRelatedImportLicenseEntryGenPivot.XX_RelationType);
			AssertEquals(JobDeclarationSchema.Constants.Prefix, DeclarationRelatedImportLicenseEntryGenPivot.XX_Relation1TableCode);
			AssertEquals(CusEntryInstructionSchema.Constants.Prefix, DeclarationRelatedImportLicenseEntryGenPivot.XX_Relation2TableCode);
		}

		public void TestProperties()
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
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.CH_EntryReleaseDate = date.AddDays(1);
			entryHeader.MovementReferenceNumberSetter("TST1", date);

			var pivot = GetNewBusinessObject() as DeclarationRelatedImportLicenseEntryGenPivot;
			pivot.XX_Relation1ID = Factory.NewWithValidTestData<JobDeclaration>().PK;
			pivot.XX_Relation2ID = entryInstruction.PK;

			CombineAssertions(() =>
			{
				AssertEquals("ImportLicenseEntryDescription", "Inst-1", pivot.ImportLicenseEntryDescription);
				AssertEquals("ImportLicenseEntryMRN", "TST1", pivot.ImportLicenseEntryMRN);
				AssertEquals("ImportLicenseEntryRegistrationDate", date, pivot.ImportLicenseEntryRegistrationDate);
				AssertEquals("ImportLicenseEntryStatusDescription", ZString.Empty, pivot.ImportLicenseEntryStatusDescription);
				AssertEquals("ImportLicenseConcessionDate", date.AddDays(1), pivot.ImportLicenseConcessionDate);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return DeclarationRelatedImportLicenseEntryGenPivot;
		}

		DeclarationRelatedImportLicenseEntryGenPivot DeclarationRelatedImportLicenseEntryGenPivot
		{
			get
			{
				return fDeclarationRelatedImportLicenseEntryGenPivot ?? (fDeclarationRelatedImportLicenseEntryGenPivot = Factory.New<DeclarationRelatedImportLicenseEntryGenPivot>());
			}
		}

		DeclarationRelatedImportLicenseEntryGenPivot fDeclarationRelatedImportLicenseEntryGenPivot;
	}
}
