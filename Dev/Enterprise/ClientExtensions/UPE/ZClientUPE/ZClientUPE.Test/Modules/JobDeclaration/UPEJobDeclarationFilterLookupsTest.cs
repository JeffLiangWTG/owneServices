using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.Module.Testing
{
	internal class UPEJobDeclarationFilterLookupsTest : TestCaseWithFactory
	{
		#region Code Description Pair Lists
		public void TestQueueNames_List()
		{
			AssertEquals("Correct queue names code desc pair type", typeof(DeclarationQueueCodeDescriptionPairList), Lookups.QueueNames_List.GetType());
		}

		public void TestZoneNameList()
		{
			AssertEquals("Zone name list should be correct", true, Lookups.ZoneNameList.Count >= 2);
		}

		#endregion
		#region FindBox Lists
		public void TestTaskAssignedToStaff_List()
		{
			AssertNotNull("TaskAssignedToStaff_List", Lookups.TaskAssignedToStaff_List);
		}

		public void TestServiceLevelList()
		{
			AssertNotNull("ServiceLevelList", Lookups.ServiceLevelList);
		}

		public void TestAccountClassList()
		{
			AssertNotNull("ServiceLevelList", Lookups.AccountClassList);
		}

		#endregion
		#region Implementation
		UPEJobDeclarationFilterLookups Lookups;
		protected override void SetUp()
		{
			base.SetUp();
			UPEJobDeclarationFilterBusinessObject filterBizObj = new UPEJobDeclarationFilterBusinessObject();
			Lookups = filterBizObj.Lookups;
		}
		#endregion
	}
}
