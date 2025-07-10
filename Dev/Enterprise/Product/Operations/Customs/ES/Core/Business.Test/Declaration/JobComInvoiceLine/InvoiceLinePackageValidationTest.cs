using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	class InvoiceLinePackageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPackQty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();

			entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.A;
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryInstruction1.CEI_SubStyle = ExsEntrySubStyleList.Codes.A;
			invoiceLine1.JI_CEI = entryInstruction1.PK;

			var packingGroup = declaration.Bills.AddNew().PackingGroups.AddNew();
			var package1 = packingGroup.Packages.AddNew();
			package1.CW_PackType = "FR";
			package1.CW_PackQty = 1;
			package1.CW_MarksAndNos = "AAAAA";
			var package2 = packingGroup.Packages.AddNew();
			package2.CW_PackType = "FR";
			package2.CW_PackQty = 0;
			package2.CW_MarksAndNos = "AAAAA";

			var linePackageCollection = invoiceLine.PackagesForInvoiceLinesForBindingOnly;
			var lineLinkPackage1 = linePackageCollection[0];
			lineLinkPackage1.Package = package1;
			lineLinkPackage1.IsLinked = true;
			lineLinkPackage1.Quantity = 1;
			var linePackageCollection1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly;
			var lineLinkPackage2 = linePackageCollection1[0];
			lineLinkPackage2.Package = package2;
			lineLinkPackage2.IsLinked = true;
			lineLinkPackage1.Quantity = 0;

			var messageError = "You have not entered a value.";
			var messageErrorPackageQuantity = "You have not entered a Pack Quantity.";

			CombineAssertions(() =>
			{
				lineLinkPackage1.Validation.ValidateAll();
				lineLinkPackage2.Validation.ValidateAll();
				AssertNoNotifications("For two entry A and quantity greater than 0, not exist message in line 1", lineLinkPackage1.PackQtyInfo);
				AssertHasMessageErrorContaining("For two entry A and quantity 0, have message in line 2", lineLinkPackage2.PackQtyInfo, messageError);

				entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				invoiceLine.JI_CEI = entryInstruction.PK;
				entryInstruction1.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				invoiceLine1.JI_CEI = entryInstruction1.PK;
				lineLinkPackage1.Validation.ValidateAll();
				lineLinkPackage2.Validation.ValidateAll();
				AssertNoNotifications("For two entry EXS and quantity greater than 0, not exist message in line 1", lineLinkPackage1.PackQtyInfo);
				AssertNoNotifications("For two entry EXS and Quantity 0, not exist message in line 2", lineLinkPackage2.PackQtyInfo);

				package2.CW_PackType = "BX";
				lineLinkPackage2.Validation.ValidateAll();
				AssertHasMessageErrorContaining("For two entry EXS and Quantity 0 but differents type of packages, exist message in line 2", lineLinkPackage2.PackQtyInfo, messageErrorPackageQuantity);

				package2.CW_PackType = "FR";
				entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.A;
				invoiceLine.JI_CEI = entryInstruction.PK;
				lineLinkPackage2.Validation.ValidateAll();
				AssertHasMessageErrorContaining("For two entry, one EXS and other A and Quantity 0, exist message in line 2", lineLinkPackage2.PackQtyInfo, messageErrorPackageQuantity);

				lineLinkPackage1.PackQty = 100000;
				var expectedMessageError = "In provisional period, Pack Quantity cannot be greater than 99999.";
				using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
				{
					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					lineLinkPackage1.Validation.ValidatePackQty();
					AssertNoMessageErrorContaining("Assert > 99999 PackQty in AES IMPORT", lineLinkPackage1.PackQtyInfo, expectedMessageError);

					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					lineLinkPackage1.Validation.ValidatePackQty();
					AssertHasMessageErrorContaining("Assert > 99999 PackQty in AES EXPORT", lineLinkPackage1.PackQtyInfo, expectedMessageError);

					lineLinkPackage1.PackQty = 99999;
					lineLinkPackage1.Validation.ValidatePackQty();
					AssertNoMessageErrorContaining("Assert <= 99999 PackQty in AES EXPORT", lineLinkPackage1.PackQtyInfo, expectedMessageError);
				}

				lineLinkPackage1.PackQty = 100000;
				using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
				{
					lineLinkPackage1.Validation.ValidatePackQty();
					AssertNoMessageErrorContaining("Assert > 99999 PackQty in EDI", lineLinkPackage1.PackQtyInfo, expectedMessageError);
				}
			});
		}
	}
}
