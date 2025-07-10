using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class ZTranslatableTextBoxColumnStyleTest : TestCaseWithDummy
	{
		public void TestColumnStyleBehavoiur()
		{
			var item = Factory.New<TranslatableDataFieldTestCase.DummyWithTranslatable>();
			item.Z0_Description = "One";
			item.ReadOnly = true;
			Dummy.Collection.Add(item);
			item = Factory.New<TranslatableDataFieldTestCase.DummyWithTranslatable>();
			item.Z0_Description = "Two";
			Dummy.Collection.Add(item);
			item = Factory.New<TranslatableDataFieldTestCase.DummyWithTranslatable>();
			item.Z0_Description = "Three";
			Dummy.Collection.Add(item);

			using (var form = new TestForm(Dummy))
			{
				form.Show();
				var columnStyle = (ZTranslatableTextBoxColumnStyle)form.grid.Columns[form.columnStyleInfo.ColumnName].ColumnStyle;
				form.grid.BeginEdit(columnStyle, 0);
				AssertType(typeof(ZTranslatableTextControl), columnStyle.EditControl);
				AssertEquals("One", columnStyle.EditControl.Text);
				AssertEquals(true, ((ZTranslatableTextControl)columnStyle.EditControl).ReadOnly);

				form.grid.BeginEdit(columnStyle, 1);
				AssertEquals("Two", columnStyle.EditControl.Text);
				AssertEquals(false, ((ZTranslatableTextControl)columnStyle.EditControl).ReadOnly);

				form.grid.BeginEdit(columnStyle, 2);
				AssertEquals("Three", columnStyle.EditControl.Text);
				columnStyle.EditControl.Text = "Changed Text";
				form.grid.BeginEdit(columnStyle, 0);
				Application.DoEvents();
				AssertEquals("Changed Text", item.Z0_Description);

				Form formCreated = null;
				var formCreatedHandler = new EventHandler(delegate(object sender, EventArgs args) { formCreated = sender as Form; });
				ZForm.FormCreated += formCreatedHandler;
				try
				{
					((ZTranslatableTextControl)columnStyle.EditControl).OnLanguageClick();
					AssertNotNull(formCreated);
					AssertEquals("CustomizableDataTranslationForm", formCreated.Name);
					formCreated.Dispose();
				}
				finally
				{
					ZForm.FormCreated -= formCreatedHandler;
				}
			}
		}

		class TestForm : ZForm
		{
			public TestForm(DummyBusinessObject dummy)
				: base(dummy)
			{
				grid = new ZGrid();
				grid.Dock = System.Windows.Forms.DockStyle.Fill;
				columnStyleInfo = new ZTranslatableTextBoxColumnStyleInfo();
				columnStyleInfo.ColumnName = "Z0_Description";
				columnStyleInfo.Width = 100;
				grid.ColumnStyles.Add(columnStyleInfo);
				grid.BindTo = "Collection";
				this.Controls.Add(grid);
			}

			public readonly ZGrid grid;
			public readonly ZTranslatableTextBoxColumnStyleInfo columnStyleInfo;
		}
	}
}
