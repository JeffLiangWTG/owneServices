using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing
{
	public class ARAPJNLGeneralLedgerDataLineCreatorTest : GeneralLedgerDataLineCreatorTest
	{
		public void TestCreateDRCRLines_ARJNL()
		{
			var aRJournal = TestObjectCreator.CreateJournal<ARJournal>(100m, new ZDateTime(2023, 5, 10), TestObjectCreator.Creditor1.PK);
			var creator = new ARAPJNLGeneralLedgerDataLineCreator();
			var entry = creator.CreateDRCREntries(((INeedRow)aRJournal).Row);

			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = aRJournal.AH_AG, LocalAmount = -100m, OSAmount = -100m, DRCRSign = DebitCredit.CR, GLDAccountType =  GLDAccountTypes.TransactionHeaderGLAccount, JournalDate = aRJournal.AH_PostDate, GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 0 },
				new DebitCreditEntryItem { AccountPK = GLControlAccounts.Instance.ARControlAccount.PK, LocalAmount = 100m, OSAmount = 100m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.ARControlAccount, JournalDate = aRJournal.AH_PostDate, GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 0 },
			};
			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);
		}

		public void TestCreateDRCRLines_APJNL()
		{
			var aPJournal = TestObjectCreator.CreateJournal<APJournal>(100m, new ZDateTime(2023, 5, 10), TestObjectCreator.Creditor1.PK);
			var creator = new ARAPJNLGeneralLedgerDataLineCreator();
			var entry = creator.CreateDRCREntries(((INeedRow)aPJournal).Row);

			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = aPJournal.AH_AG, LocalAmount = 100m, OSAmount = 100m, DRCRSign = DebitCredit.DR, GLDAccountType =  GLDAccountTypes.TransactionHeaderGLAccount, JournalDate = aPJournal.AH_PostDate, GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 0 },
				new DebitCreditEntryItem { AccountPK = GLControlAccounts.Instance.APControlAccount.PK, LocalAmount = -100m, OSAmount = -100m, DRCRSign = DebitCredit.CR, GLDAccountType =  GLDAccountTypes.APControlAccount, JournalDate = aPJournal.AH_PostDate, GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 0 },
			};
			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);
		}

		public override void TestHasValidControlAccount()
		{
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);

			var aRJournal = TestObjectCreator.CreateJournal<ARJournal>(100m, new ZDateTime(2023, 5, 10), TestObjectCreator.Creditor1.PK);
			var creator = new ARAPJNLGeneralLedgerDataLineCreator();

			var entry = new DebitCreditEntry { EntryItems = Array.Empty<DebitCreditEntryItem>() };
			AssertExceptionThrown<MissingGLHeaderException>(() =>
			{
				entry = creator.CreateDRCREntries(((INeedRow)aRJournal).Row);
			});

			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE1.PK.ToGuid());
			entry = creator.CreateDRCREntries(((INeedRow)aRJournal).Row);
			Assert(entry.EntryItems.Any());
		}
	}
}
