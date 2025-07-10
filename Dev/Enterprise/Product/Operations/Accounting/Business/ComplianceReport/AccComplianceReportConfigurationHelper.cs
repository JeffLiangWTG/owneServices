using System.Linq;
using Enterprise.Registry.Business;
using static Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport;
using static Enterprise.Core.Constants;
using BaseTablePrefixListCodes = Enterprise.Registry.Business.ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes;
using GroupingCodes = Enterprise.Registry.Business.ComplianceReportConfigurationLookups.ReportLineGroupingListCodes;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public static class AccComplianceReportConfigurationHelper
	{
		public static bool IsAllTransactionConfig(this ComplianceReportConfiguration config) => config != null && config.ReportBaseTablePrefix == BaseTablePrefixListCodes.AllTransactions;
		public static bool IsTransactionPaymentConfig(this ComplianceReportConfiguration config) => config != null && config.ReportLineGrouping == GroupingCodes.TransactionPayments && config.ReportBaseTablePrefix == BaseTablePrefixListCodes.TransactionHeader;
		public static bool IsPTRS_ReportableConfig(this ComplianceReportConfiguration config) => config != null && config.ReportLineGrouping == GroupingCodes.PaymentTimesSmallBusinessReportable && config.ReportCode == AccountingConstants.ComplianceReportTypes.PaymentTimesSmallBusinessReportType && config.ReportBaseTablePrefix == BaseTablePrefixListCodes.TransactionHeader;
		public static bool IsPTRS_AllPaymentsConfig(this ComplianceReportConfiguration config) => config != null && config.ReportLineGrouping == GroupingCodes.PaymentTimesAll && config.ReportCode == AccountingConstants.ComplianceReportTypes.PaymentTimesAllPaymentsReportType && config.ReportBaseTablePrefix == BaseTablePrefixListCodes.TransactionHeader;
		public static bool IsPTRS2024_ReportableConfig(this ComplianceReportConfiguration config) => config != null && config.ReportLineGrouping == GroupingCodes.PaymentTimesSmallBusinessReportable && config.ReportCode == AccountingConstants.ComplianceReportTypes.PaymentTimesSmallBusiness2024ReportType && config.ReportBaseTablePrefix == BaseTablePrefixListCodes.TransactionHeader;
		public static bool IsPTRS2024_AllPaymentsConfig(this ComplianceReportConfiguration config) => config != null && config.ReportLineGrouping == GroupingCodes.PaymentTimesAll && config.ReportCode == AccountingConstants.ComplianceReportTypes.PaymentTimesAllPayments2024ReportType && config.ReportBaseTablePrefix == BaseTablePrefixListCodes.TransactionHeader;
		public static bool IsZMGermanyConfig(this ComplianceReportConfiguration config) => config != null && config.Country == CountryCodes.Germany && config.ReportCode == ReportTypes.ZMGermany;
		public static bool IsIDEAConfig(this ComplianceReportConfiguration config) => config != null && config.Country == CountryCodes.Germany && config.ReportCode == ReportTypes.IDEA;
		public static bool IsFECConfig(this ComplianceReportConfiguration config) => config != null && config.Country == CountryCodes.France && config.ReportCode == ReportTypes.FEC;
		public static bool IsJPKV7MConfig(this ComplianceReportConfiguration config) => config != null && config.Country == CountryCodes.Poland && config.ReportCode == ReportTypes.JPKV7M && config.ReportBaseTablePrefix == BaseTablePrefixListCodes.TransactionLine;

		public static bool IsQueueOnPostingConfig(this ComplianceReportConfiguration config) => config != null && config.Settings.Any()
			&& (config.ReportBaseTablePrefix == BaseTablePrefixListCodes.TransactionHeader || config.ReportBaseTablePrefix == BaseTablePrefixListCodes.TransactionLine)
			&& !config.IsServiceTaskQueuingConfig();

		public static bool IsServiceTaskQueuingConfig(this ComplianceReportConfiguration config) => config != null &&
			(config.IsAllTransactionConfig()
			|| config.IsTransactionPaymentConfig()
			|| config.IsPTRS_ReportableConfig()
			|| config.IsPTRS_AllPaymentsConfig()
			|| config.IsPTRS2024_ReportableConfig()
			|| config.IsPTRS2024_AllPaymentsConfig()
			|| config.IsZMGermanyConfig());

		public static bool IsServiceTaskOutputGeneratingConfig(this ComplianceReportConfiguration config) => config != null && (config.IsIDEAConfig() || config.IsFECConfig() || config.IsJPKV7MConfig());

		public static string GetStatusForReportOutputGeneration(this ComplianceReportConfiguration config) =>
			config != null && config.IsJPKV7MConfig()
				? Status.ReportGenerated
				: config.IsServiceTaskOutputGeneratingConfig()
					? Status.ReportDataQueued
					: string.Empty;
	}
}
