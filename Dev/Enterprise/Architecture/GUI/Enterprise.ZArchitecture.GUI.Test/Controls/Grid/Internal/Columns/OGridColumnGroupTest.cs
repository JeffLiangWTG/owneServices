using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Core.Forms
{
	sealed class OGridColumnGroupTest : OGridColumnGroupBaseTest
	{
		public void TestOGridColumnGroupWithoutGroupName()
		{
			var systemColumn = NewColumn("Column1", new ResourceStringData("", ""), true, false);
			var systemGroup = new ZGridColumnGroup(systemColumn);
			AssertEquals("Group IsVisible", true, systemGroup.IsVisible);
			AssertEquals("Group Name", "Column1", systemGroup.GroupName.Caption);

			var customColumn = NewColumn("Column1", new ResourceStringData("", ""), true, true);
			var customGroup = new ZGridColumnGroup(customColumn);
			AssertEquals("Group IsVisible", true, customGroup.IsVisible);
			AssertEquals("Group Name", "Column1*", customGroup.GroupName.Caption);

			systemColumn.ColumnStyle.Dispose();
			customColumn.ColumnStyle.Dispose();
		}

		public void TestOGridColumnGroupWithGroupName()
		{
			var column1 = NewColumn("Column1", new ResourceStringData("", "Group1"), false);
			var group = new ZGridColumnGroup(column1);
			AssertEquals("Group IsVisible", false, group.IsVisible);
			AssertEquals("Group Name", "Group1", group.GroupName.Caption);
			AssertEquals("Group Column Count", 1, group.Columns.Length);

			var column2 = NewColumn("Column2", new ResourceStringData("", "Group1"), true);
			group.Add(column2);
			AssertEquals("Group IsVisible", true, group.IsVisible);
			AssertEquals("Group Name", "Group1", group.GroupName.Caption);
			AssertEquals("Group Column Count", 2, group.Columns.Length);

			column1.ColumnStyle.Dispose();
			column2.ColumnStyle.Dispose();
		}

		public void TestToString()
		{
			using (var form = new ZForm())
			using (var grid = new ZGrid())
			{
				var id = Guid.NewGuid().ToString();

				form.Controls.Add(grid);
				grid.GridId = id;
				var info1 =
					new ZTextBoxColumnStyleInfo
					{
						ColumnName = DummyBizoSchema.Constants.Z0_Code,
						Caption = "Column Caption 1",
						GroupName = new ResourceStringData("", "Some Group name")
					};
				var info2 =
					new ZTextBoxColumnStyleInfo
					{
						ColumnName = DummyBizoSchema.Constants.Z0_VarCharMax,
						Caption = "Column Caption 2",
						GroupName = new ResourceStringData("", "Some Group name"),
					};
				grid.ColumnStyles.Add(info1);
				grid.ColumnStyles.Add(info2);
				grid.SetDataBinding(Dummy, "Collection", Dummy.Collection.GetType().Name);

				var group = new ZGridColumnGroup(grid.Columns[0]);

				AssertEquals("Column Caption 1", group.ToString());

				group.Add(grid.Columns[1]);

				AssertEquals("Some Group name", group.ToString());
			}
		}

		public void TestICustomizableColumn()
		{
			var column1 = NewColumn("Column1", new ResourceStringData("", "Group1"), false);
			column1.IsVisible = true;
			var group = new ZGridColumnGroup(column1);
			ICustomizableColumn customizableColumn = group;

			AssertEquals(true, column1.ToString().Equals("Column1"));

			AssertEquals(true, group.IsVisible);
			AssertEquals(true, customizableColumn.IsVisible);

			customizableColumn.IsVisible = false;

			AssertEquals(false, group.IsVisible);
			AssertEquals(false, customizableColumn.IsVisible);

			customizableColumn.IsVisible = true;

			AssertEquals(true, group.IsVisible);
			AssertEquals(true, customizableColumn.IsVisible);

			column1.ColumnStyle.Dispose();

			var column2 = NewColumn("Column2", new ResourceStringData("", "Group1"), false, true);
			group = new ZGridColumnGroup(column2);
			customizableColumn = group;

			AssertEquals(true, group.IsCustomColumn);
			AssertEquals("Column2*", customizableColumn.ToString());

			column2.ColumnStyle.Dispose();
		}
	}
}
