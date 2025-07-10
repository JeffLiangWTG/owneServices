using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.Customs.EU.H7.Module.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Module.Testing
{
	[TestedType(typeof(ESH7BillFilterBusinessObject))]
	public class ESH7BillFilterBusinessObjectTest : EUH7BillFilterBusinessObjectTest
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ESH7BillFilterBusinessObject();

		protected override CodeDescriptionPairList GetExpectedCustomStatusList(FilterStripBusinessObject filterObj) => Factory.GetCachedValue<ESH7AISEntryStatusList>();

		public override void TestLRNFilter()
		{
			var asyheader1 = Factory.New<AsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			var bill1 = CreateBillAndEntry(asyheader1, "G3", "LRN", "LRN1");
			var bill2 = CreateBillAndEntry(asyheader1, "G3", "LRN", "LRN2");
			var bill3 = CreateBillAndEntry(asyheader1, "G3", "MRN", "LRN1");

			Factory.Save();

			var filterObj = GetNewFilterStripBusinessObject();
			var g3LRNFilter = (ModuleTextFilter)filterObj[ESH7BillFilterBusinessObject.ESDescriptions.G3LRN];
			AssertNotNull(g3LRNFilter);
			g3LRNFilter.IsActive = true;
			g3LRNFilter.Property = "LRN1";

			CombineAssertions(() =>
			{
				Assert("G3&LRN&LRN1", bill1.MatchesFilter(filterObj.Filter));
				Assert("G3&LRN&LRN2", !bill2.MatchesFilter(filterObj.Filter));
				Assert("G3&MRN&LRN1", !bill3.MatchesFilter(filterObj.Filter));
			});
		}

		public override void TestMRNFilter()
		{
			var asyheader1 = Factory.New<AsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			var bill1 = CreateBillAndEntry(asyheader1, "G3", "MRN", "MRN1");
			var bill2 = CreateBillAndEntry(asyheader1, "G3", "MRN", "MRN2");
			var bill3 = CreateBillAndEntry(asyheader1, "G3", "LRN", "MRN1");
			var bill4 = CreateBillAndEntry(asyheader1, "H7", "MRN", "MRN1");
			var bill5 = CreateBillAndEntry(asyheader1, "H7", "MRN", "MRN2");
			var bill6 = CreateBillAndEntry(asyheader1, "H7", "LRN", "MRN1");

			Factory.Save();

			var filterObj = GetNewFilterStripBusinessObject();
			var g3MRNFilter = (ModuleTextFilter)filterObj[ESH7BillFilterBusinessObject.ESDescriptions.G3MRN];
			AssertNotNull(g3MRNFilter);
			g3MRNFilter.IsActive = true;
			g3MRNFilter.Property = "MRN1";

			CombineAssertions(() =>
			{
				Assert("G3&MRN&MRN1", bill1.MatchesFilter(filterObj.Filter));
				Assert("G3&MRN&MRN2", !bill2.MatchesFilter(filterObj.Filter));
				Assert("G3&LRN&MRN1", !bill3.MatchesFilter(filterObj.Filter));
				Assert("H7&MRN&MRN1", !bill4.MatchesFilter(filterObj.Filter));
				Assert("H7&MRN&MRN2", !bill5.MatchesFilter(filterObj.Filter));
				Assert("H7&LRN&MRN1", !bill6.MatchesFilter(filterObj.Filter));
			});

			g3MRNFilter.Clear();
			var h7MRNFilter = (ModuleTextFilter)filterObj[ESH7BillFilterBusinessObject.ESDescriptions.H7MRN];
			AssertNotNull(h7MRNFilter);
			h7MRNFilter.IsActive = true;
			h7MRNFilter.Property = "MRN1";

			CombineAssertions(() =>
			{
				Assert("G3&MRN&MRN1", !bill1.MatchesFilter(filterObj.Filter));
				Assert("G3&MRN&MRN2", !bill2.MatchesFilter(filterObj.Filter));
				Assert("G3&LRN&MRN1", !bill3.MatchesFilter(filterObj.Filter));
				Assert("H7&MRN&MRN1", bill4.MatchesFilter(filterObj.Filter));
				Assert("H7&MRN&MRN2", !bill5.MatchesFilter(filterObj.Filter));
				Assert("H7&LRN&MRN1", !bill6.MatchesFilter(filterObj.Filter));
			});
		}

		public override void AssertLRNAndMRNFilterCategory(FilterStripBusinessObject filterObj)
		{
			AssertEquals(FilterCategories.NumbersAndReferences, filterObj[ESH7BillFilterBusinessObject.ESDescriptions.H7MRN].Category);
			AssertEquals(FilterCategories.NumbersAndReferences, filterObj[ESH7BillFilterBusinessObject.ESDescriptions.G3MRN].Category);
			AssertEquals(FilterCategories.NumbersAndReferences, filterObj[ESH7BillFilterBusinessObject.ESDescriptions.G3LRN].Category);
		}
	}
}
