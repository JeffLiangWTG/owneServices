using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes.Testing
{
	sealed class GLAccountDescriptorCSVFlatFileConverterTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport()
		{
			Xsd.GLHeaderMultiLanguageMappingsGLHeaderMultiLanguageMappingCollection valueObject = new Xsd.GLHeaderMultiLanguageMappingsGLHeaderMultiLanguageMappingCollection();
			GLAccountDescriptorCSVFlatFileConverter converter = new GLAccountDescriptorCSVFlatFileConverter(null, Factory);
			using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLHeadersAndChargeCodes\GLAccountDescriptor\Testing\ValidTestData.csv"))
			{
				converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals(1, valueObject.Count);
			AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, valueObject[0].Language);
			AssertEquals("5101.000", valueObject[0].LocalAccountNumber.ToString());
			AssertEquals("CR", valueObject[0].DebitCredit.ToString());
			AssertEquals("主营业务收入", valueObject[0].Description.ToString());
			AssertEquals("P&L", valueObject[0].ReportCategory.ToString());
			AssertEquals("8370.00.00", valueObject[0].PercentNum.ToString());
			AssertEquals("1040.10.00", valueObject[0].ConsolidationNum.ToString());
			AssertEquals("3150.00.00", valueObject[0].AlternativeNum.ToString());
			AssertEquals("9799.00.00", valueObject[0].HeaderDependsOnTotal.ToString());
			AssertEquals("9999.00.00", valueObject[0].CarriedForwardAccount.ToString());
			AssertEquals(new ZInt(10), valueObject[0].TotalLevel);
			AssertEquals(new ZInt(20), valueObject[0].PrintSequence);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_MoreThanOneGLHeaders()
		{
			Xsd.GLHeaderMultiLanguageMappingsGLHeaderMultiLanguageMappingCollection valueObject = new Xsd.GLHeaderMultiLanguageMappingsGLHeaderMultiLanguageMappingCollection();
			NotificationBuffer notificationBuffer = new NotificationBuffer();
			GLAccountDescriptorCSVFlatFileConverter converter = new GLAccountDescriptorCSVFlatFileConverter(notificationBuffer, Factory);

			AssertEquals(false, notificationBuffer.HasErrors);

			using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLHeadersAndChargeCodes\GLAccountDescriptor\Testing\ValidGLAccDescriptor.csv"))
			{
				converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals(false, notificationBuffer.HasErrors);
			AssertEquals(2, valueObject.Count);
			AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, valueObject[0].Language);
			AssertEquals("5101.000", valueObject[0].LocalAccountNumber.ToString());
			AssertEquals("CR", valueObject[0].DebitCredit.ToString());
			AssertEquals("主营业务收入", valueObject[0].Description.ToString());
			AssertEquals("P&L", valueObject[0].ReportCategory.ToString());
			AssertEquals("8370.00.00", valueObject[0].PercentNum.ToString());
			AssertEquals("1040.10.00", valueObject[0].ConsolidationNum.ToString());
			AssertEquals("3150.00.00", valueObject[0].AlternativeNum.ToString());
			AssertEquals("9799.00.00", valueObject[0].HeaderDependsOnTotal.ToString());
			AssertEquals("9999.00.00", valueObject[0].CarriedForwardAccount.ToString());
			AssertEquals(new ZInt(10), valueObject[0].TotalLevel);
			AssertEquals(new ZInt(20), valueObject[0].PrintSequence);

			AssertEquals("1001.000", valueObject[1].LocalAccountNumber.ToString());
			AssertEquals("DR", valueObject[1].DebitCredit.ToString());
			AssertEquals("现金", valueObject[1].Description.ToString());
			AssertEquals("BSH", valueObject[1].ReportCategory.ToString());
			AssertEquals("7370.00.00", valueObject[1].PercentNum.ToString());
			AssertEquals("2040.10.00", valueObject[1].ConsolidationNum.ToString());
			AssertEquals("4150.00.00", valueObject[1].AlternativeNum.ToString());
			AssertEquals("8799.00.00", valueObject[1].HeaderDependsOnTotal.ToString());
			AssertEquals("8999.00.00", valueObject[1].CarriedForwardAccount.ToString());
			AssertEquals(new ZInt(30), valueObject[1].TotalLevel);
			AssertEquals(new ZInt(40), valueObject[1].PrintSequence);
			AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, valueObject[1].Language);
		}
	}
}
