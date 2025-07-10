using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(AddressWrapper))]
	public class AddressWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = new AddressWrapper(null, ContactType.All, Factory);
			AssertEquals("wrapperEmpty.Country.Code", ZString.Empty, wrapperEmpty.Country.Code);
			AssertEquals("wrapperEmpty.Location.UNLOCO", ZString.Empty, wrapperEmpty.Location.UNLOCO);
			AssertEquals("wrapperEmpty.CompanyCode", ZString.Empty, wrapperEmpty.CompanyCode);
			AssertEquals("wrapperEmpty.CompanyName", ZString.Empty, wrapperEmpty.CompanyName);
			AssertEquals("wrapperEmpty.CompanyNameAndAddress", ZString.Empty, wrapperEmpty.CompanyNameAndAddress);
			AssertEquals("wrapperEmpty.AddressCaption.", ZString.Empty, wrapperEmpty.AddressCaption);
			AssertEquals("wrapperEmpty.Address", ZString.Empty, wrapperEmpty.Address);
			AssertEquals("wrapperEmpty.DeliveryRoute", ZString.Empty, wrapperEmpty.DeliveryRoute);
			AssertEquals("wrapperEmpty.DeliveryRouteSequence", ZShort.Zero, wrapperEmpty.DeliveryRouteSequence);
			AssertEquals("wrapperEmpty.AddressLine1", ZString.Empty, wrapperEmpty.AddressLine1);
			AssertEquals("wrapperEmpty.AddressLine2", ZString.Empty, wrapperEmpty.AddressLine2);
			AssertEquals("wrapperEmpty.City", ZString.Empty, wrapperEmpty.City);
			AssertEquals("wrapperEmpty.ShortCode", ZString.Empty, wrapperEmpty.ShortCode);
			AssertEquals("wrapperEmpty.State", ZString.Empty, wrapperEmpty.State);
			AssertEquals("wrapperEmpty.PostCode", ZString.Empty, wrapperEmpty.PostCode);
			AssertEquals("wrapperEmpty.Phone", ZString.Empty, wrapperEmpty.Phone);
			AssertEquals("wrapperEmpty.Fax", ZString.Empty, wrapperEmpty.Fax);
			AssertEquals("wrapperEmpty.Mobile", ZString.Empty, wrapperEmpty.Mobile);
			AssertEquals("wrapperEmpty.Email", ZString.Empty, wrapperEmpty.Email);
			AssertEquals("wrapperEmpty.ContactName", ZString.Empty, wrapperEmpty.ContactName);
			AssertEquals("wrapperEmpty.FurtherConstraints", ZString.Empty, wrapperEmpty.FurtherConstraints);
			AssertEquals("wrapperEmpty.OtherWarehouseFacilities", ZString.Empty, wrapperEmpty.OtherWarehouseFacilities);
			AssertEquals("wrapperEmpty.HasDockLeveller", ZBool.False, wrapperEmpty.HasDockLeveller);
			AssertEquals("wrapperEmpty.HasPalletJack", ZBool.False, wrapperEmpty.HasPalletJack);
			AssertEquals("wrapperEmpty.HasForkLift", ZBool.False, wrapperEmpty.HasForkLift);
			AssertEquals("wrapperEmpty.HasWareHousing", ZBool.False, wrapperEmpty.HasWarehousing);
			AssertEquals("wrapperEmpty.HasLoadingUnloadingConstraints", ZBool.False, wrapperEmpty.HasLoadingUnloadingConstraints);
			AssertEquals("wrapperEmpty.AccessPoint.Code", ZString.Empty, wrapperEmpty.AccessPoint.Code);
			AssertEquals("wrapperEmpty.CommunicationRequired.Code", ZString.Empty, wrapperEmpty.CommunicationRequired.Code);
			AssertEquals("wrapperEmpty.ContainerHandling.Code", ZString.Empty, wrapperEmpty.ContainerHandling.Code);
			AssertEquals("wrapperEmpty.DockHeight.Code", ZString.Empty, wrapperEmpty.DockHeight.Code);
			AssertEquals("wrapperEmpty.LabourRequired.Code", ZString.Empty, wrapperEmpty.LabourRequired.Code);
			AssertEquals("wrapperEmpty.AddressPK", ZGuid.Empty, wrapperEmpty.AddressPK);
			Assert(wrapperEmpty.Organization != null);

			wrapperEmpty = new AddressWrapper((JobDocAddress)null, Factory);
			AssertEquals("wrapperEmpty.Country.Code", ZString.Empty, wrapperEmpty.Country.Code);
			AssertEquals("wrapperEmpty.Location.UNLOCO", ZString.Empty, wrapperEmpty.Location.UNLOCO);
			AssertEquals("wrapperEmpty.CompanyName", ZString.Empty, wrapperEmpty.CompanyName);
			AssertEquals("wrapperEmpty.CompanyNameAndAddress", ZString.Empty, wrapperEmpty.CompanyNameAndAddress);
			AssertEquals("wrapperEmpty.AddressCaption.", ZString.Empty, wrapperEmpty.AddressCaption);
			AssertEquals("wrapperEmpty.Address", ZString.Empty, wrapperEmpty.Address);
			AssertEquals("wrapperEmpty.Phone", ZString.Empty, wrapperEmpty.Phone);
			AssertEquals("wrapperEmpty.ShortCode", ZString.Empty, wrapperEmpty.ShortCode);
			AssertEquals("wrapperEmpty.Fax", ZString.Empty, wrapperEmpty.Fax);
			AssertEquals("wrapperEmpty.Mobile", ZString.Empty, wrapperEmpty.Mobile);
			AssertEquals("wrapperEmpty.Email", ZString.Empty, wrapperEmpty.Email);
			AssertEquals("wrapperEmpty.ContactName", ZString.Empty, wrapperEmpty.ContactName);
			AssertEquals("wrapperEmpty.FurtherConstraints", ZString.Empty, wrapperEmpty.FurtherConstraints);
			AssertEquals("wrapperEmpty.OtherWarehouseFacilities", ZString.Empty, wrapperEmpty.OtherWarehouseFacilities);

			AssertEquals("wrapperEmpty.AccessPoint.Code", ZString.Empty, wrapperEmpty.AccessPoint.Code);
			AssertEquals("wrapperEmpty.CommunicationRequired.Code", ZString.Empty, wrapperEmpty.CommunicationRequired.Code);
			AssertEquals("wrapperEmpty.ContainerHandling.Code", ZString.Empty, wrapperEmpty.ContainerHandling.Code);
			AssertEquals("wrapperEmpty.DockHeight.Code", ZString.Empty, wrapperEmpty.DockHeight.Code);
			AssertEquals("wrapperEmpty.LabourRequired.Code", ZString.Empty, wrapperEmpty.LabourRequired.Code);
			Assert(wrapperEmpty.Organization != null);
		}

		public void TestWrapperMappingFromOrgAddress()
		{
			var organisation = OrgTestHelper.GetNewOrganisation("CLNTEATDOG", "CLINTY EATS DOGS",
				"'IMPRESSIVE TOWER' BUILDING", "344 RED CRAYON ROAD", "AU", "MANLYVALE", "2354", "VIC", "AUMEL",
				"PHONE", "FAX", "email@domain.com", "MOBILE", "Sh1", Factory);
			var address = organisation.MainAddress;
			SetupAdditionalAddressProperties(address);
			var contact = OrgTestHelper.AddNewContact(organisation, ContactType.All, "BEN");
			address.OA_DeliveryRoute = "TST";
			address.OA_DeliveryRouteSequence = 2;

			var wrapperFull = new AddressWrapper(address, ContactType.All, Factory);
			AssertEquals("wrapperFull.Country.Code", "AU", wrapperFull.Country.Code);
			AssertEquals("wrapperFull.Location.UNLOCO", "AUMEL", wrapperFull.Location.UNLOCO);
			AssertEquals("wrapperFull.CompanyCode", "CLNTEATDOG", wrapperFull.CompanyCode);
			AssertEquals("wrapperFull.CompanyName", "CLINTY EATS DOGS", wrapperFull.CompanyName);
			AssertEquals("wrapperFull.CompanyNameAndAddress", "CLINTY EATS DOGS\n'IMPRESSIVE TOWER' BUILDING\n344 RED CRAYON ROAD\nMANLYVALE VIC 2354\nAUSTRALIA", wrapperFull.CompanyNameAndAddress);
			AssertEquals("wrapperFull.Address", "'IMPRESSIVE TOWER' BUILDING\n344 RED CRAYON ROAD\nMANLYVALE VIC 2354\nAUSTRALIA", wrapperFull.Address);
			AssertEquals("wrapperFull.AddressCaption", "", wrapperFull.AddressCaption);
			AssertEquals("wrapperFull.AddressLine1", "'IMPRESSIVE TOWER' BUILDING", wrapperFull.AddressLine1);
			AssertEquals("wrapperFull.AddressLine2", "344 RED CRAYON ROAD", wrapperFull.AddressLine2);
			AssertEquals("wrapperFull.City", "MANLYVALE", wrapperFull.City);
			AssertEquals("wrapperFull.ShortCode", "SH1", wrapperFull.ShortCode);
			AssertEquals("wrapperFull.State", "VIC", wrapperFull.State);
			AssertEquals("wrapperFull.PostCode", "2354", wrapperFull.PostCode);
			AssertEquals("wrapperFull.Phone", "PHONE", wrapperFull.Phone);
			AssertEquals("wrapperFull.Fax", "FAX", wrapperFull.Fax);
			AssertEquals("wrapperFull.Mobile", "MOBILE", wrapperFull.Mobile);
			AssertEquals("wrapperFull.Email", "email@domain.com", wrapperFull.Email);
			AssertEquals("wrapperFull.ContactName", "BEN_NAME", wrapperFull.ContactName);
			AssertEquals("wrapperFull.FurtherConstraints", "Further Constraints", wrapperFull.FurtherConstraints);
			AssertEquals("wrapperFull.OtherWarehouseFacilities", "Other Warehouse Facilities", wrapperFull.OtherWarehouseFacilities);
			AssertEquals("wrapperFull.AddressPK", address.PK, wrapperFull.AddressPK);
			AssertEquals("wrapperEmpty.HasDockLeveller", ZBool.True, wrapperFull.HasDockLeveller);
			AssertEquals("wrapperEmpty.HasPalletJack", ZBool.True, wrapperFull.HasPalletJack);
			AssertEquals("wrapperEmpty.HasForkLift", ZBool.True, wrapperFull.HasForkLift);
			AssertEquals("wrapperEmpty.HasWareHousing", ZBool.True, wrapperFull.HasWarehousing);
			AssertEquals("wrapperEmpty.HasLoadingUnloadingConstraints", ZBool.True, wrapperFull.HasLoadingUnloadingConstraints);
			AssertEquals("wrapperFull.DeliveryRoute", "TST", wrapperFull.DeliveryRoute);
			AssertEquals("wrapperFull.OA_DeliveryRouteSequence", (short)2, wrapperFull.DeliveryRouteSequence);

			AssertEquals("wrapperFull.AccessPoint.Code", OrgConstants.AccessPoint.Code.Dock, wrapperFull.AccessPoint.Code);
			AssertEquals("wrapperFull.CommunicationRequired.Code", OrgConstants.CommunicationRequired.Code.Appointment, wrapperFull.CommunicationRequired.Code);
			AssertEquals("wrapperFull.ContainerHandling.Code", OrgConstants.ContainerHandling.Code.Ask, wrapperFull.ContainerHandling.Code);
			AssertEquals("wrapperFull.DockHeight.Code", OrgConstants.DockHeight.Code.NonStandard, wrapperFull.DockHeight.Code);
			AssertEquals("wrapperFull.LabourRequired.Code", OrgConstants.LabourRequired.Code.Ask, wrapperFull.LabourRequired.Code);
			Assert(wrapperFull.Organization != null);
		}

		public void TestWrapperMappingFromOrgAddress_UseTranslatedAddressInSpecificLanguage()
		{
			var organisation = OrgTestHelper.GetNewOrganisation("CLNTEATDOG", "WiseTechGlobal",
				"5th road", "Mascot", "AU", "Sydney", "2354", "VIC", "AUSYD",
				"PHONE", "FAX", "email@domain.com", "MOBILE", "Sh1", Factory);
			var address = organisation.MainAddress;

			var translatedAddress = address.TranslatedAddresses.AddNew();
			translatedAddress.OTA_Language = Core.Constants.Languages.ChineseSimplified;
			translatedAddress.OTA_CompanyName = "慧咨全球";
			translatedAddress.OTA_Address1 = "第五大道";
			translatedAddress.OTA_Address2 = "麦斯考特";
			translatedAddress.OTA_City = "悉尼";
			translatedAddress.OTA_State = "新南威尔士";
			translatedAddress.OTA_PostCode = "2100";

			using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.ChineseSimplified))
			{
				var wrapperFull = new AddressWrapper(address, ContactType.All, Factory);
				AssertEquals("慧咨全球", wrapperFull.CompanyName);
				AssertEquals("第五大道", wrapperFull.AddressLine1);
				AssertEquals("麦斯考特", wrapperFull.AddressLine2);
				AssertEquals("悉尼", wrapperFull.City);
				AssertEquals("新南威尔士", wrapperFull.State);
				AssertEquals("2100", wrapperFull.PostCode);
				AssertEquals("第五大道\n麦斯考特\n悉尼 新南威尔士 2100\n澳大利亚", wrapperFull.Address);
			}
		}

		public void TestWrapperMappingFromOrgAddress_SwitchAddressToDisplayWhenLanguageChanged()
		{
			var organisation = OrgTestHelper.GetNewOrganisation("CLNTEATDOG", "WiseTechGlobal",
				"5th road", "Mascot", "AU", "Sydney", "2354", "VIC", "AUSYD",
				"PHONE", "FAX", "email@domain.com", "MOBILE", "Sh1", Factory);
			var address = organisation.MainAddress;

			var translatedAddress = address.TranslatedAddresses.AddNew();
			translatedAddress.OTA_Language = Core.Constants.Languages.ChineseSimplified;
			translatedAddress.OTA_CompanyName = "慧咨全球";
			translatedAddress.OTA_Address1 = "第五大道";
			translatedAddress.OTA_Address2 = "麦斯考特";
			translatedAddress.OTA_City = "悉尼";
			translatedAddress.OTA_State = "新南威尔士";
			translatedAddress.OTA_PostCode = "2100";

			var wrapperFull = new AddressWrapper(address, ContactType.All, Factory);

			CombineAssertions(() =>
			{
				AssertEquals("WISETECHGLOBAL", wrapperFull.CompanyName);
				AssertEquals("WISETECHGLOBAL\n5TH ROAD\nMASCOT\nSYDNEY VIC 2354\nAUSTRALIA", wrapperFull.CompanyNameAndAddress);
				AssertEquals("5TH ROAD", wrapperFull.AddressLine1);
				AssertEquals("MASCOT", wrapperFull.AddressLine2);
				AssertEquals("SYDNEY", wrapperFull.City);
				AssertEquals("VIC", wrapperFull.State);
				AssertEquals("2354", wrapperFull.PostCode);
				AssertEquals("5TH ROAD\nMASCOT\nSYDNEY VIC 2354\nAUSTRALIA", wrapperFull.Address);
			});

			wrapperFull = new AddressWrapper(address, ContactType.All, Factory);

			using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.ChineseSimplified))
			{
				CombineAssertions(() =>
				{
					AssertEquals("慧咨全球", wrapperFull.CompanyName);
					AssertEquals("慧咨全球\n第五大道\n麦斯考特\n悉尼 新南威尔士 2100\n澳大利亚", wrapperFull.CompanyNameAndAddress);
					AssertEquals("第五大道\n麦斯考特\n悉尼 新南威尔士 2100\n澳大利亚", wrapperFull.Address);
				});
			}

			CombineAssertions(() =>
			{
				AssertValueChangedByLanguage("5TH ROAD", "第五大道", () => wrapperFull.AddressLine1);
				AssertValueChangedByLanguage("MASCOT", "麦斯考特", () => wrapperFull.AddressLine2);
				AssertValueChangedByLanguage("SYDNEY", "悉尼", () => wrapperFull.City);
				AssertValueChangedByLanguage("VIC", "新南威尔士", () => wrapperFull.State);
				AssertValueChangedByLanguage("2354", "2100", () => wrapperFull.PostCode);
			});
		}

		public void TestWrapperMappingFromJobDocAddress()
		{
			var organisation = OrgTestHelper.GetNewOrganisation("CLNTEATDOG", "CLINTY EATS DOGS",
				"'IMPRESSIVE TOWER' BUILDING", "344 RED CRAYON ROAD", "AU", "MANLYVALE", "2354", "VIC", "AUMEL",
				"PHONE", "FAX", "email@domain.com", "MOBILE", "Sh1", Factory);
			var address = organisation.MainAddress;
			SetupAdditionalAddressProperties(address);
			var contact = OrgTestHelper.AddNewContact(organisation, ContactType.All, "BEN");
			address.OA_DeliveryRoute = "TST";
			address.OA_DeliveryRouteSequence = 2;

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.DefaultContactType = ContactType.All;
			docAddress.E2_OA_Address = address.PK;

			var wrapperFull = new AddressWrapper(address, ContactType.All, Factory);
			AssertEquals("wrapperFull.Country.Code", "AU", wrapperFull.Country.Code);
			AssertEquals("wrapperFull.Location.UNLOCO", "AUMEL", wrapperFull.Location.UNLOCO);
			AssertEquals("wrapperFull.CompanyCode", "CLNTEATDOG", wrapperFull.CompanyCode);
			AssertEquals("wrapperFull.CompanyName", "CLINTY EATS DOGS", wrapperFull.CompanyName);
			AssertEquals("wrapperFull.CompanyNameAndAddress", "CLINTY EATS DOGS\n'IMPRESSIVE TOWER' BUILDING\n344 RED CRAYON ROAD\nMANLYVALE VIC 2354\nAUSTRALIA", wrapperFull.CompanyNameAndAddress);
			AssertEquals("wrapperFull.Address", "'IMPRESSIVE TOWER' BUILDING\n344 RED CRAYON ROAD\nMANLYVALE VIC 2354\nAUSTRALIA", wrapperFull.Address);
			AssertEquals("wrapperFull.AddressCaption", "", wrapperFull.AddressCaption);
			AssertEquals("wrapperFull.AddressLine1", "'IMPRESSIVE TOWER' BUILDING", wrapperFull.AddressLine1);
			AssertEquals("wrapperFull.AddressLine2", "344 RED CRAYON ROAD", wrapperFull.AddressLine2);
			AssertEquals("wrapperFull.City", "MANLYVALE", wrapperFull.City);
			AssertEquals("wrapperFull.ShortCode", "SH1", wrapperFull.ShortCode);
			AssertEquals("wrapperFull.State", "VIC", wrapperFull.State);
			AssertEquals("wrapperFull.PostCode", "2354", wrapperFull.PostCode);
			AssertEquals("wrapperFull.Phone", "PHONE", wrapperFull.Phone);
			AssertEquals("wrapperFull.Fax", "FAX", wrapperFull.Fax);
			AssertEquals("wrapperFull.Mobile", "MOBILE", wrapperFull.Mobile);
			AssertEquals("wrapperFull.Email", "email@domain.com", wrapperFull.Email);
			AssertEquals("wrapperFull.ContactName", "BEN_NAME", wrapperFull.ContactName);
			AssertEquals("wrapperFull.FurtherConstraints", "Further Constraints", wrapperFull.FurtherConstraints);
			AssertEquals("wrapperFull.OtherWarehouseFacilities", "Other Warehouse Facilities", wrapperFull.OtherWarehouseFacilities);
			AssertEquals("wrapperFull.DeliveryRoute", "TST", wrapperFull.DeliveryRoute);
			AssertEquals("wrapperFull.DeliveryRouteSequence", (short)2, wrapperFull.DeliveryRouteSequence);

			AssertEquals("wrapperFull.AccessPoint.Code", OrgConstants.AccessPoint.Code.Dock, wrapperFull.AccessPoint.Code);
			AssertEquals("wrapperFull.CommunicationRequired.Code", OrgConstants.CommunicationRequired.Code.Appointment, wrapperFull.CommunicationRequired.Code);
			AssertEquals("wrapperFull.ContainerHandling.Code", OrgConstants.ContainerHandling.Code.Ask, wrapperFull.ContainerHandling.Code);
			AssertEquals("wrapperFull.DockHeight.Code", OrgConstants.DockHeight.Code.NonStandard, wrapperFull.DockHeight.Code);
			AssertEquals("wrapperFull.LabourRequired.Code", OrgConstants.LabourRequired.Code.Ask, wrapperFull.LabourRequired.Code);
			Assert(wrapperFull.Organization != null);

			docAddress.E2_AddressOverride = true;
			docAddress.E2_CompanyName = "ROTTEN DOGS AND HATSTANDS";
			docAddress.E2_Address1 = "UNMAIN ADDRESS 1";
			docAddress.E2_Address2 = "UNMAIN ADDRESS 2";
			docAddress.E2_City = "ALEXANDRIA";
			docAddress.E2_State = "NSW";
			docAddress.E2_RN_NKCountryCode = "ZA";
			docAddress.E2_Postcode = "2015";
			docAddress.E2_Contact = "OV_CONTACT";
			docAddress.E2_Phone = "OV_PHONE";
			docAddress.E2_Fax = "OV_FAX";
			docAddress.E2_Email = "OV_EMAIL";
			docAddress.E2_Mobile = "OV_MOBILE";

			wrapperFull = new AddressWrapper(docAddress, Factory);
			AssertEquals("wrapperFull.Country.Code", "ZA", wrapperFull.Country.Code);
			AssertEquals("wrapperFull.Location.UNLOCO", "AUSYD", wrapperFull.Location.UNLOCO);
			AssertEquals("wrapperFull.CompanyCode", "MISC", wrapperFull.CompanyCode);
			AssertEquals("wrapperFull.CompanyName", "ROTTEN DOGS AND HATSTANDS", wrapperFull.CompanyName);
			AssertMultilineASCIIEquals("wrapperFull.CompanyNameAndAddress", "ROTTEN DOGS AND HATSTANDS\nUNMAIN ADDRESS 1\nUNMAIN ADDRESS 2\nALEXANDRIA\n2015\nSOUTH AFRICA", wrapperFull.CompanyNameAndAddress);
			AssertMultilineASCIIEquals("wrapperFull.Address", "UNMAIN ADDRESS 1\nUNMAIN ADDRESS 2\nALEXANDRIA\n2015\nSOUTH AFRICA", wrapperFull.Address);
			AssertEquals("wrapperFull.AddressCaption", "", wrapperFull.AddressCaption);
			AssertEquals("wrapperFull.AddressLine1", "UNMAIN ADDRESS 1", wrapperFull.AddressLine1);
			AssertEquals("wrapperFull.AddressLine2", "UNMAIN ADDRESS 2", wrapperFull.AddressLine2);
			AssertEquals("wrapperFull.City", "ALEXANDRIA", wrapperFull.City);
			AssertEquals("wrapperFull.State", "NSW", wrapperFull.State);
			AssertEquals("wrapperFull.PostCode", "2015", wrapperFull.PostCode);
			AssertEquals("wrapperFull.Phone", "OV_PHONE", wrapperFull.Phone);
			AssertEquals("wrapperFull.Fax", "OV_FAX", wrapperFull.Fax);
			AssertEquals("wrapperFull.Mobile", "OV_MOBILE", wrapperFull.Mobile);
			AssertEquals("wrapperFull.Email", "OV_EMAIL", wrapperFull.Email);
			AssertEquals("wrapperFull.ContactName", "OV_CONTACT", wrapperFull.ContactName);
			AssertEquals("wrapperFull.FurtherConstraints", ZString.Empty, wrapperFull.FurtherConstraints);
			AssertEquals("wrapperFull.OtherWarehouseFacilities", ZString.Empty, wrapperFull.OtherWarehouseFacilities);

			AssertEquals("wrapperFull.AccessPoint.Code", ZString.Empty, wrapperFull.AccessPoint.Code);
			AssertEquals("wrapperFull.CommunicationRequired.Code", ZString.Empty, wrapperFull.CommunicationRequired.Code);
			AssertEquals("wrapperFull.ContainerHandling.Code", ZString.Empty, wrapperFull.ContainerHandling.Code);
			AssertEquals("wrapperFull.DockHeight.Code", ZString.Empty, wrapperFull.DockHeight.Code);
			AssertEquals("wrapperFull.LabourRequired.Code", ZString.Empty, wrapperFull.LabourRequired.Code);
			AssertEquals("wrapperFull.DeliveryRoute", ZString.Empty, wrapperFull.DeliveryRoute);
			AssertEquals("wrapperFull.DeliveryRouteSequence", ZShort.Zero, wrapperFull.DeliveryRouteSequence);
			Assert(wrapperFull.Organization != null);
		}

		public void TestOrganizationWrapperWhenBuildAddressWrapperFromJobDocAddress()
		{
			var organisation = OrgTestHelper.GetNewOrganisation("CLNTEATDOG", "CLINTY EATS DOGS",
	"'IMPRESSIVE TOWER' BUILDING", "344 RED CRAYON ROAD", "AU", "MANLYVALE", "2354", "VIC", "AUMEL",
	"PHONE", "FAX", "email@domain.com", "MOBILE", "", Factory);
			var address = organisation.MainAddress;

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.DefaultContactType = ContactType.All;
			docAddress.E2_OA_Address = address.PK;
			docAddress.E2_AddressOverride = true;
			docAddress.E2_CompanyName = "ROTTEN DOGS AND HATSTANDS";
			docAddress.E2_Address1 = "ADDRESS 1";
			docAddress.E2_Address2 = "ADDRESS 2";
			docAddress.E2_City = "ALEXANDRIA";
			docAddress.E2_State = "NSW";
			docAddress.E2_RN_NKCountryCode = "ZA";
			docAddress.E2_Postcode = "123456";
			docAddress.E2_Contact = "OV_CONTACT";
			docAddress.E2_Phone = "OV_PHONE";
			docAddress.E2_Fax = "OV_FAX";
			docAddress.E2_Email = "OV_EMAIL";

			var addressWrapper = new AddressWrapper(docAddress, Factory);
			var organizationWrapper = addressWrapper.Organization;
			Assert(organizationWrapper != null);
			AssertEquals("organizationWrapper.TypeDescription", ZString.Empty, organizationWrapper.TypeDescription);
			AssertEquals("organizationWrapper.Location.UNLOCO", "AUSYD", organizationWrapper.ClosestPort.UNLOCO);
			AssertEquals("organizationWrapper.CompanyName", "ROTTEN DOGS AND HATSTANDS", organizationWrapper.CompanyName);
			AssertEquals("organizationWrapper.Phone", "OV_PHONE", organizationWrapper.MainPhone);
			AssertEquals("organizationWrapper.Fax", "OV_FAX", organizationWrapper.MainFax);
			AssertEquals("organizationWrapper.Email", "OV_EMAIL", organizationWrapper.MainEmail);
			AssertEquals("organizationWrapper.ContactName", "OV_CONTACT", organizationWrapper.ContactName);
			AssertEquals("organizationWrapper.ContactPhone", "OV_PHONE", organizationWrapper.ContactPhone);
			AssertEquals("organizationWrapper.ContactFax", "OV_FAX", organizationWrapper.ContactFax);
			AssertEquals("organizationWrapper.ContactEmail", "OV_EMAIL", organizationWrapper.ContactEmail);
			AssertEquals("organizationWrapper.Organisation.OH_Code", "MISC", organizationWrapper.Organisation.OH_Code);
			AssertEquals("organizationWrapper.Organisation.OH_FullName", "MISCELLANEOUS ORGANISATION (SYSTEM DEFINED)", organizationWrapper.Organisation.OH_FullName);
			AssertNotEquals("organizationWrapper.Organisation.PK", organisation.PK, organizationWrapper.Organisation.PK);
			AssertNotEquals("organizationWrapper.Organisation.PK", ZGuid.Empty, organizationWrapper.Organisation.PK);
		}

		public void TestOrganizationWrapperWhenBuildAddressWrapperFromOrgAddressAndContactType()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			testHeader.MainAddress.OA_Address1 = "MAIN ADDRESS";
			var secondAddress = testHeader.Addresses.AddNew();
			secondAddress.OA_Address1 = "SECOND ADDRESS";
			secondAddress.OA_RN_NKCountryCode = "AU";

			var addressWrapper = new AddressWrapper(secondAddress, ContactType.ShippingLine, Factory);
			var organizationWrapper = addressWrapper.Organization;
			Assert(organizationWrapper != null);
			AssertEquals(organizationWrapper.Organisation, testHeader);
			AssertEquals("SECOND ADDRESS\nAUSTRALIA", organizationWrapper.CompanyNameAndAddress);
		}

		public void TestOrganizationWrapperWhenBuildAddressWrapperFromCompanyNameAndAddress()
		{
			var addressWrapper = new AddressWrapper("NAME AND ADDRESS", Factory);
			var organizationWrapper = addressWrapper.Organization;
			Assert(organizationWrapper != null);
			AssertEquals("NAME AND ADDRESS", organizationWrapper.CompanyNameAndAddress);
		}

		public void TestWrapperMappingFromJobDocAddressContact()
		{
			var organisation = OrgTestHelper.GetNewOrganisation("CLNTEATDOG", "CLINTY EATS DOGS",
				"'IMPRESSIVE TOWER' BUILDING", "344 RED CRAYON ROAD", "AU", "MANLYVALE", "2354", "VIC", "AUMEL",
				"PHONE", "FAX", "email@domain.com", "MOBILE", "Sh1", Factory);
			var address = organisation.MainAddress;
			SetupAdditionalAddressProperties(address);
			var contact = OrgTestHelper.AddNewContact(organisation, ContactType.All, "BEN");

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.DefaultContactType = ContactType.All;
			docAddress.E2_OA_Address = address.PK;
			docAddress.E2_AddressOverride = true;
			var wrapperFull = new AddressWrapper(docAddress, Factory);
			AssertEquals("wrapperFull.ContactName", "BEN_NAME", wrapperFull.ContactName);

			docAddress.E2_AddressOverride = false;
			wrapperFull = new AddressWrapper(docAddress, Factory);
			AssertEquals("wrapperFull.ContactName", "BEN_NAME", wrapperFull.ContactName);

			docAddress.E2_Contact = "Test User";
			wrapperFull = new AddressWrapper(docAddress, Factory);
			AssertEquals("wrapperFull.ContactName", "Test User", wrapperFull.ContactName);
		}

		public void TestWrapperMappingFromJobDocAddressConsigneePhoneMobileFax()
		{
			var organisation = OrgTestHelper.GetNewOrganisation("CLNTEATDOG", "CLINTY EATS DOGS",
				"'IMPRESSIVE TOWER' BUILDING", "344 RED CRAYON ROAD", "AU", "MANLYVALE", "2354", "VIC", "AUMEL",
				ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "Sh1", Factory);
			var address = organisation.MainAddress;
			SetupAdditionalAddressProperties(address);

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.DefaultContactType = ContactType.Consignee;
			docAddress.E2_OA_Address = address.PK;
			docAddress.E2_Contact = "BENJAMIN";

			var contact = OrgTestHelper.AddNewContact(organisation, ContactType.Consignee, "BENJAMIN");
			contact.OC_Phone = "8001 2200";
			contact.OC_Fax = "8001 2201";
			contact.OC_Mobile = "8001 2202";
			contact.OC_Email = "e@mail.com";

			docAddress.ContactPK = contact.PK;

			var wrapperFull = new AddressWrapper(docAddress, Factory);
			AssertEquals("Should use E2_Phone", "8001 2200", wrapperFull.Phone);
			AssertEquals("Should use E2_Phone", docAddress.E2_Phone, wrapperFull.Phone);
			AssertEquals("Should use E2_Fax", "8001 2201", wrapperFull.Fax);
			AssertEquals("Should use E2_Fax", docAddress.E2_Fax, wrapperFull.Fax);
			AssertEquals("Should use E2_Email", "e@mail.com", wrapperFull.Email);
			AssertEquals("Should use E2_Email", docAddress.E2_Email, wrapperFull.Email);
			AssertEquals("Should use E2_Mobile", "8001 2202", wrapperFull.Mobile);
			AssertEquals("Should use E2_Mobile", docAddress.E2_Mobile, wrapperFull.Mobile);
		}

		public void TestWrapperMappingFromJobDocAddress_AdditionalAddressInformation_WhenRegistryIsEnabled()
		{
			var orgHeader = OrgTestHelper.GetNewOrganisation("CLNTEATDOG", "CLINTY EATS DOGS",
				"'IMPRESSIVE TOWER' BUILDING", "344 RED CRAYON ROAD", "AU", "MANLYVALE", "2354", "VIC", "AUMEL",
				"PHONE", "FAX", "email@domain.com", "MOBILE", "Sh1", Factory);
			var address = orgHeader.MainAddress;
			var additional1 = address.AdditionalInfos.AddNew();
			additional1.OAI_AdditionalInfo = "TEST ADDITIONAL 1";
			additional1.OAI_IsPrimary = true;

			var additional2 = address.AdditionalInfos.AddNew();
			additional2.OAI_AdditionalInfo = "TEST ADDITIONAL 2";
			additional2.OAI_IsPrimary = false;

			var translatedAddress1 = address.TranslatedAddresses.AddNew();
			translatedAddress1.OTA_Language = Core.Constants.Languages.ChineseSimplified;
			translatedAddress1.OTA_CompanyName = "慧咨全球";
			translatedAddress1.OTA_Address1 = "第五大道";
			translatedAddress1.OTA_Address2 = "麦斯考特";
			translatedAddress1.OTA_City = "悉尼";
			translatedAddress1.OTA_State = "新南威尔士";
			translatedAddress1.OTA_PostCode = "2100";

			var translatedAddress2 = address.TranslatedAddresses.AddNew();
			translatedAddress2.OTA_Language = Core.Constants.Languages.Russian;
			translatedAddress2.OTA_Address1 = "МОСКВА";

			var translatedAdditionalInfo1 = additional1.TranslatedInfos.AddNew();
			translatedAdditionalInfo1.OTI_Language = Core.Constants.Languages.ChineseSimplified;
			translatedAdditionalInfo1.OTI_AdditionalInfo = "虚拟地址";

			var translatedAdditionalInfo2 = additional2.TranslatedInfos.AddNew();
			translatedAdditionalInfo2.OTI_Language = Core.Constants.Languages.Russian;
			translatedAdditionalInfo2.OTI_AdditionalInfo = "ДОПОЛНИТЕЛЬНЫЙ";

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = address.PK;

			using (OrganisationRegistry.Instance.AllowOverrideAddressAdditionalInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var wrapperFull = new AddressWrapper(docAddress, Factory);
				AssertEquals("TEST ADDITIONAL 1\n'IMPRESSIVE TOWER' BUILDING\n344 RED CRAYON ROAD\nMANLYVALE VIC 2354\nAUSTRALIA", wrapperFull.Address);

				docAddress.E2_AdditionalAddressInformation = "NEW ADDITIONAL";
				wrapperFull = new AddressWrapper(docAddress, Factory);
				AssertEquals("NEW ADDITIONAL\n'IMPRESSIVE TOWER' BUILDING\n344 RED CRAYON ROAD\nMANLYVALE VIC 2354\nAUSTRALIA", wrapperFull.Address);

				using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.ChineseSimplified))
				{
					wrapperFull = new AddressWrapper(docAddress, Factory);
					AssertStartsWith("Use override value", "NEW ADDITIONAL\n第五大道\n麦斯考特\n悉尼 新南威尔士 2100", wrapperFull.Address);

					docAddress.E2_AdditionalAddressInformation = "TEST ADDITIONAL 2";
					wrapperFull = new AddressWrapper(docAddress, Factory);
					AssertStartsWith("Use override value", "TEST ADDITIONAL 2\n第五大道\n麦斯考特\n悉尼 新南威尔士 2100", wrapperFull.Address);

					docAddress.E2_AdditionalAddressInformation = "TEST ADDITIONAL 1";
					wrapperFull = new AddressWrapper(docAddress, Factory);
					AssertStartsWith("Use translated value if translated info exists", "虚拟地址\n第五大道\n麦斯考特\n悉尼 新南威尔士 2100", wrapperFull.Address);
				}

				using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.Russian))
				{
					wrapperFull = new AddressWrapper(docAddress, Factory);
					AssertStartsWith("Use override value", "TEST ADDITIONAL 1\nМОСКВА", wrapperFull.Address);

					docAddress.E2_AdditionalAddressInformation = "TEST ADDITIONAL 2";
					wrapperFull = new AddressWrapper(docAddress, Factory);
					AssertStartsWith("Use translated value if translated info exists", "ДОПОЛНИТЕЛЬНЫЙ\nМОСКВА", wrapperFull.Address);

					docAddress.E2_AdditionalAddressInformation = "NEW ADDITIONAL";
					wrapperFull = new AddressWrapper(docAddress, Factory);
					AssertStartsWith("Use override value", "NEW ADDITIONAL\nМОСКВА", wrapperFull.Address);
				}
			}
		}

		public void TestWrapperMappingFromJobDocAddress_AdditionalAddressInformation_WhenRegistryIsDisabled()
		{
			var orgHeader = OrgTestHelper.GetNewOrganisation("CLNTEATDOG", "CLINTY EATS DOGS",
							"'IMPRESSIVE TOWER' BUILDING", "344 RED CRAYON ROAD", "AU", "MANLYVALE", "2354", "VIC", "AUMEL",
							"PHONE", "FAX", "email@domain.com", "MOBILE", "Sh1", Factory);
			var address = orgHeader.MainAddress;
			var additional1 = address.AdditionalInfos.AddNew();
			additional1.OAI_AdditionalInfo = "TEST ADDITIONAL 1";
			additional1.OAI_IsPrimary = true;

			var additional2 = address.AdditionalInfos.AddNew();
			additional2.OAI_AdditionalInfo = "TEST ADDITIONAL 2";
			additional2.OAI_IsPrimary = false;

			var translatedAddress1 = address.TranslatedAddresses.AddNew();
			translatedAddress1.OTA_Language = Core.Constants.Languages.ChineseSimplified;
			translatedAddress1.OTA_CompanyName = "慧咨全球";
			translatedAddress1.OTA_Address1 = "第五大道";
			translatedAddress1.OTA_Address2 = "麦斯考特";
			translatedAddress1.OTA_City = "悉尼";
			translatedAddress1.OTA_State = "新南威尔士";
			translatedAddress1.OTA_PostCode = "2100";

			var translatedAddress2 = address.TranslatedAddresses.AddNew();
			translatedAddress2.OTA_Language = Core.Constants.Languages.Russian;
			translatedAddress2.OTA_Address1 = "МОСКВА";

			var translatedAdditionalInfo1 = additional1.TranslatedInfos.AddNew();
			translatedAdditionalInfo1.OTI_Language = Core.Constants.Languages.ChineseSimplified;
			translatedAdditionalInfo1.OTI_AdditionalInfo = "虚拟地址";

			var translatedAdditionalInfo2 = additional2.TranslatedInfos.AddNew();
			translatedAdditionalInfo2.OTI_Language = Core.Constants.Languages.Russian;
			translatedAdditionalInfo2.OTI_AdditionalInfo = "ДОПОЛНИТЕЛЬНЫЙ";

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = address.PK;

			using (OrganisationRegistry.Instance.AllowOverrideAddressAdditionalInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				docAddress.E2_AdditionalAddressInformation = "NEW ADDITIONAL";
				AssertEquals("Ignored E2_AdditionalAddressInformation value when AllowOverrideAddressAdditionalInformation is false", address.OA_AdditionalAddressInformation, docAddress.E2_AdditionalAddressInformation);
				var wrapperFull = new AddressWrapper(docAddress, Factory);
				AssertEquals("TEST ADDITIONAL 1\n'IMPRESSIVE TOWER' BUILDING\n344 RED CRAYON ROAD\nMANLYVALE VIC 2354\nAUSTRALIA", wrapperFull.Address);

				using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.ChineseSimplified))
				{
					wrapperFull = new AddressWrapper(docAddress, Factory);
					AssertStartsWith("Use translated value if translated info exists", "虚拟地址\n第五大道\n麦斯考特\n悉尼 新南威尔士 2100", wrapperFull.Address);
				}

				using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.Russian))
				{
					wrapperFull = new AddressWrapper(docAddress, Factory);
					AssertStartsWith("Use translated value if translated info exists", "TEST ADDITIONAL 1\nМОСКВА", wrapperFull.Address);
				}
			}
		}

		public void TestWrapperMappingWhenOrgAllowMixedCaseIsNo()
		{
			var originalAllowMixedCase = Env.Registry.OrgAllowMixedCase;
			if (!originalAllowMixedCase)
			{
				Env.Registry.SetOrgAllowMixedCase(true);
			}

			try
			{
				var organisation = OrgTestHelper.GetNewOrganisation("Clnteatdog", "Clinty eats dogs",
				"'Impressive Tower' Building", "344 red crayon road", "AU", "Manlyvale", "2354", "Victoria", "Aumel",
				"PHONE", "FAX", "email@domain.com", "MOBILE", "Sh1", Factory);
				var address = organisation.MainAddress;
				SetupAdditionalAddressProperties(address);

				AddressWrapper wrapperFull = new AddressWrapper(address, ContactType.All, Factory);
				AssertEquals("wrapperFull.Country.Code", "AU", wrapperFull.Country.Code);
				AssertEquals("wrapperFull.Location.UNLOCO", "AUMEL", wrapperFull.Location.UNLOCO);
				AssertEquals("wrapperFull.CompanyCode", "Clnteatdog", wrapperFull.CompanyCode);
				AssertEquals("wrapperFull.CompanyName", "Clinty eats dogs", wrapperFull.CompanyName);
				AssertEquals("wrapperFull.CompanyNameAndAddress", "Clinty eats dogs\n'Impressive Tower' Building\n344 red crayon road\nManlyvale Victoria 2354\nAustralia", wrapperFull.CompanyNameAndAddress);
				AssertEquals("wrapperFull.Address", "'Impressive Tower' Building\n344 red crayon road\nManlyvale Victoria 2354\nAustralia", wrapperFull.Address);
				AssertEquals("wrapperFull.AddressLine1", "'Impressive Tower' Building", wrapperFull.AddressLine1);
				AssertEquals("wrapperFull.AddressLine2", "344 red crayon road", wrapperFull.AddressLine2);
				AssertEquals("wrapperFull.City", "Manlyvale", wrapperFull.City);
				AssertEquals("wrapperFull.State", "Victoria", wrapperFull.State);
				AssertEquals("wrapperFull.ShortCode", "Sh1", wrapperFull.ShortCode);
				AssertEquals("wrapperFull.PostCode", "2354", wrapperFull.PostCode);

				Env.Registry.SetOrgAllowMixedCase(false);

				wrapperFull = new AddressWrapper(address, ContactType.All, Factory);
				AssertEquals("wrapperFull.Country.Code", "AU", wrapperFull.Country.Code);
				AssertEquals("wrapperFull.Location.UNLOCO", "AUMEL", wrapperFull.Location.UNLOCO);
				AssertEquals("wrapperFull.CompanyCode", "CLNTEATDOG", wrapperFull.CompanyCode);
				AssertEquals("wrapperFull.CompanyName", "CLINTY EATS DOGS", wrapperFull.CompanyName);
				AssertEquals("wrapperFull.CompanyNameAndAddress", "CLINTY EATS DOGS\n'IMPRESSIVE TOWER' BUILDING\n344 RED CRAYON ROAD\nMANLYVALE VICTORIA 2354\nAUSTRALIA", wrapperFull.CompanyNameAndAddress);
				AssertEquals("wrapperFull.Address", "'IMPRESSIVE TOWER' BUILDING\n344 RED CRAYON ROAD\nMANLYVALE VICTORIA 2354\nAUSTRALIA", wrapperFull.Address);
				AssertEquals("wrapperFull.AddressLine1", "'IMPRESSIVE TOWER' BUILDING", wrapperFull.AddressLine1);
				AssertEquals("wrapperFull.AddressLine2", "344 RED CRAYON ROAD", wrapperFull.AddressLine2);
				AssertEquals("wrapperFull.City", "MANLYVALE", wrapperFull.City);
				AssertEquals("wrapperFull.State", "VICTORIA", wrapperFull.State);
				AssertEquals("wrapperFull.ShortCode", "SH1", wrapperFull.ShortCode);
				AssertEquals("wrapperFull.PostCode", "2354", wrapperFull.PostCode);
			}
			finally
			{
				Env.Registry.SetOrgAllowMixedCase(originalAllowMixedCase);
			}
		}

		[TestDate(2007, 1, 1)]
		public void TestPickupDeliveryAndDoNotAttendTimes()
		{
			OrgHeader organisation = OrgTestHelper.GetNewOrganisation("JHSFURH", "JUST ANOTHER ORG",
				"THE CARGOWISE DUMP", "", "AU", "ALEXANDRIA", "2354", "NSW", "AUSYD",
				"PHONE", "FAX", "email@domain.com", "MOBILE", "", Factory);
			OrgAddress address = organisation.Addresses[0];
			address.OA_PickupFromTimeOnly = ZDateTime.Now.AddHours(10);
			address.OA_PickupToTimeOnly = ZDateTime.Now.AddHours(15);
			address.OA_DeliverFromTimeOnly = ZDateTime.Now.AddHours(17);
			address.OA_DeliverToTimeOnly = ZDateTime.Now.AddHours(20);
			address.OA_DoNotAttendFrom = ZDateTime.Now.AddHours(21);
			address.OA_DoNotAttendTo = ZDateTime.Now.AddHours(23);

			AddressWrapper wrapper = new AddressWrapper(address, ContactType.All, Factory);
			AssertEquals("wrapper.PickupFromTime", "10:00", wrapper.PickupFromTime);
			AssertEquals("wrapper.PickupToTime", "15:00", wrapper.PickupToTime);
			AssertEquals("wrapper.DeliverFromTime", "17:00", wrapper.DeliverFromTime);
			AssertEquals("wrapper.DeliverToTime", "20:00", wrapper.DeliverToTime);
			AssertEquals("wrapper.DoNotAttendFromTime", "21:00", wrapper.DoNotAttendFromTime);
			AssertEquals("wrapper.DoNotAttendToTime", "23:00", wrapper.DoNotAttendToTime);
		}

		public void TestJobDocAddressOverrideForPickupDeliveryAndDoNotAttendTimes()
		{
			OrgHeader organisation = OrgTestHelper.GetNewOrganisation("KJHFSKJRT", "COMPANY NAME",
				"ADDRESS LINE 1", "ADDRESS LINE 2", "CC", "CITY", "POSTCODE", "STATE", "PORT",
				"PHONE", "FAX", "email@domain.com", "MOBILE", "", Factory);
			OrgAddress address = organisation.MainAddress;
			OrgContact contact = OrgTestHelper.AddNewContact(organisation, ContactType.All, "HAPPY");
			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.DefaultContactType = ContactType.All;
			docAddress.E2_OA_Address = address.PK;
			docAddress.E2_AddressOverride = true;
			docAddress.E2_CompanyName = "OVERRIDE COMPANY NAME";
			docAddress.E2_Address1 = "OVERRIDE ADDRESS 1";
			docAddress.E2_Address2 = "OVERRIDE ADDRESS 2";
			docAddress.E2_RN_NKCountryCode = "CC";
			docAddress.E2_City = "OVERRIDE CITY";
			docAddress.E2_State = "OVERRIDE STATE";
			docAddress.E2_Postcode = "OVER CODE";
			docAddress.E2_Contact = "OVERRIDE CONTACT";
			docAddress.E2_Phone = "OVERRIDE PHONE";
			docAddress.E2_Fax = "OVERRIDE FAX";
			docAddress.E2_Email = "OVERRIDE EMAIL";

			AddressWrapper wrapper = new AddressWrapper(docAddress, Factory);
			AssertEquals("wrapper.PickupFromTime", ZString.Empty, wrapper.PickupFromTime);
			AssertEquals("wrapper.PickupToTime", ZString.Empty, wrapper.PickupToTime);
			AssertEquals("wrapper.DeliverFromTime", ZString.Empty, wrapper.DeliverFromTime);
			AssertEquals("wrapper.DeliverToTime", ZString.Empty, wrapper.DeliverToTime);
			AssertEquals("wrapper.DoNotAttendFromTime", ZString.Empty, wrapper.DoNotAttendFromTime);
			AssertEquals("wrapper.DoNotAttendToTime", ZString.Empty, wrapper.DoNotAttendToTime);
		}

		public void TestJobDocAddressCountryIsSet()
		{
			OrgHeader organisation = OrgTestHelper.GetNewOrganisation("KJHFSKJRT", "COMPANY NAME",
				"ADDRESS LINE 1", "ADDRESS LINE 2", "AU", "CITY", "POSTCODE", "STATE", "PORT",
				"PHONE", "FAX", "email@domain.com", "MOBILE", "", Factory);
			OrgAddress address = organisation.MainAddress;
			OrgContact contact = OrgTestHelper.AddNewContact(organisation, ContactType.All, "HAPPY");
			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.DefaultContactType = ContactType.All;
			docAddress.E2_OA_Address = address.PK;
			docAddress.E2_AddressOverride = false;
			docAddress.E2_CompanyName = "OVERRIDE COMPANY NAME";
			docAddress.E2_Address1 = "OVERRIDE ADDRESS 1";
			docAddress.E2_Address2 = "OVERRIDE ADDRESS 2";
			docAddress.E2_RN_NKCountryCode = "CC";
			docAddress.E2_City = "OVERRIDE CITY";
			docAddress.E2_RN_NKCountryCode = "US";
			docAddress.E2_State = "OVERRIDE STATE";
			docAddress.E2_Postcode = "OVER CODE";
			docAddress.E2_Contact = "OVERRIDE CONTACT";
			docAddress.E2_Phone = "OVERRIDE PHONE";
			docAddress.E2_Fax = "OVERRIDE FAX";
			docAddress.E2_Email = "OVERRIDE EMAIL";

			var list = new CodeDescriptionPairList();
			list.AddPair("FR", "France");

			var wrapper = new AddressWrapper(docAddress, "FR", list.GetMultilingualDescriptionFromCode("FR"), "FR", Factory);
			AssertContains("country should be the one set.", "FRANCE", wrapper.CompanyNameAndAddress);
		}

		public void TestMainAddressInDifferentLanguageTranslations()
		{
			var organisation = OrgTestHelper.GetNewOrganisation("CLNTEATDOG", "WiseTechGlobal",
			"Huaxin Cheng", "Jiangdong Rd.", "CN", "Nanjing", "2354", "32", "NJKDF",
			"PHONE", "FAX", "email@domain.com", "MOBILE", "Sh1", Factory);

			var translatedAddress = organisation.MainAddress.TranslatedAddresses.AddNew();
			translatedAddress.OTA_Language = Core.Constants.Languages.ChineseSimplified;
			translatedAddress.OTA_CompanyName = "慧咨环球";
			translatedAddress.OTA_Address1 = "华新城";
			translatedAddress.OTA_Address2 = "江东中路";
			translatedAddress.OTA_City = "南京";
			translatedAddress.OTA_PostCode = "2100";

			using (DocumentGenerationHelper.SetIsGeneratingDocument())
			{
				AddressWrapper addressWrapper;
				using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.ChineseSimplified))
				{
					addressWrapper = new AddressWrapper(organisation.MainAddress, ContactType.LocalClient, Factory);
					AssertEquals("慧咨环球", addressWrapper.CompanyName);
					AssertEquals("华新城", addressWrapper.AddressLine1);
					AssertEquals("江东中路", addressWrapper.AddressLine2);
					AssertEquals("南京", addressWrapper.City);
					AssertEquals("华新城\n江东中路\n南京\n2100\n中国", addressWrapper.Address);
				}

				using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.Faeroese))
				{
					AssertEquals("慧咨环球", addressWrapper.CompanyName);
					AssertEquals("华新城", addressWrapper.AddressLine1);
					AssertEquals("江东中路", addressWrapper.AddressLine2);
					AssertEquals("南京", addressWrapper.City);
					AssertEquals("华新城\n江东中路\n南京\n2100\nCHINA", addressWrapper.Address);
				}
			}
		}

		public void TestAddressAsASingleLine()
		{
			OrgHeader organisation = OrgTestHelper.GetNewOrganisation("CLNTEATDOG", "CLINTY EATS DOGS",
	"'IMPRESSIVE TOWER' BUILDING", "  344 RED   CRAYON   ROAD", "AU", "MANLY  VALE", "2354", "VIC", "AUMEL",
	"PHONE", "FAX", "email@domain.com", "MOBILE", "", Factory);
			OrgAddress address = organisation.MainAddress;
			OrgContact contact = OrgTestHelper.AddNewContact(organisation, ContactType.All, "BEN");

			AddressWrapper wrapperFull = new AddressWrapper(address, ContactType.All, Factory);
			AssertEquals("AddressAsASingleLine", "'IMPRESSIVE TOWER' BUILDING 344 RED CRAYON ROAD MANLY VALE VIC 2354 AUSTRALIA", wrapperFull.AddressAsASingleLine);
		}

		public void TestOrganization()
		{
			var organisation = OrgTestHelper.GetNewOrganisation("CLNTEATDOG", "CLINTY EATS DOGS",
	"'IMPRESSIVE TOWER' BUILDING", "  344 RED   CRAYON   ROAD", "AU", "MANLY  VALE", "2354", "VIC", "AUMEL",
	"PHONE", "FAX", "email@domain.com", "MOBILE", "", Factory);
			var address = organisation.MainAddress;

			var customCode = organisation.CustomsCodes.AddNew();
			customCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			customCode.OK_CustomsRegNo = "XCV12345";

			var wrapper = new AddressWrapper(OrganisationUsageType.Test, address, ContactType.All, Factory);
			AssertEquals("Organization custom code", "XCV12345", wrapper.Organization.CustomsCodes[0].RegistrationNumberOrCode);
		}

		public void TestAviationSecurityWithData()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var address = Factory.NewWithValidTestData<OrgAddress>();

				var orgCountryData = address.KnownShipperDetails.AddNew();
				orgCountryData.OV_EXApprovedOrMajorExporter = "AC";
				orgCountryData.OV_EXApprovalNumber = "AC12345";

				var anotherOrgCountryData = address.KnownShipperDetails.AddNew();
				orgCountryData.OV_EXApprovedOrMajorExporter = "KC";
				orgCountryData.OV_EXApprovalNumber = "KC54321";

				var wrapper = new AddressWrapper(OrganisationUsageType.Test, address, ContactType.All, Factory);
				AssertEquals("Aviation Security Approval Type", "KC", wrapper.AviationSecurity.ApprovalType.Code);
				AssertEquals("Aviation Security Approval Number", "KC54321", wrapper.AviationSecurity.ApprovalNumber);
			}
		}

		public void TestAviationSecurityWithoutData()
		{
			var wrapper = new AddressWrapper(OrganisationUsageType.Test, null, ContactType.All, Factory);
			AssertNotNull(wrapper.AviationSecurity);
			AssertEquals("NO", wrapper.AviationSecurity.ApprovalType.Code);
		}

		public void TestAviationSecurityWhenDisabled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_AU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var address = Factory.NewWithValidTestData<OrgAddress>();

				var orgCountryData = address.KnownShipperDetails.AddNew();
				orgCountryData.OV_EXApprovedOrMajorExporter = "AC";
				orgCountryData.OV_EXApprovalNumber = "AC12345";

				var wrapper = new AddressWrapper(OrganisationUsageType.Test, address, ContactType.All, Factory);
				AssertEquals("Not approved", "NO", wrapper.AviationSecurity.ApprovalType.Code);
				AssertEquals("No Approval Number", "", wrapper.AviationSecurity.ApprovalNumber);
			}
		}

		[SetOrgAllowMixedCase(true)]
		public void TestNewConstructorWithZString()
		{
			AddressWrapper testWrapper = new AddressWrapper("Firstline\r\nSecondline\r\nThirdline", Factory);
			AssertEquals("Firstline", testWrapper.CompanyName);
			AssertEquals("Secondline\r\nThirdline", testWrapper.Address);
			AssertEquals("Firstline\r\nSecondline\r\nThirdline", testWrapper.CompanyNameAndAddress);

			testWrapper = new AddressWrapper(ZString.Empty, Factory);

			bool flag = Env.Registry.OrgAllowMixedCase;
			try
			{
				Env.Registry.SetOrgAllowMixedCase(false);
				testWrapper = new AddressWrapper("Firstline\r\nSecondline\r\nThirdline", Factory);
				AssertEquals("Firstline".ToUpperInvariant(), testWrapper.CompanyName);
				AssertEquals("Secondline\r\nThirdline".ToUpperInvariant(), testWrapper.Address);
				AssertEquals("Firstline\r\nSecondline\r\nThirdline".ToUpperInvariant(), testWrapper.CompanyNameAndAddress);
				Assert(testWrapper.Organization != null);

				testWrapper = new AddressWrapper((ZString)null, Factory);
				AssertEquals(ZString.Empty, testWrapper.CompanyName);
				AssertEquals(ZString.Empty, testWrapper.Address);
				AssertEquals(ZString.Empty, testWrapper.CompanyNameAndAddress);
				Assert(testWrapper.Organization != null);
			}
			finally
			{
				Env.Registry.SetOrgAllowMixedCase(flag);
			}
		}

		public void TestSameAs()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "Town";

			var address = organisation.MainAddress;
			address.OA_RN_NKCountryCode = "AU";
			address.OA_Address1 = "And Country";

			var addressWrapper = new AddressWrapper(address, ContactType.Consignee, Factory);
			var simpleAddressWrapper = new AddressWrapper("Town And Country\nAustralia", Factory);
			AssertEquals("SameAs", true, addressWrapper.SameAs(simpleAddressWrapper));
		}

		[ExpectNoExceptions]
		public void TestAddressWrapperWithFactory()
		{
			var locationWrapper = AddressWrapper.Empty(Factory).Location;
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestAddressWrapperWithoutFactory()
		{
			var addressWrapper = new AddressWrapper((JobDocAddress)null, null);
			var locationWrapper = addressWrapper.Location;
		}

		public void TestTimetableAdvanced()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "Org1";

			var address = organisation.MainAddress;
			address.OA_Address1 = "Address1";
			using (var mockData = Res.UseMockData())
			{
				var key1 = ((ResourceString)DayOfWeekCodeList.LocalizedCodes.Monday).ResourceKey;
				var key2 = ((ResourceString)DayOfWeekCodeList.LocalizedCodes.Tuesday).ResourceKey;

				mockData.Put(key1, new ResourceStringData(key1, "一"));
				mockData.Put(key2, new ResourceStringData(key2, "二"));

				address.SetTimetablesRangeType(OrgTimeTableRangeType.Advanced);
				address.Timetables.allowDeleteLastTimeTable = true;
				address.Timetables.DeleteAll();
				OrgTimetable t1 = address.Timetables.AddNew();
				t1.OTT_OA = address.PK;
				t1.OTT_Type = OrgTimetableType.Codes.Pickup;
				t1.DayOfWeek = "FRI";
				t1.OTT_TimeFrom = new DateTime(2015, 1, 1, 8, 0, 0);
				t1.OTT_TimeTo = new DateTime(2015, 1, 1, 9, 0, 0);

				OrgTimetable t2 = address.Timetables.AddNew();
				t2.OTT_OA = address.PK;
				t2.OTT_Type = OrgTimetableType.Codes.Pickup;
				t2.DayOfWeek = "WED";
				t2.OTT_TimeFrom = new DateTime(2015, 1, 1, 8, 0, 0);
				t2.OTT_TimeTo = new DateTime(2015, 1, 1, 9, 30, 0);

				OrgTimetable t3 = address.Timetables.AddNew();
				t3.OTT_OA = address.PK;
				t3.OTT_Type = OrgTimetableType.Codes.Pickup;
				t3.DayOfWeek = "MON";
				t3.OTT_TimeFrom = new DateTime(2015, 1, 1, 9, 0, 0);
				t3.OTT_TimeTo = new DateTime(2015, 1, 1, 9, 30, 0);

				OrgTimetable t4 = address.Timetables.AddNew();
				t4.OTT_OA = address.PK;
				t4.OTT_Type = OrgTimetableType.Codes.Deliver;
				t4.DayOfWeek = "TUE";
				t4.OTT_TimeFrom = new DateTime(2015, 1, 1, 14, 0, 0);
				t4.OTT_TimeTo = new DateTime(2015, 1, 1, 14, 30, 0);

				var addressWrapper = new AddressWrapper(address, ContactType.All, Factory);
				AssertEquals("[一] 09:00 – 09:30 [WED] 08:00 – 09:30 [FRI] 08:00 – 09:00", addressWrapper.PickupTimetable);
				AssertEquals("[二] 14:00 – 14:30", addressWrapper.DeliverTimetable);
			}
		}

		public void TestTimetableWeekday()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "Org1";

			var address = organisation.MainAddress;
			address.Country.IsSaturdayNonWorkingDay = true;
			address.Country.IsSundayNonWorkingDay = true;
			address.OA_Address1 = "Address1";

			address.Timetables.allowDeleteLastTimeTable = true;
			address.Timetables.DeleteAll();

			OrgTimetable ttb;
			foreach (var day in new string[] { "MON", "TUE", "WED", "THU", "FRI" })
			{
				ttb = address.Timetables.AddNew();
				ttb.OTT_OA = address.PK;
				ttb.OTT_Type = OrgTimetableType.Codes.Deliver;
				ttb.DayOfWeek = day;
				ttb.OTT_TimeFrom = new DateTime(2015, 1, 1, 14, 0, 0);
				ttb.OTT_TimeTo = new DateTime(2015, 1, 1, 15, 0, 0);
			}

			var addressWrapper = new AddressWrapper(address, ContactType.All, Factory);
			AssertEquals("[Weekday] 14:00 – 15:00", addressWrapper.DeliverTimetable);
		}

		public void TestTimetableWeekday_WithIsForAllWeekdayField()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "Org1";

			var address = organisation.MainAddress;
			address.Country.IsSaturdayNonWorkingDay = true;
			address.Country.IsSundayNonWorkingDay = true;
			address.OA_Address1 = "Address1";

			address.Timetables.allowDeleteLastTimeTable = true;
			address.Timetables.DeleteAll();

			OrgTimetable ttb;
			ttb = address.Timetables.AddNew();
			ttb.OTT_OA = address.PK;
			ttb.OTT_Type = OrgTimetableType.Codes.Deliver;
			ttb.OTT_IsForAllWeekDays = true;
			ttb.OTT_TimeFrom = new DateTime(2015, 1, 1, 14, 0, 0);
			ttb.OTT_TimeTo = new DateTime(2015, 1, 1, 15, 0, 0);

			var addressWrapper = new AddressWrapper(address, ContactType.All, Factory);
			AssertEquals("[Weekday] 14:00 – 15:00", addressWrapper.DeliverTimetable);
		}

		public void TestTimetableWeekdaySplit()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "Org1";

			var address = organisation.MainAddress;
			address.Country.IsSaturdayNonWorkingDay = true;
			address.Country.IsSundayNonWorkingDay = true;
			address.OA_Address1 = "Address1";

			address.Timetables.allowDeleteLastTimeTable = true;
			address.Timetables.DeleteAll();

			OrgTimetable ttb;
			foreach (var day in new string[] { "MON", "TUE", "WED", "THU", "FRI" })
			{
				ttb = address.Timetables.AddNew();
				ttb.OTT_OA = address.PK;
				ttb.OTT_Type = OrgTimetableType.Codes.Deliver;
				ttb.DayOfWeek = day;
				ttb.OTT_TimeFrom = new DateTime(2015, 1, 1, 8, 0, 0);
				ttb.OTT_TimeTo = new DateTime(2015, 1, 1, 12, 0, 0);

				ttb = address.Timetables.AddNew();
				ttb.OTT_OA = address.PK;
				ttb.OTT_Type = OrgTimetableType.Codes.Deliver;
				ttb.DayOfWeek = day;
				ttb.OTT_TimeFrom = new DateTime(2015, 1, 1, 13, 0, 0);
				ttb.OTT_TimeTo = new DateTime(2015, 1, 1, 17, 0, 0);
			}

			var addressWrapper = new AddressWrapper(address, ContactType.All, Factory);
			AssertEquals("[Weekday] 08:00 – 12:00, 13:00 – 17:00", addressWrapper.DeliverTimetable);
		}

		public void TestTimetableWeekdaySplit_WithIsForAllWeekdayField()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "Org1";

			var address = organisation.MainAddress;
			address.Country.IsSaturdayNonWorkingDay = true;
			address.Country.IsSundayNonWorkingDay = true;
			address.OA_Address1 = "Address1";

			address.Timetables.allowDeleteLastTimeTable = true;
			address.Timetables.DeleteAll();

			OrgTimetable ttb;
			ttb = address.Timetables.AddNew();
			ttb.OTT_OA = address.PK;
			ttb.OTT_Type = OrgTimetableType.Codes.Deliver;
			ttb.OTT_IsForAllWeekDays = true;
			ttb.OTT_TimeFrom = new DateTime(2015, 1, 1, 8, 0, 0);
			ttb.OTT_TimeTo = new DateTime(2015, 1, 1, 12, 0, 0);

			ttb = address.Timetables.AddNew();
			ttb.OTT_OA = address.PK;
			ttb.OTT_Type = OrgTimetableType.Codes.Deliver;
			ttb.OTT_IsForAllWeekDays = true;
			ttb.OTT_TimeFrom = new DateTime(2015, 1, 1, 13, 0, 0);
			ttb.OTT_TimeTo = new DateTime(2015, 1, 1, 17, 0, 0);

			var addressWrapper = new AddressWrapper(address, ContactType.All, Factory);
			AssertEquals("[Weekday] 08:00 – 12:00, 13:00 – 17:00", addressWrapper.DeliverTimetable);
		}

		public void TestTimetableByTypeWhenCountryIsEmpty()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "Org1";
			var address = organisation.MainAddress;
			address.OA_Address1 = "Address1";
			address.Timetables.allowDeleteLastTimeTable = true;
			address.Timetables.DeleteAll();
			address.OA_RN_NKCountryCode = null;

			OrgTimetable ttb;
			foreach (var day in new string[] { "MON", "TUE", "WED", "THU", "FRI" })
			{
				ttb = address.Timetables.AddNew();
				ttb.OTT_OA = address.PK;
				ttb.OTT_Type = OrgTimetableType.Codes.Deliver;
				ttb.DayOfWeek = day;
				ttb.OTT_TimeFrom = new DateTime(2015, 1, 1, 14, 0, 0);
				ttb.OTT_TimeTo = new DateTime(2015, 1, 1, 15, 0, 0);
			}

			var addressWrapper = new AddressWrapper(address, ContactType.All, Factory);
			AssertNoExceptionThrown(() => { var addressWrp = addressWrapper.DeliverTimetable; });
			AssertEquals("[Weekday] 09:00 – 17:00, 14:00 – 15:00", addressWrapper.DeliverTimetable);
		}

		public void TestTimetableParametersUpdateOnPickupDeliveryChange()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "Org1";
			var address = organisation.MainAddress;
			address.OA_Address1 = "Address1";
			address.Timetables.allowDeleteLastTimeTable = true;
			address.Timetables.DeleteAll();
			address.OA_RN_NKCountryCode = null;

			OrgTimetable ttb;
			foreach (var day in new string[] { "MON", "TUE", "WED", "THU", "FRI" })
			{
				ttb = address.Timetables.AddNew();
				ttb.OTT_OA = address.PK;
				ttb.OTT_Type = OrgTimetableType.Codes.Deliver;
				ttb.DayOfWeek = day;
				ttb.OTT_TimeFrom = new DateTime(2015, 1, 1, 14, 0, 0);
				ttb.OTT_TimeTo = new DateTime(2015, 1, 1, 15, 0, 0);
			}

			var addressWrapper = new AddressWrapper(address, ContactType.All, Factory);
			AssertEquals("[Weekday] 09:00 – 17:00, 14:00 – 15:00", addressWrapper.DeliverTimetable);

			address.Timetables.allowDeleteLastTimeTable = true;
			address.Timetables.DeleteAll();

			ttb = address.Timetables.AddNew();
			ttb.OTT_OA = address.PK;
			ttb.OTT_Type = OrgTimetableType.Codes.Deliver;

			addressWrapper = new AddressWrapper(address, ContactType.All, Factory);
			AssertNoExceptionThrown(() => { var addressWrp = addressWrapper.DeliverTimetable; });
		}

		public void TestTimetableWeekdayDefault()
		{
			var defaultCollection = new DefaultOrgTimetableSettingsCollection();
			var defaultSetting = defaultCollection.AddNew();

			foreach (var day in new string[] { "MON", "TUE", "WED", "THU", "FRI" })
			{
				AddTimeTableInfo(OrgTimetableType.Codes.Deliver, new ZDateTime(ZDateTime.Now.Year, 1, 1, 14, 0, 0), new ZDateTime(ZDateTime.Now.Year, 1, 1, 15, 0, 0), day, defaultSetting.Timetables);
			}

			using (OrganisationRegistry.Instance.DefaultOrgTimetable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultCollection))
			{
				var organisation = Factory.New<OrgHeader>();
				organisation.OH_FullName = "Org1";
				var address = organisation.MainAddress;
				address.Country.IsSaturdayNonWorkingDay = true;
				address.Country.IsSundayNonWorkingDay = true;
				address.OA_Address1 = "Address1";

				var addressWrapper = new AddressWrapper(address, ContactType.All, Factory);
				AssertEquals("[Weekday] 14:00 – 15:00", addressWrapper.DeliverTimetable);
			}
		}

		public void TestTimetableEveryday()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "Org1";

			var address = organisation.MainAddress;
			address.Country.IsSaturdayNonWorkingDay = true;
			address.Country.IsSundayNonWorkingDay = true;
			address.OA_Address1 = "Address1";

			address.Timetables.allowDeleteLastTimeTable = true;
			address.Timetables.DeleteAll();

			OrgTimetable ttb;
			foreach (var day in new string[] { "MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN" })
			{
				ttb = address.Timetables.AddNew();
				ttb.OTT_OA = address.PK;
				ttb.OTT_Type = OrgTimetableType.Codes.Deliver;
				ttb.DayOfWeek = day;
				ttb.OTT_TimeFrom = new DateTime(2015, 1, 1, 14, 0, 0);
				ttb.OTT_TimeTo = new DateTime(2015, 1, 1, 15, 0, 0);
			}

			var addressWrapper = new AddressWrapper(address, ContactType.All, Factory);
			AssertEquals("[Everyday] 14:00 – 15:00", addressWrapper.DeliverTimetable);
		}

		public void TestTimeTableReportsDictionaryKeyException()
		{
			// Arrange
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "Org1";
			var address = organisation.MainAddress;
			address.OA_Address1 = "Address1";
			address.Timetables.allowDeleteLastTimeTable = true;
			address.Timetables.DeleteAll();
			address.OA_RN_NKCountryCode = "ZZ";

			OrgTimetable ttb;
			foreach (var day in new string[] { "MN", "TUE", "WED", "THU", "FRI" })
			{
				ttb = address.Timetables.AddNew();
				ttb.OTT_OA = address.PK;
				ttb.OTT_Type = OrgTimetableType.Codes.Deliver;
				ttb.DayOfWeek = day;
				ttb.OTT_TimeFrom = new DateTime(2015, 1, 1, 14, 0, 0);
				ttb.OTT_TimeTo = new DateTime(2015, 1, 1, 15, 0, 0);
			}

			var addressWrapper = new AddressWrapper(address, ContactType.All, Factory);
			var errorReporterMock = new Mock<IErrorReporter>();

			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				// Act
				AssertNoExceptionThrown(() => { var addressWrp = addressWrapper.DeliverTimetable; });
			}

			// Assert
			errorReporterMock.Verify(reporter =>
				reporter.Report(
					null,
					It.Is<string>(s =>
						s.StartsWith("Dictionary key exception for timetable days:")
						&& s.Contains("MN")
						&& s.Contains("[MON,TUE,WED,THU,FRI]")
					),
					It.IsAny<KeyNotFoundException>()
				),
				Times.Once,
				"Dictionary key exception should be reported with details of the current instances");
		}

		public void TestTimetableEverydaySplit()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "Org1";

			var address = organisation.MainAddress;
			address.OA_Address1 = "Address1";

			address.Timetables.allowDeleteLastTimeTable = true;
			address.Timetables.DeleteAll();

			OrgTimetable ttb;
			foreach (var day in new string[] { "SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT" })
			{
				ttb = address.Timetables.AddNew();
				ttb.OTT_OA = address.PK;
				ttb.OTT_Type = OrgTimetableType.Codes.Deliver;
				ttb.DayOfWeek = day;
				ttb.OTT_TimeFrom = new DateTime(2015, 1, 1, 8, 0, 0);
				ttb.OTT_TimeTo = new DateTime(2015, 1, 1, 12, 0, 0);

				ttb = address.Timetables.AddNew();
				ttb.OTT_OA = address.PK;
				ttb.OTT_Type = OrgTimetableType.Codes.Deliver;
				ttb.DayOfWeek = day;
				ttb.OTT_TimeFrom = new DateTime(2015, 1, 1, 13, 0, 0);
				ttb.OTT_TimeTo = new DateTime(2015, 1, 1, 17, 0, 0);
			}

			var addressWrapper = new AddressWrapper(address, ContactType.All, Factory);
			AssertEquals("[Everyday] 08:00 – 12:00, 13:00 – 17:00", addressWrapper.DeliverTimetable);
		}

		public void TestTimetableEverydayDefault()
		{
			var defaultCollection = new DefaultOrgTimetableSettingsCollection();
			var defaultSetting = defaultCollection.AddNew();

			foreach (var day in new string[] { "MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN" })
			{
				AddTimeTableInfo(OrgTimetableType.Codes.Deliver, new ZDateTime(ZDateTime.Now.Year, 1, 1, 14, 0, 0), new ZDateTime(ZDateTime.Now.Year, 1, 1, 15, 0, 0), day, defaultSetting.Timetables);
			}

			using (OrganisationRegistry.Instance.DefaultOrgTimetable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultCollection))
			{
				var organisation = Factory.New<OrgHeader>();
				organisation.OH_FullName = "Org1";
				var address = organisation.MainAddress;
				address.OA_Address1 = "Address1";

				var addressWrapper = new AddressWrapper(address, ContactType.All, Factory);
				AssertEquals("[Everyday] 14:00 – 15:00", addressWrapper.DeliverTimetable);
			}
		}

		public void TestTimetableDefault()
		{
			using (var mockData = Res.UseMockData())
			{
				var key1 = ((ResourceString)DayOfWeekCodeList.LocalizedCodes.Monday).ResourceKey;
				var key2 = ((ResourceString)DayOfWeekCodeList.LocalizedCodes.Tuesday).ResourceKey;

				mockData.Put(key1, new ResourceStringData(key1, "一"));
				mockData.Put(key2, new ResourceStringData(key2, "二"));

				var defaultCollection = new DefaultOrgTimetableSettingsCollection();
				var defaultSetting = defaultCollection.AddNew();

				AddTimeTableInfo(OrgTimetableType.Codes.Pickup, new ZDateTime(ZDateTime.Now.Year, 1, 1, 9, 0, 0), new ZDateTime(ZDateTime.Now.Year, 1, 1, 10, 0, 0), "MON", defaultSetting.Timetables);
				AddTimeTableInfo(OrgTimetableType.Codes.Pickup, new ZDateTime(ZDateTime.Now.Year, 1, 1, 9, 0, 0), new ZDateTime(ZDateTime.Now.Year, 1, 1, 17, 0, 0), "TUE", defaultSetting.Timetables);
				AddTimeTableInfo(OrgTimetableType.Codes.Pickup, new ZDateTime(ZDateTime.Now.Year, 1, 1, 9, 0, 0), new ZDateTime(ZDateTime.Now.Year, 1, 1, 17, 0, 0), "WED", defaultSetting.Timetables);

				AddTimeTableInfo(OrgTimetableType.Codes.Deliver, new ZDateTime(ZDateTime.Now.Year, 1, 1, 8, 0, 0), new ZDateTime(ZDateTime.Now.Year, 1, 1, 16, 0, 0), "MON", defaultSetting.Timetables);
				AddTimeTableInfo(OrgTimetableType.Codes.Deliver, new ZDateTime(ZDateTime.Now.Year, 1, 1, 8, 0, 0), new ZDateTime(ZDateTime.Now.Year, 1, 1, 18, 0, 0), "THU", defaultSetting.Timetables);

				using (OrganisationRegistry.Instance.DefaultOrgTimetable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultCollection))
				{
					var organisation = Factory.New<OrgHeader>();
					organisation.OH_FullName = "Org1";
					var address = organisation.MainAddress;
					address.OA_Address1 = "Address1";

					var addressWrapper = new AddressWrapper(address, ContactType.All, Factory);
					AssertEquals("[一] 09:00 – 10:00 [二] 09:00 – 17:00 [WED] 09:00 – 17:00", addressWrapper.PickupTimetable);
					AssertEquals("[一] 08:00 – 16:00 [THU] 08:00 – 18:00", addressWrapper.DeliverTimetable);
				}

				defaultSetting.Timetables.RemoveAndDeleteAll();
				using (OrganisationRegistry.Instance.DefaultOrgTimetable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultCollection))
				{
					var organisation = Factory.New<OrgHeader>();
					organisation.OH_FullName = "Org1";
					var address = organisation.MainAddress;
					address.OA_Address1 = "Address1";

					var addressWrapper = new AddressWrapper(address, ContactType.All, Factory);
					AssertEquals(string.Empty, addressWrapper.PickupTimetable);
					AssertEquals(string.Empty, addressWrapper.DeliverTimetable);
				}
			}
		}

		public void TestTimetableNotApplicable()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "Org1";

			var address = organisation.MainAddress;
			address.OA_Address1 = "Address1";

			var t1 = Factory.New<OrgTimetable>();
			t1.OTT_OA = address.PK;
			t1.OTT_Type = OrgTimetableType.Codes.Pickup;
			address.Timetables.Add(t1);

			var t3 = Factory.New<OrgTimetable>();
			t3.OTT_OA = address.PK;
			t3.OTT_Type = OrgTimetableType.Codes.Deliver;
			address.Timetables.Add(t3);

			var addressWrapper = new AddressWrapper(address, ContactType.All, Factory);
			AssertEquals("", addressWrapper.PickupTimetable);
		}

		public void TestWrappedLanguage()
		{
			var wrapper = default(AddressWrapper);

			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.French))
			{
				wrapper = new AddressWrapper("Test", Factory);
			}

			AssertEquals(SharedConstants.Languages.French, wrapper.WrappedLanguage);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Address                         (Default Field: CompanyNameAndAddress)
