using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class WrapperPostalAddressFormatterTest : TestCaseWithFactory
	{
		#region Test PostalAddress with DeliveryContact
		public void TestInternationalOneLineAddressForDeliveryContact()
		{
			DocDeliveryContact internationalOneLineRecipient = SetupTestAddressForDeliveryContact("William Smith Jnr.", "SUNSHINE CORP.", "1234 Smith Street", "", "New Foo City", "ABC", "123 XG 27", CanadianUNLOCO);
			string expectedAddress = "WILLIAM SMITH JNR.\nSUNSHINE CORP.\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nCANADA";
			string actualAddress = NewPostalAddressFormatter(internationalOneLineRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestInternationalTwoLineAddressForDeliveryContact()
		{
			DocDeliveryContact internationalTwoLineRecipient = SetupTestAddressForDeliveryContact("William Smith Jnr.", "SUNSHINE CORP.", "1234 Smith Street", "Millenium Tower", "New Foo City", "ABC", "123 XG 27", CanadianUNLOCO);
			string expectedAddress = "WILLIAM SMITH JNR.\nSUNSHINE CORP.\n1234 SMITH STREET\nMILLENIUM TOWER\nNEW FOO CITY ABC 123 XG 27\nCANADA";
			string actualAddress = NewPostalAddressFormatter(internationalTwoLineRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestDomesticOneLineAddressForDeliveryContact()
		{
			DocDeliveryContact domesticOneLineRecipient = SetupTestAddressForDeliveryContact("William Smith Jnr.", "SUNSHINE CORP.", "1234 Smith Street", "", "New Foo City", "ABC", "123 XG 27", AustralianUNLOCO);
			string expectedAddress = "WILLIAM SMITH JNR.\nSUNSHINE CORP.\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nAUSTRALIA";
			string actualAddress = NewPostalAddressFormatter(domesticOneLineRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestDomesticTwoLineAddressForDeliveryContact()
		{
			DocDeliveryContact domesticTwoLineRecipient = SetupTestAddressForDeliveryContact("WILLIAM SMITH JNR.", "SUNSHINE CORP.", "1234 Smith Street", "MILLENIUM TOWER", "New Foo City", "ABC", "123 XG 27", AustralianUNLOCO);
			string expectedAddress = "WILLIAM SMITH JNR.\nSUNSHINE CORP.\n1234 SMITH STREET\nMILLENIUM TOWER\nNEW FOO CITY ABC 123 XG 27\nAUSTRALIA";
			string actualAddress = NewPostalAddressFormatter(domesticTwoLineRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestFrenchAddressForDeliveryContact()
		{
			DocDeliveryContact frenchRecipient = SetupTestAddressForDeliveryContact("A", "B", "1", "2", "PARIS", "", "12345", FrenchUNLOCO);
			string expectedAddress = "A\nB\n1\n2\n12345 PARIS\nFRANCE";
			string actualAddress = NewPostalAddressFormatter(frenchRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestFrenchAddressWithoutPostCodeForDeliveryContact()
		{
			DocDeliveryContact frenchRecipient = SetupTestAddressForDeliveryContact("A", "B", "1", "2", "PARIS", "", "", FrenchUNLOCO);
			string expectedAddress = "A\nB\n1\n2\nPARIS\nFRANCE";
			string actualAddress = NewPostalAddressFormatter(frenchRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestSpanishAddressForDeliveryContact()
		{
			DocDeliveryContact spanishRecipient = SetupTestAddressForDeliveryContact("A", "B", "1", "2", "ARANJUEZ", "MADRID", "1234", SpanishUNLOCO);
			string expectedAddress = "A\nB\n1\n2\n1234 ARANJUEZ (MADRID)\nSPAIN";
			string actualAddress = NewPostalAddressFormatter(spanishRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestSpanishAddressWithoutProvinceForDeliveryContact()
		{
			DocDeliveryContact spanishRecipient = SetupTestAddressForDeliveryContact("A", "B", "1", "2", "ARANJUEZ", "", "1234", SpanishUNLOCO);
			string expectedAddress = "A\nB\n1\n2\n1234 ARANJUEZ\nSPAIN";
			string actualAddress = NewPostalAddressFormatter(spanishRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		#endregion

		#region Test PostalAddress with OrgAddress
		public void TestInternationalOneLineAddressForOrgAddress()
		{
			OrgAddress internationalOneLineRecipient = SetupTestAddressForOrgAddress("XYZ Forwarders", "XYZFWLLC", "1234 Smith Street", "", "New Foo City", "ABC", "123 XG 27", CanadianUNLOCO);
			string expectedAddress = "XYZ FORWARDERS\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nCANADA";
			string actualAddress = NewPostalAddressFormatter(internationalOneLineRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestInternationalTwoLineAddressForOrgAddress()
		{
			OrgAddress internationalTwoLineRecipient = SetupTestAddressForOrgAddress("XYZ Forwarders", "XYZFWLLC", "Level 69", "1234 Smith Street", "New Foo City", "ABC", "123 XG 27", CanadianUNLOCO);
			string expectedAddress = "XYZ FORWARDERS\nLEVEL 69\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nCANADA";
			string actualAddress = NewPostalAddressFormatter(internationalTwoLineRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestDomesticOneLineAddressForOrgAddress()
		{
			OrgAddress domesticOneLineRecipient = SetupTestAddressForOrgAddress("XYZ Forwarders", "XYZFWLLC", "1234 Smith Street", "", "New Foo City", "ABC", "123 XG 27", AustralianUNLOCO);
			string expectedAddress = "XYZ FORWARDERS\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nAUSTRALIA";
			string actualAddress = NewPostalAddressFormatter(domesticOneLineRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestDomesticTwoLineAddressForOrgAddress()
		{
			OrgAddress domesticTwoLineRecipient = SetupTestAddressForOrgAddress("XYZ Forwarders", "XYZFWLLC", "Level 69", "1234 Smith Street", "New Foo City", "ABC", "123 XG 27", AustralianUNLOCO);
			string expectedAddress = "XYZ FORWARDERS\nLEVEL 69\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nAUSTRALIA";
			string actualAddress = NewPostalAddressFormatter(domesticTwoLineRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestFrenchAddressForOrgAddress()
		{
			OrgAddress frenchRecipient = SetupTestAddressForOrgAddress("A", "A", "1", "2", "PARIS", "", "12345", FrenchUNLOCO);
			string expectedAddress = "A\n1\n2\n12345 PARIS\nFRANCE";
			string actualAddress = NewPostalAddressFormatter(frenchRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestFrenchAddressWithoutPostCodeForOrgAddress()
		{
			OrgAddress frenchRecipient = SetupTestAddressForOrgAddress("A", "A", "1", "2", "PARIS", "", "", FrenchUNLOCO);
			string expectedAddress = "A\n1\n2\nPARIS\nFRANCE";
			string actualAddress = NewPostalAddressFormatter(frenchRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestSpanishAddressForOrgAddress()
		{
			OrgAddress spanishRecipient = SetupTestAddressForOrgAddress("A", "A", "1", "2", "ARANJUEZ", "MADRID", "1234", SpanishUNLOCO);
			string expectedAddress = "A\n1\n2\n1234 ARANJUEZ (MADRID)\nSPAIN";
			string actualAddress = NewPostalAddressFormatter(spanishRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestSpanishAddressWithoutProvinceForOrgAddress()
		{
			OrgAddress spanishRecipient = SetupTestAddressForOrgAddress("A", "A", "1", "2", "ARANJUEZ", "", "1234", SpanishUNLOCO);
			string expectedAddress = "A\n1\n2\n1234 ARANJUEZ\nSPAIN";
			string actualAddress = NewPostalAddressFormatter(spanishRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestFallsBackToOrgHeaderUNLOCOForOrgAddress()
		{
			OrgAddress internationalOneLineRecipient = SetupTestAddressForOrgAddress("XYZ Forwarders", "XYZFWLLC", "1234 Smith Street", "", "New Foo City", "ABC", "123 XG 27", CanadianUNLOCO);
			string saveState = internationalOneLineRecipient.OA_State;
			TestHeader.OH_RL_NKClosestPort = CanadianUNLOCO.RL_Code;
			internationalOneLineRecipient.OA_RL_NKRelatedPortCode = ""; // Changes OA_State
			internationalOneLineRecipient.OA_State = saveState;
			string expectedAddress = "XYZ FORWARDERS\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nCANADA";
			string actualAddress = NewPostalAddressFormatter(internationalOneLineRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		#endregion

		#region Test PostalAddress with JobDocAddress AS OA Pivot

		public void TestInternationalOneLineAddressForJobDocAddressWithOA()
		{
			JobDocAddress internationalOneLineRecipient = SetupTestAddressForJobDocAddressWithOA("XYZ Forwarders", "XYZFWLLC", "1234 Smith Street", "", "New Foo City", "ABC", "123 XG 27", CanadianUNLOCO);
			string expectedAddress = "XYZ FORWARDERS\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nCANADA";
			string actualAddress = NewPostalAddressFormatter(internationalOneLineRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestInternationalOneLineAddressForJobDocAddressWithOAWhenAddressDeleted()
		{
			JobDocAddress internationalOneLineRecipient = SetupTestAddressForJobDocAddressWithOA("XYZ Forwarders", "XYZFWLLC", "1234 Smith Street", "", "New Foo City", "ABC", "123 XG 27", CanadianUNLOCO);
			var formatter = NewPostalAddressFormatter(internationalOneLineRecipient);
			internationalOneLineRecipient.Delete();
			string actualAddress = formatter.PostalAddress();
			AssertEquals("Empty", "", actualAddress);
		}

		public void TestInternationalTwoLineAddressForJobDocAddressWithOA()
		{
			JobDocAddress internationalTwoLineRecipient = SetupTestAddressForJobDocAddressWithOA("XYZ Forwarders", "XYZFWLLC", "Level 69", "1234 Smith Street", "New Foo City", "ABC", "123 XG 27", CanadianUNLOCO);
			string expectedAddress = "XYZ FORWARDERS\nLEVEL 69\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nCANADA";
			string actualAddress = NewPostalAddressFormatter(internationalTwoLineRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestDomesticOneLineAddressForJobDocAddressWithOA()
		{
			JobDocAddress domesticOneLineRecipient = SetupTestAddressForJobDocAddressWithOA("XYZ Forwarders", "XYZFWLLC", "1234 Smith Street", "", "New Foo City", "ABC", "123 XG 27", AustralianUNLOCO);
			string expectedAddress = "XYZ FORWARDERS\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nAUSTRALIA";
			string actualAddress = NewPostalAddressFormatter(domesticOneLineRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestDomesticTwoLineAddressForJobDocAddressWithOA()
		{
			JobDocAddress domesticTwoLineRecipient = SetupTestAddressForJobDocAddressWithOA("XYZ Forwarders", "XYZFWLLC", "Level 69", "1234 Smith Street", "New Foo City", "ABC", "123 XG 27", AustralianUNLOCO);
			string expectedAddress = "XYZ FORWARDERS\nLEVEL 69\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nAUSTRALIA";
			string actualAddress = NewPostalAddressFormatter(domesticTwoLineRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestFrenchAddressForJobDocAddressWithOA()
		{
			JobDocAddress frenchRecipient = SetupTestAddressForJobDocAddressWithOA("A", "A", "1", "2", "PARIS", "", "12345", FrenchUNLOCO);
			string expectedAddress = "A\n1\n2\n12345 PARIS\nFRANCE";
			string actualAddress = NewPostalAddressFormatter(frenchRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestFrenchAddressWithoutPostCodeForJobDocAddressWithOA()
		{
			JobDocAddress frenchRecipient = SetupTestAddressForJobDocAddressWithOA("A", "A", "1", "2", "PARIS", "", "", FrenchUNLOCO);
			string expectedAddress = "A\n1\n2\nPARIS\nFRANCE";
			string actualAddress = NewPostalAddressFormatter(frenchRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestSpanishAddressForJobDocAddressWithOA()
		{
			JobDocAddress spanishRecipient = SetupTestAddressForJobDocAddressWithOA("A", "A", "1", "2", "ARANJUEZ", "MADRID", "1234", SpanishUNLOCO);
			string expectedAddress = "A\n1\n2\n1234 ARANJUEZ (MADRID)\nSPAIN";
			string actualAddress = NewPostalAddressFormatter(spanishRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestSpanishAddressWithoutProvinceForJobDocAddressWithOA()
		{
			JobDocAddress spanishRecipient = SetupTestAddressForJobDocAddressWithOA("A", "A", "1", "2", "ARANJUEZ", "", "1234", SpanishUNLOCO);
			string expectedAddress = "A\n1\n2\n1234 ARANJUEZ\nSPAIN";
			string actualAddress = NewPostalAddressFormatter(spanishRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestFallsBackToOrgHeaderUNLOCOForJobDocAddressWithOA()
		{
			JobDocAddress internationalOneLineRecipient = SetupTestAddressForJobDocAddressWithOA("XYZ Forwarders", "XYZFWLLC", "1234 Smith Street", "", "New Foo City", "ABC", "123 XG 27", CanadianUNLOCO);
			string saveState = internationalOneLineRecipient.E2_State;
			TestHeader.OH_RL_NKClosestPort = CanadianUNLOCO.RL_Code;
			internationalOneLineRecipient.E2_RN_NKCountryCode = ""; // Changes OA_State
			internationalOneLineRecipient.E2_State = saveState;
			string expectedAddress = "XYZ FORWARDERS\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nCANADA";
			string actualAddress = NewPostalAddressFormatter(internationalOneLineRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		#endregion

		#region Test PostalAddress with JobDocAddress as Override Address

		public void TestInternationalOneLineAddressForJobDocAddress()
		{
			JobDocAddress internationalOneLineRecipient = SetupTestAddressForJobDocAddress("XYZ Forwarders", "XYZ", "1234 Smith Street", "", "New Foo City", "ABC", "123 XG 27", CanadianUNLOCO);
			string expectedAddress = "XYZ FORWARDERS\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nCANADA";
			string actualAddress = NewPostalAddressFormatter(internationalOneLineRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestInternationalTwoLineAddressForJobDocAddress()
		{
			JobDocAddress internationalTwoLineRecipient = SetupTestAddressForJobDocAddress("XYZ Forwarders", "XYZ", "Level 69", "1234 Smith Street", "New Foo City", "ABC", "123 XG 27", CanadianUNLOCO);
			string expectedAddress = "XYZ FORWARDERS\nLEVEL 69\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nCANADA";
			string actualAddress = NewPostalAddressFormatter(internationalTwoLineRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestDomesticOneLineAddressForJobDocAddress()
		{
			JobDocAddress domesticOneLineRecipient = SetupTestAddressForJobDocAddress("XYZ Forwarders", "XYZ", "1234 Smith Street", "", "New Foo City", "ABC", "123 XG 27", AustralianUNLOCO);
			string expectedAddress = "XYZ FORWARDERS\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nAUSTRALIA";
			string actualAddress = NewPostalAddressFormatter(domesticOneLineRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestDomesticTwoLineAddressForJobDocAddress()
		{
			JobDocAddress domesticTwoLineRecipient = SetupTestAddressForJobDocAddress("XYZ Forwarders", "XYZ", "Level 69", "1234 Smith Street", "New Foo City", "ABC", "123 XG 27", AustralianUNLOCO);
			string expectedAddress = "XYZ FORWARDERS\nLEVEL 69\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nAUSTRALIA";
			string actualAddress = NewPostalAddressFormatter(domesticTwoLineRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestFrenchAddressForJobDocAddress()
		{
			JobDocAddress frenchRecipient = SetupTestAddressForJobDocAddress("A", "A", "1", "2", "PARIS", "", "12345", FrenchUNLOCO);
			string expectedAddress = "A\n1\n2\n12345 PARIS\nFRANCE";
			string actualAddress = NewPostalAddressFormatter(frenchRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestFrenchAddressWithoutPostCodeForJobDocAddress()
		{
			JobDocAddress frenchRecipient = SetupTestAddressForJobDocAddress("A", "A", "1", "2", "PARIS", "", "", FrenchUNLOCO);
			string expectedAddress = "A\n1\n2\nPARIS\nFRANCE";
			string actualAddress = NewPostalAddressFormatter(frenchRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestSpanishAddressForJobDocAddress()
		{
			JobDocAddress spanishRecipient = SetupTestAddressForJobDocAddress("A", "A", "1", "2", "ARANJUEZ", "MADRID", "1234", SpanishUNLOCO);
			string expectedAddress = "A\n1\n2\n1234 ARANJUEZ (MADRID)\nSPAIN";
			string actualAddress = NewPostalAddressFormatter(spanishRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestSpanishAddressWithoutProvinceForJobDocAddress()
		{
			JobDocAddress spanishRecipient = SetupTestAddressForJobDocAddress("A", "A", "1", "2", "ARANJUEZ", "", "1234", SpanishUNLOCO);
			string expectedAddress = "A\n1\n2\n1234 ARANJUEZ\nSPAIN";
			string actualAddress = NewPostalAddressFormatter(spanishRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestFallsBackToEmptyCountryForJobDocAddress()
		{
			JobDocAddress internationalOneLineRecipient = SetupTestAddressForJobDocAddress("XYZ Forwarders", "XYZ", "1234 Smith Street", "", "New Foo City", "ABC", "123 XG 27", CanadianUNLOCO);
			internationalOneLineRecipient.E2_RN_NKCountryCode = ""; // Changes OA_State
			string expectedAddress = "XYZ FORWARDERS\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27";
			string actualAddress = NewPostalAddressFormatter(internationalOneLineRecipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		#endregion

		#region Test PostalAddress with Organisaction
		public void TestPostalAddressForOrgHeader()
		{
			OrgHeader recipient = SetupTestAddressForOrganisation("AAA Pty Ltd", "AAAPTY", "22A John Street", "Address 123", "Lala Land", "MMM", "9999", AustralianUNLOCO);
			string expectedAddress = "AAA PTY LTD\n22A JOHN STREET\nADDRESS 123\nLALA LAND MMM 9999\nAUSTRALIA";
			string actualAddress = NewPostalAddressFormatter(recipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestPostalAddressWithAdditionalInfo()
		{
			OrgHeader recipient = SetupTestAddressForOrganisation("AAA Pty Ltd", "AAAPTY", "22A John Street", "Address 123", "Lala Land", "MMM", "9999", AustralianUNLOCO);
			recipient.MainAddress.OA_AdditionalAddressInformation = "AAA BUILDING";
			string expectedAddress = "AAA PTY LTD\nAAA BUILDING\n22A JOHN STREET\nADDRESS 123\nLALA LAND MMM 9999\nAUSTRALIA";
			string actualAddress = NewPostalAddressFormatter(recipient).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}
		#endregion

		#region Test PostalAddress For DocDocAddress

		public void TestPostalAddressForDocDocAddress()
		{
			JobDocAddress jobDocAddress = SetupTestAddressForJobDocAddressWithOA("XYZ Forwarders", "XYZFWLLC", "1234 Smith Street", "", "New Foo City", "ABC", "123 XG 27", CanadianUNLOCO);
			string expectedAddress = "XYZ FORWARDERS\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nCANADA";
			var recipient = DocOrganisation.New(jobDocAddress, Factory).SelectedAddress;
			string actualAddress = new WrapperPostalAddressFormatter(recipient, DocCompany, false).PostalAddress();
			AssertEquals("Postal Address", expectedAddress, actualAddress);
		}

		#endregion

		#region Implementation
		DocDeliveryContact SetupTestAddressForDeliveryContact(string name, string companyName, string address1, string address2, string city, string state, string postCode, RefUNLOCO uNLOCO)
		{
			DocDeliveryContact contact = new DocDeliveryContact(Factory);
			contact.Name = name;
			contact.CompanyName = companyName;
			contact.Address1 = address1;
			contact.Address2 = address2;
			contact.City = city;
			contact.State = state;
			contact.PostCode = postCode;
			contact.UNLOCO = uNLOCO;
			return contact;
		}

		OrgAddress SetupTestAddressForOrgAddress(string name, string code, string address1, string address2, string city, string state, string postCode, RefUNLOCO uNLOCO)
		{
			TestHeader = Factory.New<OrgHeader>();
			TestAddress = TestHeader.Addresses.AddNew();
			TestAddress.OA_OH = TestHeader.PK;
			TestHeader.OH_FullName = name;
			TestHeader.OH_Code = code;
			TestHeader.OH_RL_NKClosestPort = uNLOCO.RL_Code;
			TestAddress.OA_Address1 = address1;
			TestAddress.OA_Address2 = address2;
			TestAddress.OA_City = city;
			TestAddress.OA_PostCode = postCode;
			TestAddress.OA_State = state; // Set State after UNLOCO to avoid it being defaulted to the UNLOCO's state
			TestAddress.OA_RN_NKCountryCode = uNLOCO.Country.RN_Code;

			return TestAddress;
		}

		JobDocAddress SetupTestAddressForJobDocAddressWithOA(string name, string code, string address1, string address2, string city, string state, string postCode, RefUNLOCO uNLOCO)
		{
			TestHeader = Factory.New<OrgHeader>();
			TestAddress = TestHeader.Addresses.AddNew();
			TestAddress.OA_OH = TestHeader.PK;
			TestHeader.OH_FullName = name;
			TestHeader.OH_Code = code;
			TestHeader.OH_RL_NKClosestPort = uNLOCO.RL_Code;
			TestAddress.OA_Address1 = address1;
			TestAddress.OA_Address2 = address2;
			TestAddress.OA_City = city;
			TestAddress.OA_PostCode = postCode;
			TestAddress.OA_State = state; // Set State after UNLOCO to avoid it being defaulted to the UNLOCO's state
			TestAddress.OA_RN_NKCountryCode = uNLOCO.Country.RN_Code;

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = TestAddress.PK;

			return docAddress;
		}

		JobDocAddress SetupTestAddressForJobDocAddress(string name, string code, string address1, string address2, string city, string state, string postCode, RefUNLOCO uNLOCO)
		{
			var testAddress = Factory.New<JobDocAddress>();
			testAddress.E2_AddressOverride = true;
			testAddress.E2_CompanyName = name;
			testAddress.E2_AddressType = code;
			testAddress.E2_Address1 = address1;
			testAddress.E2_Address2 = address2;
			testAddress.E2_City = city;
			testAddress.E2_Postcode = postCode;
			testAddress.E2_State = state; // Set State after UNLOCO to avoid it being defaulted to the UNLOCO's state
			testAddress.E2_RN_NKCountryCode = uNLOCO.Country.RN_Code;
			testAddress.Address.OA_RN_NKCountryCode = uNLOCO.Country.RN_Code;

			return testAddress;
		}

		OrgHeader SetupTestAddressForOrganisation(string name, string code, string address1, string address2, string city, string state, string postCode, RefUNLOCO uNLOCO)
		{
			TestOrg = Factory.New<OrgHeader>();
			TestOrg.OH_FullName = name;
			TestOrg.OH_Code = code;
			TestOrg.MainAddress.OA_Address1 = address1;
			TestOrg.MainAddress.OA_Address2 = address2;
			TestOrg.MainAddress.OA_City = city;
			TestOrg.MainAddress.OA_PostCode = postCode;
			TestOrg.OH_RL_NKClosestPort = uNLOCO.RL_Code;
			TestOrg.MainAddress.OA_State = state;
			return TestOrg;
		}

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			AustralianUNLOCO = Top1UNLOCOForCountry("AUSTRALIA");
			CanadianUNLOCO = Top1UNLOCOForCountry("CANADA");
			FrenchUNLOCO = Top1UNLOCOForCountry("FRANCE");
			SpanishUNLOCO = Top1UNLOCOForCountry("SPAIN");
			DocCompany = DocCompany.New(GlbCompany.CurrentCompany, Factory);
			base.SetUp();
		}

		RefUNLOCO Top1UNLOCOForCountry(string countryName)
		{
			ZQuery countryFilter = new ZQuery(RefCountrySchema.RN_Desc, countryName);
			RefCountry country = Factory.LoadTop1<RefCountry>(countryFilter);
			ZQuery uNLOCOFilter = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, country.RN_Code);
			return Factory.LoadTop1<RefUNLOCO>(uNLOCOFilter);
		}

		WrapperPostalAddressFormatter NewPostalAddressFormatter(DocDeliveryContact recipient)
		{
			return new WrapperPostalAddressFormatter(recipient, DocCompany);
		}

		WrapperPostalAddressFormatter NewPostalAddressFormatter(OrgAddress recipient)
		{
			DocAddress docAddress = DocAddress.New(recipient, Factory);
			return new WrapperPostalAddressFormatter(docAddress, DocCompany);
		}

		WrapperPostalAddressFormatter NewPostalAddressFormatter(JobDocAddress recipient)
		{
			DocDocAddress docAddress = DocDocAddress.New(recipient, Factory);
			return new WrapperPostalAddressFormatter(docAddress, DocCompany);
		}

		WrapperPostalAddressFormatter NewPostalAddressFormatter(OrgHeader recipient)
		{
			DocOrganisation org = DocOrganisation.New(recipient, Factory);
			return new WrapperPostalAddressFormatter(org, DocCompany);
		}

		RefUNLOCO AustralianUNLOCO, CanadianUNLOCO, FrenchUNLOCO, SpanishUNLOCO;
		OrgAddress TestAddress;
		OrgHeader TestHeader;
		OrgHeader TestOrg;
		DocCompany DocCompany;

		#endregion
	}
}
