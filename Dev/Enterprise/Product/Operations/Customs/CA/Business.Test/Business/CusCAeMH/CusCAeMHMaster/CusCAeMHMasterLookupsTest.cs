using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHMasterLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookupLists()
		{
			var lookups = Factory.New<CusCAeMHMaster>().Lookups;
			AssertEquals(4, lookups.TransportModeList.Count);
			AssertType(typeof(ZZRefCarrierCombinedCollection), lookups.Carriers);
			AssertType(typeof(ZZRefCusCodeListCombinedCollection), lookups.DischargeOffices);
			AssertType(typeof(CACSubLocationCollection), lookups.DischargeSubLocations);
			AssertType<EManifestForwarderJobStatusList>(lookups.CustomsStatuses);
			AssertType<MessageStatusList>(lookups.MessageStatuses);
			Assert(lookups.ForwardersAndServices.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types:Property5"));
			Assert(lookups.ForwardersAndServices.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types:Property9"));

			lookups.CurrentAddressType = DocAddressTypes.Codes.ConsigneeDocumentaryAddress;
			AssertType<ConsigneeCollection>(lookups.ThirdParties);
			lookups.CurrentAddressType = DocAddressTypes.Codes.ConsigneePickupDeliveryAddress;
			AssertType<ConsigneeCollection>(lookups.ThirdParties);
			lookups.CurrentAddressType = DocAddressTypes.Codes.ConsignorDocumentaryAddress;
			AssertType<ConsignorCollection>(lookups.ThirdParties);
			lookups.CurrentAddressType = DocAddressTypes.Codes.ImportBroker;
			AssertType<BrokerCollection>(lookups.ThirdParties);
			lookups.CurrentAddressType = DocAddressTypes.Codes.ReceivingForwarderAddress;
			AssertType<ForwarderCollection>(lookups.ThirdParties);
			lookups.CurrentAddressType = DocAddressTypes.Codes.Carrier;
			AssertType<ShippingProviderCollection>(lookups.ThirdParties);
			lookups.CurrentAddressType = DocAddressTypes.Codes.Warehouse;
			AssertType<WarehouseClientCollection>(lookups.ThirdParties);
			lookups.CurrentAddressType = DocAddressTypes.Codes.Consolidator;
			Assert(lookups.ThirdParties.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types:Property5"));
			Assert(lookups.ThirdParties.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types:Property9"));
			lookups.CurrentAddressType = DocAddressTypes.Codes.PlaceOfConsolidation;
			Assert(lookups.ThirdParties.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types:Property5"));
			Assert(lookups.ThirdParties.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types:Property9"));
			lookups.CurrentAddressType = "XXX";
			AssertType<OrganisationsFindBoxCollection>(lookups.ThirdParties);
		}

		public void TestDischargeOffices()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1111", "CA Customs Office Code", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1111", "US Customs Office Code", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "1111", "CA Customs Port Code", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1234", "CA Customs Office Code (Expired)", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(-1));
			Factory.Save();

			var dischargeOffices = Factory.New<CusCAeMHMaster>().Lookups.DischargeOffices;
			dischargeOffices.Load();

			AssertEquals(1, dischargeOffices.Count);
			AssertType<ZZRefCusCodeListCombinedCollection>(dischargeOffices);
			Assert(dischargeOffices.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "1111"));
			AssertEquals("CA Customs Office Code", dischargeOffices.Cast<ZZRefCusCodeListCombined>().FirstOrDefault(x => x.ZZD_Code == "1111").ZZD_Description);
			Assert(!dischargeOffices.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "1234"));
		}
	}
}
