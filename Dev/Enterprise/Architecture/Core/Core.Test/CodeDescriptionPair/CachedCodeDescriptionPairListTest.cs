using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class CachedCodeDescriptionPairListTest : TestCase
	{
		public CachedCodeDescriptionPairListTest() : base()
		{
		}

		public void TestNeedsRefreshNullNull()
		{
			AssertEquals(false, PairList.NeedsRefresh(null));
		}

		public void TestNeedsRefreshNullData()
		{
			AssertEquals(true, PairList.NeedsRefresh(""));
		}

		public void TestNeedsRefreshDataNull()
		{
			PairList.FilterString = "";
			AssertEquals(true, PairList.NeedsRefresh(null));
		}

		public void TestNeedsRefreshSameData()
		{
			PairList.FilterString = "XYZ";
			AssertEquals(false, PairList.NeedsRefresh(PairList.FilterString));
		}

		public void TestNeedsRefreshDifferentData()
		{
			PairList.FilterString = "XYZ";
			AssertEquals(true, PairList.NeedsRefresh("XY"));
		}

		public void TestCurrentFilterStringIsNotChangedByCallingNeedsRefresh()
		{
			PairList.FilterString = "XYZ";
			PairList.NeedsRefresh("XY");
			AssertEquals("XYZ", PairList.FilterString);
		}

		public void TestSettingFilterStringClearsData()
		{
			PairList.AddPair("Code", "Description");
			AssertEquals("PreCondition : PairList.Count", 1, PairList.Count);
			PairList.FilterString = "New FilterString";
			AssertEquals("PairList.Count after changing FilterString", 0, PairList.Count);
		}

		#region Implementation
		CachedCodeDescriptionPairList PairList;
		protected override void SetUp()
		{
			base.SetUp();
			PairList = new CachedCodeDescriptionPairList();
		}
		#endregion

	}
}
