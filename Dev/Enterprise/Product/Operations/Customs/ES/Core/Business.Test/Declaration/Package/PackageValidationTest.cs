using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	sealed class PackageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidatePackageIsAssignedToInvoiceLine()
		{
			const string message = "Package Line is not assigned to an Invoice Line.";

			declaration.FillWithValidTestData();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			var collection = (InvoiceLineCusLinkPackageCollection)invoiceLine.PackagesForInvoiceLinesForBindingOnly;

			var linkPackage = collection.AddNew();
			linkPackage.Package = package;
			linkPackage.IsLinked = false;

			CombineAssertions(() =>
			{
				package.Validation.ValidateAll();
				AssertHasRowMessageError("In export has row message when not linked", package, message);

				linkPackage.IsLinked = true;
				package.Validation.ValidateAll();
				AssertNoRowMessageError("In export no row message when linked", package, message);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				linkPackage.IsLinked = false;
				package.Validation.ValidateAll();
				AssertHasRowMessageError("In import has row message when not linked", package, message);

				linkPackage.IsLinked = true;
				package.Validation.ValidateAll();
				AssertNoRowMessageError("In import no row message when linked", package, message);
			});
		}

		public void TestCheckCW_MarksAndNos()
		{
			var expectedMessageError = "In provisional period, Marks length could not be greater than 42.";
			package.CW_MarksAndNos = "1111111111222222222233333333334444444444123";
			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				package.Validation.ValidateCW_MarksAndNos();
				AssertHasMessageErrorContaining(package.CW_MarksAndNosInfo, expectedMessageError);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				package.Validation.ValidateCW_MarksAndNos();
				AssertNoMessageErrorContaining(package.CW_MarksAndNosInfo, expectedMessageError);

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				package.CW_MarksAndNos = "111111111122222222223333333333444444444412";
				AssertNoMessageErrorContaining(package.CW_MarksAndNosInfo, expectedMessageError);
			}

			package.CW_MarksAndNos = "1111111111222222222233333333334444444444123";
			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
			{
				package.Validation.ValidateCW_MarksAndNos();
				AssertNoMessageErrorContaining(package.CW_MarksAndNosInfo, expectedMessageError);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			package = (Package)declaration.Packages.AddNew();
		}

		JobDeclaration declaration;
		Package package;
	}
}
