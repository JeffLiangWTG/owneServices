using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS.Testing
{
	[TestedType(typeof(NctsDepartureCargoDescWrapperCollection))]
	sealed class NctsCargoDescWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NctsDepartureCargoDescWrapperCollection>
	{
		protected override NctsDepartureCargoDescWrapperCollection GetCollectionToTest()
		{
			return (NctsDepartureCargoDescWrapperCollection)NctsHeaderDocumentWrapperTest.NctsHeaderDocumentWrapperWithoutSecurity(Factory).Lines;
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(NctsDepartureCargoDescWrapperCollection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			NctsDepartureCargoDesc line = Factory.New<NctsDepartureCargoDesc>();
			return NctsDepartureCargoDescWrapper.New(line);
		}

		public void TestAddGoodsItemsFromPhase4Header()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			var goodsItem1 = movementHeader.GoodsItems.AddNew();
			goodsItem1.BY_Description = "ITEM 1";
			var goodsItem2 = movementHeader.GoodsItems.AddNew();
			goodsItem2.BY_Description = "ITEM 2";

			var collection = new NctsDepartureCargoDescWrapperCollection(nctsHeader, Factory);
			AssertEquals(2, collection.Count);
			AssertEquals(goodsItem1.BY_Description, collection[0].BOX312DESCRIPTION);
			AssertEquals(goodsItem2.BY_Description, collection[1].BOX312DESCRIPTION);
		}

		public void TestAddGoodsItemsFromPhase5Header()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill1 = nctsHeader.Bills.AddNew();
			var goodsItem1 = bill1.GoodsItems.AddNew();
			goodsItem1.BY_Description = "ITEM 1";
			var goodsItem2 = bill1.GoodsItems.AddNew();
			goodsItem2.BY_Description = "ITEM 2";
			var bill2 = nctsHeader.Bills.AddNew();
			var goodsItem3 = bill2.GoodsItems.AddNew();
			goodsItem3.BY_Description = "ITEM 3";

			var collection1 = new NctsDepartureCargoDescWrapperCollection(nctsHeader, Factory);
			AssertEquals(3, collection1.Count);
			AssertEquals(goodsItem1.BY_Description, collection1[0].BOX312DESCRIPTION);
			AssertEquals(goodsItem2.BY_Description, collection1[1].BOX312DESCRIPTION);
			AssertEquals(goodsItem3.BY_Description, collection1[2].BOX312DESCRIPTION);

			var collection2 = new NctsDepartureCargoDescWrapperCollection(bill1, Factory);
			AssertEquals(2, collection2.Count);
			AssertEquals(goodsItem1.BY_Description, collection2[0].BOX312DESCRIPTION);
			AssertEquals(goodsItem2.BY_Description, collection2[1].BOX312DESCRIPTION);
		}
	}
}
