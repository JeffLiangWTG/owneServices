using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Grid.Testing
{
	[TestedType(typeof(GridRowFinderForm))]
	class GridRowFinderFormTest : ZFormBasherTest
	{
		public void TestCancelButtonShouldWork()
		{
			using (TestFormForGridRowFinder form = new TestFormForGridRowFinder(Factory.New<DummyBusinessObjectForRowFinderTest>()))
			{
				form.Show();

				GridRowFinderBusinessObject rowFinderBizO = new GridRowFinderBusinessObject(form.Grid);
				rowFinderBizO.ColumnsToSearch.AddNew("Test1", false);
				rowFinderBizO.ColumnsToSearch.AddNew("Test2", true);
				using (GridRowFinderForm rowFinderForm = new GridRowFinderForm(rowFinderBizO))
				{
					rowFinderForm.Show();
					rowFinderForm.CancelButton.PerformClick();
					AssertEquals(DialogResult.Cancel, rowFinderForm.DialogResult);
					Assert("form should be closed.", rowFinderForm.IsDisposed);
				}
			}
		}

		public void TestSelectedColumnDoesntResetOnFormLoad()
		{
			using (TestFormForGridRowFinder form = new TestFormForGridRowFinder(Factory.New<DummyBusinessObjectForRowFinderTest>()))
			{
				form.Show();

				GridRowFinderBusinessObject rowFinderBizO = new GridRowFinderBusinessObject(form.Grid);
				rowFinderBizO.ColumnsToSearch.AddNew("Test1", false);
				rowFinderBizO.ColumnsToSearch.AddNew("Test2", true);
				using (GridRowFinderForm rowFinderForm = new GridRowFinderForm(rowFinderBizO))
				{
					AssertEquals(false, rowFinderBizO.ColumnsToSearch["Test1"].Value);
					AssertEquals(true, rowFinderBizO.ColumnsToSearch["Test2"].Value);
				}
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			using (TestFormForGridRowFinder form = new TestFormForGridRowFinder(Factory.New<DummyBusinessObjectForRowFinderTest>()))
			{
				form.Show();
				//Application.DoEvents();

				GridRowFinderBusinessObject rowFinderBizO = new GridRowFinderBusinessObject(form.Grid);
				return new GridRowFinderForm(rowFinderBizO);
			}
		}

		#endregion
	}
}
