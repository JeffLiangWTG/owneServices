using System.IO;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	static class SectionRepositoryTestHelper
	{
		public static byte[] GetSystemRepositoryTemplateBlob()
		{
			using (var excel = new ExcelInterface())
			{
				excel.NewExcelFile(1);
				var workSheet = excel.WorkSheets[0];

				workSheet[0, 0] = "#Config";
				workSheet[1, 0] = "#ConfigurableSection:AAA, AAA Section";
				workSheet[2, 1] = "AAA 1-1";
				workSheet[2, 2] = "AAA 1-2";
				workSheet[3, 1] = "AAA 2-1";
				workSheet[4, 0] = "#ConfigurableSection:BBB, BBB Section";
				workSheet[5, 1] = "BBB 1-1";
				workSheet[5, 2] = "BBB 1-2";
				workSheet[6, 0] = "#EndOfReport";

				using (var stream = new MemoryStream())
				{
					excel.SaveToStream(stream);
					return stream.ToArray();
				}
			}
		}

		public static byte[] GetUserRepositoryTemplateBlob()
		{
			return GetConfigurableTemplateBlob("ZZZ 1-1");
		}

		public static byte[] GetConfigurableTemplateBlob(string aaaSectionContents)
		{
			using (var excel = new ExcelInterface())
			{
				excel.NewExcelFile(1);
				var workSheet = excel.WorkSheets[0];

				workSheet[0, 0] = "#Config";
				workSheet[1, 0] = "#ConfigurableSection:aaa, aaa Section";
				workSheet[2, 1] = aaaSectionContents;
				workSheet[3, 0] = "#ConfigurableSection:CCC, CCC Section";
				workSheet[4, 1] = "CCC 1-1";
				workSheet[5, 2] = "CCC 2-2";
				workSheet[6, 0] = "#EndOfReport";

				using (var stream = new MemoryStream())
				{
					excel.SaveToStream(stream);
					return stream.ToArray();
				}
			}
		}
	}
}
