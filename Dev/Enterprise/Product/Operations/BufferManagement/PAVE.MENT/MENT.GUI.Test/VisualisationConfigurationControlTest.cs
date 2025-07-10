using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.PAVE.MENT.Business.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.PAVE.MENT.GUI.Test
{
	class VisualisationConfigurationControlTest : TestCaseWithFactory
	{
		public void TestFilterStrips_SwitchingBetweenExtractions_FiltersRefreshCorrectly()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "JIMMY");
			var extraction1 = MENTTestHelper.CreateExtraction(Factory, "JIMMY Extraction 1", query);
			var extraction2 = MENTTestHelper.CreateExtraction(Factory, "JIMMY Extraction 2", query);

			using (var form = new ZForm(query))
			{
				var visualisationConfigControl = new VisualisationConfigurationControl()
				{
					Name = "TestControl"
				};

				form.Controls.Add(visualisationConfigControl);
				visualisationConfigControl.SetBindingMember(".");

				form.Show();
				Application.DoEvents();

				ZGrid grid = visualisationConfigControl.FindAll<ZGrid>().First(g => g.Name == "extractionsGrid");
				grid.Select(1);
				Application.DoEvents();

				var filterStripControl = visualisationConfigControl.FindAll<FilterRuleFilterStripControl>().Single();

				var addStripButton = filterStripControl.FindAll<ZFilterStripAddButton>().Single().FindAll<ZButton>().Single();

				addStripButton.PerformClick();
				addStripButton.PerformClick();
				Application.DoEvents();

				var stripControls = filterStripControl.FindAll<ZFilterStrip>().ToArray();
				AssertEquals(3, stripControls.Length);
				AssertEquals(extraction1.SeriesFilter, filterStripControl.CurrentDataItem);

				grid.ListManager.Position = 1;
				Application.DoEvents();
				AssertEquals(1, filterStripControl.FindAll<ZFilterStrip>().ToArray().Length);
				AssertEquals(extraction2.SeriesFilter, filterStripControl.CurrentDataItem);

				grid.ListManager.Position = 0;
				Application.DoEvents();
				AssertEquals(1, filterStripControl.FindAll<ZFilterStrip>().ToArray().Length);
				AssertEquals(extraction1.SeriesFilter, filterStripControl.CurrentDataItem);
			}
		}

		public void TestOnCloneCorrectBindingOccursOnChange()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "JIMMY");
			var extraction1 = MENTTestHelper.CreateExtraction(Factory, "JIMMY Extraction 1", query);

			using (var control = new VisualisationConfigurationControl()
			{
				Name = "TestControl"
			})
			using (var form = new ZForm(query))
			{
				form.Controls.Add(control);
				control.SetBindingMember(".");

				form.Show();
				Application.DoEvents();

				ZGrid grid = control.FindAll<ZGrid>().First(g => g.Name == "extractionsGrid");
				grid.Select(0);
				Application.DoEvents();

				var filterStripControl = control.FindAll<FilterRuleFilterStripControl>().Single();

				var addStripButton = filterStripControl.FindAll<ZFilterStripAddButton>().Single().FindAll<ZButton>().Single();

				addStripButton.PerformClick();
				addStripButton.PerformClick();
				Application.DoEvents();

				var stripControls = filterStripControl.FindAll<ZFilterStrip>().ToArray();
				AssertEquals(3, stripControls.Length);
				AssertEquals(extraction1.SeriesFilter, filterStripControl.CurrentDataItem);

				grid.Select(0);
				grid.ContextMenu.DoPopup();
				var cloneMenuItem = grid.ContextMenu.MenuItems.FindByName("clone", true);
				cloneMenuItem.PerformClick();
				Application.DoEvents();

				grid.Select(1);
				grid.ListManager.Position = 1;
				Application.DoEvents();

				stripControls = filterStripControl.FindAll<ZFilterStrip>().ToArray();
				AssertNotEquals(extraction1, grid.ListManager.GetCurrent());
				AssertNotEquals(extraction1.SeriesFilter, filterStripControl.CurrentDataItem);
				AssertEquals(1, stripControls.Length);
			}
		}

		public void TestNoCrashWhenSwitchFromExtractionsGridToFilterStrip()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "JIMMY");

			using (var form = new ZForm(query))
			{
				var visualisationConfigControl = new VisualisationConfigurationControl()
				{
					Name = "TestControl"
				};

				form.Controls.Add(visualisationConfigControl);
				visualisationConfigControl.SetBindingMember(".");
				form.Show();
				Application.DoEvents();

				var grid = visualisationConfigControl.FindAll<ZGrid>().Single(g => g.Name == "extractionsGrid");
				Application.DoEvents();

				grid.BeginEdit(grid.TableStyles[0].GridColumnStyles[0], 0);
				grid[0, 0] = new ZString("TEST");
				TestKeyStrokeHelper.SendKeyToControl(grid, Keys.Tab);
				TestKeyStrokeHelper.SendKeyToControl(grid.LastFocusedColumn.EditControl, Keys.Space);
				TestKeyStrokeHelper.SendKeyToControl(grid, Keys.Tab);
				grid[0, 2] = new ZString("NON");
				TestKeyStrokeHelper.SendKeyToControl(grid, Keys.Tab);
				grid[0, 3] = new ZString("NON");
				TestKeyStrokeHelper.SendKeyToControl(grid, Keys.Tab);
				Application.DoEvents();

				var filterStripControl = visualisationConfigControl.FindAll<FilterRuleFilterStripControl>().Single();
				var filterStripDropEdit = filterStripControl.FindAll<ZFilterStripDropEdit>().Single();
				AssertNotNull(filterStripDropEdit);
				filterStripDropEdit.Focus();
				Application.DoEvents();
				AssertNotNull(filterStripDropEdit);
			}
		}
	}
}
