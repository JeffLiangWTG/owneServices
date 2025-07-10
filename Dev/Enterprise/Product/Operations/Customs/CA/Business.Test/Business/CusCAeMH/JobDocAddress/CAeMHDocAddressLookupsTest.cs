using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CAeMHDocAddressLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookupLists()
		{
			var docAddress = Factory.New<CAeMHDocAddress>();
			var lookups = docAddress.Lookups;

			docAddress.E2_AddressType = DocAddressTypes.Codes.ConsigneeDocumentaryAddress;
			AssertType<ConsigneeCollection>(lookups.ThirdParties);
			docAddress.E2_AddressType = DocAddressTypes.Codes.ConsigneePickupDeliveryAddress;
			AssertType<ConsigneeCollection>(lookups.ThirdParties);
			docAddress.E2_AddressType = DocAddressTypes.Codes.ConsignorDocumentaryAddress;
			AssertType<ConsignorCollection>(lookups.ThirdParties);
			docAddress.E2_AddressType = DocAddressTypes.Codes.ImportBroker;
			AssertType<BrokerCollection>(lookups.ThirdParties);
			docAddress.E2_AddressType = DocAddressTypes.Codes.ReceivingForwarderAddress;
			AssertType<ForwarderCollection>(lookups.ThirdParties);
			docAddress.E2_AddressType = DocAddressTypes.Codes.Carrier;
			AssertType<ShippingProviderCollection>(lookups.ThirdParties);
			docAddress.E2_AddressType = DocAddressTypes.Codes.Warehouse;
			AssertType<WarehouseClientCollection>(lookups.ThirdParties);
			docAddress.E2_AddressType = DocAddressTypes.Codes.Consolidator;
			AssertNotNull(lookups.ThirdParties.FilterBusinessObjectDefaults.OfType<FilterBusinessObjectDefault>().FirstOrDefault(x => IsMatch(x, "Organisation Types", "Property5", ZBool.True)));
			AssertNotNull(lookups.ThirdParties.FilterBusinessObjectDefaults.OfType<FilterBusinessObjectDefault>().FirstOrDefault(x => IsMatch(x, "Organisation Types", "Property9", ZBool.True)));
			docAddress.E2_AddressType = DocAddressTypes.Codes.PlaceOfConsolidation;
			AssertNotNull(lookups.ThirdParties.FilterBusinessObjectDefaults.OfType<FilterBusinessObjectDefault>().FirstOrDefault(x => IsMatch(x, "Organisation Types", "Property5", ZBool.True)));
			AssertNotNull(lookups.ThirdParties.FilterBusinessObjectDefaults.OfType<FilterBusinessObjectDefault>().FirstOrDefault(x => IsMatch(x, "Organisation Types", "Property9", ZBool.True)));
			docAddress.E2_AddressType = "XXX";
			AssertType<OrganisationsFindBoxCollection>(lookups.ThirdParties);
		}

		bool IsMatch(FilterBusinessObjectDefault bizObjDefault, ZString filterName, ZString propertyName, ZBool value)
		{
			return bizObjDefault.FilterName == filterName && bizObjDefault.PropertyName == propertyName && (ZBool)bizObjDefault.Value == value;
		}
	}
}
