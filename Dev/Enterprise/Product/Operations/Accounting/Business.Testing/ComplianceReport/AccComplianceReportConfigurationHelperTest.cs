using Enterprise.Registry.Business;
using NUnit.Framework;
using static Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport;
using static Enterprise.Core.Constants;
using BaseTablePrefixListCodes = Enterprise.Registry.Business.ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes;
using GroupingCodes = Enterprise.Registry.Business.ComplianceReportConfigurationLookups.ReportLineGroupingListCodes;

namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	public class AccComplianceReportConfigurationHelperTest : TestCase
	{
		public void TestIsAllTransactionConfig()
		{
			var config = new ComplianceReportConfiguration();
			config.ReportBaseTablePrefix = BaseTablePrefixListCodes.AllTransactions;
			Assert("IsAllTransactionConfig", config.IsAllTransactionConfig());
			Assert("IsServiceTaskQueuingConfig", config.IsServiceTaskQueuingConfig());
		}

		public void TestIsTransactionPaymentConfig()
		{
			var config = new ComplianceReportConfiguration();
			config.ReportLineGrouping = GroupingCodes.TransactionPayments;
			config.ReportBaseTablePrefix = BaseTablePrefixListCodes.TransactionHeader;
			Assert("IsTransactionPaymentConfig", config.IsTransactionPaymentConfig());
			Assert("IsServiceTaskQueuingConfig", config.IsServiceTaskQueuingConfig());
		}

		public void TestIsPTRS_ReportableConfig()
		{
			var config = new ComplianceReportConfiguration();
			config.ReportCode = "PTR";
			config.ReportLineGrouping = "PTR";
			config.ReportBaseTablePrefix = BaseTablePrefixListCodes.TransactionHeader;
			Assert("IsPTRS_ReportableConfig", config.IsPTRS_ReportableConfig());
			Assert("IsServiceTaskQueuingConfig", config.IsServiceTaskQueuingConfig());
		}

		public void TestIsPTRS_AllPaymentsConfig()
		{
			var config = new ComplianceReportConfiguration();
			config.ReportCode = "PTA";
			config.ReportLineGrouping = "PTA";
			config.ReportBaseTablePrefix = BaseTablePrefixListCodes.TransactionHeader;
			Assert("IsPTRS_AllPaymentsConfig", config.IsPTRS_AllPaymentsConfig());
			Assert("IsServiceTaskQueuingConfig", config.IsServiceTaskQueuingConfig());
		}

		public void TestIsPTRS2024_AllPaymentsConfig()
		{
			var config = new ComplianceReportConfiguration();
			config.ReportCode = "TCP";
			config.ReportLineGrouping = "PTA";
			config.ReportBaseTablePrefix = BaseTablePrefixListCodes.TransactionHeader;
			Assert("IsPTRS_AllPaymentsConfig2024", config.IsPTRS2024_AllPaymentsConfig());
			Assert("IsServiceTaskQueuingConfig", config.IsServiceTaskQueuingConfig());
		}

		public void TestIsPTRS2024_ReportableConfig()
		{
			var config = new ComplianceReportConfiguration();
			config.ReportCode = "PTS";
			config.ReportLineGrouping = "PTR";
			config.ReportBaseTablePrefix = BaseTablePrefixListCodes.TransactionHeader;
			Assert("IsPTRS_ReportableConfig2024", config.IsPTRS2024_ReportableConfig());
			Assert("IsServiceTaskQueuingConfig", config.IsServiceTaskQueuingConfig());
		}

		public void TestIsZMGermanyConfig()
		{
			var config = new ComplianceReportConfiguration();
			config.Country = CountryCodes.Germany;
			config.ReportCode = ReportTypes.ZMGermany;
			Assert("IsZMGermanyConfig", config.IsZMGermanyConfig());
			Assert("IsServiceTaskQueuingConfig", config.IsServiceTaskQueuingConfig());
		}

		public void TestIsIDEAConfig()
		{
			var config = new ComplianceReportConfiguration();
			config.Country = CountryCodes.Germany;
			config.ReportCode = ReportTypes.IDEA;
			Assert("IsIDEAConfig", config.IsIDEAConfig());
			Assert("NOT IsServiceTaskQueuingConfig", !config.IsServiceTaskQueuingConfig());
			Assert("IsServiceTaskOutputGeneratingConfig", config.IsServiceTaskOutputGeneratingConfig());
		}

		public void TestIsFECConfig()
		{
			var config = new ComplianceReportConfiguration();
			config.Country = CountryCodes.France;
			config.ReportCode = ReportTypes.FEC;
			Assert("IsFECConfig", config.IsFECConfig());
			Assert("NOT IsServiceTaskQueuingConfig", !config.IsServiceTaskQueuingConfig());
			Assert("IsServiceTaskOutputGeneratingConfig", config.IsServiceTaskOutputGeneratingConfig());
		}

		public void TestIsJPKV7MConfig()
		{
			var config = new ComplianceReportConfiguration();
			Assert("Wrong country, report code or table prefix", !config.IsJPKV7MConfig());

			config.Country = CountryCodes.Poland;
			config.ReportCode = ReportTypes.JPKV7M;
			config.ReportBaseTablePrefix = BaseTablePrefixListCodes.TransactionLine;

			Assert("IsJPKV7MConfig", config.IsJPKV7MConfig());

			config.Country = CountryCodes.France;
			Assert("Wrong country", !config.IsJPKV7MConfig());

			config.Country = CountryCodes.Poland;
			config.ReportCode = ReportTypes.FEC;
			Assert("Wrong report code", !config.IsJPKV7MConfig());

			config.ReportCode = ReportTypes.JPKV7M;
			config.ReportBaseTablePrefix = BaseTablePrefixListCodes.TransactionHeader;
			Assert("Wrong table prefix", !config.IsJPKV7MConfig());
		}

		public void TestIsQueueOnPostingConfig()
		{
			var config = new ComplianceReportConfiguration();
			config.Settings.AddNew();

			config.ReportBaseTablePrefix = BaseTablePrefixListCodes.TransactionLine;
			Assert("Any report based on TransactionLine and having Settins", config.IsQueueOnPostingConfig());

			config.ReportBaseTablePrefix = BaseTablePrefixListCodes.TransactionHeader;
			Assert("Any report based on TransactionHeader and having Settins", config.IsQueueOnPostingConfig());

			config.ReportLineGrouping = GroupingCodes.TransactionPayments;
			Assert("IsTransactionPaymentConfig", config.IsTransactionPaymentConfig());
			Assert("IsServiceTaskQueuingConfig", config.IsServiceTaskQueuingConfig());
			Assert("But not one which IsTransactionPayments and also IsServiceTaskQueuingConfig", !config.IsQueueOnPostingConfig());
		}

		public void TestGetStatusForReportOutputGeneration()
		{
			var config = new ComplianceReportConfiguration();
			config.Country = CountryCodes.Poland;
			config.ReportCode = ReportTypes.JPKV7M;
			config.ReportBaseTablePrefix = BaseTablePrefixListCodes.TransactionLine;

			Assert("NOT IsServiceTaskQueuingConfig", !config.IsServiceTaskQueuingConfig());
			Assert("IsServiceTaskOutputGeneratingConfig", config.IsServiceTaskOutputGeneratingConfig());
			AssertEquals("GetStatusForReportOutputGeneration", Status.ReportGenerated, config.GetStatusForReportOutputGeneration());

			config.Country = CountryCodes.Germany;
			config.ReportCode = ReportTypes.ZMGermany;

			Assert("IsServiceTaskQueuingConfig", config.IsServiceTaskQueuingConfig());
			Assert("NOT IsServiceTaskOutputGeneratingConfig", !config.IsServiceTaskOutputGeneratingConfig());
			AssertEquals("GetStatusForReportOutputGeneration", string.Empty, config.GetStatusForReportOutputGeneration());

			config.ReportBaseTablePrefix = BaseTablePrefixListCodes.AllTransactions;
			config.ReportCode = ReportTypes.IDEA;

			Assert("IsServiceTaskQueuingConfig", config.IsServiceTaskQueuingConfig());
			Assert("IsServiceTaskOutputGeneratingConfig", config.IsServiceTaskOutputGeneratingConfig());
			AssertEquals("GetStatusForReportOutputGeneration", Status.ReportDataQueued, config.GetStatusForReportOutputGeneration());

			config.Country = CountryCodes.France;
			config.ReportCode = ReportTypes.FEC;

			Assert("IsServiceTaskQueuingConfig", config.IsServiceTaskQueuingConfig());
			Assert("IsServiceTaskOutputGeneratingConfig", config.IsServiceTaskOutputGeneratingConfig());
			AssertEquals("GetStatusForReportOutputGeneration", Status.ReportDataQueued, config.GetStatusForReportOutputGeneration());
		}
	}
}
