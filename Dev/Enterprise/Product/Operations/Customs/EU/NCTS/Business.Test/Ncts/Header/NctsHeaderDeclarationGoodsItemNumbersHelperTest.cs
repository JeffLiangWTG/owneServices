using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsHeaderDeclarationGoodsItemNumbersHelperTest : TestCaseWithFactory
	{
		public void TestGuardClause()
		{
			AssertExceptionThrown<ArgumentNullException>("When header is null",
				() => NctsHeaderDeclarationGoodsItemNumbersHelper.AssignUnassignedDeclarationGoodsItemNumbers(null));
		}

		public void TestAssignUnassignedDeclarationGoodsItemNumber()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var bill1 = header.Bills.AddNew();
			bill1.SequenceNumber = 2;

			var goodsItem1 = bill1.GoodsItems.AddNew();
			goodsItem1.BY_DeclarationGoodsItemNumber = 1;
			var goodsItem2 = bill1.GoodsItems.AddNew();

			var bill2 = header.Bills.AddNew();
			bill2.SequenceNumber = 1;

			var goodsItem3 = bill2.GoodsItems.AddNew();
			goodsItem3.BY_LineNo = 2;
			var goodsItem4 = bill2.GoodsItems.AddNew();
			goodsItem4.BY_DeclarationGoodsItemNumber = 5;
			goodsItem4.BY_LineNo = 3;
			var goodsItem5 = bill2.GoodsItems.AddNew();
			goodsItem5.BY_LineNo = 1;

			var goodsItemCollection = header.GetGoodsItems();

			AssertEquals("[PRE-CONDITION]: Number of Goods Items with empty DeclarationGoodsItemNumber", 3, goodsItemCollection.Count(e => e.BY_DeclarationGoodsItemNumber.IsEmpty));

			NctsHeaderDeclarationGoodsItemNumbersHelper.AssignUnassignedDeclarationGoodsItemNumbers(header);
			AssertEquals("Number of Goods Items with empty DeclarationGoodsItemNumber", 0, goodsItemCollection.Count(e => e.BY_DeclarationGoodsItemNumber.IsEmpty));

			CombineAssertions("The function should populate BY_DeclarationGoodsItemNumber with the lowest value available", () =>
			{
				AssertEquals("GoodsItem1", 1, goodsItem1.BY_DeclarationGoodsItemNumber);
				AssertEquals("GoodsItem2", 8, goodsItem2.BY_DeclarationGoodsItemNumber);

				AssertEquals("GoodsItem3", 7, goodsItem3.BY_DeclarationGoodsItemNumber);
				AssertEquals("GoodsItem4", 5, goodsItem4.BY_DeclarationGoodsItemNumber);
				AssertEquals("GoodsItem5", 6, goodsItem5.BY_DeclarationGoodsItemNumber);
			});
		}

		public void TestAssignUnassignedDeclarationGoodsItemNumber_WhenFilter()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var bill = header.Bills.AddNew();

			var goodsItem = bill.GoodsItems.AddNew();
			var goodsItemXXX = bill.GoodsItems.AddNew();
			goodsItemXXX.BY_Description = "XXX";

			NctsHeaderDeclarationGoodsItemNumbersHelper.AssignUnassignedDeclarationGoodsItemNumbers(header, Array.Empty<NctsCommonCargoDesc>());

			CombineAssertions(() =>
			{
				AssertEquals(0, goodsItem.BY_DeclarationGoodsItemNumber);
				AssertEquals(0, goodsItemXXX.BY_DeclarationGoodsItemNumber);
			});

			NctsHeaderDeclarationGoodsItemNumbersHelper.AssignUnassignedDeclarationGoodsItemNumbers(header, filter: x => x.BY_Description == "XXX");

			CombineAssertions(() =>
			{
				AssertEquals(0, goodsItem.BY_DeclarationGoodsItemNumber);
				AssertEquals(1, goodsItemXXX.BY_DeclarationGoodsItemNumber);
			});
		}

		public void TestIsDeclarationGoodsItemNumbersNonSequential()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			Assert("No GoodsItems present.", !NctsHeaderDeclarationGoodsItemNumbersHelper.IsDeclarationGoodsItemNumbersNonSequential(header));

			AddGoodsItemWithDeclarationGoodsItemNumber(bill1, 1);
			AddGoodsItemWithDeclarationGoodsItemNumber(bill1, 2);
			AddGoodsItemWithDeclarationGoodsItemNumber(bill1, 3);
			AddGoodsItemWithDeclarationGoodsItemNumber(bill2, 4);
			AddGoodsItemWithDeclarationGoodsItemNumber(bill2, 5);
			var goodsItems = header.GetGoodsItems();

			Assert("DeclarationGoodsItemNumber are sequential.", !NctsHeaderDeclarationGoodsItemNumbersHelper.IsDeclarationGoodsItemNumbersNonSequential(header));

			goodsItems.FirstOrDefault(g => g.BY_DeclarationGoodsItemNumber == 4).BY_DeclarationGoodsItemNumber = 6;
			Assert("DeclarationGoodsItemNumber are not sequential.", NctsHeaderDeclarationGoodsItemNumbersHelper.IsDeclarationGoodsItemNumbersNonSequential(header));

			goodsItems.FirstOrDefault(g => g.BY_DeclarationGoodsItemNumber == 6).BY_DeclarationGoodsItemNumber = 4;
			AddGoodsItemWithDeclarationGoodsItemNumber(bill1, 0);
			Assert("DeclarationGoodsItemNumbers throughout the bills should start from 1 and be sequential.", NctsHeaderDeclarationGoodsItemNumbersHelper.IsDeclarationGoodsItemNumbersNonSequential(header));

			goodsItems = header.GetGoodsItems();
			goodsItems.FirstOrDefault(g => g.BY_DeclarationGoodsItemNumber == 0).BY_DeclarationGoodsItemNumber = 3;
			Assert("Different bills cannot have duplicate DeclarationGoodsItemNumbers.", NctsHeaderDeclarationGoodsItemNumbersHelper.IsDeclarationGoodsItemNumbersNonSequential(header));

			goodsItems.FirstOrDefault(g => g.BY_DeclarationGoodsItemNumber == 2).BY_DeclarationGoodsItemNumber = 6;
			Assert("DeclarationGoodsItemNumber are not sequential.", NctsHeaderDeclarationGoodsItemNumbersHelper.IsDeclarationGoodsItemNumbersNonSequential(header));
		}

		public void TestSetDeclarationGoodsItemNumbersToZero()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			AddGoodsItemWithDeclarationGoodsItemNumber(bill1, 1);
			AddGoodsItemWithDeclarationGoodsItemNumber(bill1, 2);
			AddGoodsItemWithDeclarationGoodsItemNumber(bill1, 3);
			AddGoodsItemWithDeclarationGoodsItemNumber(bill2, 4);
			AddGoodsItemWithDeclarationGoodsItemNumber(bill2, 5);
			var goodsItems = header.GetGoodsItems();

			Assert("[PRE-CONDITION] None of the GoodsItems have BY_DeclarationGoodsItemNumber set to zero.", !goodsItems.Any(g => g.BY_DeclarationGoodsItemNumber == 0));

			NctsHeaderDeclarationGoodsItemNumbersHelper.SetDeclarationGoodsItemNumbersToZero(header);
			Assert("All the GoodsItems have BY_DeclarationGoodsItemNumber set to zero.", goodsItems.All(g => g.BY_DeclarationGoodsItemNumber == 0));
		}

		void AddGoodsItemWithDeclarationGoodsItemNumber(NctsBill bill, ZInt number)
		{
			var goodsItem = bill.GoodsItems.AddNew();
			goodsItem.BY_DeclarationGoodsItemNumber = number;
		}
	}
}
