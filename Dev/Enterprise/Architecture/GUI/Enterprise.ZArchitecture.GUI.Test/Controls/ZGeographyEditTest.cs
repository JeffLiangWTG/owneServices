using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZGeographyEditTest : TestCase
	{
		public void TestControlLayout()
		{
			AssertNoErrorMessage(testControl.LayoutErrors);
		}

		#region Binding

		public void TestBinding()
		{
			var table = new DataTable();
			var column1 = new DataColumn("TestGeography", typeof(ZGeography));
			table.Columns.Add(column1);

			var row1 = table.NewRow();
			row1[column1] = new ZGeography("POINT (-121 48)");
			table.Rows.Add(row1);

			using (var testForm = new ZForm())
			{
				testForm.Controls.Add(testControl);
				testForm.Show();

				testControl.SetDataBinding(table, "TestGeography");
				AssertEquals("Selected Geography value after binding", row1[column1], testControl.GeographyValue);
				var expectedText = ((ZGeography)row1[column1]).ToString().ToUpper();
				AssertEquals("Text property value after binding", expectedText, testControl.Text);

				testControl.GeographyValue = new ZGeography("POINT (-122 46.5)");
				expectedText = testControl.GeographyValue.ToString().ToUpper();
				AssertEquals("Text property value after changing SelectedGeography", expectedText, testControl.Text);
			}
		}

		#endregion

		#region EditableControl

		public void TestIsEditing()
		{
			Form.Controls.Add(GeographyEdit);
			Form.Show();
			GeographyEdit.BindTo = AutoDummyBizo.Schema.Z0_Geography;
			Form.SetDataBinding(Dummy, "");
			Application.DoEvents();
			var editableControl = EditableControl.Get(GeographyEdit);

			GeographyEdit.Focus();
			AssertEquals(false, editableControl.IsEditing);
			GeographyEdit.GeographyTextBox.Text = "POINT (-122 44)";
			AssertEquals(true, editableControl.IsEditing);
		}

		public void TestGetFrontMostActiveControl()
		{
			Form.Controls.Add(GeographyEdit);
			Form.Show();
			GeographyEdit.BindTo = AutoDummyBizo.Schema.Z0_Geography;
			Form.SetDataBinding(Dummy, "");
			Application.DoEvents();

			GeographyEdit.Focus();
			var control = Form.GetFrontMostActiveControl();
			AssertEquals(GeographyEdit, control);
		}

		#endregion

		#region GeographyValue

		public void TestGeographyValueChanged()
		{
			Form.Controls.Add(GeographyEdit);
			Form.Show();
			Application.DoEvents();

			var geographyValueChangedFired = false;
			GeographyEdit.GeographyValueChanged += delegate
			{ geographyValueChangedFired = true; };

			GeographyEdit.GeographyValue = ZGeography.Empty;
			AssertEquals(false, geographyValueChangedFired);
			GeographyEdit.GeographyValue = new ZGeography("POINT (-121 48)");
			AssertEquals(true, geographyValueChangedFired);
			geographyValueChangedFired = false;
			GeographyEdit.GeographyValue = new ZGeography("POINT (-121.1 48.1)");
			AssertEquals(true, geographyValueChangedFired);
			geographyValueChangedFired = false;
			GeographyEdit.GeographyValue = ZGeography.Empty;
			AssertEquals(true, geographyValueChangedFired);
			geographyValueChangedFired = false;
			GeographyEdit.Text = "POINT (-121.1 48.1)";
			AssertEquals(true, geographyValueChangedFired);
		}

		#endregion

		#region ReadOnly

		public void TestReadOnlyChanged()
		{
			Form.Controls.Add(GeographyEdit);
			Form.Show();
			Application.DoEvents();

			var readOnlyChangedFiredCount = 0;
			GeographyEdit.ReadOnlyChanged += delegate
			{ readOnlyChangedFiredCount++; };

			GeographyEdit.ReadOnly = false;
			AssertEquals(0, readOnlyChangedFiredCount);

			GeographyEdit.ReadOnly = true;
			AssertEquals(1, readOnlyChangedFiredCount);

			GeographyEdit.GeographyTextBox.ReadOnly = false;
			AssertEquals(2, readOnlyChangedFiredCount);
		}

		#endregion

		#region Implementation

		GeographyEditTestClass testControl;

		DummyBusinessObject Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = Factory.New<DummyBusinessObject>();
				}
				return dummy;
			}
		}
		DummyBusinessObject dummy;

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		ZChildForm Form
		{
			get { return form ?? (form = new ZChildForm()); }
		}
		ZChildForm form;

		ZGeographyEdit GeographyEdit
		{
			get { return geographyEdit ?? (geographyEdit = new ZGeographyEdit()); }
		}
		ZGeographyEdit geographyEdit;

		protected override void SetUp()
		{
			base.SetUp();
			testControl = new GeographyEditTestClass();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
			if (geographyEdit != null)
			{
				geographyEdit.Dispose();
			}
			if (testControl != null)
			{
				testControl.Dispose();
			}
		}

		static void AssertNoErrorMessage(string message)
		{
			Assert(message, string.IsNullOrEmpty(message));
		}

		#endregion
	}
}
