

using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class VoucherDetailReportTest : ScriptTest
	{
		public void TestCurrencyVoucherNumber()
		{
			AccGLHeader glHeader1 = TestObjectCreator.CreateAccGLHeader("1200.03.97", "TS", "Test AR", "P&L", Constants.DebitCredit.Debit);
			AccGLHeader glHeader2 = TestObjectCreator.CreateAccGLHeader("1200.03.98", "TS", "Test AP", "P&L", Constants.DebitCredit.Credit);
			TestObjectCreator.CreateAccountDescriptor(glHeader1, "6100.097", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit);
			TestObjectCreator.CreateAccountDescriptor(glHeader2, "5300.098", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Credit);

			AccGLHeader glHeaderGST = TestObjectCreator.CreateAccGLHeader("1200.10.40", "TS", "GST Acc", "BSH", Constants.DebitCredit.Credit);
			AccGLHeader glHeaderGST1 = TestObjectCreator.CreateAccGLHeader("1200.10.50", "TS", "GST Acc", "BSH", Constants.DebitCredit.Credit);

			TestObjectCreator.CreateAccountDescriptor(glHeaderGST, "6000.098", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Credit);
			AccountingConfigurationRegistry.Instance.GSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeaderGST.PK.ToGuid());
			TestObjectCreator.CreateAccountDescriptor(glHeaderGST1, "6000.099", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Credit);
			AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeaderGST1.PK.ToGuid());

			TestObjectCreator.ABIGAS.ARSettlementGroupPK = ZGuid.Empty;
			TestObjectCreator.LocalClient.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.LocalClient2.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			InvoicingBase aRInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.USD, 6.5m, 100m, 10m, 650m, 65m);
			aRInvoice.Lines[0].AL_AG = glHeader1.PK;
			aRInvoice.AH_TransactionNum = "01234567890";
			aRInvoice.AH_TransactionReference = "11234567890";
			InvoicingBase aPInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV2", TestObjectCreator.LocalCurrency, 1M, 20M, 3M, 20M, 3M);
			aPInvoice.Lines[0].AL_AG = glHeader2.PK;
			aPInvoice.AH_TransactionNum = "01234567891";
			aPInvoice.AH_TransactionReference = "11234567891";
			aRInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV3", TestObjectCreator.LocalCurrency, 1M, 30M, 0M, 30M, 0M);
			aRInvoice.Lines[0].AL_AG = glHeader1.PK;
			aRInvoice.AH_TransactionNum = "01234567892";
			aRInvoice.AH_TransactionReference = "11234567892";
			Factory.Save();

			DataTable table = DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"select * from dbo.VoucherDetailReport('SubLedger')"));
			AssertNotNull("DataTable Should not null", table);
			AssertEquals("The result should have add 2 GST rows, 3+2=5. ", 5, table.Rows.Count);
			Assert("The result should have Currency column", table.Columns.Contains("Currency"));
		}

		#region Implementation

		readonly ZString country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			base.SetUp();
		}
		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = country;
			base.TearDown();
		}

		#endregion
	}
}



