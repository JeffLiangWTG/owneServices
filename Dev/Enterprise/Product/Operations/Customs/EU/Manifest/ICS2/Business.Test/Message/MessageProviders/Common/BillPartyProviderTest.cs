using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class BillPartyProviderTest : DataProviderTestCase<BillPartyProvider>
	{
		public void TestNullWhenBillIsNull()
		{
			AssertNull(BillPartyProvider.NewOrNull(null, AsycudaBillAddress.AddressType.NotifyParty));
			AssertNull(BillPartyProvider.NewOrNull(null, AsycudaBillAddress.AddressType.Consignee));
			AssertNull(BillPartyProvider.NewOrNull(null, AsycudaBillAddress.AddressType.Shipper));
			AssertNull(BillPartyProvider.NewOrNull(null, AsycudaBillAddress.AddressType.Buyer));
			AssertNull(BillPartyProvider.NewOrNull(null, AsycudaBillAddress.AddressType.Seller));
		}

		public void TestNewOrNull_Seller()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var propertiesAffectingSeller = new[]
			{
				bill.ABL_SellerNameInfo,
				bill.ABL_SellerRegNoInfo,
				bill.SellerPersonTypeInfo,
				bill.ABL_SellerPhoneInfo,
				bill.ABL_SellerCityInfo,
				bill.ABL_RN_NKSellerCountryInfo,
				bill.ABL_SellerStreet1Info,
				bill.ABL_SellerPostcodeInfo,
				bill.ABL_SellerStreet2Info
			};
			AssertNull(BillPartyProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Seller));

			foreach (var propertyInfo in propertiesAffectingSeller)
			{
				AssertEquals("Relevant properties are empty, provider should return null", null, BillPartyProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Seller));

				propertyInfo.SetValueFromString("A");
				AssertNotEquals($"Should not be null, {propertyInfo.HumanReadableName} has a value", null, BillPartyProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Seller));

				propertyInfo.SetValueFromString(ZString.Empty);
			}

			bill.ABL_OA_Seller = OrgAddress.New(Factory).PK;
			bill.ABL_RN_NKSellerCountry = ZString.Empty;
			bill.SellerPersonType = ZString.Empty;
			AssertNotEquals("Should not be null, Seller has Value", null, BillPartyProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Seller));
		}

		public void TestNewOrNull_Buyer()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var propertiesAffectingBuyer = new[]
			{
				bill.ABL_BuyerNameInfo,
				bill.ABL_BuyerRegNoInfo,
				bill.BuyerPersonTypeInfo,
				bill.ABL_BuyerPhoneInfo,
				bill.ABL_BuyerCityInfo,
				bill.ABL_RN_NKBuyerCountryInfo,
				bill.ABL_BuyerStreet1Info,
				bill.ABL_BuyerPostcodeInfo,
				bill.ABL_BuyerStreet2Info
			};
			AssertNull(BillPartyProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Buyer));

			foreach (var propertyInfo in propertiesAffectingBuyer)
			{
				AssertEquals("Relevant properties are empty, provider should return null", null, BillPartyProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Buyer));

				propertyInfo.SetValueFromString("A");
				AssertNotEquals($"Should not be null, {propertyInfo.HumanReadableName} has a value", null, BillPartyProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Buyer));

				propertyInfo.SetValueFromString(ZString.Empty);
			}

			bill.ABL_OA_Buyer = OrgAddress.New(Factory).PK;
			bill.ABL_RN_NKBuyerCountry = ZString.Empty;
			bill.BuyerPersonType = ZString.Empty;
			AssertNotEquals("Should not be null, Buyer has Value", null, BillPartyProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Buyer));
		}

		public void TestNewOrNull_NotifyParty()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var propertiesAffectingNotifyParty = new[]
			{
				bill.ABL_NotifyPartyNameInfo,
				bill.ABL_NotifyPartyRegNoInfo,
				bill.NotifyPartyPersonTypeInfo,
				bill.ABL_NotifyPartyPhoneInfo,
				bill.ABL_NotifyPartyCityInfo,
				bill.ABL_RN_NKNotifyPartyCountryInfo,
				bill.ABL_NotifyPartyStreet1Info,
				bill.ABL_NotifyPartyPostcodeInfo,
				bill.ABL_NotifyPartyStreet2Info
			};
			AssertNull(BillPartyProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.NotifyParty));

			foreach (var propertyInfo in propertiesAffectingNotifyParty)
			{
				AssertEquals("Relevant properties are empty, provider should return null", null, BillPartyProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.NotifyParty));

				propertyInfo.SetValueFromString("A");
				AssertNotEquals($"Should not be null, {propertyInfo.HumanReadableName} has a value", null, BillPartyProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.NotifyParty));

				propertyInfo.SetValueFromString(ZString.Empty);
			}

			bill.ABL_OA_NotifyParty = OrgAddress.New(Factory).PK;
			bill.ABL_RN_NKNotifyPartyCountry = ZString.Empty;
			bill.NotifyPartyPersonType = ZString.Empty;
			AssertNotEquals("Should not be null, NotifyParty has Value", null, BillPartyProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.NotifyParty));
		}

		public void TestNewOrNull_Shipper()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var propertiesAffectingShipper = new[]
			{
				bill.ABL_ShipperNameInfo,
				bill.ABL_ShipperRegNoInfo,
				bill.ShipperPersonTypeInfo,
				bill.ABL_ShipperPhoneInfo,
				bill.ABL_ShipperCityInfo,
				bill.ABL_RN_NKShipperCountryInfo,
				bill.ABL_ShipperStreet1Info,
				bill.ABL_ShipperPostcodeInfo,
				bill.ABL_ShipperStreet2Info
			};
			AssertNull(BillPartyProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Shipper));

			foreach (var propertyInfo in propertiesAffectingShipper)
			{
				AssertEquals("Relevant properties are empty, provider should return null", null, BillPartyProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Shipper));

				propertyInfo.SetValueFromString("A");
				AssertNotEquals($"Should not be null, {propertyInfo.HumanReadableName} has a value", null, BillPartyProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Shipper));

				propertyInfo.SetValueFromString(ZString.Empty);
			}

			bill.ABL_OA_Shipper = OrgAddress.New(Factory).PK;
			bill.ABL_RN_NKShipperCountry = ZString.Empty;
			bill.ShipperPersonType = ZString.Empty;
			AssertNotEquals("Should not be null, Shipper has Value", null, BillPartyProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Shipper));
		}

		public void TestNewOrNull_Consignee()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var propertiesAffectingConsignee = new[]
			{
				bill.ABL_ConsigneeNameInfo,
				bill.ABL_ConsigneeRegNoInfo,
				bill.ConsigneePersonTypeInfo,
				bill.ABL_ConsigneePhoneInfo,
				bill.ABL_ConsigneeCityInfo,
				bill.ABL_RN_NKConsigneeCountryInfo,
				bill.ABL_ConsigneeStreet1Info,
				bill.ABL_ConsigneePostcodeInfo,
				bill.ABL_ConsigneeStreet2Info
			};
			AssertNull(BillPartyProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Consignee));

			foreach (var propertyInfo in propertiesAffectingConsignee)
			{
				AssertEquals("Relevant properties are empty, provider should return null", null, BillPartyProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Consignee));

				propertyInfo.SetValueFromString("A");
				AssertNotEquals($"Should not be null, {propertyInfo.HumanReadableName} has a value", null, BillPartyProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Consignee));

				propertyInfo.SetValueFromString(ZString.Empty);
			}

			bill.ABL_OA_Consignee = OrgAddress.New(Factory).PK;
			bill.ABL_RN_NKConsigneeCountry = ZString.Empty;
			bill.ConsigneePersonType = ZString.Empty;
			AssertNotEquals("Should not be null, Consignee has Value", null, BillPartyProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Consignee));
		}

		public void TestName()
		{
			AssertEquals("ConsigneeName", "ConsigneeName", consigneeProvider.Name);
			AssertEquals("ShipperName", "ShipperName", shipperProvider.Name);
			AssertEquals("NotifyPartyName", "NotifyPartyName", notifyPartyProvider.Name);
			AssertEquals("BuyerName", "BuyerName", buyerProvider.Name);
			AssertEquals("SellerName", "SellerName", sellerProvider.Name);

			bill.ABL_ConsigneeName = string.Empty;
			bill.ABL_ShipperName = string.Empty;
			bill.ABL_NotifyPartyName = string.Empty;
			bill.ABL_BuyerName = string.Empty;
			bill.ABL_SellerName = string.Empty;

			AssertNull("ConsigneeName", consigneeProvider.Name);
			AssertNull("ShipperName", shipperProvider.Name);
			AssertNull("NotifyPartyName", notifyPartyProvider.Name);
			AssertNull("BuyerName", buyerProvider.Name);
			AssertNull("SellerName", sellerProvider.Name);
		}

		public void TestIdentificationNumber()
		{
			AssertEquals("ConsigneeRegNo", "123456", consigneeProvider.IdentificationNumber);
			AssertEquals("ShipperRegNo", "234567", shipperProvider.IdentificationNumber);
			AssertEquals("NotifyPartyRegNo", "345678", notifyPartyProvider.IdentificationNumber);
			AssertEquals("BuyerRegNo", "718012", buyerProvider.IdentificationNumber);
			AssertEquals("SellerRegNo", "369850", sellerProvider.IdentificationNumber);

			bill.ABL_ConsigneeRegNo = string.Empty;
			bill.ABL_ShipperRegNo = string.Empty;
			bill.ABL_NotifyPartyRegNo = string.Empty;
			bill.ABL_BuyerRegNo = string.Empty;
			bill.ABL_SellerRegNo = string.Empty;

			AssertNull("ConsigneeRegNo", consigneeProvider.IdentificationNumber);
			AssertNull("ShipperRegNo", consigneeProvider.IdentificationNumber);
			AssertNull("NotifyPartyRegNo", notifyPartyProvider.IdentificationNumber);
			AssertNull("BuyerRegNo", buyerProvider.IdentificationNumber);
			AssertNull("SellerRegNo", sellerProvider.IdentificationNumber);
		}

		public void TestTypeOfPerson()
		{
			AssertEquals("ConsigneePersonType", "1", consigneeProvider.TypeOfPerson);
			AssertEquals("ShipperPersonType", "2", shipperProvider.TypeOfPerson);
			AssertEquals("NotifyPartyPersonType", "3", notifyPartyProvider.TypeOfPerson);
			AssertEquals("BuyerPersonType", "4", buyerProvider.TypeOfPerson);
			AssertEquals("SellerPersonType", "5", sellerProvider.TypeOfPerson);

			bill.ConsigneePersonType = string.Empty;
			bill.ShipperPersonType = string.Empty;
			bill.NotifyPartyPersonType = string.Empty;
			bill.BuyerPersonType = string.Empty;
			bill.SellerPersonType = string.Empty;

			AssertNull("ConsigneePersonType", consigneeProvider.TypeOfPerson);
			AssertNull("ShipperPersonType", shipperProvider.TypeOfPerson);
			AssertNull("NotifyPartyPersonType", notifyPartyProvider.TypeOfPerson);
			AssertNull("BuyerPersonType", buyerProvider.TypeOfPerson);
			AssertNull("SellerPersonType", sellerProvider.TypeOfPerson);
		}

		public void TestCommunications()
		{
			Assert("ConsigneePhone", consigneeProvider.Communications.Any(c => c.Type == CommunicationType.Codes.TE && c.Identifier == "1234"));
			Assert("ShipperPhone", shipperProvider.Communications.Any(c => c.Type == CommunicationType.Codes.TE && c.Identifier == "3456"));
			Assert("NotifyPartyPhone", notifyPartyProvider.Communications.Any(c => c.Type == CommunicationType.Codes.TE && c.Identifier == "5678"));
			Assert("BuyerPhone", buyerProvider.Communications.Any(c => c.Type == CommunicationType.Codes.TE && c.Identifier == "3245"));
			Assert("SellerPhone", sellerProvider.Communications.Any(c => c.Type == CommunicationType.Codes.TE && c.Identifier == "3269"));

			bill.ABL_ConsigneePhone = string.Empty;
			bill.ABL_ShipperPhone = string.Empty;
			bill.ABL_NotifyPartyPhone = string.Empty;
			bill.ABL_BuyerPhone = string.Empty;
			bill.ABL_SellerPhone = string.Empty;

			consigneeProvider = GenerateProvider(bill, AsycudaBillAddress.AddressType.Consignee);
			shipperProvider = GenerateProvider(bill, AsycudaBillAddress.AddressType.Shipper);
			notifyPartyProvider = GenerateProvider(bill, AsycudaBillAddress.AddressType.NotifyParty);
			buyerProvider = GenerateProvider(bill, AsycudaBillAddress.AddressType.Buyer);
			sellerProvider = GenerateProvider(bill, AsycudaBillAddress.AddressType.Seller);

			Assert("ConsigneePhone", consigneeProvider.Communications.Any(c => c.Type == CommunicationType.Codes.TE && c.Identifier == null));
			Assert("ShipperPhone", shipperProvider.Communications.Any(c => c.Type == CommunicationType.Codes.TE && c.Identifier == null));
			Assert("NotifyPartyPhone", notifyPartyProvider.Communications.Any(c => c.Type == CommunicationType.Codes.TE && c.Identifier == null));
			Assert("BuyerPhone", buyerProvider.Communications.Any(c => c.Type == CommunicationType.Codes.TE && c.Identifier == null));
			Assert("SellerPhone", sellerProvider.Communications.Any(c => c.Type == CommunicationType.Codes.TE && c.Identifier == null));
		}

		public void TestStatus()
		{
			AssertNull("ConsigneeStatus", consigneeProvider.Status);
			AssertNull("ShipperStatus", shipperProvider.Status);
			AssertNull("NotifyPartyStatus", notifyPartyProvider.Status);
			AssertNull("BuyerStatus", buyerProvider.Status);
			AssertNull("SellerStatus", sellerProvider.Status);
		}

		public void TestAddress()
		{
			AssertNull("ConsigneeAddress", consigneeProvider.Address);
			AssertNull("ShipperAddress", shipperProvider.Address);
			AssertNull("NotifyPartyAddress", notifyPartyProvider.Address);
			AssertNull("BuyerAddress", buyerProvider.Address);
			AssertNull("SellerAddress", sellerProvider.Address);

			bill.ABL_ConsigneeCity = "ConsigneeCity";
			bill.ABL_ShipperCity = "ShipperCity";
			bill.ABL_NotifyPartyCity = "NotifyPartyCity";
			bill.ABL_BuyerCity = "BuyerCity";
			bill.ABL_SellerCity = "SellerCity";

			consigneeProvider = GenerateProvider(bill, AsycudaBillAddress.AddressType.Consignee);
			shipperProvider = GenerateProvider(bill, AsycudaBillAddress.AddressType.Shipper);
			notifyPartyProvider = GenerateProvider(bill, AsycudaBillAddress.AddressType.NotifyParty);
			buyerProvider = GenerateProvider(bill, AsycudaBillAddress.AddressType.Buyer);
			sellerProvider = GenerateProvider(bill, AsycudaBillAddress.AddressType.Seller);

			AssertNotNull("ConsigneeAddress", consigneeProvider.Address);
			AssertNotNull("ShipperAddress", shipperProvider.Address);
			AssertNotNull("NotifyPartyAddress", notifyPartyProvider.Address);
			AssertNotNull("BuyerAddress", buyerProvider.Address);
			AssertNotNull("SellerAddress", sellerProvider.Address);
		}

		public void TestNullableWhenPartyTypeIsNotifyParty()
		{
			bill = Factory.New<AsycudaBill>();
			AssertNull(BillPartyProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.NotifyParty));

			bill.ABL_NotifyPartyCity = "City";
			AssertNotNull(BillPartyProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.NotifyParty));
		}

		protected override void SetUp()
		{
			base.SetUp();

			bill = Factory.New<AsycudaBill>();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = TransportTypeList.Codes.Road;
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
			bill.ABL_AMA = header.PK;
			bill.ABL_ConsigneeName = "ConsigneeName";
			bill.ABL_ShipperName = "ShipperName";
			bill.ABL_NotifyPartyName = "NotifyPartyName";
			bill.ABL_BuyerName = "BuyerName";
			bill.ABL_SellerName = "SellerName";
			bill.ABL_ConsigneeRegNo = "123456";
			bill.ABL_ShipperRegNo = "234567";
			bill.ABL_NotifyPartyRegNo = "345678";
			bill.ABL_BuyerRegNo = "718012";
			bill.ABL_SellerRegNo = "369850";
			bill.ConsigneePersonType = "1";
			bill.ShipperPersonType = "2";
			bill.NotifyPartyPersonType = "3";
			bill.BuyerPersonType = "4";
			bill.SellerPersonType = "5";
			bill.ABL_ConsigneePhone = "1234";
			bill.ABL_ShipperPhone = "3456";
			bill.ABL_NotifyPartyPhone = "5678";
			bill.ABL_BuyerPhone = "3245";
			bill.ABL_SellerPhone = "3269";

			consigneeProvider = GenerateProvider(bill, AsycudaBillAddress.AddressType.Consignee);
			shipperProvider = GenerateProvider(bill, AsycudaBillAddress.AddressType.Shipper);
			notifyPartyProvider = GenerateProvider(bill, AsycudaBillAddress.AddressType.NotifyParty);
			buyerProvider = GenerateProvider(bill, AsycudaBillAddress.AddressType.Buyer);
			sellerProvider = GenerateProvider(bill, AsycudaBillAddress.AddressType.Seller);
		}
		AsycudaBill bill;
		BillPartyProvider consigneeProvider;
		BillPartyProvider shipperProvider;
		BillPartyProvider notifyPartyProvider;
		BillPartyProvider buyerProvider;
		BillPartyProvider sellerProvider;

		BillPartyProvider GenerateProvider(AsycudaBill bill, AsycudaBillAddress.AddressType addressType) => BillPartyProvider.NewOrNull(bill, addressType);

		protected sealed override BillPartyProvider GetProvider()
		{
			return BillPartyProvider.NewOrNull(bill, AsycudaBillAddress.AddressType.Consignee);
		}
	}
}
