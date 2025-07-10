using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.Testing.JobInvoicing.ProfitLoss;
using Enterprise.Accounting.Integration;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class ConsolJobProfitLossControlTest : JobProfitLossControlTest
	{
		protected override JobProfitLossControl ControlToTest
		{
			get { return new ConsolJobProfitLossControl(); }
		}

		protected override ZForm GetPopulatedForm()
		{
			JobProfitLoss profitLoss = new JobProfitLoss(Factory);
			profitLoss.SetJobPKs(new ZGuid[] { ZGuid.NewZGuid() });

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();

			consol.ProfitLossContainer.Add(profitLoss);

			return new ZForm(consol);
		}

		protected override string GetPrintingMessageWhenControlWithoutSecurity()
		{
			return "Please save this Consol before printing the Job Profit Document";
		}

		protected override void SetDataBinding(JobProfitLoss pl, IJobCostingPlugIn consol, JobProfitLossControl ctrl)
		{
			consol.ProfitLossContainer.Add(pl);
			ctrl.SetDataBinding(consol, "");
		}

		protected override object GetBindingBizO(bool isAccrual, string jobStatus = null)
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			if (isAccrual)
			{
				TestObjectCreator.CreateAccrual(job);
			}
			else
			{
				TestObjectCreator.CreateWIP(job);
			}
			if (!string.IsNullOrEmpty(jobStatus))
			{
				job.JH_Status = jobStatus;
			}
			Factory.Save();

			var pl = job.ProfitLoss[0];
			pl.ProfitLossDetails.Load();

			AssertEquals(1, pl.ProfitLossDetails.Count);

			consol.ProfitLossContainer.Add((JobProfitLoss)pl);
			return consol;
		}

		public override void TestPrintJobProfitDocumentDisplaysPrintingOptionsFormWhenJobIsSaved()
		{
			try
			{
				ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
				Factory.Save();

				using (ZForm form = new ZForm())
				{
					using (ConsolJobProfitLossControl ctrl = (ConsolJobProfitLossControl)ControlToTest)
					{
						ctrl.SetDataBinding(consol, "");
						ctrl.Show();
						ctrl.JobProfitReportButton_Click_ForTestOnly(null, null);
						AssertNotNull("Last form shown should not be null", ZFormModaliser.LastFormShownDialogForTest);
						AssertEquals("Last form shown", typeof(ConsolJobProfitDocumentPrintingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

						ZFormModaliser.LastFormShownDialogForTest.Close();
						ZFormModaliser.LastFormShownDialogForTest.Dispose();
						ZFormModaliser.LastFormShownDialogForTest = null;

						consol.HasChanges = true;
						ctrl.JobProfitReportButton_Click_ForTestOnly(null, null);
						AssertNull("Last form shown should be null", ZFormModaliser.LastFormShownDialogForTest);

						string expectedError = "Please save this Consol before printing the Job Profit Document";
						AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
			finally
			{
				if (ZFormModaliser.LastFormShownDialogForTest != null)
				{
					ZFormModaliser.LastFormShownDialogForTest.Close();
					ZFormModaliser.LastFormShownDialogForTest.Dispose();
				}
			}
		}

		public void TestNoNullReferenceExceptionWhenPrintProfitReport()
		{
			var originalResultForShowDialog = ZFormModaliser.ResultToReturnFromShowDialog;

			try
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var commandFilter = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, Core.Constants.MenuNameConstantsForPrinting.ConsolJobProfitDocument);
				commandFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_IsSystemDefined, SQLComparisonOperator.Equal, Core.Constants.BooleanTrueChar);
				var consolJobProfitDocument = Factory.LoadTop1<DocumentCommand>(commandFilter);
				commandFilter = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "Agent Departure Notice");
				commandFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Legacy");
				var otherDocument = Factory.LoadTop1<DocumentCommand>(commandFilter);

				var pivot = Factory.New<StmMenuMenuPivotBase>();
				pivot.SF_SU_Inward = consolJobProfitDocument.PK;
				pivot.SF_SU_Outward = otherDocument.PK;
				pivot.SF_OverriddenBusinessContext = ZString.Empty;
				consolJobProfitDocument.ChildMenus.Add(pivot);

				Factory.Save();
				using (var ctrl = (ConsolJobProfitLossControl)ControlToTest)
				{
					ctrl.SetDataBinding(consol, "");
					ctrl.Show();

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					AssertNoExceptionThrown(() => ctrl.JobProfitReportButton_Click_ForTestOnly(null, null));
				}
			}
			finally
			{
				ZFormModaliser.ResultToReturnFromShowDialog = originalResultForShowDialog;
				if (ZFormModaliser.LastFormShownDialogForTest != null)
				{
					ZFormModaliser.LastFormShownDialogForTest.Close();
					ZFormModaliser.LastFormShownDialogForTest.Dispose();
				}
			}
		}

		public void TestGlobalJobCostingTabPageVisibility()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();

			using (ZForm form = new ZForm())
			using (ConsolJobProfitLossControl ctl = (ConsolJobProfitLossControl)ControlToTest)
			{
				ctl.SetDataBinding(consol, "");
				ctl.Show();
				AssertEquals("GlobalJobCostingTabPage_ForTestOnly shouldn't be visible", false, ctl.GlobalJobCostingTabPage_ForTestOnly.TabVisible);
			}
		}

		public new void TestDetailsTabNotLoadedOnBindingOrFirstShownEvent()
		{
			Assert(true);
		}

		public new void TestSelectingTabOnlyUpdatesTheCorrespondingTab()
		{
			Assert(true);
		}

		public new void TestDetailsFndButtonClickOnlyUpdatesDetailsAndSummaryTab()
		{
			Assert(true);
		}

		public new void TestGlobalFndButtonClickOnlyUpdatesGlobalTab()
		{
			Assert(true);
		}

		public new void TestSummaryFindButtonClickOnlyUpdatesSummaryTab()
		{
			Assert(true);
		}

		public new void TestGlobalJobCostingTabPageVisibilityDependsOnRegistrySetting()
		{
			Assert(true);
		}

		public new void TestSecurityGlobalJobCostingTab_GlobalChargeConsolSendingAgent()
		{
			Assert(true);
		}

		public new void TestSecurityGlobalJobCostingTab_GlobalChargeConsolReceivingAgent()
		{
			Assert(true);
		}

		public new void TestSecurityGlobalJobCostingTab_GlobalChargeConsolReceivingAgentARSettlementGroup()
		{
			Assert(true);
		}

		public new void TestSecurityGlobalJobCostingTab_GlobalChargeConsolSendingAgentARSettlementGroup()
		{
			Assert(true);
		}

		public new void TestSecurityGlobalJobCostingTab_GlobalChargeControllingAgent()
		{
			Assert(true);
		}

		public new void TestSecurityGlobalJobCostingTab_GlobalChargeAdministrationUser()
		{
			Assert(true);
		}

		public new void TestSecurityGlobalJobCostingTab_GlobalChargeDetails()
		{
			Assert(true);
		}

		public new void TestSecurityGlobalJobCostingTab_NonForwardingShipmentDescendants()
		{
			Assert(true);
		}

		public override void TestPrintingDocumentForAJobWithoutAValidParentCausesNoException()
		{
			Assert(true);
		}

		protected override void AssertGlobalChargeDetailTabWhenSwitchingTab(JobProfitLossControl ctrl, int count, bool clickBtn = false)
		{
		}

		protected override SecurityCheckpoint PluginSecurityForTest
		{
			get { return Env.Security.MaintainConsol; }
		}

		protected override BusinessObject PrepareBindingBizo()
		{
			JobProfitLoss profitLoss = new JobProfitLoss(Factory);
			profitLoss.SetJobPKs(new ZGuid[] { base.JobWithParent.PK });

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.ProfitLossContainer.Add(profitLoss);

			return consol;
		}

		#region RegenerateJournalEntries

		protected override object GetTestBindingObject()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.ProfitLossContainer.Add(JobProfitLossForRegenerateJournalEntriesTest);
			return consol;
		}

		protected override object GetTestBindingObjectForTestMessage()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.ProfitLossContainer.Add(JobProfitLossForRegenerateJournalEntriesTestMessage);
			return consol;
		}

		#endregion
	}
}
