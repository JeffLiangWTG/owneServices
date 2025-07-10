using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	sealed class DocDeliveryContactAddressFormatterTest : TestCaseWithFactory
	{
		DocDeliveryContact SetupTestAddress(string name, string code, string additionalAddress, string address1, string address2, string city, string state, string postCode, RefUNLOCO uNLOCO, string language)
		{
			DocDeliveryContact testContact = new DocDeliveryContact(Factory);
			testContact.CompanyName = name;
			testContact.Address1 = address1;
			testContact.Address2 = address2;
			testContact.City = city;
			testContact.State = state;
			testContact.PostCode = postCode;
			testContact.UNLOCO = uNLOCO;
			testContact.Language = language;
			testContact.AdditionalAddress = additionalAddress;

			return testContact;
		}

		DocDeliveryContact SetupTestAddressWithTranslatedAddresses(string name, string code, string additionalAddress, string address1, string address2, string city, string state, string postCode, RefUNLOCO uNLOCO, string language)
		{
			var testContact = SetupTestAddress(name, code, additionalAddress, address1, address2, city, state, postCode, uNLOCO, language);

			var testOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			testOrgHeader.MainAddress.CompanyName = name;
			testOrgHeader.MainAddress.Address1 = address1;
			testOrgHeader.MainAddress.Address2 = address2;
			testOrgHeader.MainAddress.City = city;
			testOrgHeader.MainAddress.Postcode = postCode;
			testOrgHeader.MainAddress.State = state;

			testContact.OrgHeaderPK = testOrgHeader.PK;

			var translatedAddress1 = testContact.OrgAddress.TranslatedAddresses.AddNew();
			translatedAddress1.OTA_Language = Core.Constants.Languages.PortugueseBrazil;
			translatedAddress1.OTA_CompanyName = "Name pbr";
			translatedAddress1.OTA_Address1 = "Addr1 pbr";
			translatedAddress1.OTA_Address2 = "Addr2 pbr";
			translatedAddress1.OTA_City = "PSydney";
			translatedAddress1.OTA_State = "NSW";
			translatedAddress1.OTA_PostCode = "2100";

			return testContact;
		}

		protected override void SetUp()
		{
			base.SetUp();
			AustralianUNLOCO = Top1UNLOCOForCountry("AUSTRALIA");
			CanadianUNLOCO = Top1UNLOCOForCountry("CANADA");
			FrenchUNLOCO = Top1UNLOCOForCountry("FRANCE");
			SpanishUNLOCO = Top1UNLOCOForCountry("SPAIN");
			IcelandUNLOCO = Top1UNLOCOForCountry("ICELAND");
		}

		RefUNLOCO Top1UNLOCOForCountry(string countryName)
		{
			ZQuery countryFilter = new ZQuery(RefCountrySchema.RN_Desc, countryName);
			RefCountry country = Factory.LoadTop1<RefCountry>(countryFilter);

			ZQuery uNLOCOFilter = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, country.RN_Code);
			return Factory.LoadTop1<RefUNLOCO>(uNLOCOFilter);
		}

		RefUNLOCO AustralianUNLOCO, CanadianUNLOCO, FrenchUNLOCO, SpanishUNLOCO, IcelandUNLOCO;

		DocDeliveryContactAddressFormatter NewPostalAddressFormatter(DocDeliveryContact recipient)
		{
			return new DocDeliveryContactAddressFormatter(recipient.Factory, recipient, GlbCompany.CurrentCompany);
		}

		public void TestLanguage()
		{
			DocDeliveryContact internationalRecipient = SetupTestAddress("XYZ Forwarders", "XYZFWLLC", string.Empty, "1234 Smith Street", "", "New Foo City", "ABC", "123 XG 27", CanadianUNLOCO, Core.Constants.Languages.English);
			AssertEquals("English language", Core.Constants.Languages.English, internationalRecipient.Language);
		}

		public void TestInternationalOneLineAddress()
		{
			DocDeliveryContact internationalOneLineRecipient = SetupTestAddress("XYZ Forwarders", "XYZFWLLC", string.Empty, "1234 Smith Street", "", "New Foo City", "ABC", "123 XG 27", CanadianUNLOCO, Core.Constants.Languages.English);
			string expectedAddress = "XYZ FORWARDERS\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nCANADA";
			string actualAddress = NewPostalAddressFormatter(internationalOneLineRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestInternationalTwoLineAddress()
		{
			DocDeliveryContact internationalTwoLineRecipient = SetupTestAddress("XYZ Forwarders", "XYZFWLLC", string.Empty, "Level 69", "1234 Smith Street", "New Foo City", "ABC", "123 XG 27", CanadianUNLOCO, Core.Constants.Languages.English);
			string expectedAddress = "XYZ FORWARDERS\nLEVEL 69\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nCANADA";
			string actualAddress = NewPostalAddressFormatter(internationalTwoLineRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestDomesticOneLineAddress()
		{
			DocDeliveryContact domesticOneLineRecipient = SetupTestAddress("XYZ Forwarders", "XYZFWLLC", string.Empty, "1234 Smith Street", "", "New Foo City", "ABC", "123 XG 27", AustralianUNLOCO, Core.Constants.Languages.English);
			string expectedAddress = "XYZ FORWARDERS\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27";
			string actualAddress = NewPostalAddressFormatter(domesticOneLineRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestDomesticTwoLineAddress()
		{
			DocDeliveryContact domesticTwoLineRecipient = SetupTestAddress("XYZ Forwarders", "XYZFWLLC", string.Empty, "Level 69", "1234 Smith Street", "New Foo City", "ABC", "123 XG 27", AustralianUNLOCO, Core.Constants.Languages.English);
			string expectedAddress = "XYZ FORWARDERS\nLEVEL 69\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27";
			string actualAddress = NewPostalAddressFormatter(domesticTwoLineRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestDomesticAdditionalAddress()
		{
			DocDeliveryContact domesticAdditionalAddress = SetupTestAddress("XYZ Forwarders", "XYZFWLLC", "Additional Info", "Level 69", "1234 Smith Street", "New Foo City", "ABC", "123 XG 27", AustralianUNLOCO,
				Core.Constants.Languages.English);
			string expectedAddress = "XYZ FORWARDERS\nADDITIONAL INFO\nLEVEL 69\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27";
			string actualAddress = NewPostalAddressFormatter(domesticAdditionalAddress).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestFrenchAddress()
		{
			DocDeliveryContact frenchRecipient = SetupTestAddress("A", "A", string.Empty, "1", "2", "PARIS", "", "12345", FrenchUNLOCO, Core.Constants.Languages.French);
			string expectedAddress = "A\n1\n2\n12345 PARIS\nFRANCE";
			string actualAddress = NewPostalAddressFormatter(frenchRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestFrenchAddressWithoutPostCode()
		{
			DocDeliveryContact frenchRecipient = SetupTestAddress("A", "A", string.Empty, "1", "2", "PARIS", "", "", FrenchUNLOCO, Core.Constants.Languages.French);
			string expectedAddress = "A\n1\n2\nPARIS\nFRANCE";
			string actualAddress = NewPostalAddressFormatter(frenchRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestSpanishAddress()
		{
			DocDeliveryContact spanishRecipient = SetupTestAddress("A", "A", string.Empty, "1", "2", "ARANJUEZ", "MADRID", "1234", SpanishUNLOCO, Core.Constants.Languages.Spanish);
			string expectedAddress = "A\n1\n2\n1234 ARANJUEZ (MADRID)\nESPAÑA";
			string actualAddress = NewPostalAddressFormatter(spanishRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestSpanishAddressWithoutProvince()
		{
			DocDeliveryContact spanishRecipient = SetupTestAddress("A", "A", string.Empty, "1", "2", "ARANJUEZ", "", "1234", SpanishUNLOCO, Core.Constants.Languages.Spanish);
			string expectedAddress = "A\n1\n2\n1234 ARANJUEZ\nESPAÑA";
			string actualAddress = NewPostalAddressFormatter(spanishRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestTakePADPostalAddress()
		{
			using (new CountrySwitcher(Core.Constants.CountryCodes.Iceland))
			{
				DocDeliveryContact icelandRecipient = SetupTestAddress(
					"A", "A", string.Empty, "1", "2", "ARANJUEZ", "", "1234", IcelandUNLOCO, Core.Constants.Languages.Icelandic);
				icelandRecipient.OrgHeaderPK = Factory.NewWithValidTestData<OrgHeader>().PK;

				icelandRecipient.OrgHeader.OH_RL_NKClosestPort = IcelandUNLOCO.Code;
				icelandRecipient.OrgHeader.Addresses.AddNew(OrgAddressType.PickupAndDelivery, false).OA_Address1 =
					"Delivery Address";

				DocDeliveryContactAddressFormatter formatter = NewPostalAddressFormatter(icelandRecipient);
				formatter.TakePADPostalAddress = true;

				string address = formatter.PostalAddress();

				Assert("Wrong address", address.Contains("Delivery Address".ToUpper()));
			}
		}

		public void TestTakeMainAddressWhenNoPADAddress()
		{
			using (new CountrySwitcher(Core.Constants.CountryCodes.Iceland))
			{
				DocDeliveryContact icelandRecipient = SetupTestAddress(
					"A", "A", string.Empty, "1", "2", "ARANJUEZ", "", "1234", IcelandUNLOCO, Core.Constants.Languages.Icelandic);
				icelandRecipient.OrgHeaderPK = Factory.NewWithValidTestData<OrgHeader>().PK;

				icelandRecipient.OrgHeader.OH_RL_NKClosestPort = IcelandUNLOCO.Code;
				icelandRecipient.OrgHeader.Addresses.MainAddress.OA_Address1 = "Office Address";

				DocDeliveryContactAddressFormatter formatter = NewPostalAddressFormatter(icelandRecipient);
				formatter.TakePADPostalAddress = true;

				string address = formatter.PostalAddress();

				Assert("Wrong address", address.Contains("Office Address".ToUpper()));
			}
		}

		public void TestTranslatedAddress()
		{
			var recipient = SetupTestAddressWithTranslatedAddresses("XYZ Forwarders", "XYZFWLLC", "", "Level 69", "1234 Smith Street", "New Foo City", "ABC", "123 XG 27", AustralianUNLOCO, Core.Constants.Languages.English);

			var expectedAddress = "XYZ FORWARDERS\nLEVEL 69\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27";
			var actualAddress = new DocDeliveryContactAddressFormatter(recipient.Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);

			var expectedAddressTranslated = "NAME PBR\nADDR1 PBR\nADDR2 PBR\nPSYDNEY NSW 2100";
			var actualAddressTranslated = new DocDeliveryContactAddressFormatter(recipient.Factory, recipient, GlbCompany.CurrentCompany, false, Core.Constants.Languages.PortugueseBrazil).PostalAddress();
			AssertEquals("Wrong translated address", expectedAddressTranslated, actualAddressTranslated);
		}

		public void TestPostalAddressWithoutCompanyName()
		{
			var recipient = SetupTestAddressWithTranslatedAddresses("XYZ Forwarders", "XYZFWLLC", "", "Level 69", "1234 Smith Street", "New Foo City", "ABC", "123 XG 27", AustralianUNLOCO, Core.Constants.Languages.English);

			var actualAddress = new DocDeliveryContactAddressFormatter(recipient.Factory, recipient, GlbCompany.CurrentCompany).PostalAddressWithoutCompanyName();
			AssertEquals("LEVEL 69\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27", actualAddress);
		}
	}
}
