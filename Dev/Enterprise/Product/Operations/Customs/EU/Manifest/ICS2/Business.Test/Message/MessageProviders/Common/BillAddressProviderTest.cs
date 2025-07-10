using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class BillAddressProviderTest : DataProviderTestCase<BillAddressProvider>
	{
		public void TestNullWhenBillIsNull()
		{
			AssertNull(BillAddressProvider.NewOrNull(null, AsycudaBillAddress.AddressType.Seller));
			AssertNull(BillAddressProvider.NewOrNull(null, AsycudaBillAddress.AddressType.Buyer));
			AssertNull(BillAddressProvider.NewOrNull(null, AsycudaBillAddress.AddressType.NotifyParty));
			AssertNull(BillAddressProvider.NewOrNull(null, AsycudaBillAddress.AddressType.Shipper));
			AssertNull(BillAddressProvider.NewOrNull(null, AsycudaBillAddress.AddressType.Consignee));
		}

		public void TestNewOrNull_Seller()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var propertiesAffectingSeller = new[]
			{
				bill.ABL_SellerCityInfo,
				bill.ABL_RN_NKSellerCountryInfo,
				bill.ABL_SellerStreet1Info,
				bill.ABL_SellerStreet2Info,
				bill.ABL_SellerPostcodeInfo
			};
			AssertNull(BillAddressProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Seller));

			foreach (var propertyInfo in propertiesAffectingSeller)
			{
				AssertEquals("Relevant properties are empty, provider should return null", null, BillAddressProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Seller));

				propertyInfo.SetValueFromString("A");
				AssertNotNull(propertyInfo.HumanReadableName, BillAddressProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Seller));

				propertyInfo.SetValueFromString(ZString.Empty);
			}
		}

		public void TestNewOrNull_Buyer()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var propertiesAffectingBuyer = new[]
			{
				bill.ABL_BuyerCityInfo,
				bill.ABL_RN_NKBuyerCountryInfo,
				bill.ABL_BuyerStreet1Info,
				bill.ABL_BuyerStreet2Info,
				bill.ABL_BuyerPostcodeInfo
			};
			AssertNull(BillAddressProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Buyer));

			foreach (var propertyInfo in propertiesAffectingBuyer)
			{
				AssertEquals("Relevant properties are empty, provider should return null", null, BillAddressProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Buyer));

				propertyInfo.SetValueFromString("A");
				AssertNotNull(propertyInfo.HumanReadableName, BillAddressProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Buyer));

				propertyInfo.SetValueFromString(ZString.Empty);
			}
		}

		public void TestNewOrNull_NotifyParty()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var propertiesAffectingNotifyParty = new[]
			{
				bill.ABL_NotifyPartyCityInfo,
				bill.ABL_RN_NKNotifyPartyCountryInfo,
				bill.ABL_NotifyPartyStreet1Info,
				bill.ABL_NotifyPartyStreet2Info,
				bill.ABL_NotifyPartyPostcodeInfo
			};
			AssertNull(BillAddressProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.NotifyParty));

			foreach (var propertyInfo in propertiesAffectingNotifyParty)
			{
				AssertEquals("Relevant properties are empty, provider should return null", null, BillAddressProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.NotifyParty));

				propertyInfo.SetValueFromString("A");
				AssertNotNull(propertyInfo.HumanReadableName, BillAddressProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.NotifyParty));

				propertyInfo.SetValueFromString(ZString.Empty);
			}
		}

		public void TestNewOrNull_Shipper()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var propertiesAffectingShipper = new[]
			{
				bill.ABL_ShipperCityInfo,
				bill.ABL_RN_NKShipperCountryInfo,
				bill.ABL_ShipperStreet1Info,
				bill.ABL_ShipperStreet2Info,
				bill.ABL_ShipperPostcodeInfo
			};
			AssertNull(BillAddressProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Shipper));

			foreach (var propertyInfo in propertiesAffectingShipper)
			{
				AssertEquals("Relevant properties are empty, provider should return null", null, BillAddressProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Shipper));

				propertyInfo.SetValueFromString("A");
				AssertNotNull(propertyInfo.HumanReadableName, BillAddressProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Shipper));

				propertyInfo.SetValueFromString(ZString.Empty);
			}
		}

		public void TestNewOrNull_Consignee()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var propertiesAffectingConsignee = new[]
			{
				bill.ABL_ConsigneeCityInfo,
				bill.ABL_RN_NKConsigneeCountryInfo,
				bill.ABL_ConsigneeStreet1Info,
				bill.ABL_ConsigneeStreet2Info,
				bill.ABL_ConsigneePostcodeInfo
			};
			AssertNull(BillAddressProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Consignee));

			foreach (var propertyInfo in propertiesAffectingConsignee)
			{
				AssertEquals("Relevant properties are empty, provider should return null", null, BillAddressProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Consignee));

				propertyInfo.SetValueFromString("A");
				AssertNotNull(propertyInfo.HumanReadableName, BillAddressProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Consignee));

				propertyInfo.SetValueFromString(ZString.Empty);
			}
		}

		public void TestCity()
		{
			AssertEquals("Consignee City", "ConsigneeCity", consigneeProvider.City);
			AssertEquals("Shipper City", "ShipperCity", shipperProvider.City);
			AssertEquals("NotifyParty City", "NotifyPartyCity", notifyPartyProvider.City);
			AssertEquals("Buyer City", "BuyerCity", buyerProvider.City);
			AssertEquals("Seller City", "SellerCity", sellerProvider.City);

			bill.ABL_ConsigneeCity = string.Empty;
			bill.ABL_ShipperCity = string.Empty;
			bill.ABL_NotifyPartyCity = string.Empty;
			bill.ABL_BuyerCity = string.Empty;
			bill.ABL_SellerCity = string.Empty;

			AssertNull("Consignee City", consigneeProvider.City);
			AssertNull("Shipper City", shipperProvider.City);
			AssertNull("NotifyParty City", notifyPartyProvider.City);
			AssertNull("Buyer City", buyerProvider.City);
			AssertNull("Seller City", sellerProvider.City);
		}

		public void TestCountry()
		{
			AssertEquals("Consignee Country", "FR", consigneeProvider.Country);
			AssertEquals("Shipper Country", "IE", shipperProvider.Country);
			AssertEquals("NotifyParty Country", "IT", notifyPartyProvider.Country);
			AssertEquals("Buyer Country", "DE", buyerProvider.Country);
			AssertEquals("Seller Country", "BE", sellerProvider.Country);

			bill.ABL_RN_NKConsigneeCountry = string.Empty;
			bill.ABL_RN_NKShipperCountry = string.Empty;
			bill.ABL_RN_NKNotifyPartyCountry = string.Empty;
			bill.ABL_RN_NKBuyerCountry = string.Empty;
			bill.ABL_RN_NKSellerCountry = string.Empty;

			AssertNull("Consignee Country", consigneeProvider.Country);
			AssertNull("Shipper Country", shipperProvider.Country);
			AssertNull("NotifyParty Country", notifyPartyProvider.Country);
			AssertNull("Buyer Country", buyerProvider.Country);
			AssertNull("Seller Country", sellerProvider.Country);
		}

		public void TestStreet()
		{
			AssertEquals("Consignee Street1", "ConsigneeStreet1", consigneeProvider.Street);
			AssertEquals("Shipper Street1", "ShipperStreet1", shipperProvider.Street);
			AssertEquals("Notify Party Street1", "NotifyPartyStreet1", notifyPartyProvider.Street);
			AssertEquals("Buyer Street1", "BuyerStreet1", buyerProvider.Street);
			AssertEquals("Seller Street1", "SellerStreet1", sellerProvider.Street);

			bill.ABL_ConsigneeStreet1 = string.Empty;
			bill.ABL_ShipperStreet1 = string.Empty;
			bill.ABL_NotifyPartyStreet1 = string.Empty;
			bill.ABL_BuyerStreet1 = string.Empty;
			bill.ABL_SellerStreet1 = string.Empty;

			AssertNull("Consignee Street1", consigneeProvider.Street);
			AssertNull("Shipper Street1", shipperProvider.Street);
			AssertNull("Notify Party Street1", notifyPartyProvider.Street);
			AssertNull("Buyer Street1", buyerProvider.Street);
			AssertNull("Seller Street1", sellerProvider.Street);
		}

		public void TestStreetAdditionalLine()
		{
			AssertEquals("Consignee Street Additional Line", "ConsigneeStreet2", consigneeProvider.StreetAdditionalLine);
			AssertEquals("Shipper Street Additional Line", "ShipperStreet2", shipperProvider.StreetAdditionalLine);
			AssertEquals("Notify Party Street Additional Line", "NotifyPartyStreet2", notifyPartyProvider.StreetAdditionalLine);
			AssertEquals("Buyer Street Additional Line", "BuyerStreet2", buyerProvider.StreetAdditionalLine);
			AssertEquals("Seller Street Additional Line", "SellerStreet2", sellerProvider.StreetAdditionalLine);

			bill.ABL_ConsigneeStreet2 = string.Empty;
			bill.ABL_ShipperStreet2 = string.Empty;
			bill.ABL_NotifyPartyStreet2 = string.Empty;
			bill.ABL_BuyerStreet2 = string.Empty;
			bill.ABL_SellerStreet2 = string.Empty;

			AssertNull("Consignee Street Additional Line", consigneeProvider.StreetAdditionalLine);
			AssertNull("Shipper Street Additional Line", shipperProvider.StreetAdditionalLine);
			AssertNull("Notify Party Street Additional Line", notifyPartyProvider.StreetAdditionalLine);
			AssertNull("Buyer Street Additional Line", buyerProvider.StreetAdditionalLine);
			AssertNull("Seller Street Additional Line", sellerProvider.StreetAdditionalLine);
		}

		public void TestPostCode()
		{
			AssertEquals("Consignee Post Code", "1234", consigneeProvider.PostCode);
			AssertEquals("Shipper Post Code", "3456", shipperProvider.PostCode);
			AssertEquals("Notify Party Post Code", "5678", notifyPartyProvider.PostCode);
			AssertEquals("Buyer Post Code", "9876", buyerProvider.PostCode);
			AssertEquals("Seller Post Code", "7654", sellerProvider.PostCode);

			bill.ABL_ConsigneePostcode = string.Empty;
			bill.ABL_ShipperPostcode = string.Empty;
			bill.ABL_NotifyPartyPostcode = string.Empty;
			bill.ABL_BuyerPostcode = string.Empty;
			bill.ABL_SellerPostcode = string.Empty;

			AssertNull("Consignee Post Code", consigneeProvider.PostCode);
			AssertNull("Shipper Post Code", shipperProvider.PostCode);
			AssertNull("Notify Party Post Code", notifyPartyProvider.PostCode);
			AssertNull("Buyer Post Code", buyerProvider.PostCode);
			AssertNull("Seller Post Code", sellerProvider.PostCode);
		}

		public void TestSubDivision()
		{
			AssertNull("ConsigneeSubDivision", consigneeProvider.SubDivision);
			AssertNull("ShipperSubDivision", shipperProvider.SubDivision);
			AssertNull("NotifyPartySubDivision", notifyPartyProvider.SubDivision);
			AssertNull("BuyerSubDivision", buyerProvider.SubDivision);
			AssertNull("SellerSubDivision", sellerProvider.SubDivision);
		}

		public void TestNumber()
		{
			AssertEquals("ConsigneeNumber", "0", consigneeProvider.Number);
			AssertEquals("ShipperNumber", "0", shipperProvider.Number);
			AssertEquals("NotifyPartyNumber", "0", notifyPartyProvider.Number);
			AssertEquals("BuyerNumber", "0", buyerProvider.Number);
			AssertEquals("SellerNumber", "0", sellerProvider.Number);

			bill.ABL_ConsigneeCity = string.Empty;
			bill.ABL_ShipperCity = string.Empty;
			bill.ABL_NotifyPartyCity = string.Empty;
			bill.ABL_BuyerCity = string.Empty;
			bill.ABL_SellerCity = string.Empty;
			bill.ABL_RN_NKConsigneeCountry = string.Empty;
			bill.ABL_RN_NKShipperCountry = string.Empty;
			bill.ABL_RN_NKNotifyPartyCountry = string.Empty;
			bill.ABL_RN_NKBuyerCountry = string.Empty;
			bill.ABL_RN_NKSellerCountry = string.Empty;
			bill.ABL_ConsigneeStreet1 = string.Empty;
			bill.ABL_ShipperStreet1 = string.Empty;
			bill.ABL_NotifyPartyStreet1 = string.Empty;
			bill.ABL_BuyerStreet1 = string.Empty;
			bill.ABL_SellerStreet1 = string.Empty;
			bill.ABL_ConsigneeStreet2 = string.Empty;
			bill.ABL_ShipperStreet2 = string.Empty;
			bill.ABL_NotifyPartyStreet2 = string.Empty;
			bill.ABL_BuyerStreet2 = string.Empty;
			bill.ABL_SellerStreet2 = string.Empty;
			bill.ABL_ConsigneePostcode = string.Empty;
			bill.ABL_ShipperPostcode = string.Empty;
			bill.ABL_NotifyPartyPostcode = string.Empty;
			bill.ABL_BuyerPostcode = string.Empty;
			bill.ABL_SellerPostcode = string.Empty;

			AssertNull("ConsigneeNumber", consigneeProvider.Number);
			AssertNull("ShipperNumber", shipperProvider.Number);
			AssertNull("NotifyPartyNumber", notifyPartyProvider.Number);
			AssertNull("BuyerNumber", buyerProvider.Number);
			AssertNull("SellerNumber", sellerProvider.Number);
		}

		public void TestPoBox()
		{
			AssertNull("ConsigneePoBox", consigneeProvider.PoBox);
			AssertNull("ShipperPoBox", shipperProvider.PoBox);
			AssertNull("NotifyPartyPoBox", notifyPartyProvider.PoBox);
			AssertNull("BuyerPoBox", buyerProvider.PoBox);
			AssertNull("SellerPoBox", sellerProvider.PoBox);
		}

		protected override void SetUp()
		{
			base.SetUp();

			bill = Factory.New<AsycudaBill>();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = TransportTypeList.Codes.Road;
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
			bill.ABL_AMA = header.PK;
			bill.ABL_ConsigneeCity = "ConsigneeCity";
			bill.ABL_ShipperCity = "ShipperCity";
			bill.ABL_NotifyPartyCity = "NotifyPartyCity";
			bill.ABL_BuyerCity = "BuyerCity";
			bill.ABL_SellerCity = "SellerCity";
			bill.ABL_RN_NKConsigneeCountry = "FR";
			bill.ABL_RN_NKShipperCountry = "IE";
			bill.ABL_RN_NKNotifyPartyCountry = "IT";
			bill.ABL_RN_NKBuyerCountry = "DE";
			bill.ABL_RN_NKSellerCountry = "BE";
			bill.ABL_ConsigneeStreet1 = "ConsigneeStreet1";
			bill.ABL_ShipperStreet1 = "ShipperStreet1";
			bill.ABL_NotifyPartyStreet1 = "NotifyPartyStreet1";
			bill.ABL_BuyerStreet1 = "BuyerStreet1";
			bill.ABL_SellerStreet1 = "SellerStreet1";
			bill.ABL_ConsigneeStreet2 = "ConsigneeStreet2";
			bill.ABL_ShipperStreet2 = "ShipperStreet2";
			bill.ABL_NotifyPartyStreet2 = "NotifyPartyStreet2";
			bill.ABL_BuyerStreet2 = "BuyerStreet2";
			bill.ABL_SellerStreet2 = "SellerStreet2";
			bill.ABL_ConsigneePostcode = "1234";
			bill.ABL_ShipperPostcode = "3456";
			bill.ABL_NotifyPartyPostcode = "5678";
			bill.ABL_BuyerPostcode = "9876";
			bill.ABL_SellerPostcode = "7654";

			consigneeProvider = GenerateProvider(bill, AsycudaBillAddress.AddressType.Consignee);
			shipperProvider = GenerateProvider(bill, AsycudaBillAddress.AddressType.Shipper);
			notifyPartyProvider = GenerateProvider(bill, AsycudaBillAddress.AddressType.NotifyParty);
			buyerProvider = GenerateProvider(bill, AsycudaBillAddress.AddressType.Buyer);
			sellerProvider = GenerateProvider(bill, AsycudaBillAddress.AddressType.Seller);
		}
		AsycudaBill bill;
		BillAddressProvider consigneeProvider;
		BillAddressProvider shipperProvider;
		BillAddressProvider notifyPartyProvider;
		BillAddressProvider buyerProvider;
		BillAddressProvider sellerProvider;

		BillAddressProvider GenerateProvider(AsycudaBill bill, AsycudaBillAddress.AddressType addressType) => new BillAddressProvider(bill, addressType);

		protected sealed override BillAddressProvider GetProvider()
		{
			return new BillAddressProvider(bill, AsycudaBillAddress.AddressType.Consignee);
		}
	}
}
