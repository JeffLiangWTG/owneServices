using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Module.Testing
{
	[TestedType(typeof(EntryHeaderFilterBusinessObject))]
	class EntryHeaderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestEntryHeaderFilterLookups()
		{
			var filterStripBO = (EntryHeaderFilterBusinessObject)filterStripBusinessObject;
			AssertType<EntryHeaderFilterLookups>(filterStripBO.Lookups);
		}

		public void TestParallel()
		{
			entryHeader1.ZG_Parallel = true;
			entryHeader2.ZG_Parallel = false;
			Factory.Save();

			entryHeaderCollection.Load(filterStripBusinessObject.Filter);
			AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection Count", 2, entryHeaderCollection.Count);

			var entryParallelFilter = (ModuleTextFilter)filterStripBusinessObject["Parallel"];
			entryParallelFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			entryParallelFilter.Property = Customs.Business.YesNoList.Codes.Yes;
			entryParallelFilter.IsActive = true;
			entryHeaderCollection.Load(filterStripBusinessObject.Filter);

			CombineAssertions("[POST-CONDITION] When Exact 'Y' filter is applied", () =>
			{
				AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
				AssertEquals("Single EntryHeader found PK", entryHeader1.PK, entryHeaderCollection[0].PK);
			});

			entryParallelFilter.Property = Customs.Business.YesNoList.Codes.No;
			entryHeaderCollection.Load(filterStripBusinessObject.Filter);
			CombineAssertions("[POST-CONDITION] When Exact 'N' filter is applied", () =>
			{
				AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
				AssertEquals("Single EntryHeader found PK", entryHeader2.PK, entryHeaderCollection[0].PK);
			});

			entryParallelFilter.Property = "Z";
			entryHeaderCollection.Load(filterStripBusinessObject.Filter);
			CombineAssertions("[POST-CONDITION] When Exact 'Z' filter is applied", () =>
			{
				AssertEquals("EntryHeaderCollection Count", 0, entryHeaderCollection.Count);
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EntryHeaderFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2 = declaration.CustomsEntryHeaders.AddNew();

			entryHeaderCollection = new EU.Business.Declaration.CusEntryHeaderCollection<CusEntryHeader>(declaration, Factory);
			filterStripBusinessObject = GetNewFilterStripBusinessObject();
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader1;
		CusEntryHeader entryHeader2;
		EU.Business.Declaration.CusEntryHeaderCollection<CusEntryHeader> entryHeaderCollection;
		FilterStripBusinessObject filterStripBusinessObject;
	}
}
