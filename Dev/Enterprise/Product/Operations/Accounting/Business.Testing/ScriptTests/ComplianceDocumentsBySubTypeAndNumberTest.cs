using System.Data;
using CargoWise.Data;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class ComplianceDocumentsBySubTypeAndNumberTest : ScriptTest
	{
		DataTable RunScript(string ledger, int reportingPeriodFrom, int reportingPeriodTo, string documentDateFrom, string documentDateTo, string status)
		{
			var sql = string.Format(@"
SELECT * FROM ComplianceDocumentsBySubTypeAndNumber(
	'{0}',				-- Ledger
	'{1}',				-- Country
	'{2}',				-- Company
	'{3}',				-- TaxRegistrationOrgCusCode
	'{4}',				-- ReportingPeriodFrom
	'{5}',				-- ReportingPeriodTo
	'{6}',				-- DocumentDateFrom
	'{7}',				-- DocumentDateTo
	'',					-- SubTypeList
	'{8}',				-- Status
	'',					-- ComplianceSequenceBookList
	''					-- TransactionTypeList
)",
							ledger,
							GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
							GlbCompany.CurrentCompany.PK,
							"VAT",
							reportingPeriodFrom,
							reportingPeriodTo,
							documentDateFrom,
							documentDateTo,
							status
			);
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		DataTable RunScriptForDocumentDateIsNull(string ledger, int reportingPeriodFrom, int reportingPeriodTo, string status)
		{
			var sql = string.Format(@"
SELECT * FROM ComplianceDocumentsBySubTypeAndNumber(
	'{0}',				-- Ledger
	'{1}',				-- Country
	'{2}',				-- Company
	'{3}',				-- TaxRegistrationOrgCusCode
	'{4}',				-- ReportingPeriodFrom
	'{5}',				-- ReportingPeriodTo
	NULL,				-- DocumentDateFrom
	NULL,				-- DocumentDateTo
	'',					-- SubTypeList
	'{6}',				-- Status
	'',					-- ComplianceSequenceBookList
	''					-- TransactionTypeList
)",
							ledger,
							GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
							GlbCompany.CurrentCompany.PK,
							"VAT",
							reportingPeriodFrom,
							reportingPeriodTo,
							status
			);
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		public void TestComplianceDocumentsBySubTypeAndNumber()
		{
			var headers = new[] { "DocumentNumber", "LocalExcludingTax", "LocalTaxAmount" };

			var resultForAP = RunScript("AP", apInvoiceDocumentHeader.ADH_ReportingPeriod, apInvoiceDocumentHeader1.ADH_ReportingPeriod, apInvoiceDocumentHeader.ADH_DocumentDate.ToString("yyyy-MM-dd"), apInvoiceDocumentHeader1.ADH_DocumentDate.AddDays(1).ToString("yyyy-MM-dd"), "ALL");

			var aplines = new[] {
					new object[] { "TX00090001", apInvLine.AL_LineAmount, apInvLine.AL_GSTVAT },
					new object[] { "TX00090002", apInvLine1.AL_LineAmount, apInvLine1.AL_GSTVAT },
					new object[] { "TX00090003", null, null }
				};

			AssertDataTableAllRowsByKeyColumns("AP", resultForAP, headers, aplines);

			var resultForAR = RunScript("AR", arInvoiceDocumentHeader.ADH_ReportingPeriod, arInvoiceDocumentHeader1.ADH_ReportingPeriod, arInvoiceDocumentHeader.ADH_DocumentDate.ToString("yyyy-MM-dd"), arInvoiceDocumentHeader1.ADH_DocumentDate.AddDays(1).ToString("yyyy-MM-dd"), "ALL");

			var arlines = new[] {
					new object[] { "T00010003", arInvLine.AL_LineAmount, arInvLine.AL_GSTVAT },
					new object[] { "T00010004", arInvLine1.AL_LineAmount, arInvLine1.AL_GSTVAT },
					new object[] { "T00010005", null, null }
				};

			AssertDataTableAllRowsByKeyColumns("AR", resultForAR, headers, arlines);
		}

		public void TestComplianceDocumentsBySubTypeAndNumber_WhenDocumentDateFromAndNextDateToIsNull()
		{
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.AccComplianceDocumentHeader(ADH_PK, ADH_Ledger, ADH_DocumentDate, ADH_DocumentStatus, ADH_DocumentType, ADH_ReportingPeriod, ADH_GC_Company, ADH_SystemCreateTimeUtc, ADH_SystemLastEditTimeUtc, ADH_SystemCreateUser, ADH_SystemLastEditUser)
			VALUES (newid(), 'AP', GETDATE(), 'SET', 'VAT',	201501,	'{GlbCompany.CurrentCompany.PK}', GETDATE(), GETDATE(), '~BP', '~BP')");

			var result = RunScriptForDocumentDateIsNull("AP", 201501, 201512, "ALL");

			AssertEquals("Result should have 1 row", 1, result.Rows.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var apInv = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Debtor);
			apInvLine = (APInvoiceLine)apInv.Lines.AddNew();
			apInvLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			apInvLine.AL_LocalExTaxAmount = 100;
			apInvLine.AL_LocalTaxAmount = 10m;
			apInvoiceDocumentHeader = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsPayable, "desc", "TX00090001", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", apInvLine);
			apInvoiceDocumentHeader.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			apInvoiceDocumentHeader.ADH_ReportingPeriod = 201802;

			var apInv1 = TestObjectCreator.CreateAPInvoice<APInvoice>("INV002", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Debtor);
			apInvLine1 = (APInvoiceLine)apInv1.Lines.AddNew();
			apInvLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
			apInvLine1.AL_LocalExTaxAmount = 100;
			apInvLine1.AL_LocalTaxAmount = 10m;
			apInvoiceDocumentHeader1 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsPayable, "desc", "TX00090002", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", apInvLine1);
			apInvoiceDocumentHeader1.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			apInvoiceDocumentHeader1.ADH_ReportingPeriod = 201803;

			var apInv2 = TestObjectCreator.CreateAPInvoice<APInvoice>("INV003", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Debtor);
			apInvLine2 = (APInvoiceLine)apInv2.Lines.AddNew();
			apInvLine2.AL_AG = TestObjectCreator.GLHeader1.PK;
			apInvLine2.AL_LocalExTaxAmount = 100;
			apInvLine2.AL_LocalTaxAmount = 10m;
			apVoidInvoiceDocumentHeader = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsPayable, "desc", "TX00090003", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", apInvLine2);
			apVoidInvoiceDocumentHeader.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			apVoidInvoiceDocumentHeader.ADH_ReportingPeriod = 201803;
			apVoidInvoiceDocumentHeader.ADH_DocumentStatus = "VOD";

			var arInv = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1.0m, TestObjectCreator.Debtor);
			arInvLine = (ARInvoiceLine)arInv.Lines.AddNew();
			arInvLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			arInvLine.AL_LocalExTaxAmount = 100;
			arInvLine.AL_LocalTaxAmount = 10m;
			arInvoiceDocumentHeader = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "T00010003", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", arInvLine);
			arInvoiceDocumentHeader.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			arInvoiceDocumentHeader.ADH_ComplianceSubType = "TXI";
			arInvoiceDocumentHeader.ADH_ReportingPeriod = 201804;

			var arInv1 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV002", TestObjectCreator.AUD, 1.0m, TestObjectCreator.Debtor);
			arInvLine1 = (ARInvoiceLine)arInv1.Lines.AddNew();
			arInvLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
			arInvLine1.AL_LocalExTaxAmount = 100;
			arInvLine1.AL_LocalTaxAmount = 10m;
			arInvoiceDocumentHeader1 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "T00010004", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", arInvLine1);
			arInvoiceDocumentHeader1.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			arInvoiceDocumentHeader1.ADH_ComplianceSubType = "TXI";
			arInvoiceDocumentHeader1.ADH_ReportingPeriod = 201805;

			var arInv2 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV003", TestObjectCreator.AUD, 1.0m, TestObjectCreator.Debtor);
			arInvLine2 = (ARInvoiceLine)arInv2.Lines.AddNew();
			arInvLine2.AL_AG = TestObjectCreator.GLHeader1.PK;
			arInvLine2.AL_LocalExTaxAmount = 100;
			arInvLine2.AL_LocalTaxAmount = 10m;
			arVoidInvoiceDocumentHeader = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "T00010005", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", arInvLine2);
			arVoidInvoiceDocumentHeader.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			arVoidInvoiceDocumentHeader.ADH_ComplianceSubType = "TXI";
			arVoidInvoiceDocumentHeader.ADH_ReportingPeriod = 201805;
			arVoidInvoiceDocumentHeader.ADH_DocumentStatus = "VOD";

			Factory.Save();
		}

		AccComplianceDocumentHeader apInvoiceDocumentHeader;
		AccComplianceDocumentHeader apInvoiceDocumentHeader1;
		AccComplianceDocumentHeader apVoidInvoiceDocumentHeader;

		AccComplianceDocumentHeader arInvoiceDocumentHeader;
		AccComplianceDocumentHeader arInvoiceDocumentHeader1;
		AccComplianceDocumentHeader arVoidInvoiceDocumentHeader;

		APInvoiceLine apInvLine;
		APInvoiceLine apInvLine1;
		APInvoiceLine apInvLine2;

		ARInvoiceLine arInvLine;
		ARInvoiceLine arInvLine1;
		ARInvoiceLine arInvLine2;
	}
}
