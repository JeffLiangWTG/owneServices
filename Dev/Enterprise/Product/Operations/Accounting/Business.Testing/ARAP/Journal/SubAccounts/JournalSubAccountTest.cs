using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	public abstract class JournalSubAccountTest : AccTransactionHeaderSubAccountTest
	{
		public void TestParent()
		{
			var subAccount1 = Factory.New<JournalSubAccount>();
			AssertNull("AHS_AH is not valid", subAccount1.Parent);

			var journal = CreateJournalCore();
			var subAccount2 = journal.SubAccounts.AddNew();
			AssertEquals("JournalSubAccount Parent", journal, subAccount2.Parent);
		}

		public void TestAHS_Calc_SubClassParent()
		{
			var journal = CreateJournalCore();
			var subAccount = journal.SubAccounts.AddNew();

			subAccount.AHS_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
			AssertEquals("AHS_SubAccountType should be 'Organization'", "Organization", subAccount.AHS_Calc_SubClassParent);

			subAccount.AHS_SubClassParentTableCode = AccGroupsSchema.Constants.Prefix;
			AssertEquals("AHS_SubAccountType should be 'Sales/Expense Groups'", "Sales/Expense Groups", subAccount.AHS_Calc_SubClassParent);

			subAccount.AHS_SubClassParentTableCode = GlbStaffSchema.Constants.Prefix;
			AssertEquals("AHS_SubAccountType should be 'Staff and Resources'", "Staff and Resources", subAccount.AHS_Calc_SubClassParent);

			subAccount.AHS_SubClassParentTableCode = GlbGroupSchema.Constants.Prefix;
			AssertEquals("AHS_SubAccountType should be 'Staff Group'", "Staff Group", subAccount.AHS_Calc_SubClassParent);
		}

		public void TestAHS_SubClassParentTableCode()
		{
			var journal = CreateJournalCore();

			var subAccount = journal.SubAccounts.AddNew();

			subAccount.AHS_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
			subAccount.AHS_SubClassParentId = TestObjectCreator.Creditor1.PK;

			AssertEquals("Pre-condition", OrgHeaderSchema.Constants.Prefix, subAccount.AHS_SubClassParentTableCode);
			AssertEquals("Pre-condition", TestObjectCreator.Creditor1.PK, subAccount.AHS_SubClassParentId);

			subAccount.AHS_SubClassParentTableCode = GlbStaffSchema.Constants.Prefix;

			AssertEquals("AHS_SubClassParentTableCode", GlbStaffSchema.Constants.Prefix, subAccount.AHS_SubClassParentTableCode);
			AssertEquals("AHS_SubClassParentId", ZGuid.Empty, subAccount.AHS_SubClassParentId);
		}

		public void TestAHS_SubClassParentId()
		{
			var journal = CreateJournalCore();

			var subAccount = journal.SubAccounts.AddNew();

			AssertEquals("Pre-condition", ZGuid.Empty, subAccount.AHS_SubClassParentId);
			subAccount.SubAccountParentId = TestObjectCreator.Creditor1.PK;
			AssertEquals("AHS_SubClassParentId", TestObjectCreator.Creditor1.PK, subAccount.AHS_SubClassParentId);

			AssertHasCustomAttribute(subAccount.GetType(),
				JournalSubAccount.Schema.AHS_SubClassParentId, false, (ListAttribute l) => l.ListDataSourceMember == "Lookups.SubAccountList");
		}

		public void TestAHS_SubClassParentId_ReadOnly()
		{
			var line = CreateJournalCore();
			var subAccount = line.SubAccounts.AddNew();

			subAccount.AHS_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
			AssertEquals("AHS_SubClassParentIdInfo should not be readonly when AHS_SubClassParentTableCode is not empty", false, subAccount.AHS_SubClassParentIdInfo.ReadOnly);

			line.AH_JH = TestObjectCreator.Job1.PK;
			AssertEquals("AHS_SubClassParentIdInfo should be readonly when line has job.", true, subAccount.AHS_SubClassParentIdInfo.ReadOnly);

			AssertEquals("line IsMultiSubAccountsSupported shoule be true.", true, line.IsMultiSubAccountsSupported);
		}

		public void TestAHS_Calc_SubAccountDescription()
		{
			var line = CreateJournalCore();
			var journalSubAccount = line.SubAccounts.AddNew();

			var orgHeader = TestObjectCreator.Creditor1;
			orgHeader.OH_FullName = "Test Company Name";
			journalSubAccount.AHS_SubClassParentTableCode = AccountingMasterFilesConstants.SubAccountTypeList.Organization.Code;
			journalSubAccount.AHS_SubClassParentId = orgHeader.PK;
			AssertEquals("organization name", "Test Company Name", journalSubAccount.AHS_Calc_SubAccountDescription);

			var salesGroup = TestObjectCreator.CreateSalesGroup("TestSales");
			salesGroup.AR_Desc = "Test Sales Group Desc";
			journalSubAccount.AHS_SubClassParentTableCode = AccountingMasterFilesConstants.SubAccountTypeList.SalesGroup.Code;
			journalSubAccount.AHS_SubClassParentId = salesGroup.PK;
			AssertEquals("sales group name", "Test Sales Group Desc", journalSubAccount.AHS_Calc_SubAccountDescription);

			var staff = TestObjectCreator.CreateStaff("AAA");
			staff.GS_FullName = "Test Staff Name";
			journalSubAccount.AHS_SubClassParentTableCode = AccountingMasterFilesConstants.SubAccountTypeList.StaffAndResources.Code;
			journalSubAccount.AHS_SubClassParentId = staff.PK;
			AssertEquals("staff name", "Test Staff Name", journalSubAccount.AHS_Calc_SubAccountDescription);

			var group = TestObjectCreator.CreateStaffGroup("TestGroup");
			group.GG_Desc = "Test Group Name";
			journalSubAccount.AHS_SubClassParentTableCode = AccountingMasterFilesConstants.SubAccountTypeList.StaffGroup.Code;
			journalSubAccount.AHS_SubClassParentId = group.PK;
			AssertEquals("group name", "Test Group Name", journalSubAccount.AHS_Calc_SubAccountDescription);
		}

		public void TestAHS_Calc_Sequence()
		{
			var journal = CreateJournalCore();

			var subAccount1 = journal.SubAccounts.AddNew();
			var subAccount2 = journal.SubAccounts.AddNew();
			var subAccount3 = journal.SubAccounts.AddNew();
			var subAccount4 = journal.SubAccounts.AddNew();

			subAccount1.AHS_SubClassParentTableCode = GlbGroupSchema.Constants.Prefix;
			subAccount2.AHS_SubClassParentTableCode = GlbStaffSchema.Constants.Prefix;
			subAccount3.AHS_SubClassParentTableCode = AccGroupsSchema.Constants.Prefix;
			subAccount4.AHS_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;

			AssertEquals("The first sequence should be 4", 4, subAccount1.AHS_Calc_Sequence);
			AssertEquals("The second sequence should be 3", 3, subAccount2.AHS_Calc_Sequence);
			AssertEquals("The third sequence should be 2", 2, subAccount3.AHS_Calc_Sequence);
			AssertEquals("The fourth sequence should be 1", 1, subAccount4.AHS_Calc_Sequence);
		}

		public void TestDeleteDataWhenSubClassParentIdIsEmptyDataOnSaving()
		{
			var journal = CreateJournalCore();
			var firstSubAccount = journal.SubAccounts.AddNew();
			var secondSubAccount = journal.SubAccounts.AddNew();
			var thirdSubAccount = journal.SubAccounts.AddNew();

			firstSubAccount.SubAccountParentId = TestObjectCreator.Creditor1.PK;

			secondSubAccount.AHS_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;

			thirdSubAccount.AHS_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
			thirdSubAccount.AHS_SubClassParentId = TestObjectCreator.Creditor1.PK;

			AssertEquals("Pre-condition", ZString.Empty, firstSubAccount.AHS_SubClassParentTableCode);
			AssertEquals("Pre-condition", TestObjectCreator.Creditor1.PK, firstSubAccount.AHS_SubClassParentId);

			AssertEquals("Pre-condition", OrgHeaderSchema.Constants.Prefix, secondSubAccount.AHS_SubClassParentTableCode);
			AssertEquals("Pre-condition", ZGuid.Empty, secondSubAccount.AHS_SubClassParentId);

			AssertEquals("Pre-condition", OrgHeaderSchema.Constants.Prefix, thirdSubAccount.AHS_SubClassParentTableCode);
			AssertEquals("Pre-condition", TestObjectCreator.Creditor1.PK, thirdSubAccount.AHS_SubClassParentId);

			Factory.Save();

			AssertEquals("first sub acount has been deleled when AHS_SubClassParentTableCode is empty.", true, firstSubAccount.IsDeleted);
			AssertEquals("second sub acount has been deleled when AHS_SubClassParentId is empty.", true, secondSubAccount.IsDeleted);
			AssertEquals("third sub acount has been saved", false, thirdSubAccount.IsDeleted);
		}

		public void TestSubAccountsWithMultipleSubAccounts()
		{
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, AccGroupsSchema.Constants.Prefix, false);
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, OrgHeaderSchema.Constants.Prefix, true);
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, GlbStaffSchema.Constants.Prefix, true);

			var journal = CreateJournalCore();

			Assert("Sub Account 1 is read Only", journal.AH_Calc_SecondSubClassParentId_ReadOnly);
			Assert("Sub Account 2 is read Only", journal.AH_Calc_SecondSubClassParentId_ReadOnly);

			journal.AH_AG = TestObjectCreator.GLHeader1.PK;

			Assert("Sub Account 1 is not read Only", !journal.AH_Calc_SecondSubClassParentId_ReadOnly);
			Assert("Sub Account 2 is not read Only", !journal.AH_Calc_SecondSubClassParentId_ReadOnly);

			journal.SubAccounts.OfType<JournalSubAccount>().First(x => x.AHS_SubClassParentTableCode == OrgHeaderSchema.Constants.Prefix).AHS_SubClassParentId = TestObjectCreator.Creditor1.PK;
			journal.SubAccounts.OfType<JournalSubAccount>().First(x => x.AHS_SubClassParentTableCode == AccGroupsSchema.Constants.Prefix).AHS_SubClassParentId = TestObjectCreator.AR1.PK;

			AssertEquals("Sub Account 1", TestObjectCreator.Creditor1.PK, journal.AH_Calc_FirstSubClassParentId);
			AssertEquals("Sub Account 2", TestObjectCreator.AR1.PK, journal.AH_Calc_SecondSubClassParentId);

			AssertEquals("Sub Account 1 Type", Constants.SubAccountTypeDescriptions.Organization, journal.AH_Calc_FirstSubClassParent);
			AssertEquals("Sub Account 2 Type", Constants.SubAccountTypeDescriptions.SalesGroup, journal.AH_Calc_SecondSubClassParent);

			Assert("Sub Account is Mandatory", journal.SubAccounts.OfType<JournalSubAccount>().First(x => x.AHS_SubClassParentTableCode == OrgHeaderSchema.Constants.Prefix).IsSubClassValidationRuleMandatory);
			Assert("Sub Account is Mandatory", journal.SubAccounts.OfType<JournalSubAccount>().First(x => x.AHS_SubClassParentTableCode == GlbStaffSchema.Constants.Prefix).IsSubClassValidationRuleMandatory);
			Assert("Sub Account is not Mandatory", !journal.SubAccounts.OfType<JournalSubAccount>().First(x => x.AHS_SubClassParentTableCode == AccGroupsSchema.Constants.Prefix).IsSubClassValidationRuleMandatory);
		}

		public void TestLookups()
		{
			var journalSubAccount = Factory.New<JournalSubAccount>();
			AssertType<JournalSubAccountLookups>(journalSubAccount.Lookups);
		}

		public void TestValidation()
		{
			var journalSubAccount = Factory.New<JournalSubAccount>();
			AssertType<JournalSubAccountValidation>(journalSubAccount.Validation);
		}

		public void TestReverseWithMultipleSubAccounts()
		{
			AccountingConfigurationRegistry.Instance.ARJournalAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APJournalAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, OrgHeaderSchema.Constants.Prefix, false);
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, AccGroupsSchema.Constants.Prefix, false);
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, GlbStaffSchema.Constants.Prefix, true);
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, GlbGroupSchema.Constants.Prefix, false);
			Factory.Save();

			var journal = CreateJournalCore();
			var subAccounts = journal.SubAccounts.Cast<ISupportSubAccount>();
			subAccounts.First(x => x.SubAccountTypeParentTableCode == OrgHeaderSchema.Constants.Prefix).SubAccountParentId = TestObjectCreator.ABIGAS.PK;
			subAccounts.First(x => x.SubAccountTypeParentTableCode == AccGroupsSchema.Constants.Prefix).SubAccountParentId = TestObjectCreator.AR1.PK;
			subAccounts.First(x => x.SubAccountTypeParentTableCode == GlbGroupSchema.Constants.Prefix).SubAccountParentId = TestObjectCreator.GG1.PK;
			Factory.Save();

			new ReversingBase(journal).Reverse();

			var reverseJournal = journal.ReverseTransaction as Journal;
			var reverseJournalSubAccounts = reverseJournal.SubAccounts.Cast<ISupportSubAccount>();
			AssertNotNull(reverseJournal);
			AssertEquals(3, reverseJournal.SubAccounts.Count);
			AssertEquals(TestObjectCreator.GLHeader1.PK, reverseJournal.AH_AG);
			Assert(reverseJournalSubAccounts.Any(x => x.SubAccountTypeParentTableCode == OrgHeaderSchema.Constants.Prefix && x.SubAccountParentId == TestObjectCreator.ABIGAS.PK));
			Assert(reverseJournalSubAccounts.Any(x => x.SubAccountTypeParentTableCode == AccGroupsSchema.Constants.Prefix && x.SubAccountParentId == TestObjectCreator.AR1.PK));
			Assert(reverseJournalSubAccounts.Any(x => x.SubAccountTypeParentTableCode == GlbGroupSchema.Constants.Prefix && x.SubAccountParentId == TestObjectCreator.GG1.PK));

			reverseJournal.RunPreSaveValidation();
			Assert(!reverseJournal.NotificationsIncludingChildren.Contains("Error - AHS_SubClassParentId: Please enter a Sub Account."));
		}

		public void TestUpdateRelateJournalSubAccount()
		{
			AccGLHeader controlAccount = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			TestObjectCreator.CreateGLHeaderSubAccount(controlAccount, OrgHeaderSchema.Constants.Prefix, true);
			Factory.Save();

			Journal journal = Factory.NewWithValidTestData<APJournal>();
			journal.AH_AG = controlAccount.PK;

			Journal journalCopy = Factory.NewWithValidTestData<APJournal>();
			journalCopy.AH_AG = controlAccount.PK;

			journal.RelatedJournal = journalCopy;

			var subAccounts = journal.SubAccounts.Cast<ISupportSubAccount>();
			var subAccountsCopy = journal.RelatedJournal.SubAccounts.Cast<ISupportSubAccount>();
			AssertEquals(1, subAccounts.Count());
			AssertEquals(1, subAccountsCopy.Count());

			Assert(subAccounts.Any(x => x.SubAccountTypeParentTableCode == OrgHeaderSchema.Constants.Prefix && x.SubAccountParentId == ZGuid.Empty));
			Assert(subAccountsCopy.Any(x => x.SubAccountTypeParentTableCode == OrgHeaderSchema.Constants.Prefix && x.SubAccountParentId == ZGuid.Empty));
			Assert(journal.NotificationsIncludingChildren.Contains("Error - AHS_SubClassParentId: Please enter a Sub Account."));
			Assert(journal.RelatedJournal.NotificationsIncludingChildren.Contains("Error - AHS_SubClassParentId: Please enter a Sub Account."));

			subAccounts.FirstOrDefault(x => x.SubAccountTypeParentTableCode == OrgHeaderSchema.Constants.Prefix).SubAccountParentId = testObjectCreator.ABIGAS.PK;

			Assert(subAccounts.Any(x => x.SubAccountTypeParentTableCode == OrgHeaderSchema.Constants.Prefix && x.SubAccountParentId == testObjectCreator.ABIGAS.PK));
			Assert(subAccountsCopy.Any(x => x.SubAccountTypeParentTableCode == OrgHeaderSchema.Constants.Prefix && x.SubAccountParentId == testObjectCreator.ABIGAS.PK));
			Assert(!journal.NotificationsIncludingChildren.Contains("Error - AHS_SubClassParentId: Please enter a Sub Account."));
			Assert(!journal.RelatedJournal.NotificationsIncludingChildren.Contains("Error - AHS_SubClassParentId: Please enter a Sub Account."));
		}

		[ExpectNoExceptions]
		public void TestDelete()
		{
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, OrgHeaderSchema.Constants.Prefix, false);
			var journal = CreateJournalCore();
			journal.AH_AG = TestObjectCreator.GLHeader1.PK;
			journal.AH_Calc_FirstSubClassParentId = TestObjectCreator.ABIGAS.PK;
			journal.Delete();

			Factory.Save();
		}

		protected abstract Journal CreateJournalCore();

		JournalSubAccount CreateSubAccount()
		{
			var line = CreateJournalCore();
			var subAccount = line.SubAccounts.AddNew();
			return subAccount;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var journalSubAccount = CreateSubAccount();
			journalSubAccount.AHS_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
			journalSubAccount.AHS_SubClassParentId = TestObjectCreator.Creditor1.PK;
			return journalSubAccount;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return CreateSubAccount();
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		[TestedType(typeof(JournalSubAccount))]
		public class APJournalSubAccountTest : JournalSubAccountTest
		{
			[TestDate(2020, 3, 11)]
			protected override Journal CreateJournalCore()
			{
				return TestObjectCreator.CreateJournal<APJournal>(0m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK);
			}

			protected override BusinessObject GetNewBusinessObject() => Factory.New<JournalSubAccount>();
		}

		[TestedType(typeof(JournalSubAccount))]
		public class ARJournalSubAccountTest : JournalSubAccountTest
		{
			[TestDate(2020, 3, 11)]
			protected override Journal CreateJournalCore()
			{
				return TestObjectCreator.CreateJournal<ARJournal>(0m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK);
			}

			protected override BusinessObject GetNewBusinessObject() => Factory.New<JournalSubAccount>();
		}
	}
}
