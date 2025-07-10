using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	class ExportExcelForSingleWindowHelperTesting : TestCaseWithFactory
	{
		public void TestExport()
		{
			var items = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory);
			var entryHeader1 = items.EntryHeader;
			var entryHeader2 = items.JobDeclaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			var exporter = new ExportExcelForSingleWindowHelper(new[] { entryHeader1, entryHeader2 });
			var message = exporter.ExportAndSaveAsEDocs();
			AssertEquals($"Excel for {entryHeader1.LocalReferenceNumber} has been saved to eDocs.\r\nExcel for {entryHeader2.LocalReferenceNumber} has been saved to eDocs.", message);
			var storageMain1 = ((IDocManagerSupport)entryHeader1).DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(entryHeader1, Core.Constants.DocManagerCodes.CustomsEntry);
			AssertEquals(1, storageMain1.Files.Count);
			using (var doc1 = storageMain1.Files[0].GetImageDataReader())
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(doc1);
				var xlsFile = excelInterface.Xls;
				for (var i = 1; i < xlsFile.SheetCount; i++)
				{
					xlsFile.ActiveSheet = i;
					AssertEquals($"The first columns should have been unhidden, sheet: {i}", false, xlsFile.GetColHidden(1));
				}
			}
		}
	}
}
