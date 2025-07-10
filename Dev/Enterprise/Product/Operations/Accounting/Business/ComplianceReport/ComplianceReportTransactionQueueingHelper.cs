using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business
{
	internal class ComplianceReportTransactionQueueingHelper
	{
		internal static IEnumerable<ComplianceReportConfiguration> GetComplianceReportsOfCompanyCountry() =>
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value.Cast<ComplianceReportConfiguration>().Where(x => x.Country == GlbCompany.CurrentCompany.Country.Code);

		internal static ComplianceReportConfigurationSetting GetMatchingReportComplianceRuleSetting(ComplianceSubTypeRule complianceSubTypeRule, ComplianceReportConfiguration report) =>
			report.Settings.Cast<ComplianceReportConfigurationSetting>().FirstOrDefault(setting => complianceSubTypeRule.IsTransactionMatchingRuleWithComplianceSubType(setting));

		internal static string[] GetExemptReportCodes(TransactionHeader transaction)
		{
			var docUsage = transaction.AH_Ledger == LedgerTypes.AccountsReceivable ? JobRequiredDocument.DocUsage.Debtor :
				transaction.AH_Ledger == LedgerTypes.AccountsPayable ? JobRequiredDocument.DocUsage.Creditor : string.Empty;

			var exemptReportQuery = new ZQuery(JobRequiredDocumentSchema.EQ_ParentID, transaction.AH_OH);
			exemptReportQuery.AddToFilter(JobRequiredDocumentSchema.EQ_ParentTableCode, OrgHeaderSchema.Constants.Prefix);
			exemptReportQuery.AddToFilter(JobRequiredDocumentSchema.EQ_DocCategory, ReferenceTypes.ComplianceReport);
			exemptReportQuery.AddToFilter(JobRequiredDocumentSchema.EQ_RN_NKRelatedCountry, GlbCompany.CurrentCompany.Country.Code);
			exemptReportQuery.AddToFilter(JobRequiredDocumentSchema.EQ_DocUsage, docUsage);
			exemptReportQuery.AddToFilter(JobRequiredDocumentSchema.EQ_DateReceived, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, transaction.AH_PostDate);
			exemptReportQuery.AddToFilter(JobRequiredDocumentSchema.EQ_ValidToDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, transaction.AH_PostDate);

			var exemptReports = transaction.Factory.Load<JobRequiredDocument>(exemptReportQuery);
			return exemptReports.Select(x => x.EQ_DocType.ToString()).ToArray();
		}

		internal static Dictionary<ZString, ZString> BuildChangeCodeToReportSubCodeMap(BusinessObjectFactory factory, ZGuid recipientCodeMappingOrgPK)
		{
			var result = new Dictionary<ZString, ZString>();

			// Fallback to current Company as a source of Charge Code to Report Sub Code mapping
			var recipientCodeMappingOrg = recipientCodeMappingOrgPK.IsValid ?
				factory.Load<OrgHeader>(recipientCodeMappingOrgPK) :
				GlbCompany.CurrentCompany.OrgProxy;

			var filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_Relationship, OrgPatternMatchOverrideRelationships.ChargeCodes)
				.AddToFilter(OrgPatternMatchOverrideSchema.OO_OH, recipientCodeMappingOrg.PK);
			var mappings = factory.Load<OrgPatternMatchOverride>(filter).Select(x => new KeyValuePair<ZString, ZString>(x.OO_LocalCode, x.OO_ForeignCode)).Distinct();

			foreach (var map in mappings)
			{
				result.Add(map.Key, map.Value);
			}

			return result;
		}
	}
}
