using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing;

public class AccountingJournalLineProviderForGeneralLedgerDataTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new AccountingJournalLineProviderForGeneralLedgerData(null, null, null, null));
		AssertExceptionThrown<ArgumentNullException>(() => new AccountingJournalLineProviderForGeneralLedgerData(null, null, null));

		var invoice = Factory.NewWithValidTestData<ARInvoice>();
		var invoiceLine = invoice.Lines.AddNew();
		var factory = new ReadOnlyBusinessObjectFactory();
		AssertNoExceptionThrown(() => new AccountingJournalLineProviderForGeneralLedgerData(factory, invoice, null, null));
		AssertNoExceptionThrown(() => new AccountingJournalLineProviderForGeneralLedgerData(invoiceLine, null, null));
	}

	public void TestGetAccountingJournalLines()
	{
		var invoice = Factory.NewWithValidTestData<ARInvoice>();
		var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
		var charge = Factory.NewWithValidTestData<AccChargeCode>();
		var invoiceLine = invoice.Lines.AddNew();
		invoiceLine.AL_AC = charge.PK;

		var accGeneralLedgerData = Factory.NewWithValidTestData<AccGeneralLedgerData>();
		accGeneralLedgerData.GLD_AL_TransactionLine = invoiceLine.PK;
		accGeneralLedgerData.GLD_AH_TransactionHeader = invoice.PK;
		accGeneralLedgerData.GLD_Type = "REC";
		accGeneralLedgerData.GLD_AG_GLAccount = glHeader.PK;
		accGeneralLedgerData.GLD_ExchangeRate = 10m;
		accGeneralLedgerData.GLD_GC_Company = Env.CurrentCompanyPK;
		accGeneralLedgerData.GLD_GB_Branch = Env.CurrentBranchPK;
		accGeneralLedgerData.GLD_GE_Department = Env.CurrentDepartmentPK;
		accGeneralLedgerData.GLD_PostPeriod = 202411;
		accGeneralLedgerData.GLD_PostDate = ZDateTime.Today;
		accGeneralLedgerData.GLD_Currency = "USD";
		accGeneralLedgerData.GLD_OSCreditAmount = 1000m;
		accGeneralLedgerData.GLD_OSDebitAmount = 0m;
		accGeneralLedgerData.GLD_GLAccountType = "APS";

		Factory.Save();

		var factory = new ReadOnlyBusinessObjectFactory();
		var provider = new AccountingJournalLineProviderForGeneralLedgerData(factory, invoice, null, null);
		var accountingJournalLines = provider.GetAccountingJournalLines();
		AssertEquals(1, accountingJournalLines.Count());
	}
}
