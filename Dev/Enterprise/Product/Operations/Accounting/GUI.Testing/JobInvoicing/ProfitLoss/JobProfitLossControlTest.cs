using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.GUI.JobManagement;
using Enterprise.Accounting.GUI.WipAccrual;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing.JobInvoicing.ProfitLoss
{
	[MasterFiles.Integration.Test.MatchAgainstOnlineFlightsInUnitTest]
	public class JobProfitLossControlTest : TestCaseWithFactory
	{
		public Job JobWithParent;

		protected override void SetUp()
		{
			base.SetUp();
			var shipment = TestObjectCreator.CreateShipment("S003", true);
			JobWithParent = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			Factory.Save();
		}

		protected virtual JobProfitLossControl ControlToTest
		{
			get { return new JobProfitLossControl(); }
		}

		public void TestDetailsTabNotLoadedOnBindingOrFirstShownEvent()
		{
			var ctrl = new Mock<JobProfitLossControl> { CallBase = true };
			using (ZForm form = GetPopulatedForm())
			{
				form.Controls.Add(ctrl.Object);
				form.Show();
				ctrl.Object.DetailsTabPage_ForTestOnly.NotifyBindingOrShowing();
				ctrl.Protected()
					.Verify("DetailsTabPage_TabInitialized", Times.Never(), ItExpr.IsAny<object>(), ItExpr.IsAny<EventArgs>());
				ctrl.VerifyAll();
				ctrl.Reset();
				ctrl.Protected()
					.Setup("DetailsTabPage_TabInitialized", ItExpr.IsAny<object>(), ItExpr.IsAny<EventArgs>());
				ctrl.Object.DetailsTabPage_ForTestOnly.Show();
				ctrl.VerifyAll();
			}
		}

		public void TestProfitLossSummaryFilterIsLoadedOnSummaryTabIsInitialized()
		{
			var jobProfitLossControlMock = new Mock<JobProfitLossControl> { CallBase = true };
			using (var form = new ZForm())
			{
				SetupConsolAndShipment();
				var pl = new JobProfitLoss(Factory);
				pl.SetJobPKs(new ZGuid[] { job.PK });
				pl.SetParent(shipment);
				pl.SetConsol(consol);

				var ctrl = jobProfitLossControlMock.Object;
				ctrl.SetDataBinding(pl, "");
				form.Controls.Add(ctrl);

				jobProfitLossControlMock
					.Protected()
					.Setup("HandleSummaryChargeHidingMessageLabelVisibility");

				ctrl.SummaryTabPage_ForTestOnly.Show();
				jobProfitLossControlMock.VerifyAll();
			}
		}

		public void TestProfitLossSummaryInfoIsNotLoadedOnSummaryTabDataBinding()
		{
			var jobProfitLossControlMock = new Mock<JobProfitLossControl> { CallBase = true };
			using (var form = new ZForm())
			{
				SetupConsolAndShipment();
				var pl = new JobProfitLoss(Factory);
				pl.SetJobPKs(new ZGuid[] { job.PK });
				pl.SetParent(shipment);
				pl.SetConsol(consol);

				var ctrl = jobProfitLossControlMock.Object;
				form.Controls.Add(ctrl);
				ctrl.SetDataBinding(pl, "");
				jobProfitLossControlMock
					.Protected()
					.Verify("HandleSummaryChargeHidingMessageLabelVisibility", Times.Never());
				jobProfitLossControlMock.VerifyAll();
			}
		}

		public void TestProfitLossGridColumns()
		{
			string[] columns = new string[] { "ZY_Calc_AC", "ZY_Calc_JH", "ZY_Calc_GB", "ZY_Calc_GE", "ZY_Calc_ChargeCodeDescription", "ZY_Calc_InvoiceDate", "ZY_Calc_LineAmount", "ZY_Calc_LineType",
				"ZY_Calc_PostDate", "ZY_Calc_FullyPaidDate", "ZY_Calc_TransactionNum", "ZY_Calc_Ledger", "ZY_Calc_TransactionType", "ZY_Calc_JobLocalReferenceNum", "ZY_Calc_OH", "ZY_Calc_RecognizedDate",
				"ZY_Calc_RecognitionType", "ZY_Calc_ConsolNum", "ZY_Calc_ReversalDate", "ZY_Calc_SystemCreateTime", "ZY_Calc_AuditedBy" };

			using (ZForm form = GetPopulatedForm())
			using (JobProfitLossControl ctrl = ControlToTest)
			{
				form.Controls.Add(ctrl);
				form.Show();
				ctrl.DetailsTabPage_ForTestOnly.Show();

				int i = 0;
				string[] profitLossGridColumns = new string[ctrl.ProfitLossGrid_ForTestOnly.Columns.Count];

				foreach (ZGridColumn column in ctrl.ProfitLossGrid_ForTestOnly.Columns)
				{
					profitLossGridColumns[i] = column.ColumnName;
					i++;
				}
				AssertContainsExactElementsInAnyOrder(columns, profitLossGridColumns);
			}
		}

		public void TestReverseAccrualHasNewFactory()
		{
			AssertReverseWIPAccrualHasNewFactory(true);
		}

		public void TestReverseWIPHasNewFactory()
		{
			AssertReverseWIPAccrualHasNewFactory(false);
		}

		void AssertReverseWIPAccrualHasNewFactory(bool isAccrual)
		{
			var bizObject = GetBindingBizO(isAccrual);

			using (var form = new ZForm(bizObject))
			using (JobProfitLossControl control = ControlToTest)
			{
				control.SetDataBinding(bizObject, "");
				control.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				form.Controls.Add(control);
				form.Show();
				control.DetailsTabPage_ForTestOnly.Show();
				control.ProfitLossGrid_ForTestOnly.SelectAllElements();

				var reverseMenuItem = control.ProfitLossGrid_ForTestOnly.ContextMenu.MenuItems.FindByText(control.ReverseWIPAccrualText_ForTestOnly);
				AssertNotNull("Reverse WIP/Accrual not found", reverseMenuItem);
				reverseMenuItem.PerformClick();
				AssertNotNull("Last form shown should not be null", ZFormModaliser.LastFormShownForTest);
				var lastForm = (ZForm)ZFormModaliser.LastFormShownForTest;
				var previousFactoryInstance = lastForm.BusinessEntity.Factory._Instance;
				lastForm.Dispose();
				reverseMenuItem.PerformClick();
				AssertNotNull("Last form shown should not be null", ZFormModaliser.LastFormShownForTest);
				lastForm = (ZForm)ZFormModaliser.LastFormShownForTest;
				try
				{
					AssertNotEquals("Every Reverse should use new factory.", previousFactoryInstance, lastForm.BusinessEntity.Factory._Instance);
				}
				finally
				{
					lastForm.Dispose();
				}
			}
		}

		public void TestCanNotReverseAccrealWhenRelatedJobStatusIsJFC()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var bizObject = GetBindingBizO(true, JobHeaderStatus.JobReadyForFinancialClosure.Code);
			var cacheValue = Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed;

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cacheValue))
			using (var form = new ZForm(bizObject))
			using (JobProfitLossControl control = ControlToTest)
			{
				control.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				form.Controls.Add(control);
				form.Show();
				control.DetailsTabPage_ForTestOnly.Show();
				control.ProfitLossGrid_ForTestOnly.SelectAllElements();

				var reverseMenuItem = control.ProfitLossGrid_ForTestOnly.ContextMenu.MenuItems.FindByText(control.ReverseWIPAccrualText_ForTestOnly);
				AssertNotNull("Reverse WIP/Accrual not found", reverseMenuItem);

				ZFormModaliser.LastFormShownForTest = null;
				reverseMenuItem.PerformClick();
				Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasError);
				Assert("Popup should be security error", UnitTestUserNotification.Instance.LastMessage.Contains("This transaction cannot be reversed as the related job has Ready For Financial Closure status."));
				AssertEquals("We show only the error popup", null, ZFormModaliser.LastFormShownForTest);
			}

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cacheValue))
			using (var form = new ZForm(bizObject))
			using (JobProfitLossControl control = ControlToTest)
			{
				control.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				form.Controls.Add(control);
				form.Show();
				control.DetailsTabPage_ForTestOnly.Show();
				control.ProfitLossGrid_ForTestOnly.SelectAllElements();

				var reverseMenuItem = control.ProfitLossGrid_ForTestOnly.ContextMenu.MenuItems.FindByText(control.ReverseWIPAccrualText_ForTestOnly);
				AssertNotNull("Reverse WIP/Accrual not found", reverseMenuItem);
				reverseMenuItem.PerformClick();
				var reverseForm = (WIPAccrualForm)ZFormModaliser.LastFormShownForTest;
				AssertNotNull("Last form shown should not be null", reverseForm);
			}
		}

		public void TestCanNotReverseWIPWhenRelatedJobStatusIsJFC()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var bizObject = GetBindingBizO(false, JobHeaderStatus.JobReadyForFinancialClosure.Code);
			var cacheValue = Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed;

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cacheValue))
			using (var form = new ZForm(bizObject))
			using (JobProfitLossControl control = ControlToTest)
			{
				control.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				form.Controls.Add(control);
				form.Show();
				control.DetailsTabPage_ForTestOnly.Show();
				control.ProfitLossGrid_ForTestOnly.SelectAllElements();

				var reverseMenuItem = control.ProfitLossGrid_ForTestOnly.ContextMenu.MenuItems.FindByText(control.ReverseWIPAccrualText_ForTestOnly);
				AssertNotNull("Reverse WIP/Accrual not found", reverseMenuItem);

				ZFormModaliser.LastFormShownForTest = null;
				reverseMenuItem.PerformClick();
				Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasError);
				Assert("Popup should be security error", UnitTestUserNotification.Instance.LastMessage.Contains("This transaction cannot be reversed as the related job has Ready For Financial Closure status."));
				AssertEquals("We show only the error popup", null, ZFormModaliser.LastFormShownForTest);
			}

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cacheValue))
			using (var form = new ZForm(bizObject))
			using (JobProfitLossControl control = ControlToTest)
			{
				control.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				form.Controls.Add(control);
				form.Show();
				control.DetailsTabPage_ForTestOnly.Show();
				control.ProfitLossGrid_ForTestOnly.SelectAllElements();

				var reverseMenuItem = control.ProfitLossGrid_ForTestOnly.ContextMenu.MenuItems.FindByText(control.ReverseWIPAccrualText_ForTestOnly);
				AssertNotNull("Reverse WIP/Accrual not found", reverseMenuItem);
				reverseMenuItem.PerformClick();
				var reverseForm = (WIPAccrualForm)ZFormModaliser.LastFormShownForTest;
				AssertNotNull("Last form shown should not be null", reverseForm);
			}
		}

		public void TestReverseAccrualFormUseANewFactory()
		{
			AssertReverseWIPFormUseANewFactory(true);
		}

		public void TestReverseWIPFormUseANewFactory()
		{
			AssertReverseWIPFormUseANewFactory(false);
		}

		public void AssertReverseWIPFormUseANewFactory(bool isAccrual)
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var bizObject = GetBindingBizO(isAccrual);

			using (var form = new ZForm(bizObject))
			using (JobProfitLossControl control = ControlToTest)
			{
				control.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				form.Controls.Add(control);
				form.Show();
				control.DetailsTabPage_ForTestOnly.Show();
				control.ProfitLossGrid_ForTestOnly.SelectAllElements();

				var reverseMenuItem = control.ProfitLossGrid_ForTestOnly.ContextMenu.MenuItems.FindByText(control.ReverseWIPAccrualText_ForTestOnly);
				AssertNotNull("Reverse WIP/Accrual not found", reverseMenuItem);
				reverseMenuItem.PerformClick();
				var reverseForm = (WIPAccrualForm)ZFormModaliser.LastFormShownForTest;
				AssertNotNull("Last form shown should not be null", reverseForm);
				var wipAccrualPK = ((BaseWIPAccrual)reverseForm.BusinessEntity).PK;
				reverseForm.Close();

				var newFactory = new BusinessObjectFactory();
				newFactory.RefreshEnabled = false;
				var wipLoadInNewFactory = newFactory.Load<BaseWIPAccrual>(wipAccrualPK);
				wipLoadInNewFactory.Reverse();
				newFactory.Save();

				ZFormModaliser.LastFormShownForTest = null;
				reverseMenuItem.PerformClick();
				Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasError);
				Assert("Popup should be security error", UnitTestUserNotification.Instance.LastMessage.Contains("This transaction has already been reversed."));
				AssertEquals("We show only the error popup", null, ZFormModaliser.LastFormShownForTest);
			}
		}

		protected virtual object GetBindingBizO(bool isAccrual, string jobStatus = null)
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
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
			return pl;
		}

		public void TestReverseWIP()
		{
			bool isSingleReverseAllowed = Env.Security.ReverseSingleWipOrAccrual.IsAllowed;
			bool isMultipleReverseAllowed = Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed;
			try
			{
				using (JobProfitLossControl ctrl = ControlToTest)
				{
					ctrl.Show();
					ctrl.DetailsTabPage_ForTestOnly.Show();
					ctrl.SummaryTabPage_ForTestOnly.Show();

					var wIPToReverse = TestObjectCreator.CreateWIP();
					Factory.Save();

					Env.Security.ReverseSingleWipOrAccrual.IsAllowed = false;
					Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed = false;

					var controller = ZControllerFactory.Create(ControllerIDs.WIP);
					ctrl.ShowWIPAccrualForReversal_ForTestOnly(wIPToReverse, controller);
					AssertNull("Last shown form should be null", controller.LastShownForm);

					Env.Security.ReverseSingleWipOrAccrual.IsAllowed = true;
					Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed = true;

					ctrl.ShowWIPAccrualForReversal_ForTestOnly(wIPToReverse, controller);
					var form1 = (ZForm)controller.LastShownForm;
					var wip = (WIP)form1.BusinessEntity;
					Assert("WipAndAccrual BusinessContext is set", wip.IsReversing);
					AssertNotNull("Last shown form should not be null", form1);
					AssertType(typeof(WIPForm), form1);
					AssertType(typeof(WIP), wip);
					form1.Close();
					Assert("WipAndAccrual BusinessContext is removed", !wip.IsReversing);

					wIPToReverse.Reverse();
					Factory.Save();

					ctrl.ShowWIPAccrualForReversal_ForTestOnly(wIPToReverse, controller);
					AssertEquals("Last shown form should be the old form and not a new one", form1, controller.LastShownForm);

					Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasError);
					Assert("Popup should be security error", UnitTestUserNotification.Instance.LastMessage.Contains("This transaction has already been reversed."));
				}
			}
			finally
			{
				Env.Security.ReverseSingleWipOrAccrual.IsAllowed = isSingleReverseAllowed;
				Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed = isMultipleReverseAllowed;
			}
		}

		[ExpectNoExceptions]
		public void TestReverseWIPDoesNotThrowNullReferenceException()
		{
			using (JobProfitLossControl ctrl = ControlToTest)
			{
				ctrl.Show();
				ctrl.DetailsTabPage_ForTestOnly.Show();
				ctrl.SummaryTabPage_ForTestOnly.Show();

				WIP wIPToReverse = TestObjectCreator.CreateWIP();
				Factory.Save();

				var controller = ZControllerFactory.Create(ControllerIDs.WIP);
				ctrl.ShowWIPAccrualForReversal_ForTestOnly(wIPToReverse, controller);
				var form1 = (ZForm)controller.LastShownForm;

				ctrl.ShowWIPAccrualForReversal_ForTestOnly(wIPToReverse, controller);
				var form2 = (ZForm)controller.LastShownForm;

				form1.Close();
				form2.Close();
			}
		}

		public void TestReverseAccrual()
		{
			bool isSingleReverseAllowed = Env.Security.ReverseSingleWipOrAccrual.IsAllowed;
			bool isMultipleReverseAllowed = Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed;
			try
			{
				using (JobProfitLossControl ctrl = ControlToTest)
				{
					ctrl.Show();
					ctrl.DetailsTabPage_ForTestOnly.Show();
					ctrl.SummaryTabPage_ForTestOnly.Show();

					Accrual accrualToReverse = Factory.New<Accrual>();
					JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
					charge1.JR_AL_APLine = accrualToReverse.PK;
					accrualToReverse.AL_JH = charge1.JR_JH;
					accrualToReverse.AL_AC = charge1.JR_AC;
					accrualToReverse.AL_GB = charge1.JR_GB;
					accrualToReverse.AL_GE = charge1.JR_GE;
					Factory.Save();

					Env.Security.ReverseSingleWipOrAccrual.IsAllowed = false;
					Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed = false;

					var controller = ZControllerFactory.Create(ControllerIDs.Accrual);
					ctrl.ShowWIPAccrualForReversal_ForTestOnly(accrualToReverse, controller);
					AssertNull("Last shown form should be null", controller.LastShownForm);

					Env.Security.ReverseSingleWipOrAccrual.IsAllowed = true;
					Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed = true;

					ctrl.ShowWIPAccrualForReversal_ForTestOnly(accrualToReverse, controller);
					AssertNotNull("Last shown form should not be null", controller.LastShownForm);
					var form1 = (ZForm)controller.LastShownForm;
					var accrual = (Accrual)form1.BusinessEntity;
					Assert("WipAndAccrual BusinessContext is set", accrual.IsReversing);
					AssertNotNull("Last shown form should not be null", form1);
					AssertType(typeof(AccrualForm), form1);
					AssertType(typeof(Accrual), accrual);
					form1.Close();
					Assert("WipAndAccrual BusinessContext is removed", !accrual.IsReversing);

					accrualToReverse.Reverse();

					ctrl.ShowWIPAccrualForReversal_ForTestOnly(accrualToReverse, controller);
					AssertEquals("Last shown form should be the old form and not a new one", form1, controller.LastShownForm);
					Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasError);
					Assert("Popup should be security error", UnitTestUserNotification.Instance.LastMessage.Contains("This transaction has already been reversed."));

					accrualToReverse.AL_ReverseDate = ZDateTime.Empty;

					TestObjectCreator creator = new TestObjectCreator(Factory);

					JobConsolCost cost = Factory.NewWithValidTestData<ForwardingConsol>().GetApportionments().CostsCollection.TryAddNew();
					cost.E6_AC_ChargeCode = creator.CC1.PK;
					cost.E6_GC = GlbCompany.CurrentCompany.PK;

					charge1.JR_AL_APLine = accrualToReverse.PK;
					charge1.JR_E6 = cost.PK;

					JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
					charge2.JR_E6 = cost.PK;
					Factory.Save();

					ctrl.ShowWIPAccrualForReversal_ForTestOnly(accrualToReverse, controller);
					AssertEquals("Last shown form should be the old form and not a new one", form1, controller.LastShownForm);
					Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasError);
					ZString errorMessage = "This accrual is apportioned at Consol level." + System.Environment.NewLine;
					errorMessage += "Please go to the Costing Tab of Consol " + accrualToReverse.JK_UniqueConsignRef + " to reverse this accrual.";
					Assert("Popup should be security error", UnitTestUserNotification.Instance.LastMessage.Contains(errorMessage));
				}
			}
			finally
			{
				Env.Security.ReverseSingleWipOrAccrual.IsAllowed = isSingleReverseAllowed;
				Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed = isMultipleReverseAllowed;
			}
		}

		public void TestSelectingTabOnlyUpdatesTheCorrespondingTab()
		{
			AccountingConfigurationRegistry.Instance.EnableGlobalChargesDetail.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var securityHelper = new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing);
			var securityCheckpoint = securityHelper.GetInvSecurity(SecurityCore.GlobalChargeDetails);
			securityCheckpoint.IsAllowed = true;
			SetupConsolAndShipment();

			var pl = new JobProfitLoss(Factory);
			pl.SetJobPKs(new ZGuid[] { job.PK });
			pl.SetParent(shipment);
			pl.SetConsol(consol);

			using (var form = new ZForm())
			using (var ctrl = ControlToTest)
			{
				ctrl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				SetDataBinding(pl, consol, ctrl);
				form.Controls.Add(ctrl);
				form.Show();

				ctrl.TabControl_ForTestOnly.SelectTab(0);
				AssertEquals("Selecting summary tab loads summary grid", 3, ctrl.ProfitLossSummaryGrid_ForTestOnly.ListManager.List.Count);
				AssertNull("Details grid is not yet loaded", ctrl.ProfitLossGrid_ForTestOnly.ListManager);
				AssertNull("Global grid is not yet loaded", ctrl.GJCProfitLossGrid_ForTestOnly.ListManager);

				ctrl.TabControl_ForTestOnly.SelectTab(1);
				AssertEquals("Selecting details tab loads details grid", 8, ctrl.ProfitLossGrid_ForTestOnly.ListManager.List.Count);
				AssertNull("Global grid is not yet loaded", ctrl.GJCProfitLossGrid_ForTestOnly.ListManager);

				ctrl.TabControl_ForTestOnly.SelectTab(2);
				AssertEquals("Selecting global tab loads global grid", 8, ctrl.GJCProfitLossGrid_ForTestOnly.ListManager.List.Count);
			}
		}

		public void TestSwitchingTabWontUpdatesTheCorrespondingTab()
		{
			AccountingConfigurationRegistry.Instance.EnableGlobalChargesDetail.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var securityHelper = new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing);
			var securityCheckpoint = securityHelper.GetInvSecurity(SecurityCore.GlobalChargeDetails);
			securityCheckpoint.IsAllowed = true;
			SetupConsolAndShipment();

			var pl = new JobProfitLoss(Factory);
			pl.SetJobPKs(new ZGuid[] { job.PK });
			pl.SetParent(shipment);
			pl.SetConsol(consol);

			using (var form = new ZForm())
			using (var ctrl = ControlToTest)
			{
				ctrl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				SetDataBinding(pl, consol, ctrl);
				form.Controls.Add(ctrl);
				form.Show();

				ctrl.TabControl_ForTestOnly.SelectTab(0);
				AssertEquals("Selecting summary tab loads summary grid", 3, ctrl.ProfitLossSummaryGrid_ForTestOnly.ListManager.List.Count);
				AssertNull("Details grid is not yet loaded", ctrl.ProfitLossGrid_ForTestOnly.ListManager);
				AssertNull("Global grid is not yet loaded", ctrl.GJCProfitLossGrid_ForTestOnly.ListManager);

				ctrl.TabControl_ForTestOnly.SelectTab(1);
				AssertEquals("Selecting details tab loads details grid", 8, ctrl.ProfitLossGrid_ForTestOnly.ListManager.List.Count);
				AssertNull("Global grid is not yet loaded", ctrl.GJCProfitLossGrid_ForTestOnly.ListManager);

				AssertGlobalChargeDetailTabWhenSwitchingTab(ctrl, 8);

				var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV10011", TestObjectCreator.AUD, 1M, 150M, 0M, 150M, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC10.PK);
				var revenueLine = arInvoice.Lines[0];
				revenueLine.AL_JH = job.PK;
				var charge = TestObjectCreator.CreateJobCharge(revenueLine, job, TestObjectCreator.CC10, TestObjectCreator.AUD);

				Factory.Save();

				ctrl.TabControl_ForTestOnly.SelectTab(0);
				AssertEquals("Selecting summary tab loads summary grid", 3, ctrl.ProfitLossSummaryGrid_ForTestOnly.ListManager.List.Count);

				ctrl.TabControl_ForTestOnly.SelectTab(1);
				AssertEquals("Selecting details tab loads details grid", 8, ctrl.ProfitLossGrid_ForTestOnly.ListManager.List.Count);

				AssertGlobalChargeDetailTabWhenSwitchingTab(ctrl, 8);

				ctrl.TabControl_ForTestOnly.SelectTab(0);
				ctrl.FindButton_Click_ForTestOnly(null, null);
				AssertEquals("Selecting summary tab loads summary grid", 4, ctrl.ProfitLossSummaryGrid_ForTestOnly.ListManager.List.Count);

				ctrl.TabControl_ForTestOnly.SelectTab(1);
				ctrl.FindButton_Click_ForTestOnly(null, null);
				AssertEquals("Selecting details tab loads details grid", 10, ctrl.ProfitLossGrid_ForTestOnly.ListManager.List.Count);

				AssertGlobalChargeDetailTabWhenSwitchingTab(ctrl, 10, true);
			}
		}

		protected virtual void AssertGlobalChargeDetailTabWhenSwitchingTab(JobProfitLossControl ctrl, int count, bool clickBtn = false)
		{
			ctrl.TabControl_ForTestOnly.SelectTab(2);
			if (clickBtn)
			{
				ctrl.FindButton_Click_ForTestOnly(null, null);
			}
			AssertEquals("Selecting global tab loads global grid", count, ctrl.GJCProfitLossGrid_ForTestOnly.ListManager.List.Count);
		}

		public void TestGlobalFndButtonClickOnlyUpdatesGlobalTab()
		{
			SetupConsolAndShipment();
			var pl = new JobProfitLoss(Factory);
			pl.SetJobPKs(new ZGuid[] { job.PK });
			pl.SetParent(shipment);

			using (var form = new ZForm())
			using (var ctrl = ControlToTest)
			{
				ctrl.GlobalJobCostingTabPage_ForTestOnly.TabVisible = true;
				SetDataBinding(pl, consol, ctrl);
				form.Controls.Add(ctrl);

				form.Show();

				Assert("should show all charges", pl.ProfitLossSummaryFilteredDetails.Cast<ProfitLossSummaryDetailView>().All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK || x.ChargeCode.PK == TestObjectCreator.CC3.PK));
				Assert("should show all charges", pl.ProfitLossFilteredDetails.Cast<ProfitLossDetailView>().All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK || x.ChargeCode.PK == TestObjectCreator.CC3.PK));
				Assert("should show all charges", pl.GlobalJobCostingProfitLoss.All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK || x.ChargeCode.PK == TestObjectCreator.CC3.PK));

				AssertEquals(150M, pl.TotalRevenue);
				AssertEquals(-50M, pl.TotalCost);
				AssertEquals(150M, pl.TotalWIP);
				AssertEquals(-350M, pl.TotalAccrual);

				ctrl.TabControl_ForTestOnly.SelectedTab = ctrl.GlobalJobCostingTabPage_ForTestOnly;
				pl.Filter.ChargeCodeFilter = TestObjectCreator.CC1.AC_Code;
				ctrl.FindButton_Click_ForTestOnly(null, null);

				Assert("The Sumamry tab is not refreshed", !pl.ProfitLossSummaryFilteredDetails.Cast<ProfitLossSummaryDetailView>().All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK));
				Assert("The Details tab is not refreshed", !pl.ProfitLossFilteredDetails.Cast<ProfitLossDetailView>().All(x => (x).ChargeCode.PK == TestObjectCreator.CC1.PK));
				Assert("The Global tab is refreshed", pl.GlobalJobCostingProfitLoss.All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK));

				AssertEquals(150M, pl.TotalRevenue);
				AssertEquals(-50M, pl.TotalCost);
				AssertEquals(150M, pl.TotalWIP);
				AssertEquals(-350M, pl.TotalAccrual);

				ctrl.TabControl_ForTestOnly.SelectedTab = ctrl.DetailsTabPage_ForTestOnly;
				ctrl.FindButton_Click_ForTestOnly(null, null);
				Assert("The Sumamry tab is refreshed", pl.ProfitLossSummaryFilteredDetails.Cast<ProfitLossSummaryDetailView>().All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK));
				Assert("The Details tab is refreshed", pl.ProfitLossFilteredDetails.Cast<ProfitLossDetailView>().All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK));

				AssertEquals(150M, pl.TotalRevenue);
				AssertEquals(-50M, pl.TotalCost);
				AssertEquals(150M, pl.TotalWIP);
				AssertEquals(-150M, pl.TotalAccrual);
			}
		}

		public void TestDetailsFndButtonClickOnlyUpdatesDetailsAndSummaryTab()
		{
			SetupConsolAndShipment();
			var pl = new JobProfitLoss(Factory);
			pl.SetJobPKs(new ZGuid[] { job.PK });
			pl.SetParent(shipment);

			using (var form = new ZForm())
			using (var ctrl = ControlToTest)
			{
				ctrl.GlobalJobCostingTabPage_ForTestOnly.TabVisible = true;
				SetDataBinding(pl, consol, ctrl);
				form.Controls.Add(ctrl);
				form.Show();

				Assert("should show all charges", pl.ProfitLossSummaryFilteredDetails.Cast<ProfitLossSummaryDetailView>().All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK || x.ChargeCode.PK == TestObjectCreator.CC3.PK));
				Assert("should show all charges", pl.ProfitLossFilteredDetails.Cast<ProfitLossDetailView>().All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK || x.ChargeCode.PK == TestObjectCreator.CC3.PK));
				Assert("should show all charges", pl.GlobalJobCostingProfitLoss.All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK || x.ChargeCode.PK == TestObjectCreator.CC3.PK));

				AssertEquals(150M, pl.TotalRevenue);
				AssertEquals(-50M, pl.TotalCost);
				AssertEquals(150M, pl.TotalWIP);
				AssertEquals(-350M, pl.TotalAccrual);

				ctrl.TabControl_ForTestOnly.SelectedTab = ctrl.DetailsTabPage_ForTestOnly;
				pl.Filter.ChargeCodeFilter = TestObjectCreator.CC1.AC_Code;
				ctrl.FindButton_Click_ForTestOnly(null, null);

				Assert("The Sumamry tab is refreshed", pl.ProfitLossSummaryFilteredDetails.Cast<ProfitLossSummaryDetailView>().All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK));
				Assert("The Details tab is refreshed", pl.ProfitLossFilteredDetails.Cast<ProfitLossDetailView>().All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK));
				Assert("The Global tab is not refreshed", !pl.GlobalJobCostingProfitLoss.All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK));

				AssertEquals(150M, pl.TotalRevenue);
				AssertEquals(-50M, pl.TotalCost);
				AssertEquals(150M, pl.TotalWIP);
				AssertEquals(-150M, pl.TotalAccrual);

				ctrl.TabControl_ForTestOnly.SelectedTab = ctrl.GlobalJobCostingTabPage_ForTestOnly;
				ctrl.FindButton_Click_ForTestOnly(null, null);
				Assert("The Global tab is refreshed", pl.GlobalJobCostingProfitLoss.All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK));
			}
		}

		public void TestSummaryFindButtonClickOnlyUpdatesSummaryTab()
		{
			SetupConsolAndShipment();
			var pl = new JobProfitLoss(Factory);
			pl.SetJobPKs(new ZGuid[] { job.PK });
			pl.SetParent(shipment);

			using (var form = new ZForm())
			using (var ctrl = ControlToTest)
			{
				ctrl.GlobalJobCostingTabPage_ForTestOnly.TabVisible = true;
				form.Controls.Add(ctrl);
				SetDataBinding(pl, consol, ctrl);
				form.Show();
				ctrl.ProfitLossSummaryGrid_ForTestOnly.Focus();

				Assert("should show all charges", pl.ProfitLossSummaryFilteredDetails.Cast<ProfitLossSummaryDetailView>().All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK || x.ChargeCode.PK == TestObjectCreator.CC3.PK));
				Assert("should show all charges", pl.ProfitLossFilteredDetails.Cast<ProfitLossDetailView>().All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK || x.ChargeCode.PK == TestObjectCreator.CC3.PK));
				Assert("should show all charges", pl.GlobalJobCostingProfitLoss.All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK || x.ChargeCode.PK == TestObjectCreator.CC3.PK));

				AssertEquals(150M, pl.TotalRevenue);
				AssertEquals(-50M, pl.TotalCost);
				AssertEquals(150M, pl.TotalWIP);
				AssertEquals(-350M, pl.TotalAccrual);

				ctrl.TabControl_ForTestOnly.SelectedTab = ctrl.SummaryTabPage_ForTestOnly;
				pl.Filter.ChargeCodeFilter = TestObjectCreator.CC1.AC_Code;
				ctrl.FindButton_Click_ForTestOnly(null, null);

				Assert("The Sumamry tab is refreshed", pl.ProfitLossSummaryFilteredDetails.Cast<ProfitLossSummaryDetailView>().All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK));
				Assert("The Details tab is not refreshed", !pl.ProfitLossFilteredDetails.Cast<ProfitLossDetailView>().All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK));
				Assert("The Global tab is not refreshed", !pl.GlobalJobCostingProfitLoss.All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK));

				AssertEquals(150M, pl.TotalRevenue);
				AssertEquals(-50M, pl.TotalCost);
				AssertEquals(150M, pl.TotalWIP);
				AssertEquals(-150M, pl.TotalAccrual);

				ctrl.TabControl_ForTestOnly.SelectedTab = ctrl.DetailsTabPage_ForTestOnly;
				ctrl.FindButton_Click_ForTestOnly(null, null);
				Assert("The Details tab is refreshed", pl.ProfitLossFilteredDetails.Cast<ProfitLossDetailView>().All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK));
				Assert("The Global tab is not refreshed", !pl.GlobalJobCostingProfitLoss.All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK));

				ctrl.TabControl_ForTestOnly.SelectedTab = ctrl.GlobalJobCostingTabPage_ForTestOnly;
				ctrl.FindButton_Click_ForTestOnly(null, null);
				Assert("The Global tab is refreshed", pl.GlobalJobCostingProfitLoss.All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK));
			}
		}

		void SetupConsolAndShipment()
		{
			consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			shipment = TestObjectCreator.CreateShipment("S001001", consol);
			job = TestObjectCreator.CreateJob(shipment, createWithMutex: false);

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV10010", TestObjectCreator.AUD, 1M, 150M, 0M, 150M, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK);
			var revenueLine = arInvoice.Lines[0];
			revenueLine.AL_JH = job.PK;
			var charge = TestObjectCreator.CreateJobCharge(revenueLine, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "AP101", TestObjectCreator.AUD, 1M, 50M, 0M, 50M, 0M, TestObjectCreator.AALSHI, TestObjectCreator.CC1.PK);
			var costLine = apInvoice.Lines[0];
			costLine.AL_JH = job.PK;
			var cost = TestObjectCreator.CreateJobCharge(costLine, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

			var wip = TestObjectCreator.CreateWIP(job, TestObjectCreator.CC1, 1M, "WIP line", 100M);
			wip.AL_GB = TestObjectCreator.NonCurrentBranch.PK;

			var acr = TestObjectCreator.CreateAccrual(job, TestObjectCreator.CC3, 1M, "ACR line", 200M);
			acr.AL_GB = GlbBranch.CurrentBranch.PK;

			Factory.Save();
		}

		ForwardingConsol consol;
		ForwardingShipment shipment;
		Job job;

		protected virtual void SetDataBinding(JobProfitLoss pl, IJobCostingPlugIn consol, JobProfitLossControl ctrl)
		{
			ctrl.SetDataBinding(pl, "");
		}

		public void TestColours()
		{
			using (JobProfitLossControl ctrl = ControlToTest)
			{
				ctrl.Show();
				ctrl.DetailsTabPage_ForTestOnly.Show();
				ctrl.SummaryTabPage_ForTestOnly.Show();

				ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				Job shipmentJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
				shipmentJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
				shipmentJob.JH_GB = GlbBranch.CurrentBranch.PK;

				TestObjectCreator creator = new TestObjectCreator(Factory);

				AccTransactionLines wIPLine = Factory.NewWithValidTestData<AccTransactionLines>();
				wIPLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
				wIPLine.AL_JH = shipmentJob.PK;
				wIPLine.AL_AC = creator.CC1.PK;
				wIPLine.AL_GB = GlbBranch.CurrentBranch.PK;
				wIPLine.AL_RX_NKTransactionCurrency = creator.AUD.RX_Code;

				ARInvoiceLine rEVLine = Factory.NewWithValidTestData<ARInvoiceLine>();
				rEVLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
				rEVLine.AL_JH = shipmentJob.PK;
				rEVLine.AL_AC = creator.CC1.PK;
				rEVLine.AL_GB = GlbBranch.CurrentBranch.PK;
				rEVLine.AL_AH = Factory.NewWithValidTestData<ARInvoice>().PK;
				rEVLine.AL_RX_NKTransactionCurrency = creator.AUD.RX_Code;
				creator.CreateJobCharge(rEVLine, shipmentJob, creator.CC1, creator.AUD);

				AccTransactionLines aCRLine = Factory.NewWithValidTestData<AccTransactionLines>();
				aCRLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
				aCRLine.AL_JH = shipmentJob.PK;
				aCRLine.AL_AC = creator.CC1.PK;
				aCRLine.AL_GB = GlbBranch.CurrentBranch.PK;
				aCRLine.AL_RX_NKTransactionCurrency = creator.AUD.RX_Code;

				APInvoiceLine cST = Factory.NewWithValidTestData<APInvoiceLine>();
				cST.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
				cST.AL_JH = shipmentJob.PK;
				cST.AL_AC = creator.CC1.PK;
				cST.AL_GB = GlbBranch.CurrentBranch.PK;
				cST.AL_AH = Factory.NewWithValidTestData<APInvoice>().PK;
				cST.AL_RX_NKTransactionCurrency = creator.AUD.RX_Code;
				creator.CreateJobCharge(cST, shipmentJob, creator.CC1, creator.AUD);

				Factory.Save();

				JobProfitLoss pL = new JobProfitLoss(Factory);
				pL.SetJobPKs(new ZGuid[] { shipmentJob.PK });
				pL.ProfitLossDetails.Load();

				AssertEquals(4, pL.ProfitLossDetails.Count);

				ColourDecidingEventArgs args = new ColourDecidingEventArgs(GetFirstDetailLineMatchingType(pL, ZArchitecture.Core.TransactionLineTypes.WIP));
				ctrl.ProfitLossGrid_ColourDeciding_ForTestOnly(this, args);
				AssertEquals(Color.PaleGreen, args.Colour);

				args = new ColourDecidingEventArgs(GetFirstDetailLineMatchingType(pL, ZArchitecture.Core.TransactionLineTypes.Revenue));
				ctrl.ProfitLossGrid_ColourDeciding_ForTestOnly(this, args);
				AssertEquals(Color.MediumSeaGreen, args.Colour);

				args = new ColourDecidingEventArgs(GetFirstDetailLineMatchingType(pL, ZArchitecture.Core.TransactionLineTypes.Accrual));
				ctrl.ProfitLossGrid_ColourDeciding_ForTestOnly(this, args);
				AssertEquals(Color.LightSalmon, args.Colour);

				args = new ColourDecidingEventArgs(GetFirstDetailLineMatchingType(pL, ZArchitecture.Core.TransactionLineTypes.Cost));
				ctrl.ProfitLossGrid_ColourDeciding_ForTestOnly(this, args);
				AssertEquals(Color.Tomato, args.Colour);
			}
		}

		ProfitLossDetailView GetFirstDetailLineMatchingType(JobProfitLoss pL, string lineType)
		{
			ProfitLossDetailView result = null;
			foreach (ProfitLossDetailView detail in pL.ProfitLossFilteredDetails)
			{
				if (detail.ZY_Calc_LineType == lineType)
				{
					result = detail;
					break;
				}
			}
			return result;
		}

		public void TestContextMenu()
		{
			using (ZForm form = GetPopulatedForm())
			using (JobProfitLossControl ctrl = ControlToTest)
			{
				form.Controls.Add(ctrl);
				form.Show();
				ctrl.DetailsTabPage_ForTestOnly.Show();
				ctrl.SummaryTabPage_ForTestOnly.Show();

				bool reverseFound = false;
				bool viewFound = false;

				foreach (MenuItem item in ctrl.ProfitLossGrid_ForTestOnly.ContextMenu.MenuItems)
				{
					if (item.Text == ctrl.ReverseWIPAccrualText_ForTestOnly)
					{
						reverseFound = true;
					}
					if (item.Text == ctrl.ViewTransactionText_ForTestOnly)
					{
						viewFound = true;
					}
				}

				Assert(reverseFound);
				Assert(viewFound);
			}
		}

		public void TestAccountingVoucherMenuItem()
		{
			AssertAccountingVoucherMenuItem(Core.Constants.CountryCodes.China);
			AssertAccountingVoucherMenuItem(Core.Constants.CountryCodes.Taiwan);
			AssertAccountingVoucherMenuItem(Core.Constants.CountryCodes.Australia);
		}

		void AssertAccountingVoucherMenuItem(string countryCode)
		{
			var journal = TestObjectCreator.CreateJCJournalHeader(ZDateTime.Now, 100M);
			TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.CC1, JobWithParent, ZDateTime.Now, 100M);
			BusinessObject bindingObject = PrepareBindingBizo();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			using (ZForm form = new ZForm())
			using (JobProfitLossControl control = ControlToTest)
			{
				control.SetDataBinding(bindingObject, "");
				control.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				form.Controls.Add(control);
				form.Show();
				control.DetailsTabPage_ForTestOnly.Show();
				control.ProfitLossGrid_ForTestOnly.SelectAllElements();

				control.ProfitLossGrid_ForTestOnly.ContextMenu.DoPopup();
				var menuItem = control.ProfitLossGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Print Accounting Voucher");
				if (countryCode == Core.Constants.CountryCodes.China || countryCode == Core.Constants.CountryCodes.Taiwan)
				{
					AssertNotNull("when CN or TW login ,should not be null", menuItem);
					Assert("should be enabled", menuItem.Enabled);
				}
				else
				{
					AssertNull("should be null", menuItem);
				}
			}
		}

		public void TestAuditCFXJournal()
		{
			const string creator = "JYW";
			string auditor = GlbStaff.CurrentUser.GS_Code;

			var journal = TestObjectCreator.CreateJCJournalHeader(ZDateTime.Now, 100M);
			TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.CC1, JobWithParent, ZDateTime.Now, 100M);
			journal.AH_SystemCreateUser = creator;
			var journal1 = TestObjectCreator.CreateJCJournalHeader(ZDateTime.Now, 100M);
			TestObjectCreator.CreateJCJournalLine(journal1, TestObjectCreator.CC1, JobWithParent, ZDateTime.Now, 100M);
			journal1.AH_SystemCreateUser = creator;

			Factory.Save();

			AssertNotEquals("Precondition: The two users should not be identical.", creator, auditor);
			AssertEquals("Precondition: Auditer should be empty.", ZString.Empty, journal.AH_GS_NKAuditedBy);
			AssertEquals("Precondition: Auditer should be empty.", ZString.Empty, journal1.AH_GS_NKAuditedBy);

			BusinessObject bindingObject = PrepareBindingBizo();

			using (ZForm form = new ZForm())
			using (JobProfitLossControl control = ControlToTest)
			{
				control.SetDataBinding(bindingObject, "");
				control.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				form.Controls.Add(control);
				form.Show();
				control.DetailsTabPage_ForTestOnly.Show();
				control.ProfitLossGrid_ForTestOnly.SelectAllElements();

				control.ProfitLossGrid_ForTestOnly.ContextMenu.DoPopup();
				var menuItem = control.ProfitLossGrid_ForTestOnly.ContextMenu.MenuItems.FindByText(AccountingConstants.AuditAndCashActionText.AuditTransactionText);
				Assert("Menu item 'Audit Transaction' should be enabled.", menuItem.Enabled);
				menuItem.PerformClick();

				journal.Reload();
				journal1.Reload();

				AssertEquals("Journal should be audit by auditor.", auditor, journal.AH_GS_NKAuditedBy);
				AssertEquals("Journal should be audit by auditor.", auditor, journal1.AH_GS_NKAuditedBy);
			}
		}

		public void TestUndoAuditCFXJournal()
		{
			const string creator = "JYW";
			string auditor = GlbStaff.CurrentUser.GS_Code;

			var journal = TestObjectCreator.CreateJCJournalHeader(ZDateTime.Now, 100M);
			TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.CC1, JobWithParent, ZDateTime.Now, 100M);
			journal.AH_SystemCreateUser = creator;
			journal.AH_GS_NKAuditedBy = auditor;
			var journal1 = TestObjectCreator.CreateJCJournalHeader(ZDateTime.Now, 100M);
			TestObjectCreator.CreateJCJournalLine(journal1, TestObjectCreator.CC1, JobWithParent, ZDateTime.Now, 100M);
			journal1.AH_SystemCreateUser = creator;
			journal1.AH_GS_NKAuditedBy = auditor;

			Factory.Save();

			AssertNotEquals("Precondition: The two users should not be identical.", creator, auditor);
			AssertEquals("Precondition: Auditer should be auditor.", auditor, journal.AH_GS_NKAuditedBy);
			AssertEquals("Precondition: Auditer should be auditor.", auditor, journal1.AH_GS_NKAuditedBy);

			BusinessObject bindingObject = PrepareBindingBizo();

			using (ZForm form = new ZForm())
			using (JobProfitLossControl control = ControlToTest)
			{
				control.SetDataBinding(bindingObject, "");
				control.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				form.Controls.Add(control);
				form.Show();
				control.DetailsTabPage_ForTestOnly.Show();
				control.ProfitLossGrid_ForTestOnly.SelectAllElements();

				control.ProfitLossGrid_ForTestOnly.ContextMenu.DoPopup();
				var menuItem = control.ProfitLossGrid_ForTestOnly.ContextMenu.MenuItems.FindByText(AccountingConstants.AuditAndCashActionText.UndoAuditTransactionText);
				Assert("Menu item 'Undo Audit Transaction' should be enabled.", menuItem.Enabled);
				menuItem.PerformClick();

				journal.Reload();
				journal1.Reload();

				AssertEquals("Journal should be undo audit.", ZString.Empty, journal.AH_GS_NKAuditedBy);
				AssertEquals("Journal should be undo audit.", ZString.Empty, journal1.AH_GS_NKAuditedBy);
			}
		}

		public void TestControlWithoutSecuritySetDontAllowPrint()
		{
			using (ZForm form = GetPopulatedForm())
			using (JobProfitLossControl ctrl = ControlToTest)
			{
				ctrl.PluginSecurity = null;
				form.Controls.Add(ctrl);
				form.Show();
				try
				{
					ctrl.JobProfitReportButton_ForTestOnly.PerformClick();
				}
				catch (InvalidOperationException ex)
				{
					UnitTestUserNotification.Instance.ShowError(ex.Message);
				}
				AssertEquals("Message should be shown", GetPrintingMessageWhenControlWithoutSecurity(), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected virtual string GetPrintingMessageWhenControlWithoutSecurity()
		{
			return "Security checkpoint is not found.";
		}

		public void TestSecurityForViewTransaction()
		{
			using (ZForm form = new ZForm())
			{
				using (JobProfitLossControl ctrl = ControlToTest)
				{
					string expectedError = SetCheckPoint(form, ctrl, SecurityCore.ViewTransaction);
					ctrl.ViewTransaction_ForTestOnly(form, new EventArgs());
					AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public virtual void TestSecurityForPrintProfitLoss()
		{
			using (ZForm form = new ZForm())
			{
				using (JobProfitLossControl ctrl = ControlToTest)
				{
					string expectedError = SetCheckPoint(form, ctrl, SecurityCore.PrintJob);
					ctrl.PrintJobProfitDocument_ForTestOnly();
					AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestSecurityForReverseWIPACR()
		{
			using (ZForm form = new ZForm())
			{
				using (JobProfitLossControl ctrl = ControlToTest)
				{
					string expectedError = SetCheckPoint(form, ctrl, SecurityCore.ReverseWIPACR);
					ctrl.ReverseWIPAccrual_ForTestOnly(form, new EventArgs());
					AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestGlobalJobCostingTabPageVisibilityDependsOnRegistrySetting()
		{
			foreach (bool value in new bool[] { false, true })
			{
				AccountingConfigurationRegistry.Instance.EnableGlobalChargesDetail.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value);

				using (ZForm form = new ZForm())
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.Show();
					AssertEquals(value, ctl.GlobalJobCostingTabPage_ForTestOnly.TabVisible);
				}
			}
		}

		public void TestSecurityGlobalJobCostingTab_GlobalChargeDetails()
		{
			GlbStaff nonadmin = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			AccountingConfigurationRegistry.Instance.EnableGlobalChargesDetail.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			using (Env.SetTemporaryUserContext(nonadmin.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				JobInvoicingSecurityHelper securityHelper = new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing);
				SecurityCheckpoint securityCheckpoint = securityHelper.GetInvSecurity(SecurityCore.GlobalChargeDetails);
				securityCheckpoint.IsAllowed = true;

				IJobInvoicingPlugIn bizO = TestObjectCreator.CreateShipment("S00001234");
				Job job = TestObjectCreator.CreateJob(bizO);
				Factory.Save();

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("'View Global Charge Details' security item shouldn't affect Global Job Costing panel's visibility on shipments", false, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}

				securityHelper = new JobInvoicingSecurityHelper(Env.Security.JobMAWBJobInvoicing);
				securityCheckpoint = securityHelper.GetInvSecurity(SecurityCore.GlobalChargeDetails);
				securityCheckpoint.IsAllowed = true;

				bizO = (IJobInvoicingPlugIn)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IJobMAWB)));
				job = TestObjectCreator.CreateJob(bizO);
				Factory.Save();

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.JobMAWBJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("'View Global Charge Details' security item should affect non-shipment jobs", true, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}
			}
		}

		public void TestSecurityGlobalJobCostingTab_NonForwardingShipmentDescendants()
		{
			Factory.Save();

			AccountingConfigurationRegistry.Instance.EnableGlobalChargesDetail.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			JobInvoicingSecurityHelper securityHelper = new JobInvoicingSecurityHelper(Env.Security.AgencyBillOfLadingJobInvoicing);
			SecurityCheckpoint securityCheckpoint = securityHelper.GetInvSecurity(SecurityCore.GlobalChargeDetails);
			securityCheckpoint.IsAllowed = true;

			var billOfLading = (IJobInvoicingPlugIn)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Agency.IBillOfLading)));
			Job job = TestObjectCreator.CreateJob(billOfLading);
			Factory.Save();

			using (ZForm form = new ZForm(job))
			using (JobProfitLossControl ctl = ControlToTest)
			{
				ctl.PluginSecurity = Env.Security.AgencyBillOfLadingJobInvoicing;
				form.Controls.Add(ctl);
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
				form.Show();
				ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
				AssertEquals("Panel should be visible because this is a shipping job, not a forwarding job", true, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
			}
		}

		public void TestSecurityGlobalJobCostingTab_GlobalChargeConsolSendingAgent()
		{
			GlbStaff nonadmin = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			AccountingConfigurationRegistry.Instance.EnableGlobalChargesDetail.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			using (Env.SetTemporaryUserContext(nonadmin.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = TestObjectCreator.ABIGAS.PK;
				AssertNotEquals(GlbBranch.CurrentBranch.OrgProxy.PK, GlbCompany.CurrentCompany.OrgProxy.PK);

				JobInvoicingSecurityHelper securityHelper = new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing);
				SecurityCheckpoint securityCheckpoint = securityHelper.GetInvSecurity(SecurityCore.GlobalChargeConsolSendingAgent);
				securityCheckpoint.IsAllowed = true;

				ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
				ForwardingConsol consol = TestObjectCreator.CreateConsol("USLAX", "AUSYD", "C00001001");
				consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
				shipment.Consols.Add(consol);
				Job job = TestObjectCreator.CreateJob(shipment);
				Factory.Save();

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("Panel should be visible because Sending agent is the same as GlbCompany.CurrentCompany.OrgProxy", true, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}

				consol.SetDefaultSendingForwarderAddress(GlbBranch.CurrentBranch.OrgProxy);
				Factory.Save();

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("Panel should be visible because Sending agent is the same as GlbBranch.CurrentBranch.OrgProxy", true, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}

				GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertNull("GlbBranch.CurrentBranch.OrgProxy should be null", GlbBranch.CurrentBranch.OrgProxy);
				Factory.Save();

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("Panel should not be visible because Sending agent not the same as GlbBranch.CurrentBranch.OrgProxy", false, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}

				consol.SetDefaultSendingForwarderAddress(TestObjectCreator.AALSHI);
				Factory.Save();

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("Panel should not be visible because Sending agent is not the same as GlbCompany.CurrentCompany.OrgProxy", false, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}
			}
		}

		public void TestSecurityGlobalJobCostingTab_GlobalChargeConsolReceivingAgent()
		{
			GlbStaff nonadmin = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			AccountingConfigurationRegistry.Instance.EnableGlobalChargesDetail.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			using (Env.SetTemporaryUserContext(nonadmin.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = TestObjectCreator.ABIGAS.PK;
				AssertNotEquals(GlbBranch.CurrentBranch.OrgProxy.PK, GlbCompany.CurrentCompany.OrgProxy.PK);

				JobInvoicingSecurityHelper securityHelper = new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing);
				SecurityCheckpoint securityCheckpoint = securityHelper.GetInvSecurity(SecurityCore.GlobalChargeConsolReceivingAgent);
				securityCheckpoint.IsAllowed = true;

				ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
				ForwardingConsol consol = TestObjectCreator.CreateConsol("USLAX", "AUSYD", "C00001001");
				consol.SetDefaultReceivingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
				shipment.Consols.Add(consol);
				Job job = TestObjectCreator.CreateJob(shipment);
				Factory.Save();

				AccountingConfigurationRegistry.Instance.EnableGlobalChargesDetail.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("Panel should be visible because Receiving agent is the same as GlbCompany.CurrentCompany.OrgProxy", true, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}

				consol.SetDefaultReceivingForwarderAddress(GlbBranch.CurrentBranch.OrgProxy);
				Factory.Save();

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("Panel should be visible because Receiving agent is the same as GlbBranch.CurrentBranch.OrgProxy", true, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}

				GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertNull("GlbBranch.CurrentBranch.OrgProxy should be null", GlbBranch.CurrentBranch.OrgProxy);
				Factory.Save();

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("Panel should not be visible because Receiving agent is not the same as GlbBranch.CurrentBranch.OrgProxy", false, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}

				consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.AALSHI);
				Factory.Save();

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("Panel should not be visible because Receiving agent is not the same as GlbCompany.CurrentCompany.OrgProxy", false, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}
			}
		}

		public void TestSecurityGlobalJobCostingTab_GlobalChargeConsolSendingAgentARSettlementGroup()
		{
			GlbStaff originalUser = GlbStaff.CurrentUser;
			GlbStaff nonadmin = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			AccountingConfigurationRegistry.Instance.EnableGlobalChargesDetail.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			using (Env.SetTemporaryUserContext(nonadmin.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = TestObjectCreator.ABIGAS.PK;
				AssertNotEquals(GlbBranch.CurrentBranch.OrgProxy.PK, GlbCompany.CurrentCompany.OrgProxy.PK);

				JobInvoicingSecurityHelper securityHelper = new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing);
				SecurityCheckpoint securityCheckpoint = securityHelper.GetInvSecurity(SecurityCore.GlobalChargeConsolSendingAgentSettlementGroup);
				securityCheckpoint.IsAllowed = true;

				ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
				ForwardingConsol consol = TestObjectCreator.CreateConsol("USLAX", "AUSYD", "C00001001");
				consol.SetDefaultSendingForwarderAddress(TestObjectCreator.AALSHI);
				TestObjectCreator.AALSHI.SetRelatedParty(GlbCompany.CurrentCompany.OrgProxy, RelatedPartyTypeList.Codes.ARSettlementGroup, LedgerTypes.AccountsReceivable);
				shipment.Consols.Add(consol);
				Job job = TestObjectCreator.CreateJob(shipment);
				Factory.Save();

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("Panel should be visible because Sending agent's AR Settlement Group is the same as GlbCompany.CurrentCompany.OrgProxy", true, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}

				TestObjectCreator.AALSHI.SetRelatedParty(GlbBranch.CurrentBranch.OrgProxy, RelatedPartyTypeList.Codes.ARSettlementGroup, LedgerTypes.AccountsReceivable);
				Factory.Save();

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("Panel should be visible because Sending agent's AR Settlement Group is the same as GlbBranch.CurrentBranch.OrgProxy", true, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}

				GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertNull("GlbBranch.CurrentBranch.OrgProxy should be null", GlbBranch.CurrentBranch.OrgProxy);
				Factory.Save();

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("Panel should not be visible because Sending agent's AR Settlement Group is not the same as GlbBranch.CurrentBranch.OrgProxy", false, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}

				TestObjectCreator.AALSHI.SetRelatedParty(TestObjectCreator.Agent, RelatedPartyTypeList.Codes.ARSettlementGroup, LedgerTypes.AccountsReceivable);
				Factory.Save();

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("Panel should not be visible because Sending agent's AR Settlement Group is not the same as GlbCompany.CurrentCompany.OrgProxy", false, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}
			}
		}

		public void TestSecurityGlobalJobCostingTab_GlobalChargeConsolReceivingAgentARSettlementGroup()
		{
			GlbStaff originalUser = GlbStaff.CurrentUser;
			GlbStaff nonadmin = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			AccountingConfigurationRegistry.Instance.EnableGlobalChargesDetail.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			using (Env.SetTemporaryUserContext(nonadmin.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = TestObjectCreator.ABIGAS.PK;
				AssertNotEquals(GlbBranch.CurrentBranch.OrgProxy.PK, GlbCompany.CurrentCompany.OrgProxy.PK);

				JobInvoicingSecurityHelper securityHelper = new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing);
				SecurityCheckpoint securityCheckpoint = securityHelper.GetInvSecurity(SecurityCore.GlobalChargeConsolReceivingAgentSettlementGroup);
				securityCheckpoint.IsAllowed = true;

				ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
				ForwardingConsol consol = TestObjectCreator.CreateConsol("USLAX", "AUSYD", "C00001001");
				consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.AALSHI);
				TestObjectCreator.AALSHI.SetRelatedParty(GlbCompany.CurrentCompany.OrgProxy, RelatedPartyTypeList.Codes.ARSettlementGroup, LedgerTypes.AccountsReceivable);
				shipment.Consols.Add(consol);
				Job job = TestObjectCreator.CreateJob(shipment);
				Factory.Save();

				AccountingConfigurationRegistry.Instance.EnableGlobalChargesDetail.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("Panel should be visible because Receiving agent's AR Settlement Group is the same as GlbCompany.CurrentCompany.OrgProxy", true, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}

				TestObjectCreator.AALSHI.SetRelatedParty(GlbBranch.CurrentBranch.OrgProxy, RelatedPartyTypeList.Codes.ARSettlementGroup, LedgerTypes.AccountsReceivable);
				Factory.Save();

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("Panel should be visible because Receiving agent's AR Settlement Group is the same as GlbBranch.CurrentBranch.OrgProxy", true, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}

				GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertNull("GlbBranch.CurrentBranch.OrgProxy should be null", GlbBranch.CurrentBranch.OrgProxy);
				Factory.Save();

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("Panel should not be visible because Receiving agent's AR Settlement Group is not the same as GlbBranch.CurrentBranch.OrgProxy", false, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}

				TestObjectCreator.AALSHI.SetRelatedParty(TestObjectCreator.Agent, RelatedPartyTypeList.Codes.ARSettlementGroup, LedgerTypes.AccountsReceivable);
				Factory.Save();

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("Panel should not be visible because Receiving agent's AR Settlement Group is not the same as GlbCompany.CurrentCompany.OrgProxy", false, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}
			}
		}

		public void TestSecurityGlobalJobCostingTab_GlobalChargeControllingAgent()
		{
			GlbStaff originalUser = GlbStaff.CurrentUser;
			GlbStaff nonadmin = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			AccountingConfigurationRegistry.Instance.EnableGlobalChargesDetail.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			using (Env.SetTemporaryUserContext(nonadmin.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = TestObjectCreator.ABIGAS.PK;
				AssertNotEquals(GlbBranch.CurrentBranch.OrgProxy.PK, GlbCompany.CurrentCompany.OrgProxy.PK);

				JobInvoicingSecurityHelper securityHelper = new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing);
				SecurityCheckpoint securityCheckpoint = securityHelper.GetInvSecurity(SecurityCore.GlobalChargeControllingAgent);
				securityCheckpoint.IsAllowed = true;

				ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
				TestObjectCreator.AALSHI.SetRelatedParty(GlbCompany.CurrentCompany.OrgProxy, RelatedPartyTypeList.Codes.ControllingAgent, RelatedPartyDirectionList.Codes.Sales);
				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = TestObjectCreator.AALSHI.Addresses[0].PK;
				Job job = TestObjectCreator.CreateJob(shipment);
				Factory.Save();

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("Panel should be visible because shipment Consignee is the same as GlbCompany.CurrentCompany.OrgProxy", true, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}

				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = TestObjectCreator.AALSHI.Addresses[0].PK;
				Factory.Save();

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("Panel should be visible because shipment Consignor is the same as GlbCompany.CurrentCompany.OrgProxy", true, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}

				TestObjectCreator.AALSHI.SetRelatedParty(GlbBranch.CurrentBranch.OrgProxy, RelatedPartyTypeList.Codes.ControllingAgent, RelatedPartyDirectionList.Codes.Sales);
				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = TestObjectCreator.AALSHI.Addresses[0].PK;
				Factory.Save();

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("Panel should be visible because shipment Consignee is the same as GlbBranch.CurrentBranch.OrgProxy", true, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}

				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = TestObjectCreator.AALSHI.Addresses[0].PK;
				Factory.Save();

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("Panel should be visible because shipment Consignor is the same as GlbBranch.CurrentBranch.OrgProxy", true, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}

				TestObjectCreator.AALSHI.SetRelatedParty(GlbCompany.CurrentCompany.OrgProxy, RelatedPartyTypeList.Codes.ControllingAgent, RelatedPartyDirectionList.Codes.Sales);
				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
				Factory.Save();

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("Panel should not be visible because neither shipment Consignee nor shipment Consignor is not the same as GlbCompany.CurrentCompany.OrgProxy", false, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}

				TestObjectCreator.AALSHI.SetRelatedParty(GlbBranch.CurrentBranch.OrgProxy, RelatedPartyTypeList.Codes.ControllingAgent, RelatedPartyDirectionList.Codes.Sales);
				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
				Factory.Save();

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("Panel should not be visible because neither shipment Consignee nor shipment Consignor is not the same as GlbBranch.CurrentBranch.OrgProxy", false, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}

				GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertNull("GlbBranch.CurrentBranch.OrgProxy should be null", GlbBranch.CurrentBranch.OrgProxy);
				Factory.Save();

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("Panel should not be visible because neither shipment Consignee nor shipment Consignor is not the same as GlbBranch.CurrentBranch.OrgProxy", false, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}
			}
		}

		public void TestSecurityGlobalJobCostingTab_GlobalChargeAdministrationUser()
		{
			GlbStaff originalUser = GlbStaff.CurrentUser;
			GlbStaff nonadmin = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			AccountingConfigurationRegistry.Instance.EnableGlobalChargesDetail.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			using (Env.SetTemporaryUserContext(nonadmin.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				JobInvoicingSecurityHelper securityHelper = new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing);
				SecurityCheckpoint securityCheckpoint = securityHelper.GetInvSecurity(SecurityCore.GlobalChargeAdministrationUser);
				securityCheckpoint.IsAllowed = true;

				IJobInvoicingPlugIn bizO = TestObjectCreator.CreateShipment("S00001234");
				Job job = TestObjectCreator.CreateJob(bizO);
				Factory.Save();

				AccountingConfigurationRegistry.Instance.EnableGlobalChargesDetail.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.DetailsTabPage_ForTestOnly.Show();
					ctl.SummaryTabPage_ForTestOnly.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("Panel should be visible because user has a GlobalChargeAdministrationUser right", true, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}

				securityCheckpoint.IsAllowed = false;

				using (ZForm form = new ZForm(job))
				using (JobProfitLossControl ctl = ControlToTest)
				{
					ctl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					form.Controls.Add(ctl);
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctl, "ProfitLoss");
					form.Show();
					ctl.GlobalJobCostingTabPage_ForTestOnly.Show();
					AssertEquals("Panel should not be visible because user does not have a GlobalChargeAdministrationUser right", false, ctl.GlobalJobCostingTabPanel_ForTestOnly.Visible);
				}
			}
		}

		public virtual void TestPrintJobProfitDocumentDisplaysPrintingOptionsFormWhenJobIsSaved()
		{
			try
			{
				TestObjectCreator objectCreator = new TestObjectCreator(Factory);
				objectCreator.CreateAccrual(JobWithParent, objectCreator.CC1, 1.0M, "", 100M);
				Factory.Save();

				JobProfitLoss profitLoss = new JobProfitLoss(Factory);
				profitLoss.SetJobPKs(new ZGuid[] { JobWithParent.PK });

				using (ZForm form = new ZForm())
				{
					using (JobProfitLossControl ctrl = ControlToTest)
					{
						ctrl.SetDataBinding(profitLoss, "");
						ctrl.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
						ctrl.Show();
						ctrl.DetailsTabPage_ForTestOnly.Show();
						ctrl.SummaryTabPage_ForTestOnly.Show();
						ctrl.JobProfitReportButton_Click_ForTestOnly(null, null);

						AssertNotNull("Last form shown should not be null", ZFormModaliser.LastFormShownDialogForTest);
						AssertEquals("Last form shown", typeof(JobProfitDocumentPrintingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
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

		public void TestButtonsEnabledWithViewMode()
		{
			using (ZForm form = GetPopulatedForm())
			using (JobProfitLossControl ctrl = ControlToTest)
			{
				form.DisplayMode = ODisplayMode.ReadOnly;
				form.ControllerID = DummyControllerIDs.Dummy;
				form.Controls.Add(ctrl);
				var module = form.GetModule();
				form.Show();

				Assert(ctrl.JobProfitReportButton_ForTestOnly.Enabled);
				Assert(ctrl.SummaryClearButton_ForTestOnly.Enabled);
				Assert(ctrl.SummaryFindButton_ForTestOnly.Enabled);
			}
		}

		#region ViewTransaction

		public void TestViewTransactionAccrual()
		{
			TestObjectCreator.CreateAccrual(JobWithParent, TestObjectCreator.CC1, 1.0M, "", 100M);
			Factory.Save();
			AssertViewTransaction(typeof(AccrualForm));
		}

		public void TestViewTransactionWIP()
		{
			TestObjectCreator.CreateWIP(JobWithParent, TestObjectCreator.CC1, 1.0M, "", 100M);
			Factory.Save();
			AssertViewTransaction(typeof(WIPForm));
		}

		public void TestViewTransactionJCJournal()
		{
			var journal = TestObjectCreator.CreateJCJournalHeader(ZDateTime.Now, 100M);
			TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.CC1, JobWithParent, ZDateTime.Now, 100M);
			Factory.Save();
			AssertViewTransaction(typeof(JobCostingJournalForm));
		}

		public void TestViewTransactionARInvoice()
		{
			var invoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV", TestObjectCreator.AUD, 1M);
			var line = TestObjectCreator.CreateARInvoiceLine(invoice, JobWithParent, TestObjectCreator.CC1, TestObjectCreator.AUD, 1M, "Desc", 100M);
			var charge = TestObjectCreator.CreateCharge(JobWithParent, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 0M, null, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI);
			charge.JR_AL_ARLine = line.PK;
			Factory.Save();
			AssertViewTransaction(typeof(InvoiceForm));
		}

		public void TestViewTransactionAPInvoice()
		{
			var invoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV", TestObjectCreator.AUD, 1M);
			var line = TestObjectCreator.CreateAPInvoiceLine(invoice, JobWithParent, TestObjectCreator.CC1, TestObjectCreator.AUD, 1M, "Desc", 100M);
			var charge = TestObjectCreator.CreateCharge(JobWithParent, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 0M, null);
			charge.JR_AL_APLine = line.PK;
			Factory.Save();
			AssertViewTransaction(typeof(InvoiceForm));
		}

		public void TestViewTransactionJRJournal()
		{
			TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, JobWithParent, 100M);
			Factory.Save();
			AssertViewTransaction(typeof(JobRevenueJournalForm));
		}

		[SuspendCriticalValidation]
		public void TestViewTransactionWithUnsupportedTransaction()
		{
			var invoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV", TestObjectCreator.AUD, 1M);
			var line = TestObjectCreator.CreateAPInvoiceLine(invoice, JobWithParent, TestObjectCreator.CC1, TestObjectCreator.AUD, 1M, "Desc", 100M);
			var charge = TestObjectCreator.CreateCharge(JobWithParent, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 0M, null);
			charge.JR_AL_APLine = line.PK;
			invoice.AH_TransactionType = TransactionTypes.JobRevenueJournal;
			Factory.Save();
			BusinessObject bindingObject = PrepareBindingBizo();

			using (ZForm form = GetPopulatedForm())
			using (JobProfitLossControl control = ControlToTest)
			{
				control.SetDataBinding(bindingObject, "");
				control.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				form.Controls.Add(control);
				form.Show();
				control.DetailsTabPage_ForTestOnly.Show();
				control.ProfitLossGrid_ForTestOnly.SelectAllElements();

				control.ProfitLossGrid_ForTestOnly.ContextMenu.MenuItems.FindByText(control.ViewTransactionText_ForTestOnly).PerformClick();

				AssertNull("Last form shown should be null", ZFormModaliser.LastFormShownForTest);
				AssertEquals("ErrorReporter.LastKeyReported", "Transaction Type 'JRJ' is not a valid AP Transaction Type", ErrorReporter.LastKeyReported);
				AssertEquals("ErrorReporter.LastMessageReported", "Not supported transaction in JobProfitLossControl.ViewTransactions", ErrorReporter.LastMessageReported);
				AssertEquals("The operation is not supported for the transaction.", UnitTestUserNotification.Instance.LastMessage.Text);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		[SuspendCriticalValidation]
		public void TestViewTransactionWithUnsupportedController()
		{
			var invoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV", TestObjectCreator.AUD, 1M);
			invoice.AH_TransactionType = TransactionTypes.IncompleteInvoice;
			var line = TestObjectCreator.CreateAPInvoiceLine(invoice, JobWithParent, TestObjectCreator.CC1, TestObjectCreator.AUD, 1M, "Desc", 100M);
			var charge = TestObjectCreator.CreateCharge(JobWithParent, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 0M, null);
			charge.JR_AL_APLine = line.PK;
			Factory.Save();
			BusinessObject bindingObject = PrepareBindingBizo();

			using (ZForm form = GetPopulatedForm())
			using (JobProfitLossControl control = ControlToTest)
			{
				control.SetDataBinding(bindingObject, "");
				control.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				form.Controls.Add(control);
				form.Show();
				control.DetailsTabPage_ForTestOnly.Show();
				control.ProfitLossGrid_ForTestOnly.SelectAllElements();

				control.ProfitLossGrid_ForTestOnly.ContextMenu.MenuItems.FindByText(control.ViewTransactionText_ForTestOnly).PerformClick();

				AssertNull("Last form shown should be null", ZFormModaliser.LastFormShownForTest);
				AssertEquals("ErrorReporter.LastKeyReported", "Can't create controller for transaction with ledger 'AP' and type 'INI'.", ErrorReporter.LastKeyReported);
				AssertEquals("ErrorReporter.LastMessageReported", "Not supported controller in JobProfitLossControl.ViewTransactions", ErrorReporter.LastMessageReported);
				AssertEquals("The operation is not supported for the transaction.", UnitTestUserNotification.Instance.LastMessage.Text);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		void AssertViewTransaction(Type formType)
		{
			BusinessObject bindingObject = PrepareBindingBizo();

			using (ZForm form = GetPopulatedForm())
			using (JobProfitLossControl control = ControlToTest)
			{
				control.SetDataBinding(bindingObject, "");
				control.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				form.Controls.Add(control);
				form.Show();
				control.DetailsTabPage_ForTestOnly.Show();
				control.ProfitLossGrid_ForTestOnly.SelectAllElements();

				control.ProfitLossGrid_ForTestOnly.ContextMenu.MenuItems.FindByText(control.ViewTransactionText_ForTestOnly).PerformClick();

				AssertNotNull("Last form shown should not be null", ZFormModaliser.LastFormShownForTest);
				AssertType(formType, ZFormModaliser.LastFormShownForTest);
			}
		}

		public virtual void TestPrintingDocumentForAJobWithoutAValidParentCausesNoException()
		{
			var jobWithoutParent = Factory.NewJobWithValidTestDataForTesting<Job>();
			Factory.Save();

			var profitLoss = new JobProfitLoss(Factory);
			profitLoss.SetJobPKs(new[] { jobWithoutParent.PK });

			using (var form = new JobManagementForm(profitLoss))
			using (var ctrl = ControlToTest)
			{
				ctrl.SetDataBinding(profitLoss, "");
				ctrl.Show();
				ctrl.JobProfitReportButton_Click_ForTestOnly(null, null);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNoExceptionThrown(() => ctrl.PrintJobProfitDocument_ForTestOnly());
				var expectedMessage = "Cannot find corresponding operation job.\r\nYou can use the filter 'Missing/Invalid Job Parent' in Job Management module to list all jobs without a valid parent";
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestClickingOnALineWithoutAValidParentCausesNoException()
		{
			var jobWithoutParent = Factory.NewJobWithValidTestDataForTesting<Job>();
			jobWithoutParent.PlugInData = null;
			Factory.Save();

			var profitLoss = new JobProfitLoss(new BusinessObjectFactory());
			profitLoss.SetJobPKs(new[] { jobWithoutParent.PK });

			using (var form = new ZForm(jobWithoutParent.ProfitLoss[0]))
			using (var control = ControlToTest)
			{
				control.SetDataBinding(profitLoss, "");
				form.Controls.Add(control);
				control.DetailsTabPage_ForTestOnly.Show();
				control.ProfitLossGrid_ForTestOnly.SelectAllElements();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNoExceptionThrown(() => control.ProfitLossGrid_ForTestOnly.ContextMenu.MenuItems.FindByText(control.ViewTransactionText_ForTestOnly).PerformClick());
				var expectedMessage = "Cannot find corresponding operation job.\r\nYou can use the filter 'Missing/Invalid Job Parent' in Job Management module to list all jobs without a valid parent";
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region RegenerateJournalEntries

		public void TestRegenerateJournalEntries()
		{
			var bizObjuect = GetTestBindingObject();
			var mockDataRecover = new Mock<IGeneralLedgerDataRecover>();

			using (ObjectFactory.Substitute(mockDataRecover.Object))
			{
				using (var form = new ZForm(bizObjuect))
				using (var control = ControlToTest)
				{
					var bizos = new BusinessObject[] { Factory.Load<JCJournalHeader>(JobProfitLossForRegenerateJournalEntriesTest.ProfitLossDetails[0].ZY_Calc_AH) };
					Assert(bizos.Any());

					form.Controls.Add(control);
					form.Show();

					control.DetailsTabPage_ForTestOnly.Show();
					control.ProfitLossGrid_ForTestOnly.SelectAllElements();

					AssertEquals(bizos.Length, control.SelectedBusinessObjects_ForTestOnly().Length);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					control.HandleRegenerateJournalEntries_ForTestOnly(null, null);

					var headerWithLinesTransactions = bizos.Where(x => x is TransactionHeaderWithLines).Cast<TransactionHeaderWithLines>();
					var headerLines = headerWithLinesTransactions.SelectMany(x => x.Lines).Cast<AccTransactionLines>();

					mockDataRecover.Verify(x => x.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, It.Is<BusinessObject[]>(y => y.Length == headerLines.Count() && y.All(z => headerLines.Any(a => a.PK == z.PK)))), Times.Once);
				}
			}
		}

		public void TestRegenerateJournalEntriesActionMenuItem()
		{
			var moq = new Mock<JobProfitLossControl>();
			moq.CallBase = true;

			using (var testModule = moq.Object)
			{
				AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AssertHasMenuItem(testModule);

				AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AssertHasMenuItem(testModule, hasMenuItem: false);

				AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var nonSupportStaff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, SQLComparisonOperator.NotEqual, User.SupportUserName));
				using (Env.SetTemporaryUserContext(nonSupportStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					AssertHasMenuItem(testModule, hasMenuItem: false);
				}
			}
		}

		void AssertHasMenuItem(JobProfitLossControl testConsol, bool hasMenuItem = true)
		{
			testConsol.ReSetMenuItem();
			var testMenu = testConsol.FindMenuItemByText_ForTestOnly("Regenerate Journal Entries (CWSupport Only)");

			if (hasMenuItem)
			{
				AssertNotNull(testMenu);
			}
			else
			{
				AssertNull(testMenu);
			}
		}

		public void TestRegenerateJournalEntries_PromptMessgae()
		{
			using (var form = new ZForm(GetTestBindingObjectForTestMessage()))
			using (var control = ControlToTest)
			{
				form.Controls.Add(control);
				form.Show();

				control.DetailsTabPage_ForTestOnly.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				control.HandleRegenerateJournalEntries_ForTestOnly(null, null);

				var expectedMessage = "Journal Entries can only be regenerated if all selected Transactions are JC JNL's (CFX Journals)";
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				control.ProfitLossGrid_ForTestOnly.SelectAllElements();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				control.HandleRegenerateJournalEntries_ForTestOnly(null, null);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected virtual object GetTestBindingObject()
		{
			return JobProfitLossForRegenerateJournalEntriesTest;
		}

		protected JobProfitLoss JobProfitLossForRegenerateJournalEntriesTest
		{
			get
			{
				if (jobProfitLoss == null)
				{
					var shipment = Factory.New<ForwardingShipment>();
					var shipmentJob = new Job.Loader(shipment).TryCreate();
					shipmentJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
					shipmentJob.JH_GB = GlbBranch.CurrentBranch.PK;

					var postDate = DateTime.Now;
					var journal = TestObjectCreator.CreateJCJournalHeader(postDate, 20m);
					TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.RevenueChargeCode, shipmentJob, postDate, 20m);
					Factory.Save();

					jobProfitLoss = new JobProfitLoss(Factory);
					jobProfitLoss.SetJobPKs(new ZGuid[] { shipmentJob.PK });
					jobProfitLoss.ProfitLossDetails.Load();
				}

				return jobProfitLoss;
			}
		}

		JobProfitLoss jobProfitLoss;

		protected virtual object GetTestBindingObjectForTestMessage()
		{
			return JobProfitLossForRegenerateJournalEntriesTestMessage;
		}

		protected JobProfitLoss JobProfitLossForRegenerateJournalEntriesTestMessage
		{
			get
			{
				if (jobProfitLossForTestMessage == null)
				{
					var shipment = Factory.New<ForwardingShipment>();
					var shipmentJob = new Job.Loader(shipment).TryCreate();
					shipmentJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
					shipmentJob.JH_GB = GlbBranch.CurrentBranch.PK;

					var rEVLine = Factory.NewWithValidTestData<ARInvoiceLine>();
					rEVLine.AL_LineType = TransactionLineTypes.Revenue;
					rEVLine.AL_JH = shipmentJob.PK;
					rEVLine.AL_AC = TestObjectCreator.CC1.PK;
					rEVLine.AL_GB = GlbBranch.CurrentBranch.PK;
					rEVLine.AL_AH = Factory.New<ARInvoice>().PK;
					rEVLine.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
					TestObjectCreator.CreateJobCharge(rEVLine, shipmentJob, TestObjectCreator.CC1, TestObjectCreator.AUD);

					Factory.Save();

					jobProfitLossForTestMessage = new JobProfitLoss(Factory);
					jobProfitLossForTestMessage.SetJobPKs(new ZGuid[] { shipmentJob.PK });
					jobProfitLossForTestMessage.ProfitLossDetails.Load();
				}

				return jobProfitLossForTestMessage;
			}
		}

		JobProfitLoss jobProfitLossForTestMessage;

		#endregion

		#region Implementation

		protected virtual BusinessObject PrepareBindingBizo()
		{
			JobProfitLoss profitLoss = new JobProfitLoss(Factory);
			profitLoss.SetJobPKs(new ZGuid[] { JobWithParent.PK });

			return profitLoss;
		}

		protected string SetCheckPoint(ZForm form, JobProfitLossControl ctrl, string securityItemKey)
		{
			form.Controls.Add(ctrl);
			ctrl.PluginSecurity = PluginSecurityForTest;

			SecurityCheckpoint checkPoint = ctrl.SecurityHelper_ForTestOnly.GetInvSecurity(securityItemKey);
			checkPoint.IsAllowed = false;

			string expectedError = checkPoint.ErrorMessageForNotAllowed;
			return expectedError;
		}

		protected virtual SecurityCheckpoint PluginSecurityForTest
		{
			get { return Env.Security.MaintainShipmentJobInvoicing; }
		}

		protected virtual ZForm GetPopulatedForm()
		{
			var job = new TestObjectCreator(new BusinessObjectFactory()).CreateJob(TestObjectCreator.CreateShipment("S001"), false);
			return new ZForm(job.ProfitLoss[0]);
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;

		#endregion
	}
}
