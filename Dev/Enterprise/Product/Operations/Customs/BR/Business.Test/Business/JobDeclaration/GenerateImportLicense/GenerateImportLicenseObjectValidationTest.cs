using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class GenerateImportLicenseObjectValidationTest : TestCaseWithFactory
	{
		public void TestCheckImportLicenseDeclarationPK()
		{
			var iswDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			iswDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			iswDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = iswDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "Inst-1";
			var invHeader = iswDeclaration.Invoices.AddNew();
			invHeader.JZ_InvoiceNumber = "ISW_HEADER";
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = entryInstruction.PK;
			var entryHeader = iswDeclaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invLine.JI_CL = entryLine.PK;

			entryInstruction.EntryHeader.MovementReferenceNumberSetter("TST1", ZDateTime.Now);

			var licDeclarationWithoutHeader = Factory.NewWithValidTestData<JobDeclaration>();
			licDeclarationWithoutHeader.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var licDeclarationWithHeader = Factory.NewWithValidTestData<JobDeclaration>();
			licDeclarationWithHeader.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var licInvHeaderWithHeader = licDeclarationWithHeader.Invoices.AddNew();
			licInvHeaderWithHeader.JZ_InvoiceNumber = "ONLY_HEADER";

			var licDeclarationWithLine = Factory.NewWithValidTestData<JobDeclaration>();
			licDeclarationWithLine.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var licInvHeaderWithLine = licDeclarationWithLine.Invoices.AddNew();
			licInvHeaderWithLine.JZ_InvoiceNumber = "WITH_LINE";
			licInvHeaderWithLine.InvoiceLines.AddNew();

			var generateIL = new GenerateImportLicenseObject(entryLine);

			generateIL.ImportLicenseDeclarationPK = ZGuid.Invalid;
			AssertNoErrorContaining(generateIL.ImportLicenseDeclarationPKInfo, "License declaration already contains the Invoice Header:");
			AssertHasError(generateIL.ImportLicenseDeclarationPKInfo, "Please select one Import License to Generate.");

			generateIL.ImportLicenseDeclarationPK = licDeclarationWithoutHeader.PK;
			AssertNoErrorContaining(generateIL.ImportLicenseDeclarationPKInfo, "License declaration already contains the Invoice Header:");
			AssertNoError(generateIL.ImportLicenseDeclarationPKInfo, "Please select one Import License to Generate.");

			generateIL.ImportLicenseDeclarationPK = licDeclarationWithHeader.PK;
			AssertNoErrorContaining(generateIL.ImportLicenseDeclarationPKInfo, "License declaration already contains the Invoice Header:");

			licInvHeaderWithHeader.JZ_InvoiceNumber = "ISW_HEADER";
			generateIL.ImportLicenseDeclarationPK = licDeclarationWithHeader.PK;
			AssertNoErrorContaining(generateIL.ImportLicenseDeclarationPKInfo, "License declaration already contains the Invoice Header:");

			generateIL.ImportLicenseDeclarationPK = licDeclarationWithLine.PK;
			AssertNoErrorContaining(generateIL.ImportLicenseDeclarationPKInfo, "License declaration already contains the Invoice Header:");

			licInvHeaderWithLine.JZ_InvoiceNumber = "ISW_HEADER";
			generateIL.ImportLicenseDeclarationPK = licDeclarationWithLine.PK;
			AssertHasErrorContaining(generateIL.ImportLicenseDeclarationPKInfo, "License declaration already contains the Invoice Header:");
			AssertHasError(generateIL.ImportLicenseDeclarationPKInfo, "License declaration already contains the Invoice Header: ISW_HEADER");
		}
	}
}
