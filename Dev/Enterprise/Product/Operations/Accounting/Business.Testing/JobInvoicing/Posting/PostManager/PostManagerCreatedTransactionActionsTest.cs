using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class PostManagerCreatedTransactionActionsTest : TransactionCreatorBaseTest
	{
		#region TestPerformInvoiceExchangeRateCheck

		[TestDate(2021, 11, 04)]
		public void TestPerformInvoiceExchangeRateCheck_NonZeroExchangeRate_AR()
		{
			using (AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.Default.Code))
			{
				var notifier = new UserNotifierForTest();
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1000", TestObjectCreator.USD, 0.9m, TestObjectCreator.AALSHI);
				var transactions = new TransactionCreatorHashtable();
				transactions.AddARInvoice(invoice);
				AssertNotEquals("Precondition: non-zero exchange rate", 0.0m, invoice.AH_ExchangeRate);

				var obj = ObjectFactory.Get<IPostManagerCreatedTransactionActions>();
				var result = obj.PerformInvoiceExchangeRateCheck(transactions, notifier);

				AssertInvoiceExchangeRateCheckForSuccess(result, notifier);
			}
		}

		[TestDate(2021, 11, 04)]
		public void TestPerformInvoiceExchangeRateCheck_NonZeroExchangeRate_AP()
		{
			using (AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.Default.Code))
			{
				var notifier = new UserNotifierForTest();
				var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("1000", TestObjectCreator.USD, 0.9m, 10m, 0m, 0m, 9m, 0m, 0m, TestObjectCreator.AALSHI);
				var transactions = new TransactionCreatorHashtable();
				transactions.AddARInvoice(invoice);
				AssertNotEquals("Precondition: non-zero exchange rate", 0.0m, invoice.AH_ExchangeRate);

				var obj = ObjectFactory.Get<IPostManagerCreatedTransactionActions>();
				var result = obj.PerformInvoiceExchangeRateCheck(transactions, notifier);

				AssertInvoiceExchangeRateCheckForSuccess(result, notifier);
			}
		}

		static void AssertInvoiceExchangeRateCheckForSuccess(bool result, UserNotifierForTest notifier)
		{
			AssertEquals("When a positive AH_ExchangeRate is set, result is successful", true, result);
			AssertNullOrEmpty("No errors", notifier.LastError);
			AssertNullOrEmpty("No warnings", notifier.LastWarning);
		}

		[TestDate(2021, 11, 04)]
		public void TestPerformInvoiceExchangeRateCheck_ZeroExchangeRate_AP_DEF()
			=> TestPerformInvoiceExchangeRateCheckForFailureAP(AccountingConstants.InvoicePostingExchangeRateOption.Default.Code);

		[TestDate(2021, 11, 04)]
		public void TestPerformInvoiceExchangeRateCheck_ZeroExchangeRate_AP_TOD()
			=> TestPerformInvoiceExchangeRateCheckForFailureAP(AccountingConstants.InvoicePostingExchangeRateOption.TodayExchangeRate.Code);

		[TestDate(2021, 11, 04)]
		public void TestPerformInvoiceExchangeRateCheck_ZeroExchangeRate_AP_PST()
			=> TestPerformInvoiceExchangeRateCheckForFailureAP(AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code);

		[TestDate(2021, 11, 04)]
		public void TestPerformInvoiceExchangeRateCheck_ZeroExchangeRate_AP_INV()
			=> TestPerformInvoiceExchangeRateCheckForFailureAP(AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code);

		void TestPerformInvoiceExchangeRateCheckForFailureAP(string registryValue)
		{
			using (AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var notifier = new UserNotifierForTest();
				var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("1000", TestObjectCreator.USD, 0.0m, 10m, 0m, 0m, 0m, 0m, 0m, TestObjectCreator.AALSHI);
				invoice.AH_ExchangeRate = 0.0m;
				var transactions = new TransactionCreatorHashtable();
				transactions.AddAPInvoice(invoice, TestObjectCreator.AALSHI.OH_Code, "1000");
				AssertEquals("Precondition: zero exchange rate", 0.0m, invoice.AH_ExchangeRate);

				var obj = ObjectFactory.Get<IPostManagerCreatedTransactionActions>();
				var result = obj.PerformInvoiceExchangeRateCheck(transactions, notifier);

				AssertInvoiceExchangeRateCheckForFailure(result, notifier, LedgerTypes.AccountsPayable);
			}
		}

		[TestDate(2021, 11, 04)]
		public void TestPerformInvoiceExchangeRateCheck_ZeroExchangeRate_AR_DEF()
			=> TestPerformInvoiceExchangeRateCheckForFailureAR(AccountingConstants.InvoicePostingExchangeRateOption.Default.Code);

		[TestDate(2021, 11, 04)]
		public void TestPerformInvoiceExchangeRateCheck_ZeroExchangeRate_AR_TOD()
			=> TestPerformInvoiceExchangeRateCheckForFailureAR(AccountingConstants.InvoicePostingExchangeRateOption.TodayExchangeRate.Code);

		[TestDate(2021, 11, 04)]
		public void TestPerformInvoiceExchangeRateCheck_ZeroExchangeRate_AR_PST()
			=> TestPerformInvoiceExchangeRateCheckForFailureAR(AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code);

		[TestDate(2021, 11, 04)]
		public void TestPerformInvoiceExchangeRateCheck_ZeroExchangeRate_AR_INV()
			=> TestPerformInvoiceExchangeRateCheckForFailureAR(AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code);

		void TestPerformInvoiceExchangeRateCheckForFailureAR(string registryValue)
		{
			using (AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var notifier = new UserNotifierForTest();
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1000", TestObjectCreator.USD, 0.0m, TestObjectCreator.AALSHI);
				invoice.AH_ExchangeRate = 0.0m;
				var transactions = new TransactionCreatorHashtable();
				transactions.AddARInvoice(invoice);
				AssertEquals("Precondition: zero exchange rate", 0.0m, invoice.AH_ExchangeRate);

				var obj = ObjectFactory.Get<IPostManagerCreatedTransactionActions>();
				var result = obj.PerformInvoiceExchangeRateCheck(transactions, notifier);

				AssertInvoiceExchangeRateCheckForFailure(result, notifier, LedgerTypes.AccountsReceivable);
			}
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestPerformCashAdvanceRelatedCheck_AR()
		{
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0M, TestObjectCreator.Agent, 1.0M);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 200M, 200M);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
			var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, 400M, 400M);
			var charge4 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC4, 150M, 150M);

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = true;
			charge3.JR_IsARCashAdvance = true;
			charge4.JR_IsARCashAdvance = true;

			var (cah1, cal1) = CreateCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Requested, "AUD");
			var cal2 = TestObjectCreator.CreateCashAdvanceRequestLine(cah1, 300M, 300M);
			charge2.JR_CAL_ARLine = cal2.PK;
			var (cah3, cal3) = CreateCashAdvanceRequest(charge3, 400M, 400M, CashAdvanceStatusCodes.RequestLine.Requested, "AUD");
			var (cah4, cal4) = CreateCashAdvanceRequest(charge4, 150M, 150M, CashAdvanceStatusCodes.RequestLine.Paid, "EUR");
			cal4.CAL_LocalPaidAmount = 150M;
			cal4.CAL_OSPaidAmount = 150M;
			Factory.Save();

			var arinvoice = GetInvoice(charge1, charge3, charge4);
			var notifier = new UserNotifierForTest();
			var transactions = new TransactionCreatorHashtable();
			transactions.AddARInvoice(arinvoice);

			var expepctedFullMessage = FormattableString.Invariant($@"One or more charges have a paid Advance Payment in a different currency to the currency of the invoice being posted. Unable to apply Advance Payment to the invoice.
If the Advance Payment was incorrectly created with the wrong currency, please cancel and recreate the Advance Payment in the correct currency.
***Important:
If the currency entered on this invoice is incorrect, the following steps must be taken to post the invoice:
1. Change the currency on this invoice.
2. Go to Actions and Save as Incomplete.
3. Close the invoice and go to the Payables > Incomplete Invoices module.
4. Open the invoice and post.
Note: If you do not change the invoice currency before saving as incomplete, the invoice cannot be posted from the Incomplete Invoices module. It will need to be canceled, and the invoice re-entered.
{TestObjectCreator.Debtor.OH_Code}-{TestObjectCreator.Debtor.OH_FullName} | {TestObjectCreator.CC4.AC_Code} | {TestObjectCreator.CC4.AC_Desc} | AUD | 150.0 | {cah4.CAH_RequestReferenceNumber}

One or more charges have an unpaid Advance Payment. However, not all charges linked to the same Advance Payment are being posted. Please post all charges that relate to a single Advance Payment. Otherwise, cancel the request in order to post this charge.
{TestObjectCreator.Debtor.OH_Code}-{TestObjectCreator.Debtor.OH_FullName} | {TestObjectCreator.CC2.AC_Code} | {TestObjectCreator.CC2.AC_Desc} | AUD | 300 | {cah1.CAH_RequestReferenceNumber}
");

			foreach (var regValueWithExpectedErrorMessage in new (bool RegValue, string ErrorMessage)[] { (false, null), (true, expepctedFullMessage) })
			{
				AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regValueWithExpectedErrorMessage.RegValue);

				var obj = ObjectFactory.Get<IPostManagerCreatedTransactionActions>();
				var result = obj.PerformCashAdvanceRelatedCheck(transactions, notifier);
				AssertEquals("Validation should fail", !regValueWithExpectedErrorMessage.RegValue, result);
				AssertMultilineASCIIEquals("User is notified of error", regValueWithExpectedErrorMessage.ErrorMessage, notifier.LastError);
				AssertNullOrEmpty("No warnings", notifier.LastWarning);
			}

			(AccCashAdvanceRequestHeader cah, AccCashAdvanceRequestLine cal) CreateCashAdvanceRequest(Charge charge, ZDecimal localAmount, ZDecimal osAmount, ZString status, ZString currency)
			{
				var cah = TestObjectCreator.CreateCashAdvanceRequestHeader(charge.Job as Job, TestObjectCreator.Debtor, LedgerTypes.AccountsReceivable, localAmount, osAmount, currency);
				var cal = TestObjectCreator.CreateCashAdvanceRequestLine(cah, localAmount, osAmount);
				charge.JR_CAL_ARLine = cal.PK;
				cal.CAL_Status = status;
				return (cah, cal);
			}

			ARInvoice GetInvoice(params Charge[] charges)
			{
				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), currency: TestObjectCreator.AUD, organisation: TestObjectCreator.Debtor) as ARInvoice;
				foreach (var charge in charges)
				{
					var line = TestObjectCreator.CreateARInvoiceLine(invoice, charge.Job as Job, charge.ChargeCode, charge.SellCurrency, 1.0M, "DESC01", charge.JR_OSSellAmt);
					charge.WIP.AL_ReverseDate = ZDateTime.Today;
					charge.JR_AL_ARLine = line.PK;
				}
				return invoice;
			}
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestPerformCashAdvanceRelatedCheck_AP()
		{
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0M, TestObjectCreator.Agent, 1.0M);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 200M, 200M);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, 400M, 400M);
			var charge4 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC4, 150M, 150M);

			charge1.JR_IsAPCashAdvance = true;
			charge2.JR_IsAPCashAdvance = true;
			charge3.JR_IsAPCashAdvance = true;
			charge4.JR_IsAPCashAdvance = true;

			var (cah1, cal1) = CreateCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Requested, "AUD");
			var cal2 = TestObjectCreator.CreateCashAdvanceRequestLine(cah1, 300M, 300M);
			charge2.JR_CAL_APLine = cal2.PK;
			var (cah3, cal3) = CreateCashAdvanceRequest(charge3, 400M, 400M, CashAdvanceStatusCodes.RequestLine.Requested, "AUD");
			var (cah4, cal4) = CreateCashAdvanceRequest(charge4, 150M, 150M, CashAdvanceStatusCodes.RequestLine.Paid, "EUR");
			cal4.CAL_LocalPaidAmount = 150M;
			cal4.CAL_OSPaidAmount = 150M;
			Factory.Save();

			var apinvoice = GetInvoice(charge1, charge3, charge4);
			var notifier = new UserNotifierForTest();
			var transactions = new TransactionCreatorHashtable();
			transactions.AddAPInvoice(apinvoice, Creditor1.OH_Code, TestObjectCreator.GetRandomString(8));

			var expepctedFullMessage = FormattableString.Invariant($@"One or more charges have a paid Advance Payment in a different currency to the currency of the invoice being posted. Unable to apply Advance Payment to the invoice.
If the Advance Payment was incorrectly created with the wrong currency, please cancel and recreate the Advance Payment in the correct currency.
***Important:
If the currency entered on this invoice is incorrect, the following steps must be taken to post the invoice:
1. Change the currency on this invoice.
2. Go to Actions and Save as Incomplete.
3. Close the invoice and go to the Payables > Incomplete Invoices module.
4. Open the invoice and post.
Note: If you do not change the invoice currency before saving as incomplete, the invoice cannot be posted from the Incomplete Invoices module. It will need to be canceled, and the invoice re-entered.
{TestObjectCreator.Creditor1.OH_Code}-{TestObjectCreator.Creditor1.OH_FullName} | {TestObjectCreator.CC4.AC_Code} | {TestObjectCreator.CC4.AC_Desc} | AUD | 150 | {cah4.CAH_RequestReferenceNumber}

One or more accruals have an unpaid AP Advance Payment Request, however not all accruals linked to the same Advance Payment Request are being posted.  
If all costs that relate to a single Advance Payment Request are not included in the invoice, the Advance Payment Request must be canceled before the invoice can be posted.
{TestObjectCreator.Creditor1.OH_Code}-{TestObjectCreator.Creditor1.OH_FullName} | {TestObjectCreator.CC2.AC_Code} | {TestObjectCreator.CC2.AC_Desc} | AUD | 300 | {cah1.CAH_RequestReferenceNumber}
");

			foreach (var regValueWithExpectedErrorMessage in new (bool RegValue, string ErrorMessage)[] { (false, null), (true, expepctedFullMessage) })
			{
				AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regValueWithExpectedErrorMessage.RegValue);

				var obj = ObjectFactory.Get<IPostManagerCreatedTransactionActions>();
				var result = obj.PerformCashAdvanceRelatedCheck(transactions, notifier);
				AssertEquals("Validation should fail", !regValueWithExpectedErrorMessage.RegValue, result);
				AssertMultilineASCIIEquals("User is notified of error", regValueWithExpectedErrorMessage.ErrorMessage, notifier.LastError);
				AssertNullOrEmpty("No warnings", notifier.LastWarning);
			}

			(AccCashAdvanceRequestHeader cah, AccCashAdvanceRequestLine cal) CreateCashAdvanceRequest(Charge charge, ZDecimal localAmount, ZDecimal osAmount, ZString status, ZString currency)
			{
				var cah = TestObjectCreator.CreateCashAdvanceRequestHeader(charge.Job as Job, TestObjectCreator.Debtor, LedgerTypes.AccountsPayable, localAmount, osAmount, currency);
				var cal = TestObjectCreator.CreateCashAdvanceRequestLine(cah, localAmount, osAmount);
				charge.JR_CAL_APLine = cal.PK;
				cal.CAL_Status = status;
				return (cah, cal);
			}

			APInvoice GetInvoice(params Charge[] charges)
			{
				var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), currency: TestObjectCreator.AUD, organisation: TestObjectCreator.Creditor1) as APInvoice;
				foreach (var charge in charges)
				{
					var line = TestObjectCreator.CreateAPInvoiceLine(invoice, charge.Job as Job, charge.ChargeCode, charge.CostCurrency, 1.0M, "DESC01", charge.JR_OSCostAmt);
					charge.Accrual.AL_ReverseDate = ZDateTime.Today;
					charge.JR_AL_APLine = line.PK;
				}
				return invoice;
			}
		}

		static void AssertInvoiceExchangeRateCheckForFailure(bool result, UserNotifierForTest notifier, string ledger)
		{
			AssertEquals("When a zero AH_ExchangeRate is set, result is failure", false, result);
			AssertEquals("User is notified of error", $@"Invoice was created with zero exchange rate. Exchange rate must be greater than 0.
Invoice currency: USD, debtor / creditor: AALSHI, ledger: {ledger}, post date: 04-Nov-21 00:00:00, invoice date 04-Nov-21 00:00:00.", notifier.LastError);
			AssertNullOrEmpty("No warnings", notifier.LastWarning);
		}

		#endregion

		#region PerformExporterExemptionCheck

		[TestDate(2021, 11, 04)]
		public void TestPerformExporterExemptionCheck_ExceedingCeilingLimit()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			using (AccountingMasterFilesRegistry.Instance.ValidateTaxIDApplicationForExporterExemptionItaly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				SetupDataForExporterExemption();

				var notifier = new UserNotifierForTest();
				var invoice = (InvoicingBase)Factory.New(typeof(ARInvoice));
				var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.EUR, 1M, 1400M, 70M, 0M);
				line.AL_AT = dichIntTaxRate.PK;
				invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
				invoice.AH_OH = TestObjectCreator.Debtor.PK;

				var transactions = new TransactionCreatorHashtable();
				transactions.AddARInvoice(invoice);

				var obj = ObjectFactory.Get<IPostManagerCreatedTransactionActions>();
				var result = obj.PerformExporterExemptionCheck(transactions, notifier);

				AssertEquals("Error is raised on Ceiling Limit exceeding", false, result);
				AssertEquals(@"There are errors that need to be corrected before saving the current Accounts Receivable Invoice.
TAX ID: DICH.INT, based on the Registry [Accounting -> Receivable Defaults -> Default Settings -> Validate Tax ID Application for Exporter Exemption (Italy)] it is not possible to proceed with the post because you are posting a transaction on a debtor that has one or more EXV-VAT/GST Exporter Exemption document with CEILING LIMIT.
The sum of the transactions that contain DICH.INT Tax ID including this one you are posting, exceeds the CEILING LIMIT by EUR 100.00.
To proceed with the post please fix Tax ID Code or save a new EXV-VAT/GST Exporter Exemption in the Debtor Organization eDocs, with an higher CEILING LIMIT or disable the Registry.", notifier.LastError);
			}
		}

		[TestDate(2022, 11, 10)]
		public void TestPerformExporterExemptionCheck_EXVDocumentNotValidOrNotFound()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			using (AccountingMasterFilesRegistry.Instance.ValidateTaxIDApplicationForExporterExemptionItaly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				SetupDataForExporterExemption(expired: true);

				var notifier = new UserNotifierForTest();
				var invoice = (InvoicingBase)Factory.New(typeof(ARInvoice));
				var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.EUR, 1M, 1400M, 70M, 0M);
				line.AL_AT = dichIntTaxRate.PK;
				invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
				invoice.AH_OH = TestObjectCreator.Debtor.PK;

				var transactions = new TransactionCreatorHashtable();
				transactions.AddARInvoice(invoice);

				var obj = ObjectFactory.Get<IPostManagerCreatedTransactionActions>();
				var result = obj.PerformExporterExemptionCheck(transactions, notifier);

				AssertEquals("Error is raised for EXV Document non valid or not found", false, result);
				AssertEquals("If [Accounting -> Receivable Defaults -> Default Settings -> Validate Tax ID Application for Exporter Exemption (Italy)] is set to Yes, the DICH.INT Tax ID can only be used for a Debtor with a valid Exporter Exemption Certificate, where the certificate Ceiling Limit has not been exceeded.", notifier.LastError);
			}
		}

		void SetupDataForExporterExemption(bool expired = false)
		{
			var stampDutyRecharge = new StampDutyRecharge
			{
				StampDutyRechargeOrganizationType = StampDutyRechargeOrganizationType.NotRecharging,
				StampDutyRechargeTransactionType = StampDutyRechargeTransactionType.All
			};
			AccountingConfigurationRegistry.Instance.StampDutyRecharge.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, stampDutyRecharge);

			var query = new ZQuery(AccTaxRateSchema.AT_IsActive, true);
			query.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, CountryCodes.Italy);
			query.AddToFilter(AccTaxRateSchema.AT_Code, "DICH.INT");
			dichIntTaxRate = Factory.LoadTop1<AccTaxRate>(query);

			var dichInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV002", TestObjectCreator.EUR, 1M, TestObjectCreator.Debtor);
			var line = TestObjectCreator.CreateInvoiceLine(dichInvoice, TestObjectCreator.EUR, 1M, 700M, 70M, 0M);
			line.AL_AT = dichIntTaxRate.PK;

			var dichInvoice1 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV003", TestObjectCreator.EUR, 1M, TestObjectCreator.Creditor1);
			var line1 = TestObjectCreator.CreateInvoiceLine(dichInvoice1, TestObjectCreator.EUR, 1M, 700M, 70M, 0M);
			line1.AL_AT = dichIntTaxRate.PK;

			var dateReceived1 = expired ? ZDate.Today.AddDays(-30) : ZDate.Today.AddDays(-1);
			var validToDate1 = expired ? ZDate.Today.AddDays(-1) : ZDate.Today.AddDays(30);
			var validToDate2 = expired ? ZDate.Today.AddDays(-1) : ZDate.Today.AddDays(29);

			CreateEXVDocument(dichInvoice, JobRequiredDocument.DocUsage.Debtor, CountryCodes.Italy, "000001", dateReceived1, validToDate1, "1000");
			CreateEXVDocument(dichInvoice, JobRequiredDocument.DocUsage.Debtor, CountryCodes.Italy, "000002", dateReceived1, validToDate2, "1000");
			CreateEXVDocument(dichInvoice, JobRequiredDocument.DocUsage.Debtor, CountryCodes.UnitedStates, "000003", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(28));
			CreateEXVDocument(dichInvoice1, JobRequiredDocument.DocUsage.Creditor, CountryCodes.Italy, "000004", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(30), "1000");
			CreateEXVDocument(dichInvoice1, JobRequiredDocument.DocUsage.Creditor, CountryCodes.Italy, "000005", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(29), "1000");
			CreateEXVDocument(dichInvoice1, JobRequiredDocument.DocUsage.Creditor, CountryCodes.UnitedStates, "000006", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(28));

			Factory.Save();

			void CreateEXVDocument(InvoicingBase invoice, ZString docUsage, ZString country, ZString docNumber, ZDateTime dateReceived, ZDateTime validToDate, string ceilingLimit = null)
			{
				var doc = invoice.Header.RequiredDocuments.AddNew();
				doc.EQ_DocCategory = ReferenceTypes.ClientSupplierRelationship;
				doc.EQ_DocType = RefDocTypes.VATExporterExemption;
				doc.EQ_DocUsage = docUsage;
				doc.EQ_RN_NKRelatedCountry = country;
				doc.EQ_DocNumber = docNumber;
				doc.EQ_DateReceived = dateReceived.ToDateTimeOffset(null);
				doc.EQ_ValidToDate = validToDate;

				if (ceilingLimit != null)
				{
					var ceilingLimitAttribute = doc.Attributes.AddNew();
					ceilingLimitAttribute.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CeilingLimit;
					ceilingLimitAttribute.D0_AttribValue = ceilingLimit;
				}
			}
		}

		AccTaxRate dichIntTaxRate;

		#endregion

		#region PerformInvoicingTermsCheck

		public void TestPerformInvoicingTermsCheck_APInvoice()
		{
			// Arrange
			var creditor = TestObjectCreator.CreateOrgHeader("BADCRED", creditor: true, debtor: false);
			creditor.CompanyData.OB_APPaymentTerms = "BAD";

			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = creditor.PK;

			AssertEquals("Precondition", LedgerTypes.AccountsPayable, invoice.AH_Ledger);
			AssertEquals("Precondition: should have defaulted from creditor", "BAD", invoice.AH_InvoiceTerm);
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 100m, 0m, 0m);

			var obj = ObjectFactory.Get<IPostManagerCreatedTransactionActions>();
			var transactions = new TransactionCreatorHashtable();
			transactions.AddARInvoice(invoice);
			var notifier = new UserNotifierForTest();

			// Act
			var result = obj.PerformInvoicingTermsCheck(transactions, notifier);

			// Assert
			AssertEquals("Error is raised due to invalid Invoice Terms", false, result);
			AssertEquals("AP Invoice cannot be created as the defaulted Invoice Term BAD is invalid. Please check the Invoice Terms on the debtor / creditor: ZBADCRED.", notifier.LastError);
		}

		public void TestPerformInvoicingTermsCheck_ARInvoice()
		{
			// Arrange
			var debtor = TestObjectCreator.CreateOrgHeader("BADDEBT", creditor: false, debtor: true);
			debtor.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = "BAD";

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = debtor.PK;

			AssertEquals("Precondition", LedgerTypes.AccountsReceivable, invoice.AH_Ledger);
			AssertEquals("Precondition: should have defaulted from debtor", "BAD", invoice.AH_InvoiceTerm);
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 100m, 0m, 0m);

			var postManagerCreatedTransactionActions = ObjectFactory.Get<IPostManagerCreatedTransactionActions>();
			var transactions = new TransactionCreatorHashtable();
			transactions.AddARInvoice(invoice);
			var notifier = new UserNotifierForTest();

			// Act
			var result = postManagerCreatedTransactionActions.PerformInvoicingTermsCheck(transactions, notifier);

			// Assert
			AssertEquals("Error is raised due to invalid Invoice Terms", false, result);
			AssertEquals("AR Invoice cannot be created as the defaulted Invoice Term BAD is invalid. Please check the Invoice Terms on the debtor / creditor: ZBADDEBT.", notifier.LastError);
		}

		#endregion

		#region PerformDuplicateTransactionNumberCheckForPayableTransactions

		public void TestPerformDuplicateTransactionNumberCheckForPayableTransactions_APInvoice()
		{
			// Arrange
			var creditor = TestObjectCreator.LocalClient;

			var old_invoice = Factory.NewWithValidTestData<APInvoice>();
			old_invoice.AH_OH = creditor.PK;
			old_invoice.AH_TransactionNum = "INV001";
			old_invoice.AH_PostDate = ZDateTime.Today.AddMonths(-AccountingUtils.DuplicateInvoiceNumberPeriodMonths);
			old_invoice.AH_InvoiceDate = ZDateTime.Today.AddMonths(-AccountingUtils.DuplicateInvoiceNumberPeriodMonths);
			Factory.Save();

			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = creditor.PK;
			invoice.AH_TransactionNum = "INV001";
			invoice.AH_PostDate = ZDateTime.Today;

			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 100m, 0m, 0m);

			var obj = ObjectFactory.Get<IPostManagerCreatedTransactionActions>();
			var transactions = new TransactionCreatorHashtable();
			transactions.AddAPInvoice(invoice, TestObjectCreator.AALSHI.OH_Code, "INV001");
			var notifier = new UserNotifierForTest();

			// Act
			bool result = obj.PerformDuplicateTransactionNumberCheckForPayableTransactions(transactions, notifier);

			// Assert
			AssertEquals("Duplicate invoice numers are possible if they are 12 months apart", true, result);
			//AssertEquals("AP Invoice cannot be created as the defaulted Invoice Term BAD is invalid. Please check the Invoice Terms on the debtor / creditor: ZBADCRED.", notifier.LastError);
		}

		public void TestPerformDuplicateTransactionNumberCheckForPayableTransactions_APInvoice_Duplicates()
		{
			// Arrange
			var creditor = TestObjectCreator.LocalClient;

			var old_invoice = Factory.NewWithValidTestData<APInvoice>();
			old_invoice.AH_OH = creditor.PK;
			old_invoice.AH_TransactionNum = "INV001";
			old_invoice.AH_PostDate = ZDateTime.Today.AddMonths(-1);
			old_invoice.AH_InvoiceDate = ZDateTime.Today.AddMonths(-1);
			Factory.Save();

			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = creditor.PK;
			invoice.AH_TransactionNum = "INV001";
			old_invoice.AH_PostDate = ZDateTime.Today;

			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 100m, 0m, 0m);

			var obj = ObjectFactory.Get<IPostManagerCreatedTransactionActions>();
			var transactions = new TransactionCreatorHashtable();
			transactions.AddAPInvoice(invoice, TestObjectCreator.AALSHI.OH_Code, "INV001");
			var notifier = new UserNotifierForTest();

			// Act
			bool result = obj.PerformDuplicateTransactionNumberCheckForPayableTransactions(transactions, notifier);

			// Assert
			AssertEquals("Duplicate invoice numers are possible if they are 12 months apart", false, result);
		}

		#endregion

		class UserNotifierForTest : IPostManagerUserNotifier
		{
			public string LastWarning { get; private set; }
			public string LastError { get; private set; }

			public void NotifyPostingWarning(string warning)
			{
				LastWarning = warning;
			}

			public void NotifyPostValidationError(string error)
			{
				LastError = error;
			}
		}
	}
}
