using System;
using System.Data;
using CargoWise.Data;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class ARTransactionsByComplianceSubTypeAndNumberTest : TransactionsByComplianceSubTypeCoreTest
	{
		protected override Type TransactionType => typeof(ARInvoice);

		protected override DataTable RunScript(string fromDate, string toDate, string transactionType, string status, string subtype, string allocationLevel, string orgCusCode = null)
		{
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var currentCountryTaxRegistrationOrgCusCode = string.IsNullOrEmpty(orgCusCode) ? Country.GetConsumptionTaxRegistrationOrgCusCode(currentCountry) : orgCusCode;

			var sql = string.Format(@"
SELECT * FROM TransactionsByComplianceSubTypeAndNumber(
	'{0}',				-- CurrentCountry
	'{1}',				-- Company
	'{2}',				-- CurrentCountryTaxRegistrationOrgCusCode
	'{3}',				-- FromDate
	'{4}',				-- ToDate
	'{5}',				-- TransactionType
	'{6}',				-- Status
	'{7}',				-- Subtype
	'',					-- TransactionBranchList
	'{8}',				-- AllocationLevel
	''					-- BookBranchList
)",
							GlbCompany.CurrentCompany.GC_RN_NKCountryCode,  //@CurrentCountry
							GlbCompany.CurrentCompany.PK,                   //@Company
							currentCountryTaxRegistrationOrgCusCode,        //@CurrentCountryTaxRegistrationOrgCusCode
							fromDate,                                       //@@FromDate
							toDate,                                         //@ToDate
							transactionType,                                //@TransactionType
							status,                                         //@Status
							subtype,                                        //@Subtype
							allocationLevel                                 //@AllocationLevel
	);
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}
