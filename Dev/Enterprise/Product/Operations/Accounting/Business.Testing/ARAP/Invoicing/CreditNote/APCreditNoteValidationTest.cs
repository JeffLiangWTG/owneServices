using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class APCreditNoteValidationTest : CrediteNoteValidationTest<APCreditNote>
	{
		public void TestCheckAH_OriginalInvoiceDate()
		{
			const string expectedError = "Please enter an Original Invoice Date or select an Original Reference.";
			var date = ZDate.Today;

			SetupIOriginalInvoiceNumberAndDateValidationDecider(shouldValidate: true, (mockIOriginalInvoiceNumberAndDateValidationDecider) =>
			{
				Header.AH_ComplianceSubType = "DAR";
				Header.AH_OriginalInvoiceDate = date;

				mockIOriginalInvoiceNumberAndDateValidationDecider.Verify(x => x.ShouldValidateOriginalTransactionNumberAndDate("DAR"), Times.Once);
				AssertNoError(Header.AH_OriginalInvoiceDateInfo, expectedError);

				mockIOriginalInvoiceNumberAndDateValidationDecider.Invocations.Clear();
				Header.AH_OriginalInvoiceDate = ZDate.Empty;

				mockIOriginalInvoiceNumberAndDateValidationDecider.Verify(x => x.ShouldValidateOriginalTransactionNumberAndDate("DAR"), Times.Once);
				AssertHasError(Header.AH_OriginalInvoiceDateInfo, expectedError);
			});

			SetupIOriginalInvoiceNumberAndDateValidationDecider(shouldValidate: false, (mockIOriginalInvoiceNumberAndDateValidationDecider) =>
			{
				Header.AH_ComplianceSubType = "DAR";
				Header.AH_OriginalInvoiceDate = ZDate.Empty;

				mockIOriginalInvoiceNumberAndDateValidationDecider.Verify(x => x.ShouldValidateOriginalTransactionNumberAndDate("DAR"), Times.Once);
				AssertNoError(Header.AH_OriginalInvoiceDateInfo, expectedError);
			});
		}

		public void TestCheckAH_OriginalTransactionNum()
		{
			const string expectedError = "Please enter an Original Invoice Number or select an Original Reference.";

			SetupIOriginalInvoiceNumberAndDateValidationDecider(shouldValidate: true, (mockIOriginalInvoiceNumberAndDateValidationDecider) =>
			{
				Header.AH_ComplianceSubType = "DAR";
				Header.AH_OriginalTransactionNum = "12345";

				mockIOriginalInvoiceNumberAndDateValidationDecider.Verify(x => x.ShouldValidateOriginalTransactionNumberAndDate("DAR"), Times.Once);
				AssertNoError(Header.AH_OriginalTransactionNumInfo, expectedError);

				mockIOriginalInvoiceNumberAndDateValidationDecider.Invocations.Clear();
				Header.AH_OriginalTransactionNum = "";

				mockIOriginalInvoiceNumberAndDateValidationDecider.Verify(x => x.ShouldValidateOriginalTransactionNumberAndDate("DAR"), Times.Once);
				AssertHasError(Header.AH_OriginalTransactionNumInfo, expectedError);
			});

			SetupIOriginalInvoiceNumberAndDateValidationDecider(shouldValidate: false, (mockIOriginalInvoiceNumberAndDateValidationDecider) =>
			{
				Header.AH_ComplianceSubType = "DAR";
				Header.AH_OriginalTransactionNum = "";

				mockIOriginalInvoiceNumberAndDateValidationDecider.Verify(x => x.ShouldValidateOriginalTransactionNumberAndDate("DAR"), Times.Once);
				AssertNoError(Header.AH_OriginalTransactionNumInfo, expectedError);
			});
		}

		void SetupIOriginalInvoiceNumberAndDateValidationDecider(bool shouldValidate, Action<Mock<IOriginalInvoiceNumberAndDateValidationDecider>> assert)
		{
			var mockCountryComplianceInfo = new Mock<ICountryComplianceInfo>();

			var mockIOriginalInvoiceNumberAndDateValidationDecider = mockCountryComplianceInfo.As<IOriginalInvoiceNumberAndDateValidationDecider>();
			mockIOriginalInvoiceNumberAndDateValidationDecider
				.Setup(x => x.ShouldValidateOriginalTransactionNumberAndDate(It.IsAny<ZString>()))
				.Returns(shouldValidate);

			var countryComplianceFactoryIntegrationMock = new Mock<ICountryComplianceFactoryIntegration>();
			countryComplianceFactoryIntegrationMock.Setup(c => c.GetICountryComplianceInfo(It.IsAny<ZString>())).Returns(mockCountryComplianceInfo.Object);

			ObjectFactory.DisposeSubstitutions();
			ObjectFactory.Substitute(countryComplianceFactoryIntegrationMock.Object);

			assert(mockIOriginalInvoiceNumberAndDateValidationDecider);
		}

		public override void TestCheckAH_LocalTotalAmount_DetectsCreditedExceedInvoiced()
		{
			var originalInvoiceWithNegativeTaxAmount = (InvoicingBase)Factory.NewWithValidTestData<APInvoice>();
			var expectedWarning = "Posting this transaction will result in the Payables Organization being debited more than they have been invoiced in the related transaction/s. Do you want to continue posting?";
			var expectedError = "This amending transaction cannot be posted. Posting this transaction would result in the Payables Organization being debited more than they have been invoiced in the related transaction/s. Please review the charges you are attempting to debit.";

			if (originalInvoiceWithNegativeTaxAmount is IAmending amending)
			{
				//test original invoice with negative tax.
				AccountingConfigurationRegistry.Instance.AmendingTransactionLocalTotalBehavior.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				originalInvoiceWithNegativeTaxAmount.AH_InvoiceAmount = -10m;
				originalInvoiceWithNegativeTaxAmount.AH_GSTAmount = 0;
				AssertEquals(-10m, originalInvoiceWithNegativeTaxAmount.AH_LocalTotal);

				var invoiceToTest1 = Factory.NewWithValidTestData<APCreditNote>();
				invoiceToTest1.OriginalTransactionReference = originalInvoiceWithNegativeTaxAmount.PK;
				invoiceToTest1.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				invoiceToTest1.ExchangeRate.Currency = "USD";
				invoiceToTest1.AH_InvoiceAmount = -5m;
				invoiceToTest1.AH_GSTAmount = 0;
				AssertEquals(-5m, invoiceToTest1.AH_LocalTotal);

				var testValidation = GetValidation(invoiceToTest1);
				testValidation.ValidateAH_LocalTotalAmount();
				AssertNoWarning("Total amount is -15, so expect no warning", invoiceToTest1.AH_LocalTotalAmountInfo, expectedWarning);

				var invoiceToTest2 = Factory.NewWithValidTestData<APCreditNote>();
				invoiceToTest2.OriginalTransactionReference = originalInvoiceWithNegativeTaxAmount.PK;
				invoiceToTest2.ExchangeRate.Currency = "USD";
				invoiceToTest2.AH_InvoiceAmount = 20m;
				invoiceToTest2.AH_GSTAmount = 0;
				AssertEquals(20m, invoiceToTest2.AH_LocalTotal);
				testValidation = GetValidation(invoiceToTest2);
				testValidation.ValidateAH_LocalTotalAmount();
				AssertHasWarning("Total amount is 5 so expect warning", invoiceToTest2.AH_LocalTotalAmountInfo, expectedWarning);

				var invoiceToTest3 = Factory.NewWithValidTestData<APCreditNote>();
				invoiceToTest3.OriginalTransactionReference = originalInvoiceWithNegativeTaxAmount.PK;
				invoiceToTest3.ExchangeRate.Currency = "USD";
				invoiceToTest3.AH_InvoiceAmount = -1m;
				invoiceToTest3.AH_GSTAmount = 0;
				AssertEquals(-1m, invoiceToTest3.AH_LocalTotal);
				testValidation = GetValidation(invoiceToTest3);
				testValidation.ValidateAH_LocalTotalAmount();
				AssertNoWarning("Although total amount is 4, but since new invoice amount is negative, so expect no warning", invoiceToTest3.AH_LocalTotalAmountInfo, expectedWarning);

				var invoiceToTest4 = Factory.NewWithValidTestData<APCreditNote>();
				invoiceToTest4.OriginalTransactionReference = originalInvoiceWithNegativeTaxAmount.PK;
				invoiceToTest4.ExchangeRate.Currency = "USD";
				invoiceToTest4.AH_InvoiceAmount = 1m;
				invoiceToTest4.AH_GSTAmount = 0;
				AssertEquals(1m, invoiceToTest4.AH_LocalTotal);
				testValidation = GetValidation(invoiceToTest4);
				testValidation.ValidateAH_LocalTotalAmount();
				AssertHasWarning("Total amount is 5 and new invoice amount is positive, so expect warning", invoiceToTest4.AH_LocalTotalAmountInfo, expectedWarning);

				//test original invoice with positive tax 
				AccountingConfigurationRegistry.Instance.AmendingTransactionLocalTotalBehaviorForAP.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				var originalInvoiceWithPositiveTaxAmount = Factory.NewWithValidTestData<APInvoice>();
				originalInvoiceWithPositiveTaxAmount.AH_InvoiceAmount = 10m;
				originalInvoiceWithPositiveTaxAmount.AH_GSTAmount = 0;
				AssertEquals(10m, originalInvoiceWithPositiveTaxAmount.AH_LocalTotal);

				var invoiceToTest5 = Factory.NewWithValidTestData<APCreditNote>();
				invoiceToTest5.OriginalTransactionReference = originalInvoiceWithNegativeTaxAmount.PK;
				invoiceToTest5.ExchangeRate.Currency = "USD";
				invoiceToTest5.AH_InvoiceAmount = -5m;
				invoiceToTest5.AH_GSTAmount = 0;
				AssertEquals(-5m, invoiceToTest5.AH_LocalTotal);
				testValidation = GetValidation(invoiceToTest5);
				testValidation.ValidateAH_LocalTotalAmount();
				AssertNoError("New invoice amount is negative, so expect no error", invoiceToTest5.AH_LocalTotalAmountInfo, expectedError);

				var invoiceToTest6 = Factory.NewWithValidTestData<APCreditNote>();
				invoiceToTest6.OriginalTransactionReference = originalInvoiceWithNegativeTaxAmount.PK;
				invoiceToTest6.ExchangeRate.Currency = "USD";
				invoiceToTest6.AH_InvoiceAmount = 2m;
				invoiceToTest6.AH_GSTAmount = 0;
				AssertEquals(2m, invoiceToTest6.AH_LocalTotal);
				testValidation = GetValidation(invoiceToTest6);
				testValidation.ValidateAH_LocalTotalAmount();
				AssertHasError("Total amount is positive, so expect error", invoiceToTest6.AH_LocalTotalAmountInfo, expectedError);
			}
		}

		public override void TestCheckAH_LocalTaxAmount_DetectsTaxCreditedExceedInvoiced()
		{
			var originalInvoiceWithNegativeTaxAmount = (InvoicingBase)Factory.NewWithValidTestData<APInvoice>();
			originalInvoiceWithNegativeTaxAmount.AH_GSTAmount = -10m;

			var expectedWarning = "Posting this transaction will result in the Payables Organization being debited more Tax than has been invoiced to your Payables Organization in the related transaction/s. Do you want to continue posting?";

			if (originalInvoiceWithNegativeTaxAmount is IAmending amending)
			{
				//test original invoice with negative tax.
				var invoiceToTest1 = Factory.NewWithValidTestData<APCreditNote>();
				invoiceToTest1.OriginalTransactionReference = originalInvoiceWithNegativeTaxAmount.PK;
				invoiceToTest1.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				invoiceToTest1.AH_TransactionBelongsToGroup = originalInvoiceWithNegativeTaxAmount.PK;
				invoiceToTest1.ExchangeRate.Currency = "USD";
				invoiceToTest1.AH_GSTAmount = -5m;

				var testValidation = GetValidation(invoiceToTest1);
				testValidation.ValidateAH_LocalTaxAmount();
				AssertNoWarnings("Total tax is -15, so expect no warning", invoiceToTest1.AH_LocalTaxAmountInfo);

				var invoiceToTest2 = Factory.NewWithValidTestData<APCreditNote>();
				invoiceToTest2.OriginalTransactionReference = originalInvoiceWithNegativeTaxAmount.PK;
				invoiceToTest2.ExchangeRate.Currency = "USD";
				invoiceToTest2.AH_GSTAmount = 20m;
				testValidation = GetValidation(invoiceToTest2);
				testValidation.ValidateAH_LocalTaxAmount();
				AssertHasWarning("Total tax is 5 so expect warning", invoiceToTest2.AH_LocalTaxAmountInfo, expectedWarning);

				var invoiceToTest3 = Factory.NewWithValidTestData<APCreditNote>();
				invoiceToTest3.OriginalTransactionReference = originalInvoiceWithNegativeTaxAmount.PK;
				invoiceToTest3.ExchangeRate.Currency = "USD";
				invoiceToTest3.AH_GSTAmount = -1m;
				testValidation = GetValidation(invoiceToTest3);
				testValidation.ValidateAH_LocalTaxAmount();
				AssertNoWarnings("Although total is 4, but since new invoice tax amount is negative, i.e. -1, so expect no warning", invoiceToTest3.AH_LocalTaxAmountInfo);

				var invoiceToTest4 = Factory.NewWithValidTestData<APCreditNote>();
				invoiceToTest4.OriginalTransactionReference = originalInvoiceWithNegativeTaxAmount.PK;
				invoiceToTest4.ExchangeRate.Currency = "USD";
				invoiceToTest4.AH_GSTAmount = 1m;
				testValidation = GetValidation(invoiceToTest4);
				testValidation.ValidateAH_LocalTaxAmount();
				AssertHasWarning("Total tax is 5 and new invoice tax amount is positive, i.e. 1, so expect warning", invoiceToTest4.AH_LocalTaxAmountInfo, expectedWarning);

				//test original invoice with positive tax.
				var originalInvoiceWithPositiveTaxAmount = Factory.NewWithValidTestData<APInvoice>();
				originalInvoiceWithPositiveTaxAmount.AH_GSTAmount = 10m;
				amending = originalInvoiceWithPositiveTaxAmount;

				var invoiceToTest5 = Factory.NewWithValidTestData<APCreditNote>();
				invoiceToTest5.OriginalTransactionReference = originalInvoiceWithPositiveTaxAmount.PK;
				invoiceToTest5.ExchangeRate.Currency = "USD";
				invoiceToTest5.AH_GSTAmount = -5m;
				testValidation = GetValidation(invoiceToTest5);
				testValidation.ValidateAH_LocalTaxAmount();
				AssertNoWarnings("new invoice tax is negative, i.e. -5, so expect no warning", invoiceToTest5.AH_LocalTaxAmountInfo);

				var invoiceToTest6 = Factory.NewWithValidTestData<APCreditNote>();
				invoiceToTest6.OriginalTransactionReference = originalInvoiceWithPositiveTaxAmount.PK;
				invoiceToTest6.ExchangeRate.Currency = "USD";
				invoiceToTest6.AH_GSTAmount = 2m;
				testValidation = GetValidation(invoiceToTest6);
				testValidation.ValidateAH_LocalTaxAmount();
				AssertNoWarnings("Total amount of tax amends is still negative, expect no warning", invoiceToTest6.AH_LocalTaxAmountInfo);

				var invoiceToTest7 = Factory.NewWithValidTestData<APCreditNote>();
				invoiceToTest7.OriginalTransactionReference = originalInvoiceWithPositiveTaxAmount.PK;
				invoiceToTest7.ExchangeRate.Currency = "USD";
				invoiceToTest7.AH_GSTAmount = 4m;
				testValidation = GetValidation(invoiceToTest7);
				testValidation.ValidateAH_LocalTaxAmount();
				AssertHasWarning("Total amount of tax amends is positive, i.e. 1, so expect warning", invoiceToTest7.AH_LocalTaxAmountInfo, expectedWarning);

				AccountingConfigurationRegistry.Instance.AmendingTransactionTaxBehaviorForAP.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				originalInvoiceWithNegativeTaxAmount = Factory.NewWithValidTestData<APInvoice>();
				originalInvoiceWithNegativeTaxAmount.AH_GSTAmount = -10m;

				var expectedError = "This amending transaction cannot be posted. Posting this transaction would result in the Payables Organization being debited more tax than they have been invoiced in the related transaction/s. Please review the charges you are attempting to debit.";
				var invoiceToTest8 = Factory.NewWithValidTestData<APCreditNote>();
				invoiceToTest8.OriginalTransactionReference = originalInvoiceWithNegativeTaxAmount.PK;
				invoiceToTest8.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				invoiceToTest8.AH_TransactionBelongsToGroup = originalInvoiceWithNegativeTaxAmount.PK;
				invoiceToTest8.ExchangeRate.Currency = "USD";
				invoiceToTest8.AH_GSTAmount = -5m;

				testValidation = GetValidation(invoiceToTest8);
				testValidation.ValidateAH_LocalTaxAmount();
				AssertNoErrors("Total tax is -15, so expect no error", invoiceToTest1.AH_LocalTaxAmountInfo);

				var invoiceToTest9 = Factory.NewWithValidTestData<APCreditNote>();
				invoiceToTest9.OriginalTransactionReference = originalInvoiceWithNegativeTaxAmount.PK;
				invoiceToTest9.ExchangeRate.Currency = "USD";
				invoiceToTest9.AH_GSTAmount = 20m;
				testValidation = GetValidation(invoiceToTest9);
				testValidation.ValidateAH_LocalTaxAmount();
				AssertHasError("Total tax is 5 so expect error", invoiceToTest9.AH_LocalTaxAmountInfo, expectedError);
			}
		}

		protected override InvoiceBaseValidation GetValidation(TransactionHeader parent)
		{
			return parent.Validation as APCreditNoteValidation;
		}

		protected override void AssertCreditOnHoldError(ZPropertyInfo aH_OHInfo)
		{
			AssertNoErrors("There should be no Credit On Hold errors for AP Transactions", aH_OHInfo);
		}

		protected override BooleanRegistryItem OriginalInvoiceDetailsMandatoryRegistryItem => AccountingConfigurationRegistry.Instance.OriginalInvoiceDetailsMandatoryOnAPCreditNotes;
		protected override Type OriginalInvoiceType => typeof(APInvoice);

		protected override InvoicingBase GetCorrectOriginalInvoice(OrgHeader header) => TestObjectCreator.CreateInvoice(typeof(APInvoice), organisation: header);

		protected override InvoicingBase GetWrongOrgOriginalInvoice(OrgHeader header) => TestObjectCreator.CreateInvoice(typeof(APInvoice), organisation: header);

		protected override InvoicingBase GetWrongLedgerOriginalInvoice(OrgHeader header) => TestObjectCreator.CreateInvoice(typeof(ARInvoice), organisation: header);
	}
}
