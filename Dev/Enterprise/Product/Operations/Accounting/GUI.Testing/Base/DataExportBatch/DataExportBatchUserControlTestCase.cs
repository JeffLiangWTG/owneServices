using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.DataExportBatch.Testing
{
	internal sealed class DataExportBatchUserControlTestCase : TestCaseWithFactory
	{
		public void TestGridBinding()
		{
			var source = TestObjectCreator.CreateDataExportBatchSource_TransactionHeader();
			var batch = TestObjectCreator.CreateDataExportBatchForHeader(source);
			Factory.Save();

			AssertEquals("Precondition: number of elements in collection", 1, source.DataExportBatchCollection.Count);

			using (var form = new ZForm(source))
			using (var control = new DataExportBatchUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals("Number of records on grid", 1, control.DataExportBatchGrid.ListManager.List.Count);
				AssertEquals("Record on grid represents the batch", batch.PK, ((GenExportBatchSequence)control.DataExportBatchGrid.ListManager.List[0]).PK);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
		}

		TestObjectCreator TestObjectCreator;
	}
}
