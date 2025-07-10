using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocAddress))]
	public class DocAddressTest : DocumentWrapperTestCase
	{
		public void TestPKMatchesPKFromWrappedOrgAddress()
		{
			ZGuid orgAddressPK = BizAddress.PK;
			AssertEquals("DocAddress.PK should match the wrapped OrgAddresses PK", orgAddressPK, DocAddress.OrgAddress.PK);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocAddress.New(BizAddress, Factory)
			};
		}

		public void TestCompanyNameAddress1Address2City()
		{
			BizAddress.Header.OH_FullName = "FullName";
			BizAddress.OA_Address1 = "Address1";
			BizAddress.OA_Address2 = "Address2";
			BizAddress.OA_City = "City";
			AssertEquals("CompanyNameAndAddress", "FullName Address1 Address2 City", DocAddress.CompanyNameAddress1Address2City);
		}

		public void TestCode()
		{
			ZString code = new ZString("TEST");

			BizAddress.OA_Code = code;
			AssertEquals("Wrapped Value", code, DocAddress.Code);
		}

		public void TestDescription()
		{
			ZString description = new ZString("Test Description");

			BizAddress.OA_Code = description;
			AssertEquals("Wrapped Value", description, DocAddress.Description);
		}

		public void TestAddress1()
		{
			ZString address1 = "Test Address1";

			BizAddress.OA_Address1 = address1;
			AssertEquals("Wrapped Value", address1, DocAddress.Address1);
		}

		public void TestAddress2()
		{
			ZString address2 = "Test Address2";

			BizAddress.OA_Address2 = address2;
			AssertEquals("Wrapped Value", address2, DocAddress.Address2);
		}

		public void TestAddressType()
		{
			OrgAddress bizAddress = BizOrg.Addresses.AddNew();
			DocAddress = DocAddress.New(bizAddress, Factory);

			ZString addressType = "PAD";

			bizAddress.AddressCapability.SetCapabilityEnabled(addressType);
			AssertEquals("Wrapped Value", ZBool.True, DocAddress.AddressCapability.GetCapabilityEnabled(addressType));
			AssertEquals("Wrapped Value", ZBool.False, DocAddress.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Delivery));
		}

		public void TestCity()
		{
			ZString city = "Test City";

			BizAddress.OA_City = city;
			AssertEquals("Wrapped Value", city, DocAddress.City);
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

		public void TestPhone()
		{
			ZString phone = "Test Phone";

			BizAddress.OA_Phone = phone;
			AssertEquals("Wrapped Value", phone, DocAddress.Phone);
		}

		public void TestPhoneIsFormatted()
		{
			ZString phone = "+61426829924";

			BizAddress.OA_Phone = phone;
			AssertEquals("+61 426 829 924", DocAddress.Phone);
		}

		public void TestFax()
		{
			ZString fax = "Test Fax";

			BizAddress.OA_Fax = fax;
			AssertEquals("Wrapped Value", fax, DocAddress.Fax);
		}

		public void TestFaxIsFormatted()
		{
			ZString fax = "+61280012200";

			BizAddress.OA_Fax = fax;
			AssertEquals("+61 2 8001 2200", DocAddress.Fax);
		}

		public void TestLanguage()
		{
			ZString language = "TLX";

			BizAddress.OA_Language = language;
			AssertEquals("Wrapped Value", language, DocAddress.Language);
		}

		public void TestOrganisation()
		{
			AssertNotNull("Wrapped Value when FK Valid", DocAddress.Organisation);

			BizAddress.OA_OH = ZGuid.Invalid;
			AssertNull("Wrapped Value when FK Invalid", DocAddress.Organisation);
		}

		public void TestPostCode()
		{
			ZString postCode = "PostCode";

			BizAddress.OA_PostCode = postCode;
			AssertEquals("Wrapped Value", postCode, DocAddress.PostCode);
		}

		public void TestState()
		{
			ZString state = "Test State";

			BizAddress.OA_State = state;
			AssertEquals("Wrapped Value", state, DocAddress.State);
		}

		public void TestUsageComment()
		{
			ZString usageComment = "Test UsageComment";

			BizAddress.OA_Code = usageComment;
			AssertEquals("Wrapped Value", usageComment, DocAddress.UsageComment);
		}

		public void TestCountry()
		{
			BizAddress.OA_RL_NKRelatedPortCode = "";
			AssertNull("Wrapped Value when Loco NK not set for OrgAddress", DocAddress.Country);

			BizAddress.OA_RL_NKRelatedPortCode = "GBLON";
			AssertNotNull("Related country not null when NK set", BizAddress.RelatedCountry);
			AssertNotNull("Wrapped Value not null when NK set", DocAddress.Country);
			AssertEquals("Wrapped Value returned when Address Related Port set", Core.Constants.CountryCodes.UnitedKingdom, DocAddress.Country.Code);
		}

		public void TestPostalAddress()
		{
			AssertEquals("Postal Address returned by PostalAddressFormatter", ZString.Empty, DocAddress.PostalAddress);

			BizAddress.OA_Address1 = "Address 1";
			BizAddress.OA_Address2 = "Address 2";
			BizOrg.OH_FullName = "Name";
			AssertEquals("Postal Address returned by PostalAddressFormatter", "NAME\nADDRESS 1\nADDRESS 2", DocAddress.PostalAddress);
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
				AssertEquals("Postal Address In English returned by PostalAddressFormatter", "***ADDRESS NOT ON FILE***", DocAddress.PostalAddressInEnglish);

				BizAddress.OA_Address1 = "Address 1";
				BizAddress.OA_Address2 = "Address 2";
				BizAddress.OA_RN_NKCountryCode = "DE";
				BizOrg.OH_FullName = "Name";
				AssertEquals("Postal Address In English returned by PostalAddressFormatter", "NAME\nADDRESS 1\nADDRESS 2\nGERMANY", DocAddress.PostalAddressInEnglish);
			}
		}

		public void TestPostalAddressExcludeName()
		{
			var originalAllowMixedCase = Env.Registry.OrgAllowMixedCase;
			try
			{
				Env.Registry.SetOrgAllowMixedCase(true);
				BizAddress.OA_Address1 = "Address 1";
				BizAddress.OA_Address2 = "Address 2";
				BizOrg.OH_FullName = "Name";
				AssertEquals("Postal Address with Name", "Address 1\nAddress 2", DocAddress.PostalAddressExcludeName);

				BizOrg.OH_FullName = "Name with two  spaces";
				AssertEquals("Name with two spaces", "Address 1\nAddress 2", DocAddress.PostalAddressExcludeName);

				BizOrg.OH_FullName = " Name with leading space";
				AssertEquals("Name with leading space", "Address 1\nAddress 2", DocAddress.PostalAddressExcludeName);

				BizOrg.OH_FullName = "Name with trailing space ";
				AssertEquals("Name with trailing space", "Address 1\nAddress 2", DocAddress.PostalAddressExcludeName);

				Env.Registry.SetOrgAllowMixedCase(false);
				AssertEquals("Postal Address without Name", "ADDRESS 1\nADDRESS 2", DocAddress.PostalAddressExcludeName);
			}
			finally
			{
				Env.Registry.SetOrgAllowMixedCase(originalAllowMixedCase);
			}
		}

		#region ZDateTime fields

		public void TestPickupFromTimeOnly()
		{
			ZDateTime from = ZDateTime.Today;
			BizAddress.OA_PickupFromTimeOnly = from;
			AssertEquals("PickupFromTimeOnly", from, DocAddress.PickupFromTimeOnly);
		}

		public void TestPickupToTimeOnly()
		{
			ZDateTime to = ZDateTime.Today;
			BizAddress.OA_PickupToTimeOnly = to;
			AssertEquals("PickupToTimeOnly", to, DocAddress.PickupToTimeOnly);
		}

		public void TestDeliverFromTimeOnly()
		{
			ZDateTime from = ZDateTime.Today;
			BizAddress.OA_DeliverFromTimeOnly = from;
			AssertEquals("DeliverFromTimeOnly", from, DocAddress.DeliverFromTimeOnly);
		}

		public void TestDeliverToTimeOnly()
		{
			ZDateTime to = ZDateTime.Today;
			BizAddress.OA_DeliverToTimeOnly = to;
			AssertEquals("DeliverToTimeOnly", to, DocAddress.DeliverToTimeOnly);
		}

		public void TestDoNotAttendFrom()
		{
			ZDateTime from = ZDateTime.Today;
			BizAddress.OA_DoNotAttendFrom = from;
			AssertEquals("DoNotAttendFrom", from, DocAddress.DoNotAttendFrom);
		}

		public void TestDoNotAttendTo()
		{
			ZDateTime to = ZDateTime.Today;
			BizAddress.OA_DoNotAttendTo = to;
			AssertEquals("DoNotAttendTo", to, DocAddress.DoNotAttendTo);
		}

		public void TestDoNotAttendFromForCartageAdvice()
		{
			var address = Factory.New<OrgAddress>();
			ZDateTime from = ZDateTime.Today;
			address.OA_DoNotAttendFrom = from;
			DocAddress addressWrapper = DocAddress.New(address, Factory);
			AssertEquals("DoNotAttendFromForCartageAdvice", ZDateTime.Empty, addressWrapper.DoNotAttendFromForCartageAdvice);

			var org = Factory.New<OrgHeader>();
			org.Addresses.Add(address);
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
			var address = Factory.New<OrgAddress>();
			ZDateTime to = ZDateTime.Today;
			address.OA_DoNotAttendTo = to;
			DocAddress addressWrapper = DocAddress.New(address, Factory);
			AssertEquals("DoNotAttendToForCartageAdvice", ZDateTime.Empty, addressWrapper.DoNotAttendToForCartageAdvice);

			var org = Factory.New<OrgHeader>();
			org.Addresses.Add(address);
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

		#endregion

		public void TestToString()
		{
			AssertEquals("ToString is PostalAddress", DocAddress.PostalAddress, DocAddress.ToString());
		}

		#region Contacts

		public void TestGetContact()
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

			DocAddress addressWrapper = DocAddress.New(addy, Factory);
			AssertEquals("Contact should be ImportSeaDepot Contact", "ImportSeaDepot", addressWrapper.GetContact(ContactType.ImportSeaDepot).ContactName);

			addressWrapper = DocAddress.New(addy, Factory);
			AssertEquals("Contact should be LocalTransport Contact", "LocalTransport", addressWrapper.GetContact(ContactType.LocalTransport).ContactName);
		}

		public void TestGetContactShouldInitializeAddressForNonExistingContact()
		{
			OrgHeader orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "UNITTEST";

			OrgAddress mainAddress = orgHeader.MainAddress;
			mainAddress.OA_Address1 = "Main Address 1";
			mainAddress.OA_Phone = "Main Phone";
			mainAddress.OA_Fax = "Main Fax";

			OrgAddress otherAddress = orgHeader.Addresses.AddNew();
			otherAddress.OA_OH = orgHeader.PK;
			otherAddress.OA_Address1 = "Other Address 1";
			otherAddress.OA_Phone = "Other Phone";
			otherAddress.OA_Fax = "Other Fax";
			otherAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);

			Factory.Save();

			DocAddress wrapper = DocAddress.New(otherAddress, Factory);

			AssertEquals("Other Phone", wrapper.GetContact(ContactType.LocalTransport).Phone);
			AssertEquals("Other Fax", wrapper.GetContact(ContactType.LocalTransport).Fax);
		}

		#endregion

		#region Customs Code

		#region LocalControlledPremisesID

		public void TestLocalControlledPremisesID()
		{
			OrgCusCode cCP = BizOrg.CustomsCodes.AddNew();
			cCP.OK_CustomsRegNo = "123";
			cCP.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			cCP.OK_OA_PremisesAddress = BizAddress.PK;

			AssertEquals("123", DocAddress.LocalControlledPremisesID);
		}

		public void TestWarehouseLocalControlledPremisesID()
		{
			OrgCusCode cCP = BizOrg.CustomsCodes.AddNew();
			cCP.OK_CustomsRegNo = "123";
			cCP.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			cCP.OK_OA_PremisesAddress = BizAddress.PK;

			AssertEquals("123", DocAddress.WarehouseLocalControlledPremisesID);
		}

		public void TestDepotLocalControlledPremisesID()
		{
			OrgCusCode cCP = BizOrg.CustomsCodes.AddNew();
			cCP.OK_CustomsRegNo = "123";
			cCP.OK_CodeType = OrgCusCode.CodeTypes.DepotControlledPremisesID;
			cCP.OK_OA_PremisesAddress = BizAddress.PK;

			AssertEquals("123", DocAddress.DepotLocalControlledPremisesID);
		}

		#endregion

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
			ZString[] spiltFormattedAddress = DocAddress.WrapTextForAColumn(unformattedPostalAddress, 30).Split('\n');
			ZString expectedSpiltAddress1 = spiltFormattedAddress[0];
			ZString expectedSpiltAddress2 = DocAddress.WrapTextForAColumn(spiltFormattedAddress[1], 25).Split('\n')[0];

			AssertEquals("SpiltAddress1", expectedSpiltAddress1, DocAddress.SpiltAddress1);
			AssertEquals("SpiltAddress2", expectedSpiltAddress2, DocAddress.SpiltAddress2);
		}

		public void TestSplitAddressWithLengthLessThan25()
		{
			BizAddress.OA_Address1 = "12 Address one";
			BizAddress.OA_City = "City";

			ZString unformattedPostalAddress = DocAddress.PostalAddress.Replace("\n", " ");
			ZString[] spiltFormattedAddress = DocAddress.WrapTextForAColumn(unformattedPostalAddress, 30).Split('\n');
			ZString expectedSpiltAddress1 = spiltFormattedAddress[0];
			ZString expectedSpiltAddress2 = ZString.Empty;

			AssertEquals("SpiltAddress1", expectedSpiltAddress1, DocAddress.SpiltAddress1);
			AssertEquals("SpiltAddress2", expectedSpiltAddress2, DocAddress.SpiltAddress2);
		}

		#endregion

		#region Warehousing Facilities and Loading/Unloading Constraints

		public void TestHasWareHousing()
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

		public void TestHasLoadingUnloadingConstraints()
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

		public void TestHasDockLeveller()
		{
			BizAddress.OA_DockLeveler = ZBool.False;
			AssertEquals("HasDockLeveller is ", ZBool.False, DocAddress.HasDockLeveller);

			BizAddress.OA_DockLeveler = ZBool.True;
			AssertEquals("HasDockLeveller is ", ZBool.True, DocAddress.HasDockLeveller);
		}

		public void TestHasPalletJack()
		{
			BizAddress.OA_PalletJack = ZBool.False;
			AssertEquals("HasPalletJack is ", ZBool.False, DocAddress.HasPalletJack);

			BizAddress.OA_PalletJack = ZBool.True;
			AssertEquals("HasPalletJack is ", ZBool.True, DocAddress.HasPalletJack);
		}

		public void TestHasForkLift()
		{
			BizAddress.OA_ForkLift = ZBool.False;
			AssertEquals("HasForkLift is ", ZBool.False, DocAddress.HasForkLift);

			BizAddress.OA_ForkLift = ZBool.True;
			AssertEquals("HasForkLift is ", ZBool.True, DocAddress.HasForkLift);
		}

		public void TestOtherWarehouseFacilities()
		{
			BizAddress.OA_OtherWarehouseFacilities = "Other warehouse facilities";
			AssertEquals("OtherWarehouseFacilities is ", "Other warehouse facilities", DocAddress.OtherWarehouseFacilities);
		}

		public void TestAccessPoint()
		{
			BizAddress.OA_AccessPoint = BizAddress.OA_AccessPoint_List[0].Code;
			AssertEquals("AccessPoint is ", BizAddress.OA_AccessPoint_List.GetDescriptionFromCode(BizAddress.OA_AccessPoint), DocAddress.AccessPoint);
		}

		public void TestContainerHandling()
		{
			BizAddress.OA_ContainerHandling = BizAddress.OA_ContainerHandling_List[0].Code;
			AssertEquals("ContainerHandling is ", BizAddress.OA_ContainerHandling_List.GetDescriptionFromCode(BizAddress.OA_ContainerHandling), DocAddress.ContainerHandling);
		}

		public void TestCommunicationRequired()
		{
			BizAddress.OA_CommunicationRequired = BizAddress.OA_CommunicationRequired_List[0].Code;
			AssertEquals("CommunicationRequired is ", BizAddress.OA_CommunicationRequired_List.GetDescriptionFromCode(BizAddress.OA_CommunicationRequired), DocAddress.CommunicationRequired);
		}

		public void TestLabourRequired()
		{
			BizAddress.OA_LabourRequired = BizAddress.OA_LabourRequired_List[0].Code;
			AssertEquals("LabourRequired is ", BizAddress.OA_LabourRequired_List.GetDescriptionFromCode(BizAddress.OA_LabourRequired), DocAddress.LabourRequired);
		}

		public void TestDockHeight()
		{
			BizAddress.OA_Dock_Height = BizAddress.OA_Dock_Height_List[0].Code;
			AssertEquals("DockHeight is ", BizAddress.OA_Dock_Height_List.GetDescriptionFromCode(BizAddress.OA_Dock_Height), DocAddress.DockHeight);
		}

		public void TestFurtherConstraints()
		{
			BizAddress.OA_LoadingUnloadingConstraints = "Further constraints";
			AssertEquals("DockHeight is ", "Further constraints", DocAddress.FurtherConstraints);
		}
		#endregion

		#region Implementation

		protected override void SetUp()
		{
			BizOrg = Factory.New<OrgHeader>();
			BizOrg.OH_Code = "BizOrg";
			BizAddress = BizOrg.MainAddress;
			BizAddress.OA_RN_NKCountryCode = ZString.Empty;
			DocAddress = DocAddress.New(BizAddress, Factory);
			AssertNotNull("PreCondition: Valid DocAddress", DocAddress);

			base.SetUp();
		}

		DocAddress DocAddress;
		OrgAddress BizAddress;
		OrgHeader BizOrg;

		#endregion
	}
}
