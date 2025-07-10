using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class VisualBoardFormBaseNonTransactionedTest : NonTransactionedTestCase
	{
		static void AddCustomSQLFilter(StmModuleFilter moduleFilter)
		{
			FilterStripsTestHelper.AddFilterStrips(moduleFilter, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = filter => ((ModuleSQLFilter)filter).Property1 = "1 = 1"
			});
		}

		protected BMBoard SetupBoardWithGridAndFilter(ModuleIdentifier module, Action<StmModuleFilter> addFilterStripAction = null)
		{
			var moduleFilter = Factory.New<StmModuleFilter>();
			moduleFilter.S9_ModuleID = module.Name;
			moduleFilter.S9_IsPublished = true;
			moduleFilter.S9_FilterName = "Filter";

			(addFilterStripAction ?? AddCustomSQLFilter)(moduleFilter);

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var board = BMSTestHelper.CreateBoard(config.System);

			var section = BMSTestHelper.CreateBoardSection(module, board);
			section.MS_SectionType = BMConstants.ModuleGridSectionType;
			var sectionConfig = (ModuleGridSectionConfiguration)section.Configuration;
			var panelConfig = (ModuleGridSectionPanelConfiguration)sectionConfig.PanelConfigurations.First();
			panelConfig.FilterLayout = moduleFilter.PK;
			panelConfig.ShowFilters = true;

			return board;
		}

		protected void AssertShowFormFromBoard_ShouldOpenInMainThread<FormToOpen>(BMBoard board, string menuAction, Action<ContextMenu> beforeFindMenu = null) where FormToOpen : ZForm
		{
			var mainThreadId = Factory.ThreadSentry.OwnerThread.ThreadID;
			try
			{
				using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
				using (var form = VisualBoardFormDisplayer.ShowBoard(board))
				{
					form.AwaitAll();
					VisualBoardFormDisplayer.ShowBoard(board);
					var visualBoardForm = ZApplication.GetOpenForms().OfType<VisualBoardForm>().SingleOrDefault();

					var grid = visualBoardForm.FindAll<ZGrid>().Single();
					grid.SelectAllElements();

					beforeFindMenu?.Invoke(grid.ContextMenu);
					form.AwaitAll();

					var contextMenuItem = grid.ContextMenu.MenuItems.FindByText(menuAction, true);
					contextMenuItem.PerformClick();
					form.AwaitAll();
				}
			}
			finally
			{
				var form = Application.OpenForms.OfType<FormToOpen>().FirstOrDefault();
				AssertNotNull("QuotedBookingForm should open", form);

				var formIsInTheCurrentThread = form.IsCallingFromOwnedThread();
				var bo = form.BusinessEntity as BusinessObject;

				form.Close();

				AssertNotNull("Should should have a Business Object", bo);

				var boCreationThread = bo.Factory.ThreadSentry.CreationThread.ThreadID;

				CombineAssertions(() =>
				{
					Assert("Form is in the main thread", formIsInTheCurrentThread);
					AssertNotNull("Form should have a business object", bo);
					AssertEquals("Business object should have be created on the main thread", mainThreadId, boCreationThread);
				});
			}
		}
	}
}
