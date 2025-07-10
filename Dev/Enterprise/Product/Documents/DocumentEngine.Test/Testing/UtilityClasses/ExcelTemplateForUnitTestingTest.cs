using System.IO;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	sealed class ExcelTemplateForUnitTestingTest : TestCase
	{
		public void TestFilePath()
		{
			var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls", "NewStyleTemplate.xls");
			var excelTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", Path.GetFullPath(tempFileName));
			AssertEquals(tempFileName.ToUpper(), excelTemplate.TemplateSourceLocation.ToUpper());
		}

		public void TestToByteArray()
		{
			var templateStream = embeddedResourceRetriever.GetStream("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls");
			var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls", "NewStyleTemplate.xls");
			var excelTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", Path.GetFullPath(tempFileName));
			AssertEquals(templateStream.Length, excelTemplate.GetAsByteArray().Length);
		}

		EmbeddedResourceRetriever embeddedResourceRetriever;

		protected override void SetUp()
		{
			base.SetUp();
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
		}

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever.Dispose();
		}
	}
}
