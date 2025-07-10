using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes.Testing
{
	sealed class GLHeaderAndChargeCodesCSVFlatFileConverterTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportIfThereIsTheBlankRowInFile()
		{
			Xsd.GLHeadersAndChargeCodes valueObject = new Xsd.GLHeadersAndChargeCodes();
			NotificationBuffer notificationBuffer = new NotificationBuffer();
			GLHeaderAndChargeCodesCSVFlatFileConverter converter = new GLHeaderAndChargeCodesCSVFlatFileConverter(notificationBuffer, Factory);
			try
			{
				using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLHeadersAndChargeCodes\Testing\Valid2ChargeCodesAndGLHeadersAndBlankLine.csv"))
				{
					converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);
				}
			}
			catch
			{
				Fail("Should be no errors. The empty string in the file have to be handled correctly.");
			}

			AssertEquals(false, notificationBuffer.HasErrors);

			AssertEquals(2, valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders.Count);
			AssertEquals(2, valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes.Count);

			AssertEquals("Destination Quarantine Inspection", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].Description.ToString());
			AssertEquals("OBILL", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].Code.ToString());
			AssertEquals("FEA, FIA, FES, FIS, CEA, CIA, CES, CIS", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].DepartmentFilterList.ToString());
			AssertEquals("MRG", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].ChargeType.ToString());
			AssertEquals("100", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].MarginPercentage.ToString());
			AssertEquals("FREEGST", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].GSTRate.ToString());
			AssertEquals("TEST_CODE", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].WithholdingTaxRate.ToString());

			AssertEquals("CARTAGE", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].SalesGroup.ToString());
			AssertEquals("CARTAGE", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].ExpenseGroup.ToString());
			AssertEquals("8999.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].RevenueAccount.ToString());
			AssertEquals("1080.20.10", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].WIPAccount.ToString());
			AssertEquals("1090.10.20", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].CostAccount.ToString());
			AssertEquals("2020.10.00", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].AccrualAccount.ToString());
			AssertEquals("BRK", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].ChargeGroup.ToString());
			AssertEquals("Y", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].IsGroupageCharge.ToString());
			AssertEquals("BRK", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].SubGroup.ToString());
			AssertEquals("FLT", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].RateCalculator.ToString());

			AssertEquals("N", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].ShowOnQuotation.ToString());
			AssertEquals("N", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].SuppressOnQuoteIfZero.ToString());
			AssertEquals("XX", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].IATA_ChargeCodeMap.ToString());

			AssertEquals("Destination Quarantine Department", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].Description.ToString());
			AssertEquals("OEHC", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].Code.ToString());
			AssertEquals("FES, FIS, CEA, CIA, CES, CIS", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].DepartmentFilterList.ToString());
			AssertEquals("MRG", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].ChargeType.ToString());
			AssertEquals("50.55", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].MarginPercentage.ToString());
			AssertEquals("FREEGST", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].GSTRate.ToString());
			AssertEquals("TEST_CODE", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].WithholdingTaxRate.ToString());

			AssertEquals("CARTAGE", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].SalesGroup.ToString());
			AssertEquals("CARTAGE", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].ExpenseGroup.ToString());
			AssertEquals("5511.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].RevenueAccount.ToString());
			AssertEquals("1093.25.10", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].WIPAccount.ToString());
			AssertEquals("9498.10.98", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].CostAccount.ToString());
			AssertEquals("9849.30.00", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].AccrualAccount.ToString());
			AssertEquals("BRK", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].ChargeGroup.ToString());
			AssertEquals("N", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].IsGroupageCharge.ToString());
			AssertEquals("BRK", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].SubGroup.ToString());
			AssertEquals("FLT", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].RateCalculator.ToString());

			AssertEquals("Y", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].ShowOnQuotation.ToString());
			AssertEquals("Y", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].SuppressOnQuoteIfZero.ToString());
			AssertEquals("TT", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].IATA_ChargeCodeMap.ToString());

			AssertEquals("4710.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[0].AccNumber.ToString());
			AssertEquals("CR", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[0].DebitCredit.ToString());
			AssertEquals("EXCHANGE DIFFERENCES", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[0].Description.ToString());
			AssertEquals("PL", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[0].AccountType.ToString());
			AssertEquals("8370.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[0].PercentNum.ToString());
			AssertEquals("1040.10.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[0].ConsolidationNum.ToString());

			AssertEquals("3150.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[0].AlternateNum.ToString());
			AssertEquals("9799.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[0].HeaderDependsOnTotal.ToString());
			AssertEquals(new ZInt(10), valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[0].TotalLevel);
			AssertEquals("Y", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[0].ControlAccount.ToString());
			AssertEquals("N", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[0].DisallowDirectPosting.ToString());
			AssertEquals(new ZInt(10), valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[0].PrintSequence);

			AssertEquals("4110.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[1].AccNumber.ToString());
			AssertEquals("DR", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[1].DebitCredit.ToString());
			AssertEquals("PORT & TERMINAL REVENUE ACTUAL", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[1].Description.ToString());
			AssertEquals("PL", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[1].AccountType.ToString());
			AssertEquals("1370.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[1].PercentNum.ToString());
			AssertEquals("1050.99.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[1].ConsolidationNum.ToString());

			AssertEquals("3190.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[1].AlternateNum.ToString());
			AssertEquals("1339.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[1].HeaderDependsOnTotal.ToString());
			AssertEquals(new ZInt(15), valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[1].TotalLevel);
			AssertEquals("N", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[1].ControlAccount.ToString());
			AssertEquals("Y", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[1].DisallowDirectPosting.ToString());
			AssertEquals(new ZInt(15), valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[1].PrintSequence);

			AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[0].Language);
			AssertEquals("5101.000", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[0].LocalAccountNumber.ToString());
			AssertEquals("CR", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[0].DebitCredit.ToString());
			AssertEquals("主营业务收入", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[0].Description.ToString());
			AssertEquals("P&L", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[0].ReportCategory.ToString());
			AssertEquals("8370.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[0].PercentNum.ToString());
			AssertEquals("1040.10.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[0].ConsolidationNum.ToString());
			AssertEquals("3150.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[0].AlternativeNum.ToString());
			AssertEquals("9799.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[0].HeaderDependsOnTotal.ToString());
			AssertEquals("9999.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[0].CarriedForwardAccount.ToString());
			AssertEquals(new ZInt(10), valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[0].TotalLevel);
			AssertEquals(new ZInt(20), valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[0].PrintSequence);

			AssertEquals("1001.000", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[1].LocalAccountNumber.ToString());
			AssertEquals("DR", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[1].DebitCredit.ToString());
			AssertEquals("现金", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[1].Description.ToString());
			AssertEquals("BSH", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[1].ReportCategory.ToString());
			AssertEquals("7370.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[1].PercentNum.ToString());
			AssertEquals("2040.10.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[1].ConsolidationNum.ToString());
			AssertEquals("4150.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[1].AlternativeNum.ToString());
			AssertEquals("8799.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[1].HeaderDependsOnTotal.ToString());
			AssertEquals("8999.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[1].CarriedForwardAccount.ToString());
			AssertEquals(new ZInt(30), valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[1].TotalLevel);
			AssertEquals(new ZInt(40), valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[1].PrintSequence);
			AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[1].Language);

			AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageReportSetups[0].Language);
			AssertEquals("1001", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageReportSetups[0].LocalAccountNumber);
			AssertEquals("BSH", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageReportSetups[0].ReportType);
			AssertEquals("A01", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageReportSetups[0].ReportCategory);

			AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageReportSetups[1].Language);
			AssertEquals("1001.001", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageReportSetups[1].LocalAccountNumber);
			AssertEquals("BSH", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageReportSetups[1].ReportType);
			AssertEquals("A02", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageReportSetups[1].ReportCategory);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_MoreThanOneChargeCodeAndGLHeader()
		{
			Xsd.GLHeadersAndChargeCodes valueObject = new Xsd.GLHeadersAndChargeCodes();
			NotificationBuffer notificationBuffer = new NotificationBuffer();
			GLHeaderAndChargeCodesCSVFlatFileConverter converter = new GLHeaderAndChargeCodesCSVFlatFileConverter(notificationBuffer, Factory);

			AssertEquals(false, notificationBuffer.HasErrors);

			using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLHeadersAndChargeCodes\Testing\Valid2ChargeCodesAndGLHeadersTestData.csv"))
			{
				converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals(false, notificationBuffer.HasErrors);

			AssertEquals(2, valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders.Count);
			AssertEquals(3, valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes.Count);

			AssertEquals("Destination Quarantine Inspection", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].Description.ToString());
			AssertEquals("OBILL", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].Code.ToString());
			AssertEquals("FEA, FIA, FES, FIS, CEA, CIA, CES, CIS", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].DepartmentFilterList.ToString());
			AssertEquals("MRG", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].ChargeType.ToString());
			AssertEquals("100", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].MarginPercentage.ToString());
			AssertEquals("FREEGST", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].GSTRate.ToString());
			AssertEquals("TEST_CODE", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].WithholdingTaxRate.ToString());

			AssertEquals("CARTAGE", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].SalesGroup.ToString());
			AssertEquals("CARTAGE", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].ExpenseGroup.ToString());
			AssertEquals("8999.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].RevenueAccount.ToString());
			AssertEquals("1080.20.10", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].WIPAccount.ToString());
			AssertEquals("1090.10.20", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].CostAccount.ToString());
			AssertEquals("2020.10.00", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].AccrualAccount.ToString());
			AssertEquals("BRK", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].ChargeGroup.ToString());
			AssertEquals("Y", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].IsGroupageCharge.ToString());
			AssertEquals("BRK", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].SubGroup.ToString());
			AssertEquals("FLT", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].RateCalculator.ToString());

			AssertEquals("N", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].ShowOnQuotation.ToString());
			AssertEquals("N", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].SuppressOnQuoteIfZero.ToString());
			AssertEquals("XX", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[0].IATA_ChargeCodeMap.ToString());

			AssertEquals("Destination Quarantine Department", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].Description.ToString());
			AssertEquals("OEHC", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].Code.ToString());
			AssertEquals("FES, FIS, CEA, CIA, CES, CIS", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].DepartmentFilterList.ToString());
			AssertEquals("MRG", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].ChargeType.ToString());
			AssertEquals("50.55", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].MarginPercentage.ToString());
			AssertEquals("FREEGST", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].GSTRate.ToString());
			AssertEquals("TEST_CODE", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].WithholdingTaxRate.ToString());

			AssertEquals("CARTAGE", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].SalesGroup.ToString());
			AssertEquals("CARTAGE", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].ExpenseGroup.ToString());
			AssertEquals("5511.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].RevenueAccount.ToString());
			AssertEquals("1093.25.10", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].WIPAccount.ToString());
			AssertEquals("9498.10.98", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].CostAccount.ToString());
			AssertEquals("9849.30.00", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].AccrualAccount.ToString());
			AssertEquals("BRK", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].ChargeGroup.ToString());
			AssertEquals("N", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].IsGroupageCharge.ToString());
			AssertEquals("BRK", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].SubGroup.ToString());
			AssertEquals("FLT", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].RateCalculator.ToString());

			AssertEquals("Y", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].ShowOnQuotation.ToString());
			AssertEquals("Y", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].SuppressOnQuoteIfZero.ToString());
			AssertEquals("TT", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[1].IATA_ChargeCodeMap.ToString());

			AssertEquals("Y", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[2].IsGlobal);
			AssertEquals("OBILLG", valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes[2].Code.ToString());

			AssertEquals("4710.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[0].AccNumber.ToString());
			AssertEquals("CR", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[0].DebitCredit.ToString());
			AssertEquals("EXCHANGE DIFFERENCES", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[0].Description.ToString());
			AssertEquals("PL", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[0].AccountType.ToString());
			AssertEquals("8370.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[0].PercentNum.ToString());
			AssertEquals("1040.10.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[0].ConsolidationNum.ToString());

			AssertEquals("3150.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[0].AlternateNum.ToString());
			AssertEquals("9799.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[0].HeaderDependsOnTotal.ToString());
			AssertEquals(new ZInt(10), valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[0].TotalLevel);
			AssertEquals("Y", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[0].ControlAccount.ToString());
			AssertEquals("N", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[0].DisallowDirectPosting.ToString());
			AssertEquals(new ZInt(10), valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[0].PrintSequence);

			AssertEquals("4110.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[1].AccNumber.ToString());
			AssertEquals("DR", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[1].DebitCredit.ToString());
			AssertEquals("PORT & TERMINAL REVENUE ACTUAL", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[1].Description.ToString());
			AssertEquals("PL", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[1].AccountType.ToString());
			AssertEquals("1370.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[1].PercentNum.ToString());
			AssertEquals("1050.99.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[1].ConsolidationNum.ToString());

			AssertEquals("3190.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[1].AlternateNum.ToString());
			AssertEquals("1339.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[1].HeaderDependsOnTotal.ToString());
			AssertEquals(new ZInt(15), valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[1].TotalLevel);
			AssertEquals("N", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[1].ControlAccount.ToString());
			AssertEquals("Y", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[1].DisallowDirectPosting.ToString());
			AssertEquals(new ZInt(15), valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders[1].PrintSequence);

			AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[0].Language);
			AssertEquals("5101.000", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[0].LocalAccountNumber.ToString());
			AssertEquals("CR", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[0].DebitCredit.ToString());
			AssertEquals("主营业务收入", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[0].Description.ToString());
			AssertEquals("P&L", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[0].ReportCategory.ToString());
			AssertEquals("8370.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[0].PercentNum.ToString());
			AssertEquals("1040.10.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[0].ConsolidationNum.ToString());
			AssertEquals("3150.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[0].AlternativeNum.ToString());
			AssertEquals("9799.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[0].HeaderDependsOnTotal.ToString());
			AssertEquals("9999.00.00", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[0].CarriedForwardAccount.ToString());
			AssertEquals(new ZInt(10), valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[0].TotalLevel);
			AssertEquals(new ZInt(20), valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings[0].PrintSequence);

			AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageReportSetups[0].Language);
			AssertEquals("1001", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageReportSetups[0].LocalAccountNumber);
			AssertEquals("BSH", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageReportSetups[0].ReportType);
			AssertEquals("A01", valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageReportSetups[0].ReportCategory);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportWhenWrongTypeOfRowIsSpecified()
		{
			ZString expectedText = "Warning: Unrecognized row type detected. Only 'GLACCOUNT', 'CHARGECODE', 'GLMAPPING' and 'REPSETUP' row types are used for import. The following row will be ignored: 'CHARGE CODE'";
			Xsd.GLHeadersAndChargeCodes valueObject = new Xsd.GLHeadersAndChargeCodes();
			NotificationBuffer notificationBuffer = new NotificationBuffer();
			GLHeaderAndChargeCodesCSVFlatFileConverter converter = new GLHeaderAndChargeCodesCSVFlatFileConverter(notificationBuffer, Factory);

			AssertNotContains("Precondition: ", expectedText, notificationBuffer.AsString);

			using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLHeadersAndChargeCodes\Testing\InvalidChargeCodeTestData.csv"))
			{
				converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);
			}
			AssertContains("A warning must be shown.", expectedText, notificationBuffer.AsString);
			AssertContains("A warning must be shown.", "The CSV file format is incorrect", notificationBuffer.AsString);
			AssertEquals(0, valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaders.Count);
			AssertEquals(0, valueObject.SingleGLHeadersAndChargeCodesElement.ChargeCodes.Count);
			AssertEquals(0, valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings.Count);
			AssertEquals(0, valueObject.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageReportSetups.Count);
		}
	}
}
