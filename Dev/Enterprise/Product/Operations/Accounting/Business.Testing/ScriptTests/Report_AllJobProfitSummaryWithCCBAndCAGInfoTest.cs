using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.Testing.ScriptTests.BaseTests;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_AllJobProfitSummaryWithCCBAndCAGInfoTest : JobProfitFilterTest
	{
		protected override DataTable RunScriptWithFilter(string activeStatus = "", string notIncludeReversedWIPACR = "")
		{
			return RunScript(ZDateTime.Empty, ZDateTime.Empty, activeStatus: activeStatus, notIncludeReversedWIPACR: notIncludeReversedWIPACR);
		}

		DataTable RunScript(ZDateTime transactionFrom, ZDateTime transactionTo, string jobType = "",
			string activeStatus = "", bool outstandingWIP = false, bool outstandingACR = false,
			string chargeCode = "", string chargeCodeNotIn = "", string chargeGroup = "", string transactionBranch = "",
			string transactionDepartment = "", string transactionDebtor = "", string transactionCreditor = "",
			bool isCommissionable = false, string profitLossReasonCode = "", string cfsJobType = "", string notIncludeReversedWIPACR = "")
		{
			var sql = string.Format(@"
SELECT * FROM Report_AllJobProfitSummaryWithCCBAndCAGInfo(
	'{0}',						-- @CompanyPK
	'{1}',						-- @TransactionFrom
	'{2}',						-- @TransactionTo
	'{3}',						-- @JobType
	'{4}',						-- @OutstandingWIP
	'{5}',						-- @OutstandingACR
	'{6}',						-- @ChargeCode
	'{7}',						-- @@ChargeCodeNOTIN
	'{8}',						-- @ChargeGroup
	NULL,						-- @SalesGroup
	NULL,						-- @ExpenseGroup
	'{9}',						-- @TransactionBranch
	'{10}',						-- @TransactionDepartment
	'{11}',						-- @TransactionDebtor
	'{12}',						-- @TransactionCreditor
	{13},						-- @IsCommissionable
	'{14}',						-- @ProfitLossReasonCode
	'1900-01-01 00:00:00',		-- @RevRecogFrom
	'2079-06-06 23:59:29',		-- @RevRecogTo
	'{15}',						-- @CFSJobType
	@MNGListValue,				-- @MNGList
	@MNGListIsEmptyValue,		-- @MNGListIsEmpty
	'{16}',						-- @ActiveStatus
	'{17}'						-- @NotIncludeReversedWIPACR
)",
				GlbCompany.CurrentCompany.PK,
				GetMinDateTimeString(transactionFrom),
				GetMaxDateTimeString(transactionTo),
				jobType,
				outstandingWIP ? "Y" : "",
				outstandingACR ? "Y" : "",
				chargeCode,
				chargeCodeNotIn,
				chargeGroup,
				transactionBranch,
				transactionDepartment,
				transactionDebtor,
				transactionCreditor,
				isCommissionable ? 1 : 0,
				profitLossReasonCode,
				cfsJobType,
				activeStatus,
				notIncludeReversedWIPACR
			);
			var command = Db.Connection.Command(sql);
			AddTVPAndIsEmptyParameters(command, "@MNGListValue", "dbo.TVP_uniqueidentifier", "@MNGListIsEmptyValue", Array.Empty<Guid>());
			return DataUtils.GetDataTableFromCommand(command);
		}

		protected override object[][] DoNotShowReversedWIPACRFilterForShow =>
		[
			["S0001", 0.0m, 0.0m],
			["S0002", 0.0m, 12m],
			["S0003", -89.0m,  89.0m],
		];

		protected override object[][] DoNotShowReversedWIPACRFilterForNotShow =>
		[
			["S0002", 0.0m, 12m],
			["S0003", -89.0m,  89.0m],
		];
	}
}
