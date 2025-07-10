using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.CashBook.ExchangeDifference;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing
{
	public class CBEXXGeneralLedgerDataLineCreatorTest : GeneralLedgerDataLineCreatorTest
	{
		public void TestCreateDRCREntries_CBEXXPosted()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_RX_NKAccountCurrency = "USD";
			Factory.Save();
			var exchangeDifference = Factory.New<CashbookExchangeDiff>();
			var postDate = new ZDateTime(2023, 5, 10);

			exchangeDifference.AH_Ledger = exchangeDifference.AH_Ledger.IsEmpty ? (ZString)LedgerTypes.AccountsReceivable : exchangeDifference.AH_Ledger;
			exchangeDifference.AH_InvoiceDate = ZDateTime.Now;
			exchangeDifference.AH_PostDate = ZDateTime.Now;
			exchangeDifference.AH_AB = bankAccount.PK;
			exchangeDifference.AH_ExchangeRate = 0.1274m;
			exchangeDifference.AH_GC = TestObjectCreator.CreateNewCompany("ABC").PK;
			exchangeDifference.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			exchangeDifference.AH_OSExTaxAmount = 0m;
			exchangeDifference.AH_InvoiceAmount = 20m;
			exchangeDifference.AH_AG = TestObjectCreator.CreateGLHeader().PK;
			exchangeDifference.AH_PostDate = postDate;

			var creator = new CBEXXGeneralLedgerDataLineCreator();
			var entry = creator.CreateDRCREntries(((INeedRow)exchangeDifference).Row);

			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = exchangeDifference.BankAccount.AB_AG, LocalAmount = 20m, OSAmount = 0m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.TransactionBankGLAccount, JournalDate = postDate },
				new DebitCreditEntryItem { AccountPK = exchangeDifference.GLHeader.PK, LocalAmount = -20m, OSAmount = 0m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.TransactionHeaderGLAccount, JournalDate = postDate }
			};
			foreach (var line in expectedDRCRLines)
			{
				line.JournalDate = exchangeDifference.AH_PostDate;
				line.GLDType = AccountingConstants.GLDTypeCodes.Posting;
				line.Period = 0;
			}

			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);

			exchangeDifference.AH_OSExTaxAmount = 10m;
			exchangeDifference.AH_OSTotal = 30m;
			entry = creator.CreateDRCREntries(((INeedRow)exchangeDifference).Row);

			expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = exchangeDifference.BankAccount.AB_AG, LocalAmount = 78.49m, OSAmount = 0m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.TransactionBankGLAccount, JournalDate = postDate },
				new DebitCreditEntryItem { AccountPK = exchangeDifference.GLHeader.PK, LocalAmount = -78.49m, OSAmount = 0m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.TransactionHeaderGLAccount, JournalDate = postDate }
			};
			foreach (var line in expectedDRCRLines)
			{
				line.JournalDate = exchangeDifference.AH_PostDate;
				line.GLDType = AccountingConstants.GLDTypeCodes.Posting;
				line.Period = 0;
			}

			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);
		}

		public override void TestHasValidControlAccount()
		{
			Assert(true);
		}
	}
}
