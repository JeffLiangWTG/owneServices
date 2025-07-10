using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.PRA.GUI.Testing
{
	sealed class AUContainerMessagingPlugInTest : ZArchitecture.PlugIn.Testing.ZPlugInGenericTest
	{
		public void TestBusinessObjectSet()
		{
			using (AUContainerMessagingFormForTest form = new AUContainerMessagingFormForTest(collection))
			{
				form.Show();
				UserIdleWorker.Flush();
				form.PlugIns.Instances[0].SelectTabPage();
				for (int i = 0; i < collection.Count; i++)
				{
					form.zGrid1.CurrentRowIndex = i;
					AssertEquals("Current set correctly", collection[i], ((PRAContainerCollection)form.PlugIns.Instances[0].BusinessEntity)[0]);
				}

				collection.AddNew();
				collection.AddNew();
				for (int i = collection.Count - 1; i >= 0; i--)
				{
					form.zGrid1.CurrentRowIndex = i;
					AssertEquals("Current set correctly", collection[i], ((PRAContainerCollection)form.PlugIns.Instances[0].BusinessEntity)[0]);
				}

				collection.RemoveAll();
				Assert("No current", ((PRAContainerCollection)form.PlugIns.Instances[0].BusinessEntity).Count == 0);
			}
		}

		public void TestAddColumnsToGrid()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			using (AUContainerMessagingPlugIn plugin = new AUContainerMessagingPlugIn(container))
			{
				using (ZGrid grid = new ZGrid())
				{
					((IAddColumnsToGrid)plugin).AddColumnsToContainerGrid(grid);
					AssertEquals("Should have added 1 column", 1, grid.ColumnStyles.Count);
					AssertEquals("Should be ZTextBoxColumnStyleInfo", nameof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[0].GetType().Name);
					ZTextBoxColumnStyleInfo columnInfo = (ZTextBoxColumnStyleInfo)grid.ColumnStyles[0];
					AssertEquals("PRA Status", columnInfo.Caption);
					AssertEquals("CurrentPRAStatus", columnInfo.ColumnName);
					AssertEquals(true, columnInfo.IsVisible);
					AssertEquals("The current PRA Status.", columnInfo.ToolTip);
					AssertEquals(125, columnInfo.Width);
				}
			}
		}

		public void TestPlugInDoesNotCreatePRAMessageUsageLog()
		{
			var logFilter = new ZDBOnlyQuery(typeof(StmActivityLog));
			logFilter.AddToFilter(StmActivityLogSchema.S7_FormCaption, Env.Licence.PRAMessaging.Name);
			var beforSavingLogCount = Factory.GetDatabaseCount(typeof(StmActivityLog), logFilter);
			using (var form = new AUContainerMessagingFormForTest(collection))
			{
				form.Show();
				UserIdleWorker.Flush();
				form.PlugIns.Instances[0].SelectTabPage();
			}

			var afterSavingLogCount = Factory.GetDatabaseCount(typeof(StmActivityLog), logFilter);
			AssertEquals(0, afterSavingLogCount - beforSavingLogCount);
		}

		public void TestMessagingMenu_ForUnsavedConsol()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			using (var plugIn = new AUContainerMessagingPlugIn(consol))
			{
				var menu = plugIn.TopLevelMenu;
				plugIn.OnMenuShown();
				var menuText = string.Join(", ", plugIn.TopLevelMenu.MenuItems.Cast<MenuItem>().Select(x => x.Text));
				AssertMultilineASCIIEquals("menu items", @"Please save before sending messages", menuText);
				Factory.Save();
				plugIn.OnMenuShown();
				menuText = string.Join(", ", plugIn.TopLevelMenu.MenuItems.Cast<MenuItem>().Select(x => x.Text));
				AssertMultilineASCIIEquals("menu items", @"&Submit PRA for Selected Container, &Re-Send PRA for Selected Container, &Cancel PRA for Selected Container", menuText);
				consol.JK_AgentsReference = "ABCDE";
				plugIn.OnMenuShown();
				menuText = string.Join(", ", plugIn.TopLevelMenu.MenuItems.Cast<MenuItem>().Select(x => x.Text));
				AssertMultilineASCIIEquals("menu items", @"Please save before sending messages", menuText);
			}
		}

		protected override ZPlugIn GetPlugInToTest() => new AUContainerMessagingPlugIn(collection);

		ContainerNonDependentCollection collection;
		protected override void SetUp()
		{
			base.SetUp();
			collection = new ContainerNonDependentCollection(Factory);
			collection.AddNew();
		}
	}
}
