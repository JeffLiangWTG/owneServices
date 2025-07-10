using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	sealed class TemporaryStorageBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBillKindList()
		{
			AssertType<TemporaryStorageBillKindList>(lookups.BillKindList);
		}

		[TestDate(2023, 02, 13)]
		public void TestBillTypeListIsCached()
		{
			var header1 = Factory.New<TemporaryStorageHeader>();
			header1.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;
			var bill1 = header1.Bills.AddNew();
			var bill2 = header1.Bills.AddNew();
			var header2 = Factory.New<TemporaryStorageHeader>();
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Italy;
			var bill3 = header2.Bills.AddNew();
			var header3 = Factory.New<TemporaryStorageHeader>();
			header3.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;
			var bill4 = header3.Bills.AddNew();
			var list = bill1.Lookups.BillTypeList;
			AssertSame("bill1 - bill2", list, bill2.Lookups.BillTypeList);
			AssertEquals("bill1 - bill3", false, object.ReferenceEquals(bill3.Lookups.BillTypeList, list));
			AssertSame("bill1 - bill4", list, bill4.Lookups.BillTypeList);
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			AssertEquals("bill1 - bill1 next date", false, object.ReferenceEquals(bill1.Lookups.BillTypeList, list));
		}

		public void TestBillTypeList()
		{
			TemporaryStorageTestDataHelper.SetUpBillDocumentTypeCodeList(Factory);
			var list = lookups.BillTypeList;
			AssertType<ZZRefCusCodeListCombinedCollection>(list);
			AssertEquals("Bill Docuemnt Type list should contain 14 codes", 14, list.Count);
			AssertContainsExactElementsInExactOrder("Bill Document Type list CodeType should be TD44T", new[] { new ZString("TD44T") }, list.CodeTypes.ToArray());
			AssertContainsExactElementsInAnyOrder("Bill Document Type list codes", new[]
			{
				"C624", "C625", "C664", "C665", "N703", "N704", "N705", "N720", "N722", "N730", "N740", "N741", "N750", "N760"
			}, list.Select(x => x.ZZD_Code));
			AssertContainsExactElementsInAnyOrder("Bill Document Type list descriptions", new[]
			{
				"Form 302",
				"Rhine Manifest",
				"CN22 declaration according to Article 237 of the Regulation (ECC) No 2454/93",
				"CN23 declaration according to Article 237 of the Regulation (ECC) No 2454/93",
				"House waybill",
				"Master bill of lading",
				"Bill of lading",
				"Consignment bill of lading",
				"Road list – SMGS",
				"Road consignment note",
				"Air waybill",
				"Master air waybill",
				"Movement by post including parcel post",
				"Multi-model / combined transport document"
			}, list.Select(x => x.ZZD_Description));
		}

		public void TestGrossWeightUnitList()
		{
			AssertType<CodeDescriptionPairList>(lookups.GrossWeightUnitList);
			AssertEquals("Weight unit quantity", 14, lookups.GrossWeightUnitList.Count);
		}

		public void TestConsignorOrganizationList()
		{
			AssertType<ConsignorCollection>(lookups.ConsignorOrganizationList);
		}

		public void TestConsigneeOrganizationList()
		{
			AssertType<ConsigneeCollection>(lookups.ConsigneeOrganizationList);
		}
		public void TestNotifyPartyOrganizationList()
		{
			AssertType<OrganisationsFindBoxCollection>(lookups.NotifyPartyOrganizationList);
		}

		public void TestTypeOfPersonList()
		{
			AssertType<TypeOfPersonList>(lookups.TypeOfPersonList);
			AssertEquals("Types Of Person quantity", 3, lookups.TypeOfPersonList.Count);
		}

		public void TestShipperState_List()
		{
			SetUpTestState_List();
			AssertEquals("ShipperState_List should have no elements.", 0, bill.Lookups.ShipperState_List.Count);
			bill.ABL_RN_NKShipperCountry = "X7";
			var list = bill.Lookups.ShipperState_List;
			AssertEquals("ShipperState_List should have 2 elements.", 2, list.Count);
			AssertEquals("ST2 DESC 2", list.GetDescriptionFromCode("ST2"));
			AssertEquals("TP1 DESC", list.GetDescriptionFromCode("TP1"));
		}

		public void TestConsigneeState_List()
		{
			SetUpTestState_List();
			AssertEquals("ConsigneeState_List should have no elements.", 0, bill.Lookups.ConsigneeState_List.Count);
			bill.ABL_RN_NKConsigneeCountry = "X7";
			var list = bill.Lookups.ConsigneeState_List;
			AssertEquals("ConsigneeState_List should have 2 elements.", 2, list.Count);
			AssertEquals("ST2 DESC 2", list.GetDescriptionFromCode("ST2"));
			AssertEquals("TP1 DESC", list.GetDescriptionFromCode("TP1"));
		}

		public void TestNotifyPartyState_List()
		{
			SetUpTestState_List();
			AssertEquals("NotifyPartyState_List should have no elements.", 0, bill.Lookups.NotifyPartyState_List.Count);
			bill.ABL_RN_NKNotifyPartyCountry = "X7";
			var list = bill.Lookups.NotifyPartyState_List;
			AssertEquals("NotifyPartyState_List should have 2 elements.", 2, list.Count);
			AssertEquals("ST2 DESC 2", list.GetDescriptionFromCode("ST2"));
			AssertEquals("TP1 DESC", list.GetDescriptionFromCode("TP1"));
		}
		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<TemporaryStorageHeader>();
			bill = header.Bills.AddNew();
			lookups = bill.Lookups;
		}
		TemporaryStorageBillLookups lookups;
		TemporaryStorageBill bill;

		public void SetUpTestState_List()
		{
			var country = Factory.New<RefCountry>();
			country.RN_Code = "X7";
			var state1 = Factory.New<RefCountryStates>();
			var state2 = Factory.New<RefCountryStates>();
			state1.RW_RN_NKCountryCode = country.RN_Code;
			state1.RW_Code = "TP1";
			state1.RW_Description = "TP1 DESC";
			state2.RW_RN_NKCountryCode = country.RN_Code;
			state2.RW_Code = "ST2";
			state2.RW_Description = "ST2 DESC 2";
		}
	}
}