======================================================================
Name                                    Type
----------------------------------------------------------------------
AviationSecurity                        AviationSecurity
AccessPoint                             CodeAndDescription
CommunicationRequired                   CodeAndDescription
ContainerHandling                       CodeAndDescription
DockHeight                              CodeAndDescription
LabourRequired                          CodeAndDescription
Country                                 Country
Location                                Location
Organization                            Organisation
Address                                 String
AddressAsASingleLine                    String
AddressCaption                          String
AddressLine1                            String
AddressLine2                            String
City                                    String
CompanyCode                             String
CompanyName                             String
CompanyNameAndAddress                   String
ContactName                             String
DeliverFromTime                         String
DeliverTimetable                        String
DeliverToTime                           String
DeliveryRoute                           String
DeliveryRouteSequence                   Short
DoNotAttendFromTime                     String
DoNotAttendToTime                       String
Email                                   String
Fax                                     String
FurtherConstraints                      String
HasDockLeveller                         Bool
HasForkLift                             Bool
HasLoadingUnloadingConstraints          Bool
HasPalletJack                           Bool
HasWarehousing                          Bool
Mobile                                  String
OtherWarehouseFacilities                String
Phone                                   String
PickupFromTime                          String
PickupTimetable                         String
PickupToTime                            String
PostCode                                String
ShortCode                               String
State                                   String

