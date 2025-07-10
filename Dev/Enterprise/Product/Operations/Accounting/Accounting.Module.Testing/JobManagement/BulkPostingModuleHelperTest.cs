using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Accounting.Module.BulkPostingModuleHelper;

namespace Enterprise.Accounting.Module.Testing
{
	public class BulkPostingModuleHelperTest : TestCaseWithFactory
	{
		public void TestPostTransactions()
		{
			BulkPostingModuleHelper helper = new BulkPostingModuleHelper();
			helper.Initialize("Name of my Bizobject", false);
			var testObjectCreator = new TestObjectCreator(Factory);
			var job = testObjectCreator.CreateJob(testObjectCreator.CreateShipment("S0001"), false);

			AssertNoExceptionThrown(() => helper.PostTransactions(JobInvoicingPostingOption.All, new Job[] { job }));
			AssertEquals(@"Are you sure you want to Post All Charges and Costs for this Name of my Bizobject?
S0001", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertNoExceptionThrown(() => helper.PostTransactions(JobInvoicingPostingOption.All, Array.Empty<BusinessObject>()));
			AssertEquals("Message should be shown", "Please select a Job before posting.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestGetSecurityCheckPoint()
		{
			BulkPostingModuleHelper helper = new BulkPostingModuleHelper();
			helper.Initialize("Name of my Bizobject", false);
			var testObjectCreator = new TestObjectCreator(Factory);
			var job = testObjectCreator.CreateJob(testObjectCreator.CreateShipment("S0001"), false);

			string code = Env.Security.MaintainShipmentJobInvoicing.Code;

			Func<JobInvoicingPostingOption, SecurityCheckpoint[]> getSecurityCheckpoint = option => helper.GetSecurityCheckPoints_ForTestOnly(option, new BusinessObject[] { job });
			var checkpoints = getSecurityCheckpoint(JobInvoicingPostingOption.All);
			AssertEquals("Checkpoint Code", code + SecurityCore.BulkPostAll, checkpoints[0].Code);

			checkpoints = getSecurityCheckpoint(JobInvoicingPostingOption.LocalClient);
			AssertEquals("Checkpoint Code", code + SecurityCore.BulkPostLocalClient, checkpoints[0].Code);

			checkpoints = getSecurityCheckpoint(JobInvoicingPostingOption.Agent);
			AssertEquals("Checkpoint Code", code + SecurityCore.BulkPostOverseas, checkpoints[0].Code);

			checkpoints = getSecurityCheckpoint(JobInvoicingPostingOption.Revenue);
			AssertEquals("Checkpoint Code", code + SecurityCore.BulkPostRevenue, checkpoints[0].Code);

			checkpoints = getSecurityCheckpoint(JobInvoicingPostingOption.Costs);
			AssertEquals("Checkpoint Code", code + SecurityCore.BulkPostCost, checkpoints[0].Code);

			checkpoints = getSecurityCheckpoint(JobInvoicingPostingOption.Disbursement);
			AssertEquals("Checkpoint Code", code + SecurityCore.BulkPostDSB, checkpoints[0].Code);

			checkpoints = getSecurityCheckpoint(JobInvoicingPostingOption.AllSisterCompanyCharges);
			AssertEquals("Checkpoint Code", code + SecurityCore.BulkPostAllSisterCompanyCharges, checkpoints[0].Code);

			checkpoints = getSecurityCheckpoint(JobInvoicingPostingOption.LocalSisterCompanyChargesOnly);
			AssertEquals("Checkpoint Code", code + SecurityCore.BulkPostLocalSisterCompanyChargesOnly, checkpoints[0].Code);

			checkpoints = getSecurityCheckpoint(JobInvoicingPostingOption.CustomsDSBChargeAPOnly);
			AssertEquals("There should be no security checkpoint for Posting Option: " + JobInvoicingPostingOption.CustomsDSBChargeAPOnly, 0, checkpoints.Length);

			checkpoints = getSecurityCheckpoint(JobInvoicingPostingOption.CustomsDSBChargeAROnly);
			AssertEquals("There should be no security checkpoint for Posting Option: " + JobInvoicingPostingOption.CustomsDSBChargeAROnly, 0, checkpoints.Length);

			helper = new BulkPostingModuleHelper();
			helper.Initialize("", true);

			checkpoints = getSecurityCheckpoint(JobInvoicingPostingOption.All);
			AssertEquals("Checkpoint Code", Env.Security.ConsolBulkPostWholeConsol.Code, checkpoints[0].Code);

			checkpoints = getSecurityCheckpoint(JobInvoicingPostingOption.Agent);
			AssertEquals("Checkpoint Code", Env.Security.ConsolBulkPostAgent.Code, checkpoints[0].Code);

			checkpoints = getSecurityCheckpoint(JobInvoicingPostingOption.Costs);
			AssertEquals("Checkpoint Code", Env.Security.ConsolBulkPostCosts.Code, checkpoints[0].Code);
		}

		public void TestPostTransactionsShowsSecurityErrorWhenSecurityIsNotAllowed_Job()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var job1 = testObjectCreator.CreateJob(testObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment), false);
			var job2 = testObjectCreator.CreateJob(testObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.CFSLoadList), false);

			var job1BulkPostAllCheckpoint = new JobInvoicingSecurityHelper(job1.PlugInData.InvoicingSupporter.JobInvoicingSecurity).GetInvSecurity(SecurityCore.BulkPostAll);
			var job2BulkPostAllCheckpoint = new JobInvoicingSecurityHelper(job2.PlugInData.InvoicingSupporter.JobInvoicingSecurity).GetInvSecurity(SecurityCore.BulkPostAll);

			BulkPostingModuleHelper helper = new BulkPostingModuleHelper();
			helper.Initialize("Name of my Bizobject", false);

			Action<string> assertAction = expectedText =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					helper.PostTransactions(JobInvoicingPostingOption.All, new Job[] { job1, job2 });
					var message = UnitTestUserNotification.Instance.LastMessage;

					AssertNotNull("Message should be shown", message);
					AssertEquals("Message", expectedText, message.Text);
				};

