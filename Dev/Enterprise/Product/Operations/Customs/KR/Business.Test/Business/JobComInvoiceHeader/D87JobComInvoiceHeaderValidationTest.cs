using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class D87JobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationTest
	{
		public override void TestCheckJZ_MarksAndNumbersIsWesternEuropeanIfRequired()
		{
			invoice.JZ_MarksAndNumbers = "XX1";
			AssertNoErrors(invoice.JZ_MarksAndNumbersInfo);
			invoice.JZ_MarksAndNumbers = "你好";
			AssertNoErrors(invoice.JZ_MarksAndNumbersInfo);
		}

		public override void TestValidateJZ_CU_RelatedHouseBill()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				Bill bill = testDeclaration.Bills.AddNew();
				invoice.Validation.ValidateJZ_CU_RelatedHouseBill();
				AssertNoMessageErrors(invoice.JZ_CU_RelatedHouseBillInfo);
				testDeclaration.ActiveEntryHeaders.AddNew();
				invoice.Validation.ValidateJZ_CU_RelatedHouseBill();
				AssertNoMessageErrors(invoice.JZ_CU_RelatedHouseBillInfo);
				testDeclaration.ActiveEntryHeaders.AddNew();
				invoice.Validation.ValidateJZ_CU_RelatedHouseBill();
				AssertNoMessageErrors(invoice.JZ_CU_RelatedHouseBillInfo);
				invoice.JZ_CU_RelatedHouseBill = bill.PK;
				AssertNoMessageErrors(invoice.JZ_CU_RelatedHouseBillInfo);
			}
		}

		public override void TestValidateBalance()
		{
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 100;
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			testDeclaration.ResumeApportionment();
			AssertNoMessageErrors(invoiceHeader.JZ_Calc_BalanceInfo);

			invoiceHeader.JZ_InvoiceAmount = 100m;
			testDeclaration.ResumeApportionment();
			AssertNoMessageErrors(invoiceHeader.JZ_Calc_BalanceInfo);

			invoiceHeader.JZ_InvoiceAmount = 0m;
			testDeclaration.ResumeApportionment();
			AssertNoMessageErrors(invoiceHeader.JZ_Calc_BalanceInfo);
		}

		public override void TestValidateJZ_OH_Buyer()
		{
			Assert(!invoice.JZ_OH_BuyerInfo.HasErrors());
			invoice.JZ_OH_Buyer = ZGuid.Empty;
			invoice.Validation.ValidateJZ_OH_Buyer();
			Assert(!invoice.JZ_OH_BuyerInfo.HasErrors());

			invoice.JZ_JE = testDeclaration.PK;
			invoice.Validation.ValidateJZ_OH_Buyer();
			Assert(!invoice.JZ_OH_BuyerInfo.HasErrors());
		}

		public override void TestValidateJZ_OH_Supplier()
		{
			AssertNotNull(testDeclaration);
			invoice.JZ_JE = ZGuid.Empty;
			new FakeDeclarationCreatorForInvoice(invoice);
			invoice.JZ_OH_Supplier = ZGuid.Empty;
			AssertNoMessageErrors(invoice.JZ_OH_SupplierInfo);
			invoice.JZ_OH_Supplier = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
			AssertNoMessageErrors(invoice.JZ_OH_SupplierInfo);
			invoice.JZ_JE = testDeclaration.PK;
			invoice.JZ_OH_Supplier = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
			invoice.Validation.ValidateJZ_OH_Supplier();
			AssertNoErrors(invoice.JZ_OH_SupplierInfo);
			invoice.JZ_OH_Supplier = ZGuid.Empty;
			AssertNoErrors(invoice.JZ_OH_SupplierInfo);
		}

		public void TestNoMessageErrorsForUnusedFieldsOfD87()
		{
			invoice.JZ_InvoiceAmount = 1;
			invoice.JZ_RX_NKInvoice_Currency = "KRW";
			invoice.JZ_ImportCargoManagementNumber = "NO";

			var line = invoice.InvoiceLines.AddNew();
			line.JI_Weight = 1;

			invoice.Validation.ValidateAll();
			Assert(!invoice.HasMessageErrors);
		}

		protected override void SetUp()
		{
			base.SetUp();
			testDeclaration = Factory.New<JobDeclaration>();
			testDeclaration.JE_MessageType = ElectronicDocumentTypeList.Codes._D87;
			invoice = testDeclaration.Invoices[0];
		}
		JobComInvoiceHeader invoice;
		JobDeclaration testDeclaration;
	}
}
