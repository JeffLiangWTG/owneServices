
using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.Testing.ScriptTests.BaseTests;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_AllJobProfitSummaryByLocalClientNEWTest : JobProfitFilterTest
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
			activeJob1.LocalChargesPK = TestObjectCreator.LocalClient.PK;
			TestObjectCreator.CreateCharge(activeJob1, TestObjectCreator.CC1, 10m, 20m);

			var shipment2 = TestObjectCreator.CreateShipment("S0002");
			var activeJob2 = TestObjectCreator.CreateJob(shipment2, false);
			activeJob2.LocalChargesPK = TestObjectCreator.LocalClient2.PK;
			TestObjectCreator.CreateCharge(activeJob2, TestObjectCreator.CC1, 40m, 60m);

			var shipment3 = TestObjectCreator.CreateShipment("S0003");
			var inactiveJob = TestObjectCreator.CreateJob(shipment3, false);
			inactiveJob.LocalChargesPK = TestObjectCreator.LocalClient2.PK;
			TestObjectCreator.CreateCharge(inactiveJob, TestObjectCreator.CC1, 90m, 120m);
			TestObjectCreator.CreateCharge(inactiveJob, TestObjectCreator.CC1, 160m, 200m);
			Factory.Save();

			inactiveJob.MarkAsInactive();

			Factory.Save();
		}

		protected override string[] ActiveStatusFilterHeadersForTest =>
			["JH_LocalClientCode", "JH_LocalClientName", "JH_Profit"];

		protected override object[][] ActiveStatusFilterActiveLinesForTest =>
			[
				[TestObjectCreator.LocalClient.OH_Code, TestObjectCreator.LocalClient.OH_FullName, 10m],
				[TestObjectCreator.LocalClient2.OH_Code, TestObjectCreator.LocalClient2.OH_FullName, 20m]
			];

		protected override object[][] ActiveStatusFilterInactiveLinesForTest =>
			[
				[TestObjectCreator.LocalClient2.OH_Code, TestObjectCreator.LocalClient2.OH_FullName, 0m],
			];
		protected override object[][] ActiveStatusFilterAllLinesForTest =>
		[
			[TestObjectCreator.LocalClient.OH_Code, TestObjectCreator.LocalClient.OH_FullName, 10m],
			[TestObjectCreator.LocalClient2.OH_Code, TestObjectCreator.LocalClient2.OH_FullName, 20m]
		];

		#endregion

		#region Show Reverse WIP/ACR filter test

		protected override void InitDataForShowReverseFilter()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			job1.LocalChargesPK = TestObjectCreator.LocalClient.PK;
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, 10m, 0m);

			var shipment2 = TestObjectCreator.CreateShipment("S0002");
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			job2.LocalChargesPK = TestObjectCreator.LocalClient2.PK;
			var charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, 0m, 11m);
			var charge3 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC3, 0m, 12m);

			Factory.Save();

			charge1.JR_LocalCostAmt = 0m;
			charge1.ReverseAccrual(ZDateTime.Now);
			charge2.JR_LocalSellAmt = 0m;
			charge2.ReverseWIP(ZDateTime.Now);

			Factory.Save();
		}

		protected override string[] DoNotShowReversedWIPACRFilterHeaders =>
				["JH_LocalClientCode", "JH_LocalClientName", "ACRAmount", "WIPAmount"];

		protected override string[] DoNotShowReversedWIPACRFilterKeyColumns =>
				DoNotShowReversedWIPACRFilterHeaders;

		protected override object[][] DoNotShowReversedWIPACRFilterForShow =>
		[
			[TestObjectCreator.LocalClient.OH_Code, TestObjectCreator.LocalClient.OH_FullName, 0m, 0m],
			[TestObjectCreator.LocalClient2.OH_Code, TestObjectCreator.LocalClient2.OH_FullName, 0m, 12m],
		];

		protected override object[][] DoNotShowReversedWIPACRFilterForNotShow =>
		[
			[TestObjectCreator.LocalClient2.OH_Code, TestObjectCreator.LocalClient2.OH_FullName, 0m, 12m],
		];

		#endregion

		DataTable RunScript(ZDateTime transactionFrom, ZDateTime transactionTo, string jobType = "",
			string activeStatus = "", bool outstandingWIP = false, bool outstandingACR = false,
			string chargeCode = "", string chargeCodeNotIn = "", string chargeGroup = "", string transactionBranch = "",
			string transactionDepartment = "", string transactionDebtor = "", string transactionCreditor = "",
			bool isCommissionable = false, string profitLossReasonCode = "", string cfsJobType = "", string notIncludeReversedWIPACR = "")
		{
			var sql = string.Format(@"
SELECT * FROM Report_AllJobProfitSummaryByLocalClientNEW(
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
	'',							-- @JobStatus
	'',							-- @JobOpenedFrom
	'',							-- @JobOpenedTo
	'',							-- @JobClosedFrom
	'',							-- @JobClosedTo
	'1900-01-01 00:00:00',		-- @JobRevRecogFrom
	'2079-06-06 23:59:29',		-- @JobRevRecogTo
	NULL,						-- @JobSalesRep
	NULL,						-- @JobOperator
	'',							-- @JobLocalClient
	'',							-- @JobBranch
	'',							-- @JobDepartment
	{13},						-- @IsCommissionable
	'{14}',						-- @ProfitLossReasonCode
	'{15}',						-- @CFSJobType
	'',							-- @BranchManagementCode
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
	}
}
