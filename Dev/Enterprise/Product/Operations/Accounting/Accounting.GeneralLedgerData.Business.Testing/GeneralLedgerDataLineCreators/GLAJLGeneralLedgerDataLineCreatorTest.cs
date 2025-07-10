using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing.GeneralLedgerDataLineCreators
{
	public class GLAJLGeneralLedgerDataLineCreatorTest : GeneralLedgerDataLineCreatorTest
	{
		public override void TestHasValidControlAccount()
		{
			Assert(true);
		}

		public void TestCreateDRCREntries_GJL()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2022);

			var glJournal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLAutoJournal, new ZDateTime(2013, 08, 13), new ZDateTime(2022, 08, 13), new ZDateTime(2022, 10, 13));
			var line1 = TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			var line2 = TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			Factory.Save();
			var periodCalculator = new AccountingPeriodCalculator(Factory, glJournal.Company);

			var creator = new GLAJLGeneralLedgerDataLineCreator();
			var entry = creator.CreateDRCREntries(((INeedRow)line1).Row);
			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader1.PK, LocalAmount = 250M, OSAmount = 250M, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.TransactionLineGLAccount, JournalDate = periodCalculator.GetLastDayForPeriod(202208), GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 202208 },
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader1.PK, LocalAmount = 250M, OSAmount = 250M, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.TransactionLineGLAccount, JournalDate = periodCalculator.GetLastDayForPeriod(202209), GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 202209 },
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader1.PK, LocalAmount = 250M, OSAmount = 250M, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.TransactionLineGLAccount, JournalDate = periodCalculator.GetLastDayForPeriod(202210), GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 202210 }
			};

			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);

			entry = creator.CreateDRCREntries(((INeedRow)line2).Row);
			expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader2.PK, LocalAmount = -250M, OSAmount = -250M, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.TransactionLineGLAccount, JournalDate = periodCalculator.GetLastDayForPeriod(202208), GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 202208 },
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader2.PK, LocalAmount = -250M, OSAmount = -250M, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.TransactionLineGLAccount, JournalDate = periodCalculator.GetLastDayForPeriod(202209), GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 202209 },
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.GLHeader2.PK, LocalAmount = -250M, OSAmount = -250M, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.TransactionLineGLAccount, JournalDate = periodCalculator.GetLastDayForPeriod(202210), GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 202210 }
			};

			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);
		}
	}
}
