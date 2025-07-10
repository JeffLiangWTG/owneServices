using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing
{
	public class CBDRCDPYGeneralLedgerDataLineCreatorTest : GeneralLedgerDataLineCreatorTest
	{
		public override void TestHasValidControlAccount()
		{
			var directPayment = TestObjectCreator.CreateDirectPayment(new DateTime(2023, 05, 12), 12m, 2m, 10, 2m);
			var directReceipt = TestObjectCreator.CreateDirectReceipt(new DateTime(2023, 05, 12), 12m, 2m, 10, 2m);
			Factory.Save();

			AssertValidControlAccount(directReceipt, AccountingConfigurationRegistry.Instance.GSTOutputControlAccount);
			AssertValidControlAccount(directPayment, AccountingConfigurationRegistry.Instance.GSTInputControlAccount);

			void AssertValidControlAccount(DirectTransactionHeaderBase headerBase, GuidRegistryItem registryItem)
			{
				var creator = new CBDRCDPYGeneralLedgerDataLineCreator();
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);

				var entry = new DebitCreditEntry { EntryItems = Array.Empty<DebitCreditEntryItem>() };
				AssertExceptionThrown<MissingGLHeaderException>(() =>
				{
					entry = creator.CreateDRCREntries(((INeedRow)headerBase.Lines[0]).Row);
				});
				Assert(!entry.EntryItems.Any());

				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
				AssertNoExceptionThrown(() =>
				{
					entry = creator.CreateDRCREntries(((INeedRow)headerBase.Lines[0]).Row);
				});
				Assert(entry.EntryItems.Any());
			}
		}

		public void TestCreateDRCRLines_DRC()
		{
			var headerBranch = TestObjectCreator.NonCurrentBranch.PK;
			var headerDepartment = TestObjectCreator.NonCurrentDepartment.PK;
			var lineBranch = GlbBranch.CurrentBranch.PK;
			var lineDepartment = GlbDepartment.CurrentDepartment.PK;
			var postDate = new ZDateTime(2023, 5, 10);
			var directReceipt = TestObjectCreator.CreateDirectReceipt(postDate, 10m, 2m, 10, 2m);
			directReceipt.AH_GB = headerBranch;
			directReceipt.AH_GE = headerDepartment;
			var directReceiptLine = directReceipt.Lines[0];
			directReceiptLine.AL_GB = lineBranch;
			directReceiptLine.AL_GE = lineDepartment;
			Factory.Save();

			var creator = new CBDRCDPYGeneralLedgerDataLineCreator();
			var entry = creator.CreateDRCREntries(((INeedRow)directReceipt.Lines[0]).Row);

			var controlAccount = AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value;
			var glHeaderOfLine = directReceipt.Lines[0].AL_AG.ToGuid();
			var glHeaderOfBankAccount = directReceipt.BankAccount.AB_AG.ToGuid();
			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = glHeaderOfBankAccount, LocalAmount = 2m, OSAmount = 2m, DRCRSign = DebitCredit.DR, GLDAccountType =  GLDAccountTypes.TransactionBankGLAccountForGST, JournalDate = postDate },
				new DebitCreditEntryItem { AccountPK = controlAccount, LocalAmount = -2m, OSAmount = -2m, DRCRSign = DebitCredit.CR, GLDAccountType =  GLDAccountTypes.GSTOutputControlAccount, JournalDate = postDate },
				new DebitCreditEntryItem { AccountPK = glHeaderOfBankAccount, LocalAmount = 10m, OSAmount = 10m, DRCRSign = DebitCredit.DR, GLDAccountType =  GLDAccountTypes.TransactionBankGLAccount, JournalDate = postDate },
				new DebitCreditEntryItem { AccountPK = glHeaderOfLine, LocalAmount = -10m, OSAmount = -10m, DRCRSign = DebitCredit.CR, GLDAccountType =  GLDAccountTypes.TransactionLineGLAccount, JournalDate = postDate },
			};
			foreach (var line in expectedDRCRLines)
			{
				line.JournalDate = directReceipt.Lines[0].AL_PostDate;
				line.GLDType = AccountingConstants.GLDTypeCodes.Posting;
				line.Period = 0;
			}

			Assert(entry.EntryItems.Where(x => x.AccountPK == glHeaderOfBankAccount).All(y => y.BranchPK == headerBranch && y.DepartmentPK == headerDepartment));
			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);
		}

		public void TestCreateDRCRLines_DPY()
		{
			var headerBranch = TestObjectCreator.NonCurrentBranch.PK;
			var headerDepartment = TestObjectCreator.NonCurrentDepartment.PK;
			var lineBranch = GlbBranch.CurrentBranch.PK;
			var lineDepartment = GlbDepartment.CurrentDepartment.PK;
			var postDate = new ZDateTime(2023, 5, 10);
			var directPayment = TestObjectCreator.CreateDirectPayment(postDate, 100m, 2m, 100m, 2m);
			directPayment.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			directPayment.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			directPayment.Lines[0].AL_InputGSTVATRecoverable = 0.5m;
			directPayment.AH_GB = headerBranch;
			directPayment.AH_GE = headerDepartment;
			var directPaymentLine = directPayment.Lines[0];
			directPaymentLine.AL_GB = lineBranch;
			directPaymentLine.AL_GE = lineDepartment;
			Factory.Save();

			var creator = new CBDRCDPYGeneralLedgerDataLineCreator();
			var entry = creator.CreateDRCREntries(((INeedRow)directPayment.Lines[0]).Row);

			var controlAccount = AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value;
			var glHeaderOfLine = directPayment.Lines[0].AL_AG.ToGuid();
			var glHeaderOfBankAccount = directPayment.BankAccount.AB_AG.ToGuid();

			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = glHeaderOfBankAccount, LocalAmount = -2m, OSAmount = -2m, DRCRSign = DebitCredit.CR, GLDAccountType =  GLDAccountTypes.TransactionBankGLAccountForGST, JournalDate = postDate },
				new DebitCreditEntryItem { AccountPK = controlAccount, LocalAmount = 1m, OSAmount = 1m, DRCRSign = DebitCredit.DR, GLDAccountType =  GLDAccountTypes.GSTInputControlAccount, JournalDate = postDate },
				new DebitCreditEntryItem { AccountPK = glHeaderOfLine, LocalAmount = 1m, OSAmount = 1m, DRCRSign = DebitCredit.DR, GLDAccountType =  GLDAccountTypes.GSTRecoverableTransactionLineGLAccount, JournalDate = postDate },
				new DebitCreditEntryItem { AccountPK = glHeaderOfBankAccount, LocalAmount = -100m, OSAmount = -100m, DRCRSign = DebitCredit.CR, GLDAccountType =  GLDAccountTypes.TransactionBankGLAccount, JournalDate = postDate },
				new DebitCreditEntryItem { AccountPK = glHeaderOfLine, LocalAmount = 100m, OSAmount = 100m, DRCRSign = DebitCredit.DR, GLDAccountType =  GLDAccountTypes.TransactionLineGLAccount, JournalDate = postDate },
			};
			foreach (var line in expectedDRCRLines)
			{
				line.JournalDate = directPayment.Lines[0].AL_PostDate;
				line.GLDType = AccountingConstants.GLDTypeCodes.Posting;
				line.Period = 0;
			}

			Assert(entry.EntryItems.Where(x => x.AccountPK == glHeaderOfBankAccount).All(y => y.BranchPK == headerBranch && y.DepartmentPK == headerDepartment));
			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);

			creator = new CBDRCDPYGeneralLedgerDataLineCreator();
			entry = creator.CreateDRCREntries(((INeedRow)directPayment.Lines[1]).Row);
			glHeaderOfLine = directPayment.Lines[1].AL_AG.ToGuid();
			expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = glHeaderOfBankAccount, LocalAmount = -2m, OSAmount = -2m, DRCRSign = DebitCredit.CR, GLDAccountType =  GLDAccountTypes.TransactionBankGLAccountForGST, JournalDate = postDate },
				new DebitCreditEntryItem { AccountPK = controlAccount, LocalAmount = 2m, OSAmount = 2m, DRCRSign = DebitCredit.DR, GLDAccountType =  GLDAccountTypes.GSTInputControlAccount, JournalDate = postDate },
				new DebitCreditEntryItem { AccountPK = glHeaderOfBankAccount, LocalAmount = -100m, OSAmount = -100m, DRCRSign = DebitCredit.CR, GLDAccountType =  GLDAccountTypes.TransactionBankGLAccount, JournalDate = postDate },
				new DebitCreditEntryItem { AccountPK = glHeaderOfLine, LocalAmount = 100m, OSAmount = 100m, DRCRSign = DebitCredit.DR, GLDAccountType =  GLDAccountTypes.TransactionLineGLAccount, JournalDate = postDate },
			};
			foreach (var line in expectedDRCRLines)
			{
				line.JournalDate = directPayment.Lines[1].AL_PostDate;
				line.GLDType = AccountingConstants.GLDTypeCodes.Posting;
				line.Period = 0;
			}
			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);
		}
	}
}
