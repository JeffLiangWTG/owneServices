using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class DeltaIEJobComInvoiceHeaderValidationTest : TestCaseWithFactory
	{
		public void TestCheckJZ_RX_NKInvoice_Currency()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceHeader.JZ_RX_NKInvoice_CurrencyInfo);
		}

		public void TestCheckJZ_OA_SupplierAddress()
		{
			invoiceHeader.JZ_OA_SupplierAddress = orgHeader.MainAddress.PK;
			var address = invoiceHeader.SupplierAddress;
			var info = invoiceHeader.JZ_OA_SupplierAddressInfo;
			DeclarationValidationTestHelper.AssertEORIOrFullAddress(address, info, invoiceHeader.Validation.ValidateJZ_OA_SupplierAddress);
		}

		public void TestCheckJZ_OA_BuyerAddress()
		{
			invoiceHeader.JZ_OA_BuyerAddress = orgHeader.MainAddress.PK;
			var address = invoiceHeader.BuyerAddress;
			var info = invoiceHeader.JZ_OA_BuyerAddressInfo;
			DeclarationValidationTestHelper.AssertEORIOrFullAddress(address, info, invoiceHeader.Validation.ValidateJZ_OA_BuyerAddress);
		}

		public void TestCheckJZ_OA_SellerAddress()
		{
			invoiceHeader.JZ_OA_SellerAddress = orgHeader.MainAddress.PK;
			var address = invoiceHeader.SellerAddress;
			var info = invoiceHeader.JZ_OA_SellerAddressInfo;
			DeclarationValidationTestHelper.AssertEORIOrFullAddress(address, info, invoiceHeader.Validation.ValidateJZ_OA_SellerAddress);
		}

		public void TestShouldCheckMissingPreviousDocuments()
		{
			var validation = new JobComInvoiceHeaderValidationForTest(invoiceHeader);
			AssertEquals("ShouldCheckMissingPreviousDocuments", false, validation.ShouldCheckMissingPreviousDocumentsExposed);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			invoiceHeader = declaration.Invoices.AddNew();
			orgHeader = Factory.New<OrgHeader>();
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		OrgHeader orgHeader;

		class JobComInvoiceHeaderValidationForTest : DeltaIEJobComInvoiceHeaderValidation
		{
			public JobComInvoiceHeaderValidationForTest(JobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
			{
			}

			public ZBool ShouldCheckMissingPreviousDocumentsExposed => ShouldCheckMissingPreviousDocuments;
		}
	}
}
