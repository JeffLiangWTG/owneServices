using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	public abstract class JournalSubAccountLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSubAccountList()
		{
			JournalSubAccount.AHS_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
			AssertType("SubAccountList should be OrgHeaderCollection when type is 'Organization'", typeof(OrgHeaderCollection), Lookups.SubAccountList);

			JournalSubAccount.AHS_SubClassParentTableCode = AccGroupsSchema.Constants.Prefix;
			AssertType("SubAccountList should be AccGroupsCollection when type is 'Sales/Expense Groups'", typeof(AccGroupsCollection), Lookups.SubAccountList);

			JournalSubAccount.AHS_SubClassParentTableCode = GlbStaffSchema.Constants.Prefix;
			AssertType("SubAccountList should be GlbStaffAndResourceCollection when type is 'Staff and Resources'", typeof(GlbStaffAndResourceCollection), Lookups.SubAccountList);

			JournalSubAccount.AHS_SubClassParentTableCode = GlbGroupSchema.Constants.Prefix;
			AssertType("SubAccountList should be GlbGroupCollection when type is 'Staff Group'", typeof(GlbGroupCollection), Lookups.SubAccountList);
		}

		public void TestSubAccountTypeList()
		{
			AssertEquals(@"ORG - Organization
SEG - Sales/Expense Groups
STR - Staff and Resources
SGP - Staff Group", Lookups.SubAccountTypeList.ElementsAsString);
		}

		protected abstract Journal CreateJournalCore();

		protected override void SetUp()
		{
			base.SetUp();
			var line = CreateJournalCore();
			JournalSubAccount = line.SubAccounts.AddNew();
			Lookups = JournalSubAccount.Lookups;
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		JournalSubAccountLookups Lookups;

		JournalSubAccount JournalSubAccount;

		public class APJournalSubAccountLookupsTest : JournalSubAccountLookupsTest
		{
			[TestDate(2020, 3, 11)]
			protected override Journal CreateJournalCore()
			{
				return TestObjectCreator.CreateJournal<APJournal>(0m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK);
			}
		}

		public class ARJournalSubAccountLookupsTest : JournalSubAccountLookupsTest
		{
			[TestDate(2020, 3, 11)]
			protected override Journal CreateJournalCore()
			{
				return TestObjectCreator.CreateJournal<ARJournal>(0m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK);
			}
		}
	}
}