			string expectedMessage =
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Shipments -> Billing -> Bulk Job Billing Actions -> Post All Charges and Cost
Operate -> CFS/CTO -> Load Lists -> Billing -> Bulk Job Billing Actions -> Post All Charges and Cost";

			job1BulkPostAllCheckpoint.IsAllowed = false;
			job2BulkPostAllCheckpoint.IsAllowed = false;
			assertAction(expectedMessage);

			expectedMessage =
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Shipments -> Billing -> Bulk Job Billing Actions -> Post All Charges and Cost";

			job1BulkPostAllCheckpoint.IsAllowed = false;
			job2BulkPostAllCheckpoint.IsAllowed = true;
			assertAction(expectedMessage);

			expectedMessage =
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> CFS/CTO -> Load Lists -> Billing -> Bulk Job Billing Actions -> Post All Charges and Cost";

			job1BulkPostAllCheckpoint.IsAllowed = true;
			job2BulkPostAllCheckpoint.IsAllowed = false;
			assertAction(expectedMessage);

			expectedMessage = string.Format(
@"Are you sure you want to Post All Charges and Costs for these Name of my Bizobjects?
{0}
{1}",
			job1.JH_JobNum, job2.JH_JobNum);

			job1BulkPostAllCheckpoint.IsAllowed = true;
			job2BulkPostAllCheckpoint.IsAllowed = true;
			assertAction(expectedMessage);
		}

		public void TestPostTransactionsShowsSecurityErrorWhenSecurityIsNotAllowed_Consol()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001");

			var bulkPostWholeConsolCheckpoint = Env.Security.ConsolBulkPostWholeConsol;

