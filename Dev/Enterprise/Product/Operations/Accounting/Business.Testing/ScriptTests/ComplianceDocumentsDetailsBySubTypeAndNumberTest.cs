using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class ComplianceDocumentsDetailsBySubTypeAndNumberTest : ScriptTest
	{
		DataTable RunScript(string ledger, int reportingPeriodFrom, int reportingPeriodTo, string documentDateFrom, string documentDateTo, string status)
		{
			var sql = string.Format(@"
SELECT * FROM ComplianceDocumentsDetailsBySubTypeAndNumber(
	'{0}',				-- Ledger
	'{1}',				-- Country
	'{2}',				-- Company
	'VAT',				-- TaxRegistrationOrgCusCode
	'{3}',				-- ReportingPeriodFrom
	'{4}',				-- ReportingPeriodTo
	'{5}',				-- DocumentDateFrom
	'{6}',				-- DocumentDateTo
	'',					-- SubTypeList
	'{7}',				-- Status
	'',					-- ComplianceSequenceBookList
	''					-- TransactionTypeList
) ORDER BY TransactionNum",
							ledger,
							GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
							GlbCompany.CurrentCompany.PK,
							reportingPeriodFrom,
							reportingPeriodTo,
							documentDateFrom,
							documentDateTo,
							status
			);
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		DataTable RunScriptForDocunmentDateIsNull(string ledger, int reportingPeriodFrom, int reportingPeriodTo, string status)
		{
			var sql = string.Format(@"
SELECT * FROM ComplianceDocumentsDetailsBySubTypeAndNumber(
	'{0}',				-- Ledger
	'{1}',				-- Country
	'{2}',				-- Company
	'VAT',				-- TaxRegistrationOrgCusCode
	'{3}',				-- ReportingPeriodFrom
	'{4}',				-- ReportingPeriodTo
	NULL,				-- DocumentDateFrom
	NULL,				-- DocumentDateTo
	'',					-- SubTypeList
	'{5}',				-- Status
	'',					-- ComplianceSequenceBookList
	''					-- TransactionTypeList
) ORDER BY TransactionNum",
							ledger,
							GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
							GlbCompany.CurrentCompany.PK,
							reportingPeriodFrom,
							reportingPeriodTo,
							status
			);
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		public void TestTransactionLinesByComplianceSubTypesAndNumber()
		{
			var headers = new[] { "DocumentNumber", "LocalExcludingTax", "LocalTaxAmount", "TransactionNum" };

			var resultForAP = RunScript("AP", apInvoiceDocumentHeader.ADH_ReportingPeriod, apInvoiceDocumentHeader1.ADH_ReportingPeriod, apInvoiceDocumentHeader.ADH_DocumentDate.ToString("yyyy-MM-dd"), apInvoiceDocumentHeader1.ADH_DocumentDate.AddDays(1).ToString("yyyy-MM-dd"), "ALL");

			var aplines = new[] {
					new object[] { apInvoiceDocumentHeader.ADH_DocumentNumber, apInvLine.AL_LineAmount, apInvLine.AL_GSTVAT, apInvLine.TransactionHeader.AH_TransactionNum },
					new object[] { apInvoiceDocumentHeader.ADH_DocumentNumber, apInvLine1.AL_LineAmount, apInvLine1.AL_GSTVAT, apInvLine1.TransactionHeader.AH_TransactionNum },
					new object[] { apInvoiceDocumentHeader1.ADH_DocumentNumber, apInvLine2.AL_LineAmount, apInvLine2.AL_GSTVAT, apInvLine2.TransactionHeader.AH_TransactionNum },
					new object[] { apVoidInvoiceDocumentHeader.ADH_DocumentNumber, null, null, apInvLine3.TransactionHeader.AH_TransactionNum + ", " + apInvLine4.TransactionHeader.AH_TransactionNum }
				};

			AssertDataTableAllRowsByKeyColumns("AP", resultForAP, headers, aplines);

			var resultForAR = RunScript("AR", arInvoiceDocumentHeader.ADH_ReportingPeriod, arInvoiceDocumentHeader1.ADH_ReportingPeriod, arInvoiceDocumentHeader.ADH_DocumentDate.ToString("yyyy-MM-dd"), arInvoiceDocumentHeader1.ADH_DocumentDate.AddDays(1).ToString("yyyy-MM-dd"), "ALL");

			var arlines = new[] {
					new object[] { arInvoiceDocumentHeader.ADH_DocumentNumber, arInvLine.AL_LineAmount, arInvLine.AL_GSTVAT, arInvLine.TransactionHeader.AH_TransactionNum },
					new object[] { arInvoiceDocumentHeader.ADH_DocumentNumber, arInvLine1.AL_LineAmount, arInvLine1.AL_GSTVAT, arInvLine1.TransactionHeader.AH_TransactionNum },
					new object[] { arInvoiceDocumentHeader1.ADH_DocumentNumber, arInvLine2.AL_LineAmount, arInvLine2.AL_GSTVAT, arInvLine2.TransactionHeader.AH_TransactionNum },
					new object[] { arVoidInvoiceDocumentHeader.ADH_DocumentNumber, null, null, arInvLine3.TransactionHeader.AH_TransactionNum + ", " + arInvLine4.TransactionHeader.AH_TransactionNum }
				};

			AssertDataTableAllRowsByKeyColumns("AR", resultForAR, headers, arlines);
		}

		public void TestComplianceDocumentsDetailsBySubTypeAndNumber_WhenDocumentDateFromAndNextDateToIsNull()
		{
			var adh_PK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.AccComplianceDocumentHeader(ADH_PK, ADH_Ledger, ADH_DocumentDate, ADH_DocumentStatus, ADH_DocumentType, ADH_ReportingPeriod, ADH_GC_Company, ADH_SystemCreateTimeUtc, ADH_SystemLastEditTimeUtc, ADH_SystemCreateUser, ADH_SystemLastEditUser)
			VALUES ('{adh_PK}', 'AP', GETDATE(), 'VOD', 'VAT', 201501, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GETDATE(), GETDATE(), '~BP', '~BP')");

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.AccComplianceDocumentLine(ADL_PK, ADL_ADH, ADL_Description, ADL_Sequence, ADL_SystemCreateTimeUtc, ADL_SystemCreateUser, ADL_SystemLastEditTimeUtc, ADL_SystemLastEditUser)
			VALUES(newid(),	'{adh_PK}', 'DESC', 1,	GetUtcDate(), '~BP', GetUtcDate(), '~BP')");

			var result = RunScriptForDocunmentDateIsNull("AP", 201501, 201512, "ALL");

			AssertEquals("Result should have 1 row", 1, result.Rows.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var apInv = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV001", TestObjectCreator.AUD, 1M, TestObjectCreator.Creditor1);
			apInvLine = TestObjectCreator.CreateInvoiceLine(apInv, TestObjectCreator.AUD, 1M, 100M);
			apInvLine.AL_AT = TestObjectCreator.GST1.PK;

			var apInv1 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV002", TestObjectCreator.AUD, 1M, TestObjectCreator.Creditor1);
			apInvLine1 = TestObjectCreator.CreateInvoiceLine(apInv1, TestObjectCreator.AUD, 1M, 100M);
			apInvLine1.AL_AT = TestObjectCreator.GST1.PK;

			var apInv2 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV003", TestObjectCreator.AUD, 1M, TestObjectCreator.Creditor2);
			apInvLine2 = TestObjectCreator.CreateInvoiceLine(apInv2, TestObjectCreator.AUD, 1M, 100M);
			apInvLine2.AL_AT = TestObjectCreator.GST1.PK;

			var apInv3 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV004", TestObjectCreator.AUD, 1M, TestObjectCreator.Creditor4);
			apInvLine3 = TestObjectCreator.CreateInvoiceLine(apInv3, TestObjectCreator.AUD, 1M, 100M);
			apInvLine3.AL_AT = TestObjectCreator.GST1.PK;

			var apInv4 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV0041", TestObjectCreator.AUD, 1M, TestObjectCreator.Creditor4);
			apInvLine4 = TestObjectCreator.CreateInvoiceLine(apInv4, TestObjectCreator.AUD, 1M, 100M);
			apInvLine4.AL_AT = TestObjectCreator.GST1.PK;

			new ComplianceDocumentCreator(new[] { apInv, apInv1 }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
			var apQuery1 = new ZQuery();
			apQuery1.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_Ledger, LedgerTypes.AccountsPayable);
			apQuery1.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.Creditor1.PK);
			apInvoiceDocumentHeader = Factory.Load<AccComplianceDocumentHeader>(apQuery1)[0];
			apInvoiceDocumentHeader.ADH_ReportingPeriod = 201802;
			apInvoiceDocumentHeader.ComplianceDocumentLines[0].ADL_Description = "desc";

			new ComplianceDocumentCreator(new[] { apInv2 }, OrganisationCreateComplianceDocumentOnPostingTypes.NotRollup).CreateComplianceDocumentRecords();
			var apQuery2 = new ZQuery();
			apQuery2.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_Ledger, LedgerTypes.AccountsPayable);
			apQuery2.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.Creditor2.PK);
			apInvoiceDocumentHeader1 = Factory.Load<AccComplianceDocumentHeader>(apQuery2)[0];
			apInvoiceDocumentHeader1.ADH_ReportingPeriod = 201803;
			apInvoiceDocumentHeader1.ComplianceDocumentLines[0].ADL_Description = "desc";

			new ComplianceDocumentCreator(new[] { apInv3, apInv4 }, OrganisationCreateComplianceDocumentOnPostingTypes.NotRollup).CreateComplianceDocumentRecords();
			var apQuery3 = new ZQuery();
			apQuery3.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_Ledger, LedgerTypes.AccountsPayable);
			apQuery3.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.Creditor4.PK);
			apVoidInvoiceDocumentHeader = Factory.Load<AccComplianceDocumentHeader>(apQuery3)[0];
			apVoidInvoiceDocumentHeader.ADH_ReportingPeriod = 201803;
			apVoidInvoiceDocumentHeader.ComplianceDocumentLines[0].ADL_Description = "desc";
			apVoidInvoiceDocumentHeader.ADH_DocumentStatus = ComplianceDocumentStatus.Voided;

			Factory.Save();

			var arInv = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV005", TestObjectCreator.AUD, 1M, TestObjectCreator.Debtor);
			arInvLine = TestObjectCreator.CreateInvoiceLine(arInv, TestObjectCreator.AUD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
			arInvLine.AL_AT = TestObjectCreator.GST1.PK;

			var arInv1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV006", TestObjectCreator.AUD, 1M, TestObjectCreator.Debtor);
			arInvLine1 = TestObjectCreator.CreateInvoiceLine(arInv1, TestObjectCreator.AUD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
			arInvLine1.AL_AT = TestObjectCreator.GST1.PK;

			var arInv2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV007", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			arInvLine2 = TestObjectCreator.CreateInvoiceLine(arInv2, TestObjectCreator.AUD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
			arInvLine2.AL_AT = TestObjectCreator.GST1.PK;

			var arInv3 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV008", TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			arInvLine3 = TestObjectCreator.CreateInvoiceLine(arInv3, TestObjectCreator.AUD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
			arInvLine3.AL_AT = TestObjectCreator.GST1.PK;

			var arInv4 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV0081", TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			arInvLine4 = TestObjectCreator.CreateInvoiceLine(arInv4, TestObjectCreator.AUD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
			arInvLine4.AL_AT = TestObjectCreator.GST1.PK;

			new ComplianceDocumentCreator(new[] { arInv, arInv1 }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
			var arQuery1 = new ZQuery();
			arQuery1.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_Ledger, LedgerTypes.AccountsReceivable);
			arQuery1.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.Debtor.PK);
			arInvoiceDocumentHeader = Factory.Load<AccComplianceDocumentHeader>(arQuery1)[0];
			arInvoiceDocumentHeader.ADH_ReportingPeriod = 201804;
			arInvoiceDocumentHeader.ComplianceDocumentLines[0].ADL_Description = "desc";

			new ComplianceDocumentCreator(new[] { arInv2 }, OrganisationCreateComplianceDocumentOnPostingTypes.NotRollup).CreateComplianceDocumentRecords();
			var arQuery2 = new ZQuery();
			arQuery2.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_Ledger, LedgerTypes.AccountsReceivable);
			arQuery2.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.ABIGAS.PK);
			arInvoiceDocumentHeader1 = Factory.Load<AccComplianceDocumentHeader>(arQuery2)[0];
			arInvoiceDocumentHeader1.ADH_ReportingPeriod = 201805;
			arInvoiceDocumentHeader1.ComplianceDocumentLines[0].ADL_Description = "desc";

			new ComplianceDocumentCreator(new[] { arInv3, arInv4 }, OrganisationCreateComplianceDocumentOnPostingTypes.NotRollup).CreateComplianceDocumentRecords();
			var arQuery3 = new ZQuery();
			arQuery3.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_Ledger, LedgerTypes.AccountsReceivable);
			arQuery3.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.AALSHI.PK);
			arVoidInvoiceDocumentHeader = Factory.Load<AccComplianceDocumentHeader>(arQuery3)[0];
			arVoidInvoiceDocumentHeader.ADH_ReportingPeriod = 201805;
			arVoidInvoiceDocumentHeader.ComplianceDocumentLines[0].ADL_Description = "desc";
			arVoidInvoiceDocumentHeader.ADH_DocumentStatus = ComplianceDocumentStatus.Voided;

			Factory.Save();
		}

		AccComplianceDocumentHeader apInvoiceDocumentHeader;
		AccComplianceDocumentHeader apInvoiceDocumentHeader1;
		AccComplianceDocumentHeader apVoidInvoiceDocumentHeader;

		AccTransactionLines apInvLine;
		AccTransactionLines apInvLine1;
		AccTransactionLines apInvLine2;
		AccTransactionLines apInvLine3;
		AccTransactionLines apInvLine4;

		AccComplianceDocumentHeader arInvoiceDocumentHeader;
		AccComplianceDocumentHeader arInvoiceDocumentHeader1;
		AccComplianceDocumentHeader arVoidInvoiceDocumentHeader;

		AccTransactionLines arInvLine;
		AccTransactionLines arInvLine1;
		AccTransactionLines arInvLine2;
		AccTransactionLines arInvLine3;
		AccTransactionLines arInvLine4;
	}
}
