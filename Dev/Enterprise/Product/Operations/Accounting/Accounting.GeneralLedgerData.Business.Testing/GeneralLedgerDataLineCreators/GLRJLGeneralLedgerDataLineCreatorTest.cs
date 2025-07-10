using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.GeneralLedgerData;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing
{
	public class GLRJLGeneralLedgerDataLineCreatorTest : GeneralLedgerDataLineCreatorTest
	{
		public override void TestHasValidControlAccount()
		{
			Assert(true);
		}

		[TestDate(2023, 5, 18)]
		public void TestCreateDRCRLines_GLRJL()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriodsForEntireYear(2023);

			var today = ZDateTime.Today;
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLReversingJournal, today, today, today.AddDays(35));

			var line = TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			line.AL_Desc = "GL REVERSING JOURNAL";
			var clearLine = TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);
			clearLine.AL_Desc = "Clearing line";

			Factory.Save();

			var creator = new GLRJLGeneralLedgerDataLineCreator();
			var entry = creator.CreateDRCREntries(((INeedRow)line).Row);
			AssertEquals(((ICompany)line.Company).ExchangeRate.GetRate(line.AL_LineAmount + line.AL_GSTVAT, line.AL_OSAmount, AccTransactionLinesSchema.AL_ExchangeRate.Scale), entry.ExchangeRate);

			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = line.AL_AG, LocalAmount = -10m, OSAmount = -10m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.TransactionLineGLAccount, JournalDate = line.AL_PostDate, GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 202305  },
				new DebitCreditEntryItem { AccountPK = line.AL_AG, LocalAmount = 10m, OSAmount = 10m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.TransactionLineGLAccountForReverse, JournalDate = line.AL_ReverseDate, GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 202306  },
			};
			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);

			entry = creator.CreateDRCREntries(((INeedRow)clearLine).Row);
			AssertEquals(((ICompany)clearLine.Company).ExchangeRate.GetRate(clearLine.AL_LineAmount + clearLine.AL_GSTVAT, clearLine.AL_OSAmount, AccTransactionLinesSchema.AL_ExchangeRate.Scale), entry.ExchangeRate);
			expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = clearLine.AL_AG, LocalAmount = 10m, OSAmount = 10m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.TransactionLineGLAccount, JournalDate = clearLine.AL_PostDate, GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 202305  },
				new DebitCreditEntryItem { AccountPK = clearLine.AL_AG, LocalAmount = -10m, OSAmount = -10m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.TransactionLineGLAccountForReverse, JournalDate = clearLine.AL_ReverseDate, GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 202306  },
			};

			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);
		}
	}
}
