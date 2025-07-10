using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ExportJobDocAddressValidationTest : TestCaseWithFactory
	{
		public void TestCheckOrganisationPK()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			vendorOrg.OH_FullName = "123ABC";
			vendorOrg.MainAddress.Address1 = "123ABC";

			declaration.WarehouseDocAddress.OrganisationPK = vendorOrg.PK;
			declaration.VendorDocAddress.OrganisationPK = vendorOrg.PK;

			declaration.WarehouseDocAddress.Validation.ValidateOrganisationPK();
			declaration.VendorDocAddress.Validation.ValidateOrganisationPK();
			AssertNoWarning(declaration.WarehouseDocAddress.OrganisationPKInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertNoWarning(declaration.WarehouseDocAddress.OrganisationPKInfo, "The Address of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertNoWarning(declaration.VendorDocAddress.OrganisationPKInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertNoWarning(declaration.VendorDocAddress.OrganisationPKInfo, "The Address of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");

			vendorOrg.OH_FullName = "123ABC– ";
			vendorOrg.MainAddress.Address1 = "123ABC– ";
			declaration.WarehouseDocAddress.Validation.ValidateOrganisationPK();
			declaration.VendorDocAddress.Validation.ValidateOrganisationPK();
			AssertHasWarning(declaration.WarehouseDocAddress.OrganisationPKInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertHasWarning(declaration.WarehouseDocAddress.OrganisationPKInfo, "The Address of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertHasWarning(declaration.VendorDocAddress.OrganisationPKInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertHasWarning(declaration.VendorDocAddress.OrganisationPKInfo, "The Address of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
		}

		public void TestVendorName()
		{
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoNotifications(declaration.VendorDocAddress.E2_OA_AddressInfo);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoNotifications(declaration.VendorDocAddress.E2_OA_AddressInfo);
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, "You have not entered a name");
			vendorOrg.OH_FullName = "NAME";
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, "You have not entered a name");
		}

		public void TestVendorAddressLine1()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, "You have not entered an address line 1");
			vendorAddress.OA_Address1 = "123456789012345678901234567890123456";
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, "You have not entered an address line 1");
			AssertHasWarningContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, "The length of data entered into address line 1 (36) exceeds the maximum allowed");
			vendorAddress.OA_Address1 = "12345678901234567890123456789012345";
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, "The length of data entered into address line 1 (36) exceeds the maximum allowed");
		}

		public void TestVendorAddressLine2()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			vendorAddress.OA_Address2 = "123456789012345678901234567890123456";
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasWarningContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, "The length of data entered into address line 2 (36) exceeds the maximum allowed");
			vendorAddress.OA_Address2 = "12345678901234567890123456789012345";
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, "The length of data entered into address line 2 (36) exceeds the maximum allowed");
		}

		public void TestVendorCity()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, "You have not entered a city");
			vendorAddress.OA_City = "1234567890123456789012345";
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, "You have not entered a city");
		}

		public void TestVendorProvinceState()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			vendorOrg.OH_RL_NKClosestPort = "AUSYD";
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			var messageError = CAAddressValidator.ProvinceStateIsRequiredCountryCanadaOrUnitedStates.TrimEnd('.');
			AssertNoMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, messageError);
			vendorOrg.OH_RL_NKClosestPort = "CABLO";
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, messageError);
			vendorAddress.OA_State = "1234567890";
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, messageError);
			AssertHasWarningContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, "The length of data entered into province / state (10) exceeds the maximum allowed");
			vendorAddress.OA_State = "123456789";
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, "The length of data entered into province / state (10) exceeds the maximum allowed");
			AssertHasMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, CAAddressValidator.invalidCAProvince);
			vendorAddress.OA_State = "ON";
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, CAAddressValidator.invalidCAProvince);
			vendorOrg.OH_RL_NKClosestPort = "USNYK";
			vendorAddress.OA_State = "";
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, messageError);
			vendorAddress.OA_State = "123456789";
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, messageError);
			AssertHasMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, CAAddressValidator.invalidUSState);
			vendorAddress.OA_State = "NY";
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, CAAddressValidator.invalidUSState);
		}

		public void TestVendorPostCode()
		{
			var exporterPostCodeIsRequiredCountryCanadaOrUnitedStates = CAAddressValidator.ExporterPostCodeIsRequiredCountryCanadaOrUnitedStates.TrimEnd('.');
			var canadianExporterPostCodeFormat = CAAddressValidator.CanadianExporterPostCodeFormat.TrimEnd('.');

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			vendorOrg.OH_RL_NKClosestPort = "AUSYD";
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, exporterPostCodeIsRequiredCountryCanadaOrUnitedStates);
			vendorOrg.OH_RL_NKClosestPort = "CABLO";
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, exporterPostCodeIsRequiredCountryCanadaOrUnitedStates);
			vendorAddress.OA_PostCode = "1234567890";
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, exporterPostCodeIsRequiredCountryCanadaOrUnitedStates);
			AssertHasWarningContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, "The length of data entered into postal / zip code (10) exceeds the maximum allowed");
			vendorAddress.OA_PostCode = "123456789";
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, "The length of data entered into postal / zip code (10) exceeds the maximum allowed");
			AssertHasMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, canadianExporterPostCodeFormat);
			vendorAddress.OA_PostCode = "A2B2C2";
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, canadianExporterPostCodeFormat);
			vendorOrg.OH_RL_NKClosestPort = "USNYK";
			vendorAddress.OA_PostCode = "";
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, exporterPostCodeIsRequiredCountryCanadaOrUnitedStates);
			vendorAddress.OA_PostCode = "123456";
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, exporterPostCodeIsRequiredCountryCanadaOrUnitedStates);
			AssertNoMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, canadianExporterPostCodeFormat);
			vendorOrg.OH_RL_NKClosestPort = "AUMEL";
			vendorAddress.OA_PostCode = "";
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, exporterPostCodeIsRequiredCountryCanadaOrUnitedStates);
			vendorAddress.OA_PostCode = "3000";
			declaration.VendorDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, exporterPostCodeIsRequiredCountryCanadaOrUnitedStates);
			AssertNoMessageErrorContaining(declaration.VendorDocAddress.E2_OA_AddressInfo, canadianExporterPostCodeFormat);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			using (declaration.GetValidationSuspender())
			{
				vendorAddress = Factory.New<OrgAddress>();
				vendorOrg = Factory.New<OrgHeader>();
				vendorAddress.OA_OH = vendorOrg.PK;
				vendorAddress.OA_RN_NKCountryCode = "";
				declaration.VendorDocAddress.E2_OA_Address = vendorAddress.PK;
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		JobDeclaration declaration;
		OrgAddress vendorAddress;
		OrgHeader vendorOrg;
	}
}
