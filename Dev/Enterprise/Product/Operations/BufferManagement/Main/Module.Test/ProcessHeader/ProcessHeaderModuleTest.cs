using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.GUI;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(ProcessHeaderModule))]
	class ProcessHeaderModuleTest : ZModuleBasherTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}

		public void TestHasOperationalActionsPlugin()
		{
			using (var module = new ProcessHeaderModule())
			{
				AssertNotNull(module.Plugins.GetPlugin(ControllerIDs.OperationalActions));
			}
		}

		public void TestHasOperationalActions()
		{
			using (var module = new ProcessHeaderModule())
			{
				var supportable = module as IOperationalActionSupportable;
				AssertNotNull(supportable);
				AssertNotNull(supportable.OperationalActionSupporter);
			}
		}

		public void TestShouldNotAllowNew()
		{
			using (var module = new ProcessHeaderModule())
			{
				Assert(!module.AllowNew);
			}
		}

		public void TestShouldAllowDelete()
		{
			using (var module = new ProcessHeaderModule())
			{
				Assert(module.AllowDelete);
			}
		}

		public void TestShouldAllowMultiDeleteWithoutListing()
		{
			using (var module = new DummyProcessHeaderModule())
			{
				Assert(module.AllowMultiDeleteWithoutListing_Exposed());
			}
		}

		public void TestDelete()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			Factory.Save();

			using (var module = new DummyProcessHeaderModule())
			{
				module.fSelectedBusinessObjects = Array.Empty<BusinessObject>();
				module.HandleDeleteClick_Exposed();
				var expectedMessage = "Please select a record in the grid.";
				AssertEquals("On deleting empty selection - Message must be shown", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				module.fSelectedBusinessObjects = new BusinessObject[] { workflow1 };
				module.HandleDeleteClick_Exposed();
				expectedMessage = "Would you like to deactivate the selected items? If some objects are already inactive, no action will be performed on them.";
				AssertEquals("On deleting single object - confirmation Message must be shown", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				module.fSelectedBusinessObjects = new BusinessObject[] { workflow2, workflow3 };
				module.HandleDeleteClick_Exposed();
				AssertEquals("On deleting multiple objects - confirmation Message must be shown", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestFilterCollection_ShouldAllowJobs()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			Factory.Save();

			using (var module = new ProcessHeaderModule())
			{
				var collection = module.GridCollection;

				AssertEquals(3, collection.Count);
				Assert(collection.Cast<ProcessHeader>().Any(h => h.PK == jobHeader.PK));
				Assert(collection.Cast<ProcessHeader>().Any(h => h.PK == workflow1.PK));
				Assert(collection.Cast<ProcessHeader>().Any(h => h.PK == workflow2.PK));
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ProcessHeader;
		}

		public void TestLoad_ShouldSetReleaseSequence()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			Factory.Save();

			var processHeaders = jobHeader.ProcessHeaders;

			using (var dummyModule = new DummyProcessHeaderModule())
			{
				dummyModule.PerformSearch_ForTest();
				AssertNotNull(processHeaders.FirstOrDefault());
				Assert(processHeaders.All(h => h.ReleaseSequence != "None"));
			}
		}

		public void TestFilterControl_ConstraintStatusColumn()
		{
			using (var module = new DummyProcessHeaderModule())
			using (var processHeaderFilterControl = (ProcessHeaderFilterControl)module.GetNewFilterControl_ForTest())
			{
				var constraintStatusColumn = processHeaderFilterControl.Grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "ConstraintStatus");
				AssertNotNull("Constraint-Status column should exist", constraintStatusColumn);
				Assert("Constraint-Status column length should be able to show widest text i.e. Ready for Constraint", constraintStatusColumn.Width >= CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(144));
			}
		}

		public void TestFilterControl_CategoryColumns()
		{
			using (var module = new DummyProcessHeaderModule())
			using (var processHeaderFilterControl = (ProcessHeaderFilterControl)module.GetNewFilterControl_ForTest())
			{
				var categoryColumn = processHeaderFilterControl.Grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "FH_Category");
				AssertNotNull("FH_Category column should exist", categoryColumn);
				Assert("FH_Category column length should be able to show caption", categoryColumn.Width >= CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80));

				var categoryDescriptionColumn = processHeaderFilterControl.Grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "CategoryDescription");
				AssertNotNull("CategoryDescription column should exist", categoryDescriptionColumn);
				Assert("CategoryDescription column length should be able to show caption", categoryDescriptionColumn.Width >= CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120));
			}
		}

		public void TestFilterControl_LastTransferTypeDescriptionColumn()
		{
			using (var module = new DummyProcessHeaderModule())
			using (var processHeaderFilterControl = (ProcessHeaderFilterControl)module.GetNewFilterControl_ForTest())
			{
				var lastTransferTypeColumn = processHeaderFilterControl.Grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "LastTransferTypeDescription");
				AssertNotNull("Last transfer type description column should exist", lastTransferTypeColumn);
				Assert("Last transfer type description column length should be able to show caption", lastTransferTypeColumn.Width >= CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(320));
			}
		}

		public void TestFilterControl_ShouldNotContainNewReleaseGateRelatedColumns_WhenDisplayResponsiveReleaseGateUiSettingsRegistryItemIsDisabled()
		{
			BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (var module = new DummyProcessHeaderModule())
			using (var processHeaderFilterControl = (ProcessHeaderFilterControl)module.GetNewFilterControl_ForTest())
			{
				AssertNull("Approved column should not present", GetColumnByName(processHeaderFilterControl, ProcessHeaderSchema.Constants.FH_IsApproved));
				AssertNull("Dedicated Buffer column should not present", GetColumnByName(processHeaderFilterControl, ProcessHeaderSchema.Constants.FH_FC_DedicatedBuffer));
				AssertNull("Branch column should not present", GetColumnByName(processHeaderFilterControl, ProcessHeaderSchema.Constants.FH_GB_Branch));
				AssertNull("Department column should not present", GetColumnByName(processHeaderFilterControl, ProcessHeaderSchema.Constants.FH_GE_Department));
			}
		}

		public void TestFilterControl_ShouldContainNewReleaseGateRelatedColumn_WhenDisplayResponsiveReleaseGateUiSettingsRegistryItemIsEnabled()
		{
			BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (var module = new DummyProcessHeaderModule())
			using (var processHeaderFilterControl = (ProcessHeaderFilterControl)module.GetNewFilterControl_ForTest())
			{
				var approvedColumn = GetColumnByName(processHeaderFilterControl, ProcessHeaderSchema.Constants.FH_IsApproved);
				AssertNotNull("Approved column should present", approvedColumn);
				AssertEquals("Approved column should be visible by default", true, approvedColumn.IsVisible);

				var dedicatedBufferColumn = GetColumnByName(processHeaderFilterControl, "DedicatedBufferName");
				AssertNotNull("Dedicated Buffer column should present", dedicatedBufferColumn);
				AssertEquals("Dedicated Buffer column should not be visible by default", false, dedicatedBufferColumn.IsVisible);

				var branchColumn = GetColumnByName(processHeaderFilterControl, ProcessHeaderSchema.Constants.FH_GB_Branch);
				AssertNotNull("Branch column should present", branchColumn);
				AssertEquals("Branch column should not be visible by default", false, branchColumn.IsVisible);

				var departmentColumn = GetColumnByName(processHeaderFilterControl, ProcessHeaderSchema.Constants.FH_GE_Department);
				AssertNotNull("Department column should present", departmentColumn);
				AssertEquals("Department column should not be visible by default", false, departmentColumn.IsVisible);
			}
		}

		ZGridColumnInfo GetColumnByName(ProcessHeaderFilterControl control, string columnName) => control.Grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == columnName);

		public void TestJobSchedulesMenuItem_ShouldSelectRowsAlreadySelectedInModuleGrid()
		{
			BMSTestHelper.CreateSystem(Factory, "INQ");

			var jobHeader1 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader1");
			var jobHeader2 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader2");
			var jobHeader3 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader3");

			jobHeader1.FH_VoteUpDownAmount = 1;
			jobHeader2.FH_VoteUpDownAmount = 2;
			jobHeader3.FH_VoteUpDownAmount = 3;

			Factory.Save();

			using (var form = new ZForm())
			using (var module = new DummyProcessHeaderModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				module.PerformSearch_ForTest();

				Application.DoEvents();

				module.DisplayGrid.Select(1);
				module.DisplayGrid.Select(2);
				var selectedInModule = module.DisplayGrid.SelectedElements.Cast<ProcessHeader>().Select(x => x.FH_CompletionStatement);

				var provider = (IFilterGridMenuItemProvider)new MultiJobHeaderEditorMenuItemProvider();
				var menuItem = provider.GetMenuItems(module).Single();
				menuItem.PerformClick();

				using (var editorForm = Application.OpenForms.OfType<MultiJobHeaderEditorForm>().Single())
				{
					var viewModel = (MultiJobHeaderEditorViewModel)editorForm.DataSource;
					AssertEquals(3, viewModel.JobHeaderViews.Count);

					var grid = editorForm.SchedulesGrid;
					var selectedInEditor = grid.SelectedElements.Cast<JobHeaderView>().Select(x => x.ProcessHeader.FH_CompletionStatement);

					AssertContainsExactElementsInAnyOrder(selectedInModule, selectedInEditor);
				}
			}
		}

		public void TestProviderJobDescription_ShouldMinimiseDatabaseHits()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			BMSTestHelper.CreateSystem(Factory, "INQ");
			BMSTestHelper.CreateSystem(Factory, "DUM");

			for (int i = 0; i < 10; i++)
			{
				var job = Factory.NewWithValidTestData<OrgHeader>();
				ProcessJobHeader.GetForParent(job, Factory);
			}

			for (int i = 0; i < 10; i++)
			{
				var job = Factory.NewWithValidTestData<SalesEnquiry>();
				ProcessJobHeader.GetForParent(job, Factory);
			}

			for (int i = 0; i < 10; i++)
			{
				var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
				ProcessJobHeader.GetForParent(job, Factory);
			}

			Factory.Save();

			using (var form = new ZForm())
			using (var dummyModule = new DummyProcessHeaderModule())
			{
				form.Controls.Add(dummyModule.EmbeddedControl);
				form.Show();

				Application.DoEvents();

				var dbHits = new Dictionary<string, int>()
				{
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ OrgColdCallRegisterSchema.Constants.TableName, 1 },
					{ DummyBizoSchema.Constants.TableName, 1 },
				};

				using (AssertDbHitsForAllFactories(dbHits, ignoreUnspecified: true))
				{
					dummyModule.PerformSearch_ForTest();
				}
			}
		}
	}

	#region Dummy ProcessHeaderModule

	class DummyProcessHeaderModule : ProcessHeaderModule
	{
		public BusinessObject[] fSelectedBusinessObjects;

		protected override BusinessObject[] SelectedBusinessObjects
		{
			get { return fSelectedBusinessObjects; }
		}

		public bool AllowMultiDeleteWithoutListing_Exposed()
		{
			return AllowMultiDeleteWithoutListing;
		}

		public void HandleDeleteClick_Exposed()
		{
			this.HandleDeleteClick(this, new EventArgs());
		}

		public IFilterControl GetNewFilterControl_ForTest()
		{
			return this.GetNewFilterControl();
		}
	}

	#endregion
}
