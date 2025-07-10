using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(EntryHeaderFilterBusinessObject))]
	sealed class EntryHeaderNumberOfEntryLinesFilterTest : FilterStripBusinessObjectTestCase
	{
		public void TestNumberOfEntryLinesFilter()
		{
			entryHeaderCollectionOne.Load(filterStripBusinessObject.Filter);
			entryHeaderCollectionTwo.Load(filterStripBusinessObject.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollectionOne Count", 1, entryHeaderCollectionOne.Count);
				AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollectionTwo Count", 1, entryHeaderCollectionTwo.Count);
			});

			var cusQuantityFilter =
				(ModuleNumberRangeFilter)filterStripBusinessObject[
					EntryHeaderFilterBusinessObject.EUFilterConstants.NumberOfEntryLines];
			AssertNotNull("Number Of Entry Lines Filter", cusQuantityFilter);

			cusQuantityFilter.IsActive = true;
			cusQuantityFilter.Property1 = 1;
			cusQuantityFilter.Property2 = 1;
			entryHeaderCollectionOne.Load(filterStripBusinessObject.Filter);
			entryHeaderCollectionTwo.Load(filterStripBusinessObject.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("EntryHeaderCollectionOne Count", 0, entryHeaderCollectionOne.Count);
				AssertEquals("Single Entry Header Found", entryHeaderTwo.PK, entryHeaderCollectionTwo[0].PK);
				AssertEquals("EntryHeaderCollectionTwo Count", 1, entryHeaderCollectionTwo.Count);
			});

			cusQuantityFilter.Property1 = 2;
			cusQuantityFilter.Property2 = 5;
			entryHeaderCollectionOne.Load(filterStripBusinessObject.Filter);
			entryHeaderCollectionTwo.Load(filterStripBusinessObject.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("EntryHeaderCollectionOne Count", 1, entryHeaderCollectionOne.Count);
				AssertEquals("Single Entry Header Found", entryHeaderOne.PK, entryHeaderCollectionOne[0].PK);
				AssertEquals("EntryHeaderCollectionTwo Count", 0, entryHeaderCollectionTwo.Count);
			});
		}

		public void TestEntryHeaderNoOfEntryLinesFilterGenerator()
		{
			var noOfEntryLinesFilter = new EntryHeaderNoOfEntryLinesFilterGenerator()
				.Generate("My Filter",
							FilterCategories.Other,
							(NoResString)"Filter Description");

			AssertNotNull("Number of Entry Lines filter", noOfEntryLinesFilter);
			AssertEquals("Number of Entry Lines Filter Type", typeof(ModuleNumberRangeFilter), noOfEntryLinesFilter.GetType());
			AssertEquals("Decimals", (byte)0, ((ModuleNumberRangeFilter)noOfEntryLinesFilter).Decimals);
		}

		protected override void SetUp()
		{
			base.SetUp();

			JobDeclaration declarationOne = Factory.New<JobDeclaration>();
			entryHeaderOne = declarationOne.CustomsEntryHeaders.AddNew();
			entryHeaderOne.AllEntryLines.AddNew();
			entryHeaderOne.AllEntryLines.AddNew();
			entryHeaderOne.AllEntryLines.AddNew();
			entryHeaderCollectionOne =
				new Customs.Business.CusEntryHeaderCollection<CusEntryHeader>(declarationOne, Factory);

			JobDeclaration declarationTwo = Factory.New<JobDeclaration>();
			entryHeaderTwo = declarationTwo.CustomsEntryHeaders.AddNew();
			entryHeaderTwo.AllEntryLines.AddNew();
			entryHeaderCollectionTwo =
				new Customs.Business.CusEntryHeaderCollection<CusEntryHeader>(declarationTwo, Factory);

			Factory.Save();

			filterStripBusinessObject = GetNewFilterStripBusinessObject();
		}

		CusEntryHeader entryHeaderOne, entryHeaderTwo;
		Customs.Business.CusEntryHeaderCollection<CusEntryHeader> entryHeaderCollectionOne, entryHeaderCollectionTwo;
		FilterStripBusinessObject filterStripBusinessObject;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
			=> new EntryHeaderFilterBusinessObject();
	}
}
