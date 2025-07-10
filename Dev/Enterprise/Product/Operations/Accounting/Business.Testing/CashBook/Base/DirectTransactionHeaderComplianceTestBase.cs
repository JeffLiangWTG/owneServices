using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.Accounting.Business.Testing
{
	public abstract class DirectTransactionHeaderComplianceTestBase : TestCaseWithFactory
	{
		public void TestSupportQueueingForComplianceReports()
		{
			var transaction = GetSampleTransaction();
			var supporter = transaction as ISupportQueueingForComplianceReports;
			AssertNotNull(supporter);
			AssertNotNull(supporter.ComplianceMatchingRule);
			Assert(supporter.CheckIsValidForQueueing(supporter.Factory));
			AssertType(typeof(ComplianceReportTransactionQueuer<DirectTransactionHeaderBase>), supporter.Queuer);
		}

		public void TestEvaluateComplianceRule()
		{
			var transaction = GetSampleTransaction();
			var supporter = transaction as IEvaluateComplianceRule;
			AssertEquals(supporter.Company, transaction.Company);
			AssertEquals(ZString.Empty, supporter.ComplianceSubType);
			Assert(!supporter.EmptyLedgerMatchesAll);
			Assert(!supporter.EmptyTransactionTypeMatchesAll);
			AssertEquals(transaction.Header, supporter.Header);
			AssertEquals(false, supporter.IsDisbursementOrFinal);
			AssertEquals(false, supporter.IsAmendingTransaction);
			AssertEquals(transaction.IsReversalTransaction, supporter.IsReversalTransaction);
			AssertEquals(false, supporter.IsSelfBillingInvoice);
			AssertEquals(transaction.AH_Ledger, supporter.Ledger);
			AssertEquals(transaction.AH_TransactionType, supporter.TransactionType);
			AssertEquals(660.0M, supporter.LocalTotalAmount);

			AssertNotNull(supporter.TaxTransactions);
			Assert(!supporter.TaxTransactions.Any());

			// Tax Transactions should not be created for the DirectTransactionHeaderBase but we simulate their creation
			var creator = new TaxFrameworkTestObjectCreator(Factory);
			creator.CreateTaxSystem("TS");
			var taxTransaction1 = creator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters() { TransactionHeader = transaction });
			var taxTransaction2 = creator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters() { TransactionHeader = transaction });

			AssertNotNull(supporter.TaxTransactions);
			Assert("Created Tax Transactions are ignored", !supporter.TaxTransactions.Any());
		}

		public void TestQueueForComplianceReport_TransactionHeader()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				ConfigureRegistryForCashBook(AccTransactionHeaderSchema.Constants.Prefix
					, CreateTaxRegistration().Code
					, reportLineGrouping: ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.TaxReporting);

				var transaction = GetSampleTransaction();

				Factory.Save();

				AssertQueueDataForAH(transaction.PK);
			}
		}

		public void TestQueueForComplianceReport_TransactionLine()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				ConfigureRegistryForCashBook(AccTransactionLinesSchema.Constants.Prefix
					, CreateTaxRegistration().Code
					, reportLineGrouping: ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.TaxReporting);

				var transaction = GetSampleTransaction();

				Factory.Save();

				AssertQueueDataForAL(transaction.PK);
			}
		}

		public void TestCheckIsValidForQueueing()
		{
			var transaction1 = GetSampleTransaction();
			AssertIsValidForQueueingBeforeSaving(transaction1);

			Factory.Save();
			AssertIsValidForQueueingAfterSaving(transaction1);
		}

		protected abstract DirectTransactionHeaderBase GetSampleTransaction();

		protected virtual void AssertIsValidForQueueingBeforeSaving(DirectTransactionHeaderBase transaction) => Assert("Should allow queueing", (transaction as ISupportQueueingForComplianceReports).CheckIsValidForQueueing(transaction.Factory));

		protected virtual void AssertIsValidForQueueingAfterSaving(DirectTransactionHeaderBase transaction) => Assert("Should Not allow queueing", !(transaction as ISupportQueueingForComplianceReports).CheckIsValidForQueueing(transaction.Factory));

		protected virtual void AssertQueueDataForAH(ZGuid transactionPK)
		{
			var reportQueuedHeaders = ObjectCreator.LoadReportQueuesByParentID(transactionPK);
			AssertEquals("Queue records added", 1, reportQueuedHeaders.Count);

			var abcReport = reportQueuedHeaders.Cast<DynamicBusinessObject>().FirstOrDefault(x => x[AccTransactionComplianceReportQueueSchema.ACQ_ReportType].ToString() == "CBC");
			AssertNotNull(abcReport);
			AssertEquals("ACQ_GC_Company", GlbCompany.CurrentCompany.PK.ToString(), abcReport[AccTransactionComplianceReportQueueSchema.ACQ_GC_Company].ToString());
			AssertEquals("ACQ_GB_Branch", GlbBranch.CurrentBranch.PK.ToString(), abcReport[AccTransactionComplianceReportQueueSchema.ACQ_GB_Branch].ToString());
			AssertEquals("ACQ_ParentTableCode", AccTransactionHeaderSchema.Constants.Prefix, abcReport[AccTransactionComplianceReportQueueSchema.ACQ_ParentTableCode].ToString());
		}

		protected virtual void AssertQueueDataForAL(ZGuid transactionPK)
		{
			var reportQueuedLines = ObjectCreator.LoadReportQueuedLinesByHeaderPK(transactionPK);
			AssertEquals("Queue records added", 2, reportQueuedLines.Count);

			var abcReportLines = reportQueuedLines.Cast<DynamicBusinessObject>().Where(x => x[AccTransactionComplianceReportQueueSchema.ACQ_ReportType].ToString() == "CBC");
			AssertNotNull(abcReportLines);

			foreach (var line in abcReportLines)
			{
				AssertEquals("ACQ_GC_Company", GlbCompany.CurrentCompany.PK.ToString(), line[AccTransactionComplianceReportQueueSchema.ACQ_GC_Company].ToString());
				AssertEquals("ACQ_GB_Branch", GlbBranch.CurrentBranch.PK.ToString(), line[AccTransactionComplianceReportQueueSchema.ACQ_GB_Branch].ToString());
				AssertEquals("ACQ_ParentTableCode", AccTransactionLinesSchema.Constants.Prefix, line[AccTransactionComplianceReportQueueSchema.ACQ_ParentTableCode].ToString());
			}
		}

		void ConfigureRegistryForCashBook(string tablePrefix, string taxRegistrationCode, string reportLineGrouping = "", ZGuid? recipientOrgPK = null)
		{
			var configCollection = new ComplianceReportConfigurationCollection();

			var config = ObjectCreator.CreateConfigurationForComplianceReport(
				configCollection
				, GlbCompany.CurrentCompany.Country.Code
				, "CBC"
				, "CBC Compliance Report"
				, tablePrefix
				, reportLineGrouping: reportLineGrouping
				, goodsAndService: ""
				, "RNG"
				, taxRegistrationType: taxRegistrationCode
				, recipientOrgPK ?? ZGuid.Empty
				, false);

			ObjectCreator.CreateConfigurationSettingsForComplianceReport(config
				, LedgerTypes.CashBook
				, TransactionTypes.DirectPayment
				, taxInvoiceRule: TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT
				, disbursementRule: DisbursementRuleCodes.AllTransactions
				, originalRule: OriginalRuleCodes.AllTransactions);

			ObjectCreator.CreateConfigurationSettingsForComplianceReport(config
				, LedgerTypes.CashBook
				, TransactionTypes.DirectReceipt
				, taxInvoiceRule: TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT
				, disbursementRule: DisbursementRuleCodes.AllTransactions
				, originalRule: OriginalRuleCodes.AllTransactions);

			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, configCollection);
		}

		ICodeDescription CreateTaxRegistration()
		{
			var taxRegistrationCodes = new OrgCodeLists().CustomsCodes_List(RefCountry.LoadFromCountryCode(Factory, GlbCompany.CurrentCompany.Country.Code));
			var taxRegistration = taxRegistrationCodes.ToArray().FirstOrDefault(x => !string.IsNullOrEmpty(x.Code));
			AssertNotNull("There should be a Tax Registration", taxRegistration);
			return taxRegistration;
		}

		protected TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;
	}
}
