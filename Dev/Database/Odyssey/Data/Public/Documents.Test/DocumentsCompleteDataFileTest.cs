using System;
using System.Linq;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class DocumentsCompleteDataFileTest : DocumentsDataFileTest
	{
		public void TestDataFilePath()
		{
			var dataFile = new DocumentsCompleteDataFile();
			Assert(dataFile.FileFullPath.EndsWith("Enterprise\\Product\\Documents\\ExcelTemplates\\Dbupgrader.Data\\Documents\\DocumentsComplete.xml"));
		}

		protected override DocumentsDataFile GetDocumentsDataFile()
		{
			return new DocumentsCompleteDataFile();
		}

		protected override string TemplateBolbExpected()
		{
			return "System.Byte[]";
		}

		public void TestVersionNumber()
		{
			DocumentsDataFile dataFile = GetDocumentsDataFile();
			AssertEquals(DataProxy.DocumentsVersion.VersionNumber, dataFile.Version);
		}

		public void TestAllStmTemplateColumnsAreIncluded()
		{
			var dataFile = new DocumentsCompleteDataFile();
			var expectedSelectColumnList = StmTemplateSchema.All.Cast<SchemaColumn>().Select(c => c.Name).ToArray();
			var actualSelectColumnList = dataFile.InternalStmTemplateSelectList.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

			int count = expectedSelectColumnList.Length;

			AssertEquals(count, expectedSelectColumnList.Intersect(actualSelectColumnList).Count());
		}
	}
}
