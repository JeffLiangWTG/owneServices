using System.Data;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests.BaseTests
{
	abstract class JobProfitFilterTest : ScriptTest
	{
		protected virtual DataTable RunScriptWithFilter(string activeStatus = "", string notIncludeReversedWIPACR = "")
		{
			return null;
		}

		#region Active Status filter test

		protected virtual bool ShouldTestActiveStatusFilter => true;

		protected virtual void InitDataForActiveStatusFilter()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			var activeJob1 = TestObjectCreator.CreateJob(shipment1, false);
			TestObjectCreator.CreateCharge(activeJob1, TestObjectCreator.CC1, 10m, 20m);

			var shipment2 = TestObjectCreator.CreateShipment("S0002");
			var activeJob2 = TestObjectCreator.CreateJob(shipment2, false);
			TestObjectCreator.CreateCharge(activeJob2, TestObjectCreator.CC1, 40m, 60m);

			var shipment3 = TestObjectCreator.CreateShipment("S0003");
			var inactiveJob = TestObjectCreator.CreateJob(shipment3, false);
			TestObjectCreator.CreateCharge(inactiveJob, TestObjectCreator.CC1, 90m, 120m);
			TestObjectCreator.CreateCharge(inactiveJob, TestObjectCreator.CC1, 160m, 200m);
			Factory.Save();

			inactiveJob.MarkAsInactive();

			Factory.Save();
		}

		protected virtual string[] ActiveStatusFilterKeyColumnsForTest =>
			[ActiveStatusFilterHeadersForTest[0]];

		protected virtual string[] ActiveStatusFilterHeadersForTest =>
			["JH_JobNum", "JH_Profit", "JH_IsActive"];

		protected virtual object[][] ActiveStatusFilterActiveLinesForTest =>
			[
				["S0001", 10m, true],
				["S0002", 20m, true]
			];

		protected virtual object[][] ActiveStatusFilterInactiveLinesForTest =>
			[
				["S0003", 0m, false]
			];

		protected virtual object[][] ActiveStatusFilterAllLinesForTest =>
			ActiveStatusFilterActiveLinesForTest.Concat(ActiveStatusFilterInactiveLinesForTest).ToArray();

		public void TestActiveStatusFiltering()
		{
			if (!ShouldTestActiveStatusFilter)
			{
				Assert("Unnecessary to test Active Status filter", true);
				return;
			}

			InitDataForActiveStatusFilter();

			var resultWithActiveStatusActive = RunScriptWithFilter(activeStatus: "Active");
			AssertDataTableAllRowsByKeyColumns("Filtering with active status - Active", resultWithActiveStatusActive, ActiveStatusFilterHeadersForTest, ActiveStatusFilterActiveLinesForTest, ActiveStatusFilterKeyColumnsForTest);

			var resultWithActiveStatusInactive = RunScriptWithFilter(activeStatus: "Inactive");
			AssertDataTableAllRowsByKeyColumns("Filtering with active status - Inactive", resultWithActiveStatusInactive, ActiveStatusFilterHeadersForTest, ActiveStatusFilterInactiveLinesForTest, ActiveStatusFilterKeyColumnsForTest);

			var resultWithActiveStatusAll = RunScriptWithFilter(activeStatus: "All");
			AssertDataTableAllRowsByKeyColumns("Filtering with active status - All", resultWithActiveStatusAll, ActiveStatusFilterHeadersForTest, ActiveStatusFilterAllLinesForTest, ActiveStatusFilterKeyColumnsForTest);
		}

		#endregion

		#region Show Reverse Filter Test

		protected virtual bool ShouldTestShowReverseFilter => true;

		protected virtual void InitDataForShowReverseFilter()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, 10m, 0m);

			var shipment2 = TestObjectCreator.CreateShipment("S0002");
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			var charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, 0m, 11m);
			var charge3 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC3, 0m, 12m);

			var shipment3 = TestObjectCreator.CreateShipment("S0003");
			var job3 = TestObjectCreator.CreateJob(shipment3, false);
			TestObjectCreator.CreateCharge(job3, TestObjectCreator.CC1, 89m, 89m);

			Factory.Save();

			charge1.JR_LocalCostAmt = 0m;
			charge1.ReverseAccrual(ZDateTime.Now);
			charge2.JR_LocalSellAmt = 0m;
			charge2.ReverseWIP(ZDateTime.Now);

			Factory.Save();
		}

		[TestDate(2022, 1, 2, 3, 4, 0)]
		public void TestDoNotShowReverseWIPACRFilter()
		{
			if (!ShouldTestShowReverseFilter)
			{
				Assert("Unnecessary to test Show Reverse WIP ACR filter", true);
				return;
			}

			InitDataForShowReverseFilter();

			var resultIncludeReversedWIPACR = RunScriptWithFilter(notIncludeReversedWIPACR: "");
			AssertDataTableAllRowsByKeyColumns("Filter with do not show reversed WIP/ACR - No", resultIncludeReversedWIPACR, DoNotShowReversedWIPACRFilterHeaders, DoNotShowReversedWIPACRFilterForShow, DoNotShowReversedWIPACRFilterKeyColumns);

			var resultNotIncludeReversedWIPACR = RunScriptWithFilter(notIncludeReversedWIPACR: "Y");
			AssertDataTableAllRowsByKeyColumns("Filter with do not show reversed WIP/ACR - Yes", resultNotIncludeReversedWIPACR, DoNotShowReversedWIPACRFilterHeaders, DoNotShowReversedWIPACRFilterForNotShow, DoNotShowReversedWIPACRFilterKeyColumns);
		}

		protected virtual string[] DoNotShowReversedWIPACRFilterHeaders =>
				["JH_JobNum", "ACRAmount", "WIPAmount"];

		protected virtual string[] DoNotShowReversedWIPACRFilterKeyColumns =>
				DoNotShowReversedWIPACRFilterHeaders;

		protected virtual object[][] DoNotShowReversedWIPACRFilterForShow =>
		[
			["S0001", -10m, 0m],
			["S0001", 10m, 0m],
			["S0002", 0.0m, 11m],
			["S0002", 0.0m, -11m],
			["S0002", 0.0m, 12m],
			["S0003", -89.0m,   0.0m],
			["S0003", 0.0m,  89.0m],
		];

		protected virtual object[][] DoNotShowReversedWIPACRFilterForNotShow =>
		[
			["S0002", 0.0m, 12m],
			["S0003", -89.0m,   0.0m],
			["S0003", 0.0m,  89.0m],
		];

		#endregion
	}
}
