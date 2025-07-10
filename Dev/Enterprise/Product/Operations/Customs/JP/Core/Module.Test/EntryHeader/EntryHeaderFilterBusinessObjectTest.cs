using Enterprise.Customs.JP.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Module.Test
{
	[TestedType(typeof(EntryHeaderFilterBusinessObject))]
	sealed class EntryHeaderFilterBusinessObjectTest : Customs.Module.Testing.EntryHeaderFilterBusinessObjectAbstractTest
	{
		public void TestGetEntryStatusList()
		{
			var entryHeaderFilterBusinessObject = new EntryHeaderFilterBusinessObjectForTest();
			AssertSame(entryHeaderFilterBusinessObject.Factory.GetCachedValue<CustomsStatusList>(), entryHeaderFilterBusinessObject.GetEntryStatusList_Exposed());
		}

		public void TestInspectionStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_InspectionStatus = "S1HG";
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_InspectionStatus = "S2HG";
			var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader3.CH_InspectionStatus = "S1CG";
			Factory.Save();

			var filterBizo = new EntryHeaderFilterBusinessObject();
			var filters = filterBizo.ModuleFilters;
			var inspectionStatusFilter = (InspectionStatusFilter)filters[EntryHeaderFilterBusinessObject.FilterConstants.InspectionStatus];
			inspectionStatusFilter.IsActive = true;
 
			var query = filters.GetFilterQuery(new ModuleFilter[] { inspectionStatusFilter });
			Assert(entryHeader1.MatchesFilter(query));
			Assert(entryHeader2.MatchesFilter(query));
			Assert(entryHeader3.MatchesFilter(query));

			inspectionStatusFilter.FirstChar = "S";
			inspectionStatusFilter.SecondChar = "1";
			inspectionStatusFilter.FourthChar = "G";

			query = filters.GetFilterQuery(new ModuleFilter[] { inspectionStatusFilter });
			Assert(entryHeader1.MatchesFilter(query));
			Assert(!entryHeader2.MatchesFilter(query));
			Assert(entryHeader3.MatchesFilter(query));

			inspectionStatusFilter.ThirdChar = "H";
			query = filters.GetFilterQuery(new ModuleFilter[] { inspectionStatusFilter });
			Assert(entryHeader1.MatchesFilter(query));
			Assert(!entryHeader2.MatchesFilter(query));
			Assert(!entryHeader3.MatchesFilter(query));
		}

		public void TestModuleFiltersAreAdded()
		{
			var filterObj = GetNewFilterStripBusinessObject();
			AssertNotNull(filterObj[EntryHeaderFilterBusinessObject.FilterConstants.InspectionStatus]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EntryHeaderFilterBusinessObject();
		}

		class EntryHeaderFilterBusinessObjectForTest : EntryHeaderFilterBusinessObject
		{
			public CodeDescriptionPairList GetEntryStatusList_Exposed() => base.GetEntryStatusList();
		}
	}
}
