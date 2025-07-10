using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing;

public class AccountingJournalLineForGeneralLedgerDataCreatorTest : TestCaseWithFactory
{
	public void TestCreateAccountingJournalLines()
	{
		var invoice = Factory.NewWithValidTestData<ARInvoice>();
		var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
		var charge = Factory.NewWithValidTestData<AccChargeCode>();
		var invoiceLine = invoice.Lines.AddNew();
		invoiceLine.AL_AC = charge.PK;

		CreateGeneralLedgerData(invoice, invoiceLine, glHeader, "REC", 10m, "USD", 1000m, 0m, "APS");

		Factory.Save();

		var creator = new AccountingJournalLineForGeneralLedgerDataCreator();

		var accountingJournalLinesWithoutHeader = creator.CreateAccountingJournalLines(null, invoiceLine, null, null);
		AssertEquals(1, accountingJournalLinesWithoutHeader.Count());

		var accountingJournalLinesWithHeader = creator.CreateAccountingJournalLines(invoice, invoiceLine, null, null);
		AssertEquals(1, accountingJournalLinesWithHeader.Count());
	}

	public void TestCreateAccountingJournalLinesForOppositeTransaction()
	{
		var bankTransferTo1 = Factory.NewWithValidTestData<BankTransferFromRow>();
		var bankTransferFrom1 = Factory.NewWithValidTestData<BankTransferToRow>();

		var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
		bankTransferFrom1.AH_TransactionBelongsToGroup = bankTransferTo1.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
		bankTransferFrom1.AH_TransactionNum = bankTransferTo1.AH_TransactionNum = "000010000";

		bankTransferFrom1.AH_OSExTaxAmount = bankTransferTo1.AH_OSExTaxAmount = 10M;
		bankTransferFrom1.AH_RX_NKTransactionCurrency = bankTransferTo1.AH_RX_NKTransactionCurrency = "USD";

		Factory.Save();

		var validLedgerTypes = new ZString[] { LedgerTypes.CashBook };
		var validTransactionTypes = new ZString[] { TransactionTypes.Transfer };

		CreateGeneralLedgerData(bankTransferTo1, null, glHeader, "PST", 10m, "BRL", 1000m, 0m, "APS");
		CreateGeneralLedgerData(bankTransferFrom1, null, glHeader, "PST", 10m, "USD", 0m, 2m, "APS");

		Factory.Save();

		var creator = new AccountingJournalLineForGeneralLedgerDataCreator();

		var accountingJournalLinesWithoutHeader = creator.CreateAccountingJournalLines(bankTransferTo1, null, validLedgerTypes, validTransactionTypes);
		AssertEquals(2, accountingJournalLinesWithoutHeader.Count());

		var accountingJournalLinesWithHeader = creator.CreateAccountingJournalLines(bankTransferFrom1, null, validLedgerTypes, validTransactionTypes);
		AssertEquals(2, accountingJournalLinesWithHeader.Count());
	}

	void CreateGeneralLedgerData(TransactionHeader transaction, TransactionLine transactionLine, AccGLHeader glHeader, string type, decimal exchangeRate, string currency, decimal osCreditAmount, decimal osDebitAmount, string glAccountType)
	{
		var accGeneralLedgerData = Factory.NewWithValidTestData<AccGeneralLedgerData>();
		if (transaction is not null)
		{
			accGeneralLedgerData.GLD_AH_TransactionHeader = transaction.PK;
		}

		if (transactionLine is not null)
		{
			accGeneralLedgerData.GLD_AL_TransactionLine = transactionLine.PK;
		}
		accGeneralLedgerData.GLD_Type = type;
		accGeneralLedgerData.GLD_AG_GLAccount = glHeader.PK;
		accGeneralLedgerData.GLD_ExchangeRate = exchangeRate;
		accGeneralLedgerData.GLD_GC_Company = Env.CurrentCompanyPK;
		accGeneralLedgerData.GLD_GB_Branch = Env.CurrentBranchPK;
		accGeneralLedgerData.GLD_GE_Department = Env.CurrentDepartmentPK;
		accGeneralLedgerData.GLD_PostPeriod = 202411;
		accGeneralLedgerData.GLD_PostDate = ZDateTime.Today;
		accGeneralLedgerData.GLD_Currency = currency;
		accGeneralLedgerData.GLD_OSCreditAmount = osCreditAmount;
		accGeneralLedgerData.GLD_OSDebitAmount = osDebitAmount;
		accGeneralLedgerData.GLD_GLAccountType = glAccountType;
	}
}
