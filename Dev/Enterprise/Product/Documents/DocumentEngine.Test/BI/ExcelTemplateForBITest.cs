using System.IO;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	sealed class ExcelTemplateForBITest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFilePath()
		{
			var testTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", TestFilesSubFolder.ReportTestFiles);
			AssertEquals((UnitTestingConstants.TestFilesDir + "NewStyleTemplate.xls").ToUpper(), Path.Combine(BaseSourcePath, testTemplate.TemplateSourceLocation).ToUpper());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestToByteArray()
		{
			var testTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", TestFilesSubFolder.ReportTestFiles);
			using (var xlsReader = testTemplate.GetAsTemplateStream())
			{
				AssertEquals(xlsReader.Length, testTemplate.GetAsByteArray().Length);
			}
		}
	}
}
