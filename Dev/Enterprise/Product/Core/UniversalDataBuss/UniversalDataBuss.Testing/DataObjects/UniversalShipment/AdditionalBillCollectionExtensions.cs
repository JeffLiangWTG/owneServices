using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	class AdditionalBillCollectionExtensionsTest : TestCase
	{
		public void TestGetParentAdditionalBill()
		{
			AssertNull(collection.GetParentAdditionalBill("MB1", WayBillTypeList.Codes.Master));
			AssertNull(collection.GetParentAdditionalBill("MB2", WayBillTypeList.Codes.Master));
			AssertEquals(MasterBill1, collection.GetParentAdditionalBill("HB1MB1", WayBillTypeList.Codes.House));
			AssertEquals(MasterBill1, collection.GetParentAdditionalBill("HB2MB1", WayBillTypeList.Codes.House));
			AssertEquals(MasterBill2, collection.GetParentAdditionalBill("HB1MB2", WayBillTypeList.Codes.House));
			AssertEquals(HouseBil2OfMasterBill1, collection.GetParentAdditionalBill("SBHB2MB1", WayBillTypeList.Codes.SubHouse));
			AssertEquals(HouseBil1OfMasterBill2, collection.GetParentAdditionalBill("SBHB1MB2", WayBillTypeList.Codes.SubHouse));
			AssertNull(collection.GetParentAdditionalBill("HB1MB1", WayBillTypeList.Codes.SubHouse));
			AssertNull(collection.GetParentAdditionalBill("HB1MB1", ""));
			AssertNull(collection.GetParentAdditionalBill("HB1MB1", "Z!"));
			Func<AdditionalBill, bool> extraMatch = null;
			AssertEquals(MasterBill1, collection.GetParentAdditionalBill("HB1MB1", WayBillTypeList.Codes.House, extraMatch));
			AssertEquals(MasterBill1, collection.GetParentAdditionalBill("HB2MB1", WayBillTypeList.Codes.House, extraMatch));
			AssertEquals(MasterBill2, collection.GetParentAdditionalBill("HB1MB2", WayBillTypeList.Codes.House, extraMatch));
			AssertEquals(HouseBil2OfMasterBill1, collection.GetParentAdditionalBill("SBHB2MB1", WayBillTypeList.Codes.SubHouse, extraMatch));
			AssertEquals(HouseBil1OfMasterBill2, collection.GetParentAdditionalBill("SBHB1MB2", WayBillTypeList.Codes.SubHouse, extraMatch));
			extraMatch = (x) => x.NoOfPacks.GetValueOrDefault() == 15m;
			AssertEquals(MasterBill1B, collection.GetParentAdditionalBill("HB1MB1", WayBillTypeList.Codes.House, extraMatch));
			AssertEquals(MasterBill1B, collection.GetParentAdditionalBill("HB2MB1", WayBillTypeList.Codes.House, extraMatch));
			AssertEquals(MasterBill2B, collection.GetParentAdditionalBill("HB1MB2", WayBillTypeList.Codes.House, extraMatch));
			AssertEquals(HouseBil2OfMasterBill1B, collection.GetParentAdditionalBill("SBHB2MB1", WayBillTypeList.Codes.SubHouse, extraMatch));
			AssertEquals(HouseBil1OfMasterBill2B, collection.GetParentAdditionalBill("SBHB1MB2", WayBillTypeList.Codes.SubHouse, extraMatch));
		}

		public void TestGetChildrenAdditionalBill()
		{
			AssertContainsExactElementsInAnyOrder(new AdditionalBill[] { HouseBil1OfMasterBill1, HouseBil2OfMasterBill1, HouseBil1OfMasterBill1B, HouseBil2OfMasterBill1B }, collection.GetChildrenAdditionalBill("MB1", WayBillTypeList.Codes.Master));
			AssertContainsExactElementsInAnyOrder(new AdditionalBill[] { HouseBil1OfMasterBill2, HouseBil1OfMasterBill2B }, collection.GetChildrenAdditionalBill("MB2", WayBillTypeList.Codes.Master));
			AssertEquals(0, collection.GetChildrenAdditionalBill("HB1MB1", WayBillTypeList.Codes.House).Count());
			AssertContainsExactElementsInAnyOrder(new AdditionalBill[] { SubHouseOfHouseBil2OfMasterBill1, SubHouseOfHouseBil2OfMasterBill1B }, collection.GetChildrenAdditionalBill("HB2MB1", WayBillTypeList.Codes.House));
			AssertContainsExactElementsInAnyOrder(new AdditionalBill[] { SubHouseOfHouseBil1OfMasterBill2, SubHouseOfHouseBil1OfMasterBill2B }, collection.GetChildrenAdditionalBill("HB1MB2", WayBillTypeList.Codes.House));
			AssertEquals(0, collection.GetChildrenAdditionalBill("SBHB2MB1", WayBillTypeList.Codes.SubHouse).Count());
			AssertEquals(0, collection.GetChildrenAdditionalBill("SBHB1MB2", WayBillTypeList.Codes.SubHouse).Count());
			AssertEquals(0, collection.GetChildrenAdditionalBill("HB1MB1", WayBillTypeList.Codes.SubHouse).Count());
			AssertEquals(0, collection.GetChildrenAdditionalBill("HB1MB1", "").Count());
			AssertEquals(0, collection.GetChildrenAdditionalBill("HB1MB1", "Z!").Count());
			Func<AdditionalBill, bool> extraMatch = null;
			AssertContainsExactElementsInAnyOrder(new AdditionalBill[] { HouseBil1OfMasterBill1, HouseBil2OfMasterBill1, HouseBil1OfMasterBill1B, HouseBil2OfMasterBill1B }, collection.GetChildrenAdditionalBill("MB1", WayBillTypeList.Codes.Master, extraMatch));
			AssertContainsExactElementsInAnyOrder(new AdditionalBill[] { HouseBil1OfMasterBill2, HouseBil1OfMasterBill2B }, collection.GetChildrenAdditionalBill("MB2", WayBillTypeList.Codes.Master, extraMatch));
			AssertContainsExactElementsInAnyOrder(new AdditionalBill[] { SubHouseOfHouseBil2OfMasterBill1, SubHouseOfHouseBil2OfMasterBill1B }, collection.GetChildrenAdditionalBill("HB2MB1", WayBillTypeList.Codes.House, extraMatch));
			AssertContainsExactElementsInAnyOrder(new AdditionalBill[] { SubHouseOfHouseBil1OfMasterBill2, SubHouseOfHouseBil1OfMasterBill2B }, collection.GetChildrenAdditionalBill("HB1MB2", WayBillTypeList.Codes.House, extraMatch));
			extraMatch = (x) => x.NoOfPacks.GetValueOrDefault() == 15m;
			AssertContainsExactElementsInAnyOrder(new AdditionalBill[] { HouseBil1OfMasterBill1B, HouseBil2OfMasterBill1B }, collection.GetChildrenAdditionalBill("MB1", WayBillTypeList.Codes.Master, extraMatch));
			AssertContainsExactElementsInAnyOrder(new AdditionalBill[] { HouseBil1OfMasterBill2B }, collection.GetChildrenAdditionalBill("MB2", WayBillTypeList.Codes.Master, extraMatch));
			AssertContainsExactElementsInAnyOrder(new AdditionalBill[] { SubHouseOfHouseBil2OfMasterBill1B }, collection.GetChildrenAdditionalBill("HB2MB1", WayBillTypeList.Codes.House, extraMatch));
			AssertContainsExactElementsInAnyOrder(new AdditionalBill[] { SubHouseOfHouseBil1OfMasterBill2B }, collection.GetChildrenAdditionalBill("HB1MB2", WayBillTypeList.Codes.House, extraMatch));
		}

		public void TestGetAdditionalBill()
		{
			AssertEquals(MasterBill1, collection.GetAdditionalBill("MB1", WayBillTypeList.Codes.Master));
			AssertEquals(MasterBill2, collection.GetAdditionalBill("MB2", WayBillTypeList.Codes.Master));
			AssertEquals(HouseBil1OfMasterBill1, collection.GetAdditionalBill("HB1MB1", WayBillTypeList.Codes.House));
			AssertEquals(HouseBil2OfMasterBill1, collection.GetAdditionalBill("HB2MB1", WayBillTypeList.Codes.House));
			AssertEquals(HouseBil1OfMasterBill2, collection.GetAdditionalBill("HB1MB2", WayBillTypeList.Codes.House));
			AssertEquals(SubHouseOfHouseBil2OfMasterBill1, collection.GetAdditionalBill("SBHB2MB1", WayBillTypeList.Codes.SubHouse));
			AssertEquals(SubHouseOfHouseBil1OfMasterBill2, collection.GetAdditionalBill("SBHB1MB2", WayBillTypeList.Codes.SubHouse));
			AssertNull(collection.GetAdditionalBill("HB1MB1", WayBillTypeList.Codes.SubHouse));
			AssertNull(collection.GetAdditionalBill("HB1MB1", ""));
			AssertNull(collection.GetAdditionalBill("HB1MB1", "Z!"));
			Func<AdditionalBill, bool> extraMatch = null;
			AssertEquals(MasterBill1, collection.GetAdditionalBill("MB1", WayBillTypeList.Codes.Master, extraMatch));
			AssertEquals(MasterBill2, collection.GetAdditionalBill("MB2", WayBillTypeList.Codes.Master, extraMatch));
			AssertEquals(HouseBil1OfMasterBill1, collection.GetAdditionalBill("HB1MB1", WayBillTypeList.Codes.House, extraMatch));
			AssertEquals(HouseBil2OfMasterBill1, collection.GetAdditionalBill("HB2MB1", WayBillTypeList.Codes.House, extraMatch));
			AssertEquals(HouseBil1OfMasterBill2, collection.GetAdditionalBill("HB1MB2", WayBillTypeList.Codes.House, extraMatch));
			AssertEquals(SubHouseOfHouseBil2OfMasterBill1, collection.GetAdditionalBill("SBHB2MB1", WayBillTypeList.Codes.SubHouse, extraMatch));
			AssertEquals(SubHouseOfHouseBil1OfMasterBill2, collection.GetAdditionalBill("SBHB1MB2", WayBillTypeList.Codes.SubHouse, extraMatch));
			extraMatch = (x) => x.NoOfPacks.GetValueOrDefault() == 15m;
			AssertEquals(MasterBill1B, collection.GetAdditionalBill("MB1", WayBillTypeList.Codes.Master, extraMatch));
			AssertEquals(MasterBill2B, collection.GetAdditionalBill("MB2", WayBillTypeList.Codes.Master, extraMatch));
			AssertEquals(HouseBil1OfMasterBill1B, collection.GetAdditionalBill("HB1MB1", WayBillTypeList.Codes.House, extraMatch));
			AssertEquals(HouseBil2OfMasterBill1B, collection.GetAdditionalBill("HB2MB1", WayBillTypeList.Codes.House, extraMatch));
			AssertEquals(HouseBil1OfMasterBill2B, collection.GetAdditionalBill("HB1MB2", WayBillTypeList.Codes.House, extraMatch));
			AssertEquals(SubHouseOfHouseBil2OfMasterBill1B, collection.GetAdditionalBill("SBHB2MB1", WayBillTypeList.Codes.SubHouse, extraMatch));
			AssertEquals(SubHouseOfHouseBil1OfMasterBill2B, collection.GetAdditionalBill("SBHB1MB2", WayBillTypeList.Codes.SubHouse, extraMatch));
		}

		List<AdditionalBill> collection;
		AdditionalBill MasterBill1;
		AdditionalBill MasterBill2;
		AdditionalBill HouseBil1OfMasterBill1;
		AdditionalBill HouseBil2OfMasterBill1;
		AdditionalBill HouseBil1OfMasterBill2;
		AdditionalBill SubHouseOfHouseBil2OfMasterBill1;
		AdditionalBill SubHouseOfHouseBil1OfMasterBill2;
		AdditionalBill MasterBill1B;
		AdditionalBill MasterBill2B;
		AdditionalBill HouseBil1OfMasterBill1B;
		AdditionalBill HouseBil2OfMasterBill1B;
		AdditionalBill HouseBil1OfMasterBill2B;
		AdditionalBill SubHouseOfHouseBil2OfMasterBill1B;
		AdditionalBill SubHouseOfHouseBil1OfMasterBill2B;

		protected override void SetUp()
		{
			base.SetUp();
			var billTypeList = new WayBillTypeList();
			collection = new List<AdditionalBill>();
			collection.Add(MasterBill1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB1", BillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.Master, billTypeList), NoOfPacks = 10m });
			collection.Add(MasterBill2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB2", BillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.Master, billTypeList), ParentBillNumber = ZString.Empty, NoOfPacks = 10m });
			collection.Add(HouseBil1OfMasterBill1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "HB1MB1", BillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.House, billTypeList), ParentBillNumber = "MB1", NoOfPacks = 10m });
			collection.Add(HouseBil2OfMasterBill1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "HB2MB1", BillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.House, billTypeList), ParentBillNumber = "MB1", NoOfPacks = 10m });
			collection.Add(HouseBil1OfMasterBill2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "HB1MB2", BillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.House, billTypeList), ParentBillNumber = "MB2", NoOfPacks = 10m });
			collection.Add(SubHouseOfHouseBil2OfMasterBill1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "SBHB2MB1", BillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.SubHouse, billTypeList), ParentBillNumber = "HB2MB1", NoOfPacks = 10m });
			collection.Add(SubHouseOfHouseBil1OfMasterBill2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "SBHB1MB2", BillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.SubHouse, billTypeList), ParentBillNumber = "HB1MB2", NoOfPacks = 10m });
			collection.Add(MasterBill1B = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB1", BillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.Master, billTypeList), NoOfPacks = 15m });
			collection.Add(MasterBill2B = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB2", BillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.Master, billTypeList), ParentBillNumber = ZString.Empty, NoOfPacks = 15m });
			collection.Add(HouseBil1OfMasterBill1B = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "HB1MB1", BillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.House, billTypeList), ParentBillNumber = "MB1", NoOfPacks = 15m });
			collection.Add(HouseBil2OfMasterBill1B = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "HB2MB1", BillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.House, billTypeList), ParentBillNumber = "MB1", NoOfPacks = 15m });
			collection.Add(HouseBil1OfMasterBill2B = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "HB1MB2", BillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.House, billTypeList), ParentBillNumber = "MB2", NoOfPacks = 15m });
			collection.Add(SubHouseOfHouseBil2OfMasterBill1B = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "SBHB2MB1", BillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.SubHouse, billTypeList), ParentBillNumber = "HB2MB1", NoOfPacks = 15m });
			collection.Add(SubHouseOfHouseBil1OfMasterBill2B = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "SBHB1MB2", BillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.SubHouse, billTypeList), ParentBillNumber = "HB1MB2", NoOfPacks = 15m });
		}
	}
}

