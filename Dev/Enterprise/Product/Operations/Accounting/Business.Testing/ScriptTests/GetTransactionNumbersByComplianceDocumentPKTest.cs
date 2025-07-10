using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class GetTransactionNumbersByComplianceDocumentPKTest : ScriptTest
	{
		DataTable RunScript(ZGuid complianceDocumentPK)
		{
			var sql = string.Format(@"
										SELECT * FROM GetTransactionNumbersByComplianceDocumentPK(
										'{0}' -- ComplianceDocumentPK
										)",
										complianceDocumentPK.ToString()
									);
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		public void TestGetTransactionNumbersByComplianceDocumentPKTest()
		{
			var apInv = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV001", TestObjectCreator.AUD, 1M, TestObjectCreator.Creditor1);
			var apInvLine = TestObjectCreator.CreateInvoiceLine(apInv, TestObjectCreator.AUD, 1M, 100M);
			apInvLine.AL_AT = TestObjectCreator.GST1.PK;

			var apInv1 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV002", TestObjectCreator.AUD, 1M, TestObjectCreator.Creditor1);
			var apInvLine1 = TestObjectCreator.CreateInvoiceLine(apInv1, TestObjectCreator.AUD, 1M, 100M);
			apInvLine1.AL_AT = TestObjectCreator.GST1.PK;

			new ComplianceDocumentCreator(new[] { apInv, apInv1 }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
			var apQuery1 = new ZQuery();
			apQuery1.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_Ledger, LedgerTypes.AccountsPayable);
			apQuery1.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.Creditor1.PK);
			var apInvoiceDocumentHeader = Factory.Load<AccComplianceDocumentHeader>(apQuery1)[0];
			apInvoiceDocumentHeader.ADH_ReportingPeriod = 201802;
			apInvoiceDocumentHeader.ComplianceDocumentLines[0].ADL_Description = "desc";

			Factory.Save();

			var resultForAR = RunScript(apInvoiceDocumentHeader.PK);

			AssertEquals(apInv.AH_TransactionNum + ", " + apInv1.AH_TransactionNum, resultForAR.Rows[0][0].ToString());
		}
	}
}
