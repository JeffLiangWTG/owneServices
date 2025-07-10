using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CusReconEntryCollection))]
	class CusReconEntryCollectionTest : ActiveBusinessObjectCollectionTestCase<CusReconEntryCollection>
	{
		public void TestFilter_NullBranch()
		{
			AssertNoResultFilter(ZGuid.Empty, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		}

		public void TestFilter_EmptyPeriodFrom()
		{
			AssertNoResultFilter(GlbBranch.CurrentBranch.PK, ZDate.Empty, ZDateTime.MaxSmallDateTimeValue.Date);
		}

		public void TestFilter_EmptyPeriodTo()
		{
			AssertNoResultFilter(GlbBranch.CurrentBranch.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDate.Empty);
		}

		public void TestFilter_CusReconDeclarationEntered()
		{
			var reconEntry1 = Factory.New<CusReconEntry>();
			reconEntry1.CRE_EntryDate = ZDate.Today;

			var reconDeclaration = Factory.New<CusReconDeclaration>();
			var reconEntry2 = reconDeclaration.CusReconEntries.AddNew();
			reconEntry2.CRE_EntryDate = ZDate.Today;

			var collection = new CusReconEntryCollection(Factory, GlbBranch.CurrentBranch.PK, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1));
			AssertContainsExactElementsInAnyOrder(new[] { reconEntry1 }, collection);
		}

		public void TestFilter_Branch()
		{
			var reconEntry1 = Factory.New<CusReconEntry>();
			reconEntry1.CRE_EntryDate = ZDate.Today;

			var reconEntry2 = Factory.New<CusReconEntry>();
			reconEntry2.CRE_EntryDate = ZDate.Today;
			reconEntry2.CRE_GB_Branch = GlbCompany.CurrentCompany.Branches.AddNew().PK;

			var reconEntry3 = Factory.New<CusReconEntry>();
			reconEntry3.CRE_EntryDate = ZDate.Today;
			reconEntry3.CRE_GB_Branch = Factory.New<GlbBranch>().PK;

			var collection = new CusReconEntryCollection(Factory, GlbBranch.CurrentBranch.PK, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1));
			AssertContainsExactElementsInAnyOrder("Only matches branch of ReconDeclaration", new[] { reconEntry1 }, collection);
		}

		public void TestFilter_EntryDate()
		{
			var reconEntry1 = Factory.New<CusReconEntry>();
			reconEntry1.CRE_EntryDate = new ZDate(2020, 11, 24);

			var reconEntry2 = Factory.New<CusReconEntry>();
			reconEntry2.CRE_EntryDate = new ZDate(2020, 11, 27);

			var reconEntry3 = Factory.New<CusReconEntry>();
			reconEntry3.CRE_EntryDate = new ZDate(2020, 11, 30);

			var collection = new CusReconEntryCollection(Factory, GlbBranch.CurrentBranch.PK, new ZDate(2020, 11, 26), new ZDate(2020, 11, 28));
			AssertContainsExactElementsInAnyOrder(new[] { reconEntry2 }, collection);
		}

		public void TestMaximumAvailableToAdd()
		{
			var collection = GetCollectionToTest();

			AssertEquals("Precondition", 0, collection.Count);
			AssertEquals(999, collection.MaximumAvailableToAdd);

			collection.AddNew();
			AssertEquals(998, collection.MaximumAvailableToAdd);
		}

		protected override CusReconEntryCollection GetCollectionToTest()
		{
			var declaration = Factory.New<CusReconDeclaration>();
			return new CusReconEntryCollection(declaration);
		}

		void AssertNoResultFilter(ZGuid branchPk, ZDate periodFrom, ZDate periodTo)
		{
			var reconEntry = Factory.New<CusReconEntry>();
			reconEntry.CRE_EntryDate = ZDate.Today;
			var dbHits = Factory.DatabaseLoadCount;
			var collection = new CusReconEntryCollection(Factory, branchPk, periodFrom, periodTo);
			CombineAssertions(() =>
			{
				AssertEquals("No records", 0, collection.Count);
				AssertEquals("No hits", dbHits, Factory.DatabaseLoadCount);
			});
		}
	}
}
