using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class ZListUserControlTest : TestCaseWithDummy
	{
		public void TestUpdateLookups()
		{
			var dummyWithLookups = Factory.New<DummyWithLookups>();
			using (var dummyListControl = new DummyListControl())
			{
				dummyListControl.SetDataBinding(dummyWithLookups, "Z0_Guid");

				var list = dummyListControl.List;

				AssertEquals("Number of items in the list", 3, list.Count);
				AssertEquals("Number of items counted in UpdateLookups", 3, dummyListControl.LookupCollectionCount);
			}
		}

		public void TestIAdditionalInformationMembers()
		{
			var dummyWithLookups = Factory.New<DummyWithLookups>();
			using (var dummyListControl = new DummyListControl())
			{
				dummyListControl.SetDataBinding(dummyWithLookups, DummyWithLookups.Schema.Z0_Code);
				IAdditionalInformation additionalInformation = dummyListControl;
				AssertEquals("Lookups+PairList\r\nIt is blue", additionalInformation.AdditionalInformation);
				dummyListControl.BindToList = "Lookups+PairList2";
				AssertEquals("Lookups+PairList2\r\nLookups+PairList\r\nIt is blue", additionalInformation.AdditionalInformation);
			}
		}
	}
}
