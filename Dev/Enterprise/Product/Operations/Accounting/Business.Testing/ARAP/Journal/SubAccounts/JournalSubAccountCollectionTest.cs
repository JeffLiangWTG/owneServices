using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(JournalSubAccountCollection))]
	public abstract class JournalSubAccountCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSubAccountsSequence()
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

			journal.SubAccounts.Load();
			AssertEquals("The first sub account sequence should be 4", 4, subAccount1.AHS_Calc_Sequence);
			AssertEquals("The second sub account sequence should be 3", 3, subAccount2.AHS_Calc_Sequence);
			AssertEquals("The third sub account sequence should be 2", 2, subAccount3.AHS_Calc_Sequence);
			AssertEquals("The fourth sub account sequence should be 1", 1, subAccount4.AHS_Calc_Sequence);

			subAccount3.Delete();

			journal.SubAccounts.Load();
			AssertEquals("The first sub account sequence should be 4 when table code is 'GG'", 4, subAccount1.AHS_Calc_Sequence);
			AssertEquals("The second sub account sequence should be 3 when table code is 'GS'", 3, subAccount2.AHS_Calc_Sequence);
			AssertNull("The third sub account has been deleted", journal.SubAccounts.Cast<JournalSubAccount>().FirstOrDefault(x => x == subAccount3));
			AssertEquals("The fourth sub account sequence should be 1 when table code is 'OH'", 1, subAccount4.AHS_Calc_Sequence);
		}

		protected abstract Journal CreateJournalCore();

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		[TestedType(typeof(JournalSubAccountCollection))]
		public class APJournalSubAccountCollectionTest : JournalSubAccountCollectionTest
		{
			[TestDate(2020, 3, 11)]
			protected override Journal CreateJournalCore()
			{
				return TestObjectCreator.CreateJournal<APJournal>(0m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK);
			}

			protected override BusinessObjectCollection GetCollectionToTest()
			{
				return new JournalSubAccountCollection(CreateJournalCore());
			}
		}

		[TestedType(typeof(JournalSubAccountCollection))]
		public class ARJournalSubAccountCollectionTest : JournalSubAccountCollectionTest
		{
			[TestDate(2020, 3, 11)]
			protected override Journal CreateJournalCore()
			{
				return TestObjectCreator.CreateJournal<ARJournal>(0m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK);
			}

			protected override BusinessObjectCollection GetCollectionToTest()
			{
				return new JournalSubAccountCollection(CreateJournalCore());
			}
		}
	}
}
