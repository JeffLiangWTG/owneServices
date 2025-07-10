using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.DataTransfer.Testing
{
	class BRFlatFileInvoiceDataImporterTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportExportInvoices()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUPPLIER1";
			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "IMPORTER1";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
			var importer = new BRFlatFileInvoiceDataImporter(TestCase.BaseSourcePath + @"Enterprise\Product\Operations\Customs\BR\DataTransfer.Test\TestFiles\DataImporterTestFile_BR_Customs_Export.csv", declaration);
			importer.Import();

			var line = declaration.Invoices[0].JobComInvoiceLines[0];
			Assertion.AssertEquals("35170658500398000105550010001023631156448644", line.JI_NFeNumber);
			Assertion.AssertEquals("001", line.JI_NFeItemNumber);
			Assertion.AssertEquals("80000", line.JI_Procedure);
			Assertion.AssertEquals("80001", line.JI_SecondCPC);
			Assertion.AssertEquals("80002", line.JI_ThirdCPC);
			Assertion.AssertEquals("80003", line.JI_FourthCPC);
			Assertion.AssertEquals("5001", line.JI_CargoPriority);
			Assertion.AssertEquals("Complementary Description", line.ComplementaryDescription);
			Assertion.AssertEquals("BR", line.JI_RN_NKCountryOfExport);
			Assertion.AssertEquals("Justification", line.JI_ExportJustificationInfo);
			Assertion.AssertEquals((ZShort)386, line.JI_IntendedTermDays);
			Assertion.AssertEquals("123456789", line.JI_DigitalServiceDossier);
			Assertion.AssertEquals(5m, line.JI_FinancedValue);
			Assertion.AssertEquals(33m, line.JI_NFeLinePrice);
			Assertion.AssertEquals(Core.Constants.CurrencyCodes.Brazil, line.JI_NFeLinePriceCurrency);
		}
	}
}
