using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SupportEdwDataSourceReportCollection))]
	public class SupportEdwDataSourceReportCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<SupportEdwDataSourceReportCollection>
	{
		protected override SupportEdwDataSourceReportCollection GetCollectionToTest()
		{
			return new SupportEdwDataSourceReportCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SupportEdwDataSourceReport();
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		public void TestGetDefault()
		{
			var accountingRegistryProvider = ObjectFactory.Get<IAccountingRegistryProvider>();
			AssertEquals("PreCondition: EnableGenerateJournalEntriesForPostedAccountingTransactions should be false.", false, accountingRegistryProvider.EnableGenerateJournalEntriesForPostedAccountingTransactions);

			var defaultReports = SupportEdwDataSourceReportCollection.GetDefault();
			AssertEquals("Default defaultReports should have 0 items.", 0, defaultReports.Count);

			accountingRegistryProvider.EnableGenerateJournalEntriesForPostedAccountingTransactions = true;

			defaultReports = SupportEdwDataSourceReportCollection.GetDefault();
			var distinctBusinessContexts = defaultReports.Select(x => ((SupportEdwDataSourceReport)x).BusinessContext).Distinct();
			var defaultReportNames = defaultReports.Select(x => ((SupportEdwDataSourceReport)x).ReportName).Distinct();

			var expectedReportNames = new List<ZString>
			{
				"Balance Sheet",
				"Balance Sheet Periods Analysis",
				"China Balance Sheet",
				"China Cash Flow Statement",
				"China GL Summary",
				"China GL Transaction",
				"China Profit and Loss - Monthly",
				"China Profit and Loss - Yearly",
				"China Reports Breakdown By Categories",
				"China Profit Appropriation Statement",
				"China Statement of Provision for Impairment of Assets",
				"China Statement of Shareholders' Equity",
				"China Trial Balance",
				"China VAT Detailed Report",
				"Multi-Language Balance Sheet",
				"Multi-Language Profit and Loss",
				"Multi-Language Transaction",
				"Multi-Language Trial Balance",
				"Eight Column Balance Report",
				"Profit and Loss - List of Movements by Account, Period, Branch and Department",
				"Profit and Loss by Branch",
				"Profit and Loss by Department",
				"Profit and Loss Periods Analysis",
				"Profit And Loss Report",
				"Transactions",
				"Trial Balance",
				"Trial Balance - List of Movements by Account  Period  Branch and Department",
				"Trial Balance Periods Analysis",
				"China GL Accounts Balances"
			};

			CombineAssertions(() =>
			{
				AssertEquals("Default Report should have 29 items.", 29, defaultReportNames.Count());
				AssertEquals("Default BusinessContext should have 1 items.", 1, distinctBusinessContexts.Count());
				AssertEquals("Default BusinessContext should be 'RepGLReports'.", "RepGLReports", distinctBusinessContexts.First());
				AssertContainsExactElementsInAnyOrder("Expected Report Name should be in the default list.", expectedReportNames, defaultReportNames);
			});
		}
	}
}
