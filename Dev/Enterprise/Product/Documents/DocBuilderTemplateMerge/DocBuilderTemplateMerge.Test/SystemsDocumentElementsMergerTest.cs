using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.IO;
using FlexCel.Core;
using FlexCel.XlsAdapter;
using NUnit.Framework;

namespace Enterprise.DocBuilderTemplateMerge.Testing
{
	internal class SystemsDocumentElementsMergerTest : TestCase
	{
		public void TestCustomizedDocumentElementsFileName()
		{
			var testFileName = "Customized Document Elements For Shelf [bef98a98-96ac-4c25-8495-612498b8a245].xls";
			AssertEquals("CustomizedDocumentElementsFileNameRegex.IsMatch(testFileName)", true, SystemDocumentElementsMerger.CustomizedDocumentElementsFileNameRegex.IsMatch(testFileName));

			var newFileName = SystemDocumentElementsMerger.NewCustomizedDocumentElementsFileName();
			AssertEquals("CustomizedDocumentElementsFileNameRegex.IsMatch(newFileName)", true, SystemDocumentElementsMerger.CustomizedDocumentElementsFileNameRegex.IsMatch(newFileName));

			var newFileName2 = SystemDocumentElementsMerger.NewCustomizedDocumentElementsFileName();
			AssertEquals("CustomizedDocumentElementsFileNameRegex.IsMatch(newFileName)", true, SystemDocumentElementsMerger.CustomizedDocumentElementsFileNameRegex.IsMatch(newFileName));
			AssertEquals("newFileName.Equals(newFileName2)", false, newFileName.Equals(newFileName2));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocBuilderDocumentsDir()
		{
			var docBuilderDocumentsDir = TestFilePaths.DocBuilderDocumentsDir;
			AssertEquals("Directory.Exists(docBuilderDocumentsDir)", true, Directory.Exists(docBuilderDocumentsDir));

			var systemDocumentElementsFile = Path.Combine(docBuilderDocumentsDir, SystemDocumentElementsMerger.SystemDocumentElementsFileName);
			AssertEquals("File.Exists(systemDocumentElementsFile)", true, File.Exists(systemDocumentElementsFile));
		}

		public void TestMergeValidCustomizedDocument()
		{
			var systemDocumentFilePath = TestFilePaths.SystemDocument;
			var systemDocument = File.ReadAllBytes(systemDocumentFilePath);
			tempFilePaths.Add(systemDocumentFilePath);

			DateTime originalCreationDate;
			DateTime originalModificationDate;
			DateTime newCreationDate;
			DateTime newModificationDate;

			using (var excelFileStream = new MemoryStream(systemDocument))
			{
				var excelFile = new XlsFile();
				excelFile.Open(excelFileStream);
				originalCreationDate = (DateTime)excelFile.DocumentProperties.GetStandardProperty(TPropertyId.CreateTimeDate);
				originalModificationDate = (DateTime)excelFile.DocumentProperties.GetStandardProperty(TPropertyId.LastSavedTimeDate);
			}

			var customisedDocumentModifyFilePath = TestFilePaths.CustomizedDocumentModify;
			var customizedDocumentModify = File.ReadAllBytes(customisedDocumentModifyFilePath);
			tempFilePaths.Add(customisedDocumentModifyFilePath);

			var documentMergerModify = new SystemDocumentElementsMerger(systemDocument);
			documentMergerModify.Merge(customizedDocumentModify);
			AssertEquals("ExcelFileToString(documentMergerModify.systemDocumentAsByteArray)", ExcelFileToString(File.ReadAllBytes(TestFilePaths.SystemDocumentModified)), ExcelFileToString(documentMergerModify.systemDocumentAsByteArray));

			var customizedDocumentInsertFilePath = TestFilePaths.CustomizedDocumentInsert;
			var customizedDocumentInsert = File.ReadAllBytes(customizedDocumentInsertFilePath);
			tempFilePaths.Add(customizedDocumentInsertFilePath);

			var documentMergerInsert = new SystemDocumentElementsMerger(systemDocument);
			documentMergerInsert.Merge(customizedDocumentInsert);
			AssertEquals("ExcelFileToString(documentMergerInsert.systemDocumentAsByteArray)", ExcelFileToString(File.ReadAllBytes(TestFilePaths.SystemDocumentInserted)), ExcelFileToString(documentMergerInsert.systemDocumentAsByteArray));

			var customizedDocumentInsertAndModifyFilePath = TestFilePaths.CustomizedDocumentInsertAndModify;
			var customizedDocumentInsertAndModify = File.ReadAllBytes(customizedDocumentInsertAndModifyFilePath);
			tempFilePaths.Add(customizedDocumentInsertAndModifyFilePath);

			var documentMergerInsertAndModify = new SystemDocumentElementsMerger(systemDocument);
			documentMergerInsertAndModify.Merge(customizedDocumentInsertAndModify);
			AssertEquals("ExcelFileToString(documentMergerInsertAndModify.systemDocumentAsByteArray)", ExcelFileToString(File.ReadAllBytes(TestFilePaths.SystemDocumentInsertedAndModified)), ExcelFileToString(documentMergerInsertAndModify.systemDocumentAsByteArray));

			using (var excelFileStream = new MemoryStream(documentMergerInsertAndModify.systemDocumentAsByteArray))
			{
				var excelFile = new XlsFile();
				excelFile.Open(excelFileStream);
				newCreationDate = (DateTime)excelFile.DocumentProperties.GetStandardProperty(TPropertyId.CreateTimeDate);
				newModificationDate = (DateTime)excelFile.DocumentProperties.GetStandardProperty(TPropertyId.LastSavedTimeDate);
			}

			AssertEquals(originalCreationDate, newCreationDate);
			AssertEquals(originalModificationDate, newModificationDate);
			TearDown();
		}

		public void TestMergeInvalidCutomizedDocument()
		{
			var systemDocumentFilePath = TestFilePaths.SystemDocument;
			var systemDocument = File.ReadAllBytes(systemDocumentFilePath);
			tempFilePaths.Add(systemDocumentFilePath);

			var invalidCustomizedDocumentFilePath = TestFilePaths.InvalidCustomizedDocument;
			var invalidCustomizedDocument = File.ReadAllBytes(invalidCustomizedDocumentFilePath);
			tempFilePaths.Add(invalidCustomizedDocumentFilePath);

			var documentMergerInvalid = new SystemDocumentElementsMerger(systemDocument);
			documentMergerInvalid.Merge(invalidCustomizedDocument);
			AssertEquals("system document should not change", ExcelFileToString(File.ReadAllBytes(systemDocumentFilePath)), ExcelFileToString(documentMergerInvalid.systemDocumentAsByteArray));
			TearDown();
		}

		public void TestMergeInvalidCustomizedDocumentInvalidRGBColor()
		{
			var systemDocumentFilePath = TestFilePaths.SystemDocument;
			var systemDocument = File.ReadAllBytes(systemDocumentFilePath);
			tempFilePaths.Add(systemDocumentFilePath);

			var invalidCustomizedDocumentFilePath = TestFilePaths.InvalidCustomizedDocumentInvalidRGBColor;
			var invalidCustomizedDocument = File.ReadAllBytes(invalidCustomizedDocumentFilePath);
			tempFilePaths.Add(invalidCustomizedDocumentFilePath);

			var documentMergerInvalid = new SystemDocumentElementsMerger(systemDocument);
			AssertExceptionThrown(typeof(InvalidOperationException), () => documentMergerInvalid.Merge(invalidCustomizedDocument));
			TearDown();
		}

		#region Implementation

		public static string ExcelFileToString(byte[] excelFileAsByteArray)
		{
			var result = new StringBuilder();

			using (var excelFileStream = new MemoryStream(excelFileAsByteArray))
			{
				var excelFile = new XlsFile();
				excelFile.Open(excelFileStream);
				for (int i = 1; i <= excelFile.SheetCount; i++)
				{
					excelFile.ActiveSheet = i;
					result.Append(string.Format("Sheet: [{0}]", excelFile.SheetName));
					for (int rowIndex = 1; rowIndex <= excelFile.RowCount; rowIndex++)
					{
						var rowResult = new StringBuilder();
						for (int columnIndex = 1; columnIndex <= excelFile.ColCount; columnIndex++)
						{
							var cellValue = excelFile.GetCellValue(rowIndex, columnIndex);
							string cellString = cellValue != null ? cellValue.ToString() : "";
							if (!string.IsNullOrEmpty(cellString))
							{
								rowResult.Append("{" + GetColumnTitle(columnIndex) + "}-[" + cellString + "]  ");
							}
						}
						result.Append(rowResult.ToString().Trim());
					}
				}
			}

			return result.ToString().Trim();
		}

		static string GetColumnTitle(int columnIndex)
		{
			int firstCharIndex = System.Convert.ToInt32(columnIndex / 26);
			int secondCharIndex = columnIndex % 26;
			if (firstCharIndex <= 26)
			{
				string secondChar = ColumnTitleCharList.Substring(secondCharIndex, 1);
				if (firstCharIndex == 0)
				{
					return secondChar;
				}
				return ColumnTitleCharList.Substring(firstCharIndex - 1, 1) + secondChar;
			}
			return columnIndex.ToString();
		}
		const string ColumnTitleCharList = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		class TestFilePaths
		{
			public static string SystemDocument
			{
				get { return GetTestPath("SystemDocument.xls"); }
			}

			public static string SystemDocumentInserted
			{
				get { return GetTestPath("SystemDocumentInserted.xls"); }
			}

			public static string SystemDocumentModified
			{
				get { return GetTestPath("SystemDocumentModified.xls"); }
			}

			public static string SystemDocumentInsertedAndModified
			{
				get { return GetTestPath("SystemDocumentInsertedAndModified.xls"); }
			}

			public static string CustomizedDocumentInsert
			{
				get { return GetTestPath("CustomizedDocumentInsert.xls"); }
			}

			public static string CustomizedDocumentModify
			{
				get { return GetTestPath("CustomizedDocumentModify.xls"); }
			}

			public static string CustomizedDocumentInsertAndModify
			{
				get { return GetTestPath("CustomizedDocumentInsertAndModify.xls"); }
			}

			public static string InvalidCustomizedDocument
			{
				get { return GetTestPath("InvalidCustomizedDocument.xls"); }
			}

			public static string InvalidCustomizedDocumentInvalidRGBColor
			{
				get { return GetTestPath("InvalidCustomizedDocumentInvalidRGBColor.xls"); }
			}

			public static string DocBuilderDocumentsDir
			{
				get { return Path.Combine(TestCase.BaseSourcePath, SystemDocumentElementsMerger.DocBuilderDocumentsDir); }
			}

			static string GetEmbeddedResourcePath(string filename) => "Enterprise.DocBuilderTemplateMerge.Testing." + filename;

			static string GetTestPath(string fileName)
			{
				var resourceRetriever = new EmbeddedResourceRetriever(typeof(SystemsDocumentElementsMergerTest).Assembly);
				var testDocumentPath = resourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath(fileName));
				return testDocumentPath;
			}

			public static void DeleteTestFile(string filePath)
			{
				if (!File.Exists(filePath)) { return; }
				var tempFolder = new DirectoryInfo(Path.GetDirectoryName(filePath));
				foreach (var subF in tempFolder.Parent.GetDirectories())
				{ subF.Delete(true); }
			}
		}

		#endregion

		#region Implementation

		List<string> tempFilePaths;
		protected override void SetUp()
		{
			base.SetUp();
			tempFilePaths = new List<string>();
		}
		protected override void TearDown()
		{
			foreach (var filePath in tempFilePaths)
			{
				TestFilePaths.DeleteTestFile(filePath);
			}
			base.TearDown();
		}

		#endregion
	}
}
