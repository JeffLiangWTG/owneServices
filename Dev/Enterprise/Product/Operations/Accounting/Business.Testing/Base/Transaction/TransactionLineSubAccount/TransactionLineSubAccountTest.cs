using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	public abstract class TransactionLineSubAccountTest : AccTransactionLineSubAccountTest
	{
		public void TestParent()
		{
			var subAccount1 = Factory.New<TransactionLineSubAccount>();
			AssertNull("AL1_AL is not valid", subAccount1.Parent);

			var line = CreateDependentTransactionLine();
			var subAccount2 = line.SubAccounts.AddNew();
			AssertEquals("JournalSubAccount Parent", line, subAccount2.Parent);
		}

		public void TestAL1_Calc_SubClassParent()
		{
			var line = CreateDependentTransactionLine();
			var subAccount = line.SubAccounts.AddNew();

			subAccount.AL1_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
			AssertEquals("AL1_SubAccountType should be 'Organization'", "Organization", subAccount.AL1_Calc_SubClassParent);

			subAccount.AL1_SubClassParentTableCode = AccGroupsSchema.Constants.Prefix;
			AssertEquals("AL1_SubAccountType should be 'Sales/Expense Groups'", "Sales/Expense Groups", subAccount.AL1_Calc_SubClassParent);

			subAccount.AL1_SubClassParentTableCode = GlbStaffSchema.Constants.Prefix;
			AssertEquals("AL1_SubAccountType should be 'Staff and Resources'", "Staff and Resources", subAccount.AL1_Calc_SubClassParent);

			subAccount.AL1_SubClassParentTableCode = GlbGroupSchema.Constants.Prefix;
			AssertEquals("AL1_SubAccountType should be 'Staff Group'", "Staff Group", subAccount.AL1_Calc_SubClassParent);
		}

		public void TestAL1_SubClassParentTableCode()
		{
			var glHeader = TestObjectCreator.CreateGLHeader();
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, OrgHeaderSchema.Constants.Prefix, false);
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, GlbStaffSchema.Constants.Prefix, false);
			var line = CreateDependentTransactionLine();
			line.AL_AG = glHeader.PK;
			var firstSubAccount = line.SubAccounts.FirstSubAccount;
			var secondSubAccount = line.SubAccounts.SecondSubAccount;

			AssertEquals("Pre-condition:first AL1_SubClassParentTableCode", OrgHeaderSchema.Constants.Prefix, firstSubAccount.AL1_SubClassParentTableCode);
			AssertEquals("Pre-condition:second AL1_SubClassParentTableCode", GlbStaffSchema.Constants.Prefix, secondSubAccount.AL1_SubClassParentTableCode);

			var firstSubClassParentTableCode = AccGroupsSchema.Constants.Prefix;
			var secondSubClassParentTableCode = GlbGroupSchema.Constants.Prefix;
			firstSubAccount.AL1_SubClassParentTableCode = firstSubClassParentTableCode;
			secondSubAccount.AL1_SubClassParentTableCode = secondSubClassParentTableCode;

			AssertEquals("first AL1_SubClassParentTableCode", firstSubClassParentTableCode, firstSubAccount.AL1_SubClassParentTableCode);
			AssertEquals("second AL1_SubClassParentTableCode", secondSubClassParentTableCode, secondSubAccount.AL1_SubClassParentTableCode);
		}

		public void TestAL1_SubClassParentId()
		{
			var glHeader = TestObjectCreator.CreateGLHeader();
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, OrgHeaderSchema.Constants.Prefix, false);
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, GlbStaffSchema.Constants.Prefix, false);
			var line = CreateDependentTransactionLine();
			line.AL_AG = glHeader.PK;

			var firstSubAccount = line.SubAccounts.FirstSubAccount;
			var secondSubAccount = line.SubAccounts.SecondSubAccount;

			AssertEquals("Pre-condition:first AL1_SubClassParentId", ZGuid.Empty, firstSubAccount.AL1_SubClassParentId);
			AssertEquals("Pre-condition:second AL1_SubClassParentId", ZGuid.Empty, secondSubAccount.AL1_SubClassParentId);

			var firstSubClassParentId = TestObjectCreator.ABIGAS.PK;
			var secondSubClassParentId = TestObjectCreator.CreateStaff("AAA").PK;

			firstSubAccount.AL1_SubClassParentId = firstSubClassParentId;
			secondSubAccount.AL1_SubClassParentId = secondSubClassParentId;

			AssertEquals("first AL1_SubClassParentId", firstSubClassParentId, firstSubAccount.AL1_SubClassParentId);
			AssertEquals("second AL1_SubClassParentId", secondSubClassParentId, secondSubAccount.AL1_SubClassParentId);
		}

		public void TestAL1_SubClassParentId_ReadOnly()
		{
			var line = CreateDependentTransactionLine();
			var subAccount = line.SubAccounts.AddNew();

			subAccount.AL1_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
			AssertEquals("AL1_SubClassParentIdInfo should not be readonly when AL1_SubClassParentTableCode is not empty", false, subAccount.AL1_SubClassParentIdInfo.ReadOnly);

			line.AL_JH = TestObjectCreator.Job1.PK;
			AssertEquals("AL1_SubClassParentIdInfo should be readonly when line has job.", true, subAccount.AL1_SubClassParentIdInfo.ReadOnly);

			AssertEquals("line IsMultiSubAccountsSupported shoule be true.", true, line.IsMultiSubAccountsSupported);
		}

		public void TestAL1_Calc_SubAccountDescription()
		{
			var line = CreateDependentTransactionLine();
			var transactionLineSubAccount = line.SubAccounts.AddNew();

			var orgHeader = TestObjectCreator.AALSHI;
			orgHeader.OH_FullName = "Test Company Name";
			transactionLineSubAccount.AL1_SubClassParentTableCode = AccountingMasterFilesConstants.SubAccountTypeList.Organization.Code;
			transactionLineSubAccount.AL1_SubClassParentId = orgHeader.PK;
			AssertEquals("organization name", "Test Company Name", transactionLineSubAccount.AL1_Calc_SubAccountDescription);

			var salesGroup = TestObjectCreator.CreateSalesGroup("TestSales");
			salesGroup.AR_Desc = "Test Sales Group Desc";
			transactionLineSubAccount.AL1_SubClassParentTableCode = AccountingMasterFilesConstants.SubAccountTypeList.SalesGroup.Code;
			transactionLineSubAccount.AL1_SubClassParentId = salesGroup.PK;
			AssertEquals("sales group name", "Test Sales Group Desc", transactionLineSubAccount.AL1_Calc_SubAccountDescription);

			var staff = TestObjectCreator.CreateStaff("AAA");
			staff.GS_FullName = "Test Staff Name";
			transactionLineSubAccount.AL1_SubClassParentTableCode = AccountingMasterFilesConstants.SubAccountTypeList.StaffAndResources.Code;
			transactionLineSubAccount.AL1_SubClassParentId = staff.PK;
			AssertEquals("staff name", "Test Staff Name", transactionLineSubAccount.AL1_Calc_SubAccountDescription);

			var group = TestObjectCreator.CreateStaffGroup("TestGroup");
			group.GG_Desc = "Test Group Name";
			transactionLineSubAccount.AL1_SubClassParentTableCode = AccountingMasterFilesConstants.SubAccountTypeList.StaffGroup.Code;
			transactionLineSubAccount.AL1_SubClassParentId = group.PK;
			AssertEquals("group name", "Test Group Name", transactionLineSubAccount.AL1_Calc_SubAccountDescription);
		}

		public void TestAL1_Calc_Sequence()
		{
			var line = CreateDependentTransactionLine();

			var subAccount1 = line.SubAccounts.AddNew();
			var subAccount2 = line.SubAccounts.AddNew();
			var subAccount3 = line.SubAccounts.AddNew();
			var subAccount4 = line.SubAccounts.AddNew();

			subAccount1.AL1_SubClassParentTableCode = GlbGroupSchema.Constants.Prefix;
			subAccount2.AL1_SubClassParentTableCode = GlbStaffSchema.Constants.Prefix;
			subAccount3.AL1_SubClassParentTableCode = AccGroupsSchema.Constants.Prefix;
			subAccount4.AL1_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;

			AssertEquals("The first sequence should be 4", 4, subAccount1.AL1_Calc_Sequence);
			AssertEquals("The second sequence should be 3", 3, subAccount2.AL1_Calc_Sequence);
			AssertEquals("The third sequence should be 2", 2, subAccount3.AL1_Calc_Sequence);
			AssertEquals("The fourth sequence should be 1", 1, subAccount4.AL1_Calc_Sequence);
		}

		public void TestIsInDatabase()
		{
			var line = CreateDependentTransactionLine();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			Factory.Save();
			AssertEquals("Pre-condition", true, line.IsInDatabase);

			var subAccount = line.SubAccounts.AddNew();
			AssertEquals("sub account has not changed", true, subAccount.IsInDatabase);

			subAccount.AL1_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
			AssertEquals("sub account has changed", false, subAccount.IsInDatabase);

			var line2 = CreateDependentTransactionLine();
			var subAccount2 = line2.SubAccounts.AddNew();
			AssertEquals("line2 IsInDatabase", false, line2.IsInDatabase);
			AssertEquals("sub account 2 IsInDatabase", false, subAccount2.IsInDatabase);
		}

		public void TestIsValidationEnabled()
		{
			var subAccount1 = (TransactionLineSubAccount)Factory.New(GetExpectedBusinessObjectType());
			AssertEquals("Pre-condition", true, subAccount1.IsValidationEnabled(subAccount1.AL1_SubClassParentIdInfo));

			var line = CreateDependentTransactionLine();
			var subAccount2 = line.SubAccounts.AddNew();
			var propertyInto = subAccount2.AL1_SubClassParentIdInfo;
			AssertEquals("Pre-condition", false, line.ReadOnly);
			AssertEquals("Pre-condition", true, subAccount2.IsValidationEnabled(propertyInto));

			line.ReadOnly = true;
			AssertEquals("sub account 2 is validation enabled", false, subAccount2.IsValidationEnabled(propertyInto));
		}

		public void TestDeleteDataWhenSubClassParentIdIsEmptyDataOnSaving()
		{
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, OrgHeaderSchema.Constants.Prefix, false);
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, AccGroupsSchema.Constants.Prefix, false);
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, GlbStaffSchema.Constants.Prefix, false);
			var line = CreateDependentTransactionLine();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;

			var staffGroupPK = TestObjectCreator.CreateStaffGroup("AAA").PK;
			var salesGroupPK = TestObjectCreator.CreateSalesGroup("TestSales").PK;

			var firstSubAccount = line.SubAccounts.Cast<TransactionLineSubAccount>().OrderBy(x => x.AL1_Calc_Sequence).FirstOrDefault();
			var secondSubAccount = line.SubAccounts.Cast<TransactionLineSubAccount>().OrderBy(x => x.AL1_Calc_Sequence).Skip(1).FirstOrDefault();
			var thirdSubAccount = line.SubAccounts.Cast<TransactionLineSubAccount>().OrderBy(x => x.AL1_Calc_Sequence).Skip(2).FirstOrDefault();

			secondSubAccount.AL1_SubClassParentId = staffGroupPK;
			thirdSubAccount.AL1_SubClassParentId = salesGroupPK;

			AssertEquals("Pre-condition", line.SubAccounts.FirstSubAccount, firstSubAccount);
			AssertEquals("Pre-condition", line.SubAccounts.SecondSubAccount, secondSubAccount);
			AssertEquals("Pre-condition", true, IsHasSubAccount(line, thirdSubAccount));

			Factory.Save();

			AssertEquals("old first sub acount has been deleled", false, IsHasSubAccount(line, firstSubAccount));
			AssertEquals("old second sub account will be changed to new first sub acount", line.SubAccounts.FirstSubAccount, secondSubAccount);
			AssertEquals("old third sub account will be changed to new sedond sub acount", line.SubAccounts.SecondSubAccount, thirdSubAccount);

			bool IsHasSubAccount(DependentTransactionLine sourceLine, TransactionLineSubAccount subAccount)
			{
				return sourceLine.SubAccounts.Cast<TransactionLineSubAccount>().Any(x => x == subAccount);
			}
		}

		public void TestLookups()
		{
			var transactionLineSubAccount = Factory.New<TransactionLineSubAccount>();
			AssertType<TransactionLineSubAccountLookups>(transactionLineSubAccount.Lookups);
		}

		public void TestValidation()
		{
			var transactionLineSubAccount = Factory.New<TransactionLineSubAccount>();
			AssertType<TransactionLineSubAccountValidation>(transactionLineSubAccount.Validation);
		}

		public virtual void TestCopyWithMultipleSubAccounts()
		{
			var line = SetUpSubAccountsForReverseOrCopy();
			var copyOfTransaction = line.MasterTransactionHeader.TemplateCopy();
			AssertSubAccountsForCopy((TransactionHeaderWithLines)copyOfTransaction);
		}

		public void TestReverseWithMultipleSubAccounts()
		{
			var line = SetUpSubAccountsForReverseOrCopy();
			var originalTransaction = line.MasterTransactionHeader as IReversing;
			new ReversingBase(originalTransaction).Reverse();
			AssertSubAccountsForReverse((TransactionHeaderWithLines)originalTransaction.ReverseTransaction);
		}

		[ExpectNoExceptions]
		public void TestDelete()
		{
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, OrgHeaderSchema.Constants.Prefix, false);
			var line = CreateDependentTransactionLine();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_Calc_FirstSubClassParentId = TestObjectCreator.ABIGAS.PK;
			line.Delete();

			Factory.Save();
		}

		DependentTransactionLine SetUpSubAccountsForReverseOrCopy()
		{
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, OrgHeaderSchema.Constants.Prefix, false);
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, AccGroupsSchema.Constants.Prefix, false);
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, GlbStaffSchema.Constants.Prefix, true);
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, GlbGroupSchema.Constants.Prefix, false);

			var line = CreateDependentTransactionLine();
			var lineBase = line as InvoicingLineBase;
			if (lineBase != null)
			{
				lineBase.GenericCharge = TestObjectCreator.GLHeader1.PK;
			}
			else
			{
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
			}

			line.SubAccounts.SubAccountElements.First(x => x.SubAccountTypeParentTableCode == OrgHeaderSchema.Constants.Prefix).SubAccountParentId = TestObjectCreator.ABIGAS.PK;
			line.SubAccounts.SubAccountElements.First(x => x.SubAccountTypeParentTableCode == AccGroupsSchema.Constants.Prefix).SubAccountParentId = TestObjectCreator.AR1.PK;
			line.SubAccounts.SubAccountElements.First(x => x.SubAccountTypeParentTableCode == GlbGroupSchema.Constants.Prefix).SubAccountParentId = TestObjectCreator.GG1.PK;
			Factory.Save();

			return line;
		}

		protected void AssertSubAccountsWithLineData_ShouldNotContainAllGLHeaderSubAccounts(TransactionHeaderWithLines reverseTransaction)
		{
			var subAccounts = reverseTransaction.Lines[0].SubAccounts;
			AssertNotNull(reverseTransaction);
			AssertEquals(1, reverseTransaction.Lines.Count);

			AssertEquals(3, subAccounts.Count);
			Assert(subAccounts.SubAccountElements.Any(x => x.SubAccountTypeParentTableCode == OrgHeaderSchema.Constants.Prefix && x.SubAccountParentId == TestObjectCreator.ABIGAS.PK));
			Assert(subAccounts.SubAccountElements.Any(x => x.SubAccountTypeParentTableCode == AccGroupsSchema.Constants.Prefix && x.SubAccountParentId == TestObjectCreator.AR1.PK));
			Assert(subAccounts.SubAccountElements.Any(x => x.SubAccountTypeParentTableCode == GlbGroupSchema.Constants.Prefix && x.SubAccountParentId == TestObjectCreator.GG1.PK));

			reverseTransaction.RunPreSaveValidation();
			Assert(!reverseTransaction.NotificationsIncludingChildren.Contains("Error - AL1_SubClassParentId: Please enter a Sub Account."));
		}

		protected void AssertSubAccountsWithLineData(TransactionHeaderWithLines transaction)
		{
			var subAccounts = transaction.Lines[0].SubAccounts;
			AssertNotNull(transaction);
			AssertEquals(1, transaction.Lines.Count);

			AssertEquals(4, subAccounts.Count);
			Assert(subAccounts.SubAccountElements.Any(x => x.SubAccountTypeParentTableCode == OrgHeaderSchema.Constants.Prefix && x.SubAccountParentId == TestObjectCreator.ABIGAS.PK));
			Assert(subAccounts.SubAccountElements.Any(x => x.SubAccountTypeParentTableCode == AccGroupsSchema.Constants.Prefix && x.SubAccountParentId == TestObjectCreator.AR1.PK));
			Assert(subAccounts.SubAccountElements.Any(x => x.SubAccountTypeParentTableCode == GlbGroupSchema.Constants.Prefix && x.SubAccountParentId == TestObjectCreator.GG1.PK));
			Assert(subAccounts.SubAccountElements.Any(x => x.SubAccountTypeParentTableCode == GlbStaffSchema.Constants.Prefix && x.SubAccountParentId == ZGuid.Empty));

			transaction.RunPreSaveValidation();
			Assert(transaction.NotificationsIncludingChildren.Contains("Error - AL1_SubClassParentId: Please enter a Sub Account."));

			subAccounts.SubAccountElements.First(x => x.SubAccountTypeParentTableCode == GlbStaffSchema.Constants.Prefix).SubAccountParentId = TestObjectCreator.GS1.PK;
			transaction.RunPreSaveValidation();
			Assert(!transaction.NotificationsIncludingChildren.Contains("Error - AL1_SubClassParentId: Please enter a Sub Account."));
		}

		protected virtual void AssertSubAccountsForReverse(TransactionHeaderWithLines reverseTransaction)
		{
			AssertSubAccountsWithLineData_ShouldNotContainAllGLHeaderSubAccounts(reverseTransaction);
		}

		protected virtual void AssertSubAccountsForCopy(TransactionHeaderWithLines copyTransaction)
		{
			AssertEquals(0, copyTransaction.Lines.Count);
		}

		protected abstract Type GetExpectedTransactionHeader();

		DependentTransactionLine CreateDependentTransactionLine()
		{
			var header = (TransactionHeaderWithLines)Factory.New(GetExpectedTransactionHeader());
			header.FillWithValidTestData();

			var line = header.Lines.AddNew();
			line.FillWithValidTestData();
			return line;
		}

		TransactionLineSubAccount CreateSubAccount()
		{
			var line = CreateDependentTransactionLine();
			var subAccount = line.SubAccounts.AddNew();
			return subAccount;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var transactionLineSubAccount = CreateSubAccount();
			transactionLineSubAccount.AL1_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
			transactionLineSubAccount.AL1_SubClassParentId = TestObjectCreator.Creditor1.PK;
			return transactionLineSubAccount;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return CreateSubAccount();
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
		}
		TestObjectCreator TestObjectCreator;

		[TestedType(typeof(TransactionLineSubAccount))]
		public class ARInvoiceTransactionLineSubAccountTest : TransactionLineSubAccountTest
		{
			protected override BusinessObject GetNewBusinessObject() => GetBusinessObjectForFetchForLoad();

			protected override Type GetExpectedTransactionHeader() => typeof(ARInvoice);

			protected override void AssertSubAccountsForCopy(TransactionHeaderWithLines copyTransaction)
			{
				AssertSubAccountsWithLineData(copyTransaction);
			}
		}

		[TestedType(typeof(TransactionLineSubAccount))]
		public class APInvoiceTransactionLineSubAccountTest : TransactionLineSubAccountTest
		{
			protected override BusinessObject GetNewBusinessObject() => GetBusinessObjectForFetchForLoad();

			protected override Type GetExpectedTransactionHeader() => typeof(APInvoice);

			protected override void AssertSubAccountsForCopy(TransactionHeaderWithLines copyTransaction)
			{
				AssertSubAccountsWithLineData(copyTransaction);
			}
		}

		[TestedType(typeof(TransactionLineSubAccount))]
		public class ARAdjustmentNoteTransactionLineSubAccountTest : TransactionLineSubAccountTest
		{
			protected override BusinessObject GetNewBusinessObject() => GetBusinessObjectForFetchForLoad();

			protected override Type GetExpectedTransactionHeader() => typeof(ARAdjustmentNote);
		}

		[TestedType(typeof(TransactionLineSubAccount))]
		public class APAdjustmentNoteTransactionLineSubAccountTest : TransactionLineSubAccountTest
		{
			protected override BusinessObject GetNewBusinessObject() => GetBusinessObjectForFetchForLoad();

			protected override Type GetExpectedTransactionHeader() => typeof(APAdjustmentNote);
		}

		[TestedType(typeof(TransactionLineSubAccount))]
		public class ARCreditNoteTransactionLineSubAccountTest : TransactionLineSubAccountTest
		{
			protected override BusinessObject GetNewBusinessObject() => GetBusinessObjectForFetchForLoad();

			protected override Type GetExpectedTransactionHeader() => typeof(ARCreditNote);
		}

		[TestedType(typeof(TransactionLineSubAccount))]
		public class APCreditNoteTransactionLineSubAccountTest : TransactionLineSubAccountTest
		{
			protected override BusinessObject GetNewBusinessObject() => GetBusinessObjectForFetchForLoad();

			protected override Type GetExpectedTransactionHeader() => typeof(APCreditNote);
		}

		[TestedType(typeof(TransactionLineSubAccount))]
		public class UAInvoiceTransactionLineSubAccountTest : TransactionLineSubAccountTest
		{
			protected override BusinessObject GetNewBusinessObject() => GetBusinessObjectForFetchForLoad();

			protected override Type GetExpectedTransactionHeader() => typeof(UAInvoice);

			protected override void AssertSubAccountsForCopy(TransactionHeaderWithLines copyTransaction)
			{
				AssertSubAccountsWithLineData(copyTransaction);
			}

			protected override void AssertSubAccountsForReverse(TransactionHeaderWithLines reverseTransaction)
			{
				AssertNull(reverseTransaction);
			}
		}

		[TestedType(typeof(TransactionLineSubAccount))]
		public class UACreditNoteTransactionLineSubAccountTest : TransactionLineSubAccountTest
		{
			protected override BusinessObject GetNewBusinessObject() => GetBusinessObjectForFetchForLoad();

			protected override Type GetExpectedTransactionHeader() => typeof(UACreditNote);

			protected override void AssertSubAccountsForReverse(TransactionHeaderWithLines reverseTransaction)
			{
				AssertNull(reverseTransaction);
			}
		}

		[TestedType(typeof(TransactionLineSubAccount))]
		public class DirectReceiptTransactionLineSubAccountTest : TransactionLineSubAccountTest
		{
			protected override BusinessObject GetNewBusinessObject() => GetBusinessObjectForFetchForLoad();

			protected override Type GetExpectedTransactionHeader() => typeof(DirectReceipt);

			public override void TestCopyWithMultipleSubAccounts()
			{
				//Arrange
				var line = SetUpSubAccountsForReverseOrCopy();

				//Act
				var copyOfTransaction = (TransactionHeaderWithLines)line.MasterTransactionHeader.TemplateCopy();

				//Assert
				AssertEquals(1, copyOfTransaction.Lines.Count);

				var copyLine = copyOfTransaction.Lines[0];
				AssertEquals(4, copyLine.SubAccounts.Count);
				AssertEquals(true, copyLine.SubAccounts.SubAccountElements.All(x => x.SubAccountParentId == ZGuid.Empty));
			}
		}

		[TestedType(typeof(TransactionLineSubAccount))]
		public class DirectPaymentTransactionLineSubAccountTest : TransactionLineSubAccountTest
		{
			protected override BusinessObject GetNewBusinessObject() => GetBusinessObjectForFetchForLoad();

			protected override Type GetExpectedTransactionHeader() => typeof(DirectPayment);

			public override void TestCopyWithMultipleSubAccounts()
			{
				//Arrange
				var line = SetUpSubAccountsForReverseOrCopy();
				
				//Act
				var copyOfTransaction = (TransactionHeaderWithLines)line.MasterTransactionHeader.TemplateCopy();

				//Assert
				AssertEquals(1, copyOfTransaction.Lines.Count);

				var copyLine = copyOfTransaction.Lines[0];
				AssertEquals(4, copyLine.SubAccounts.Count);
				AssertEquals(true, copyLine.SubAccounts.SubAccountElements.All(x => x.SubAccountParentId == ZGuid.Empty));
			}
		}

		[TestedType(typeof(TransactionLineSubAccount))]
		public class GLJournalTransactionLineSubAccountTest : TransactionLineSubAccountTest
		{
			protected override BusinessObject GetNewBusinessObject() => GetBusinessObjectForFetchForLoad();

			protected override Type GetExpectedTransactionHeader() => typeof(GLJournal);

			protected override void AssertSubAccountsForReverse(TransactionHeaderWithLines reverseTransaction)
			{
				reverseTransaction.Lines.SetReadOnlyIncludingChildren(false);
				AssertSubAccountsWithLineData(reverseTransaction);
			}

			protected override void AssertSubAccountsForCopy(TransactionHeaderWithLines copyTransaction)
			{
				AssertSubAccountsWithLineData(copyTransaction);
			}
		}

		[TestedType(typeof(TransactionLineSubAccount))]
		public class FCBAdjustmentJournalTransactionLineSubAccountTest : TransactionLineSubAccountTest
		{
			protected override BusinessObject GetNewBusinessObject() => GetBusinessObjectForFetchForLoad();

			protected override Type GetExpectedTransactionHeader() => typeof(FCBAdjustmentJournal);

			protected override void AssertSubAccountsForCopy(TransactionHeaderWithLines copyTransaction)
			{
				AssertSubAccountsWithLineData(copyTransaction);
			}

			protected override void AssertSubAccountsForReverse(TransactionHeaderWithLines reverseTransaction)
			{
				reverseTransaction.Lines.SetReadOnlyIncludingChildren(false);
				AssertSubAccountsWithLineData(reverseTransaction);
			}
		}
	}
}
