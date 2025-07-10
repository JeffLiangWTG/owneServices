using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.DataTransfer.Universal.Netting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Web.Business;
using Enterprise.Accounting.Web.Exceptions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Web.Testing
{
	public class InvoicePaymentDetailsUpdaterTest : TestCaseWithFactory
	{
		#region Implementation

		void setupCommonData()
		{
			TestObjectCreator.AUDBankAccount.GLHeader.AG_AccountNum = "AUDAcc";
			TestObjectCreator.USDBankAccount.GLHeader.AG_AccountNum = "USDAcc";
			TestObjectCreator.GLHeader1.AG_AccountNum = "GLHeader1";
			TestObjectCreator.GLHeader2.AG_AccountNum = "GLHeader2";
		}

		void setupPeriods()
		{
			AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(Factory);
			helper.PostPeriodsForEntireYear(2008, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			helper.PostPeriodsForEntireYear(2009, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			helper.PostPeriodsForEntireYear(2010, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			helper.PostPeriodsForEntireYear(2011, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
		}

		void setupAccountingWebServiceUsernamePassword()
		{
			AccountingConfigurationRegistry.Instance.AccountingWebServiceUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "alex");
			AccountingConfigurationRegistry.Instance.AccountingWebServicePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "password");
		}

		TestObjectCreator TestObjectCreator { get; set; }

		System.Data.Common.DbConnection Connection { get; set; }
		System.Data.Common.DbTransaction Transaction { get; set; }

		protected override void SetUp()
		{
			base.SetUp();

			Connection = ((IDbConnectionInternals)base.TestConnection).ADOConnection;
			Transaction = ((IDbConnectionInternals)base.TestConnection).ADOTransaction;

			TestObjectCreator = new TestObjectCreator(Factory);

			setupPeriods();
			setupAccountingWebServiceUsernamePassword();
		}

		#endregion
		#region Transactions

		#region Invoices

		void createInvoices(bool isLocalCurrency)
		{
			var exchangeRate = isLocalCurrency ? 1m : 0.5m;
			var currency = isLocalCurrency ? TestObjectCreator.AUD : TestObjectCreator.USD;
			var currencyBankAccountPK = isLocalCurrency ? TestObjectCreator.AUDBankAccount.PK : TestObjectCreator.USDBankAccount.PK;

			APInvoice apInvoicePositive = TestObjectCreator.CreateAPInvoice<APInvoice>("111", currency, exchangeRate, 100m * exchangeRate, 10m * exchangeRate, 0m, 100, 10m, 0m, TestObjectCreator.AALSHI);
			apInvoicePositivePK = apInvoicePositive.PK;
			apInvoicePositive.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			apInvoicePositive.AH_PostDate = new ZDateTime(2008, 06, 15);
			TestObjectCreator.CreateAPInvoiceLine(apInvoicePositive, null, TestObjectCreator.CC1, currency, exchangeRate, "Desc", -10m * exchangeRate);
			apInvoicePositive.Lines[1].AL_AC = TestObjectCreator.CC1.PK;
			Factory.Save();

			APInvoice apInvoiceNegative = TestObjectCreator.CreateAPInvoice<APInvoice>("112", currency, exchangeRate, -100m * exchangeRate, -10m * exchangeRate, 0m, -100m, -10m, 0m, TestObjectCreator.AALSHI);
			apInvoiceNegativePK = apInvoiceNegative.PK;
			apInvoiceNegative.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			apInvoiceNegative.AH_PostDate = new ZDateTime(2008, 12, 15);
			TestObjectCreator.CreateAPInvoiceLine(apInvoiceNegative, null, TestObjectCreator.CC1, currency, exchangeRate, "Desc", 10m * exchangeRate);
			apInvoiceNegative.Lines[1].AL_AC = TestObjectCreator.CC1.PK;
			Factory.Save();

			ARInvoice arInvoicePositive = TestObjectCreator.CreateARInvoice<ARInvoice>("002", currency, exchangeRate, TestObjectCreator.ABIGAS);
			arInvoicePositivePK = arInvoicePositive.PK;
			arInvoicePositive.AH_PostDate = new ZDateTime(2009, 06, 15);
			InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(arInvoicePositive, currency, exchangeRate, 100m * exchangeRate, 10m * exchangeRate, 0m);
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			InvoicingLineBase line2 = TestObjectCreator.CreateInvoiceLine(arInvoicePositive, currency, exchangeRate, -50m * exchangeRate, -5m * exchangeRate, 0m);
			line2.AL_AC = TestObjectCreator.CC1.PK;
			line2.AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			Factory.Save();

			ARInvoice arInvoiceNegative = TestObjectCreator.CreateARInvoice<ARInvoice>("002", currency, exchangeRate, TestObjectCreator.ABIGAS);
			arInvoiceNegativePK = arInvoiceNegative.PK;
			arInvoiceNegative.AH_PostDate = new ZDateTime(2009, 12, 15);
			line = TestObjectCreator.CreateInvoiceLine(arInvoiceNegative, currency, exchangeRate, -100m * exchangeRate, -10m * exchangeRate, 0m);
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			line2 = TestObjectCreator.CreateInvoiceLine(arInvoiceNegative, currency, exchangeRate, 50m * exchangeRate, 5m * exchangeRate, 0m);
			line2.AL_AC = TestObjectCreator.CC1.PK;
			line2.AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			Factory.Save();

			ARInvoice arInvoiceMatched = TestObjectCreator.CreateARInvoice<ARInvoice>("003", currency, exchangeRate, TestObjectCreator.ABIGAS);
			arInvoiceMatched.AH_PostDate = new ZDateTime(2009, 06, 15);
			line = TestObjectCreator.CreateInvoiceLine(arInvoiceMatched, currency, exchangeRate, 50m * exchangeRate, 0m, 0m);
			line.AL_AC = TestObjectCreator.CC1.PK;

			ARPayment arPaymentMatched = TestObjectCreator.CreateARPayment(exchangeRate, 20m * exchangeRate, arInvoiceMatched.AH_PostDate, arInvoiceMatched.AH_PostDate, TestObjectCreator.ABIGAS.PK, currencyBankAccountPK);
			Factory.Save();

			TransactionMatchLinkGroup matchGroup = new TransactionMatchLinkGroup(Factory);
			AccTransactionMatchLink invoiceLink = matchGroup.AddNew();
			invoiceLink.AP_AH = arInvoiceMatched.PK;
			invoiceLink.AP_Amount = -20m;

			AccTransactionMatchLink paymentLink = matchGroup.AddNew();
			paymentLink.AP_AH = arPaymentMatched.PK;
			paymentLink.AP_Amount = 20m;

			matchGroup.SetMatchGroupNumberAndMatchDate("M00001", arInvoiceMatched.AH_PostDate);
			Factory.Save();
		}

		#endregion

		#region Credit Notes

		void createCreditNotes(bool isLocalCurrency)
		{
			var exchangeRate = isLocalCurrency ? 1m : 0.5m;
			var currency = isLocalCurrency ? TestObjectCreator.AUD : TestObjectCreator.USD;
			var currencyBankAccountPK = isLocalCurrency ? TestObjectCreator.AUDBankAccount.PK : TestObjectCreator.USDBankAccount.PK;

			ZDateTime date = new ZDateTime(2008, 6, 15);
			Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00001001"));
			APCreditNote apCreditNotePositive = TestObjectCreator.CreateAPCreditNoteWithLine("001", TestObjectCreator.AALSHI, currency, exchangeRate, "Desc", job, TestObjectCreator.CC1, 100.0m * exchangeRate, date, false);
			apCreditNotePositivePK = apCreditNotePositive.PK;
			TestObjectCreator.CreateAPCreditNoteLine(apCreditNotePositive, job, TestObjectCreator.CC1, currency, exchangeRate, "Desc", -5m * exchangeRate);
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_AL_APLine = apCreditNotePositive.Lines[0].PK;
			apCreditNotePositive.Lines[0].AL_AT = ZGuid.Empty;
			charge.SetAmountsFromLinkedLinesForTests();
			charge.JR_OSSellAmt = 0M;
			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC1.PK;
			charge2.JR_AL_APLine = apCreditNotePositive.Lines[1].PK;
			apCreditNotePositive.Lines[1].AL_AT = ZGuid.Empty;
			charge2.SetAmountsFromLinkedLinesForTests();
			charge2.JR_OSSellAmt = 0M;

			Factory.Save();

			date = new ZDateTime(2008, 12, 15);
			job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00001002"));
			APCreditNote apCreditNoteNegative = TestObjectCreator.CreateAPCreditNoteWithLine("002", TestObjectCreator.AALSHI, currency, exchangeRate, "Desc", job, TestObjectCreator.CC1, -100.0m * exchangeRate, date, false);
			apCreditNoteNegativePK = apCreditNoteNegative.PK;
			TestObjectCreator.CreateAPCreditNoteLine(apCreditNoteNegative, job, TestObjectCreator.CC1, currency, exchangeRate, "Desc", 5m * exchangeRate);
			charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_AL_APLine = apCreditNoteNegative.Lines[0].PK;
			apCreditNoteNegative.Lines[0].AL_AT = ZGuid.Empty;
			charge.SetAmountsFromLinkedLinesForTests();
			charge.JR_OSSellAmt = 0M;
			charge2 = job.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC1.PK;
			charge2.JR_AL_APLine = apCreditNoteNegative.Lines[1].PK;
			apCreditNoteNegative.Lines[1].AL_AT = ZGuid.Empty;
			charge2.SetAmountsFromLinkedLinesForTests();
			charge2.JR_OSSellAmt = 0M;

			Factory.Save();

			date = new ZDateTime(2009, 06, 15);
			job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00001003"));
			ARCreditNote arCreditNotePositive = TestObjectCreator.CreateARCreditNoteWithLine("003", TestObjectCreator.ABIGAS, currency, exchangeRate, "Desc", job, TestObjectCreator.CC1, 1000.00m * exchangeRate, date, false);
			arCreditNotePositivePK = arCreditNotePositive.PK;
			TestObjectCreator.CreateARCreditNoteLine(arCreditNotePositive, job, TestObjectCreator.CC1, -100.00m * exchangeRate, currency, exchangeRate, "Desc");
			arCreditNotePositive.AH_PostDate = date;
			arCreditNotePositive.Lines[0].AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			arCreditNotePositive.Lines[1].AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_AL_ARLine = arCreditNotePositive.Lines[0].PK;
			arCreditNotePositive.Lines[0].AL_AT = ZGuid.Empty;
			charge.SetAmountsFromLinkedLinesForTests();
			charge.JR_OSCostAmt = 0M;
			charge2 = job.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC1.PK;
			charge2.JR_AL_ARLine = arCreditNotePositive.Lines[1].PK;
			arCreditNotePositive.Lines[1].AL_AT = ZGuid.Empty;
			charge2.SetAmountsFromLinkedLinesForTests();
			charge2.JR_OSCostAmt = 0M;

			Factory.Save();

			date = new ZDateTime(2009, 12, 15);
			job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00001004"));
			ARCreditNote arCreditNoteNegative = TestObjectCreator.CreateARCreditNoteWithLine("004", TestObjectCreator.ABIGAS, currency, exchangeRate, "Desc", job, TestObjectCreator.CC1, -1000.00m * exchangeRate, date, false);
			arCreditNoteNegativePK = arCreditNoteNegative.PK;
			TestObjectCreator.CreateARCreditNoteLine(arCreditNoteNegative, job, TestObjectCreator.CC1, 100.00m * exchangeRate, currency, exchangeRate, "Desc");
			arCreditNoteNegative.AH_PostDate = date;
			arCreditNoteNegative.Lines[0].AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			arCreditNoteNegative.Lines[1].AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_AL_ARLine = arCreditNoteNegative.Lines[0].PK;
			arCreditNoteNegative.Lines[0].AL_AT = ZGuid.Empty;
			charge.SetAmountsFromLinkedLinesForTests();
			charge.JR_OSCostAmt = 0M;
			charge2 = job.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC1.PK;
			charge2.JR_AL_ARLine = arCreditNoteNegative.Lines[1].PK;
			arCreditNoteNegative.Lines[1].AL_AT = ZGuid.Empty;
			charge2.SetAmountsFromLinkedLinesForTests();
			charge2.JR_OSCostAmt = 0M;

			Factory.Save();

			job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00001005"));
			ARCreditNote arCreditNoteMatched = TestObjectCreator.CreateARCreditNoteWithLine("005", TestObjectCreator.ABIGAS, currency, exchangeRate, "Desc", job, TestObjectCreator.CC1, 100.00m * exchangeRate, date, false);
			arCreditNoteMatched.AH_PostDate = date;
			arCreditNoteMatched.Lines[0].AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_AL_ARLine = arCreditNoteMatched.Lines[0].PK;
			arCreditNoteMatched.Lines[0].AL_AT = ZGuid.Empty;
			charge.SetAmountsFromLinkedLinesForTests();
			charge.JR_OSCostAmt = 0M;

			ARPayment arPaymentMatched = TestObjectCreator.CreateARPayment(exchangeRate, -20m * exchangeRate, date, date, TestObjectCreator.ABIGAS.PK, currencyBankAccountPK);

			Factory.Save();

			TransactionMatchLinkGroup matchGroup = new TransactionMatchLinkGroup(Factory);
			AccTransactionMatchLink invoiceLink = matchGroup.AddNew();
			invoiceLink.AP_AH = arCreditNoteMatched.PK;
			invoiceLink.AP_Amount = -20m;

			AccTransactionMatchLink paymentLink = matchGroup.AddNew();
			paymentLink.AP_AH = arPaymentMatched.PK;
			paymentLink.AP_Amount = 20m;

			matchGroup.SetMatchGroupNumberAndMatchDate("M00002", date);

			Factory.Save();
		}

		#endregion

		#region Create All Transactions

		void createTransactions(bool isLocalCurrency)
		{
			createInvoices(isLocalCurrency);
			createCreditNotes(isLocalCurrency);
		}

		ZGuid apInvoicePositivePK;
		ZGuid apInvoiceNegativePK;
		ZGuid arInvoicePositivePK;
		ZGuid arInvoiceNegativePK;

		ZGuid apCreditNotePositivePK;
		ZGuid apCreditNoteNegativePK;
		ZGuid arCreditNotePositivePK;
		ZGuid arCreditNoteNegativePK;

		#endregion

		#endregion

		#region Tests

		public void TestEnableInvoicePaymentWebServiceRegistryIsOnlyOnCompanyLevel()
		{
			AssertEquals("Must be disabled by default", false, AccountingConfigurationRegistry.Instance.EnableInvoicePaymentWebService.Value);

			TransactionPaymentDataAccess dataAccess = new TransactionPaymentDataAccess(Connection, Transaction);
			AssertEquals("Must be disabled by default for  current company", false, dataAccess.GetBoolRegistryValue("ENABLEINVOICEPAYMENTWEBSERVICE", GlbCompany.CurrentCompany.GC_Code));
			AssertEquals("Must be disabled by default for other company", false, dataAccess.GetBoolRegistryValue("ENABLEINVOICEPAYMENTWEBSERVICE", TestObjectCreator.NonCurrentCompany.GC_Code));

			AccountingConfigurationRegistry.Instance.EnableInvoicePaymentWebService.SetValue(TestObjectCreator.NonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("Must still be disabled", false, AccountingConfigurationRegistry.Instance.EnableInvoicePaymentWebService.Value);
			AssertEquals("Must be not affected by enabling for other company", false, dataAccess.GetBoolRegistryValue("ENABLEINVOICEPAYMENTWEBSERVICE", GlbCompany.CurrentCompany.GC_Code));
			AssertEquals("Must be enabled for other company", true, dataAccess.GetBoolRegistryValue("ENABLEINVOICEPAYMENTWEBSERVICE", TestObjectCreator.NonCurrentCompany.GC_Code));

			AccountingConfigurationRegistry.Instance.EnableInvoicePaymentWebService.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("Must be enabled now", true, AccountingConfigurationRegistry.Instance.EnableInvoicePaymentWebService.Value);
			AssertEquals("Must be enabled for the current company", true, dataAccess.GetBoolRegistryValue("ENABLEINVOICEPAYMENTWEBSERVICE", GlbCompany.CurrentCompany.GC_Code));
			AssertEquals("Must be enabled for other company", true, dataAccess.GetBoolRegistryValue("ENABLEINVOICEPAYMENTWEBSERVICE", TestObjectCreator.NonCurrentCompany.GC_Code));

			AssertEquals("Must be false on System level", false, dataAccess.GetBoolRegistryValue("ENABLEINVOICEPAYMENTWEBSERVICE"));
			AssertEquals("Must be false for an incorrect company code", false, dataAccess.GetBoolRegistryValue("ENABLEINVOICEPAYMENTWEBSERVICE", "XYZ"));

			AccountingConfigurationRegistry.Instance.EnableInvoicePaymentWebService.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("Must be disabled now", false, AccountingConfigurationRegistry.Instance.EnableInvoicePaymentWebService.Value);
			AssertEquals("Must be disabled for the current company", false, dataAccess.GetBoolRegistryValue("ENABLEINVOICEPAYMENTWEBSERVICE", GlbCompany.CurrentCompany.GC_Code));
			AssertEquals("Must be enabled for other company", true, dataAccess.GetBoolRegistryValue("ENABLEINVOICEPAYMENTWEBSERVICE", TestObjectCreator.NonCurrentCompany.GC_Code));
		}

		[SuspendCriticalValidation]
		public void TestPayInvoicesForeignCurrency() => AssertPayInvoices(false, false);

		[SuspendCriticalValidation]
		public void TestPayInvoicesForeignCurrency_EnableNewOSOutstandingAmountFeature() => AssertPayInvoices(false, true);

		[SuspendCriticalValidation]
		public void TestPayInvoicesLocalCurrency() => AssertPayInvoices(true, false);

		[SuspendCriticalValidation]
		public void TestPayInvoicesLocalCurrency_EnableNewOSOutstandingAmountFeature() => AssertPayInvoices(true, true);

		void AssertPayInvoices(bool isLocalCurrency, bool isOSOutStandingAmountNewFeatureEnabled)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isOSOutStandingAmountNewFeatureEnabled);

			setupCommonData();
			createTransactions(isLocalCurrency);
			Factory.Save();

			UpdateInvoicePaymentDetailsResponse result = PayTransaction("AP", "INV", "XXXXXX", "111", null, 5m, null);
			AssertEquals("Succeeded", false, result.Succeeded);
			AssertEquals("ErrorMessage", "No Transactions were found for the given key data.", result.ErrorMessage);

			result = PayTransaction("AP", "INV", "AALSHI", "111", null, 5m, null);
			Assert("Succeeded", result.Succeeded);
			AssertPaymentStatus(result, "AUD", -99m, -5m, null);
			AssertPaymentLog(apInvoicePositivePK, "AUD", -94m);
			AssertOSOutstandingAmount(apInvoicePositivePK, isOSOutStandingAmountNewFeatureEnabled, isLocalCurrency ? -94m : -47m);

			result = PayTransaction("AP", "INV", "AALSHI", null, "00001000", 10m, null);
			Assert("Succeeded", result.Succeeded);
			AssertPaymentStatus(result, "AUD", -99m, -15m, null);
			AssertPaymentLog(apInvoicePositivePK, "AUD", -84m);
			AssertOSOutstandingAmount(apInvoicePositivePK, isOSOutStandingAmountNewFeatureEnabled, isLocalCurrency ? -84m : -42m);

			DateTime paidDate = new DateTime(2011, 10, 19);
			result = PayTransaction("AP", "INV", "AALSHI", "111", null, 84m, paidDate);
			Assert("Succeeded", result.Succeeded);
			AssertPaymentStatus(result, "AUD", -99m, -99m, paidDate);
			AssertPaymentLog(apInvoicePositivePK, "AUD", 0m);
			AssertOSOutstandingAmount(apInvoicePositivePK, isOSOutStandingAmountNewFeatureEnabled, 0);

			result = PayTransaction("AP", "INV", "AALSHI", "111", null, -50m, paidDate);
			Assert("Succeeded", result.Succeeded);
			AssertPaymentStatus(result, "AUD", -99m, -49m, null);
			AssertPaymentLog(apInvoicePositivePK, "AUD", -50m);
			AssertOSOutstandingAmount(apInvoicePositivePK, isOSOutStandingAmountNewFeatureEnabled, isLocalCurrency ? -50m : -25m);

			result = PayTransaction("AP", "INV", "AALSHI", "111", null, -50m, null);
			AssertEquals("Succeeded", false, result.Succeeded);
			AssertEquals("ErrorMessage", "Reversing payment cannot produce Outstanding Amount greater than Total Amount.", result.ErrorMessage);

			result = PayTransaction("AP", "INV", "AALSHI", "112", null, 25m, null);
			Assert("Succeeded", result.Succeeded);
			AssertPaymentStatus(result, "AUD", 99m, 25m, null);
			AssertPaymentLog(apInvoiceNegativePK, "AUD", 74m);
			AssertOSOutstandingAmount(apInvoiceNegativePK, isOSOutStandingAmountNewFeatureEnabled, isLocalCurrency ? 74m : 37m);

			result = PayTransaction("AP", "INV", "AALSHI", null, "00001001", 74m, paidDate, "012345678901234567890");
			AssertEquals("Succeeded", false, result.Succeeded);
			AssertEquals("ErrorMessage", "Payment Reference should be not longer than 20 characters.", result.ErrorMessage);

			result = PayTransaction("AP", "INV", "AALSHI", null, "00001001", 74m, paidDate, "0123456789?");
			AssertEquals("Succeeded", false, result.Succeeded);
			AssertEquals("ErrorMessage", "Payment Reference should contain only numbers or letters.", result.ErrorMessage);

			result = PayTransaction("AP", "INV", "AALSHI", null, "00001001", 74m, paidDate, "Abc001");
			Assert("Succeeded", result.Succeeded);
			AssertPaymentStatus(result, "AUD", 99m, 99m, paidDate);
			AssertPaymentLog(apInvoiceNegativePK, "AUD", 0m);
			AssertPaymentReference(apInvoiceNegativePK, "Abc001");
			AssertOSOutstandingAmount(apInvoiceNegativePK, isOSOutStandingAmountNewFeatureEnabled, 0);

			result = PayTransaction("AP", "INV", "AALSHI", null, "00001001", 14m, null);
			AssertEquals("Succeeded", false, result.Succeeded);
			AssertEquals("ErrorMessage", "Transaction has been already fully paid.", result.ErrorMessage);

			result = PayTransaction("AR", "INV", "ABIGAS", "00001000", null, 20m, paidDate); // Paid Date will be ignored transaction is not fully paid. See AssertPaymentStatus below 
			Assert("Succeeded", result.Succeeded);
			AssertPaymentStatus(result, "AUD", 55m, 20m, null);
			AssertPaymentLog(arInvoicePositivePK, "AUD", 35m);
			AssertOSOutstandingAmount(arInvoicePositivePK, isOSOutStandingAmountNewFeatureEnabled, isLocalCurrency ? 35m : 17.5m);

			result = PayTransaction("AR", "INV", "ABIGAS", "00001001", null, 55m, null);
			AssertEquals("Succeeded", false, result.Succeeded);
			AssertEquals("ErrorMessage", "Payment Date must be provided when transaction is fully paid.", result.ErrorMessage);

			result = PayTransaction("AR", "INV", "ABIGAS", "00001001", null, 55m, new DateTime(2012, 7, 1));
			AssertEquals("Succeeded", false, result.Succeeded);
			AssertEquals("ErrorMessage", "Payment date 1/07/2012 does not fall into an open accounting period for the EDI company.", result.ErrorMessage);

			result = PayTransaction("AR", "INV", "ABIGAS", "00001001", null, 155m, paidDate);
			AssertEquals("Succeeded", false, result.Succeeded);
			AssertEquals("ErrorMessage", "AmountPaidInCompanyCurrency exceeds Transaction's Outstanding Amount.", result.ErrorMessage);

			result = PayTransaction("AR", "INV", "ABIGAS", "00001001", null, 55m, paidDate);
			Assert("Succeeded", result.Succeeded);
			AssertPaymentStatus(result, "AUD", -55m, -55m, paidDate);
			AssertPaymentLog(arInvoiceNegativePK, "AUD", 0m);
			AssertOSOutstandingAmount(arInvoiceNegativePK, isOSOutStandingAmountNewFeatureEnabled, 0);

			result = PayTransaction("AR", "INV", "ABIGAS", "00001002", null, 25m, null);
			AssertEquals("Succeeded", false, result.Succeeded);
			AssertEquals("ErrorMessage", "Transaction already has Matching and cannot be paid via this Web Service.", result.ErrorMessage);
		}

		[SuspendCriticalValidation]
		public void TestPayCreditNotesForeignCurrency() => AssertPayCreditNotes(false, false);

		[SuspendCriticalValidation]
		public void TestPayCreditNotesForeignCurrency_EnableNewOSOutstandingAmountFeature() => AssertPayCreditNotes(false, true);

		[SuspendCriticalValidation]
		public void TestPayCreditNotesLocalCurrency() => AssertPayCreditNotes(true, false);

		[SuspendCriticalValidation]
		public void TestPayCreditNotesLocalCurrency_EnableNewOSOutstandingAmountFeature() => AssertPayCreditNotes(true, true);

		void AssertPayCreditNotes(bool isLocalCurrency, bool isOSOutStandingAmountNewFeatureEnabled)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isOSOutStandingAmountNewFeatureEnabled);

			setupCommonData();
			createTransactions(isLocalCurrency);
			Factory.Save();
			DateTime paidDate = new DateTime(2011, 10, 19);

			UpdateInvoicePaymentDetailsResponse result = PayTransaction("AP", "CRD", "XXXXXX", "111", null, 5m, null);
			AssertEquals("Succeeded", false, result.Succeeded);
			AssertEquals("ErrorMessage", "No Transactions were found for the given key data.", result.ErrorMessage);

			result = PayTransaction("AP", "CRD", "AALSHI", "001", null, 5m, null);
			Assert("Succeeded", result.Succeeded);
			AssertPaymentStatus(result, "AUD", 95m, 5m, null);
			AssertPaymentLog(apCreditNotePositivePK, "AUD", 90m);
			AssertOSOutstandingAmount(apCreditNotePositivePK, isOSOutStandingAmountNewFeatureEnabled, isLocalCurrency ? 90m : 45m);

			result = PayTransaction("AP", "CRD", "AALSHI", null, "00001000", 10m, null);
			Assert("Succeeded", result.Succeeded);
			AssertPaymentStatus(result, "AUD", 95m, 15m, null);
			AssertPaymentLog(apCreditNotePositivePK, "AUD", 80m);
			AssertOSOutstandingAmount(apCreditNotePositivePK, isOSOutStandingAmountNewFeatureEnabled, isLocalCurrency ? 80m : 40m);

			result = PayTransaction("AP", "CRD", "AALSHI", "001", null, 80m, paidDate, "012345678901234567890");
			AssertEquals("Succeeded", false, result.Succeeded);
			AssertEquals("ErrorMessage", "Payment Reference should be not longer than 20 characters.", result.ErrorMessage);

			result = PayTransaction("AP", "CRD", "AALSHI", "001", null, 80m, paidDate, "0123456789?");
			AssertEquals("Succeeded", false, result.Succeeded);
			AssertEquals("ErrorMessage", "Payment Reference should contain only numbers or letters.", result.ErrorMessage);

			result = PayTransaction("AP", "CRD", "AALSHI", "001", null, 80m, paidDate, "Abc001");
			Assert("Succeeded", result.Succeeded);
			AssertPaymentStatus(result, "AUD", 95m, 95m, paidDate);
			AssertPaymentLog(apCreditNotePositivePK, "AUD", 0m);
			AssertPaymentReference(apCreditNotePositivePK, "Abc001");
			AssertOSOutstandingAmount(apCreditNotePositivePK, isOSOutStandingAmountNewFeatureEnabled, 0);

			result = PayTransaction("AP", "CRD", "AALSHI", "001", null, -50m, paidDate);
			Assert("Succeeded", result.Succeeded);
			AssertPaymentStatus(result, "AUD", 95m, 45m, null);
			AssertPaymentLog(apCreditNotePositivePK, "AUD", 50m);
			AssertOSOutstandingAmount(apCreditNotePositivePK, isOSOutStandingAmountNewFeatureEnabled, isLocalCurrency ? 50m : 25m);

			result = PayTransaction("AP", "CRD", "AALSHI", "001", null, -50m, null);
			AssertEquals("Succeeded", false, result.Succeeded);
			AssertEquals("ErrorMessage", "Reversing payment cannot produce Outstanding Amount greater than Total Amount.", result.ErrorMessage);

			result = PayTransaction("AP", "CRD", "AALSHI", "002", null, 25m, paidDate);
			Assert("Succeeded", result.Succeeded);
			AssertPaymentStatus(result, "AUD", -95m, -25m, null);
			AssertPaymentLog(apCreditNoteNegativePK, "AUD", -70m);
			AssertOSOutstandingAmount(apCreditNoteNegativePK, isOSOutStandingAmountNewFeatureEnabled, isLocalCurrency ? -70m : -35m);

			result = PayTransaction("AP", "CRD", "AALSHI", null, "00001001", 70m, paidDate);
			Assert("Succeeded", result.Succeeded);
			AssertPaymentStatus(result, "AUD", -95m, -95m, paidDate);
			AssertPaymentLog(apCreditNoteNegativePK, "AUD", 0m);
			AssertOSOutstandingAmount(apCreditNoteNegativePK, isOSOutStandingAmountNewFeatureEnabled, 0);

			result = PayTransaction("AP", "CRD", "AALSHI", null, "00001001", 10m, null);
			AssertEquals("Succeeded", false, result.Succeeded);
			AssertEquals("ErrorMessage", "Transaction has been already fully paid.", result.ErrorMessage);

			result = PayTransaction("AR", "CRD", "ABIGAS", "00001000", null, 200m, null);
			Assert("Succeeded", result.Succeeded);
			AssertPaymentStatus(result, "AUD", -900m, -200m, null);
			AssertPaymentLog(arCreditNotePositivePK, "AUD", -700m);
			AssertOSOutstandingAmount(arCreditNotePositivePK, isOSOutStandingAmountNewFeatureEnabled, isLocalCurrency ? -700m : -350m);

			result = PayTransaction("AR", "CRD", "ABIGAS", "00001001", null, 900m, null);
			AssertEquals("Succeeded", false, result.Succeeded);
			AssertEquals("ErrorMessage", "Payment Date must be provided when transaction is fully paid.", result.ErrorMessage);

			result = PayTransaction("AR", "CRD", "ABIGAS", "00001001", null, 900m, new DateTime(2012, 7, 1));
			AssertEquals("Succeeded", false, result.Succeeded);
			AssertEquals("ErrorMessage", "Payment date 1/07/2012 does not fall into an open accounting period for the EDI company.", result.ErrorMessage);

			result = PayTransaction("AR", "CRD", "ABIGAS", "00001001", null, 1000m, paidDate);
			AssertEquals("Succeeded", false, result.Succeeded);
			AssertEquals("ErrorMessage", "AmountPaidInCompanyCurrency exceeds Transaction's Outstanding Amount.", result.ErrorMessage);

			result = PayTransaction("AR", "CRD", "ABIGAS", "00001001", null, 900m, paidDate);
			Assert("Succeeded", result.Succeeded);
			AssertPaymentStatus(result, "AUD", 900m, 900m, paidDate);
			AssertPaymentLog(arCreditNoteNegativePK, "AUD", 0m);
			AssertOSOutstandingAmount(arCreditNoteNegativePK, isOSOutStandingAmountNewFeatureEnabled, 0);

			result = PayTransaction("AR", "CRD", "ABIGAS", "00001002", null, 25m, null);
			AssertEquals("Succeeded", false, result.Succeeded);
			AssertEquals("ErrorMessage", "Transaction already has Matching and cannot be paid via this Web Service.", result.ErrorMessage);
		}

		public void TestKeepOriginalLogicWhenDisableNewOSOutstandingAmountFeature()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.DataType.SuspendValidation())
			{
				AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var aPInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("112", TestObjectCreator.USD, 0.5m, -50m, -5m, 0m, -50m, -5m, 0m, TestObjectCreator.AALSHI);
				aPInvoice.AH_PostDate = new ZDateTime(2008, 12, 15);
				Factory.Save();

				AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				var result = PayTransaction("AP", "INV", "AALSHI", "112", null, 25m, null);
				Assert("Succeeded", result.Succeeded);

				var factory = new BusinessObjectFactory();
				var transaction = factory.Load<AccTransactionHeader>(aPInvoice.PK);
				AssertEquals("AH_OutstandingAmount, (50m + 5m) - 25m", 30m, transaction.AH_OutstandingAmount);
				AssertEquals("AH_OSOutstandingAmount is still same to OSTotal that 55m * 0.5 = 27.5m", 27.50m, transaction.AH_OSOutstandingAmount);
			}
		}

		#endregion

		UpdateInvoicePaymentDetailsResponse PayTransaction(string ledger, string type, string orgCode, string transactionNum, string invoiceRef, Decimal payment, DateTime? paidDate, string payRef = null)
		{
			TransactionPaymentDataAccess dataAccess = new TransactionPaymentDataAccess(Connection, Transaction);
			InvoicePaymentDetailsUpdater updater = new InvoicePaymentDetailsUpdater(dataAccess);

			UpdateInvoicePaymentDetailsRequest request = new UpdateInvoicePaymentDetailsRequest();
			request.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			request.AccLedger = ledger;
			request.TransactionType = type;
			request.OrgCode = orgCode;
			request.TransactionNumber = transactionNum;
			request.JobTransactionNumber = ledger == "AR" ? invoiceRef : null;
			request.InternalReference = ledger == "AP" ? invoiceRef : null;
			request.AmountPaidInCompanyCurrency = payment;
			request.PaymentDate = paidDate;
			if (!string.IsNullOrEmpty(payRef))
			{
				request.PaymentReference = payRef;
			}

			UpdateInvoicePaymentDetailsResponse result = updater.PayTransaction(request);

			AssertEquals("OriginalRequest.CompanyCode", GlbCompany.CurrentCompany.GC_Code, result.OriginalRequest.CompanyCode);
			AssertEquals("OriginalRequest.AccLedger", ledger, result.OriginalRequest.AccLedger);
			AssertEquals("OriginalRequest.TransactionType", type, result.OriginalRequest.TransactionType);
			AssertEquals("OriginalRequest.OrgCode", orgCode, result.OriginalRequest.OrgCode);
			AssertEquals("OriginalRequest.TransactionNumber", transactionNum, result.OriginalRequest.TransactionNumber);
			AssertEquals("OriginalRequest.JobTransactionNumber", ledger == "AR" ? invoiceRef : null, result.OriginalRequest.JobTransactionNumber);
			AssertEquals("OriginalRequest.InternalReference", ledger == "AP" ? invoiceRef : null, result.OriginalRequest.InternalReference);
			AssertEquals("OriginalRequest.AmountPaidInCompanyCurrency", payment, result.OriginalRequest.AmountPaidInCompanyCurrency);
			AssertEquals("OriginalRequest.PaymentDate", paidDate, result.OriginalRequest.PaymentDate);
			AssertEquals("OriginalRequest.PaymentReference", payRef, result.OriginalRequest.PaymentReference);

			return result;
		}

		void AssertPaymentStatus(UpdateInvoicePaymentDetailsResponse response, string currency, Decimal invoiced, Decimal paid, DateTime? fullyPaidDate)
		{
			AssertEquals("CurrencyCode", currency, response.CurrencyCode);
			AssertEquals("InvoiceTotal", invoiced, response.InvoiceTotal);
			AssertEquals("PaidAmount", paid, response.PaidAmount);
			AssertEquals("FullyPaidDateHasValue", fullyPaidDate.HasValue, response.FullyPaidDateHasValue);
			if (fullyPaidDate.HasValue)
			{
				AssertEquals("FullyPaidDate", fullyPaidDate, response.FullyPaidDate);
				AssertEquals("PaymentStatus", "PAID", response.PaymentStatus);
			}
			else if (paid != Decimal.Zero)
			{
				AssertEquals("PaymentStatus", "PARTPAID", response.PaymentStatus);
			}
			else
			{
				AssertEquals("PaymentStatus", "UNPAID", response.PaymentStatus);
			}
		}

		void AssertPaymentReference(ZGuid transactionPK, string payRef)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.PK, transactionPK);

			var tran = Factory.Load<AccTransactionHeader>(transactionPK);
			tran.Reload();
			AssertNotNull("Transaction should exist", tran);
			AssertEquals("AH_ChequeOrReference should match.", new ZString(payRef), tran.AH_ChequeOrReference);
		}

		void AssertPaymentLog(ZGuid transactionPK, string currencyCode, Decimal paidToAmount)
		{
			ZQuery logFilter = new ZQuery(StmALogSchema.SL_Parent, transactionPK);
			logFilter.AddToFilter(StmALogSchema.SL_SE_NKEvent, "EDT");
			logFilter.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "Transaction Payment Web Service");
			logFilter.ReLoadExistingRows = true; // To force loading from Db

			var logs = Factory.Load<StmALog>(logFilter);
			if (paidToAmount == 0M)
			{
				AssertNotNull("Should be a Log that transaction was fully paid", logs.FirstOrDefault(x => x.SL_Reference.Contains("Fully paid")));
				AssertNotNull("Should be a Log that transaction was fully paid", logs.FirstOrDefault(x => x.SL_Reference.Contains("|Fully Matched")));
			}
			else
			{
				AssertNotNull(string.Format("Should be a Log that transaction was paid to {0} {1:0.00} Outstanding Amount", currencyCode, paidToAmount),
					logs.FirstOrDefault(x => x.SL_Reference.Contains(string.Format("Paid to {0} {1:0.00} Outstanding Amount by Transaction Payment Web Service", currencyCode, paidToAmount))));
			}
		}

		void AssertOSOutstandingAmount(ZGuid transactionPK, bool isNewFeatureInvoice, decimal expectedAmount)
		{
			var factory = new BusinessObjectFactory();
			var transaction = factory.Load<AccTransactionHeader>(transactionPK);
			AssertEquals("AH_OSOutstandingAmount", isNewFeatureInvoice ? expectedAmount : 0m, transaction.AH_OSOutstandingAmount);
		}
	}

	public class UpdateInvoicePaymentDetailsServiceTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestTransmitUniversalTransactionWhenTransactionIsFullyPaidByWebService()
		{
			using (Factory.AddDisposableService())
			{
				#region Setup Data 

				var participantOrg1 = TestObjectCreator.CreateOrgHeader("TSTPCNT1", true, true);
				participantOrg1.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, "P1");
				TestObjectCreator.AddEdiCommunication(participantOrg1, EDICommunicationsMode.Modules.Netting, EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction
					, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, "P2");

				var participantOrg2 = TestObjectCreator.CreateOrgHeader("TSTPCNT3", true, true);
				TestObjectCreator.AddEdiCommunication(participantOrg2, EDICommunicationsMode.Modules.Netting, EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction
					, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, "P1");
				TestObjectCreator.AddEdiCommunication(participantOrg2, EDICommunicationsMode.Modules.Netting, EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction
					, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, "P2");

				participantOrg2.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, "P2");

				AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateARSuspenseControlAccount().PK.ToGuid());
				AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateAPSuspenseControlAccount().PK.ToGuid());
				AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateJobRevenueJournalControlAccount().PK.ToGuid());
				AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.NettingStartDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.ToDateTime());
				AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, participantOrg2.PK.ToGuid());
				Factory.Save();

				#endregion

				var dateTime = ZDateTime.Now.AddYears(-1);

				var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, participantOrg1);
				invoice.Lines[0].AL_AC = TestObjectCreator.OverheadChargeCode.PK;
				invoice.AH_PostDate = dateTime;
				Factory.Save();

				var service = new UpdateInvoicePaymentDetailsServiceForTest(Connection, Transaction);
				service.SecurityHeader = new SecuritySOAPHeader() { UserName = "username", Password = "password" };
				var request = CreateRequest("AP", "INV", "ZTSTPCNT1", "111", 110m);
				request.PaymentDate = dateTime.ToDateTime();

				var response = service.UpdateInvoicePaymentDetails(request);
				Assert(response.Succeeded);

				var logFilter = new ZQuery(StmALogSchema.SL_Parent, invoice.PK);
				logFilter.AddToFilter(StmALogSchema.SL_SE_NKEvent, "EDT");
				logFilter.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "Transaction Payment Web Service");

				var log = Factory.Load<StmALog>(logFilter).FirstOrDefault(x => x.SL_Reference.Contains("|Fully Matched"));
				AssertNotNull(log);
				AssertEquals("Should be processed successfully", true, UniversalTransactionTransmitter.UniversalTransmitForNettingSystem(new NotificationCollection(), Factory, invoice, HelperMethods.GetStatus(log.SL_Reference)));
				Factory.Save(); // Also should save lel.
			}
		}

		public void TestAmountPaidInCompanyCurrencyExceedingAllowedNumberOfDecimalPlaces()
		{
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			invoice.Lines[0].AL_AC = TestObjectCreator.OverheadChargeCode.PK;
			invoice.AH_PostDate = ZDateTime.Now.AddYears(-1);
			Factory.Save();

			var service = new UpdateInvoicePaymentDetailsServiceForTest(Connection, Transaction);
			service.SecurityHeader = new SecuritySOAPHeader() { UserName = "username", Password = "password" };
			var request = CreateRequest("AP", "INV", "AALSHI", "111", 12.3456m);

			AssertEquals(100, new TransactionPaymentDataAccess(Connection, Transaction).TryGetCompanyCurrencySubUnitRatio(GlbCompany.CurrentCompany.GC_Code));

			var response = service.UpdateInvoicePaymentDetails(request);
			Assert(!response.Succeeded);
			AssertEquals("AmountPaidInCompanyCurrency exceed the number of decimals that are allowed by the company's currency.", response.ErrorMessage);

			request.AmountPaidInCompanyCurrency = 12.34m;
			response = service.UpdateInvoicePaymentDetails(request);
			Assert(response.Succeeded);
			AssertNull(response.ErrorMessage);
		}

		[ExpectNoExceptions]
		public void TestUpdateInvoicePaymentDetails_RunSafelyFromAnotherThread()
		{
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			invoice.Lines[0].AL_AC = TestObjectCreator.OverheadChargeCode.PK;
			invoice.AH_PostDate = ZDateTime.Now.AddYears(-1);

			AccountingConfigurationRegistry.Instance.EnableInvoicePaymentWebService.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Factory.Save();

			ExceptionDispatchInfo exceptionInfo = null;

			var thread = new Thread(() =>
			{
				var service = new UpdateInvoicePaymentDetailsServiceForTest(Connection, Transaction);
				service.SecurityHeader = new SecuritySOAPHeader() { UserName = "username", Password = "password" };
				var request = CreateRequest("AP", "INV", "AALSHI", "111", 12.34m);

				try
				{
					service.UpdateInvoicePaymentDetails(request);
				}
				catch (Exception ex)
				{
					exceptionInfo = ExceptionDispatchInfo.Capture(ex);
				}
			});
			thread.Start();
			thread.Join();

			exceptionInfo?.Throw();
		}

		public void TestInvalidLoginDetails()
		{
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			invoice.Lines[0].AL_AC = TestObjectCreator.OverheadChargeCode.PK;
			invoice.AH_PostDate = ZDateTime.Now.AddYears(-1);
			Factory.Save();

			var service = new UpdateInvoicePaymentDetailsServiceForTest(Connection, Transaction);
			service.SecurityHeader = new SecuritySOAPHeader() { UserName = "", Password = "password" };
			var request = CreateRequest("AP", "INV", "AALSHI", "111", 12.34m);

			var response = service.UpdateInvoicePaymentDetails(request);
			Assert(!response.Succeeded);
			AssertEquals("Please, provide both User Name and Password to log in CargoWise Accounting Web Service.", response.ErrorMessage);

			service.SecurityHeader.UserName = "jane";

			response = service.UpdateInvoicePaymentDetails(request);
			Assert(!response.Succeeded);
			AssertEquals("Invalid User Name/Password", response.ErrorMessage);

			service.SecurityHeader.UserName = "username";

			response = service.UpdateInvoicePaymentDetails(request);
			Assert(response.Succeeded);
			AssertNull(response.ErrorMessage);
		}

		public void TestEnableInvoicePaymentWebServiceRegistrySetting()
		{
			try
			{
				var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
				invoice.Lines[0].AL_AC = TestObjectCreator.OverheadChargeCode.PK;
				invoice.AH_PostDate = ZDateTime.Now.AddYears(-1);

				AccountingConfigurationRegistry.Instance.EnableInvoicePaymentWebService.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				Factory.Save();

				var differentThread = new Thread(() =>
				{
					var service = new UpdateInvoicePaymentDetailsServiceForTest(Connection, Transaction);
					service.SecurityHeader = new SecuritySOAPHeader() { UserName = "username", Password = "password" };
					var request = CreateRequest("AP", "INV", "AALSHI", "111", 12.34m);

					var response = service.UpdateInvoicePaymentDetails(request);
					Assert(!response.Succeeded);
					AssertEquals("Currently this service is disabled for the 'EDI' company. To enable this Invoice Payment Web Service go to the 'Accounting > Web > Enable Invoice Payment Web Service' registry item.", response.ErrorMessage);
				});
				differentThread.Start();
				differentThread.Join();

				AccountingConfigurationRegistry.Instance.EnableInvoicePaymentWebService.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				Factory.Save();

				differentThread = new Thread(() =>
				{
					var service = new UpdateInvoicePaymentDetailsServiceForTest(Connection, Transaction);
					service.SecurityHeader = new SecuritySOAPHeader() { UserName = "username", Password = "password" };
					var request = CreateRequest("AP", "INV", "AALSHI", "111", 12.34m);

					var response = service.UpdateInvoicePaymentDetails(request);
					Assert(response.Succeeded);
					AssertNull(response.ErrorMessage);
				});
				differentThread.Start();
				differentThread.Join();
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.EnableInvoicePaymentWebService.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				Factory.Save();
			}
		}

		public void TestUpdateInvoicePaymentDetailsExceptionErrorReporter()
		{
			using (Factory.AddDisposableService())
			{
				// Arrange
				#region Setup Data 

				var participantOrg1 = TestObjectCreator.CreateOrgHeader("TSTPCNT1", true, true);
				participantOrg1.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, "P1");
				TestObjectCreator.AddEdiCommunication(participantOrg1, EDICommunicationsMode.Modules.Netting, EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction
					, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, "P2");

				var participantOrg2 = TestObjectCreator.CreateOrgHeader("TSTPCNT3", true, true);
				TestObjectCreator.AddEdiCommunication(participantOrg2, EDICommunicationsMode.Modules.Netting, EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction
					, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, "P1");
				TestObjectCreator.AddEdiCommunication(participantOrg2, EDICommunicationsMode.Modules.Netting, EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction
					, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, "P2");

				participantOrg2.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, "P2");

				AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateARSuspenseControlAccount().PK.ToGuid());
				AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateAPSuspenseControlAccount().PK.ToGuid());
				AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateJobRevenueJournalControlAccount().PK.ToGuid());
				AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.NettingStartDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.ToDateTime());
				AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, participantOrg2.PK.ToGuid());
				Factory.Save();

				#endregion

				var dateTime = ZDateTime.Now.AddYears(-1);

				var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, participantOrg1);
				invoice.Lines[0].AL_AC = TestObjectCreator.OverheadChargeCode.PK;
				invoice.AH_PostDate = dateTime;
				Factory.Save();

				var service = new UpdateInvoicePaymentDetailsServiceForTest(null, null);
				service.SecurityHeader = new SecuritySOAPHeader() { UserName = "username", Password = "password" };
				var request = CreateRequest("AP", "INV", "ZTSTPCNT1", "111", 110m);
				request.PaymentDate = dateTime.ToDateTime();

				// Act
				var response = service.UpdateInvoicePaymentDetails(request);

				// Assert
				Assert("Fail", !response.Succeeded);
				AssertEquals(response.ErrorMessage, "An unexpected error occurred. Please try again later.");

				var webException = ErrorReporter.LastExceptionReported as AccountingWebReportableException;
				AssertNotNull(webException);
				AssertEquals(webException.Message, "BaseDataAccess: Connection property has not been initialized.");
				AssertEquals(ErrorReporter.LastMessageReported, "BaseDataAccess: Connection property has not been initialized.");
				AssertEquals(ErrorReporter.LastKeyReported, "Accounting.Web_UpdateInvoicePaymentDetailsService.UpdateInvoicePaymentDetails_InvalidOperationException");

				var innerException = webException.InnerException as InvalidOperationException;
				AssertNotNull(innerException);
				AssertNotNullOrEmpty(innerException.StackTrace);
				AssertEquals(webException.Source, innerException.Source);
				ErrorReporter.Clear();
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		TestObjectCreator TestObjectCreator { get; set; }
		System.Data.Common.DbConnection Connection { get; set; }
		System.Data.Common.DbTransaction Transaction { get; set; }

		protected override void SetUp()
		{
			base.SetUp();

			AccountingConfigurationRegistry.Instance.EnableInvoicePaymentWebService.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AccountingWebServiceUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "username");
			AccountingConfigurationRegistry.Instance.AccountingWebServicePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "password");

			var helper = new AccountingPeriodTestHelper(Factory);
			helper.PostPeriodsForEntireYear(ZDateTime.Now.AddYears(-1).Year, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

			Connection = ((IDbConnectionInternals)base.TestConnection).ADOConnection;
			Transaction = ((IDbConnectionInternals)base.TestConnection).ADOTransaction;

			TestObjectCreator = new TestObjectCreator(Factory);
		}

		#region Helper Class

		class UpdateInvoicePaymentDetailsServiceForTest : UpdateInvoicePaymentDetailsService
		{
			readonly System.Data.Common.DbConnection TestSQLConnection;
			readonly System.Data.Common.DbTransaction TestTransaction;

			public UpdateInvoicePaymentDetailsServiceForTest(System.Data.Common.DbConnection sQLConnection, System.Data.Common.DbTransaction transaction)
			{
				TestSQLConnection = sQLConnection;
				TestTransaction = transaction;
			}

			protected override TransactionPaymentDataAccess CreateDataAccess(System.Data.Common.DbConnection connection)
			{
				return new TransactionPaymentDataAccess(TestSQLConnection, TestTransaction);
			}
		}

		#endregion

		#region Helper Method

		UpdateInvoicePaymentDetailsRequest CreateRequest(string ledger, string type, string orgCode, string transactionNum, decimal payment)
		{
			var request = new UpdateInvoicePaymentDetailsRequest();
			request.CompanyCode = "EDI";
			request.AccLedger = ledger;
			request.TransactionType = type;
			request.OrgCode = orgCode;
			request.TransactionNumber = transactionNum;
			request.AmountPaidInCompanyCurrency = payment;
			return request;
		}

		#endregion
	}
}
