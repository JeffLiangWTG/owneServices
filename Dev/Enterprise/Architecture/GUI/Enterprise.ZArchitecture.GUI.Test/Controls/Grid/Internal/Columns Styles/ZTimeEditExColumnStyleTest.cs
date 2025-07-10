using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class ZTimeEditExColumnStyleTest : TestCaseWithDummy
	{
		public void TestReadOnly_Empty()
		{
			Dummy.Collection.AddNew();
			Dummy.Collection[0].ReadOnly = true;
			Dummy.Collection[0].Z0_Date = ZDateTime.Empty;
			Form.Show();
			Application.DoEvents();
			AssertEquals("", Form.GridColumn.TextBox.Text);
		}

		public void TestReadOnly_NonEmpty()
		{
			Dummy.Collection.AddNew();
			Dummy.Collection[0].ReadOnly = true;
			Dummy.Collection[0].Z0_Date = new ZDateTime(ZDateTime.Now.Year, 1, 1).AddHours(5).AddMinutes(15);
			Form.Show();
			Application.DoEvents();
			AssertEquals("005:15", Form.GridColumn.TextBox.Text);
			AssertEquals("selected", Form.GridColumn.TextBox.SelectionLength, Form.GridColumn.TextBox.Text.Length);
		}

		public void TestInvalidDefaultValue()
		{
			Dummy.Collection.AddNew();
			Dummy.Collection[0].Z0_Date = ZDateTime.Invalid;
			Form.Show();
			Application.DoEvents();
			AssertEquals("<INVALID>", Form.GridEditControl.Text);
		}

		public void TestPoking()
		{
			Form.Show();
			AssertNotNull("Precondition - grid is bound", Form.Grid.ListManager);
			AssertEquals("Precondition - grid focused", true, Form.Grid.ContainsFocus);
			AssertEquals("Precondition - grid collection should contain one record", 1, Dummy.Collection.Count);

			KeySender.PostKeyDown(Form.GridEditControl, Keys.D0);
			KeySender.PostKeyDown(Form.GridEditControl, Keys.D0);
			KeySender.PostKeyDown(Form.GridEditControl, Keys.D5);
			KeySender.PostKeyDown(Form.GridEditControl, Keys.D1);
			KeySender.PostKeyDown(Form.GridEditControl, Keys.D5);
			Application.DoEvents();
			AssertEquals("00515", Form.GridEditControl.Text);

			KeySender.PostKeyDown(Form.GridEditControl, Keys.Tab);
			Application.DoEvents();
			AssertEquals(new ZDateTime(ZDateTime.Now.Year, 1, 1).AddHours(5).AddMinutes(15), Dummy.Collection[0].Z0_Date);
		}

		public void TestAllowNegative_False()
		{
			Form.Show();
			AssertEquals(false, Form.GridEditControl.AllowNegative);
		}

		public void TestAllowNegative_True()
		{
			Form.GridColumnInfo.AllowNegative = true;
			Form.Show();
			AssertEquals(true, Form.GridEditControl.AllowNegative);
		}

		#region Test Classes

		class ZFormWithTimeEditInGrid : ZForm
		{
			public ZFormWithTimeEditInGrid(DummyBusinessObject dummy)
				: base(dummy)
			{
				Grid = new ZGrid();
				Grid.ColumnStyles.Add(GridColumnInfo);
				Grid.BindTo = "Collection";

				var tabControl = new ZTabControl();
				var tabPage = new ZTabPage();
				tabPage.Controls.Add(Grid);
				tabControl.TabPages.Add(tabPage);
				Controls.Add(tabControl);
			}

			public ZTimeEditExColumnStyleInfo GridColumnInfo
			{
				get
				{
					if (gridColumnInfo == null)
					{
						gridColumnInfo = new ZTimeEditExColumnStyleInfo(DummyBizoSchema.Constants.Z0_Date, 80);
					}
					return gridColumnInfo;
				}
			}
			ZTimeEditExColumnStyleInfo gridColumnInfo;

			public ZTimeEditExColumnStyle GridColumn
			{
				get { return (ZTimeEditExColumnStyle)Grid.Columns[0].ColumnStyle; }
			}

			public ZTimeEditEx GridEditControl
			{
				get { return (ZTimeEditEx)GridColumn.EditControl; }
			}

			public readonly ZGrid Grid;
		}

		#endregion

		#region Implementation

		ZFormWithTimeEditInGrid Form
		{
			get
			{
				if (form == null)
				{
					form = new ZFormWithTimeEditInGrid(Dummy);
				}
				return form;
			}
		}
		ZFormWithTimeEditInGrid form;

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
