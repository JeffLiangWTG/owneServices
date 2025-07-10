using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;

namespace Enterprise.Client.ELG.Testing
{
	public class SagAccountsFlatFileFormatTest : CsvFlatFileFormatTest
	{
		public void TestGetClientSpecificFileExtension()
		{
			AssertEquals("Sage File Extension", FileExtensionType.Csv, Format.FileExtensionForExport);
		}

		public void TestIncludeQuotesWhenExporting()
		{
			Assert("Include Quotes when exporting should be false", !Format.IncludeQuotesWhenExporting);
		}

		SagAccountsFlatFileFormatTestClass Format
		{
			get
			{
				return format ?? (format = new SagAccountsFlatFileFormatTestClass());
			}
		}

		SagAccountsFlatFileFormatTestClass format;
		public class SagAccountsFlatFileFormatTestClass : SagAccountsFlatFileFormat
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
