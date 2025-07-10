using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class NewApportionmentMenuItemManagerTest : TestCaseWithFactory
	{
		public void TestDeleteGwSellApportionmentWithNoSecurity()
		{
			CreateGatewayConsolWithSellApportionment(true);

			var listing = GatewayConsol.GetApportionments(true);
			using (var form = new ZForm(listing))
			using (var grid = new ZGrid())
			{
				Env.Security.GatewayConsolJobInvoicingReverseSellApportionment.IsAllowed = false;
				var menuItem = setUpGridAndMenuItem(grid, form, listing);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				menuItem.PerformClick();
				AssertEquals(Env.Security.GatewayConsolJobInvoicingReverseSellApportionment.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDeleteGwSellApportionmentWithNoRowSelected()
		{
			CreateGatewayConsolWithSellApportionment(true);

			var listing = GatewayConsol.GetApportionments(true);
			using (var form = new ZForm(listing))
			using (var grid = new ZGrid())
			{
				var menuItem = setUpGridAndMenuItem(grid, form, listing);
				grid?.ListManager?.RemoveAt(0);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				menuItem.PerformClick();
				AssertEquals("Please click on a row before performing Reverse/Delete.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDeleteGwSellApportionmentWithoutAJRJ()
		{
			CreateGatewayConsolWithSellApportionment(true);

			var listing = GatewayConsol.GetApportionments(true);
			using (GatewayJob)
			{
				listing = GatewayConsol.GetApportionments(true);
				using (var form = new ZForm(listing))
				using (var grid = new ZGrid())
				{
					var menuItem = setUpGridAndMenuItem(grid, form, listing);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					AssertEquals(0, LoadAutoJobRevenueJournals().Length);
					AssertEquals(false, GatewayConsolCost.IsDeleted);
					var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, GatewayConsol.PK));
					AssertEquals(1, consolCosts.Length);
					AssertEquals(1, GatewayJob.Charges.Count);
					AssertEquals(250m, GatewayCharge.JR_LocalSellAmt);
					AssertEquals(250m, GatewayCharge.JR_OSSellAmt);

					menuItem.PerformClick();
					Factory.Save();
					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals(0, LoadAutoJobRevenueJournals().Length);
					AssertEquals(true, GatewayConsolCost.IsDeleted);
					consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, GatewayConsol.PK));
					AssertEquals(0, consolCosts.Length);
					AssertEquals(1, GatewayJob.Charges.Count);
					AssertEquals(0m, GatewayCharge.JR_LocalSellAmt);
					AssertEquals(0m, GatewayCharge.JR_OSSellAmt);
				}
			}
		}

		public void TestDeleteGwSellApportionmentWithOneApportionment() => RunDeleteGwSellApportionmentTest(false);
		public void TestDeleteGwSellApportionmentWithMultipleApportionments() => RunDeleteGwSellApportionmentTest(true);

		void RunDeleteGwSellApportionmentTest(bool shouldCreateMultipleShipments)
		{
			CreateGatewayConsolWithSellApportionment(shouldCreateMultipleShipments);

			using (GatewayJob)
			{
				Factory.Save();
				AssertEquals(250m, GatewayConsolCost.E6_OSCostAmount);

				AssertEquals(1, GatewayJob.Charges.Count);
				AssertEquals(250m, GatewayCharge.JR_OSSellAmt);

				var aJRJs = LoadAutoJobRevenueJournals();
				AssertEquals(1, aJRJs.Length);
				AssertEquals(false, aJRJs[0].IsCancelled);

				foreach (ForwardingShipment shipment in GatewayConsol.Shipments)
				{
					AssertEquals(1, ((Job)shipment.Job).Charges.Count);
				}

				var listing = GatewayConsol.GetApportionments(true);
				using (var form = new ZForm(listing))
				using (var grid = new ZGrid())
				{
					var menuItem = setUpGridAndMenuItem(grid, form, listing);

					grid.Select(0);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menuItem.PerformClick();

					var jobRevenueJournalForm = ZFormModaliser.LastFormShownForTest as JobRevenueJournalForm;
					jobRevenueJournalForm.FireSaveButton();

					Factory.Save();

					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);

					aJRJs = LoadAutoJobRevenueJournals();
					AssertEquals(2, aJRJs.Length);
					AssertEquals(2, aJRJs.Count(x => x.IsCancelled));

					AssertEquals(true, GatewayConsolCost.IsDeleted);
					AssertEquals(true, GatewayCharge.IsDeleted);
					AssertEquals(0, GatewayJob.Charges.Count);

					foreach (ForwardingShipment shipment in GatewayConsol.Shipments)
					{
						AssertEquals(0, ((Job)shipment.Job).Charges.Count);
					}
				}
			}
		}

		TestObjectCreator TestObjectCreator;
		ForwardingConsol GatewayConsol;
		OrgHeader GatewayAgent;
		Job GatewayJob;
		Charge GatewayCharge;
		JobConsolCost GatewayConsolCost;

		MenuItem setUpGridAndMenuItem(ZGrid grid, ZForm form, ApportionmentListing listing)
		{
			grid.BindTo = "CostsFilteredCollection";
			grid.Columns.AddTextColumn("JR_Desc", 100);
			form.Controls.Add(grid);
			form.Show();
			grid.SetDataBinding(listing, "CostsFilteredCollection");

			new NewApportionmentMenuItemManager(grid, listing).AddMenuItem();
			var menuItem = grid.ContextMenu.MenuItems.FindByText("Reverse/Delete Gateway Sell Apportionment");
			AssertNotNull(menuItem);

			grid.Select(0);
			return menuItem;
		}

		JobRevenueJournal[] LoadAutoJobRevenueJournals()
		{
			var aJRJQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.JobRevenueJournal);
			aJRJQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, TransactionCategory.Codes.AutoJobRevenueJournal);
			aJRJQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			var aJRJs = Factory.Load<JobRevenueJournal>(aJRJQuery);
			return aJRJs;
		}

		void CreateGatewayConsolWithSellApportionment(bool shouldCreateMultipleShipments = false)
		{
			TestObjectCreator = new TestObjectCreator(Factory);

			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.NewGuid());
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();
			GlbDepartment.CurrentDepartment.GE_Misc = false;
			Factory.Save();

			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());

			var aJRJQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.JobRevenueJournal);
			aJRJQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, TransactionCategory.Codes.AutoJobRevenueJournal);
			aJRJQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			var aJRJs = Factory.Load<JobRevenueJournal>(aJRJQuery);
			AssertEquals(0, aJRJs.Length);

			GatewayAgent = GlbCompany.CurrentCompany.OrgProxy;

			GatewayConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			GatewayConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			GatewayConsol.JK_UniqueConsignRef = "C10011991";
			GatewayConsol.JK_AgentType = AgentType.Agent;
			GatewayConsol.JK_RL_NKLoadPort = "AUSYD";
			GatewayConsol.JK_RL_NKDischargePort = "CNSHA";
			GatewayConsol.JK_OA_SendingForwarderAddress = GatewayAgent.MainAddress.PK;
			GatewayConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var gatewayAgentPort = GatewayConsol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			gatewayAgentPort.O5_OA_AgentOfficeAddress = GatewayConsol.JK_OA_SendingForwarderAddress;
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			GatewayConsol.Shipments.AddNew();

			if (shouldCreateMultipleShipments)
			{
				GatewayConsol.Shipments.AddNew();
			}

			Assert("Pre-condition", GatewayConsol.IsGateway());

			GatewayJob = TestObjectCreator.CreateJob(GatewayConsol);

			GatewayJob.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
			GatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

			GatewayCharge = GatewayJob.Charges.AddNew();

			TestObjectCreator.CC1.AC_AG_RevenueAccount = TestObjectCreator.GLHeader1.PK;
			TestObjectCreator.CC1.AC_AG_CostAccount = TestObjectCreator.GLHeader2.PK;
			GatewayCharge.JR_AC = TestObjectCreator.CC1.PK;

			GatewayCharge.JR_GE = TestObjectCreator.GEADepartment.PK;
			GatewayCharge.JR_OH_SellAccount = GatewayAgent.PK;
			GatewayCharge.JR_LocalSellAmt = 250;
			GatewayCharge.JR_LocalCostAmt = 0;
			GatewayCharge.JR_JH_InternalJob = GatewayJob.PK;
			GatewayCharge.JR_OH_SellAccount = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			GatewaySellToCostSynchroniser.Synchronise(GatewayJob);

			var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, GatewayConsol.PK));
			AssertEquals(1, consolCosts.Length);

			GatewayConsolCost = consolCosts.FirstOrDefault(c => c.E6_AC_ChargeCode == TestObjectCreator.CC1.PK);
			GatewayConsolCost.E6_ApportionmentMethod = "SHP";
			GatewayConsol.GetApportionments(true).PrepareForConsolCosting();

			if (shouldCreateMultipleShipments)
			{
				AssertEquals(2, GatewayConsolCost.ApportionmentCharges.Count);
				GatewayConsolCost.ApportionmentCharges[0].JR_OSCostAmt = 100;
				GatewayConsolCost.ApportionmentCharges[1].JR_OSCostAmt = 150;
			}
			else
			{
				AssertEquals(1, GatewayConsolCost.ApportionmentCharges.Count);
				GatewayConsolCost.ApportionmentCharges[0].JR_OSCostAmt = 250;
			}
		}

		protected override void TearDown()
		{
			if (GatewayConsol != null)
			{
				var apportionmentList = GatewayConsol.GetApportionments(true);
				AssertNotNull(apportionmentList);
				apportionmentList.ReleaseMutexes();
			}

			if (GatewayJob != null)
			{
				GatewayJob.Dispose();
			}

			base.TearDown();
		}
	}
}
