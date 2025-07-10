using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes.Testing
{
	sealed class GLMappingReportSetupCSVFlatFileConverterTest : TestCaseWithFactory
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport()
		{
			Xsd.GLHeaderMultiLanguageReportSetupsGLHeaderMultiLanguageReportSetupCollection valueObject = new Xsd.GLHeaderMultiLanguageReportSetupsGLHeaderMultiLanguageReportSetupCollection();
			GLMappingReportSetupCSVFlatFileConverter converter = new GLMappingReportSetupCSVFlatFileConverter(null, Factory);
			using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLHeadersAndChargeCodes\GLAccountDescriptor\Testing\ValidReportSetup.csv"))
			{
				converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals(2, valueObject.Count);
			AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, valueObject[0].Language);
			AssertEquals("CN", valueObject[0].Country);
			AssertEquals("1001", valueObject[0].LocalAccountNumber);
			AssertEquals("BSH", valueObject[0].ReportType);
			AssertEquals("A01", valueObject[0].ReportCategory);
			AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, valueObject[1].Language);
			AssertEquals("CN", valueObject[1].Country);
			AssertEquals("1001.001", valueObject[1].LocalAccountNumber);
			AssertEquals("BSH", valueObject[1].ReportType);
			AssertEquals("A02", valueObject[1].ReportCategory);
		}
	}
}
