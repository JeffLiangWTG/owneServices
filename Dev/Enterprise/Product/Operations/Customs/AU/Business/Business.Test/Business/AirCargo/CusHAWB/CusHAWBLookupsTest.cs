using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusHAWBLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPrepaidCollectListIsCached()
		{
			var hawb1 = Factory.New<CusHAWB>();
			Assert("PrepaidCollectList should be cached in same factory", ReferenceEquals(Factory.GetCachedValue<CMRMethodsOfPayment>(), hawb1.Lookups.PrepaidCollectList));
			var hawb2 = Factory.New<CusHAWB>();
			Assert("PrepaidCollectList should be cached in same factory", ReferenceEquals(hawb1.Lookups.PrepaidCollectList, hawb2.Lookups.PrepaidCollectList));
		}

		public void TestUnitOfWeightListIsCached()
		{
			var hawb1 = Factory.New<CusHAWB>();
			var hawb2 = Factory.New<CusHAWB>();
			Assert("UnitOfWeightList should be cached in same factory", ReferenceEquals(hawb1.Lookups.UnitOfWeightList, hawb2.Lookups.UnitOfWeightList));
		}

		public void TestBaseStatusListIsCached()
		{
			var hawb1 = Factory.New<CusHAWB>();
			Assert("BaseStatusList should be cached in same factory", ReferenceEquals(Factory.GetCachedValue<CMRStatuses>(), hawb1.Lookups.BaseStatusList));
			var hawb2 = Factory.New<CusHAWB>();
			Assert("BaseStatusList should be cached in same factory", ReferenceEquals(hawb1.Lookups.BaseStatusList, hawb2.Lookups.BaseStatusList));
		}

		public void TestContactListsLoaded()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			HAWB.CS_OA_ConsigneeAddress = org.MainAddress.PK;
			HAWB.CS_OA_ConsignorAddress = org.MainAddress.PK;

			AssertEquals(true, HAWB.Lookups.ConsigneeContactList.IsLoaded);
			AssertEquals(true, HAWB.Lookups.ConsignorContactList.IsLoaded);
		}

		public void TestShipmentTypeList()
		{
			AssertEquals("Three items by default", 3, HAWB.Lookups.ShipmentTypeList.Count);
		}

		public void TestCollection()
		{
			AssertNotNull("Master list", HAWB.Lookups.MasterList);
			AssertNotNull("ConsigneeCountryList", HAWB.Lookups.ConsigneeCountryList);
			AssertNotNull("ConsignorCountryList", HAWB.Lookups.ConsignorCountryList);
			AssertNotNull("ConsigneeList", HAWB.Lookups.ConsigneeList);
			AssertNotNull("ConsignorList", HAWB.Lookups.ConsignorList);
			AssertNotNull("ConsignorStateList", HAWB.Lookups.ConsignorStateList);
			AssertNotNull("ConsigneeStateList", HAWB.Lookups.ConsigneeStateList);
			AssertNotNull("DestinationList", HAWB.Lookups.DestinationList);
			AssertNotNull("OriginList", HAWB.Lookups.OriginList);
			AssertNotNull("DestinationList", HAWB.Lookups.DestinationList);
			AssertNotNull("CurrencyList", HAWB.Lookups.CurrencyList);
			AssertNotNull("LoadPorts", HAWB.Lookups.LoadPorts);
		}

		public void TestPrepaidCollect()
		{
			AssertEquals("PrepaidCollectionList CMR", 27, HAWB.Lookups.PrepaidCollectList.Count);
			AssertEquals(Factory.GetCachedValue<CMRMethodsOfPayment>(), HAWB.Lookups.PrepaidCollectList);
		}

		public void TestPrepaidCollectListForValidation()
		{
			AssertEquals("Default should be PrepaidCollectList", HAWB.Lookups.PrepaidCollectList.CodesAsString, HAWB.Lookups.PrepaidCollectListForValidation.CodesAsString);
		}

		public void TestPrepaidCollectListWithNullMAWB()
		{
			var hAWB1 = Factory.New<CTOCusHAWB>();
			AssertEquals((new CMRMethodsOfPayment()).GetType().ToString(), hAWB1.Lookups.PrepaidCollectList.ToString());
		}

		public void TestConsignorContactList()
		{
			var org1 = Factory.New<OrgHeader>();
			OrgContact contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "The Thing";
			OrgContact contact2 = org1.Contacts.AddNew();
			contact2.OC_ContactName = "Mark Twain";
			HAWB.CS_OA_ConsignorAddress = org1.MainAddress.PK;

			OrgContactDependentCollection collection = HAWB.Lookups.ConsignorContactList;
			collection.Load();
			AssertEquals(2, collection.Count);
			collection.Sort(OrgContactSchema.Constants.OC_ContactName, ListSortDirection.Ascending);
			AssertEquals("Mark Twain", collection[0].OC_ContactName);
			AssertEquals("The Thing", collection[1].OC_ContactName);

			var org2 = Factory.New<OrgHeader>();
			HAWB.CS_OA_ConsignorAddressInfo.Value = org2.MainAddress.PK;
			collection = HAWB.Lookups.ConsignorContactList;
			collection.Load();
			AssertEquals("Should refer to Org2 ContactList now", 0, collection.Count);
		}

		public void TestConsigneeContactList()
		{
			var org1 = Factory.New<OrgHeader>();
			OrgContact contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "Copernicus";
			OrgContact contact2 = org1.Contacts.AddNew();
			contact2.OC_ContactName = "Da Vinci";
			HAWB.CS_OA_ConsigneeAddress = org1.MainAddress.PK;

			OrgContactDependentCollection collection = HAWB.Lookups.ConsigneeContactList;
			collection.Load();
			AssertEquals(2, collection.Count);
			collection.Sort(OrgContactSchema.Constants.OC_ContactName, ListSortDirection.Ascending);
			AssertEquals("Copernicus", collection[0].OC_ContactName);
			AssertEquals("Da Vinci", collection[1].OC_ContactName);

			var org2 = Factory.New<OrgHeader>();
			HAWB.CS_OA_ConsigneeAddressInfo.Value = org2.MainAddress.PK;
			collection = HAWB.Lookups.ConsigneeContactList;
			collection.Load();
			AssertEquals("Should refer to Org2 ContactList now", 0, collection.Count);
		}

		public void TestRefServiceLevelList()
		{
			AssertNotNull(HAWB.Lookups.RefServiceLevelList);
			AssertEquals(Factory, HAWB.Lookups.RefServiceLevelList.Factory);
		}

		public void TestCommercialStatusList()
		{
			ReadOnlyCodeDescriptionPairList expectedList = Env.Registry.AUCustoms.AirCargoCommercialStatus;
			ReadOnlyCodeDescriptionPairList actualList = (ReadOnlyCodeDescriptionPairList)HAWB.Lookups.CommercialStatusList;

			AssertEquals(expectedList.Count, actualList.Count);
			for (int i = 0; i < expectedList.Count; i++)
			{
				AssertEquals(expectedList[i].Code, actualList[i].Code);
				AssertEquals(expectedList[i].Description, actualList[i].Description);
			}
		}

		public void TestCMRCurrencyList()
		{
			AssertNotNull(HAWB.Lookups.CMRCurrencyList);
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("AUD"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("BRL"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("CAD"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("CHF"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("CNY"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("DKK"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("EUR"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("FJD"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("GBP"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("HKD"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("IDR"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("EUR"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("ILS"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("INR"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("JPY"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("KRW"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("LKR"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("MYR"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("NOK"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("NZD"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("PGK"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("PHP"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("PKR"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("SBD"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("SEK"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("SGD"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("THB"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("TWD"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("USD"));
			AssertEquals(true, HAWB.Lookups.CMRCurrencyList.ContainsCode("ZAR"));
		}

		public void TestConsigneeList()
		{
			HAWB.CS_IsMasterHouse = false;
			AssertEquals("Consignee List should be of type ConsigneeCollection", typeof(ConsigneeCollection), HAWB.Lookups.ConsigneeList.GetType());
			AssertEquals("Filter Default not set", true, HAWB.Lookups.ConsigneeList.FilterBusinessObjectDefaults.ContainsDefaultFor("Main UNLOCO" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			HAWB.CS_IsMasterHouse = true;
			AssertEquals("Consignee List should be of type ForwarderCollection", typeof(ForwarderCollection), HAWB.Lookups.ConsigneeList.GetType());
		}

		public void TestConsigneeListUsesCountryFilter()
		{
			HAWB.CS_IsMasterHouse = false;
			HAWB.CS_RL_NKDestination = "AUSYD";
			OrgHeaderCollection result = HAWB.Lookups.Consignees;
			AssertEquals(result.Count, HAWB.Lookups.ConsigneeList.Count);
		}

		public void TestConsignorList()
		{
			HAWB.CS_IsMasterHouse = false;
			AssertEquals("Consignor List should be of type ConsignorCollection", typeof(ConsignorCollection), HAWB.Lookups.ConsignorList.GetType());
			AssertEquals("Filter Default not set", true, HAWB.Lookups.ConsignorList.FilterBusinessObjectDefaults.ContainsDefaultFor("Main UNLOCO" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			HAWB.CS_IsMasterHouse = true;
			AssertEquals("Consignor List should be of type ForwarderCollection", typeof(ForwarderCollection), HAWB.Lookups.ConsignorList.GetType());
		}

		CusHAWB hawb;
		CusHAWB HAWB => hawb ?? (hawb = Factory.New<CusMAWB>().ChildBills.AddNew());
	}
}