CustomsCodes                            RegistrationNumberCode Collection";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
AccessPoint : DCK - Dock
AviationSecurity : (No Default Field Value Available on AviationSecurity)
CommunicationRequired : APP - Appointment Required
ContainerHandling : ASK - Ask
Country : AU - Australia
DockHeight : NON - Non-Standard Dock Height.
LabourRequired : ASK - Ask
Location : AUMEL - Melbourne
Organization : VIC\nAUSTRALIA
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		void AddTimeTableInfo(string type, ZDateTime from, ZDateTime to, string day, DefaultOrgTimetableCollection timeTableCollection)
		{
			var newItem = timeTableCollection.AddNew();
			newItem.Type = type;
			newItem.From = from;
			newItem.To = to;
			newItem.Day = day;
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_RL_NKClosestPort = "AUMEL";

			OrgAddress address = organisation.MainAddress;
			address.OA_AccessPoint = OrgConstants.AccessPoint.Code.Dock;
			address.OA_CommunicationRequired = OrgConstants.CommunicationRequired.Code.Appointment;
			address.OA_ContainerHandling = OrgConstants.ContainerHandling.Code.Ask;
			address.OA_Dock_Height = OrgConstants.DockHeight.Code.NonStandard;
			address.OA_LabourRequired = OrgConstants.LabourRequired.Code.Ask;

			return new AddressWrapper(organisation.MainAddress, ContactType.All, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new AddressWrapper(null, ContactType.All, Factory);
		}

		#region Implementation

		void SetupAdditionalAddressProperties(OrgAddress address)
		{
			address.OA_LoadingUnloadingConstraints = "Further Constraints";
			address.OA_OtherWarehouseFacilities = "Other Warehouse Facilities";
			address.OA_AccessPoint = OrgConstants.AccessPoint.Code.Dock;
			address.OA_CommunicationRequired = OrgConstants.CommunicationRequired.Code.Appointment;
			address.OA_ContainerHandling = OrgConstants.ContainerHandling.Code.Ask;
			address.OA_Dock_Height = OrgConstants.DockHeight.Code.NonStandard;
			address.OA_LabourRequired = OrgConstants.LabourRequired.Code.Ask;
			address.OA_DockLeveler = true;
			address.OA_PalletJack = true;
			address.OA_ForkLift = true;
		}

		void AssertValueChangedByLanguage(string rawValue, string translatedValue, Func<string> getValue)
		{
			using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.ChineseSimplified))
			{
				AssertEquals(translatedValue, getValue());
			}

			AssertEquals(rawValue, getValue());
		}

		#endregion
	}
}
