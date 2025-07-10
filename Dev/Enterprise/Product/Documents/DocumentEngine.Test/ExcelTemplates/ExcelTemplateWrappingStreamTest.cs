using System.IO;
using System.Text;

namespace Enterprise.ExcelTemplates.Testing
{
	sealed class ExcelTemplateWrappingStreamTest : BaseExcelTemplateTest
	{
		public override void TestTemplateNameAndTemplateSourceLocation()
		{
			byte[] contents = Encoding.UTF8.GetBytes("F.U.");
			Stream stream = new MemoryStream(contents);
			ExcelTemplateWrappingStream template = new ExcelTemplateWrappingStream("Fred", stream);
			AssertEquals("template.TemplateName", "Fred", template.TemplateName);
			AssertEquals("template.TemplateSourceLocation", "ExcelTemplateFromStream", template.TemplateSourceLocation);
		}

		public override void TestGetAsStreamVsGetAsByteArray()
		{
			byte[] contents = Encoding.UTF8.GetBytes("F.U.");
			Stream stream = new MemoryStream(contents);
			ExcelTemplateWrappingStream template = new ExcelTemplateWrappingStream("Fred", stream);
			AssertStreamAndByteArrayExpectedLength(template, 4);
		}
	}
}
