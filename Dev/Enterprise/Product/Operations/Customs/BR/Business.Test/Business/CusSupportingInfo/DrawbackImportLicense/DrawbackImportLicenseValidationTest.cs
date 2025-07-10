using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class DrawbackImportLicenseValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var drawbackCollection = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().DrawbackImportLicenseCollection.AddNew();

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(drawbackCollection.CSI_CodeInfo, "8", "1");
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var drawback = invoiceLine.DrawbackImportLicenseCollection.Cast<DrawbackImportLicense>().FirstOrDefault();

			AssertNoMessageErrorContaining(drawback.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.DrawbackModality = DrawbackModalityList.Codes.GenericSuspension;
			drawback.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageErrorContaining(drawback.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.DrawbackCANumber = "XXXXX";
			drawback.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageErrorContaining(drawback.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCSI_ReferenceNumberWithDifferentCANumber()
		{
			var licDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			licDeclaration.JE_DeclarationReference = "LIC_DEC";

			var iswDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			iswDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			iswDeclaration.JE_DeclarationReference = "ISW_DEC";
			var iswInstruction = iswDeclaration.CustomsEntryInstructions.AddNew();
			iswInstruction.CEI_Description = "ISW_TEST";
			var iswInvHeader = iswDeclaration.Invoices.AddNew();
			var iswInvLine1 = iswInvHeader.InvoiceLines.AddNew();
			iswInvLine1.JI_CEI = iswInstruction.PK;
			var iswInvLine2 = iswInvHeader.InvoiceLines.AddNew();
			iswInvLine2.JI_CEI = iswInstruction.PK;
			var iswInvLine3 = iswInvHeader.InvoiceLines.AddNew();
			iswInvLine3.JI_CEI = iswInstruction.PK;
			var iswInvLine4 = iswInvHeader.InvoiceLines.AddNew();
			iswInvLine4.JI_CEI = iswInstruction.PK;
			var iswEntryHeader = iswDeclaration.ActiveEntryHeaders.AddNew();
			iswEntryHeader.CH_JE = iswDeclaration.PK;
			iswEntryHeader.CH_CEI_Instruction = iswInstruction.PK;
			var iswEntryLine1 = iswEntryHeader.MergedLines.AddNew();
			var iswEntryLine2 = iswEntryHeader.MergedLines.AddNew();

			iswInvLine1.JI_CL = iswEntryLine1.PK;
			iswInvLine2.JI_CL = iswEntryLine1.PK;

			iswInvLine3.JI_CL = iswEntryLine2.PK;
			iswInvLine4.JI_CL = iswEntryLine2.PK;

			var generator = new GenerateImportLicenseObject(iswEntryLine1);
			generator.ImportLicenseDeclarationPK = licDeclaration.PK;
			generator.GenerateImportLicense();

			generator = new GenerateImportLicenseObject(iswEntryLine2);
			generator.ImportLicenseDeclarationPK = licDeclaration.PK;
			generator.GenerateImportLicense();

			var licInvLine1 = licDeclaration.InvoiceLines[0];
			var licInvLine2 = licDeclaration.InvoiceLines[1];
			var licInvLine3 = licDeclaration.InvoiceLines[2];
			var licInvLine4 = licDeclaration.InvoiceLines[3];
			licInvLine1.DrawbackModality = DrawbackModalityList.Codes.GenericSuspension;
			licInvLine2.DrawbackModality = DrawbackModalityList.Codes.GenericSuspension;
			licInvLine3.DrawbackModality = DrawbackModalityList.Codes.GenericSuspension;
			licInvLine4.DrawbackModality = DrawbackModalityList.Codes.GenericSuspension;

			licInvLine1.DrawbackCANumber = "12345";
			licInvLine2.DrawbackCANumber = "67890";
			licInvLine3.DrawbackCANumber = "XXXXX";
			licInvLine4.DrawbackCANumber = "XXXXX";

			var drawback1 = licInvLine1.DrawbackImportLicenseCollection.Cast<DrawbackImportLicense>().FirstOrDefault();
			var drawback2 = licInvLine2.DrawbackImportLicenseCollection.Cast<DrawbackImportLicense>().FirstOrDefault();
			var drawback3 = licInvLine3.DrawbackImportLicenseCollection.Cast<DrawbackImportLicense>().FirstOrDefault();
			var drawback4 = licInvLine4.DrawbackImportLicenseCollection.Cast<DrawbackImportLicense>().FirstOrDefault();

			drawback1.Validation.ValidateCSI_ReferenceNumber();
			drawback2.Validation.ValidateCSI_ReferenceNumber();
			drawback3.Validation.ValidateCSI_ReferenceNumber();
			drawback4.Validation.ValidateCSI_ReferenceNumber();
			AssertHasError(drawback1.CSI_ReferenceNumberInfo, "CA Number cannot be different between generated invoice lines of the same Entry Line.");
			AssertHasError(drawback2.CSI_ReferenceNumberInfo, "CA Number cannot be different between generated invoice lines of the same Entry Line.");
			AssertNoError(drawback3.CSI_ReferenceNumberInfo, "CA Number cannot be different between generated invoice lines of the same Entry Line.");
			AssertNoError(drawback4.CSI_ReferenceNumberInfo, "CA Number cannot be different between generated invoice lines of the same Entry Line.");

			licInvLine2.DrawbackCANumber = "12345";
			drawback1.Validation.ValidateCSI_ReferenceNumber();
			drawback2.Validation.ValidateCSI_ReferenceNumber();
			drawback3.Validation.ValidateCSI_ReferenceNumber();
			drawback4.Validation.ValidateCSI_ReferenceNumber();
			AssertNoError(drawback1.CSI_ReferenceNumberInfo, "CA Number cannot be different between generated invoice lines of the same Entry Line.");
			AssertNoError(drawback2.CSI_ReferenceNumberInfo, "CA Number cannot be different between generated invoice lines of the same Entry Line.");
			AssertNoError(drawback3.CSI_ReferenceNumberInfo, "CA Number cannot be different between generated invoice lines of the same Entry Line.");
			AssertNoError(drawback4.CSI_ReferenceNumberInfo, "CA Number cannot be different between generated invoice lines of the same Entry Line.");

			licInvLine4.DrawbackCANumber = "ZZZZZ";
			drawback1.Validation.ValidateCSI_ReferenceNumber();
			drawback2.Validation.ValidateCSI_ReferenceNumber();
			drawback3.Validation.ValidateCSI_ReferenceNumber();
			drawback4.Validation.ValidateCSI_ReferenceNumber();
			AssertNoError(drawback1.CSI_ReferenceNumberInfo, "CA Number cannot be different between generated invoice lines of the same Entry Line.");
			AssertNoError(drawback2.CSI_ReferenceNumberInfo, "CA Number cannot be different between generated invoice lines of the same Entry Line.");
			AssertHasError(drawback3.CSI_ReferenceNumberInfo, "CA Number cannot be different between generated invoice lines of the same Entry Line.");
			AssertHasError(drawback4.CSI_ReferenceNumberInfo, "CA Number cannot be different between generated invoice lines of the same Entry Line.");
		}

		public void TestCheckCSI_ItemNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var drawback = invoiceLine.DrawbackImportLicenseCollection.Cast<DrawbackImportLicense>().FirstOrDefault();

			AssertNoMessageErrorContaining(drawback.CSI_ItemNumberInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.DrawbackModality = DrawbackModalityList.Codes.GenericSuspension;
			drawback.Validation.ValidateCSI_ItemNumber();
			AssertHasMessageErrorContaining(drawback.CSI_ItemNumberInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.DrawbackItemNumber = 111;
			drawback.Validation.ValidateCSI_ItemNumber();
			AssertNoMessageErrorContaining(drawback.CSI_ItemNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}