			BulkPostingModuleHelper helper = new BulkPostingModuleHelper();
			helper.Initialize("", true);

			Action<string> assertAction = expectedText =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					helper.PostTransactions(JobInvoicingPostingOption.All, new IJobCostingPlugIn[] { consol });
					var message = UnitTestUserNotification.Instance.LastMessage;
					AssertNotNull("Message should be shown", message);
					AssertEquals("Message", expectedText, message.Text);
				};

			string expectedMessage =
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Consolidations -> Costing/Invoicing -> Bulk Job Billing Actions -> Post Whole Consol";

			bulkPostWholeConsolCheckpoint.IsAllowed = false;
			assertAction(expectedMessage);

			expectedMessage =
@"Are you sure you want to Post Whole Consol for this consol?
C0001";
			bulkPostWholeConsolCheckpoint.IsAllowed = true;
			assertAction(expectedMessage);
		}

		public void TestJobNumberComparer()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			var shipment1 = testObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			var shipment2 = testObjectCreator.CreateShipment("S00010002", "AUSYD", "NZAKL", null);

			JobNumberComparer comparer = new JobNumberComparer();
			AssertEquals("Expected: S00010001 is less than S00010002", -1, comparer.Compare(shipment1, shipment2));
			AssertEquals("Expected: S00010002 is greater than S00010001", 1, comparer.Compare(shipment2, shipment1));
			AssertEquals("Expected: S00010001 is equal to S00010001", 0, comparer.Compare(shipment1, shipment1));
		}

		public void TestSortJobArrayByJobNumber()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			var shipment1 = testObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			var shipment2 = testObjectCreator.CreateShipment("S00010002", "AUSYD", "NZAKL", null);
			var shipment3 = testObjectCreator.CreateShipment("S00010003", "AUSYD", "NZAKL", null);
			var shipment4 = testObjectCreator.CreateShipment("S00010004", "AUSYD", "NZAKL", null);
			var shipment5 = testObjectCreator.CreateShipment("S00010005", "AUSYD", "NZAKL", null);
			IJobNumber[] selectedJobs = { shipment3, shipment1, shipment5, shipment2, shipment4 };
			BulkPostingModuleHelper helper = new BulkPostingModuleHelper();

			AssertEquals("S00010003", selectedJobs[0].JobNumber);
			AssertEquals("S00010001", selectedJobs[1].JobNumber);
			AssertEquals("S00010005", selectedJobs[2].JobNumber);
			AssertEquals("S00010002", selectedJobs[3].JobNumber);
			AssertEquals("S00010004", selectedJobs[4].JobNumber);

			helper.SortJobArrayByJobNumber(selectedJobs);

			AssertEquals("S00010001", selectedJobs[0].JobNumber);
			AssertEquals("S00010002", selectedJobs[1].JobNumber);
			AssertEquals("S00010003", selectedJobs[2].JobNumber);
			AssertEquals("S00010004", selectedJobs[3].JobNumber);
			AssertEquals("S00010005", selectedJobs[4].JobNumber);
		}

		[TestDate(2015, 1, 1)]
		public void TestGetPostMenuItem()
		{
			const string menuLine = "-";
			const string expectedPostMenuName = "&Post";
			const string expectedAgentMenuName = "Post Overseas Agent Charges";
			const string expectedCostsMenuName = "Post Costs";
			const string expectedAllMenuName = "Post All Charges and Costs";
			const string expectedLocalClientMenuName = "Post Local Client Charges";
			const string expectedRevenueMenuName = "Post All Revenue Charges";
			const string expectedPostSisterCompanyMenuName = "Post Charges for All Group Companies";
			const string expectedLocalSisterCompanyMenuName = "Post Charges for Group Companies in My Login Country/Region";
			const string expectedDisbursementMenuName = "Post Disbursement Charges only";

			var helper = new BulkPostingModuleHelper();
			helper.Initialize("Bizo", false);

			var menu = helper.GetPostMenuItem(option => { });
			Action assertConsolPostionMenuItems = () =>
				{
					AssertEquals("menu.Text", "&Post", menu.Text);
					AssertEquals("menu.MenuItems.Count", 10, menu.MenuItems.Count);
					AssertEquals("menu.MenuItems[0].Text", expectedAllMenuName, ((IMenuItem)menu.MenuItems[0]).Text);
					AssertEquals("menu.MenuItems[1].Text", menuLine, ((IMenuItem)menu.MenuItems[1]).Text);
					AssertEquals("menu.MenuItems[2].Text", expectedLocalClientMenuName, ((IMenuItem)menu.MenuItems[2]).Text);
					AssertEquals("menu.MenuItems[3].Text", expectedAgentMenuName, ((IMenuItem)menu.MenuItems[3]).Text);
					AssertEquals("menu.MenuItems[4].Text", expectedRevenueMenuName, ((IMenuItem)menu.MenuItems[4]).Text);
					AssertEquals("menu.MenuItems[5].Text", expectedPostSisterCompanyMenuName, ((IMenuItem)menu.MenuItems[5]).Text);
					AssertEquals("menu.MenuItems[6].Text", expectedLocalSisterCompanyMenuName, ((IMenuItem)menu.MenuItems[6]).Text);
					AssertEquals("menu.MenuItems[7].Text", expectedDisbursementMenuName, ((IMenuItem)menu.MenuItems[7]).Text);
					AssertEquals("menu.MenuItems[8].Text", menuLine, ((IMenuItem)menu.MenuItems[8]).Text);
					AssertEquals("menu.MenuItems[9].Text", expectedCostsMenuName, ((IMenuItem)menu.MenuItems[9]).Text);
				};
			assertConsolPostionMenuItems();
			menu = helper.GetPostMenuItem(option => { }, null);
			assertConsolPostionMenuItems();
			menu = helper.GetPostMenuItem(option => { }, Array.Empty<JobInvoicingPostingOption>());
			assertConsolPostionMenuItems();

			menu = helper.GetPostMenuItem(option => { }, JobInvoicingPostingOption.Agent, JobInvoicingPostingOption.Costs, JobInvoicingPostingOption.All
				, JobInvoicingPostingOption.LocalClient, JobInvoicingPostingOption.Revenue, JobInvoicingPostingOption.AllSisterCompanyCharges
				, JobInvoicingPostingOption.LocalSisterCompanyChargesOnly, JobInvoicingPostingOption.Disbursement);
			assertConsolPostionMenuItems(); //different order of options passed shouldn't change menu items order

			menu = helper.GetPostMenuItem(option => { }, JobInvoicingPostingOption.CustomsDSBChargeAPOnly);
			AssertEquals("menu.Text", expectedPostMenuName, menu.Text);
			AssertEquals("Passed option is not applicable. menu.MenuItems.Count", 0, menu.MenuItems.Count);

			menu = helper.GetPostMenuItem(option => { }, JobInvoicingPostingOption.Agent);
			AssertEquals("menu.Text", expectedPostMenuName, menu.Text);
			AssertEquals("menu.MenuItems.Count", 1, menu.MenuItems.Count);
			AssertEquals("menu.MenuItems[0].Text", expectedAgentMenuName, ((IMenuItem)menu.MenuItems[0]).Text);

			menu = helper.GetPostMenuItem(option => { }, JobInvoicingPostingOption.Costs);
			AssertEquals("menu.Text", expectedPostMenuName, menu.Text);
			AssertEquals("menu.MenuItems.Count", 1, menu.MenuItems.Count);
			AssertEquals("menu.MenuItems[0].Text", expectedCostsMenuName, ((IMenuItem)menu.MenuItems[0]).Text);

			menu = helper.GetPostMenuItem(option => { }, JobInvoicingPostingOption.All);
			AssertEquals("menu.Text", expectedPostMenuName, menu.Text);
			AssertEquals("menu.MenuItems.Count", 1, menu.MenuItems.Count);
			AssertEquals("menu.MenuItems[0].Text", expectedAllMenuName, ((IMenuItem)menu.MenuItems[0]).Text);

			menu = helper.GetPostMenuItem(option => { }, JobInvoicingPostingOption.LocalClient);
			AssertEquals("menu.Text", expectedPostMenuName, menu.Text);
			AssertEquals("menu.MenuItems.Count", 1, menu.MenuItems.Count);
			AssertEquals("menu.MenuItems[0].Text", expectedLocalClientMenuName, ((IMenuItem)menu.MenuItems[0]).Text);

			menu = helper.GetPostMenuItem(option => { }, JobInvoicingPostingOption.Revenue);
			AssertEquals("menu.Text", expectedPostMenuName, menu.Text);
			AssertEquals("menu.MenuItems.Count", 1, menu.MenuItems.Count);
			AssertEquals("menu.MenuItems[0].Text", expectedRevenueMenuName, ((IMenuItem)menu.MenuItems[0]).Text);

			menu = helper.GetPostMenuItem(option => { }, JobInvoicingPostingOption.Disbursement);
			AssertEquals("menu.Text", expectedPostMenuName, menu.Text);
			AssertEquals("menu.MenuItems.Count", 1, menu.MenuItems.Count);
			AssertEquals("menu.MenuItems[0].Text", expectedDisbursementMenuName, ((IMenuItem)menu.MenuItems[0]).Text);

			menu = helper.GetPostMenuItem(option => { }, JobInvoicingPostingOption.All, JobInvoicingPostingOption.LocalClient, JobInvoicingPostingOption.Disbursement);
			AssertEquals("menu.Text", expectedPostMenuName, menu.Text);
			AssertEquals("menu.MenuItems.Count", 4, menu.MenuItems.Count);
			AssertEquals("menu.MenuItems[0].Text", expectedAllMenuName, ((IMenuItem)menu.MenuItems[0]).Text);
			AssertEquals("menu.MenuItems[1].Text", menuLine, ((IMenuItem)menu.MenuItems[1]).Text);
			AssertEquals("menu.MenuItems[2].Text", expectedLocalClientMenuName, ((IMenuItem)menu.MenuItems[2]).Text);
			AssertEquals("menu.MenuItems[3].Text", expectedDisbursementMenuName, ((IMenuItem)menu.MenuItems[3]).Text);

			menu = helper.GetPostMenuItem(option => { }, JobInvoicingPostingOption.Agent, JobInvoicingPostingOption.LocalClient, JobInvoicingPostingOption.Revenue, JobInvoicingPostingOption.Disbursement);
			AssertEquals("menu.Text", expectedPostMenuName, menu.Text);
			AssertEquals("menu.MenuItems.Count", 4, menu.MenuItems.Count);
			AssertEquals("menu.MenuItems[0].Text", expectedLocalClientMenuName, ((IMenuItem)menu.MenuItems[0]).Text);
			AssertEquals("menu.MenuItems[1].Text", expectedAgentMenuName, ((IMenuItem)menu.MenuItems[1]).Text);
			AssertEquals("menu.MenuItems[2].Text", expectedRevenueMenuName, ((IMenuItem)menu.MenuItems[2]).Text);
			AssertEquals("menu.MenuItems[3].Text", expectedDisbursementMenuName, ((IMenuItem)menu.MenuItems[3]).Text);

			menu = helper.GetPostMenuItem(option => { }, JobInvoicingPostingOption.Agent, JobInvoicingPostingOption.Costs, JobInvoicingPostingOption.Revenue);
			AssertEquals("menu.Text", expectedPostMenuName, menu.Text);
			AssertEquals("menu.MenuItems.Count", 4, menu.MenuItems.Count);
			AssertEquals("menu.MenuItems[0].Text", expectedAgentMenuName, ((IMenuItem)menu.MenuItems[0]).Text);
			AssertEquals("menu.MenuItems[1].Text", expectedRevenueMenuName, ((IMenuItem)menu.MenuItems[1]).Text);
			AssertEquals("menu.MenuItems[2].Text", menuLine, ((IMenuItem)menu.MenuItems[2]).Text);
			AssertEquals("menu.MenuItems[3].Text", expectedCostsMenuName, ((IMenuItem)menu.MenuItems[3]).Text);

			menu = helper.GetPostMenuItem(option => { }, JobInvoicingPostingOption.Agent, JobInvoicingPostingOption.Costs, JobInvoicingPostingOption.All);
			AssertEquals("menu.Text", expectedPostMenuName, menu.Text);
			AssertEquals("menu.MenuItems.Count", 5, menu.MenuItems.Count);
			AssertEquals("menu.MenuItems[0].Text", expectedAllMenuName, ((IMenuItem)menu.MenuItems[0]).Text);
			AssertEquals("menu.MenuItems[1].Text", menuLine, ((IMenuItem)menu.MenuItems[1]).Text);
			AssertEquals("menu.MenuItems[2].Text", expectedAgentMenuName, ((IMenuItem)menu.MenuItems[2]).Text);
			AssertEquals("menu.MenuItems[3].Text", menuLine, ((IMenuItem)menu.MenuItems[3]).Text);
			AssertEquals("menu.MenuItems[4].Text", expectedCostsMenuName, ((IMenuItem)menu.MenuItems[4]).Text);

			menu = helper.GetPostMenuItem(option => { }, JobInvoicingPostingOption.Costs, JobInvoicingPostingOption.All);
			AssertEquals("menu.Text", expectedPostMenuName, menu.Text);
			AssertEquals("menu.MenuItems.Count", 3, menu.MenuItems.Count);
			AssertEquals("menu.MenuItems[0].Text", expectedAllMenuName, ((IMenuItem)menu.MenuItems[0]).Text);
			AssertEquals("menu.MenuItems[1].Text", menuLine, ((IMenuItem)menu.MenuItems[1]).Text);
			AssertEquals("menu.MenuItems[2].Text", expectedCostsMenuName, ((IMenuItem)menu.MenuItems[2]).Text);

			//DefaultARInvoiceDateMenuText
			MenuItemTestHelper.ValidateDefaultARInvoiceDateMenuText(
				() =>
				{
					var menus = helper.GetPostMenuItem(option => { });
					return ((IMenuItem)menus.MenuItems[0]).Text;
				}
				);
		}

		public void TestGetPostMenuItem_ForConsolPosting()
		{
			const string expectedPostMenuName = "&Post";
			const string expectedAgentMenuName = "Post Overseas Agent Charges";
			const string expectedCostsMenuName = "Post All Costs";
			const string expectedConsolCostsOnlyMenuName = "Post Consol Costs Only";
			const string expectedAllMenuName = "Post Whole Consol";

			var helper = new BulkPostingModuleHelper();
			helper.Initialize("Bizo", true);

			var menu = helper.GetPostMenuItem(option => { });
			Action assertConsolPostionMenuItems = () =>
				{
					AssertEquals("menu.Text", expectedPostMenuName, menu.Text);
					AssertEquals("menu.MenuItems.Count", 4, menu.MenuItems.Count);
					AssertEquals("menu.MenuItems[0].Text", expectedAgentMenuName, ((IMenuItem)menu.MenuItems[0]).Text);
					AssertEquals("menu.MenuItems[1].Text", expectedCostsMenuName, ((IMenuItem)menu.MenuItems[1]).Text);
					AssertEquals("menu.MenuItems[2].Text", expectedConsolCostsOnlyMenuName, ((IMenuItem)menu.MenuItems[2]).Text);
					AssertEquals("menu.MenuItems[3].Text", expectedAllMenuName, ((IMenuItem)menu.MenuItems[3]).Text);
				};
			assertConsolPostionMenuItems();
			menu = helper.GetPostMenuItem(option => { }, null);
			assertConsolPostionMenuItems();
			menu = helper.GetPostMenuItem(option => { }, Array.Empty<JobInvoicingPostingOption>());
			assertConsolPostionMenuItems();

			menu = helper.GetPostMenuItem(option => { }, JobInvoicingPostingOption.All, JobInvoicingPostingOption.Agent, JobInvoicingPostingOption.Costs, JobInvoicingPostingOption.ConsolCosts);
			assertConsolPostionMenuItems(); //different order of options passed shouldn't change menu items order

			menu = helper.GetPostMenuItem(option => { }, JobInvoicingPostingOption.Revenue);
			AssertEquals("menu.Text", expectedPostMenuName, menu.Text);
			AssertEquals("Passed option is not applicable. menu.MenuItems.Count", 0, menu.MenuItems.Count);

			menu = helper.GetPostMenuItem(option => { }, JobInvoicingPostingOption.Agent);
			AssertEquals("menu.Text", expectedPostMenuName, menu.Text);
			AssertEquals("menu.MenuItems.Count", 1, menu.MenuItems.Count);
			AssertEquals("menu.MenuItems[0].Text", expectedAgentMenuName, ((IMenuItem)menu.MenuItems[0]).Text);

			menu = helper.GetPostMenuItem(option => { }, JobInvoicingPostingOption.All);
			AssertEquals("menu.Text", expectedPostMenuName, menu.Text);
			AssertEquals("menu.MenuItems.Count", 1, menu.MenuItems.Count);
			AssertEquals("menu.MenuItems[0].Text", expectedAllMenuName, ((IMenuItem)menu.MenuItems[0]).Text);

			menu = helper.GetPostMenuItem(option => { }, JobInvoicingPostingOption.Costs);
			AssertEquals("menu.Text", expectedPostMenuName, menu.Text);
			AssertEquals("menu.MenuItems.Count", 1, menu.MenuItems.Count);
			AssertEquals("menu.MenuItems[0].Text", expectedCostsMenuName, ((IMenuItem)menu.MenuItems[0]).Text);

			menu = helper.GetPostMenuItem(option => { }, JobInvoicingPostingOption.ConsolCosts);
			AssertEquals("menu.Text", expectedPostMenuName, menu.Text);
			AssertEquals("menu.MenuItems.Count", 1, menu.MenuItems.Count);
			AssertEquals("menu.MenuItems[0].Text", expectedConsolCostsOnlyMenuName, ((IMenuItem)menu.MenuItems[0]).Text);

			menu = helper.GetPostMenuItem(option => { }, JobInvoicingPostingOption.Agent, JobInvoicingPostingOption.All);
			AssertEquals("menu.Text", expectedPostMenuName, menu.Text);
			AssertEquals("menu.MenuItems.Count", 2, menu.MenuItems.Count);
			AssertEquals("menu.MenuItems[0].Text", expectedAgentMenuName, ((IMenuItem)menu.MenuItems[0]).Text);
			AssertEquals("menu.MenuItems[1].Text", expectedAllMenuName, ((IMenuItem)menu.MenuItems[1]).Text);

			menu = helper.GetPostMenuItem(option => { }, JobInvoicingPostingOption.Agent, JobInvoicingPostingOption.Costs);
			AssertEquals("menu.Text", expectedPostMenuName, menu.Text);
			AssertEquals("menu.MenuItems.Count", 2, menu.MenuItems.Count);
			AssertEquals("menu.MenuItems[0].Text", expectedAgentMenuName, ((IMenuItem)menu.MenuItems[0]).Text);
			AssertEquals("menu.MenuItems[1].Text", expectedCostsMenuName, ((IMenuItem)menu.MenuItems[1]).Text);
		}
	}
}
