using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class ZGeographyEditColumnStyleTest : TestCaseWithDummy
	{
		public void TestCommitReadOnly()
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var item = collection.AddNew();

			using (var form = new ZForm(collection))
			{
				using (var grid = new ZGrid())
				{
					grid.SetBindingMember(".");
					var info = new TestZGeographyEditColumnStyleInfo("Z0_Geography", 100);
					grid.ColumnStyles.Add(info);
					grid.Dock = DockStyle.Fill;
					form.Controls.Add(grid);
					form.Show();
					Application.DoEvents();
					var style = (TestZGeographyEditColumnStyle)grid.Columns[0].ColumnStyle;

					style.IsEditingForTest = true;
					style.TestEditValue = new ZGeography("POINT (-122 47)");
					AssertEquals("Expecting success commit when cell is editable", true, style.Commit_DebugAccess(grid.ListManager, 0));
					AssertEquals("Expecting a value update to editable cell", new ZGeography("POINT (-122 47)"), item.Z0_Geography);

					style.IsEditingForTest = true;
					style.ReadOnly = true;
					style.TestEditValue = new ZGeography("POINT (-122 48)");
					AssertEquals("Expecting success commit when cell is read-only", true, style.Commit_DebugAccess(grid.ListManager, 0));
					AssertEquals("Expecting NO value update to read-only cell", new ZGeography("POINT (-122 47)"), item.Z0_Geography);
				}
			}
		}

		class TestZGeographyEditColumnStyleInfo : ZGeographyEditColumnStyleInfo
		{
			public TestZGeographyEditColumnStyleInfo(string columnName, int width)
				: base(columnName, width)
			{ }

			[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
			public override Type ColumnStyleType
			{
				get { return typeof(TestZGeographyEditColumnStyle); }
			}
		}

		class TestZGeographyEditColumnStyle : ZGeographyEditColumnStyle
		{
			public TestZGeographyEditColumnStyle(ZGeographyEditColumnStyleInfo columnInfo)
				: base(columnInfo)
			{
			}

			public void CommitForTest()
			{
				var context = new BindingContext();
				var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
				collection.AddNew();

				var testCurrencyManager = (CurrencyManager)context[collection];
				Commit(testCurrencyManager, 0);
			}

			protected override object EditValue
			{
				get { return TestEditValue; }
			}

			public ZGeography TestEditValue { get; set; }

			public bool IsEditingForTest
			{
				set { IsEditing = value; }
			}
		}
	}
	public class ZGeographyEditColumnStyleTestNotificationsWithEditControl : NotificationInGridTestCase
	{
		public override void TestLocationAndSizeWithAndWithoutNotifications()
		{
			CheckSizeAndLocation(50, new Point(37, 19), new Size(47, 16));

			Dummy.Collection[0].Z0_GeographyInfo.AddError("baad");
			CheckSizeAndLocation(50, new Point(49, 19), new Size(35, 16));
		}

		protected override string GetColumnName()
		{
			return AutoDummyBizo.Schema.Z0_Geography;
		}

		protected override ZGridColumnInfo GetInfo()
		{
			return new ZGeographyEditColumnStyleInfo();
		}
	}
}
