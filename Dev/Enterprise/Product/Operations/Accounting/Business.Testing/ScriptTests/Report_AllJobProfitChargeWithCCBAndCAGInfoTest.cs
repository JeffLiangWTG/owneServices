using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.Testing.ScriptTests.BaseTests;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_AllJobProfitChargeWithCCBAndCAGInfoTest : JobProfitFilterTest
	{
		protected override DataTable RunScriptWithFilter(string activeStatus = "", string notIncludeReversedWIPACR = "")
		{
			return RunScript(ZDateTime.Empty, ZDateTime.Empty, activeStatus: activeStatus, notIncludeReversedWIPACR: notIncludeReversedWIPACR);
		}

		#region Active Status filter test

		protected override void InitDataForActiveStatusFilter()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			var activeJob1 = TestObjectCreator.CreateJob(shipment1, false);
			TestObjectCreator.CreateCharge(activeJob1, TestObjectCreator.CC1, 0m, 5m);
			TestObjectCreator.CreateCharge(activeJob1, TestObjectCreator.CC3, 5m, 15m);

			var shipment2 = TestObjectCreator.CreateShipment("S0002");
			var activeJob2 = TestObjectCreator.CreateJob(shipment2, false);
			TestObjectCreator.CreateCharge(activeJob2, TestObjectCreator.CC4, 15m, 30m);
			TestObjectCreator.CreateCharge(activeJob2, TestObjectCreator.CC5, 30m, 50m);

			var shipment3 = TestObjectCreator.CreateShipment("S0003");
			var inactiveJob = TestObjectCreator.CreateJob(shipment3, false);
			TestObjectCreator.CreateCharge(inactiveJob, TestObjectCreator.CC6, 50m, 75m);
			TestObjectCreator.CreateCharge(inactiveJob, TestObjectCreator.CC7, 75m, 105m);
			Factory.Save();

			inactiveJob.MarkAsInactive();

			Factory.Save();
		}

		protected override string[] ActiveStatusFilterKeyColumnsForTest =>
			["JH_JobNum", "AC_Code"];

		protected override string[] ActiveStatusFilterHeadersForTest =>
			["JH_JobNum", "JH_IsActive", "AC_Code", "JH_Profit"];

		protected override object[][] ActiveStatusFilterActiveLinesForTest =>
			[
				["S0001", true, TestObjectCreator.CC1.AC_Code, 5m],
				["S0001", true, TestObjectCreator.CC3.AC_Code, 10m],
				["S0002", true, TestObjectCreator.CC4.AC_Code, 15m],
				["S0002", true, TestObjectCreator.CC5.AC_Code, 20m]
			];

		protected override object[][] ActiveStatusFilterInactiveLinesForTest =>
			[
				["S0003", false, TestObjectCreator.CC6.AC_Code, 0m],
				["S0003", false, TestObjectCreator.CC7.AC_Code, 0m]
			];

		#endregion

		DataTable RunScript(ZDateTime transactionFrom, ZDateTime transactionTo, string jobType = "",
			string activeStatus = "", bool outstandingWIP = false, bool outstandingACR = false,
			string chargeCode = "", string chargeCodeNotIn = "", string chargeGroup = "", string transactionBranch = "",
			string transactionDepartment = "", string transactionDebtor = "", string transactionCreditor = "",
			bool isCommissionable = false, string profitLossReasonCode = "", string cfsJobType = "", string notIncludeReversedWIPACR = "")
		{
			var sql = string.Format(@"
SELECT * FROM Report_AllJobProfitChargeWithCCBAndCAGInfo(
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
			["S0001", 0m, 0m],
			["S0002", 0m, 0m],
			["S0002", 0m, 12m],
			["S0003", -89.0m,  89.0m],
		];

		protected override object[][] DoNotShowReversedWIPACRFilterForNotShow =>
		[
			["S0002", 0m, 12m],
			["S0003", -89.0m,  89.0m],
		];
	}
}
