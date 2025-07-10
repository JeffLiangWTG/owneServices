using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsArrivalCargoDescCollection<NctsArrivalCargoDesc>))]
	class NctsArrivalCargoDescCollectionTest : ActiveBusinessObjectCollectionTestCase<INctsArrivalCargoDescCollection<NctsArrivalCargoDesc>>
	{
		public void TestParentIsNctsBill()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();
			bill.ArrivalGoodsItems.AddNew();
			AssertEquals("Arrival goods items are in the collection", 1, bill.ArrivalGoodsItems.Count);
		}

		public void TestParentIsNctsBill_Departure()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();
			bill.GoodsItems.AddNew();
			AssertEquals("Departure goods items are not in the collection", 0, bill.ArrivalGoodsItems.Count);
		}

		public void TestOnAdded()
		{
			AssertEquals("BY_UnloadedState defaults to 'NEW'", NctsUnloadedStateList.Codes.NEW, Collection.AddNew().BY_UnloadedState);
		}

		protected override INctsArrivalCargoDescCollection<NctsArrivalCargoDesc> GetCollectionToTest()
		{
			var moveHeader = Factory.New<NctsArrivalMovementHeaderForTest>();
			return new NctsArrivalCargoDescCollection<NctsArrivalCargoDesc>(moveHeader);
		}

		class NctsArrivalMovementHeaderForTest : NctsArrivalMovementHeader
		{
			public NctsArrivalMovementHeaderForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override INctsCommonCargoDescCollection<NctsCommonCargoDesc> CreateGoodsItems() => new NctsArrivalCargoDescCollection<NctsArrivalCargoDesc>(this);

			protected override Type CusInBondCargoDescTypeCore => typeof(NctsArrivalCargoDesc);
		}
	}
}
