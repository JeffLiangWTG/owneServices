using System;
using System.IO;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.ExcelComparator.Testing
{
	sealed class ComparableTextGeneratorTest : TestCase
	{
		public void TestGeneralUsage()
		{
			string generatorTempFileName = null;
			var file1XlsPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.ExcelComparator.Testing.TestFiles.File1.xls", "File1.xls");
			using (var generator = new ComparableTextGenerator(file1XlsPath, 2))
			{
				AssertEquals("generator.SaveAsComparableTextTempFile()", true, generator.SaveAsComparableTextTempFile());
				AssertEquals("generator.TempFileName.Contains('ExcelComparator')", true, generator.TempFileName.Contains(@"\ExcelComparator\"));
				AssertEquals("File.Exists(generator.TempFileName)", true, File.Exists(generator.TempFileName));
				AssertEquals("generator.TempFileName.EndsWith('txt2') - NB: File 1 is 'txt1' so that there's no possible naming conflicts", true, generator.TempFileName.EndsWith("txt2"));

				var fileContents = File.ReadAllText(generator.TempFileName);
				AssertMultilineASCIIEquals("ComparisonContents", @"
Author: [Ben Govett]
PageHeader: []
PageFooter: []

=-------------------------------------------------------------------------------
WorkSheet No: [1]   (Is Visible)
Sheet Name:   [Sheet1]
=-------------------------------------------------------------------------------
{A}-[This would be the same]
{A}-[File 1]

=-------------------------------------------------------------------------------
WorkSheet No: [2]   (Is Visible)
Sheet Name:   [Sheet2]
=-------------------------------------------------------------------------------

=-------------------------------------------------------------------------------
WorkSheet No: [3]   (Is Visible)
Sheet Name:   [Sheet3]
=-------------------------------------------------------------------------------

".Trim(), fileContents.Trim());

				generatorTempFileName = generator.TempFileName;
			}

			AssertEquals("File.Exists(generator.TempFileName) after disposing", false, File.Exists(generatorTempFileName));
		}

		public void TestWithFileContaingZeroHeightRows()
		{
			string generatorTempFileName = null;
			var fileContainingRowsWithZeroHeightXlsPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.ExcelComparator.Testing.TestFiles.FileContainingRowsWithZeroHeight.xls", "FileContainingRowsWithZeroHeight.xls");
			using (var generator = new ComparableTextGenerator(fileContainingRowsWithZeroHeightXlsPath, 2))
			{
				AssertEquals("generator.SaveAsComparableTextTempFile()", true, generator.SaveAsComparableTextTempFile());
				AssertEquals("File.Exists(generator.TempFileName)", true, File.Exists(generator.TempFileName));

				var fileContents = File.ReadAllText(generator.TempFileName);
				AssertMultilineASCIIEquals("ComparisonContents", @"
Author: [andrew.luong]
PageHeader: []
PageFooter: []

=-------------------------------------------------------------------------------
WorkSheet No: [1]   (Is Visible)
Sheet Name:   [Sheet1]
=-------------------------------------------------------------------------------
{A}-[row 1]
{A}-[row 2]
{A}-[row with height 0]
{A}-[row 4]
".Trim(), fileContents.Trim());

				generatorTempFileName = generator.TempFileName;
			}

			AssertEquals("File.Exists(generator.TempFileName) after disposing", false, File.Exists(generatorTempFileName));
		}

		public void TestWithInvalidFileFormat()
		{
			var corruptedXlsPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.ExcelComparator.Testing.TestFiles.Corrupted.xls", "Corrupted.xls");
			using (var generator = new ComparableTextGenerator(corruptedXlsPath, 1))
			{
				AssertEquals("generator.SaveAsComparableTextTempFile()", false, generator.SaveAsComparableTextTempFile());
				AssertEquals("generator.TempFileName", null, generator.TempFileName);

				var expectedErr = string.Format(@"
An Error occurred reading [{0}]
""You can only load templates saved as 'Excel 97-2003 Workbook' (.xls) or 'Excel 2007 Workbook' (.xlsx) format.""
".Trim(), corruptedXlsPath);

				AssertMultilineASCIIEquals("generator.ErrorText", expectedErr, generator.ErrorText);
			}
		}

		public void TestWithNonExistantFile()
		{
			var nonExistentXlsPath = Path.Combine(TempForTest.TempPath, "NonExistant.xls");
			using (var generator = new ComparableTextGenerator(nonExistentXlsPath, 2))
			{
				AssertEquals("generator.SaveAsComparableTextTempFile()", false, generator.SaveAsComparableTextTempFile());
				AssertEquals("generator.TempFileName", null, generator.TempFileName);

				var expectedErr = string.Format(@"
An Error occurred reading [{0}]
""Could not find file '{0}'.""
".Trim(), nonExistentXlsPath);

				AssertMultilineASCIIEquals("generator.ErrorText", expectedErr, generator.ErrorText);
			}
		}

		public void TestWithNullFile()
		{
			using (var generator = new ComparableTextGenerator(null, 2))
			{
				AssertEquals("generator.SaveAsComparableTextTempFile()", false, generator.SaveAsComparableTextTempFile());
				AssertEquals("generator.TempFileName", null, generator.TempFileName);

#if NET
				AssertMultilineASCIIEquals("generator.ErrorText", "An Error occurred reading []\r\n\"The value cannot be an empty string. (Parameter 'path')\"".Trim(), generator.ErrorText);
#else
				AssertMultilineASCIIEquals("generator.ErrorText", "An Error occurred reading []\r\n\"Empty path name is not legal.\"".Trim(), generator.ErrorText);
#endif
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;
	}
}
