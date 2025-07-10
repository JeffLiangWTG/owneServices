using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes.Testing
{
	sealed class ChargeCodeCSVFlatFileConverterTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport()
		{
			Xsd.ChargeCodesChargeCodeCollection valueObject = new Xsd.ChargeCodesChargeCodeCollection();
			ChargeCodeCSVFlatFileConverter converter = new ChargeCodeCSVFlatFileConverter(null, Factory);
			using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLHeadersAndChargeCodes\ChargeCode\Testing\ValidChargeCodeTestData.csv"))
			{
				converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals("Destination Quarantine Inspection", valueObject[0].Description.ToString());
			AssertEquals("DCART", valueObject[0].Code.ToString());
			AssertEquals("FEA, FIA, FES, FIS, CEA, CIA, CES, CIS", valueObject[0].DepartmentFilterList.ToString());
			AssertEquals("MRG", valueObject[0].ChargeType.ToString());
			AssertEquals("100", valueObject[0].MarginPercentage.ToString());
			AssertEquals("FREEGST", valueObject[0].GSTRate.ToString());
			AssertEquals("TEST_CODE", valueObject[0].WithholdingTaxRate.ToString());

			AssertEquals("CARTAGE", valueObject[0].SalesGroup.ToString());
			AssertEquals("CARTAGE", valueObject[0].ExpenseGroup.ToString());
			AssertEquals("8999.00.00", valueObject[0].RevenueAccount.ToString());
			AssertEquals("1080.20.10", valueObject[0].WIPAccount.ToString());
			AssertEquals("1090.10.20", valueObject[0].CostAccount.ToString());
			AssertEquals("2020.10.00", valueObject[0].AccrualAccount.ToString());
			AssertEquals("BRK", valueObject[0].ChargeGroup.ToString());
			AssertEquals("Y", valueObject[0].IsGroupageCharge.ToString());
			AssertEquals("BRK", valueObject[0].SubGroup.ToString());
			AssertEquals("FLT", valueObject[0].RateCalculator.ToString());

			AssertEquals("N", valueObject[0].ShowOnQuotation.ToString());
			AssertEquals("N", valueObject[0].SuppressOnQuoteIfZero.ToString());
			AssertEquals("XX", valueObject[0].IATA_ChargeCodeMap.ToString());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_MoreThanOneChargeCodeHeaders()
		{
			Xsd.ChargeCodesChargeCodeCollection valueObject = new Xsd.ChargeCodesChargeCodeCollection();
			NotificationBuffer notificationBuffer = new NotificationBuffer();
			ChargeCodeCSVFlatFileConverter converter = new ChargeCodeCSVFlatFileConverter(notificationBuffer, Factory);

			AssertEquals(false, notificationBuffer.HasErrors);

			using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLHeadersAndChargeCodes\ChargeCode\Testing\Valid2ChargeCodesTestData.csv"))
			{
				converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals(false, notificationBuffer.HasErrors);

			AssertEquals(2, valueObject.Count);

			AssertEquals("Destination Quarantine Inspection", valueObject[0].Description.ToString());
			AssertEquals("OBILL", valueObject[0].Code.ToString());
			AssertEquals("FEA, FIA, FES, FIS, CEA, CIA, CES, CIS", valueObject[0].DepartmentFilterList.ToString());
			AssertEquals("MRG", valueObject[0].ChargeType.ToString());
			AssertEquals("100", valueObject[0].MarginPercentage.ToString());
			AssertEquals("FREEGST", valueObject[0].GSTRate.ToString());
			AssertEquals("TEST_CODE", valueObject[0].WithholdingTaxRate.ToString());

			AssertEquals("CARTAGE", valueObject[0].SalesGroup.ToString());
			AssertEquals("CARTAGE", valueObject[0].ExpenseGroup.ToString());
			AssertEquals("8999.00.00", valueObject[0].RevenueAccount.ToString());
			AssertEquals("1080.20.10", valueObject[0].WIPAccount.ToString());
			AssertEquals("1090.10.20", valueObject[0].CostAccount.ToString());
			AssertEquals("2020.10.00", valueObject[0].AccrualAccount.ToString());
			AssertEquals("BRK", valueObject[0].ChargeGroup.ToString());
			AssertEquals("Y", valueObject[0].IsGroupageCharge.ToString());
			AssertEquals("BRK", valueObject[0].SubGroup.ToString());
			AssertEquals("FLT", valueObject[0].RateCalculator.ToString());

			AssertEquals("N", valueObject[0].ShowOnQuotation.ToString());
			AssertEquals("N", valueObject[0].SuppressOnQuoteIfZero.ToString());
			AssertEquals("XX", valueObject[0].IATA_ChargeCodeMap.ToString());

			AssertEquals("Destination Quarantine Department", valueObject[1].Description.ToString());
			AssertEquals("OEHC", valueObject[1].Code.ToString());
			AssertEquals("FES, FIS, CEA, CIA, CES, CIS", valueObject[1].DepartmentFilterList.ToString());
			AssertEquals("MRG", valueObject[1].ChargeType.ToString());
			AssertEquals("50.55", valueObject[1].MarginPercentage.ToString());
			AssertEquals("FREEGST", valueObject[1].GSTRate.ToString());
			AssertEquals("TEST_CODE", valueObject[1].WithholdingTaxRate.ToString());

			AssertEquals("CARTAGE", valueObject[1].SalesGroup.ToString());
			AssertEquals("CARTAGE", valueObject[1].ExpenseGroup.ToString());
			AssertEquals("5511.00.00", valueObject[1].RevenueAccount.ToString());
			AssertEquals("1093.25.10", valueObject[1].WIPAccount.ToString());
			AssertEquals("9498.10.98", valueObject[1].CostAccount.ToString());
			AssertEquals("9849.30.00", valueObject[1].AccrualAccount.ToString());
			AssertEquals("BRK", valueObject[1].ChargeGroup.ToString());
			AssertEquals("N", valueObject[1].IsGroupageCharge.ToString());
			AssertEquals("BRK", valueObject[1].SubGroup.ToString());
			AssertEquals("FLT", valueObject[1].RateCalculator.ToString());

			AssertEquals("Y", valueObject[1].ShowOnQuotation.ToString());
			AssertEquals("Y", valueObject[1].SuppressOnQuoteIfZero.ToString());
			AssertEquals("TT", valueObject[1].IATA_ChargeCodeMap.ToString());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_WithGovtChargeCodeEnabled()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				Xsd.ChargeCodesChargeCodeCollection valueObject = new Xsd.ChargeCodesChargeCodeCollection();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				ChargeCodeCSVFlatFileConverter converter = new ChargeCodeCSVFlatFileConverter(notificationBuffer, Factory);

				AssertEquals(false, notificationBuffer.HasErrors);

				using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLHeadersAndChargeCodes\ChargeCode\Testing\Valid3ChargeCodesTestData.csv"))
				{
					converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);
				}

				AssertEquals(false, notificationBuffer.HasErrors);

				AssertEquals(2, valueObject.Count);

				AssertEquals("Destination Quarantine Inspection", valueObject[0].Description.ToString());
				AssertEquals("OBILL", valueObject[0].Code.ToString());
				AssertEquals("FEA, FIA, FES, FIS, CEA, CIA, CES, CIS", valueObject[0].DepartmentFilterList.ToString());
				AssertEquals("MRG", valueObject[0].ChargeType.ToString());
				AssertEquals("100", valueObject[0].MarginPercentage.ToString());
				AssertEquals("FREEGST", valueObject[0].GSTRate.ToString());
				AssertEquals("TEST_CODE", valueObject[0].WithholdingTaxRate.ToString());

				AssertEquals("CARTAGE", valueObject[0].SalesGroup.ToString());
				AssertEquals("CARTAGE", valueObject[0].ExpenseGroup.ToString());
				AssertEquals("8999.00.00", valueObject[0].RevenueAccount.ToString());
				AssertEquals("1080.20.10", valueObject[0].WIPAccount.ToString());
				AssertEquals("1090.10.20", valueObject[0].CostAccount.ToString());
				AssertEquals("2020.10.00", valueObject[0].AccrualAccount.ToString());
				AssertEquals("BRK", valueObject[0].ChargeGroup.ToString());
				AssertEquals("Y", valueObject[0].IsGroupageCharge.ToString());
				AssertEquals("BRK", valueObject[0].SubGroup.ToString());
				AssertEquals("FLT", valueObject[0].RateCalculator.ToString());

				AssertEquals("N", valueObject[0].ShowOnQuotation.ToString());
				AssertEquals("N", valueObject[0].SuppressOnQuoteIfZero.ToString());
				AssertEquals("XX", valueObject[0].IATA_ChargeCodeMap.ToString());

				AssertEquals("GOVTCC1", valueObject[0].GovtChargeCode.ToString());
				AssertEquals("N", valueObject[0].IsGlobal.ToString());

				AssertEquals("Destination Quarantine Department", valueObject[1].Description.ToString());
				AssertEquals("OEHC", valueObject[1].Code.ToString());
				AssertEquals("FES, FIS, CEA, CIA, CES, CIS", valueObject[1].DepartmentFilterList.ToString());
				AssertEquals("MRG", valueObject[1].ChargeType.ToString());
				AssertEquals("50.55", valueObject[1].MarginPercentage.ToString());
				AssertEquals("FREEGST", valueObject[1].GSTRate.ToString());
				AssertEquals("TEST_CODE", valueObject[1].WithholdingTaxRate.ToString());

				AssertEquals("CARTAGE", valueObject[1].SalesGroup.ToString());
				AssertEquals("CARTAGE", valueObject[1].ExpenseGroup.ToString());
				AssertEquals("5511.00.00", valueObject[1].RevenueAccount.ToString());
				AssertEquals("1093.25.10", valueObject[1].WIPAccount.ToString());
				AssertEquals("9498.10.98", valueObject[1].CostAccount.ToString());
				AssertEquals("9849.30.00", valueObject[1].AccrualAccount.ToString());
				AssertEquals("BRK", valueObject[1].ChargeGroup.ToString());
				AssertEquals("N", valueObject[1].IsGroupageCharge.ToString());
				AssertEquals("BRK", valueObject[1].SubGroup.ToString());
				AssertEquals("FLT", valueObject[1].RateCalculator.ToString());

				AssertEquals("Y", valueObject[1].ShowOnQuotation.ToString());
				AssertEquals("Y", valueObject[1].SuppressOnQuoteIfZero.ToString());
				AssertEquals("TT", valueObject[1].IATA_ChargeCodeMap.ToString());

				AssertEquals("GOVTCC2", valueObject[1].GovtChargeCode.ToString());
				AssertEquals(string.Empty, valueObject[1].IsGlobal.ToString());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_GlobalChargeCode()
		{
			AssertImportGlobalChargeCode(false);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_GlobalChargeCodeWithGovtChargeCodeEnabled()
		{
			AssertImportGlobalChargeCode(true);
		}

		void AssertImportGlobalChargeCode(bool isGovtChargeCodeEnabled)
		{
			var valueObject = new Xsd.ChargeCodesChargeCodeCollection();

			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, isGovtChargeCodeEnabled))
			{
				ChargeCodeCSVFlatFileConverter converter = new ChargeCodeCSVFlatFileConverter(null, Factory);
				using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLHeadersAndChargeCodes\ChargeCode\Testing\ValidGlobalChargeCodeTestData.csv"))
				{
					converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);
				}
			}

			AssertEquals("Destination Quarantine Inspection", valueObject[0].Description.ToString());
			AssertEquals("GlobalChargeCode01", valueObject[0].Code.ToString());
			AssertEquals("FEA, FIA, FES, FIS, CEA, CIA, CES, CIS", valueObject[0].DepartmentFilterList.ToString());
			AssertEquals("MRG", valueObject[0].ChargeType.ToString());
			AssertEquals("100", valueObject[0].MarginPercentage.ToString());
			AssertEquals("FREEGST", valueObject[0].GSTRate.ToString());
			AssertEquals("TEST_CODE", valueObject[0].WithholdingTaxRate.ToString());

			AssertEquals("CARTAGE", valueObject[0].SalesGroup.ToString());
			AssertEquals("CARTAGE", valueObject[0].ExpenseGroup.ToString());
			AssertEquals("8999.00.00", valueObject[0].RevenueAccount.ToString());
			AssertEquals("1080.20.10", valueObject[0].WIPAccount.ToString());
			AssertEquals("1090.10.20", valueObject[0].CostAccount.ToString());
			AssertEquals("2020.10.00", valueObject[0].AccrualAccount.ToString());
			AssertEquals("BRK", valueObject[0].ChargeGroup.ToString());
			AssertEquals("Y", valueObject[0].IsGroupageCharge.ToString());
			AssertEquals("BRK", valueObject[0].SubGroup.ToString());
			AssertEquals("FLT", valueObject[0].RateCalculator.ToString());

			AssertEquals("N", valueObject[0].ShowOnQuotation.ToString());
			AssertEquals("N", valueObject[0].SuppressOnQuoteIfZero.ToString());
			AssertEquals("XX", valueObject[0].IATA_ChargeCodeMap.ToString());
			AssertEquals("Y", valueObject[0].IsGlobal.ToString());
			AssertEquals(string.Empty, valueObject[0].GovtChargeCode.ToString());

			AssertEquals("Destination Quarantine Inspection", valueObject[1].Description.ToString());
			AssertEquals("GlobalChargeCode02", valueObject[1].Code.ToString());
			AssertEquals("FEA, FIA, FES, FIS, CEA, CIA, CES, CIS", valueObject[1].DepartmentFilterList.ToString());
			AssertEquals("MRG", valueObject[1].ChargeType.ToString());
			AssertEquals("100", valueObject[1].MarginPercentage.ToString());
			AssertEquals("FREEGST", valueObject[1].GSTRate.ToString());
			AssertEquals("TEST_CODE", valueObject[1].WithholdingTaxRate.ToString());

			AssertEquals("CARTAGE", valueObject[1].SalesGroup.ToString());
			AssertEquals("CARTAGE", valueObject[1].ExpenseGroup.ToString());
			AssertEquals("8999.00.00", valueObject[1].RevenueAccount.ToString());
			AssertEquals("1080.20.10", valueObject[1].WIPAccount.ToString());
			AssertEquals("1090.10.20", valueObject[1].CostAccount.ToString());
			AssertEquals("2020.10.00", valueObject[1].AccrualAccount.ToString());
			AssertEquals("BRK", valueObject[1].ChargeGroup.ToString());
			AssertEquals("Y", valueObject[1].IsGroupageCharge.ToString());
			AssertEquals("BRK", valueObject[1].SubGroup.ToString());
			AssertEquals("FLT", valueObject[1].RateCalculator.ToString());

			AssertEquals("N", valueObject[1].ShowOnQuotation.ToString());
			AssertEquals("N", valueObject[1].SuppressOnQuoteIfZero.ToString());
			AssertEquals("XX", valueObject[1].IATA_ChargeCodeMap.ToString());
			AssertEquals("Y", valueObject[1].IsGlobal.ToString());
			AssertEquals(isGovtChargeCodeEnabled ? "GvtCode1" : string.Empty, valueObject[1].GovtChargeCode.ToString());
		}
	}
}
