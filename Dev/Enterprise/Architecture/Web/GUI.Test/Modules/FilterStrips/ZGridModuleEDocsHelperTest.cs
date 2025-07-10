using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	sealed class ZGridModuleEDocsHelperTest : TestCaseWithFactory
	{
		public void TestAddEDocsColumnsToProvider()
		{
			var columnProvider = new GridColumnProvider();
			AssertEquals(0, columnProvider.AllColumns.Count);

			var docTypeACV = Factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "ACV"));
			var docTypeMSC = Factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "MSC"));

			var refDocTypeEntryCollection = new RefDocTypeEntryCollection();
			var entry1 = refDocTypeEntryCollection.AddNew();
			var entry2 = refDocTypeEntryCollection.AddNew();

			entry1.RefDocTypePK = docTypeACV.PK;
			entry2.RefDocTypePK = docTypeMSC.PK;

			var dictiornary = new WebEDocsDownloadEntryDictionary();
			dictiornary.Add(new WebEDocsDownloadModulesList().AllModules.Code, new WebEDocsDownloadEntry(true, refDocTypeEntryCollection));

			using (WebDataRegistry.Instance.WebEDocsBulkDownload.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dictiornary))
			using (var module = new DummyZFilterStripGridModuleWithISupportEDocsBulkDownload(Factory, new DummyPage()))
			{
				ZGridModuleEDocsHelper.AddEDocsColumnsToProvider(Factory, new ZDataGrid(), module, columnProvider);
				AssertEquals(3, columnProvider.AllColumns.Count);
				AssertEquals("ACV", columnProvider.AllColumns[0].HeaderText);
				AssertEquals("MSC", columnProvider.AllColumns[1].HeaderText);
				AssertEquals("All eDocs", columnProvider.AllColumns[2].HeaderText);
			}
		}

		public void TestGetZippedEDocs()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_VarCharMax = "ABC";

			var docManagerInfo = ((IDocManagerSupport)parent).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ACVPublished.txt", "ACV").IsPublished = true;
			docManagerInfo.AddFileOrDocument(new byte[] { 4, 5, 6 }, "MSCPublished.xxx", "MSC").IsPublished = true;
			docManagerInfo.AddFileOrDocument(new byte[] { 7, 8, 9 }, "MSCUnpublished.yyy", "MSC").IsPublished = false;
			docManagerInfo.MasterFactory.Save();

			var grid = new ZDataGrid();
			grid.Page = new ZTestPage();
			grid.BindTo = "Collection";
			dummy.Collection.Add(parent);
			grid.Bind(dummy);

			//One eDoc type selected
			grid.SelectCellValue(parent.PK, new List<ZGuid> { parent.PK }, "Organization (UnitTest)", "ACV", true);
			using (var stream = new MemoryStream())
			{
				var result = ZGridModuleEDocsHelper.TryGetZippedEDocs(grid, stream);
				Assert(result);
				var bytes = stream.ToArray();

				var zipExtractor = new ZipExtractor();
				var zippedFileNames = zipExtractor.GetFileNames(new MemoryStream(bytes));
				AssertContainsExactElementsInAnyOrder(new string[] { "[Organization (UnitTest)]-[ACV]-ACVPublished.txt" }, zippedFileNames);

				using (var outputStream = new MemoryStream())
				{
					zipExtractor.ExtractZipStream(new MemoryStream(bytes), outputStream, "[Organization (UnitTest)]-[ACV]-ACVPublished.txt");
					AssertEquals(new byte[] { 1, 2, 3 }, outputStream.ToArray());
				}
			}

			//Two eDoc types selected
			grid.SelectCellValue(parent.PK, new List<ZGuid> { parent.PK }, "Organization (UnitTest)", "MSC", true);
			using (var stream = new MemoryStream())
			{
				var result = ZGridModuleEDocsHelper.TryGetZippedEDocs(grid, stream);
				Assert(result);
				var bytes = stream.ToArray();

				var zipExtractor = new ZipExtractor();
				var zippedFileNames = zipExtractor.GetFileNames(new MemoryStream(bytes));
				AssertContainsExactElementsInAnyOrder(new string[] { "[Organization (UnitTest)]-[ACV]-ACVPublished.txt", "[Organization (UnitTest)]-[MSC]-MSCPublished.xxx" }, zippedFileNames);

				using (var outputStream = new MemoryStream())
				{
					zipExtractor.ExtractZipStream(new MemoryStream(bytes), outputStream, "[Organization (UnitTest)]-[ACV]-ACVPublished.txt");
					AssertEquals(new byte[] { 1, 2, 3 }, outputStream.ToArray());

					outputStream.SetLength(0);
					zipExtractor.ExtractZipStream(new MemoryStream(bytes), outputStream, "[Organization (UnitTest)]-[MSC]-MSCPublished.xxx");
					AssertEquals(new byte[] { 4, 5, 6 }, outputStream.ToArray());
				}
			}
		}

		public void TestGetZippedEDocsCanIncludeRelatedDocs()
		{
			OrgHeaderWithIWebDocumentsSupport parentBizo = Factory.New<OrgHeaderWithIWebDocumentsSupport>();
			parentBizo.OH_Code = "Parent";
			_ = documentFactory.RetrieveExistingOrCreateStorageMainForPK(parentBizo.PK, Enterprise.Core.Constants.DocManagerCodes.Organisation);

			OrgHeaderWithIWebDocumentsSupport relatedBizo = Factory.New<OrgHeaderWithIWebDocumentsSupport>();
			relatedBizo.OH_Code = "Related";
			_ = documentFactory.RetrieveExistingOrCreateStorageMainForPK(relatedBizo.PK, Enterprise.Core.Constants.DocManagerCodes.Organisation);

			parentBizo.DocRelatedPKs_Exposed = new List<ZGuid> { relatedBizo.PK };
			documentFactory.Save();

			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_VarCharMax = "ABC";

			var docManInfoParent = ((IDocManagerSupport)parentBizo).DocManagerInfo;
			docManInfoParent.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ACVPublished.txt", "ACV").IsPublished = true;
			docManInfoParent.AddFileOrDocument(new byte[] { 4, 5, 6 }, "MSCPublished.xxx", "MSC").IsPublished = true;
			docManInfoParent.AddFileOrDocument(new byte[] { 7, 8, 9 }, "MSCUnpublished.yyy", "MSC").IsPublished = false;
			docManInfoParent.MasterFactory.Save();

			var docManInfoRelated = ((IDocManagerSupport)relatedBizo).DocManagerInfo;
			docManInfoRelated.AddFileOrDocument(new byte[] { 1, 2, 3 }, "RelatedACVPublished.txt", "ACV").IsPublished = true;
			docManInfoRelated.AddFileOrDocument(new byte[] { 4, 5, 6 }, "RelatedMSCPublished.xxx", "MSC").IsPublished = true;
			docManInfoRelated.AddFileOrDocument(new byte[] { 7, 8, 9 }, "RelatedMSCUnpublished.yyy", "MSC").IsPublished = false;
			docManInfoRelated.MasterFactory.Save();

			var grid = new ZDataGrid();
			grid.Page = new ZTestPage();
			grid.BindTo = "Collection";
			dummy.Collection.Add(parentBizo);
			grid.Bind(dummy);

			//One eDoc type selected
			grid.SelectCellValue(parentBizo.PK, new List<ZGuid> { parentBizo.PK, relatedBizo.PK }, "Organization (Parent)", "ACV", true);
			using (var stream = new MemoryStream())
			{
				var result = ZGridModuleEDocsHelper.TryGetZippedEDocs(grid, stream);
				Assert(result);
				var bytes = stream.ToArray();

				var zipExtractor = new ZipExtractor();
				var zippedFileNames = zipExtractor.GetFileNames(new MemoryStream(bytes));
				AssertContainsExactElementsInAnyOrder(new string[] { "[Organization (Parent)]-[ACV]-ACVPublished.txt", "[Organization (Parent)]-[ACV]-RelatedACVPublished.txt" }, zippedFileNames);

				using (var outputStream = new MemoryStream())
				{
					zipExtractor.ExtractZipStream(new MemoryStream(bytes), outputStream, "[Organization (Parent)]-[ACV]-ACVPublished.txt");
					AssertEquals(new byte[] { 1, 2, 3 }, outputStream.ToArray());

					outputStream.SetLength(0);
					zipExtractor.ExtractZipStream(new MemoryStream(bytes), outputStream, "[Organization (Parent)]-[ACV]-RelatedACVPublished.txt");
					AssertEquals(new byte[] { 1, 2, 3 }, outputStream.ToArray());
				}
			}

			//Two eDoc types selected
			grid.SelectCellValue(parentBizo.PK, new List<ZGuid> { parentBizo.PK, relatedBizo.PK }, "Organization (Parent)", "MSC", true);
			using (var stream = new MemoryStream())
			{
				var result = ZGridModuleEDocsHelper.TryGetZippedEDocs(grid, stream);
				Assert(result);
				var bytes = stream.ToArray();

				var zipExtractor = new ZipExtractor();
				var zippedFileNames = zipExtractor.GetFileNames(new MemoryStream(bytes));
				AssertContainsExactElementsInAnyOrder(new string[] { "[Organization (Parent)]-[ACV]-ACVPublished.txt", "[Organization (Parent)]-[MSC]-MSCPublished.xxx", "[Organization (Parent)]-[ACV]-RelatedACVPublished.txt", "[Organization (Parent)]-[MSC]-RelatedMSCPublished.xxx" }, zippedFileNames);

				using (var outputStream = new MemoryStream())
				{
					zipExtractor.ExtractZipStream(new MemoryStream(bytes), outputStream, "[Organization (Parent)]-[ACV]-ACVPublished.txt");
					AssertEquals(new byte[] { 1, 2, 3 }, outputStream.ToArray());

					outputStream.SetLength(0);
					zipExtractor.ExtractZipStream(new MemoryStream(bytes), outputStream, "[Organization (Parent)]-[MSC]-MSCPublished.xxx");
					AssertEquals(new byte[] { 4, 5, 6 }, outputStream.ToArray());

					outputStream.SetLength(0);
					zipExtractor.ExtractZipStream(new MemoryStream(bytes), outputStream, "[Organization (Parent)]-[ACV]-RelatedACVPublished.txt");
					AssertEquals(new byte[] { 1, 2, 3 }, outputStream.ToArray());

					outputStream.SetLength(0);
					zipExtractor.ExtractZipStream(new MemoryStream(bytes), outputStream, "[Organization (Parent)]-[MSC]-RelatedMSCPublished.xxx");
					AssertEquals(new byte[] { 4, 5, 6 }, outputStream.ToArray());
				}
			}
		}

		public void TestNoSelectedEDocs()
		{
			var parentBizo = Factory.New<OrgHeaderWithIWebDocumentsSupport>();
			parentBizo.OH_Code = "Parent";
			_ = documentFactory.RetrieveExistingOrCreateStorageMainForPK(parentBizo.PK, Enterprise.Core.Constants.DocManagerCodes.Organisation);

			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_VarCharMax = "ABC";

			Factory.Save();

			var grid = new ZDataGrid
			{
				Page = new ZTestPage(),
				BindTo = "Collection"
			};
			dummy.Collection.Add(parentBizo);
			grid.Bind(dummy);

			using (var stream = new MemoryStream())
			{
				var result = ZGridModuleEDocsHelper.TryGetZippedEDocs(grid, stream);

				Assert(!result);
			}
		}

		public void TestEDocsLongFileName()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_VarCharMax = "ABC";

			var docManagerInfo = ((IDocManagerSupport)parent).DocManagerInfo;

			var longFileName = new string('a', StorageDocsSchema.SC_FileName.MaxLength - 4) + ".txt";
			docManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, longFileName, "ACV").IsPublished = true;
			docManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, longFileName, "ACV").IsPublished = true;
			docManagerInfo.MasterFactory.Save();

			var grid = new ZDataGrid();
			grid.Page = new ZTestPage();
			grid.BindTo = "Collection";
			dummy.Collection.Add(parent);
			grid.Bind(dummy);

			grid.SelectCellValue(parent.PK, new List<ZGuid> { parent.PK }, "Organization (UnitTest)", "ACV", true);
			using (var stream = new MemoryStream())
			{
				var result = ZGridModuleEDocsHelper.TryGetZippedEDocs(grid, stream);
				Assert(result);
				var bytes = stream.ToArray();

				var zipExtractor = new ZipExtractor();
				var zippedFileNames = zipExtractor.GetFileNames(new MemoryStream(bytes));

				Assert(zippedFileNames.Any(x => x.Contains("a.txt")));
				Assert(zippedFileNames.Any(x => x.Contains("a[2].txt")));
			}
		}

		#region implementation

		protected override void SetUp()
		{
			base.SetUp();

			parent = Factory.New<OrgHeader>();
			parent.OH_Code = "UnitTest";

			documentFactory = ObjectFactory.Get<IDocumentFactoryProvider>().GetFactory(Factory);
			_ = documentFactory.RetrieveExistingOrCreateStorageMainForPK(parent.PK, Enterprise.Core.Constants.DocManagerCodes.Organisation);

			documentFactory.Save();
		}

		IDocumentFactory documentFactory;
		OrgHeader parent;

		#endregion
	}
}
