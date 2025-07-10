using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCountries()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var bill = header.Bills.AddNew();
			AssertEquals("Countries must always only be the parent country.", 1, bill.Lookups.Countries.Count);
			AssertEquals("Bill country must match parent country, and so should list.", Core.Constants.CountryCodes.Eritrea, bill.Lookups.Countries.First().Code);
		}

		public void TestMessageStatusList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals("MessageStatusList", true, object.ReferenceEquals(Factory.GetCachedValue<MessageStatusCodeList>(), bill.Lookups.MessageStatusList));
		}

		public void TestCustomsStatusList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "CustomsManifestStatus");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "8", "Proceed to Border", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "9", "Already on Customs system", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "9", "Already on Customs system", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGI");
			var bill = header.Bills.AddNew();
			var list = bill.Lookups.CustomsStatusList;
			AssertEquals("Proceed to Border", list.GetDescriptionFromCode("8"));
			AssertEquals("Already on Customs system", list.GetDescriptionFromCode("9"));
			AssertEquals(false, list.ContainsCode("10"));
		}

		public void TestLocations()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "SB", "Solomon Islands", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			helper.CreateNewOrGetExistingCusCodeList("SB", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "HONIARA SEAPORT", "Honiara International Seaport", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "SB";
			var consol = Factory.New<ForwardingConsol>();
			header.SetParent(consol);
			header.AMA_RL_NKPortOfLoading = "SBHIR";
			header.AMA_RL_NKPortOfDischarge = "GBFXT";
			var bill = header.Bills.AddNew();
			bill.ABL_RL_NKOrigin = "SBHIR";
			bill.ABL_RL_NKFinalDestination = "GBFXT";

			if (bill.Lookups.Locations is CodeDescriptionPairList locationsList)
			{
				AssertEquals(true, locationsList.ContainsCode("HONIARA SEAPORT"));
			}
		}

		public void TestBillIssuers()
		{
			var carrier1_Sea = Factory.New<ZZRefCarrierCombined>();
			carrier1_Sea.ZZ4_Code = "111";
			carrier1_Sea.ZZ4_CountryOrGrouping = "XX";
			carrier1_Sea.ZZ4_Description = "One";
			var attrib1A = carrier1_Sea.Attributes.AddNew();
			attrib1A.ZZG_Name = Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER;
			attrib1A.ZZG_Value = attrib1A.ZZG_Name;
			var attrib1B = carrier1_Sea.Attributes.AddNew();
			attrib1B.ZZG_Name = "SEA";
			attrib1B.ZZG_Value = attrib1B.ZZG_Name;

			var carrier2_Poop = Factory.New<ZZRefCarrierCombined>();
			carrier2_Poop.ZZ4_Code = "222";
			carrier2_Poop.ZZ4_CountryOrGrouping = "XX";
			carrier2_Poop.ZZ4_Description = "Two";
			var attrib2A = carrier2_Poop.Attributes.AddNew();
			attrib2A.ZZG_Name = "POOP";
			attrib2A.ZZG_Value = attrib2A.ZZG_Name;
			var attrib2B = carrier2_Poop.Attributes.AddNew();
			attrib2B.ZZG_Name = "SEA";
			attrib2B.ZZG_Value = attrib2B.ZZG_Name;

			var carrier3_Air = Factory.New<ZZRefCarrierCombined>();
			carrier3_Air.ZZ4_Code = "333";
			carrier3_Air.ZZ4_CountryOrGrouping = "XX";
			carrier3_Air.ZZ4_Description = "Three";
			var attrib3A = carrier3_Air.Attributes.AddNew();
			attrib3A.ZZG_Name = Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER;
			attrib3A.ZZG_Value = attrib3A.ZZG_Name;
			var attrib3B = carrier3_Air.Attributes.AddNew();
			attrib3B.ZZG_Name = "AIR";
			attrib3B.ZZG_Value = attrib3B.ZZG_Name;
			Factory.Save();

			var carrier4_WrongCountry = Factory.New<ZZRefCarrierCombined>();
			carrier4_WrongCountry.ZZ4_Code = "444";
			carrier4_WrongCountry.ZZ4_CountryOrGrouping = "YY";  // Note
			carrier4_WrongCountry.ZZ4_Description = "Four";
			var attrib4A = carrier4_WrongCountry.Attributes.AddNew();
			attrib4A.ZZG_Name = Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER;
			attrib4A.ZZG_Value = attrib4A.ZZG_Name;
			var attrib4B = carrier4_WrongCountry.Attributes.AddNew();
			attrib4B.ZZG_Name = "AIR";
			attrib4B.ZZG_Value = attrib4B.ZZG_Name;
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "XX";
			var bill = header.Bills.AddNew();

			var carriers = bill.Lookups.BillIssuers;
			AssertEquals(2, carriers.Count);
			AssertNotNull(carriers.FindByPK(carrier1_Sea.PK));
			AssertNotNull(carriers.FindByPK(carrier3_Air.PK));

			header.AMA_TransportMode = "SEA";
			carriers = bill.Lookups.BillIssuers;
			AssertEquals(1, carriers.Count);
			AssertEquals(carrier1_Sea.PK, ((BusinessObject)carriers[0]).PK);

			header.AMA_TransportMode = "AIR";
			carriers = bill.Lookups.BillIssuers;
			AssertEquals(1, carriers.Count);
			AssertEquals(carrier3_Air.PK, ((BusinessObject)carriers[0]).PK);

			header.AMA_RN_NKCountry = "YY";
			carriers = bill.Lookups.BillIssuers;
			AssertEquals(1, carriers.Count);
			AssertEquals(carrier4_WrongCountry.PK, ((BusinessObject)carriers[0]).PK);
		}

		public void TestBillTypes()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertContains("STD", bill.Lookups.BolTypes.CodesAsString);
			AssertContains("CLD", bill.Lookups.BolTypes.CodesAsString);
		}

		public void TestProperties()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertNotNull(bill.Lookups.DiscountValueCurrencies);
			AssertNotNull(bill.Lookups.OtherChargesValueCurrencies);
			AssertEquals(typeof(RefCurrencyCollection), bill.Lookups.DiscountValueCurrencies.GetType());
			AssertEquals(typeof(RefCurrencyCollection), bill.Lookups.OtherChargesValueCurrencies.GetType());
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			manifestHeader.Bills.Add(bill);
			AssertNotNull(bill.Lookups.CustomsLoadingPortList);
			AssertEquals(typeof(ZZRefCusCodeListCombinedCollection), bill.Lookups.CustomsLoadingPortList.GetType());
			AssertNotNull(bill.Lookups.CustomsDischargePortList);
			AssertEquals(typeof(ZZRefCusCodeListCombinedCollection), bill.Lookups.CustomsDischargePortList.GetType());
		}

		public void TestShipperState_List()
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

			var bill = Factory.New<AsycudaBill>();
			AssertEquals("ShipperState_List should have no elements.", 0, bill.Lookups.ShipperState_List.Count);

			bill.ABL_RN_NKShipperCountry = "X7";
			var list = bill.Lookups.ShipperState_List;
			AssertEquals("ShipperState_List should have 2 elements.", 2, list.Count);
			AssertEquals("ST2 DESC 2", list.GetDescriptionFromCode("ST2"));
			AssertEquals("TP1 DESC", list.GetDescriptionFromCode("TP1"));
		}

		public void TestConsigneeState_List()
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

			var bill = Factory.New<AsycudaBill>();
			AssertEquals("ConsigneeState_List should have no elements.", 0, bill.Lookups.ConsigneeState_List.Count);

			bill.ABL_RN_NKConsigneeCountry = "X7";
			var list = bill.Lookups.ConsigneeState_List;
			AssertEquals("ConsigneeState_List should have 2 elements.", 2, list.Count);
			AssertEquals("ST2 DESC 2", list.GetDescriptionFromCode("ST2"));
			AssertEquals("TP1 DESC", list.GetDescriptionFromCode("TP1"));
		}

		public void TestNotifyPartyState_List()
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

			var bill = Factory.New<AsycudaBill>();
			AssertEquals("NotifyPartyState_List should have no elements.", 0, bill.Lookups.NotifyPartyState_List.Count);

			bill.ABL_RN_NKNotifyPartyCountry = "X7";
			var list = bill.Lookups.NotifyPartyState_List;
			AssertEquals("NotifyPartyState_List should have 2 elements.", 2, list.Count);
			AssertEquals("ST2 DESC 2", list.GetDescriptionFromCode("ST2"));
			AssertEquals("TP1 DESC", list.GetDescriptionFromCode("TP1"));
		}

		public void TestListContent()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertContains("BAG", bill.Lookups.PackageTypeList.CodesAsString);
			AssertContains("KG", bill.Lookups.WeightUQList.CodesAsString);
			AssertContains("M3", bill.Lookups.VolumeUQList.CodesAsString);
			AssertContains("EXP", bill.Lookups.ShipmentTypesList.CodesAsString);
			AssertContains("IMP", bill.Lookups.ShipmentTypesList.CodesAsString);
			AssertContains("PPD", bill.Lookups.PrepaidCollectList.CodesAsString);
			AssertContains("CLT", bill.Lookups.PrepaidCollectList.CodesAsString);
		}

		public void TestIncotermList()
		{
			var bill = Factory.New<AsycudaBill>();

			AssertContains("CFR", bill.Lookups.IncotermList.CodesAsString);
			AssertContains("DAT", bill.Lookups.IncotermList.CodesAsString);
			AssertContains("EXW", bill.Lookups.IncotermList.CodesAsString);
			AssertContains("FOB", bill.Lookups.IncotermList.CodesAsString);
			AssertNotContains("XYZ", bill.Lookups.IncotermList.CodesAsString);
		}

		public void TestBuyerState_List()
		{
			var country = Factory.New<RefCountry>();
			country.RN_Code = "A1";
			var state1 = Factory.New<RefCountryStates>();
			var state2 = Factory.New<RefCountryStates>();
			state1.RW_RN_NKCountryCode = country.RN_Code;
			state1.RW_Code = "TP1";
			state1.RW_Description = "TP1 DESC";
			state2.RW_RN_NKCountryCode = country.RN_Code;
			state2.RW_Code = "ST2";
			state2.RW_Description = "ST2 DESC 2";

			var bill = Factory.New<AsycudaBill>();
			var buyerState_List = bill.Lookups.BuyerState_List;
			AssertEquals("BuyerState_List Count when country not selected", 0, buyerState_List.Count);

			bill.ABL_RN_NKBuyerCountry = "A1";
			buyerState_List = bill.Lookups.BuyerState_List;
			AssertEquals("ConsigneeState_List should have 2 elements.", 2, buyerState_List.Count);
			AssertEquals("ST2 DESC 2", buyerState_List.GetDescriptionFromCode("ST2"));
			AssertEquals("TP1 DESC", buyerState_List.GetDescriptionFromCode("TP1"));
		}

		public void TestBuyers()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertType<AsycudaBillBuyerCollection>("Type", bill.Lookups.Buyers);
		}

		public void TestShipmentTypes()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ShipmentType, "Shipment Type (Bill)", dataGrouping: Core.Constants.CountryCodes.SriLanka);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SriLanka, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ShipmentType, "EXP", "Export (22)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SriLanka, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ShipmentType, "IMP", "Import (23)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SriLanka, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ShipmentType, "TSS", "Transhipment (28)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SriLanka, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ShipmentType, "TRN", "Transit (24)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SriLanka, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ShipmentType, "TR1", "Transfer - Air to Sea or Sea to Air (31)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SriLanka, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ShipmentType, "TR2", "Transfer - Air to Air or Sea to Sea  (32)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var bill = Factory.New<AsycudaBill>();
			AssertEquals("EXP, IMP, TSS, TRN", bill.Lookups.ShipmentTypes.CodesAsString);
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.SriLanka;
			manifestHeader.Bills.Add(bill);
			AssertEquals("EXP, IMP, TR1, TR2, TRN, TSS", bill.Lookups.ShipmentTypes.CodesAsString);
		}
	}
}
