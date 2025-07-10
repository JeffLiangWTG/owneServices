using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.DataTransfer.Testing
{
	sealed class CAFlatFileInvoiceDataImporterTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportImportInvoices()
		{
			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_ClassificationNumber = "84713000";
			classHeader.ZA_EffectiveDate = ZDateTime.Today.AddYears(-1);
			classHeader.ZA_ExpiryDate = ZDateTime.Today.AddYears(1);

			var refNumHeader = Factory.New<CACTaxRefNumHeader>();
			refNumHeader.ZD_ZA_ClassNumber = classHeader.PK;
			refNumHeader.ZD_EffectiveDate = ZDateTime.Today.AddYears(-1);
			refNumHeader.ZD_ExpiryDate = ZDateTime.Today.AddYears(1);

			var refNum1 = refNumHeader.RefNumbers.AddNew();
			refNum1.ZE_GSTRefNumber = "AA3";

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUPPLIER1";
			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "IMPORTER1";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var importer = new CAFlatFileInvoiceDataImporter(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\DataTransfer\DataTransfer.Test\TestFiles\DataImporterTestFile.csv", declaration);
			importer.Import();
			declaration.ResumeApportionment();
			var invoice = declaration.Invoices[0];
			AssertEquals("US", invoice.JZ_RN_NKDefaultOrigin);
			AssertEquals("AL", invoice.JZ_RW_NKOriginState);
			AssertEquals("US", invoice.CA_RN_NKExport);
			AssertEquals("AK", invoice.CA_USStateOfExport);
			AssertEquals("CN", invoice.CA_RN_NKTranshipment);
			AssertEquals("CATOR", invoice.CA_RL_NKLastPort);
			AssertEquals(new ZDate(2012, 03, 13), invoice.JZ_ValuationDateOverride.Date);
			AssertEquals("100B", invoice.CA_TradeZone);
			AssertEquals("0105", invoice.CA_USPortOfExit);
			AssertEquals("04", invoice.CA_TreatmentCode);
			AssertEquals("13", invoice.CA_ValueForDutyCode);

			Assert(invoice.Charges.Cast<InvoiceCharge>().Any(x => x.J7_ChargeType == "CAC"));

			var line = invoice.JobComInvoiceLines[0];
			AssertEquals("01", line.CA_TreatmentCode);
			AssertEquals("AL", line.JI_StateOrRegionOfOrigin);
			AssertEquals("99AB", line.CA_99TariffCode);
			AssertEquals("13", line.CA_ValueForDutyCode);
			AssertEquals("AUTH0001", line.CA_AuthorityNumber);
			AssertEquals("TRS001", line.CA_TRSNumber);
			AssertEquals(7, line.CA_PageNumber);

			Assert(line.DutiesAndTaxes.Any(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST && x.C1_ExemptCode == "48" && !x.C1_Override && x.C1_Code == "001"));
			Assert(line.DutiesAndTaxes.Any(x => x.C1_TaxType == DutyAndTaxTypes.Codes.SIMADuty && x.C1_ExemptCode == "10" && x.C1_Override));
			Assert(line.DutiesAndTaxes.Any(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax && x.C1_ExemptCode == "85" && !x.C1_Override));

			AssertEquals("REQ001", line.CA_RequirementID);
			AssertEquals("VER1", line.CA_RequirementVer);
			AssertEquals("AIR001", line.CA_AirsCode);
			AssertEquals("AB", line.CA_DestinationProvince);
			AssertEquals("US", line.CA_RN_NKCFIAOrigin);
			AssertEquals("AL", line.CA_CFIAUSStateOfOrigin);
			AssertEquals("03", line.CA_EndUse);
			AssertEquals("011", line.CA_MiscID);

			Assert(line.CFIARegistrationNumbers.ContainsRegNum("102", "111"));
			Assert(line.CFIARegistrationNumbers.ContainsRegNum("117", "222"));

			Assert(line.SITTCertificationNumbers.ContainsNumber("C001"));
			Assert(line.SITTCertificationNumbers.ContainsNumber("C002"));

			AssertEquals("01", line.CA_ImportReasonCode);
			AssertEquals("M001", line.CA_Model);
			AssertEquals("NO001", line.CA_ModelNumber);
			AssertEquals("BRAND", line.JI_BrandName);
			AssertEquals("111", line.CA_TIIN);
			AssertEquals("33X22", line.CA_TypeSize);
			Assert(line.CA_CompliantCompletion);
			Assert(line.CA_CompliantImportDate);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLVXImportDoesNotSupportMultipleInvoiceHeaders()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var importer = new CAFlatFileInvoiceDataImporter(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\DataTransfer\DataTransfer.Test\TestFiles\DataImporterTestFile.csv", declaration);
				var logs = new ZStringBuilder();
				importer.LogEvent += (sender, args) => logs.AppendIfNotEmpty(((CAFlatFileInvoiceDataImporter)sender).LogEntry);
				importer.Import();
				AssertMultilineASCIIEquals("Import Log", @"Invalid invoice record type. Row excluded. Content: sdalshda,
--------------------
Import Successful", logs.ToStringWithNewLineBetweenAppends());

				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
				importer = new CAFlatFileInvoiceDataImporter(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\DataTransfer\DataTransfer.Test\TestFiles\DataImporterTestFile.csv", declaration);
				logs = new ZStringBuilder();
				importer.LogEvent += (sender, args) => logs.AppendIfNotEmpty(((CAFlatFileInvoiceDataImporter)sender).LogEntry);
				importer.Import();
				AssertMultilineASCIIEquals("LVS Log", @"Multiple invoice header lines found. Only one invoice header line is allowed.
File is not valid for import.", logs.ToStringWithNewLineBetweenAppends());

				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				importer = new CAFlatFileInvoiceDataImporter(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\DataTransfer\DataTransfer.Test\TestFiles\DataImporterTestFile.csv", declaration);
				logs = new ZStringBuilder();
				importer.LogEvent += (sender, args) => logs.AppendIfNotEmpty(((CAFlatFileInvoiceDataImporter)sender).LogEntry);
				importer.Import();
				AssertMultilineASCIIEquals("Import Log", @"Invalid invoice record type. Row excluded. Content: sdalshda,
--------------------
Import Successful", logs.ToStringWithNewLineBetweenAppends());
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportLVXInvoices()
		{
			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_ClassificationNumber = "84713000";
			classHeader.ZA_EffectiveDate = ZDateTime.Today.AddYears(-1);
			classHeader.ZA_ExpiryDate = ZDateTime.Today.AddYears(1);

			var refNumHeader = Factory.New<CACTaxRefNumHeader>();
			refNumHeader.ZD_ZA_ClassNumber = classHeader.PK;
			refNumHeader.ZD_EffectiveDate = ZDateTime.Today.AddYears(-1);
			refNumHeader.ZD_ExpiryDate = ZDateTime.Today.AddYears(1);

			var refNum1 = refNumHeader.RefNumbers.AddNew();
			refNum1.ZE_GSTRefNumber = "AA3";

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUPPLIER1";
			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "IMPORTER1";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var importer = new CAFlatFileInvoiceDataImporter(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\DataTransfer\DataTransfer.Test\TestFiles\DataImporterTestFileWithOneHeader.csv", declaration);
			importer.Import();
			declaration.ResumeApportionment();
			var invoice = declaration.Invoices[0];

			AssertEquals("US", invoice.JZ_RN_NKDefaultOrigin);
			AssertEquals("AL", invoice.JZ_RW_NKOriginState);
			AssertEquals("US", invoice.CA_RN_NKExport);
			AssertEquals("AK", invoice.CA_USStateOfExport);
			AssertEquals(new ZDate(2012, 03, 13), invoice.JZ_ValuationDateOverride.Date);
			AssertEquals("04", invoice.CA_TreatmentCode);

			Assert(invoice.Charges.Cast<InvoiceCharge>().Any(x => x.J7_ChargeType == "CAC"));

			var line = invoice.JobComInvoiceLines[0];
			AssertEquals("01", line.CA_TreatmentCode);
			AssertEquals("AL", line.JI_StateOrRegionOfOrigin);
			AssertEquals("99AB", line.CA_99TariffCode);
			AssertEquals("13", line.CA_ValueForDutyCode);
			AssertEquals("AUTH0001", line.CA_AuthorityNumber);
			AssertEquals("TRS001", line.CA_TRSNumber);

			Assert(line.DutiesAndTaxes.Any(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST && x.C1_ExemptCode == "48" && !x.C1_Override && x.C1_Code == "001"));
			Assert(line.DutiesAndTaxes.Any(x => x.C1_TaxType == DutyAndTaxTypes.Codes.SIMADuty && x.C1_ExemptCode == "10" && x.C1_Override));
			Assert(line.DutiesAndTaxes.Any(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax && x.C1_ExemptCode == "85" && !x.C1_Override));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportExportInvoices()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUPPLIER1";
			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "IMPORTER1";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var importer = new CAFlatFileInvoiceDataImporter(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\DataTransfer\DataTransfer.Test\TestFiles\DataImporterTestFile.csv", declaration);
			importer.Import();

			var invoice = declaration.Invoices[0];
			AssertEquals("US", invoice.JZ_RN_NKDefaultOrigin);
			AssertEquals("AL", invoice.JZ_RW_NKOriginState);

			var line = invoice.JobComInvoiceLines[0];
			AssertEquals("VID001", line.CA_ConveyanceIdentificationNumber);
			Assert(line.Permits.ContainsNumber("P001"));
			Assert(line.Permits.ContainsNumber("P002"));
			AssertEquals("AL", line.JI_StateOrRegionOfOrigin);
		}
	}
}
