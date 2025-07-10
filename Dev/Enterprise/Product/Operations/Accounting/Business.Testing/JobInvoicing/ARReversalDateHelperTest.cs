using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class ARReversalDateHelperTest : TestCaseWithFactory
	{
		public void TestGetConsumer()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ARInvoice invoice = creator.CreateARInvoice<ARInvoice>("00001000", creator.AUD, 1.0m, creator.ABIGAS);
			invoice.GenerateReverseTransaction(true);
			InvoicingBase reversedInvoice = invoice.ReverseInvoice;
			AssertNotNull("reversedInvoice", reversedInvoice);

			bool isConsumerConsol;
			IJobInvoicingPlugIn consumer = ARReversalDateHelper.GetConsumerForTest(reversedInvoice, out isConsumerConsol);
			AssertNull("consumer should be null", consumer);
			AssertEquals("isConsumerConsol", false, isConsumerConsol);

			var shipment = creator.CreateShipment("S00001000");
			Job job = creator.CreateJob(shipment, false);
			reversedInvoice.AH_JH = job.PK;
			Factory.Save();

			consumer = ARReversalDateHelper.GetConsumerForTest(reversedInvoice, out isConsumerConsol);
			AssertNotNull("consumer", consumer);
			AssertEquals("consumer", shipment, consumer);
			AssertEquals("isConsumerConsol", false, isConsumerConsol);

			var consol = creator.CreateConsol("AUSYD", "NZAKL", "C00001000");
			reversedInvoice.AH_JH = ZGuid.Empty;
			reversedInvoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			Factory.Save();

			consumer = ARReversalDateHelper.GetConsumerForTest(reversedInvoice, out isConsumerConsol);
			AssertNotNull("consumer", consumer);
			AssertEquals("consumer", consol, consumer);
			AssertEquals("isConsumerConsol", true, isConsumerConsol);
		}

		[TestDate(2012, 03, 08)]
		public void TestShowReversalDatesForm()
		{
			AssertDefaultReversalDatesCore(true, true, true);
		}

		[TestDate(2012, 03, 08)]
		public void TestDefaultReversalDates()
		{
			AssertDefaultReversalDatesCore(false, true, true);
		}

		[TestDate(2012, 03, 08)]
		public void TestShowReversalDatesForm_OverridePostDateSetToFalse()
		{
			AssertDefaultReversalDatesCore(true, false, true);
		}

		[TestDate(2012, 03, 08)]
		public void TestDefaultReversalDates_OverridePostDateSetToFalse()
		{
			AssertDefaultReversalDatesCore(false, false, true);
		}

		[TestDate(2012, 03, 08)]
		public void TestDefaultReversalDates_OverridePostDateSetToFalse_DontAllowPostDateOverride()
		{
			AssertDefaultReversalDatesCore(false, false, false);
		}

		void AssertDefaultReversalDatesCore(bool expectFormShown, bool overridePostDate, bool setOverrideAndTodayOptions)
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);

			Job testJob = testObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			ForwardingShipment forwardingShipment = testJob.PlugInData as ForwardingShipment;
			AssertNotNull("Forwarding Shipment should not be null", forwardingShipment);

			testJob = Factory.Load<Job>(testJob.PK);
			testObjectCreator.CreateCharge(testJob, testObjectCreator.CC1, "Desc",
				testObjectCreator.AUD, 1000m, testObjectCreator.Creditor1,
				testObjectCreator.AUD, 1000m, testObjectCreator.LocalClient);
			Factory.Save();

			InvoicingPostManager postManager = new InvoicingPostManager(testJob);
			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.All);
			Factory.Save();

			InvoicingBaseCollection invoicingBaseCollection = new InvoicingBaseCollection(Factory);
			invoicingBaseCollection.AddRange(transactions.GetAllARInvoicesAndCreditNotes());

			AssertEquals("invoicingBaseCollection.Count", 1, invoicingBaseCollection.Count);
			InvoicingBase invoice = invoicingBaseCollection[0];
			invoice.AH_PostDate = new ZDateTime(2012, 02, 28);

			invoice.GenerateReverseTransaction(true);
			InvoicingBase transaction = invoice.ReverseInvoice;
			AssertNotNull("transaction", transaction);

			AssertEquals("AH_InvoiceDate", new ZDateTime(2012, 03, 08), transaction.AH_InvoiceDate.Date);
			AssertEquals("AH_PostDate", new ZDateTime(2012, 03, 08), transaction.AH_PostDate.Date);

			var reversedCollection = new TransactionHeaderCollection(Factory);
			reversedCollection.Add(transaction);

			BackDateInvoicesConfiguration configuration = new BackDateInvoicesConfiguration();
			configuration.InvoiceDateConfigurationCollection.RemoveAll();
			testObjectCreator.AddInvoiceDateConfiguration(configuration, "SHP", "ALL", "ALL", "ALL",
				InvoiceDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorMonth,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.InvoiceAddDate,
				setOverrideAndTodayOptions, setOverrideAndTodayOptions);
			configuration.OverridePostDate = overridePostDate;
			configuration.DefaultPostDateFromInvoiceDate = true;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration);

			ZFormModaliser.LastFormShownDialogForTest = null;

			if (expectFormShown)
			{
				ARReversalDateHelper.DefaultReversalDatesJobRelated(reversedCollection, forwardingShipment, showReversalDateForm, false);
			}
			else
			{
				ARReversalDateHelper.DefaultReversalDatesJobRelatedForTest(transaction, forwardingShipment, false);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			}

			AssertEquals("AH_InvoiceDate", new ZDateTime(2012, 02, 29), transaction.AH_InvoiceDate);

			if (setOverrideAndTodayOptions)
			{
				AssertEquals("AH_PostDate", new ZDateTime(2012, 02, 29), transaction.AH_PostDate);
			}
			else
			{
				AssertEquals("AH_PostDate", new ZDateTime(2012, 03, 08), transaction.AH_PostDate);
			}
		}

		bool? showReversalDateForm(TransactionHeaderCollection collection, ChangeTransactionDatesBusinessObject changeTransactionDatesBusinessObject)
		{
			return true;
		}

		[TestDate(2012, 03, 03)]
		public void TestDefaultReversalDatesIfNonJobRelatedARTransaction()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			creator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);
			ARInvoice invoice = creator.CreateARInvoice<ARInvoice>("00001000", creator.AUD, 1.0m, creator.ABIGAS);
			ZDateTime date = new ZDateTime(2012, 02, 15);
			invoice.AH_InvoiceDate = date;
			invoice.AH_PostDate = date.AddDays(1);
			invoice.GenerateReverseTransaction(true);
			InvoicingBase reversedInvoice = invoice.ReverseInvoice;
			AssertNotNull("reversedInvoice", reversedInvoice);

			AccountingConfigurationRegistry.Instance.AllowARInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionInvoiceDateOrCurrentDate);
			AccountingConfigurationRegistry.Instance.AllowARPostDateToDefaultToTheOriginalTransactionInvoiceDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionInvoiceDateOrCurrentDate);

			ARReversalDateHelper.DefaultReversalDatesIfNonJobRelatedARTransactionForTest(reversedInvoice);

			AssertEquals(date, reversedInvoice.AH_InvoiceDate);
			AssertEquals(date.AddDays(1), reversedInvoice.AH_PostDate);

			AccountingConfigurationRegistry.Instance.AllowARInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionInvoiceDateOrFirstDayOfFirstOpenPeriod);
			AccountingConfigurationRegistry.Instance.AllowARPostDateToDefaultToTheOriginalTransactionInvoiceDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionInvoiceDateOrFirstDayOfFirstOpenPeriod);

			ARReversalDateHelper.DefaultReversalDatesIfNonJobRelatedARTransactionForTest(reversedInvoice);

			AssertEquals(date, reversedInvoice.AH_InvoiceDate);
			AssertEquals(date.AddDays(1), reversedInvoice.AH_PostDate);

			AccountingConfigurationRegistry.Instance.AllowARInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionPostDateOrCurrentDate);
			AccountingConfigurationRegistry.Instance.AllowARPostDateToDefaultToTheOriginalTransactionInvoiceDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionPostDateOrCurrentDate);

			ARReversalDateHelper.DefaultReversalDatesIfNonJobRelatedARTransactionForTest(reversedInvoice);

			AssertEquals(invoice.AH_PostDate, reversedInvoice.AH_InvoiceDate);
			AssertEquals(invoice.AH_PostDate, reversedInvoice.AH_PostDate);

			AccountingConfigurationRegistry.Instance.AllowARInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionPostDateOrFirstDayOfFirstOpenPeriod);
			AccountingConfigurationRegistry.Instance.AllowARPostDateToDefaultToTheOriginalTransactionInvoiceDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionPostDateOrFirstDayOfFirstOpenPeriod);

			ARReversalDateHelper.DefaultReversalDatesIfNonJobRelatedARTransactionForTest(reversedInvoice);

			AssertEquals(invoice.AH_PostDate, reversedInvoice.AH_InvoiceDate);
			AssertEquals(invoice.AH_PostDate, reversedInvoice.AH_PostDate);

			CloseSubLedger(201201);
			CloseSubLedger(201202);

			AccountingConfigurationRegistry.Instance.AllowARInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionInvoiceDateOrCurrentDate);
			AccountingConfigurationRegistry.Instance.AllowARPostDateToDefaultToTheOriginalTransactionInvoiceDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionInvoiceDateOrCurrentDate);

			ARReversalDateHelper.DefaultReversalDatesIfNonJobRelatedARTransactionForTest(reversedInvoice);

			AssertEquals(ZDateTime.Today, reversedInvoice.AH_InvoiceDate);
			AssertEquals(ZDateTime.Today, reversedInvoice.AH_PostDate);

			AccountingConfigurationRegistry.Instance.AllowARInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionPostDateOrCurrentDate);
			AccountingConfigurationRegistry.Instance.AllowARPostDateToDefaultToTheOriginalTransactionInvoiceDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionPostDateOrCurrentDate);

			ARReversalDateHelper.DefaultReversalDatesIfNonJobRelatedARTransactionForTest(reversedInvoice);

			AssertEquals(ZDateTime.Today, reversedInvoice.AH_InvoiceDate);
			AssertEquals(ZDateTime.Today, reversedInvoice.AH_PostDate);

			AccountingConfigurationRegistry.Instance.AllowARInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionInvoiceDateOrFirstDayOfFirstOpenPeriod);
			AccountingConfigurationRegistry.Instance.AllowARPostDateToDefaultToTheOriginalTransactionInvoiceDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionInvoiceDateOrFirstDayOfFirstOpenPeriod);

			ARReversalDateHelper.DefaultReversalDatesIfNonJobRelatedARTransactionForTest(reversedInvoice);

			ZDateTime firstDayOfFirstOpenPeriod = new ZDateTime(2012, 03, 01);
			AssertEquals(firstDayOfFirstOpenPeriod, reversedInvoice.AH_InvoiceDate);
			AssertEquals(firstDayOfFirstOpenPeriod, reversedInvoice.AH_PostDate);

			AccountingConfigurationRegistry.Instance.AllowARInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionPostDateOrFirstDayOfFirstOpenPeriod);
			AccountingConfigurationRegistry.Instance.AllowARPostDateToDefaultToTheOriginalTransactionInvoiceDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionPostDateOrFirstDayOfFirstOpenPeriod);

			ARReversalDateHelper.DefaultReversalDatesIfNonJobRelatedARTransactionForTest(reversedInvoice);

			AssertEquals(firstDayOfFirstOpenPeriod, reversedInvoice.AH_InvoiceDate);
			AssertEquals(firstDayOfFirstOpenPeriod, reversedInvoice.AH_PostDate);

			AccountingConfigurationRegistry.Instance.AllowARInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ReversalDefaultFromOriginalTransactionDate.TodaysDate);
			AccountingConfigurationRegistry.Instance.AllowARPostDateToDefaultToTheOriginalTransactionInvoiceDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ReversalDefaultFromOriginalTransactionDate.TodaysDate);

			ARReversalDateHelper.DefaultReversalDatesIfNonJobRelatedARTransactionForTest(reversedInvoice);

			AssertEquals(ZDateTime.Today, reversedInvoice.AH_InvoiceDate);
			AssertEquals(ZDateTime.Today, reversedInvoice.AH_PostDate);
		}

		[TestDate(2012, 03, 03)]
		public void TestDefaultReversalDatesIfNonJobRelatedARTransaction_PostDateOnReversalCantBeBeforeOriginalPostDate()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			creator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);
			ARInvoice invoice = creator.CreateARInvoice<ARInvoice>("00001000", creator.AUD, 1.0m, creator.ABIGAS);
			ZDateTime invoiceDate = new ZDateTime(2012, 02, 15);
			invoice.AH_InvoiceDate = invoiceDate;
			ZDateTime postDate = new ZDateTime(2012, 02, 20);
			invoice.AH_PostDate = postDate;
			invoice.GenerateReverseTransaction(true);
			InvoicingBase reversedInvoice = invoice.ReverseInvoice;
			AssertNotNull("reversedInvoice", reversedInvoice);

			AccountingConfigurationRegistry.Instance.AllowARInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionInvoiceDateOrCurrentDate);
			AccountingConfigurationRegistry.Instance.AllowARPostDateToDefaultToTheOriginalTransactionInvoiceDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionInvoiceDateOrCurrentDate);

			ARReversalDateHelper.DefaultReversalDatesIfNonJobRelatedARTransactionForTest(reversedInvoice);

			AssertEquals(invoiceDate, reversedInvoice.AH_InvoiceDate);
			AssertEquals(postDate, reversedInvoice.AH_PostDate);
		}

		[TestDate(2012, 03, 03)]
		public void TestDefaultReversalDatesWhenReversalRuleIsNonStandard()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);

			Job testJob = testObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			ForwardingShipment forwardingShipment = testJob.PlugInData as ForwardingShipment;
			AssertNotNull("Forwarding Shipment should not be null", forwardingShipment);

			testJob = Factory.Load<Job>(testJob.PK);
			testObjectCreator.CreateCharge(testJob, testObjectCreator.CC1, "Desc",
				testObjectCreator.AUD, 1000m, testObjectCreator.Creditor1,
				testObjectCreator.AUD, 1000m, testObjectCreator.LocalClient);
			Factory.Save();

			InvoicingPostManager postManager = new InvoicingPostManager(testJob);
			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.All);
			Factory.Save();

			InvoicingBaseCollection invoicingBaseCollection = new InvoicingBaseCollection(Factory);
			invoicingBaseCollection.AddRange(transactions.GetAllARInvoicesAndCreditNotes());

			AssertEquals("invoicingBaseCollection.Count", 1, invoicingBaseCollection.Count);
			InvoicingBase invoice = invoicingBaseCollection[0];
			invoice.AH_PostDate = new ZDateTime(2012, 02, 28);
			invoice.AH_InvoiceDate = new ZDateTime(2012, 02, 28);

			invoice.GenerateReverseTransaction(true);
			InvoicingBase transaction = invoice.ReverseInvoice;
			AssertNotNull("transaction", transaction);

			AssertReversalDateWithReversalRule(InvoiceDateConfigurationLookups.ReversalRuleCodes.DefaultFromOriginalTransactionInvoiceDateOrCurrentDate, forwardingShipment, transaction, new ZDateTime(2012, 02, 28));
			AssertReversalDateWithReversalRule(InvoiceDateConfigurationLookups.ReversalRuleCodes.DefaultFromOriginalTransactionInvoiceDateOrFirstDayOfFirstOpenPeriod, forwardingShipment, transaction, new ZDateTime(2012, 02, 28));
			AssertReversalDateWithReversalRule(InvoiceDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules, forwardingShipment, transaction, new ZDateTime(2012, 08, 25));
		}

		void AssertReversalDateWithReversalRule(string reversalRule, ForwardingShipment forwardingShipment, InvoicingBase transaction, ZDateTime expectedDate)
		{
			transaction.AH_PostDate = new ZDateTime(2012, 08, 25);
			transaction.AH_InvoiceDate = new ZDateTime(2012, 08, 25);
			var reversedCollection = new TransactionHeaderCollection(Factory);
			reversedCollection.Add(transaction);

			BackDateInvoicesConfiguration configuration = new BackDateInvoicesConfiguration();
			configuration.OverridePostDate = true;
			configuration.DefaultPostDateFromInvoiceDate = true;
			configuration.InvoiceDateConfigurationCollection.RemoveAll();
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.AddInvoiceDateConfiguration(configuration, "SHP", "ALL", "ALL", "ALL",
				InvoiceDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.InvoiceAddDate,
				true, true, reversalRule);
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration);

			ARReversalDateHelper.DefaultReversalDatesJobRelated(reversedCollection, forwardingShipment, showReversalDateForm, false);
			AssertEquals("AH_InvoiceDate", expectedDate, transaction.AH_InvoiceDate);
			AssertEquals("AH_PostDate", expectedDate, transaction.AH_PostDate);
		}

		void CloseSubLedger(ZInt period)
		{
			ZQuery query = new ZQuery(AccPeriodManagementSchema.AM_Period, period);
			query.AddToFilter(AccPeriodManagementSchema.AM_GC_Company, GlbCompany.CurrentCompany.PK);
			AccPeriodManagement periodManagement = Factory.LoadTop1<AccPeriodManagement>(query);
			periodManagement.AM_IsSubLedgerClosed = true;
		}
	}
}
