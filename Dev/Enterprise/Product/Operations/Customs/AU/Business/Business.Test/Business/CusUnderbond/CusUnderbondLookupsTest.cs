using Enterprise.Customs.Common.AU.CMR;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusUnderbondLookups))]
	public class CusUnderbondLookupsTest : Customs.Business.Testing.CusUnderbondLookupsTest
	{
		public void TestModeOfTransportListIsCached()
		{
			var bizObj1 = Factory.New<CusUnderbond>();
			Assert("ModeOfTransportList should be cached in same factory", ReferenceEquals(Factory.GetCachedValue<CMRUnderbondModeOfMovement>(), bizObj1.Lookups.ModeOfTransportList));
			var bizObj2 = Factory.New<CusUnderbond>();
			Assert("ModeOfTransportList should be cached in same factory", ReferenceEquals(bizObj1.Lookups.ModeOfTransportList, bizObj2.Lookups.ModeOfTransportList));
		}

		public void TestRequestReasonListIsCached()
		{
			var bizObj1 = Factory.New<CusUnderbond>();
			bizObj1.C4_ParentTableCode = CusHAWBSchema.Constants.Prefix;
			Assert("RequestReasonList should be cached in same factory", ReferenceEquals(Factory.GetCachedValue<CMRUnderbondRequestCodes>(), bizObj1.Lookups.RequestReasonList));
			var bizObj2 = Factory.New<CusUnderbond>();
			bizObj2.C4_ParentTableCode = CusHAWBSchema.Constants.Prefix;
			Assert("RequestReasonList should be cached in same factory", ReferenceEquals(bizObj1.Lookups.RequestReasonList, bizObj2.Lookups.RequestReasonList));
		}

		public void TestUnderbondStatusListIsCached()
		{
			var bizObj1 = Factory.New<CusUnderbond>();
			Assert("UnderbondStatusList should be cached in same factory", ReferenceEquals(CMRBaseAndUnderbondStatuses.GetStatuses(Factory), bizObj1.Lookups.UnderbondStatusList));
			var bizObj2 = Factory.New<CusUnderbond>();
			Assert("UnderbondStatusList should be cached in same factory", ReferenceEquals(bizObj1.Lookups.UnderbondStatusList, bizObj2.Lookups.UnderbondStatusList));
		}

		public void TestOutturnStatusListIsCached()
		{
			var bizObj1 = Factory.New<CusUnderbond>();
			Assert("OutturnStatusList should be cached in same factory", ReferenceEquals(Factory.GetCachedValue<CMRBaseStatuses>(), bizObj1.Lookups.OutturnStatusList));
			var bizObj2 = Factory.New<CusUnderbond>();
			Assert("OutturnStatusList should be cached in same factory", ReferenceEquals(bizObj1.Lookups.OutturnStatusList, bizObj2.Lookups.OutturnStatusList));
		}

		#region TestAllUnderbondsForList

		public void TestAllUnderbondsForList()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container1 = oceanBill.Containers.AddNew();
			container1.CN_ContainerNumber = "CONTAINER1";
			CusSCAContainer container2 = oceanBill.Containers.AddNew();
			container2.CN_ContainerNumber = "CONTAINER2";
			CusSCAHouse house1 = oceanBill.HouseBills.AddNew();
			house1.CA_HouseBill = "HOUSEBILL1";
			CusSCAHouse house2 = oceanBill.HouseBills.AddNew();
			house2.CA_HouseBill = "HOUSEBILL2";

			var pack1 = container1.Pivots.AddNew();
			house1.Pivot.Add(pack1);

			var pack2 = container1.Pivots.AddNew();
			house2.Pivot.Add(pack2);

			CusUnderbond underbond = container1.Underbonds.AddNew();

			AssertEquals("AllUnderbondForList Count", 4, underbond.Lookups.AllUnderbondForList.Count);
			Assert("List should include CONTAINER1", underbond.Lookups.AllUnderbondForList.ContainsCode("Container CONTAINER1"));
			Assert("List should include CONTAINER2", underbond.Lookups.AllUnderbondForList.ContainsCode("Container CONTAINER2"));
			Assert("List should include CONTAINER1 - HOUSEBILL1", underbond.Lookups.AllUnderbondForList.ContainsCode("Container CONTAINER1 - Housebill HOUSEBILL1"));
			Assert("List should include CONTAINER1 - HOUSEBILL2", underbond.Lookups.AllUnderbondForList.ContainsCode("Container CONTAINER1 - Housebill HOUSEBILL2"));
		}

		#endregion

		#region Implementation

		protected override int ExpectedModeOfTransportListCount
		{
			get { return 5; }
		}

		protected override int ExpectedRequestReasonListCount
		{
			get { return 6; }
		}

		protected override int ExpectedUnderbondStatusListCount
		{
			get { return 16; }
		}

		protected override int ExpectedOutturnStatusListCount
		{
			get { return 10; }
		}

		protected override Customs.Business.CusUnderbond CreateNewUnderbond() => Factory.New<CusUnderbond>();

		#endregion
	}
}
