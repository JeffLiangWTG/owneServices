using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZGridActiveCollectionBindingTestCase : TestCaseWithFactory
	{
		public void TestIt()
		{
			Form.Show();
			Application.DoEvents();
			Form.gridMaster.Focus();
			Form.gridMaster.BeginEdit(Form.gridMaster.TableStyles[0].GridColumnStyles[0], 0);

			SendKey(Form.ActiveControl, Keys.D1);
			Form.gridMaster.CurrentCell = new DataGridCell(1, 0);
			Form.gridMaster.CurrentCell = new DataGridCell(0, 0);
			AssertEquals("1 item in the collection", 1, Collection.Count);
			AssertEquals("1 row on the grid", 2, Form.gridMaster.VisibleRowCount);

			Form.gridMaster.CurrentCell = new DataGridCell(1, 0);
			SendKey(Form.ActiveControl, Keys.D2);
			Form.gridMaster.CurrentCell = new DataGridCell(0, 0);
			AssertEquals("2 items in the collection", 2, Collection.Count);
			AssertEquals("2 rows on the grid", 3, Form.gridMaster.VisibleRowCount);

			Form.gridMaster.CurrentCell = new DataGridCell(2, 0);
			SendKey(Form.ActiveControl, Keys.X);
			Form.gridMaster.CurrentCell = new DataGridCell(1, 0);
			AssertEquals("Excluded item not added to the collection", 2, Collection.Count);
			AssertEquals("Excluded item not shown on the grid", 3, Form.gridMaster.VisibleRowCount);

			Form.gridMaster.CurrentCell = new DataGridCell(0, 0);
			SendKey(Form.ActiveControl, Keys.X);
			Form.gridMaster.CurrentCell = new DataGridCell(1, 0);
			Form.gridMaster.CurrentCell = new DataGridCell(0, 0);
			AssertEquals("Excluded item not added to the collection", 1, Collection.Count);
			AssertEquals("Excluded item not shown on the grid", 2, Form.gridMaster.VisibleRowCount);
		}

		#region Implementation

		void SendKey(Control control, Keys key)
		{
			KeySender.SendKeyDown(control, control.Handle, key);
			KeySender.SendKeyPress(control, control.Handle, key);
		}

		ZGridActiveCollectionBindingTestCaseForm Form
		{
			get
			{
				if (form == null)
				{
					form = new ZGridActiveCollectionBindingTestCaseForm(Collection);
				}
				return form;
			}
		}
		ZGridActiveCollectionBindingTestCaseForm form;

		ActiveBusinessObjectCollection<DummyWithDependentsBusinessObject> Collection
		{
			get { return new ActiveBusinessObjectCollection<DummyWithDependentsBusinessObject>(Factory, new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.NotEqual, "x")); }
		}

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
