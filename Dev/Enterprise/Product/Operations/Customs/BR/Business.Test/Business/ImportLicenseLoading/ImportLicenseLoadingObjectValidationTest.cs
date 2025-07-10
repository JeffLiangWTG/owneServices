using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportLicenseLoadingObjectValidationTest : TestCaseWithFactory
	{
		public void TestParent()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var importLicenseParent = new ImportLicenseLoadingObjectParent(declaration);
			var parent = new ImportLicenseLoadingObject(importLicenseParent);
			AssertType<ImportLicenseLoadingObject>("Validation Parent must be ImportLicenseLoadingObject", parent.Validation.Parent);
		}

		public void TestCheckInvoiceHeaderPK()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "Inst-1";
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV1";

			var importLicenseParent = new ImportLicenseLoadingObjectParent(declaration);
			var importLicenseLoadingObject = importLicenseParent.Collection.AddNew();
			importLicenseLoadingObject.Declaration = declaration;

			var lookups = new ImportLicenseLoadingObjectLookups(importLicenseLoadingObject);
			AssertEquals(lookups.InvoiceHeaderList, importLicenseLoadingObject.Declaration.Invoices);

			importLicenseLoadingObject.InvoiceHeaderPK = ZGuid.Empty;
			AssertNoNotifications(importLicenseLoadingObject.InvoiceHeaderPKInfo);

			importLicenseLoadingObject.InvoiceHeaderPK = invoiceHeader.PK;
			AssertNoNotifications(importLicenseLoadingObject.InvoiceHeaderPKInfo);

			importLicenseLoadingObject.InvoiceHeaderPK = ZGuid.Invalid;
			AssertHasError(importLicenseLoadingObject.InvoiceHeaderPKInfo, "Enter a valid Invoice No..");
		}
	}
}
