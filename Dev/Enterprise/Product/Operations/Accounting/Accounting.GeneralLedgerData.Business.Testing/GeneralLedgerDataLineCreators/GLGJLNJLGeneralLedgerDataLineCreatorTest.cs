using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing
{
	public class GLGJLNJLGeneralLedgerDataLineCreatorTest : GeneralLedgerDataLineCreatorTest
	{
		public override void TestHasValidControlAccount()
		{
			Assert(true);
		}

		public void TestCreateDRCREntries_GJL()
		{
			AssertGLJournalDRCREntries(TransactionTypes.GLStandardJournal);
		}

		public void TestCreateDRCREntries_NJL()
		{
			AssertGLJournalDRCREntries(TransactionTypes.GLNoteJournal);
		}

		public void TestOSAmountNotCalculated_FCBJournal()
		{
			AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

			var glJournal = TestObjectCreator.CreateFCBJournal(ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			var line1 = TestObjectCreator.CreateGLJournalLine(glJournal, 100, DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			line1.AL_ExchangeRate = 1m;
			var line2 = TestObjectCreator.CreateGLJournalLine(glJournal, 100, DebitCredit.DR, TestObjectCreator.GLHeader2.PK);
			line2.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			line2.AL_ExchangeRate = 1.2m;

			TestObjectCreator.Factory.Save();

			var creator = new GLGJLNJLGeneralLedgerDataLineCreator();
			var entry = creator.CreateDRCREntries(((INeedRow)line2).Row);

			Assert(entry.EntryItems.Any());
			AssertEquals(0m, entry.EntryItems[0].OSAmount);
		}

		void AssertGLJournalDRCREntries(string gLJournalType)
		{
			var glJournal = TestObjectCreator.CreateGLJournal(gLJournalType, ZDateTime.Today, ZDateTime.Today);
			var line1 = TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			var line2 = TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			TestObjectCreator.Factory.Save();

			var creator = new GLGJLNJLGeneralLedgerDataLineCreator();
			var entry = creator.CreateDRCREntries(((INeedRow)line1).Row);
			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader1.PK, LocalAmount = 250M, OSAmount = 250M, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.TransactionLineGLAccount, JournalDate = line1.AL_PostDate, GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 0  }
			};

			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);

			entry = creator.CreateDRCREntries(((INeedRow)line2).Row);
			expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader2.PK, LocalAmount = -250M, OSAmount = -250M, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.TransactionLineGLAccount, JournalDate = line2.AL_PostDate, GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 0  }
			};

			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);
		}
	}
}
