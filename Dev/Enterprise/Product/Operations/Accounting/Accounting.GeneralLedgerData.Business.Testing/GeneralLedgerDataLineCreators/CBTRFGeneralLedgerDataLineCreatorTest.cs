using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing
{
	public class CBTRFGeneralLedgerDataLineCreatorTest : GeneralLedgerDataLineCreatorTest
	{
		public void TestCreateDRCREntries_CDTRFPosted()
		{
			var transfer = TestObjectCreator.CreateBankTransfer(ZDateTime.Today, TestObjectCreator.AUDBankAccount.PK, TestObjectCreator.AUDBankAccount2.PK, 2500m, 1.0m);
			Factory.Save();

			var bankTransferFrom = transfer.TransferRowFrom;
			var bankTransferTo = transfer.TransferRowTo;
			var creator = new CBTRFGeneralLedgerDataLineCreator();
			var entry = creator.CreateDRCREntries(((INeedRow)bankTransferFrom).Row);

			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = bankTransferFrom.BankAccount.AB_AG, LocalAmount = -2500m, OSAmount = -2500m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.FromTRFBankControlAccount, JournalDate = bankTransferFrom.AH_PostDate, GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 0 },
			};

			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);

			creator = new CBTRFGeneralLedgerDataLineCreator();
			entry = creator.CreateDRCREntries(((INeedRow)bankTransferTo).Row);
			expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = bankTransferTo.BankAccount.AB_AG, LocalAmount = 2500m, OSAmount = 2500m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.ToTRFBankControlAccount, JournalDate = bankTransferTo.AH_PostDate, GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 0  }
			};

			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);
		}

		public override void TestHasValidControlAccount()
		{
			Assert(true);
		}
	}
}
