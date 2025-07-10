using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocDocAddress))]
	public class DocDocAddressTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocDocAddress.New(BizAddress, Factory)
			};
		}

		[ExpectNoExceptions]
		public void TestFakeOrgHeaderShouldNotBreakFactorySave()
		{
			var orgProxy = GlbBranch.CurrentBranch.OrgProxy;

			OrganisationsDataRegistry.Instance.ExportAirBroker.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, orgProxy.PK.ToGuid());

			Factory.Save();

			var docDocAddress = DocDocAddress.New(Factory.GetNull<OrgAddress>(), Factory);
			Factory.Save();
		}

		public void TestGetContactPhoneShouldUseDocAddressPhoneBeforeFallbackIfContactExists()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "UNITTEST";

			var mainAddress = orgHeader.MainAddress;
			mainAddress.OA_Address1 = "Main Address 1";
			mainAddress.OA_Phone = "Main Phone";

			var otherAddress = orgHeader.Addresses.AddNew();
			otherAddress.OA_OH = orgHeader.PK;
			otherAddress.OA_Address1 = "Other Address 1";
			otherAddress.OA_Phone = "Other Phone";
			otherAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);

			var contact = orgHeader.Contacts.AddNew();
			contact.OC_OH = orgHeader.PK;
			contact.OC_ContactName = "Test Contact";

			Factory.Save();

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = otherAddress.PK;
			docAddress.ContactPK = contact.PK;

			var wrapper = DocDocAddress.New(docAddress, Factory);

			AssertEquals("Test Contact", wrapper.GetContactName(ContactType.LocalTransport));
			AssertEquals("Other Phone", wrapper.GetContactPhone(ContactType.LocalTransport));
		}

		public void TestGetContactFaxShouldUseDocAddressFaxBeforeFallbackIfContactExists()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "UNITTEST";

			var mainAddress = orgHeader.MainAddress;
			mainAddress.OA_Address1 = "Main Address 1";
			mainAddress.OA_Fax = "Main Fax";

			var otherAddress = orgHeader.Addresses.AddNew();
			otherAddress.OA_OH = orgHeader.PK;
			otherAddress.OA_Address1 = "Other Address 1";
			otherAddress.OA_Fax = "Other Fax";
			otherAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);

			var contact = orgHeader.Contacts.AddNew();
			contact.OC_OH = orgHeader.PK;
			contact.OC_ContactName = "Test Contact";

			Factory.Save();

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = otherAddress.PK;
			docAddress.ContactPK = contact.PK;

			var wrapper = DocDocAddress.New(docAddress, Factory);

			AssertEquals("Test Contact", wrapper.GetContactName(ContactType.LocalTransport));
			AssertEquals("Other Fax", wrapper.GetContactFax(ContactType.LocalTransport));
		}

		public void TestGetContactShouldInitializeAddressForNonExistingContact()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "UNITTEST";

			var mainAddress = orgHeader.MainAddress;
			mainAddress.OA_Address1 = "Main Address 1";
			mainAddress.OA_Phone = "Main Phone";
			mainAddress.OA_Fax = "Main Fax";

			var otherAddress = orgHeader.Addresses.AddNew();
			otherAddress.OA_OH = orgHeader.PK;
			otherAddress.OA_Address1 = "Other Address 1";
			otherAddress.OA_Phone = "Other Phone";
			otherAddress.OA_Fax = "Other Fax";
			otherAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);

			Factory.Save();

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = otherAddress.PK;

			var wrapper = DocDocAddress.New(docAddress, Factory);

			AssertEquals("Other Phone", wrapper.GetContactPhone(ContactType.LocalTransport));
			AssertEquals("Other Fax", wrapper.GetContactFax(ContactType.LocalTransport));
		}

		public void TestBasicDocAddressFields()
		{
			var dA = Factory.New<JobDocAddress>();
			dA.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			dA.E2_AddressOverride = true;
			dA.E2_AddressType = "QQQ";
			dA.E2_Contact = "Contact";
			dA.E2_CompanyName = "Company";
			dA.E2_Address1 = "11";
			dA.E2_Address2 = "21";
			dA.E2_City = "31";
			dA.E2_State = "41";
			dA.E2_Postcode = "51";
			var country = Factory.New<RefCountry>();
			country.RN_Code = Core.Constants.CountryCodes.Australia;
			AssertNotNull(country);
			AssertNotNull(dA.Country);
			dA.E2_Phone = "0420 019 999";
			dA.E2_Fax = "02 5599 7777";
			dA.E2_Email = "81";

			var contact = Factory.New<OrgContact>();
			contact.OC_Phone = "0420 019 999";
			contact.OC_Fax = "02 5599 7777";

			DocDocAddress dAA = DocDocAddress.New(dA, Factory);
			//values should return manually set values
			AssertEquals(dAA.Type, "QQQ");
			AssertEquals(dAA.ContactName, "Contact");
			AssertEquals(dAA.CompanyName, "Company");
			AssertEquals(dAA.Address1, "11");
			AssertEquals(dAA.Address2, "21");
			AssertEquals(dAA.City, "31");
			AssertEquals(dAA.State, "41");
			AssertEquals(dAA.PostCode, "51");
			AssertEquals(dAA.Country.GetType(), typeof(DocCountry));
			AssertEquals(dAA.Phone, "+61 420 019 999");
			AssertEquals(dAA.Fax, "+61 2 5599 7777");
			AssertEquals(dAA.Email, "81");
		}

		public void TestCompanyNameAddress1Address2City()
		{
			BizAddress.Header.OH_FullName = "FullName";
			BizAddress.OA_Address1 = "Address1";
			BizAddress.OA_Address2 = "Address2";
			BizAddress.OA_City = "City";
			AssertEquals("CompanyNameAndAddress", "FullName Address1 Address2 City", DocAddress.CompanyNameAddress1Address2City);
		}

		public void TestAddressSummaryExcudeAddress1Address2()
		{
			var dA = Factory.New<JobDocAddress>();
			dA.E2_Address1 = "11";
			dA.E2_Address2 = "21";
			dA.E2_City = "31";
			dA.E2_State = "41";
			dA.E2_Postcode = "51";
			dA.E2_RN_NKCountryCode = "CN";
			var country = Factory.New<RefCountry>();
			country.RN_Code = "CN";
			country.RN_Desc = "China";

			DocDocAddress dAA = DocDocAddress.New(dA, Factory);
			AssertEquals("CompanyNameAndAddress", "31 41 51 CHINA ", dAA.AddressSummaryExcudeAddress1Address2);
		}

		public void TestCode()
		{
			ZString code = new ZString("TEST");

			BizAddress.OA_Code = code;
			AssertEquals("Wrapped Value", code, DocAddress.Code);
		}

		public void TestDescription()
		{
			OrgAddress bizAddress = BizOrg.Addresses.AddNew();
			BizDocAddress = Factory.New<JobDocAddress>();
			DocAddress = DocDocAddress.New(BizDocAddress, Factory);

			ZString description = new ZString("TES");

			BizDocAddress.E2_AddressType = description;
			AssertEquals("Wrapped Value", description, DocAddress.Description);
		}

		public void TestCompanyName()
		{
			AssertEquals("Company Name", ZString.Empty, DocAddress.CompanyName);

			ZString companyName = "Test CompanyName";
			ZString companyNameOverride = "Test CompanyName Override";

			BizAddress.OA_CompanyNameOverride = "";
			BizAddress.Header.OH_FullName = companyName;
			AssertEquals("Wrapped Value when override empty", companyName, DocAddress.CompanyName);

			BizAddress.OA_CompanyNameOverride = companyNameOverride;
			AssertEquals("Wrapped Value when override set", companyNameOverride, DocAddress.CompanyName);
		}

		public void TestAddressType()
		{
			OrgAddress bizAddress = BizOrg.Addresses.AddNew();
			DocAddress = DocDocAddress.New(bizAddress, Factory);

			ZString addressType = "PAD";

			bizAddress.AddressCapability.SetCapabilityEnabled(addressType);
			AssertEquals("Wrapped Value", DocAddress.AddressType, addressType);
		}

		public void TestBasicFieldsInOrgAddressPivotSituation()
		{
			ZString address1 = "Test Address1";
			BizAddress.OA_Address1 = address1;
			AssertEquals("Wrapped Value", address1, DocAddress.Address1);

			ZString address2 = "Test Address2";
			BizAddress.OA_Address2 = address2;
			AssertEquals("Wrapped Value", address2, DocAddress.Address2);

			ZString city = "Test City";
			BizAddress.OA_City = city;
			AssertEquals("Wrapped Value", city, DocAddress.City);

			ZString phone = "Test Phone";
			BizAddress.OA_Phone = phone;
			AssertEquals("Wrapped Value", phone, DocAddress.Phone);

			ZString fax = "Test Fax";
			BizAddress.OA_Fax = fax;
			AssertEquals("Wrapped Value", fax, DocAddress.Fax);

			ZString language = "TLX";
			BizAddress.OA_Language = language;
			AssertEquals("Wrapped Value", language, DocAddress.Language);

			AssertNotNull("Wrapped Value when FK Valid", DocAddress.Organisation);

			BizAddress.OA_OH = ZGuid.Invalid;
			AssertNull("Wrapped Value when FK Invalid", DocAddress.Organisation);

			ZString postCode = "PostCode";
			BizAddress.OA_PostCode = postCode;
			AssertEquals("Wrapped Value", postCode, DocAddress.PostCode);

			ZString state = "Test State";
			BizAddress.OA_State = state;
			AssertEquals("Wrapped Value", state, DocAddress.State);

			ZString usageComment = "Test UsageComment";
			BizAddress.OA_Code = usageComment;
			AssertEquals("Wrapped Value", usageComment, DocAddress.UsageComment);
		}

		public void TestCountry()
		{
			AssertNull("Wrapped Value when Loco NK not set for OrgHeader", DocAddress.Country);

			BizAddress.Header.OH_RL_NKClosestPort = "GBLON";
			AssertNotNull("UNLOCO not null when NK set", BizAddress.Header.UNLOCO);
			AssertNotNull("Wrapped Value not null when NK set", DocAddress.Country);
			AssertEquals("Header Country returned when Org Loco FK set", "GB", DocAddress.Country.Code);
		}

		public void TestPostalAddress()
		{
			AssertEquals("Postal Address returned by PostalAddressFormatter", ZString.Empty, DocAddress.PostalAddress);

			BizAddress.OA_Address1 = "Address 1";
			BizAddress.OA_Address2 = "Address 2";
			BizOrg.OH_FullName = "Name";
			AssertEquals("Postal Address returned by PostalAddressFormatter", "NAME\nADDRESS 1\nADDRESS 2", DocAddress.PostalAddress);
		}

		public void TestPostalAddressExcludeName()
		{
			BizAddress.OA_Address1 = "Address 1";
			BizAddress.OA_Address2 = "Address 2";
			BizOrg.OH_FullName = "Name";
			AssertEquals("Postal Address without Name", "ADDRESS 1\nADDRESS 2", DocAddress.PostalAddressExcludeName);
		}

		public void TestPostalAddressInEnglish()
		{
			var staffDE = Factory.New<GlbStaff>();
			staffDE.GS_Code = "ABC";
			staffDE.GS_LoginName = "Dieter";
			staffDE.GS_FullName = "Dieter";
			staffDE.GS_WorkingLanguage = Core.SharedConstants.Languages.German;

			Factory.Save();

			using (Env.Instance.SetTemporaryUserContext(staffDE.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				BizAddress.OA_Address1 = "Address 1";
				BizAddress.OA_Address2 = "Address 2";
				BizAddress.OA_RL_NKRelatedPortCode = "DEHAM";
				BizOrg.OH_FullName = "Name";
				AssertEquals("Country name is in English", "NAME\nADDRESS 1\nADDRESS 2\nGERMANY", DocAddress.PostalAddressInEnglish);
			}
		}

		#region ZDateTime fields

		public void TestDateTimeFieldsInOrgAddressPivotSituation()
		{
			From = ZDateTime.Today;
			BizAddress.OA_PickupFromTimeOnly = From;
			AssertEquals("PickupFromTimeOnly", From, DocAddress.PickupFromTimeOnly);

			To = ZDateTime.Today;
			BizAddress.OA_PickupToTimeOnly = To;
			AssertEquals("PickupToTimeOnly", To, DocAddress.PickupToTimeOnly);

			From = ZDateTime.Today;
			BizAddress.OA_DeliverFromTimeOnly = From;
			AssertEquals("DeliverFromTimeOnly", From, DocAddress.DeliverFromTimeOnly);

			To = ZDateTime.Today;
			BizAddress.OA_DeliverToTimeOnly = To;
			AssertEquals("DeliverToTimeOnly", To, DocAddress.DeliverToTimeOnly);

			From = ZDateTime.Today;
			BizAddress.OA_DoNotAttendFrom = From;
			AssertEquals("DoNotAttendFrom", From, DocAddress.DoNotAttendFrom);

			To = ZDateTime.Today;
			BizAddress.OA_DoNotAttendTo = To;
			AssertEquals("DoNotAttendTo", To, DocAddress.DoNotAttendTo);
		}

		public void TestDoNotAttendFromForCartageAdvice()
		{
			ZDateTime from = ZDateTime.Today;

			OrgAddress address = Factory.New<OrgAddress>();
			address.OA_DoNotAttendFrom = from;
			DocDocAddress addressWrapper = DocDocAddress.New(address, Factory);
			AssertEquals("DoNotAttendFromForCartageAdvice", ZDateTime.Empty, addressWrapper.DoNotAttendFromForCartageAdvice);

			OrgHeader org = Factory.New<OrgHeader>();
			address = org.Addresses.AddNew();
			address.OA_DoNotAttendFrom = from;
			addressWrapper = DocDocAddress.New(address, Factory);
			AssertEquals("DoNotAttendFromForCartageAdvice", ZDateTime.Empty, addressWrapper.DoNotAttendFromForCartageAdvice);

			org.OH_IsPackDepot = true;
			AssertEquals("DoNotAttendFromForCartageAdvice", from, addressWrapper.DoNotAttendFromForCartageAdvice);

			org.OH_IsPackDepot = false;
			org.OH_IsUnpackDepot = true;
			AssertEquals("DoNotAttendFromForCartageAdvice", from, addressWrapper.DoNotAttendFromForCartageAdvice);

			org.OH_IsUnpackDepot = false;
			org.OH_IsContainerYard = true;
			AssertEquals("DoNotAttendFromForCartageAdvice", from, addressWrapper.DoNotAttendFromForCartageAdvice);
		}

		public void TestDoNotAttendToForCartageAdvice()
		{
			ZDateTime to = ZDateTime.Today;

			OrgAddress address = Factory.New<OrgAddress>();
			address.OA_DoNotAttendTo = to;
			DocDocAddress addressWrapper = DocDocAddress.New(address, Factory);
			AssertEquals("DoNotAttendToForCartageAdvice", ZDateTime.Empty, addressWrapper.DoNotAttendToForCartageAdvice);

			OrgHeader org = Factory.New<OrgHeader>();
			address = org.Addresses.AddNew();
			address.OA_DoNotAttendTo = to;
			addressWrapper = DocDocAddress.New(address, Factory);
			AssertEquals("DoNotAttendFromForCartageAdvice", ZDateTime.Empty, addressWrapper.DoNotAttendFromForCartageAdvice);

			org.OH_IsPackDepot = true;
			AssertEquals("DoNotAttendToForCartageAdvice", to, addressWrapper.DoNotAttendToForCartageAdvice);

			org.OH_IsPackDepot = false;
			org.OH_IsUnpackDepot = true;
			AssertEquals("DoNotAttendToForCartageAdvice", to, addressWrapper.DoNotAttendToForCartageAdvice);

			org.OH_IsUnpackDepot = false;
			org.OH_IsContainerYard = true;
			AssertEquals("DoNotAttendToForCartageAdvice", to, addressWrapper.DoNotAttendToForCartageAdvice);
		}

		public void TestNoOrgAddressDateTimeFields()
		{
			AssertEquals("PickupFromTimeOnly", ZDateTime.Empty, DocAddress.PickupFromTimeOnly);
			AssertEquals("PickupToTimeOnly", ZDateTime.Empty, DocAddress.PickupToTimeOnly);
			AssertEquals("DeliverFromTimeOnly", ZDateTime.Empty, DocAddress.DeliverFromTimeOnly);
			AssertEquals("DeliverToTimeOnly", ZDateTime.Empty, DocAddress.DeliverToTimeOnly);
			AssertEquals("DoNotAttendFrom", ZDateTime.Empty, DocAddress.DoNotAttendFrom);
			AssertEquals("DoNotAttendTo", ZDateTime.Empty, DocAddress.DoNotAttendTo);

			AssertEquals("DoNotAttendFromForCartageAdvice", ZDateTime.Empty, DocAddress.DoNotAttendFromForCartageAdvice);

			AssertEquals("DoNotAttendToForCartageAdvice", ZDateTime.Empty, DocAddress.DoNotAttendToForCartageAdvice);
		}

		#endregion

		public void TestToString()
		{
			AssertEquals("ToString is PostalAddress", DocAddress.PostalAddress, DocAddress.ToString());
		}

		#region Contacts

		public void TestGetContactInOrgAddressPivotSituation()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			OrgAddress addy = org.Addresses.AddNew();
			addy.OA_Address1 = "abcd";
			addy.OA_Phone = "123";

			OrgContact contactImportSeaDepot = org.Contacts.AddNew();
			contactImportSeaDepot.OC_Phone = "1234";
			contactImportSeaDepot.OC_ContactName = "ImportSeaDepot";
			contactImportSeaDepot.Documents.AddNew().OD_DocumentGroup = ContactType.ImportSeaDepot.Code;

			OrgContact contactLocalTransport = org.Contacts.AddNew();
			contactLocalTransport.OC_Phone = "1234";
			contactLocalTransport.OC_ContactName = "LocalTransport";
			contactLocalTransport.Documents.AddNew().OD_DocumentGroup = ContactType.LocalTransport.Code;

			DocDocAddress addressWrapper = DocDocAddress.New(addy, Factory);
			AssertEquals("Contact should be ImportSeaDepot Contact", "ImportSeaDepot", addressWrapper.GetContact(ContactType.ImportSeaDepot).ContactName);

			addressWrapper = DocDocAddress.New(addy, Factory);
			AssertEquals("Contact should be LocalTransport Contact", "LocalTransport", addressWrapper.GetContact(ContactType.LocalTransport).ContactName);
		}

		public void TestGetContactWhenOverride()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			OrgAddress addy = org.Addresses.AddNew();
			addy.OA_Address1 = "abcd";
			addy.OA_Phone = "123";

			OrgContact contactImportSeaDepot = org.Contacts.AddNew();
			contactImportSeaDepot.OC_Phone = "1234";
			contactImportSeaDepot.OC_ContactName = "ImportSeaDepot";
			contactImportSeaDepot.Documents.AddNew().OD_DocumentGroup = ContactType.ImportSeaDepot.Code;

			OrgContact contactLocalTransport = org.Contacts.AddNew();
			contactLocalTransport.OC_Phone = "1234";
			contactLocalTransport.OC_ContactName = "LocalTransport";
			contactLocalTransport.Documents.AddNew().OD_DocumentGroup = ContactType.LocalTransport.Code;

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = org.PK;
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = addy.PK;

			DocDocAddress addressWrapper = DocDocAddress.New(shipment.NotifyPartyDocumentaryAddress, Factory);
			AssertEquals("Contact should be ImportSeaDepot Contact", "ImportSeaDepot", addressWrapper.GetContact(ContactType.ImportSeaDepot).ContactName);

			addressWrapper = DocDocAddress.New(addy, Factory);
			AssertEquals("Contact should be LocalTransport Contact", "LocalTransport", addressWrapper.GetContact(ContactType.LocalTransport).ContactName);

			shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = true;
			shipment.NotifyPartyDocumentaryAddress.E2_Contact = "Other Name";
			shipment.NotifyPartyDocumentaryAddress.E2_Phone = "1212";
			shipment.NotifyPartyDocumentaryAddress.E2_Fax = "3434";
			shipment.NotifyPartyDocumentaryAddress.E2_Email = "other@email.com";

			addressWrapper = DocDocAddress.New(shipment.NotifyPartyDocumentaryAddress, Factory);
			AssertEquals("ImportSeaDepot when override - return override contact - Name is Other Name", "Other Name", addressWrapper.GetContact(ContactType.ImportSeaDepot).ContactName);
			AssertEquals("ImportSeaDepot when override - return override contact - override phone", "1212", addressWrapper.GetContact(ContactType.ImportSeaDepot).Phone);
			AssertEquals("ImportSeaDepot when override - return override contact - override fax", "3434", addressWrapper.GetContact(ContactType.ImportSeaDepot).Fax);
			AssertEquals("ImportSeaDepot when override - return override contact - override email", "other@email.com", addressWrapper.GetContact(ContactType.ImportSeaDepot).Email);

			AssertEquals("LocalTransport when override - return override contact - Name is Other Name", "Other Name", addressWrapper.GetContact(ContactType.LocalTransport).Name);
			AssertEquals("LocalTransport when override - return override contact - override phone", "1212", addressWrapper.GetContact(ContactType.LocalTransport).Phone);
			AssertEquals("LocalTransport when override - return override contact - override fax", "3434", addressWrapper.GetContact(ContactType.LocalTransport).Fax);
			AssertEquals("LocalTransport when override - return override contact - override email", "other@email.com", addressWrapper.GetContact(ContactType.LocalTransport).Email);
		}

		#endregion

		#region Customs Code - LocalControlledPremisesID

		public void TestLocalControlledPremisesIDFieldsInOrgAddressPivotSituation()
		{
			OrgCusCode cCP = BizOrg.CustomsCodes.AddNew();
			cCP.OK_CustomsRegNo = "123";
			cCP.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			cCP.OK_OA_PremisesAddress = BizAddress.PK;
			AssertEquals("123", DocAddress.LocalControlledPremisesID);

			cCP = BizOrg.CustomsCodes.AddNew();
			cCP.OK_CustomsRegNo = "123";
			cCP.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			cCP.OK_OA_PremisesAddress = BizAddress.PK;
			AssertEquals("123", DocAddress.WarehouseLocalControlledPremisesID);

			cCP = BizOrg.CustomsCodes.AddNew();
			cCP.OK_CustomsRegNo = "123";
			cCP.OK_CodeType = OrgCusCode.CodeTypes.DepotControlledPremisesID;
			cCP.OK_OA_PremisesAddress = BizAddress.PK;
			AssertEquals("123", DocAddress.DepotLocalControlledPremisesID);
		}

		public void TestNoOrgAddressLocalControlledPremisesIDFields()
		{
			var dA = Factory.New<JobDocAddress>();
			dA.E2_AddressOverride = true;
			DocAddress = DocDocAddress.New(dA, Factory);

			AssertEquals("", DocAddress.LocalControlledPremisesID);
			AssertEquals("", DocAddress.WarehouseLocalControlledPremisesID);
			AssertEquals("", DocAddress.DepotLocalControlledPremisesID);
		}

		#endregion

		#region Split Address

		public void TestSplitAddressWithLengthMoreThan25()
		{
			BizAddress.OA_Address1 = "12 Address one";
			BizAddress.OA_Address2 = "Address two";
			BizAddress.OA_City = "City";
			BizAddress.OA_State = "State";
			BizAddress.OA_PostCode = "Code";

			ZString unformattedPostalAddress = DocAddress.PostalAddress.Replace("\n", " ");
			ZString[] splitFormattedAddress = DocAddress.WrapTextForAColumn(unformattedPostalAddress, 30).Split('\n');
			ZString expectedSplitAddress1 = splitFormattedAddress[0];
			ZString expectedSplitAddress2 = DocAddress.WrapTextForAColumn(splitFormattedAddress[1], 25).Split('\n')[0];

			AssertEquals("SplitAddress1", expectedSplitAddress1, DocAddress.SplitAddress1);
			AssertEquals("SplitAddress2", expectedSplitAddress2, DocAddress.SplitAddress2);
		}

		public void TestSplitAddressWithLengthLessThan25()
		{
			BizAddress.OA_Address1 = "12 Address one";
			BizAddress.OA_City = "City";

			ZString unformattedPostalAddress = DocAddress.PostalAddress.Replace("\n", " ");
			ZString[] splitFormattedAddress = DocAddress.WrapTextForAColumn(unformattedPostalAddress, 30).Split('\n');
			ZString expectedSplitAddress1 = splitFormattedAddress[0];
			ZString expectedSplitAddress2 = ZString.Empty;

			AssertEquals("SplitAddress1", expectedSplitAddress1, DocAddress.SplitAddress1);
			AssertEquals("SplitAddress2", expectedSplitAddress2, DocAddress.SplitAddress2);
		}

		#endregion

		#region Warehousing Facilities and Loading/Unloading Constraints

		public void TestHasWareHousingInOrgAddressPivotSituation()
		{
			AssertEquals("HasWareHousing should be false", ZBool.False, DocAddress.HasWareHousing);

			BizAddress.OA_DockLeveler = ZBool.True;
			AssertEquals("HasWareHousing should be true", ZBool.True, DocAddress.HasWareHousing);

			BizAddress.OA_DockLeveler = ZBool.False;
			BizAddress.OA_PalletJack = ZBool.True;
			AssertEquals("HasWareHousing should be true", ZBool.True, DocAddress.HasWareHousing);

			BizAddress.OA_PalletJack = ZBool.False;
			BizAddress.OA_ForkLift = ZBool.True;
			AssertEquals("HasWareHousing should be true", ZBool.True, DocAddress.HasWareHousing);

			BizAddress.OA_ForkLift = ZBool.False;
			BizAddress.OA_OtherWarehouseFacilities = "Other facilities";
			AssertEquals("HasWareHousing should be true", ZBool.True, DocAddress.HasWareHousing);

			BizAddress.OA_OtherWarehouseFacilities = ZString.Empty;
			AssertEquals("HasWareHousing should be false", ZBool.False, DocAddress.HasWareHousing);
		}

		public void TestHasLoadingUnloadingConstraintsInOrgAddressPivotSituation()
		{
			AssertEquals("HasLoadingUnloadingConstraints should be false", ZBool.False, DocAddress.HasLoadingUnloadingConstraints);

			BizAddress.OA_AccessPoint = BizAddress.OA_AccessPoint_List[0].Code;
			AssertEquals("HasLoadingUnloadingConstraints should be true", ZBool.True, DocAddress.HasLoadingUnloadingConstraints);

			BizAddress.OA_AccessPoint = ZString.Empty;
			BizAddress.OA_ContainerHandling = BizAddress.OA_ContainerHandling_List[0].Code;
			AssertEquals("HasLoadingUnloadingConstraints should be true", ZBool.True, DocAddress.HasLoadingUnloadingConstraints);

			BizAddress.OA_ContainerHandling = ZString.Empty;
			BizAddress.OA_CommunicationRequired = BizAddress.OA_CommunicationRequired_List[0].Code;
			AssertEquals("HasLoadingUnloadingConstraints should be true", ZBool.True, DocAddress.HasLoadingUnloadingConstraints);

			BizAddress.OA_CommunicationRequired = ZString.Empty;
			BizAddress.OA_LabourRequired = BizAddress.OA_LabourRequired_List[0].Code;
			AssertEquals("HasLoadingUnloadingConstraints should be true", ZBool.True, DocAddress.HasLoadingUnloadingConstraints);

			BizAddress.OA_LabourRequired = ZString.Empty;
			BizAddress.OA_Dock_Height = BizAddress.OA_Dock_Height_List[0].Code;
			AssertEquals("HasLoadingUnloadingConstraints should be true", ZBool.True, DocAddress.HasLoadingUnloadingConstraints);

			BizAddress.OA_Dock_Height = ZString.Empty;
			BizAddress.OA_LoadingUnloadingConstraints = "Further Constraints";
			AssertEquals("HasLoadingUnloadingConstraints should be true", ZBool.True, DocAddress.HasLoadingUnloadingConstraints);

			BizAddress.OA_LoadingUnloadingConstraints = ZString.Empty;
			AssertEquals("HasLoadingUnloadingConstraints should be False", ZBool.False, DocAddress.HasLoadingUnloadingConstraints);
		}

		public void TestBasicWarehousingFieldsInOrgAddressPivotSituation()
		{
			BizAddress.OA_DockLeveler = ZBool.False;
			AssertEquals("HasDockLeveller is ", ZBool.False, DocAddress.HasDockLeveller);

			BizAddress.OA_DockLeveler = ZBool.True;
			AssertEquals("HasDockLeveller is ", ZBool.True, DocAddress.HasDockLeveller);

			BizAddress.OA_PalletJack = ZBool.False;
			AssertEquals("HasPalletJack is ", ZBool.False, DocAddress.HasPalletJack);

			BizAddress.OA_PalletJack = ZBool.True;
			AssertEquals("HasPalletJack is ", ZBool.True, DocAddress.HasPalletJack);

			BizAddress.OA_ForkLift = ZBool.False;
			AssertEquals("HasForkLift is ", ZBool.False, DocAddress.HasForkLift);

			BizAddress.OA_ForkLift = ZBool.True;
			AssertEquals("HasForkLift is ", ZBool.True, DocAddress.HasForkLift);

			BizAddress.OA_OtherWarehouseFacilities = "Other warehouse facilities";
			AssertEquals("OtherWarehouseFacilities is ", "Other warehouse facilities", DocAddress.OtherWarehouseFacilities);

			BizAddress.OA_AccessPoint = BizAddress.OA_AccessPoint_List[0].Code;
			AssertEquals("AccessPoint is ", BizAddress.OA_AccessPoint_List.GetDescriptionFromCode(BizAddress.OA_AccessPoint), DocAddress.AccessPoint);

			BizAddress.OA_ContainerHandling = BizAddress.OA_ContainerHandling_List[0].Code;
			AssertEquals("ContainerHandling is ", BizAddress.OA_ContainerHandling_List.GetDescriptionFromCode(BizAddress.OA_ContainerHandling), DocAddress.ContainerHandling);

			BizAddress.OA_CommunicationRequired = BizAddress.OA_CommunicationRequired_List[0].Code;
			AssertEquals("CommunicationRequired is ", BizAddress.OA_CommunicationRequired_List.GetDescriptionFromCode(BizAddress.OA_CommunicationRequired), DocAddress.CommunicationRequired);

			BizAddress.OA_LabourRequired = BizAddress.OA_LabourRequired_List[0].Code;
			AssertEquals("LabourRequired is ", BizAddress.OA_LabourRequired_List.GetDescriptionFromCode(BizAddress.OA_LabourRequired), DocAddress.LabourRequired);

			BizAddress.OA_Dock_Height = BizAddress.OA_Dock_Height_List[0].Code;
			AssertEquals("DockHeight is ", BizAddress.OA_Dock_Height_List.GetDescriptionFromCode(BizAddress.OA_Dock_Height), DocAddress.DockHeight);

			BizAddress.OA_LoadingUnloadingConstraints = "Further constraints";
			AssertEquals("DockHeight is ", "Further constraints", DocAddress.FurtherConstraints);
		}
		#endregion

		#region Warehousing when Standard DocAddress

		public void TestHasNoWareHousingWhenOverride()
		{
			var dA = Factory.New<JobDocAddress>();
			dA.E2_AddressOverride = true;
			DocAddress = DocDocAddress.New(dA, Factory);

			AssertEquals("HasWareHousing should be false", false, DocAddress.HasWareHousing);

			AssertEquals("HasLoadingUnloadingConstraints should be false", false, DocAddress.HasLoadingUnloadingConstraints);

			AssertEquals("HasDockLeveller is ", false, DocAddress.HasDockLeveller);
			AssertEquals("HasPalletJack is ", false, DocAddress.HasPalletJack);
			AssertEquals("HasForkLift is ", false, DocAddress.HasForkLift);
			AssertEquals("OtherWarehouseFacilities is ", "", DocAddress.OtherWarehouseFacilities);
			AssertEquals("AccessPoint is ", "", DocAddress.AccessPoint);
			AssertEquals("ContainerHandling is ", "", DocAddress.ContainerHandling);
			AssertEquals("CommunicationRequired is ", "", DocAddress.CommunicationRequired);
			AssertEquals("LabourRequired is ", "", DocAddress.LabourRequired);
			AssertEquals("DockHeight is ", "", DocAddress.DockHeight);
			AssertEquals("DockHeight is ", "", DocAddress.FurtherConstraints);
		}
		#endregion

		#region Test Ctor

		public void TestCreateFromNullOrgAddress()
		{
			OrgAddress orgAddress = null;
			AssertNull(DocDocAddress.New(orgAddress, Factory));
			AssertNotNull(DocDocAddress.New(Factory.GetNull<OrgAddress>(), Factory));
		}

		#endregion

		#region TestPort

		public void TestPort()
		{
			BizAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			AssertEquals("Port ", "AUSYD", DocAddress.Port.Code);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			BizOrg = Factory.New<OrgHeader>();
			BizOrg.OH_Code = "BizOrg";
			BizAddress = BizOrg.MainAddress;
			BizAddress.OA_RN_NKCountryCode = ZString.Empty;
			DocAddress = DocDocAddress.New(BizAddress, Factory);
			AssertNotNull("PreCondition: Valid DocAddress", DocAddress);

			base.SetUp();
		}

		DocDocAddress DocAddress;
		JobDocAddress BizDocAddress;
		OrgAddress BizAddress;
		OrgHeader BizOrg;

		ZDateTime From;
		ZDateTime To;

		#endregion
	}
}
