using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;

namespace Enterprise.Client.OIA.Business.Testing
{
	public class OIAGLFlatFileFormatTest : CsvFlatFileFormatTest
	{
		public void TestGetClientSpecificFileExtension()
		{
			AssertEquals("OIA GL File Extension", FileExtensionType.Csv, Format.FileExtensionForExport);
		}

		public void TestIncludeQuotesWhenExporting()
		{
			Assert("Include Quotes when exporting", Format.IncludeQuotesWhenExporting);
		}

		OIAGLFlatFileFormatTestClass Format
		{
			get
			{
				return format ?? (format = new OIAGLFlatFileFormatTestClass());
			}
		}

		OIAGLFlatFileFormatTestClass format;
		class OIAGLFlatFileFormatTestClass : OIAGLFlatFileFormat
		{
			public new string GetClientSpecificFileExtension()
			{
				return base.GetClientSpecificFileExtension();
			}

			public new bool IncludeQuotesWhenExporting
			{
				get
				{
					return base.IncludeQuotesWhenExporting;
				}
			}
		}
	}
}
