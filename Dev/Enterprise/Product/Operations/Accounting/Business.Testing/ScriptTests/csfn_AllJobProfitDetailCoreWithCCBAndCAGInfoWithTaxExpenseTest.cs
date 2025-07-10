using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.Testing.ScriptTests.BaseTests;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class csfn_AllJobProfitDetailCoreWithCCBAndCAGInfoWithTaxExpenseTest : JobProfitFilterTest
	{
		#region Active Status filter test

		protected override string[] ActiveStatusFilterKeyColumnsForTest =>
			["JH_JobNum", "AL_LineType"];

		protected override string[] ActiveStatusFilterHeadersForTest =>
			["JH_JobNum", "AL_LineType", "AL_LineAmount", "JH_IsActive"];

		protected override object[][] ActiveStatusFilterActiveLinesForTest =>
		[
			["S0001", "ACR", -10m, true],
			["S0001", "WIP", 20m, true],
			["S0002", "ACR", -40m, true],
			["S0002", "WIP", 60m, true]
		];

		protected override object[][] ActiveStatusFilterInactiveLinesForTest =>
		[
			["S0003", "ACR", 0m, false],
			["S0003", "WIP", 0m, false],
			["S0003", "ACR", 0m, false],
			["S0003", "WIP", 0m, false]
		];

		#endregion

		protected override bool ShouldTestShowReverseFilter => false;

		protected override DataTable RunScriptWithFilter(string activeStatus = "", string notIncludeReversedWIPACR = "")
		{
			var sql = string.Format(@"
SELECT * FROM csfn_AllJobProfitDetailCoreWithCCBAndCAGInfoWithTaxExpense(
	'{0}',					-- @CompanyPK
	'{1}',					-- @TransactionFrom
	'{2}',					-- @TransactionTo
	'',						-- @JobType
	'',						-- @OutstandingWIP
	'',						-- @OutstandingACR
	'',						-- @NotIncludeReversedWIPACR
	'',						-- @ChargeCode
	'',						-- @ChargeCodeNOTIN
	'',						-- @ChargeGroup
	NULL,					-- @SalesGroup
	NULL,					-- @ExpenseGroup
	NULL,					-- @AL_GBList
	NULL,					-- @AL_GEList
	'',						-- @PostedOnly
	'',						-- @CFSJobType
	'',						-- @UnPostedOnly
	@MNGListValue,			-- @MNGList
	@MNGListIsEmptyValue,	-- @MNGListIsEmpty
	'{3}'					-- @ActiveStatus
)",
				GlbCompany.CurrentCompany.PK,
				GetMinDateTimeString(ZDateTime.Empty),
				GetMaxDateTimeString(ZDateTime.Empty),
				activeStatus
			);
			var command = Db.Connection.Command(sql);
			AddTVPAndIsEmptyParameters(command, "@MNGListValue", "dbo.TVP_uniqueidentifier", "@MNGListIsEmptyValue", Array.Empty<Guid>());
			return DataUtils.GetDataTableFromCommand(command);
		}
	}
}
