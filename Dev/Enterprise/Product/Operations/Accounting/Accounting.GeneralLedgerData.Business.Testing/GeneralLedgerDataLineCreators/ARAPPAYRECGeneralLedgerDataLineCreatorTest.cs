using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing
{
	public class ARAPPAYRECGeneralLedgerDataLineCreatorTest : GeneralLedgerDataLineCreatorTest
	{
		public override void TestHasValidControlAccount()
		{
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);

			var payment = TestObjectCreator.CreateAPPayment(0.7m, 10m, new DateTime(2023, 5, 10), new DateTime(2023, 5, 10), TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);

			var creator = new ARAPPAYRECGeneralLedgerDataLineCreator();

			var entry = new DebitCreditEntry { EntryItems = Array.Empty<DebitCreditEntryItem>() };
			AssertExceptionThrown<MissingGLHeaderException>(() =>
			{
				entry = creator.CreateDRCREntries(((INeedRow)payment).Row);
			});
			Assert(!entry.EntryItems.Any());

			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE1.PK.ToGuid());
			AssertExceptionThrown<MissingGLHeaderException>(() =>
			{
				entry = creator.CreateDRCREntries(((INeedRow)payment).Row);
			});
			Assert(!entry.EntryItems.Any());

			AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE2.PK.ToGuid());
			entry = creator.CreateDRCREntries(((INeedRow)payment).Row);
			Assert(entry.EntryItems.Any());
		}

		public void TestCreateDRCRLines_APPAY_EmptyAB()
		{
			var postDate = new ZDateTime(2023, 5, 10);
			var payment = TestObjectCreator.CreateAPPayment(1m, 10m, postDate, postDate, TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);
			payment.AH_AB = Guid.Empty;
			var controlAccount = AccountingConfigurationRegistry.Instance.APControlAccount.Value;
			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = controlAccount, LocalAmount = 10m, OSAmount = 10m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.APControlAccount, JournalDate = postDate },
				new DebitCreditEntryItem { AccountPK = Guid.Empty, LocalAmount = -10m, OSAmount = -10m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.TransactionBankGLAccount, JournalDate = postDate }
			};

			AssertPAYRECInfo(payment, expectedDRCRLines);
		}

		public void TestCreateDRCRLines_APPAY()
		{
			var postDate = new ZDateTime(2023, 5, 10);
			var payment = TestObjectCreator.CreateAPPayment(1m, 10m, postDate, postDate, TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);
			var controlAccount = AccountingConfigurationRegistry.Instance.APControlAccount.Value;
			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = controlAccount, LocalAmount = 10m, OSAmount = 10m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.APControlAccount, JournalDate = postDate },
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.AUDBankAccount.AB_AG, LocalAmount = -10m, OSAmount = -10m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.TransactionBankGLAccount, JournalDate = postDate }
			};

			AssertPAYRECInfo(payment, expectedDRCRLines);
		}

		public void TestCreateDRCRLines_APREC()
		{
			var postDate = new ZDateTime(2023, 5, 10);
			var receipt = TestObjectCreator.CreateAPReceipt(1m, 10m, postDate, postDate, TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);

			var controlAccount = AccountingConfigurationRegistry.Instance.APControlAccount.Value;
			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = controlAccount, LocalAmount = -10m, OSAmount = -10m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.APControlAccount, JournalDate = postDate },
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.AUDBankAccount.AB_AG, LocalAmount = 10m, OSAmount = 10m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.TransactionBankGLAccount, JournalDate = postDate }
			};

			AssertPAYRECInfo(receipt, expectedDRCRLines);
		}

		public void TestCreateDRCRLines_ARREC()
		{
			var postDate = new ZDateTime(2023, 5, 10);
			var receipt = TestObjectCreator.CreateARReceipt(1m, 10m, postDate, postDate, TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);

			var controlAccount = AccountingConfigurationRegistry.Instance.ARControlAccount.Value;
			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = controlAccount, LocalAmount = -10m, OSAmount = -10m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.ARControlAccount, JournalDate = postDate },
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.AUDBankAccount.AB_AG, LocalAmount = 10m, OSAmount = 10m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.TransactionBankGLAccount, JournalDate = postDate }
			};

			AssertPAYRECInfo(receipt, expectedDRCRLines);
		}

		public void TestCreateDRCRLines_ARPAY()
		{
			var postDate = new DateTime(2023, 5, 10);
			var receipt = TestObjectCreator.CreateARPayment(1m, 10m, postDate, postDate, TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);

			var controlAccount = AccountingConfigurationRegistry.Instance.ARControlAccount.Value;
			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = TestObjectCreator.AUDBankAccount.AB_AG, LocalAmount = -10m, OSAmount = -10m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.TransactionBankGLAccount, JournalDate = postDate },
				new DebitCreditEntryItem { AccountPK = controlAccount, LocalAmount = 10m, OSAmount = 10m, DRCRSign = DebitCredit.DR, GLDAccountType = GLDAccountTypes.ARControlAccount , JournalDate = postDate }
			};

			AssertPAYRECInfo(receipt, expectedDRCRLines);
		}

		void AssertPAYRECInfo(AccTransactionHeader header, DebitCreditEntryItem[] expectedDRCRLines)
		{
			var creator = new ARAPPAYRECGeneralLedgerDataLineCreator();
			Factory.Save();

			foreach (var line in expectedDRCRLines)
			{
				line.JournalDate = header.AH_PostDate;
				line.GLDType = AccountingConstants.GLDTypeCodes.Posting;
				line.Period = 0;
			}

			var entry = creator.CreateDRCREntries(((INeedRow)header).Row);
			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);
		}
	}
}
