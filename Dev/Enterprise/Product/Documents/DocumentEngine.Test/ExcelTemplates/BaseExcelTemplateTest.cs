using System.IO;
using NUnit.Framework;

namespace Enterprise.ExcelTemplates.Testing
{
	abstract class BaseExcelTemplateTest : TestCase
	{
		public abstract void TestTemplateNameAndTemplateSourceLocation();
		public abstract void TestGetAsStreamVsGetAsByteArray();

		protected void AssertStreamAndByteArrayExpectedLength(ExcelTemplate excelTemplate, int expectedLength)
		{
			AssertEquals("GetAsByteArray().Length", expectedLength, excelTemplate.GetAsByteArray().Length);
			using (Stream templateAsStream = excelTemplate.GetAsTemplateStream())
			{
				AssertEquals("GetAsStream().Length", expectedLength, (int)templateAsStream.Length);
			}
		}
	}
}
