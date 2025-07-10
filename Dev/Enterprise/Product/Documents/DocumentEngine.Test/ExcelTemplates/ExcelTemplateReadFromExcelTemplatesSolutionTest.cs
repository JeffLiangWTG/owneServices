using System.IO;
using System.Reflection;

namespace Enterprise.ExcelTemplates.Testing
{
	sealed class ExcelTemplateReadFromExcelTemplatesSolutionTest : BaseExcelTemplateTest
	{
		public override void TestTemplateNameAndTemplateSourceLocation()
		{
			ExcelTemplate excelTemplate = new ExcelTemplateReadFromExcelTemplatesSolution(ExcelTemplateReadFromExcelTemplatesSolution.TemplateNames.BookingSummary);
			AssertEquals("excelTemplate.TemplateName", "BookingSummary", excelTemplate.TemplateName);
			AssertEquals("excelTemplate.TemplateSourceLocation", "ExcelTemplates.BookingSummary.xls", excelTemplate.TemplateSourceLocation);
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestGetAsStreamVsGetAsByteArray()
		{
			ExcelTemplate excelTemplate = new ExcelTemplateReadFromExcelTemplatesSolution(ExcelTemplateReadFromExcelTemplatesSolution.TemplateNames.BookingSummary);
			using (FileStream templateFromFile = File.OpenRead(BaseSourcePath + @"Enterprise\Product\Documents\ExcelTemplates\BookingSummary.xls"))
			{
				AssertStreamAndByteArrayExpectedLength(excelTemplate, (int)templateFromFile.Length);
			}
		}

		public void TestAllFilesAreEmbedded()
		{
			FieldInfo[] infos = typeof(ExcelTemplateReadFromExcelTemplatesSolution.TemplateNames).GetFields(BindingFlags.Public | BindingFlags.Static);
			int extractedFiles = 0;
			CombineAssertions(() =>
				{
					foreach (FieldInfo info in infos)
					{
						string fileName = (string)info.GetValue(null);
						ExcelTemplate template = new ExcelTemplateReadFromExcelTemplatesSolution(fileName);
						AssertNoExceptionThrown(fileName, () => template.GetAsByteArray());
						extractedFiles++;
					}
				});

			AssertEquals("File count", infos.Length, extractedFiles);
		}

		public void TestTemplateNameLength()
		{
			FieldInfo[] infos = typeof(ExcelTemplateReadFromExcelTemplatesSolution.TemplateNames).GetFields(BindingFlags.Static | BindingFlags.Public);
			foreach (FieldInfo field in infos)
			{
				Assert(field.Name + " should be less than 35 chars.", field.Name.Length <= 50);
				string name = (string)field.GetValue(null);
				Assert(name + " should be less than 35 chars.", name.Length <= 50);
			}
		}
	}
}
