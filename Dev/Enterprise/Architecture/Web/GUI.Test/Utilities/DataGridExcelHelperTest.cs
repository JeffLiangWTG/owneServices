using System.IO;
using System.Linq;
using System.Web;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.Utilities.Testing
{
	public class DataGridExcelHelperTest : TestCaseWithFactory
	{
		#region TestExportToExcel

		[HttpContextEnabledTest]
		public void TestExportToExcelWithNoColumns()
		{
			var testGrid = new ZDataGrid();
			testGrid.DataSource = new DummyBusinessObjectCollection(Factory);

			for (int i = 0; i < 20; i++)
			{
				testGrid.Collection.Add(Factory.New<DummyBusinessObject>());
			}

			var ms = new MemoryStream();
			var testResponseFilter = new TestResponseFilter(HttpContext.Current.Response.Filter, ms);
			HttpContext.Current.Response.Filter = testResponseFilter;

			testGrid.ExportIntoExcel();
			HttpContext.Current.Response.Flush();

			Assert("The response must be empty as there were no columns", ms.Length == 0);
		}

		[HttpContextEnabledTest]
		public void TestExportToExcelWithCustomColumns()
		{
			var testGrid = new ZDataGrid();
			testGrid.DataSource = new DummyBusinessObjectCollection(Factory);

			var myColumn = new ZTimelineColumn("TestColumn", AutoDummyBizo.Schema.Z0_Date, AutoDummyBizo.Schema.Z0_AnotherDate, ZDateTimePickerFormat.Short);
			testGrid.Columns.Add(myColumn);

			AssertEquals("TestGrid should be containt 1 column", 1, testGrid.Columns.Count);
			AssertEquals("Column should be ZTimelineColumn type", typeof(ZTimelineColumn), testGrid.Columns[0].GetType());

			for (int i = 0; i < 20; i++)
			{
				testGrid.Collection.Add(Factory.New<DummyBusinessObject>());
			}

			var ms = new MemoryStream();
			var testResponseFilter = new TestResponseFilter(HttpContext.Current.Response.Filter, ms);
			HttpContext.Current.Response.Filter = testResponseFilter;

			testGrid.ExportIntoExcel();
			HttpContext.Current.Response.Flush();

			Assert("The response must not be empty.", ms.Length > 0);

			var dummyApplication = (DummyHttpApplication)HttpContext.Current.ApplicationInstance;
			var dummyWorkerRequest = dummyApplication.WorkerRequest;
			AssertNotNull("The Content-Disposition header must be present.", dummyWorkerRequest.Headers["Content-Disposition"]);
			AssertEquals("The Content-Disposition header must specify the file as an attachment, and specify a filename",
				"attachment; filename=\"SearchResults.xls\"", dummyWorkerRequest.Headers["Content-Disposition"]);

			AssertEquals("The content type must be application/vnd.ms-excel", "application/vnd.ms-excel", HttpContext.Current.Response.ContentType);
		}

		public void TestDropDownListColumnsAreExcludedIfBoundToListIncorrectly()
		{
			var testGrid = new ZDataGrid();

			testGrid.DataSource = new DummyBusinessObjectCollection(Factory)
									  {
										Factory.New<DummyBusinessObject>()
									  };
			var testColumn = new ZDropDownListColumn("testHeader", DummyBizoSchema.Z0_Description.Name, "blablabla");
			testGrid.Columns.Add(testColumn);
			AssertEquals("Bad BindToList -- column shouldn't be added", 0, new DataGridExcelExportHelper(testGrid).GetExcelExportColumns().Count);

			testGrid.DataSource = new DummyBusinessObjectCollection(Factory)
									  {
										Factory.New<DummyBusinessObject>()
									  };
			testColumn = new ZDropDownListColumn("testHeader", DummyBizoSchema.Z0_Description.Name);
			testGrid.Columns.Add(testColumn);
			AssertEquals("No BindToList -- should be added", 1, new DataGridExcelExportHelper(testGrid).GetExcelExportColumns().Count);
		}

		public void TestGetExcelExportColumnsIsIgnoringArgumentException()
		{
			var grid = new ZDataGrid();
			grid.DataSource = new DummyBusinessObjectCollection(Factory) { Factory.New<DummyBusinessObject>() };
			grid.Columns.Add(new ZTextEditColumn("Valid Column", DummyBizoSchema.Z0_Description.Name));
			grid.Columns.Add(new ZTextEditColumn("Invalid Column", "Z1_Description"));
			var helper = new DataGridExcelExportHelper(grid);
			var exportColumns = helper.GetExcelExportColumns();

			AssertEquals(1, exportColumns.Count);
			AssertEquals("Valid Column", exportColumns.First().Description);
		}

		public void TestHyperLinkColumnsAreExcludedIfBoundToIsEmpty()
		{
			var testGrid = new ZDataGrid();
			testGrid.DataSource = new DummyBusinessObjectCollection(Factory)
									  {
										Factory.New<DummyBusinessObject>()
									  };
			var testHyperLinkColumn = new ZHyperLinkColumn("testHedaer", "");
			testGrid.Columns.Add(testHyperLinkColumn);
			AssertEquals("Bad BindToList -- column shouldn't be added", 0, new DataGridExcelExportHelper(testGrid).GetExcelExportColumns().Count);
		}

		[HttpContextEnabledTest]
		public void TestExportToExcelIgnoresColumnBoundToNonZType()
		{
			var testGrid = new ZDataGrid();
			testGrid.DataSource = new DummyBusinessObjectCollection(Factory);

			var myColumn = new ZNewRowColumn("DoNotBindToMe");
			testGrid.Columns.Add(myColumn);

			AssertEquals("TestGrid should be containt 1 column", 1, testGrid.Columns.Count);
			AssertEquals("Column should be ZNewRowColumn type", typeof(ZNewRowColumn), testGrid.Columns[0].GetType());

			for (var i = 0; i < 20; i++)
			{
				testGrid.Collection.Add(Factory.New<DummyBusinessObject>());
			}

			var ms = new MemoryStream();
			var testResponseFilter = new TestResponseFilter(HttpContext.Current.Response.Filter, ms);
			HttpContext.Current.Response.Filter = testResponseFilter;

			testGrid.ExportIntoExcel();
			HttpContext.Current.Response.Flush();

			Assert("The response must be empty because our single column was bound to DoNotBindToMe property that is type of string", ms.Length == 0);
		}

		[HttpContextEnabledTest]
		public void TestExportToExcelIgnoresColumnBoundToNull()
		{
			var testGrid = new ZDataGrid();
			testGrid.DataSource = new DummyBusinessObjectCollection(Factory);

			var myColumn = new ZNewRowColumn(null);
			testGrid.Columns.Add(myColumn);

			AssertEquals("TestGrid should be containt 1 column", 1, testGrid.Columns.Count);
			AssertEquals("Column should be ZNewRowColumn type", typeof(ZNewRowColumn), testGrid.Columns[0].GetType());

			for (int i = 0; i < 20; i++)
			{
				testGrid.Collection.Add(Factory.New<DummyBusinessObject>());
			}

			var ms = new MemoryStream();
			var testResponseFilter = new TestResponseFilter(HttpContext.Current.Response.Filter, ms);
			HttpContext.Current.Response.Filter = testResponseFilter;

			testGrid.ExportIntoExcel();
			HttpContext.Current.Response.Flush();

			Assert("The response must be empty because our single column was bound to null", ms.Length == 0);
		}

		#region TestExportToExcelWithCollectionsColumns

		[HttpContextEnabledTest]
		public void TestExportToExcelWithCollectionsColumns()
		{
			var testGrid = new ZDataGrid();
			testGrid.DataSource = new DummyBusinessObjectCollection(Factory);

			testGrid.Columns.Add(new ZHyperLinksColumn("TestColumn", "CollectionWithPublicSetter", PKDescription.Schema.Description));

			AssertEquals("TestGrid should be containt 1 column", 1, testGrid.Columns.Count);
			AssertEquals("Column should be ZHyperLinksColumn type", typeof(ZHyperLinksColumn), testGrid.Columns[0].GetType());

			for (int i = 0; i < 20; i++)
			{
				var bizO = Factory.New<DummyBusinessObject>();

				var collection = new PKDescriptionCollection();
				collection.Add(new PKDescription(ZGuid.NewZGuid(), "Some Description"));

				bizO.CollectionWithPublicSetter = collection;

				testGrid.Collection.Add(bizO);
			}

			var ms = new MemoryStream();
			HttpContext.Current.Response.Filter = new TestResponseFilter(HttpContext.Current.Response.Filter, ms);

			testGrid.ExportIntoExcel();
			HttpContext.Current.Response.Flush();

			Assert("The response must not be empty.", ms.Length > 0);

			var dummyApplication = (DummyHttpApplication)HttpContext.Current.ApplicationInstance;
			var dummyWorkerRequest = dummyApplication.WorkerRequest;
			AssertNotNull("The Content-Disposition header must be present.", dummyWorkerRequest.Headers["Content-Disposition"]);
			AssertEquals("The Content-Disposition header must specify the file as an attachment, and specify a filename",
				"attachment; filename=\"SearchResults.xls\"", dummyWorkerRequest.Headers["Content-Disposition"]);

			AssertEquals("The content type must be application/vnd.ms-excel", "application/vnd.ms-excel", HttpContext.Current.Response.ContentType);
		}

		#endregion

		#endregion

		#region TestShouldExportColumn

		public void TestColumnIsVisible()
		{
			var gridColumn = new ZTextEditColumn("Header1", "SomethingToBindTo1");
			var newRowColumn = new ZNewRowColumn("SomethingToBindTo2");

			Assert("ShouldExport should be true", DataGridExcelExportHelper.ColumnIsVisible(gridColumn));
			Assert("ShouldExport should be true", DataGridExcelExportHelper.ColumnIsVisible(newRowColumn));
		}

		#endregion
	}
}
