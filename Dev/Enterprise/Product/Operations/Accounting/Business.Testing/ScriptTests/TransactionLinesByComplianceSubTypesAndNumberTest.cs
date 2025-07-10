using System.Data;
using CargoWise.Data;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class TransactionLinesByComplianceSubTypesAndNumberTest : ScriptTest
	{
		DataTable RunScript(string ledger, string postDateFrom, string postDateTo, string status, string orgCusCode = "VAT")
		{
			var sql = string.Format(@"
SELECT * FROM TransactionLinesByComplianceSubTypesAndNumber(
	'{0}',				-- Ledger
	'{1}',				-- Country
	'{2}',				-- Company
	'{3}',				-- TaxRegistrationOrgCusCode
	'{4}',				-- PostDateFrom
	'{5}',				-- PostDateTo
	'',					-- TransactionTypeList
	'',					-- TransactionLinePostingBranchList
	'{6}',				-- Status
	'',					-- ComplianceSubTypeList
	''					-- ComplianceSequenceBookList
)",
							ledger,
							GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
							GlbCompany.CurrentCompany.PK,
							orgCusCode,
							postDateFrom,
							postDateTo,
							status
			);
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		public void TestTransactionLinesByComplianceSubTypesAndNumber()
		{
			var headers = new[] { "TransactionNum", "ComplianceDocumentNumber" };

			var resultForAP = RunScript("AP", apInv.AH_PostDate.ToString("yyyy-MM-dd"), apInv1.AH_PostDate.ToString("yyyy-MM-dd"), "ALL");

			var aplines = new[] {
					new object[] { apInv.AH_TransactionNum, apInvoiceDocumentHeader.ADH_DocumentNumber },
					new object[] { apInv1.AH_TransactionNum, apInvoiceDocumentHeader1.ADH_DocumentNumber },
					new object[] { apVoidInv.AH_TransactionNum, apInvoiceDocumentHeader3.ADH_DocumentNumber }
				};

			AssertDataTableAllRowsByKeyColumns("AP", resultForAP, headers, aplines);

			var resultForAR = RunScript("AR", arInv.AH_PostDate.ToString("yyyy-MM-dd"), arInv1.AH_PostDate.ToString("yyyy-MM-dd"), "ALL");

			var arlines = new[] {
					new object[] { arInv.AH_TransactionNum, arInvoiceDocumentHeader.ADH_DocumentNumber },
					new object[] { arInv1.AH_TransactionNum, arInvoiceDocumentHeader1.ADH_DocumentNumber },
					new object[] { arVoidInv.AH_TransactionNum, null }
				};

			AssertDataTableAllRowsByKeyColumns("AR", resultForAR, headers, arlines);
		}

		protected override void SetUp()
		{
			base.SetUp();

			apInv = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV001", TestObjectCreator.AUD, 1M, TestObjectCreator.Creditor1);
			var apInvLine = TestObjectCreator.CreateInvoiceLine(apInv, TestObjectCreator.AUD, 1M, 100M);
			apInvLine.AL_AT = TestObjectCreator.GST1.PK;
			apInvoiceDocumentHeader = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsPayable, "desc", "TX00090001", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", apInvLine);
			apInvoiceDocumentHeader.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			apInvoiceDocumentHeader.ADH_ComplianceSubType = "TXI";
			apInvoiceDocumentHeader.ADH_ReportingPeriod = 201802;

			apInv1 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV002", TestObjectCreator.AUD, 1M, TestObjectCreator.Creditor1);
			var apInvLine1 = TestObjectCreator.CreateInvoiceLine(apInv1, TestObjectCreator.AUD, 1M, 100M);
			apInvLine1.AL_AT = TestObjectCreator.GST1.PK;
			apInvoiceDocumentHeader1 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsPayable, "desc", "TX00090002", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", apInvLine1);
			apInvoiceDocumentHeader1.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			apInvoiceDocumentHeader1.ADH_ComplianceSubType = "TXI";
			apInvoiceDocumentHeader1.ADH_ReportingPeriod = 201803;

			apVoidInv = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV003", TestObjectCreator.AUD, 1M, TestObjectCreator.Creditor1);
			var apInvLine2 = TestObjectCreator.CreateInvoiceLine(apVoidInv, TestObjectCreator.AUD, 1M, 100M);
			apInvLine2.AL_AT = TestObjectCreator.GST1.PK;
			apInvoiceDocumentHeader2 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsPayable, "desc", "TX00090003", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", apInvLine2);
			apInvoiceDocumentHeader2.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			apInvoiceDocumentHeader2.ADH_ComplianceSubType = "TXI";
			apInvoiceDocumentHeader2.ADH_ReportingPeriod = 201803;
			apInvoiceDocumentHeader2.ADH_DocumentStatus = "VOD";

			apInvoiceDocumentHeader3 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsPayable, "desc", "TX00090004", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", apInvLine2);
			apInvoiceDocumentHeader3.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			apInvoiceDocumentHeader3.ADH_ComplianceSubType = "TXI";
			apInvoiceDocumentHeader3.ADH_ReportingPeriod = 201803;

			arInv = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV004", TestObjectCreator.AUD, 1M, TestObjectCreator.Debtor);
			var arInvLine = TestObjectCreator.CreateInvoiceLine(arInv, TestObjectCreator.AUD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
			arInvLine.AL_AT = TestObjectCreator.GST1.PK;
			arInvoiceDocumentHeader = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "T00010003", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", arInvLine);
			arInvoiceDocumentHeader.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			arInvoiceDocumentHeader.ADH_ComplianceSubType = "TXI";
			arInvoiceDocumentHeader.ADH_ReportingPeriod = 201804;

			arInv1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV005", TestObjectCreator.AUD, 1M, TestObjectCreator.Debtor);
			var arInvLine1 = TestObjectCreator.CreateInvoiceLine(arInv1, TestObjectCreator.AUD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
			arInvLine1.AL_AT = TestObjectCreator.GST1.PK;
			arInvoiceDocumentHeader1 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "T00010004", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", arInvLine1);
			arInvoiceDocumentHeader1.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			arInvoiceDocumentHeader1.ADH_ComplianceSubType = "TXI";
			arInvoiceDocumentHeader1.ADH_ReportingPeriod = 201805;

			arVoidInv = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV005", TestObjectCreator.AUD, 1M, TestObjectCreator.Debtor);
			var arInvLine2 = TestObjectCreator.CreateInvoiceLine(arVoidInv, TestObjectCreator.AUD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
			arInvLine1.AL_AT = TestObjectCreator.GST1.PK;
			arInvoiceDocumentHeader2 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "T00010005", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", arInvLine2);
			arInvoiceDocumentHeader2.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			arInvoiceDocumentHeader2.ADH_ComplianceSubType = "TXI";
			arInvoiceDocumentHeader2.ADH_ReportingPeriod = 201805;
			arInvoiceDocumentHeader2.ADH_DocumentStatus = "VOD";

			Factory.Save();
		}

		InvoicingBase apInv;
		InvoicingBase apInv1;
		InvoicingBase apVoidInv;
		InvoicingBase arInv;
		InvoicingBase arInv1;
		InvoicingBase arVoidInv;

		AccComplianceDocumentHeader apInvoiceDocumentHeader;
		AccComplianceDocumentHeader apInvoiceDocumentHeader1;
		AccComplianceDocumentHeader apInvoiceDocumentHeader2;
		AccComplianceDocumentHeader apInvoiceDocumentHeader3;
		AccComplianceDocumentHeader arInvoiceDocumentHeader;
		AccComplianceDocumentHeader arInvoiceDocumentHeader1;
		AccComplianceDocumentHeader arInvoiceDocumentHeader2;
	}
}
