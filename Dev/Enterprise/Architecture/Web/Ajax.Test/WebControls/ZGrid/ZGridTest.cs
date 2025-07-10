using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls.ZGridInternals;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	class ZGridForTesting : ZGrid
	{
		internal ZGridLayoutManager GridManagerForTesting => GridManager;
	}

	[HttpContextEnabledTest]
	sealed class ZGridTest : WebControlTest
	{
		public void TestSaveAsLayoutColumns()
		{
			var testGrid = new ZGridForTesting();
			testGrid.Page = new TestPage();
			testGrid.ID = "TestGrid";
			testGrid.GridManagerForTesting.SaveGridLayoutForTest("0,2,3", "OriginalLayout");
			testGrid.ColumnProvider = new TestColumnProvider();
			testGrid.LayoutNameToUse = "OriginalLayout";
			testGrid.RepopulateColumns();

			testGrid.SaveAsLayoutColumns("NewLayout", false, true);
			testGrid.LayoutNameToUse = "NewLayout";
			testGrid.RepopulateColumns();
			AssertEquals("NewLayout", testGrid.LayoutNameToUse);
			AssertEquals(3, testGrid.Columns.Count);
			AssertEquals("Type", testGrid.Columns[0].HeaderText);
			AssertEquals("Packs", testGrid.Columns[1].HeaderText);
			AssertEquals("Departure", testGrid.Columns[2].HeaderText);

			testGrid.GridManagerForTesting.SaveGridLayoutForTest("1,4", "OriginalLayout");
			testGrid.LayoutNameToUse = "OriginalLayout";
			testGrid.RepopulateColumns();

			testGrid.SaveAsLayoutColumns("NewLayout", true, false);
			testGrid.LayoutNameToUse = "NewLayout";
			testGrid.RepopulateColumns();
			AssertEquals("NewLayout", testGrid.LayoutNameToUse);
			AssertEquals(2, testGrid.Columns.Count);
			AssertEquals("Mode", testGrid.Columns[0].HeaderText);
			AssertEquals("Arrival", testGrid.Columns[1].HeaderText);

			testGrid.GridManagerForTesting.SaveGridLayoutForTest("1,2,3,4", "OriginalLayout");
			testGrid.LayoutNameToUse = "OriginalLayout";
			testGrid.RepopulateColumns();

			testGrid.SaveAsLayoutColumns("NewLayout", false, false);
			testGrid.LayoutNameToUse = "NewLayout";
			testGrid.RepopulateColumns();
			AssertEquals("NewLayout", testGrid.LayoutNameToUse);
			AssertEquals(4, testGrid.Columns.Count);
			AssertEquals("Mode", testGrid.Columns[0].HeaderText);
			AssertEquals("Packs", testGrid.Columns[1].HeaderText);
			AssertEquals("Departure", testGrid.Columns[2].HeaderText);
			AssertEquals("Arrival", testGrid.Columns[3].HeaderText);
		}

		public void TestDownloadEDocs()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_VarCharMax = "ABC";

			var docManagerInfo = ((IDocManagerSupport)parent).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ACVPublished.txt", "ACV").IsPublished = true;
			docManagerInfo.MasterFactory.Save();

			var grid = new ZGrid();
			grid.Page = new ZTestPage();
			grid.BindTo = "Collection";
			dummy.Collection.Add(parent);
			grid.Bind(dummy);

			grid.DownloadEDocs();

			//We can't read the content from HttpContext.Current.Response.OutputStream, so just checking the content type is an actual zip
			AssertNotEquals("No eDocs Type selected, there should be no zip to donwload", DataContentTypes.Zip, HttpContext.Current.Response.ContentType);

			grid.SelectCellValue(parent.PK, new List<ZGuid> { parent.PK }, "Dummy Bizo", "ACV", true);
			grid.DownloadEDocs();
			AssertEquals(DataContentTypes.Zip, HttpContext.Current.Response.ContentType);
		}

		protected override void SetUp()
		{
			base.SetUp();

			parent = Factory.New<OrgHeader>();
			parent.OH_Code = "UnitTest";

			documentFactory = ObjectFactory.Get<IDocumentFactoryProvider>().GetFactory(Factory);
			_ = documentFactory.RetrieveExistingOrCreateStorageMainForPK(parent.PK, "ORG");

			documentFactory.Save();
		}
		IDocumentFactory documentFactory;
		OrgHeader parent;

		protected override Control GetNewControl()
		{
			return new ZGrid();
		}

		class TestColumnProvider : GridColumnProvider
		{
			protected override void CustomizeDictionaryCore()
			{
				base.CustomizeDictionaryCore();
				AddToDictionary(new ZTextEditColumn("Type", "Type") { ColumnKey = 0 });
				AddToDictionary(new ZTextEditColumn("Mode", "Mode") { ColumnKey = 1 });
				AddToDictionary(new ZCalcEditColumn("Packs", "Packs") { ColumnKey = 2 });
				AddToDictionary(new ZDateTimeColumn("Departure", "Departure", ZDateTimePickerFormat.Long) { ColumnKey = 3 });
				AddToDictionary(new ZDateTimeColumn("Arrival", "Arrival", ZDateTimePickerFormat.Long) { ColumnKey = 4 });
			}
		}

		class TestPage : ZPage
		{
			public TestPage()
				: base()
			{
			}

			protected override string GetPageName()
			{
				return "TestPage";
			}
		}
	}
}
