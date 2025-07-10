using System.Collections;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI.Test
{
	class MultiJobHeaderEditorMenuItemProviderModuleApplicabilityTest : BMSTestCaseWithFactory
	{
		#region Module Applicability

		public void TestShouldNotDisplayWhenBMIsDisabled()
		{
			BMSTestHelper.DisableBMSInRegistry();
			var system = CreateSystem("ORG");

			Factory.Save();

			module.IDOverride = ModuleIDs.Organisation;

			var menuItems = provider.GetMenuItems(module).ToArray();
			AssertEquals(0, menuItems.Length);
		}

		public void TestShouldNotDisplayForModulesNotPartOfABMS()
		{
			var system = CreateSystem("WKI");

			Factory.Save();

			module.IDOverride = ModuleIDs.Organisation;

			var menuItems = provider.GetMenuItems(module).ToArray();
			AssertEquals(0, menuItems.Length);
		}

		public void TestShouldNotDisplayForModulePartOfBufferManagementSystemButDoesNotSupportWorkflow()
		{
			var system = CreateSystem("ORG");

			Factory.Save();

			module.IDOverride = ModuleIDs.Organisation;
			AssertEquals(0, provider.GetMenuItems(module).ToArray().Length);
		}

		public void TestShouldDisplayForModulePartOfBufferManagementSystemAndSupportingWorkflow()
		{
			var system = CreateSystem("ORG");

			Factory.Save();

			module.IDOverride = ModuleIDs.Organisation;
			module.SupportsWorkflowOverride = true;

			AssertEquals(1, provider.GetMenuItems(module).ToArray().Length);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			module = new DummyModuleForVisualBoards();
			provider = ((IEnumerable)ObjectFactory.Get("FilterGridActionsMenuItemProviders")).OfType<MultiJobHeaderEditorMenuItemProvider>().Single();
		}

		protected override void TearDown()
		{
			base.TearDown();

			module.Dispose();
		}

		IFilterGridMenuItemProvider provider;
		DummyModuleForVisualBoards module;

		#endregion
	}

	class MultiJobHeaderEditorMenuItemProviderTest : BMSTestCaseWithFactory
	{
		#region Click Handler

		public void TestClickHandler()
		{
			var system = CreateSystem("INQ");
			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var job2 = Factory.NewWithValidTestData<SalesEnquiry>();
			var job3 = Factory.NewWithValidTestData<SalesEnquiry>();

			Factory.Save();

			var menuItem = provider.GetMenuItems(module).Single();

			menuItem.PerformClick();

			using (var form = Application.OpenForms.OfType<MultiJobHeaderEditorForm>().Single())
			{
				var viewModel = (MultiJobHeaderEditorViewModel)form.DataSource;
				AssertEquals(3, viewModel.JobHeaderViews.Count);
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.SalesEnquiry);
			provider = ((IEnumerable)ObjectFactory.Get("FilterGridActionsMenuItemProviders")).OfType<MultiJobHeaderEditorMenuItemProvider>().Single();
		}

		protected override void TearDown()
		{
			base.TearDown();

			module.Dispose();
		}

		IFilterGridMenuItemProvider provider;
		ZFilterGridModule module;

		#endregion
	}
}
