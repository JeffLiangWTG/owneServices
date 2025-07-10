using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class PackageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCW_PackQty()
		{
			package.CW_PackQty = 0;
			AssertHasMessageErrorContaining(package.CW_PackQtyInfo, MandatoryValidation.YouHaveNotEntered);

			package.CW_PackQty = 1;
			AssertNoMessageErrors(package.CW_PackQtyInfo);
		}

		public void TestCheckCW_PackType()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			package.CW_PackType = "PKG";
			AssertHasMessageErrorContaining(package.CW_PackTypeInfo, ListValidation.InvalidCodeMessageError);

			package.CW_PackType = "";
			AssertHasMessageErrorContaining(package.CW_PackTypeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			package.CW_PackType = ACROSSPackageTypes.Codes.AMMOPACK;
			AssertNoMessageErrors(package.CW_PackTypeInfo);
		}

		public void TestCheckEnterAtLeastOnePackWithQuantityAndPackType()
		{
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			package.CW_PackType = "";
			package.CW_PackQty = 1;
			AssertNoRowMessageError(package, PackageValidation.EnterAtLeastOnePackWithQuantityAndPackType);

			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			package.Validation.ValidateAll();
			AssertHasRowMessageError(package, PackageValidation.EnterAtLeastOnePackWithQuantityAndPackType);

			package.CW_PackType = "123";
			package.CW_PackQty = 0;
			AssertHasRowMessageError(package, PackageValidation.EnterAtLeastOnePackWithQuantityAndPackType);

			var package1 = (Package)declaration.Packages.AddNew();
			package1.CW_PackType = "123";
			package1.CW_PackQty = 1;
			AssertNoRowMessageError(package1, PackageValidation.EnterAtLeastOnePackWithQuantityAndPackType);

			package.Validation.ValidateAll();
			AssertNoRowMessageError(package, PackageValidation.EnterAtLeastOnePackWithQuantityAndPackType);
		}

		public void TestLinkAtLeastOneInvoice()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			AssertNotNull(invoiceLine.PackagesForInvoiceLinesForBindingOnly);
			AssertEquals(1, invoiceLine.PackagesForInvoiceLinesForBindingOnly.Count);
			Assert("Is marked for invoice line", invoiceLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked);

			AssertNotNull(invoice.PackagesForInvoicesForBindingOnly);
			AssertEquals(1, invoice.PackagesForInvoicesForBindingOnly.Count);
			AssertEquals(false, invoice.PackagesForInvoicesForBindingOnly[0].IsLinked);
			AssertNoRowMessageError(package, PackageValidation.LinkAtLeastOneInvoiceOrInvoiceLine);

			invoiceLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = false;
			AssertHasRowMessageError(package, PackageValidation.LinkAtLeastOneInvoiceOrInvoiceLine);

			invoice.PackagesForInvoicesForBindingOnly[0].IsLinked = true;
			AssertNoRowMessageError(package, PackageValidation.LinkAtLeastOneInvoiceOrInvoiceLine);

			invoiceLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;
			AssertNoRowMessageError(package, PackageValidation.LinkAtLeastOneInvoiceOrInvoiceLine);
		}

		public void TestCheckCW_CW_Parent()
		{
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_MasterBill = "ABC123";

			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 1;
			package1.CW_PackType = Core.Constants.PkgUnit.Bag;
			package1.CW_HouseBill = "ABC123";

			var package2 = declaration.Packages.AddNew();
			package2.CW_PackQty = 2;
			package2.CW_PackType = Core.Constants.PkgUnit.Box;
			package2.CW_CW_Parent = package1.PK;

			var package3 = declaration.Packages.AddNew();
			package3.CW_PackQty = 10;
			package3.CW_PackType = Core.Constants.PkgUnit.Basket;
			package3.CW_CW_Parent = package2.PK;

			var package4 = declaration.Packages.AddNew();
			package4.CW_PackQty = 20;
			package4.CW_PackType = Core.Constants.PkgUnit.Roll;
			package4.CW_CW_Parent = package3.PK;
			AssertEquals("Package.CW_CW_ParentPackageInfo.HasMessageErrors() should be true", true, package4.CW_CW_ParentInfo.HasMessageError(PackageValidation.MaxPackingHeirarchyError));

			var package5 = declaration.Packages.AddNew();
			package5.CW_PackQty = 30;
			package5.CW_PackType = Core.Constants.PkgUnit.Envelope;
			package5.CW_CW_Parent = package4.PK;
			AssertEquals("Package.CW_CW_ParentPackageInfo.HasMessageErrors() should be true", true, package5.CW_CW_ParentInfo.HasMessageError(PackageValidation.MaxPackingHeirarchyError));

			package5.CW_CW_Parent = ZGuid.Empty;
			AssertEquals("Package.CW_CW_ParentPackageInfo.HasMessageErrors() should be false", false, package5.CW_CW_ParentInfo.HasMessageError(PackageValidation.MaxPackingHeirarchyError));

			package5.CW_CW_Parent = package2.PK;
			AssertEquals("Package.CW_CW_ParentPackageInfo.HasMessageErrors() should be false", false, package5.CW_CW_ParentInfo.HasMessageError(PackageValidation.MaxPackingHeirarchyError));
		}

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = "IID";
			package = (Package)declaration.Packages.AddNew();
			houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "house";
			package.CW_HouseBill = houseBill.CU_BillUniqueCode;
		}

		Bill houseBill;
		JobDeclaration declaration;
		Package package;

		#endregion
	}
}
