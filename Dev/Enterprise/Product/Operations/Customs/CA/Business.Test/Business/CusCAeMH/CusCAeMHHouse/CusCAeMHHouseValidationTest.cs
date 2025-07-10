using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHHouseValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateBW_IsMasterHouse()
		{
			var poc = Factory.New<OrgHeader>();
			poc.OH_Code = "MASPOC";
			poc.OH_FullName = "Master POC";
			var pocAddress = poc.Addresses.AddNew();
			pocAddress.OA_Address1 = "Master POC Address";
			Factory.Save();

			var validation = house.Validation;
			validation.ValidateAll();
			AssertNoNotifications(house.BW_IsMasterHouseInfo);

			var housePOC = house.DocAddresses.AddNew();
			housePOC.E2_AddressType = DocAddressTypes.Codes.PlaceOfConsolidation;
			housePOC.E2_Address1 = "House POC Address";
			housePOC.E2_CompanyName = "House POC";
			var houseCON = house.DocAddresses.AddNew();
			houseCON.E2_AddressType = DocAddressTypes.Codes.Consolidator;
			houseCON.E2_Address1 = "House CON Address";
			houseCON.E2_CompanyName = "House CON";
			validation.ValidateAll();
			AssertHasWarning(house.BW_IsMasterHouseInfo, "Place of Consolidation will not be sent when Consolidation is not ticked Yes.");
			AssertHasWarning(house.BW_IsMasterHouseInfo, "Consolidator Address will not be sent when Consolidation is not ticked Yes.");

			house.BW_IsMasterHouse = true;
			validation.ValidateAll();
			AssertNoNotifications(house.BW_IsMasterHouseInfo);

			house.DocAddresses.RemoveAndDeleteAll();
			validation.ValidateAll();
			AssertHasMessageError(house.BW_IsMasterHouseInfo, "Place of Consolidation is required when Consolidation is marked Yes.");

			house.MasterBill.PlaceOfConsolidation.OrganisationPK = poc.PK;
			validation.ValidateAll();
			AssertNoNotifications(house.BW_IsMasterHouseInfo);

			house.BW_IsMasterHouse = false;
			validation.ValidateAll();
			AssertNoNotifications(house.BW_IsMasterHouseInfo);
		}

		public void TestAddressesOnCusCAeMHHouseValidation()
		{
			house.Validation.ValidateAll();
			house.BW_IsMasterHouse = true;
			house.Validation.ValidateAll();
			AssertHasRowMessageError(house, CusCAeMHHouseValidation.YouMustEnterAnAddress("Consignee"));
			AssertHasRowMessageError(house, CusCAeMHHouseValidation.YouMustEnterAnAddress("Shipper"));

			house.BW_IsMasterHouse = true;
			var consignee = house.DocAddresses.AddNew();
			consignee.E2_AddressType = DocAddressTypes.Codes.ConsigneeDocumentaryAddress;
			var shipper = house.DocAddresses.AddNew();
			shipper.E2_AddressType = DocAddressTypes.Codes.ConsignorDocumentaryAddress;
			consignee.OrganisationPK = Factory.New<OrgHeader>().PK;
			shipper.OrganisationPK = Factory.New<OrgHeader>().PK;
			house.Validation.ValidateAll();
			AssertNoRowMessageError(house, CusCAeMHHouseValidation.YouMustEnterAnAddress("Consignee"));
			AssertNoRowMessageError(house, CusCAeMHHouseValidation.YouMustEnterAnAddress("Shipper"));
		}

		public void TestValidateEQD_ValidateContainerNumberLength()
		{
			var line = house.Items.AddNew();
			line.BX_Description = "LINE1";

			var master = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var container = master.Containers.AddNew();
			container.BQ_ContainerNumber = "C1";
			house.Pivots.AddNew().BPA_BQ_Container = container.PK;
			house.BW_MessageReference = "XXX123456";
			house.BW_MessageStatus = "AAA";
			house.BW_CustomsStatus = "BBB";

			house.Validation.ValidateAll();
			AssertHasRowMessageError(house, CusCAeMHHouseValidation.ContainerIdentifierLessThanMaxLimt);
			container.BQ_ContainerNumber = "C123456789";
			house.Validation.ValidateAll();
			AssertNoRowMessageError(house, CusCAeMHHouseValidation.ContainerIdentifierLessThanMaxLimt);
			container.BQ_ContainerNumber = "C1616161616161616";
			house.Validation.ValidateAll();
			AssertHasRowMessageError(house, CusCAeMHHouseValidation.ContainerIdentifierLessThanMaxLimt);
		}

		public void TestCountrySpecificForDocumentTypesRequired()
		{
			var consignee = house.DocAddresses.AddNew();
			consignee.E2_AddressType = DocAddressTypes.Codes.ConsigneeDocumentaryAddress;
			var shipper = house.DocAddresses.AddNew();
			shipper.E2_AddressType = DocAddressTypes.Codes.ConsignorDocumentaryAddress;
			var consolidator = house.DocAddresses.AddNew();
			consolidator.E2_AddressType = DocAddressTypes.Codes.Consolidator;
			var delivery = house.DocAddresses.AddNew();
			delivery.E2_AddressType = DocAddressTypes.Codes.ConsigneePickupDeliveryAddress;

			var consigneeOrgHeader = Factory.New<OrgHeader>();
			consigneeOrgHeader.OH_RL_NKClosestPort = "DEVEL";
			consigneeOrgHeader.OH_FullName = "CONSIGNEE COMPANY";
			var shipperOrgHeader = Factory.New<OrgHeader>();
			shipperOrgHeader.OH_RL_NKClosestPort = "CATOR";
			shipperOrgHeader.OH_FullName = "SHIPPER COMPANY";
			var consolidatorOrgHeader = Factory.New<OrgHeader>();
			consolidatorOrgHeader.OH_RL_NKClosestPort = "USPHL";
			consolidatorOrgHeader.OH_FullName = "CONSOLIDATOR COMPANY";
			var deliveryOrgHeader = Factory.New<OrgHeader>();
			deliveryOrgHeader.OH_RL_NKClosestPort = "PRSJU";
			deliveryOrgHeader.OH_FullName = "DELIVERY COMPANY";

			var consigneeOrgAddress = consigneeOrgHeader.Addresses.AddNew();
			consigneeOrgAddress.OA_Address1 = "CONSIGNEE ADDRESS 1";
			consigneeOrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			consigneeOrgAddress.OA_PostCode = "1122334455";
			consigneeOrgAddress.OA_State = "FFFFFFFFFF";
			consigneeOrgAddress.OA_City = "CONSIGNEE CITY";

			var shipperOrgAddress = shipperOrgHeader.Addresses.AddNew();
			shipperOrgAddress.OA_Address1 = "SHIPPER ADDRESS 1";
			shipperOrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			shipperOrgAddress.OA_PostCode = "5556663338";
			shipperOrgAddress.OA_State = "SSSSSSSSSSS";
			shipperOrgAddress.OA_City = "SHIPPER CITY";

			var consolidatorOrgAddress = consolidatorOrgHeader.Addresses.AddNew();
			consolidatorOrgAddress.OA_Address1 = "CONSOLIDATOR ADDRESS 1";
			consolidatorOrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			consolidatorOrgAddress.OA_PostCode = "7778889990";
			consolidatorOrgAddress.OA_State = "AAAAAAAAAA";
			consolidatorOrgAddress.OA_City = "CONSOLIDATOR CITY";

			var deliveryOrgAddress = deliveryOrgHeader.Addresses.AddNew();
			deliveryOrgAddress.OA_Address1 = "DELIVERY ADDRESS 1";
			deliveryOrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			deliveryOrgAddress.OA_PostCode = "0246897531";
			deliveryOrgAddress.OA_State = "ABCDEFGHIJ";
			deliveryOrgAddress.OA_City = "DELIVERY CITY";

			consignee.E2_OA_Address = consigneeOrgAddress.PK;
			consignee.E2_Contact = "Consignee@abc.com";
			shipper.E2_OA_Address = shipperOrgAddress.PK;
			shipper.E2_Contact = "Shipper@abc.com";
			consolidator.E2_OA_Address = consolidatorOrgAddress.PK;
			consolidator.E2_Contact = "Carrier@abc.com";
			delivery.E2_OA_Address = deliveryOrgAddress.PK;
			delivery.E2_Contact = "Delivery@abc.com";
			house.Validation.ValidateAll();

			AssertEquals("DE", Core.Constants.CountryCodes.Germany, consignee.Country.Code);
			AssertNoRowMessageError(house, CusCAeMHHouseValidation.ErrorMessageMakeSureMaxLength("Consignee: Postcode", CusCAeMHHouse.MaxPostCode));
			AssertNoRowMessageError(house, CusCAeMHHouseValidation.ErrorMessageMakeSureMaxLength("Consignee: State", CusCAeMHHouse.MaxState));

			AssertEquals("CA", Core.Constants.CountryCodes.Canada, shipper.Country.Code);
			AssertHasRowMessageError(house, CusCAeMHHouseValidation.ErrorMessageMakeSureMaxLength("Shipper: Postcode", CusCAeMHHouse.MaxPostCode));
			AssertHasRowMessageError(house, CusCAeMHHouseValidation.ErrorMessageMakeSureMaxLength("Shipper: State", CusCAeMHHouse.MaxState));

			AssertEquals("US", Core.Constants.CountryCodes.UnitedStates, consolidator.Country.Code);
			AssertHasRowMessageError(house, CusCAeMHHouseValidation.ErrorMessageMakeSureMaxLength("Consolidator Address: Postcode", CusCAeMHHouse.MaxPostCode));
			AssertHasRowMessageError(house, CusCAeMHHouseValidation.ErrorMessageMakeSureMaxLength("Consolidator Address: State", CusCAeMHHouse.MaxState));

			AssertEquals("PR", Core.Constants.CountryCodes.PuertoRico, delivery.Country.Code);
			AssertNoRowMessageError(house, CusCAeMHHouseValidation.ErrorMessageMakeSureMaxLength("Delivery Address: Postcode", CusCAeMHHouse.MaxPostCode));
			AssertNoRowMessageError(house, CusCAeMHHouseValidation.ErrorMessageMakeSureMaxLength("Delivery Address: State", CusCAeMHHouse.MaxState));
		}

		public void TestMandatoryMessageBlocks()
		{
			var consignee = house.DocAddresses.AddNew();
			consignee.E2_AddressType = DocAddressTypes.Codes.ConsigneeDocumentaryAddress;
			var shipper = house.DocAddresses.AddNew();
			shipper.E2_AddressType = DocAddressTypes.Codes.ConsignorDocumentaryAddress;
			var placeOfConsolidation = house.DocAddresses.AddNew();
			placeOfConsolidation.E2_AddressType = DocAddressTypes.Codes.PlaceOfConsolidation;
			var notifyParty = house.DocAddresses.AddNew();
			notifyParty.E2_AddressType = DocAddressTypes.Codes.NotifyParty;
			var deliveryAddress = house.DocAddresses.AddNew();
			deliveryAddress.E2_AddressType = DocAddressTypes.Codes.ConsigneePickupDeliveryAddress;

			consignee.OrganisationPK = Factory.New<OrgHeader>().PK;
			shipper.OrganisationPK = Factory.New<OrgHeader>().PK;
			notifyParty.OrganisationPK = Factory.New<OrgHeader>().PK;
			deliveryAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			placeOfConsolidation.OrganisationPK = Factory.New<OrgHeader>().PK;
			house.Validation.ValidateAll();

			consignee.Organisation.OH_FullName = "CONSIGNEE";
			shipper.Organisation.OH_FullName = "SHIPPER";
			notifyParty.Organisation.OH_FullName = "NOTIFYPARTY";
			deliveryAddress.Organisation.OH_FullName = "DELIVERYPARTY";
			placeOfConsolidation.Organisation.OH_FullName = "PLACEOFCONSOLIDATION";
			house.Validation.ValidateAll();

			AssertNoRowMessageError(house, CusCAeMHHouseValidation.ErrorMessageFieldMandatory("Consignee"));
			AssertNoRowMessageError(house, CusCAeMHHouseValidation.ErrorMessageFieldMandatory("Shipper"));
			AssertNoRowMessageError(house, CusCAeMHHouseValidation.ErrorMessageFieldMandatory("Delivery Address"));
			AssertNoRowMessageError(house, CusCAeMHHouseValidation.ErrorMessageFieldMandatory("Notify Party"));
			AssertNoRowMessageError(house, CusCAeMHHouseValidation.ErrorMessageFieldMandatory("Place of Consolidation"));

			consignee.E2_Contact = "AAAAAAAAAABBBBBBBBBBBCCCCCCCCCCDDDDDDDDDDEEEEEEEEEEGGGGGGGGGGFFFFFFFFFFXXX";
			shipper.E2_Contact = "SSSSSSSSSSBBBBBBBBBBBCCCCCCCCCCDDDDDDDDDDEEEEEEEEEEGGGGGGGGGGFFFFFFFFFFXXX";
			notifyParty.E2_Contact = "NNNNNNNNNNNBBBBBBBBBBBCCCCCCCCCCDDDDDDDDDDEEEEEEEEEEGGGGGGGGGGFFFFFFFFFFXXX";
			placeOfConsolidation.E2_Contact = "CCCCCCCCCCBBBBBBBBBBBCCCCCCCCCCDDDDDDDDDDEEEEEEEEEEGGGGGGGGGGFFFFFFFFFFXXX";
			deliveryAddress.E2_Contact = "DDDDDDDDDDBBBBBBBBBBBCCCCCCCCCCDDDDDDDDDDEEEEEEEEEEGGGGGGGGGGFFFFFFFFFFXXX";
			house.Validation.ValidateAll();

			AssertHasRowMessageError(house, CusCAeMHHouseValidation.ErrorMessageMakeSureMaxLength("Consignee: Contact Name", CusCAeMHHouse.MaxContactName));
			AssertHasRowMessageError(house, CusCAeMHHouseValidation.ErrorMessageMakeSureMaxLength("Shipper: Contact Name", CusCAeMHHouse.MaxContactName));
			AssertHasRowMessageError(house, CusCAeMHHouseValidation.ErrorMessageMakeSureMaxLength("Delivery Address: Contact Name", CusCAeMHHouse.MaxContactName));
			AssertHasRowMessageError(house, CusCAeMHHouseValidation.ErrorMessageMakeSureMaxLength("Notify Party: Contact Name", CusCAeMHHouse.MaxContactName));
			AssertHasRowMessageError(house, CusCAeMHHouseValidation.ErrorMessageMakeSureMaxLength("Place of Consolidation: Contact Name", CusCAeMHHouse.MaxContactName));

			consignee.E2_Contact = "A";
			shipper.E2_Contact = "S";
			notifyParty.E2_Contact = "N";
			placeOfConsolidation.E2_Contact = "C";
			deliveryAddress.E2_Contact = "D";
			house.Validation.ValidateAll();

			AssertNoRowMessageError(house, CusCAeMHHouseValidation.ErrorMessageMakeSureMaxLength("Consignee", CusCAeMHHouse.MaxContactName));
			AssertNoRowMessageError(house, CusCAeMHHouseValidation.ErrorMessageMakeSureMaxLength("Shipper", CusCAeMHHouse.MaxContactName));
			AssertNoRowMessageError(house, CusCAeMHHouseValidation.ErrorMessageMakeSureMaxLength("Delivery Address: Contact Name", CusCAeMHHouse.MaxContactName));
			AssertNoRowMessageError(house, CusCAeMHHouseValidation.ErrorMessageMakeSureMaxLength("Notify Party: Contact Name", CusCAeMHHouse.MaxContactName));
			AssertNoRowMessageError(house, CusCAeMHHouseValidation.ErrorMessageMakeSureMaxLength("Place of Consolidation: Contact Name", CusCAeMHHouse.MaxContactName));

			consignee.Organisation.OH_FullName = "AAAAAAAAAABBBBBBBBBBBCCCCCCCCCCDDDDDDDDDDEEEEEEEEEEGGGGGGGGGGFFFFFFFFFFXXX";
			shipper.Organisation.OH_FullName = "SSSSSSSSSSVVVVVVVVVVCCCCCCCCCCDDDDDDDDDDEEEEEEEEEEGGGGGGGGGGFFFFFFFFFFXXX";
			notifyParty.Organisation.OH_FullName = "SSSSSSSSSSNNNNNNNNNNCCCCCCCCCCDDDDDDDDDDEEEEEEEEEEGGGGGGGGGGFFFFFFFFFFXXX";
			deliveryAddress.Organisation.OH_FullName = "SSSSSSSSSSDDDDDDDDDDCCCCCCCCCCDDDDDDDDDDEEEEEEEEEEGGGGGGGGGGFFFFFFFFFFXXX";
			placeOfConsolidation.Organisation.OH_FullName = "SSSSSSSSSSPPPPPPPPPPCCCCCCCCCCDDDDDDDDDDEEEEEEEEEEGGGGGGGGGGFFFFFFFFFFXXX";
			house.Validation.ValidateAll();

			AssertHasRowMessageError(house, "The length of data entered into Consignee: Company Name exceeds the maximum allowed. Only the first 70 characters will be transmitted to Customs");
			AssertHasRowMessageError(house, "The length of data entered into Shipper: Company Name exceeds the maximum allowed. Only the first 70 characters will be transmitted to Customs");
			AssertHasRowMessageError(house, "The length of data entered into Delivery Address: Company Name exceeds the maximum allowed. Only the first 70 characters will be transmitted to Customs");
			AssertHasRowMessageError(house, "The length of data entered into Notify Party: Company Name exceeds the maximum allowed. Only the first 70 characters will be transmitted to Customs");
			AssertHasRowMessageError(house, "The length of data entered into Place of Consolidation: Company Name exceeds the maximum allowed. Only the first 70 characters will be transmitted to Customs");

			consignee.Organisation.OH_FullName = "A";
			shipper.Organisation.OH_FullName = "S";
			notifyParty.Organisation.OH_FullName = "S";
			deliveryAddress.Organisation.OH_FullName = "S";
			placeOfConsolidation.Organisation.OH_FullName = "S";
			house.Validation.ValidateAll();
			AssertNoRowMessageError(house, "The length of data entered into Consignee: Company Name exceeds the maximum allowed. Only the first 70 characters will be transmitted to Customs");
			AssertNoRowMessageError(house, "The length of data entered into Shipper: Company Name exceeds the maximum allowed. Only the first 70 characters will be transmitted to Customs");
			AssertNoRowMessageError(house, "The length of data entered into Delivery Address: Company Name exceeds the maximum allowed. Only the first 70 characters will be transmitted to Customs");
			AssertNoRowMessageError(house, "The length of data entered into Notify Party: Company Name exceeds the maximum allowed. Only the first 70 characters will be transmitted to Customs");
			AssertNoRowMessageError(house, "The length of data entered into Place of Consolidation: Company Name exceeds the maximum allowed. Only the first 70 characters will be transmitted to Customs");
		}

		public void TestOrgAddressesWhenSendMessage()
		{
			house.Validation.ValidateAll();
			house.BW_IsMasterHouse = false;
			AssertHasRowMessageError(house, CusCAeMHHouseValidation.YouMustEnterAnAddress("Consignee"));
			AssertHasRowMessageError(house, CusCAeMHHouseValidation.YouMustEnterAnAddress("Shipper"));

			var org01 = Factory.New<OrgHeader>();
			var org02 = Factory.New<OrgHeader>();
			var orgAddress01 = org01.Addresses.AddNew();
			var orgAddress02 = org02.Addresses.AddNew();
			var consignee = house.DocAddresses.AddNew();
			var shipper = house.DocAddresses.AddNew();
			consignee.E2_AddressType = DocAddressTypes.Codes.ConsigneeDocumentaryAddress;
			shipper.E2_AddressType = DocAddressTypes.Codes.ConsignorDocumentaryAddress;
			consignee.OrganisationPK = org01.PK;
			shipper.OrganisationPK = org02.PK;
			consignee.E2_OA_Address = orgAddress01.PK;
			shipper.E2_OA_Address = orgAddress02.PK;
			orgAddress01.OA_RN_NKCountryCode = string.Empty;
			orgAddress02.OA_RN_NKCountryCode = string.Empty;
			house.Validation.ValidateAll();

			AssertNoRowMessageError(house, CusCAeMHHouseValidation.YouMustEnterAnAddress("Consignee"));
			AssertNoRowMessageError(house, CusCAeMHHouseValidation.YouMustEnterAnAddress("Shipper"));

			AssertHasRowMessageError(house, "Consignee: City is mandatory when you send a message.");
			AssertHasRowMessageError(house, "Consignee: Country/Region Code is mandatory when you send a message.");
			AssertHasRowMessageError(house, "Shipper: City is mandatory when you send a message.");
			AssertHasRowMessageError(house, "Shipper: Country/Region Code is mandatory when you send a message.");

			org01.OH_FullName = "CONSIGNEE COMPANY";
			org02.OH_FullName = "SHIPPER COMPANY";
			orgAddress01.OA_Address1 = "CONSIGNEE ADDRESS 1";
			orgAddress02.OA_Address1 = "CONSIGNEE ADDRESS 1";
			orgAddress01.OA_City = "CONSIGNEE CITY";
			orgAddress02.OA_City = "SHIPPER CITY";
			orgAddress01.OA_RN_NKCountryCode = "ZZ";
			orgAddress02.OA_RN_NKCountryCode = "ZZ";
			consignee.E2_Contact = "Consignee@abc.com";
			shipper.E2_Contact = "Shipper@abc.com";
			house.Validation.ValidateAll();

			AssertNoRowMessageError(house, "Consignee: City is mandatory when you send a message.");
			AssertNoRowMessageError(house, "Consignee: Country/Region Code is mandatory when you send a message.");
			AssertNoRowMessageError(house, "Shipper: City is mandatory when you send a message.");
			AssertNoRowMessageError(house, "Shipper: Country/Region Code is mandatory when you send a message.");

			var org03 = Factory.New<OrgHeader>();
			var org04 = Factory.New<OrgHeader>();
			var org05 = Factory.New<OrgHeader>();
			var orgAddress03 = org03.Addresses.AddNew();
			var orgAddress04 = org04.Addresses.AddNew();
			var orgAddress05 = org05.Addresses.AddNew();
			var consolidator = house.DocAddresses.AddNew();
			var notifyParty = house.DocAddresses.AddNew();
			var placeOfConsolidation = house.DocAddresses.AddNew();
			consolidator.E2_AddressType = DocAddressTypes.Codes.Consolidator;
			notifyParty.E2_AddressType = DocAddressTypes.Codes.NotifyParty;
			placeOfConsolidation.E2_AddressType = DocAddressTypes.Codes.PlaceOfConsolidation;
			consolidator.OrganisationPK = org03.PK;
			notifyParty.OrganisationPK = org04.PK;
			placeOfConsolidation.OrganisationPK = org05.PK;
			consolidator.E2_OA_Address = orgAddress03.PK;
			notifyParty.E2_OA_Address = orgAddress04.PK;
			placeOfConsolidation.E2_OA_Address = orgAddress05.PK;
			orgAddress03.OA_RN_NKCountryCode = string.Empty;
			orgAddress04.OA_RN_NKCountryCode = string.Empty;
			orgAddress05.OA_RN_NKCountryCode = string.Empty;
			house.Validation.ValidateAll();

			AssertNoRowMessageError(house, "Consolidator: City is mandatory when you send a message.");
			AssertNoRowMessageError(house, "Consolidator: Country/Region Code is mandatory when you send a message.");
			AssertNoRowMessageError(house, "Notify Party: City is mandatory when you send a message.");
			AssertNoRowMessageError(house, "Notify Party: Country/Region Code is mandatory when you send a message.");
			AssertNoRowMessageError(house, "Place of Consolidation: City is mandatory when you send a message.");
			AssertNoRowMessageError(house, "Place of Consolidation: Country/Region Code is mandatory when you send a message.");

			house.BW_IsMasterHouse = true;
			house.Validation.ValidateAll();

			AssertHasRowMessageError(house, "Place of Consolidation: City is mandatory when you send a message.");
			AssertHasRowMessageError(house, "Place of Consolidation: Country/Region Code is mandatory when you send a message.");
		}

		public void TestDocAddressesE2_CompanyNameWhenSendMessage()
		{
			AssertCheckCompanyNameAfterReplacement(DocAddressTypes.Codes.ConsigneeDocumentaryAddress, "Consignee", "ABC~:@");
			AssertCheckCompanyNameAfterReplacement(DocAddressTypes.Codes.ConsignorDocumentaryAddress, "Shipper", "AB@~");
			AssertCheckCompanyNameAfterReplacement(DocAddressTypes.Codes.ConsigneePickupDeliveryAddress, "Delivery Address", "$");
			AssertCheckCompanyNameAfterReplacement(DocAddressTypes.Codes.NotifyParty, "Notify Party", "V^V");
			AssertCheckCompanyNameAfterReplacement(DocAddressTypes.Codes.Consolidator, "Consolidator Address", "[AB]");
			AssertCheckCompanyNameAfterReplacement(DocAddressTypes.Codes.PlaceOfConsolidation, "Place of Consolidation", "^:^");
		}

		void AssertCheckCompanyNameAfterReplacement(string addressType, string humanReadableName, string companyName)
		{
			var messageError = string.Format("{0}: Company Name are more than 2 illegal characters or the illegal characters represent more than 1/4.", humanReadableName);
			var org = Factory.New<OrgHeader>();
			var orgAddress = org.Addresses.AddNew();
			orgAddress.OA_CompanyNameOverride = "House DOC";
			var docAddress = house.DocAddresses.AddNew();
			docAddress.E2_AddressType = addressType;
			docAddress.OrganisationPK = org.PK;
			docAddress.E2_OA_Address = orgAddress.PK;
			house.Validation.ValidateAll();
			AssertNoRowMessageError(house, messageError);

			orgAddress.OA_CompanyNameOverride = companyName;
			house.Validation.ValidateAll();
			AssertHasRowMessageError(house, messageError);
		}

		public void TestCheckMaxNumberOfUNDG()
		{
			var line01 = house.Items.AddNew();
			var line02 = house.Items.AddNew();
			var line03 = house.Items.AddNew();

			var undg11 = line01.UNDGs.AddNew();
			var undg12 = line01.UNDGs.AddNew();
			var undg13 = line01.UNDGs.AddNew();

			house.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(house, "Please make sure the number of Dangerous(DG) Pack Line Items is less than");

			var undg21 = line02.UNDGs.AddNew();
			var undg22 = line02.UNDGs.AddNew();
			var undg23 = line02.UNDGs.AddNew();

			house.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(house, "Please make sure the number of Dangerous(DG) Pack Line Items is less than");

			var undg31 = line03.UNDGs.AddNew();
			var undg32 = line03.UNDGs.AddNew();
			var undg33 = line03.UNDGs.AddNew();

			house.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(house, "Please make sure the number of Dangerous(DG) Pack Line Items is less than");

			var undg34 = line03.UNDGs.AddNew();
			house.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(house, "Please make sure the number of Dangerous(DG) Pack Line Items is less than");
		}

		public void TestCheckBW_HouseBill()
		{
			house.BW_HouseBill = "1234";
			house.BW_HouseBill = ZString.Empty;
			AssertHasMessageErrorContaining(house.BW_HouseBillInfo, MandatoryValidation.YouHaveNotEntered);

			var master = Factory.New<CusCAeMHMaster>();
			var house1 = master.HouseBills.AddNew();
			var house2 = master.HouseBills.AddNew();

			house1.BW_HouseBill = "2345";
			house2.BW_HouseBill = "2345";
			AssertHasMessageErrorContaining(house2.BW_HouseBillInfo, "Another House Bill has same bill number.");

			house1.BW_HouseBill = "1234";
			house2.Validation.ValidateBW_HouseBill();
			AssertNoMessageErrorContaining(house2.BW_HouseBillInfo, "Another House Bill has same bill number.");
		}

		public void TestCheckBW_HouseCCN()
		{
			Factory.Save();
			house.BW_HouseBill = "1234";
			house.Validation.ValidateBW_HouseCCN();
			AssertHasMessageErrorContaining(house.BW_HouseCCNInfo, MandatoryValidation.YouHaveNotEntered);
			house.BW_HouseCCN = "CCN";
			AssertNoMessageErrorContaining(house.BW_HouseCCNInfo, MandatoryValidation.YouHaveNotEntered);
			var house1 = Factory.New<CusCAeMHMaster>().HouseBills.AddNew();
			house1.BW_HouseCCN = "CCN";
			AssertHasMessageError(house1.BW_HouseCCNInfo, CusCAeMHHouseValidation.CCNMustBeUnique("1234"));

			house.BW_HouseCCN = "8456X";
			AssertNoMessageErrorContaining(house.BW_HouseCCNInfo, "The house CCN should start with the FF Carrier Code");
			house.MasterBill.BP_CBSACarrierCode = "8123";
			house.BW_HouseCCN = "8123";
			AssertHasMessageErrorContaining(house.BW_HouseCCNInfo, "The house CCN should start with the FF Carrier Code");
			house.BW_HouseCCN = "8123X";
			AssertNoMessageErrorContaining(house.BW_HouseCCNInfo, "The house CCN should start with the FF Carrier Code");
			house.BW_HouseCCN = "8456X";
			AssertHasMessageErrorContaining(house.BW_HouseCCNInfo, "The house CCN should start with the FF Carrier Code");

			house.MasterBill.BP_CBSACarrierCode = "801k";
			house.BW_HouseCCN = "801K TPE802559520";
			AssertNoMessageErrorContaining(house.BW_HouseCCNInfo, "The house CCN should start with the FF Carrier Code");
			house.BW_HouseCCN = "801k TPE802559520";
			AssertNoMessageErrorContaining(house.BW_HouseCCNInfo, "The house CCN should start with the FF Carrier Code");

			house.MasterBill.BP_CBSACarrierCode = "801K";
			house.BW_HouseCCN = "801k TPE802559520";
			AssertNoMessageErrorContaining(house.BW_HouseCCNInfo, "The house CCN should start with the FF Carrier Code");
			house.BW_HouseCCN = "801L TPE802559520";
			AssertHasMessageErrorContaining(house.BW_HouseCCNInfo, "The house CCN should start with the FF Carrier Code");

			Factory.Save();
			house.BW_HouseCCN = "8123CCN2";
			AssertNoErrorContaining(house.BW_HouseCCNInfo, "This is a key field");
			house.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Error;
			Factory.Save();
			house.BW_HouseCCN = "8123CCN3";
			AssertHasErrorContaining(house.BW_HouseCCNInfo, "This is a key field");
			house.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Cancelled;
			Factory.Save();
			house.BW_HouseCCN = "8123CCN4";
			AssertNoErrorContaining(house.BW_HouseCCNInfo, "This is a key field");

			Enterprise.Registry.Business.FreightDataRegistry.Instance.CanadaCargoControlNumberCustomization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ShipmentCCNCustomizationTypes.Code.NumberFountain);

			house.BW_HouseCCN = "";
			AssertHasWarningContaining(house.BW_HouseCCNInfo, "System will create a House CCN automatically if it is not entered.");

			Enterprise.Registry.Business.FreightDataRegistry.Instance.CanadaCargoControlNumberCustomization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ShipmentCCNCustomizationTypes.Code.ShipmentNumber);
			house.Validation.ValidateBW_HouseCCN();
			AssertNoWarningContaining(house.BW_HouseCCNInfo, "System will create a House CCN automatically if it is not entered.");
		}

		public void TestCheckBW_MovementType()
		{
			house.BW_MovementType = "XXX";
			AssertHasMessageErrorContaining(house.BW_MovementTypeInfo, ListValidation.InvalidCodeMessageError);
			house.BW_MovementType = ZString.Empty;
			AssertHasMessageErrorContaining(house.BW_MovementTypeInfo, MandatoryValidation.YouHaveNotEntered);
			house.BW_MovementType = eMHMovementTypeList.Codes.Import;
			AssertNoMessageErrorContaining(house.BW_MovementTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(house.BW_MovementTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		[TestDate(2013, 8, 15, 10, 30, 25)]
		public void TestCheckBW_AmendReasonCode()
		{
			house.BW_AmendReasonCode = "XX";
			house.MasterBill.BP_ATA = ZDateTime.Now;

			AssertHasMessageErrorContaining(house.BW_AmendReasonCodeInfo, ListValidation.InvalidCodeMessageError);

			house.BW_AmendReasonCode = EManifestAmendmentReasonCodes.Codes.ClientOutage;

			AssertNoMessageErrorContaining(house.BW_AmendReasonCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(house.BW_AmendReasonCodeInfo, MandatoryValidation.DoNotEntered);

			house.BW_AmendReasonCode = ZString.Empty;

			AssertNoMessageErrorContaining(house.BW_AmendReasonCodeInfo, MandatoryValidation.DoNotEntered);
			AssertNoMessageErrorContaining(house.BW_AmendReasonCodeInfo, "an amendment reason will be required");

			var message = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Empty, contextNodeXml: "<Context><Type>Status</Type><Value>0002</Value></Context><Context><Type>Status</Type><Value>0011</Value></Context>");
			var stmAlog = Factory.New<StmALog>();
			using (stmAlog.LockForUpdatingKeyFieldsForTesting())
			{
				stmAlog.SL_Parent = house.PK;
				stmAlog.SL_Table = house.TableName;
			}
			var genPivot = Factory.New<GenPivot>();
			genPivot.XX_Relation1ID = stmAlog.PK;
			genPivot.XX_Relation2ID = message.PK;
			genPivot.XX_RelationType = Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage;

			Factory.Save();
			house.MessagesForDisplay.Reload(true);

			house.Validation.ValidateBW_AmendReasonCode();

			AssertNoMessageErrorContaining(house.BW_AmendReasonCodeInfo, "an amendment reason will be required");

			house.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Validated;

			house.Validation.ValidateBW_AmendReasonCode();

			AssertHasMessageErrorContaining(house.BW_AmendReasonCodeInfo, "an amendment reason will be required");

			house.BW_AmendReasonCode = EManifestAmendmentReasonCodes.Codes.ClientOutage;

			AssertNoMessageErrorContaining(house.BW_AmendReasonCodeInfo, "an amendment reason will be required");

			house.BW_CustomsStatus = ZString.Empty;
			house.BW_AmendReasonCode = ZString.Empty;
			message = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Empty, contextNodeXml: "<Context><Type>Status</Type><Value>0002</Value></Context><Context><Type>Status</Type><Value>0010</Value></Context>");
			stmAlog = Factory.New<StmALog>();
			using (stmAlog.LockForUpdatingKeyFieldsForTesting())
			{
				stmAlog.SL_Parent = house.PK;
				stmAlog.SL_Table = house.TableName;
			}
			genPivot = Factory.New<GenPivot>();
			genPivot.XX_Relation1ID = stmAlog.PK;
			genPivot.XX_Relation2ID = message.PK;

			Factory.Save();

			house.Validation.ValidateBW_AmendReasonCode();

			AssertNoMessageErrorContaining(house.BW_AmendReasonCodeInfo, "an amendment reason will be required");

			house.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Validated;

			house.Validation.ValidateBW_AmendReasonCode();

			AssertHasMessageErrorContaining(house.BW_AmendReasonCodeInfo, "an amendment reason will be required");

			house.BW_AmendReasonCode = EManifestAmendmentReasonCodes.Codes.ClientOutage;

			AssertNoMessageErrorContaining(house.BW_AmendReasonCodeInfo, "an amendment reason will be required");
		}

		[TestDate(2013, 8, 15, 10, 30, 25)]
		public void TestCheckBW_AmendReasonCode_PortOrSubLocation()
		{
			var messageError = "There is no Arrival message / event or the arrival date present – hence eManifest cannot be amended, if there is any change in information then please submit Change – not Amendment.";
			house.BW_AmendReasonCode = EManifestAmendmentReasonCodes.Codes.PortOrSubLocation;
			AssertHasMessageErrorContaining(house.BW_AmendReasonCodeInfo, messageError);

			house.MasterBill.BP_ATA = ZDateTime.Now;
			house.Validation.ValidateBW_AmendReasonCode();
			AssertNoMessageErrorContaining(house.BW_AmendReasonCodeInfo, messageError);

			house.MasterBill.BP_ATA = ZDateTime.Empty;
			house.BW_AmendReasonCode = EManifestAmendmentReasonCodes.Codes.ClientOutage;
			AssertHasMessageErrorContaining(house.BW_AmendReasonCodeInfo, messageError);

			house.BW_AmendReasonCode = ZString.Empty;
			AssertNoMessageErrorContaining(house.BW_AmendReasonCodeInfo, messageError);
		}

		public void TestCheckBW_Weight()
		{
			house.BW_Weight = -1m;
			AssertHasErrorContaining(house.BW_WeightInfo, MandatoryValidation.ValueCannotBeNegative);
			house.BW_Weight = ZDecimal.Zero;
			AssertNoErrorContaining(house.BW_WeightInfo, MandatoryValidation.ValueCannotBeNegative);
			AssertHasMessageError(house.BW_WeightInfo, "Weight is mandatory for eManifest filings.");
			house.BW_Weight = 1m;
			AssertNoMessageError(house.BW_WeightInfo, "Weight is mandatory for eManifest filings.");
		}

		public void TestCheckBW_WeightUQ()
		{
			house.BW_WeightUQ = "XX";
			AssertHasMessageErrorContaining(house.BW_WeightUQInfo, ListValidation.InvalidCodeMessageError);
			house.BW_WeightUQ = CanadianUnitOfWeightList.Codes.Kilogram;
			AssertNoMessageErrorContaining(house.BW_WeightUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBW_WeightUQ2()
		{
			house.BW_Weight = 100m;
			house.BW_WeightUQ = string.Empty;
			AssertHasMessageError(house.BW_WeightUQInfo, "You have not entered Weight UQ.");
			house.BW_WeightUQ = CanadianUnitOfWeightList.Codes.Kilogram;
			AssertNoMessageError(house.BW_WeightUQInfo, "You have not entered Weight UQ.");
		}

		public void TestCheckBW_VolumeUQ()
		{
			house.BW_Volume = 1m;
			house.BW_VolumeUQ = "XX";
			AssertHasMessageErrorContaining(house.BW_VolumeUQInfo, ListValidation.InvalidCodeMessageError);
			house.BW_VolumeUQ = CustomsUnitOfMeasureList.Codes.CubicMillimetre;
			AssertNoMessageErrorContaining(house.BW_VolumeUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBW_VolumeUQ2()
		{
			house.BW_Volume = 1m;
			house.BW_VolumeUQ = string.Empty;
			AssertHasMessageError(house.BW_VolumeUQInfo, "You have not entered Volume UQ.");
			house.BW_VolumeUQ = CustomsUnitOfMeasureList.Codes.CubicMillimetre;
			AssertNoMessageError(house.BW_VolumeUQInfo, "You have not entered Volume UQ.");
		}

		public void TestCheckBW_CBSAReleasePort()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "YYY", "YYY", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			house.BW_CBSAReleasePort = ZString.Empty;
			AssertHasMessageErrorContaining(house.BW_CBSAReleasePortInfo, MandatoryValidation.YouHaveNotEntered);

			house.BW_CBSAReleasePortInfo.ClearValue();
			house.BW_CBSAReleasePort = "XXX";
			AssertNoMessageErrorContaining(house.BW_CBSAReleasePortInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(house.BW_CBSAReleasePortInfo, ListValidation.InvalidCodeMessageError);

			house.BW_CBSAReleasePortInfo.ClearValue();
			house.BW_CBSAReleasePort = "YYY";
			AssertNoMessageErrorContaining(house.BW_CBSAReleasePortInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBW_CBSAReleaseSubLocation()
		{
			var loc = CACSubLocationTest.CreateSubLocation(Factory, "YYY");
			Factory.Save();

			house.BW_CBSAReleaseSubLocation = ZString.Empty;
			AssertHasMessageErrorContaining(house.BW_CBSAReleaseSubLocationInfo, MandatoryValidation.YouHaveNotEntered);

			house.BW_CBSAReleaseSubLocationInfo.ClearValue();
			house.BW_CBSAReleaseSubLocation = "XXX";
			AssertNoMessageErrorContaining(house.BW_CBSAReleaseSubLocationInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(house.BW_CBSAReleaseSubLocationInfo, ListValidation.InvalidCodeMessageError);

			house.BW_CBSAReleaseSubLocationInfo.ClearValue();
			house.BW_CBSAReleaseSubLocation = loc.Code;
			AssertNoMessageErrorContaining(house.BW_CBSAReleaseSubLocationInfo, ListValidation.InvalidCodeMessageError);
		}

		CusCAeMHHouse house;
		protected override void SetUp()
		{
			base.SetUp();
			house = Factory.New<CusCAeMHMaster>().HouseBills.AddNew();

			Enterprise.ZArchitecture.Core.Testing.FountainTestListener.AddFountainAccess("CAHouseBilleManifest", Guid.Empty);
			Enterprise.ZArchitecture.Core.Testing.FountainTestListener.AddFountainAccess("CAMasterBilleManifest", Guid.Empty);
		}
	}
}
