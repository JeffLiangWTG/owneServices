using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class InvoiceBatchHeaderValidationTest : TestCaseWithFactory
	{
		public void TestCheckAH_OSTotal()
		{
			InvoiceBatchHeader testBatchHeader = Factory.NewWithValidTestData<InvoiceBatchHeader>();

			testBatchHeader.Validation.ValidateAH_OSTotal();

			AssertHasError(testBatchHeader.AH_OSTotalInfo, InvoiceBatchHeaderValidation.CheckAH_OSTotalErrMsg_ForTestOnly);

			ARInvoice invoiceToAdd = Factory.NewWithValidTestData<ARInvoice>();
			testBatchHeader.Line.Add(invoiceToAdd);

			testBatchHeader.Validation.ValidateAH_OSTotal();
			AssertNoError(testBatchHeader.AH_OSTotalInfo, InvoiceBatchHeaderValidation.CheckAH_OSTotalErrMsg_ForTestOnly);
		}

		public void TestCheckAH_OSTotalWhenCancelled()
		{
			InvoiceBatchHeader testBatchHeader = Factory.NewWithValidTestData<InvoiceBatchHeader>();

			testBatchHeader.AH_IsCancelled = true;

			testBatchHeader.Validation.ValidateAH_OSTotal();

			AssertNoError(testBatchHeader.AH_OSTotalInfo, InvoiceBatchHeaderValidation.CheckAH_OSTotalErrMsg_ForTestOnly);

			testBatchHeader.AH_IsCancelled = false;

			testBatchHeader.Validation.ValidateAH_OSTotal();

			AssertHasError(testBatchHeader.AH_OSTotalInfo, InvoiceBatchHeaderValidation.CheckAH_OSTotalErrMsg_ForTestOnly);
		}

		public void TestAH_OH()
		{
			InvoiceBatchHeader testBatchHeader = Factory.NewWithValidTestData<InvoiceBatchHeader>();

			testBatchHeader.AH_OH = ZGuid.Empty;
			testBatchHeader.Validation.ValidateAH_OH();
			AssertHasError(testBatchHeader.AH_OHInfo, "Invoice Batch function is restricted to a single debtor. Please enter an organization.");

			testBatchHeader.AH_OH = TestObjectCreator.AALSHI.PK;
			testBatchHeader.Validation.ValidateAH_OH();
			AssertNoError(testBatchHeader.AH_OHInfo, "Invoice Batch function is restricted to a single debtor. Please enter an organization.");

			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes.AddNew();
			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes.AddNew();
			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes[0].PI_Module = JobInvoicingConsumerTypes.CFSShipment.Code;
			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes[0].PI_Type = InvoiceTypeLayoutList.Codes.INV;
			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes[1].PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes[1].PI_Type = InvoiceTypeLayoutList.Codes.CHG;

			testBatchHeader.JobTypeList[testBatchHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.CFS)].Value = true;
			testBatchHeader.JobTypeList[testBatchHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.FWD)].Value = true;

			testBatchHeader.Validation.ValidateAH_OH();
			AssertHasError(testBatchHeader.AH_OHInfo, "This debtor has different layout setup information for the selected job types. Review the 'Invoice Batching' configuration for this debtor");

			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes[1].PI_Type = InvoiceTypeLayoutList.Codes.INV;
			testBatchHeader.Validation.ValidateAH_OH();
			AssertNoError(testBatchHeader.AH_OHInfo, "This debtor has different layout setup information for the selected job types. Review the 'Invoice Batching' configuration for this debtor");
		}

		public void TestAH_RX_NKTransactionCurrency()
		{
			InvoiceBatchHeader testBatchHeader = Factory.NewWithValidTestData<InvoiceBatchHeader>();

			testBatchHeader.AH_RX_NKTransactionCurrency = ZString.Empty;
			testBatchHeader.Validation.ValidateAH_RX_NKTransactionCurrency();
			AssertHasError(testBatchHeader.AH_RX_NKTransactionCurrencyInfo, "Invoice Batch function is restricted to a single currency. Please enter a value.");

			testBatchHeader.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			testBatchHeader.Validation.ValidateAH_RX_NKTransactionCurrency();
			AssertNoError(testBatchHeader.AH_RX_NKTransactionCurrencyInfo, "Invoice Batch function is restricted to a single currency. Please enter a value.");
		}

		public void TestCheckAH_OH_IsValid()
		{
			var testBatchHeader = Factory.NewWithValidTestData<InvoiceBatchHeader>();
			var validation = new InvoiceBatchHeaderValidation(testBatchHeader);
			testBatchHeader.AH_OH = TestObjectCreator.Creditor1.PK;
			validation.ValidateAH_OH();
			AssertHasError(testBatchHeader.AH_OHInfo, "Enter a valid Account.");
			testBatchHeader.AH_OH = TestObjectCreator.Debtor.PK;
			validation.ValidateAH_OH();
			AssertNoErrors(testBatchHeader.AH_OHInfo);
		}

		[ExpectNoExceptions]
		public void TestCheckAH_OHDoesntThrowException()
		{
			InvoiceBatchHeader testBatchHeader = Factory.NewWithValidTestData<InvoiceBatchHeader>();
			testBatchHeader.AH_OH = ZGuid.NewZGuid();
			testBatchHeader.JobTypeList[testBatchHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.FWD)].Value = true;
			testBatchHeader.JobTypeList[testBatchHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.CFS)].Value = true;
			testBatchHeader.Validation.ValidateAH_OH();
		}

		public void TestCheckAH_InvoiceTerm()
		{
			InvoiceBatchHeader invoice = Factory.NewWithValidTestData<InvoiceBatchHeader>();
			invoice.AH_OH = ZGuid.Empty;
			invoice.AH_InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			AssertHasError(invoice.AH_InvoiceTermInfo, TermsAndDueDateCalculationProvider.MonthsFromInvoiceCycleDateError);
		}

		#region Implementation

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}

		#endregion
	}
}
