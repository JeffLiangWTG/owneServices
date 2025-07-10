using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	sealed class GridSelectionTest : TestCaseWithFactory
	{
		public void TestGetSelectedRecordPKs()
		{
			DummyBusinessObject parent = Factory.New<DummyBusinessObject>();
			DummyChildBusinessObjectExcludable child1 = Factory.New<DummyChildBusinessObjectExcludable>();
			parent.Collection.Add(child1);
			DummyChildBusinessObjectExcludable child2 = Factory.New<DummyChildBusinessObjectExcludable>();
			parent.Collection.Add(child2);
			DummyChildBusinessObjectExcludable child3 = Factory.New<DummyChildBusinessObjectExcludable>();
			parent.Collection.Add(child3);
			DummyChildBusinessObjectExcludable child4 = Factory.New<DummyChildBusinessObjectExcludable>();
			parent.Collection.Add(child4);
			DummyChildBusinessObjectExcludable child5 = Factory.New<DummyChildBusinessObjectExcludable>();
			parent.Collection.Add(child5);

			Dictionary<ZGuid, string> map = new Dictionary<ZGuid, string>();
			map[child1.PK] = "child1";
			map[child2.PK] = "child2";
			map[child3.PK] = "child3";
			map[child4.PK] = "child4";
			map[child5.PK] = "child5";

			using (ZForm form = new ZForm(parent))
			{
				form.Size = new Size(300, 200);

				ZTextBoxColumnStyleInfo column = new ZTextBoxColumnStyleInfo();
				column.Caption = "Description";
				column.ColumnName = "Z0_Description";

				ZModuleButtonGrid grid = new ZModuleButtonGrid();
				grid.Dock = DockStyle.Fill;
				grid.BindToFindBoxList = "FilteredCollection";
				grid.BindToGridList = "Collection";
				grid.ModuleID = DummyModuleIDs.Dummy;
				grid.InnerGrid.ColumnStyles.Add(column);

				form.Controls.Add(grid);
				form.Show();
				Application.DoEvents();

				ITargetRecordSelection selection = new GridSelection(grid);

				// PKs should always appear in the same order as the grid.

				const string expected1 = @"
AutoSelectedAllKeys: True
  child1
  child2
  child3
  child4
  child5
";

				const string expected2 = @"
AutoSelectedAllKeys: False
  child2
  child3
  child4
";

				AssertMultilineASCIIEquals("", expected1, TestHelper.RenderSelection(map, selection.GetSelectedRecords()));
				AssertEquals(0, selection.ExclusionReasons.Count);

				TestHelper.SelectExact(grid.InnerGrid, child2, child3, child4);
				AssertMultilineASCIIEquals("", expected2, TestHelper.RenderSelection(map, selection.GetSelectedRecords()));
				AssertEquals(0, selection.ExclusionReasons.Count);

				child2.ShouldExclude = true;
				child4.ShouldExclude = true;

				const string expected3 = @"
AutoSelectedAllKeys: True
  child1
  child3
  child5
";
				TestHelper.SelectExact(grid.InnerGrid);
				AssertMultilineASCIIEquals("", expected3, TestHelper.RenderSelection(map, selection.GetSelectedRecords()));
				AssertEquals(2, selection.ExclusionReasons.Count);
				AssertEquals("!?!", selection.ExclusionReasons[0]);
				AssertEquals("!?!", selection.ExclusionReasons[1]);

				const string expected4 = @"
AutoSelectedAllKeys: False
  child3
";
				TestHelper.SelectExact(grid.InnerGrid, child2, child3, child4);
				AssertMultilineASCIIEquals("", expected4, TestHelper.RenderSelection(map, selection.GetSelectedRecords()));
				AssertEquals(2, selection.ExclusionReasons.Count);
				AssertEquals("!?!", selection.ExclusionReasons[0]);
				AssertEquals("!?!", selection.ExclusionReasons[1]);

				const string expected5 = @"
AutoSelectedAllKeys: False
";
				TestHelper.SelectExact(grid.InnerGrid, child2, child4);
				AssertMultilineASCIIEquals("", expected5, TestHelper.RenderSelection(map, selection.GetSelectedRecords()));
				AssertEquals(2, selection.ExclusionReasons.Count);
				AssertEquals("!?!", selection.ExclusionReasons[0]);
				AssertEquals("!?!", selection.ExclusionReasons[1]);
			}
		}
	}
}
