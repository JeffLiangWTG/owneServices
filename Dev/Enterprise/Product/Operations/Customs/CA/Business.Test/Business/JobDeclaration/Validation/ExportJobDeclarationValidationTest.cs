using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ExportJobDeclarationValidationTest : JobDeclarationValidationTest
	{
		public void TestCheckJE_TransportMode()
		{
			AssertEquals("PreCondition: Container Count", 0, declaration.CusContainers.Count);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertHasMessageError(declaration.JE_TransportModeInfo, ExportJobDeclarationValidation.ContainerIsRequiredForSea);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertNoMessageError(declaration.JE_TransportModeInfo, ExportJobDeclarationValidation.ContainerIsRequiredForSea);

			declaration.CusContainers.AddNew();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertNoMessageError(declaration.JE_TransportModeInfo, ExportJobDeclarationValidation.ContainerIsRequiredForSea);
		}

		public void TestCheckJE_TransportModeAndJE_ContainerMode()
		{
			AssertEquals("PreCondition: Container Count", 0, declaration.CusContainers.Count);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertHasMessageError(declaration.JE_TransportModeInfo, ExportJobDeclarationValidation.ContainerIsRequiredForSea);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			AssertNoMessageError(declaration.JE_TransportModeInfo, ExportJobDeclarationValidation.ContainerIsRequiredForSea);
		}

		public void TestCheckJE_OA_ImporterAddress()
		{
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory, false);
			string messageError = MandatoryValidation.YouHaveNotEntered + " a Consignee.";
			declaration.JE_OA_ImporterAddress = helper.Consignee.MainAddress.PK;
			AssertNoMessageError(declaration.JE_OA_ImporterAddressInfo, messageError);

			declaration.JE_OA_ImporterAddress = ZGuid.Empty;
			AssertHasMessageError(declaration.JE_OA_ImporterAddressInfo, messageError);

			var importer = Factory.New<OrgHeader>();
			var address = importer.Addresses.AddNew();
			importer.OH_FullName = "123ABC";
			address.OA_City = ZString.Empty;
			address.OA_RN_NKCountryCode = ZString.Empty;
			declaration.JE_OA_ImporterAddress = address.PK;
			AssertNoWarning(declaration.JE_OA_ImporterAddressInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertHasMessageError(declaration.JE_OA_ImporterAddressInfo, OrganisationValidation.AddressLackOfCity);
			AssertHasMessageError(declaration.JE_OA_ImporterAddressInfo, OrganisationValidation.AddressHasInvalidCountryCode);

			importer.OH_FullName = "123ABC– ";
			address.OA_City = "CITY";
			address.OA_RN_NKCountryCode = "CA";
			declaration.Validation.ValidateJE_OA_ImporterAddress();
			AssertHasWarning(declaration.JE_OA_ImporterAddressInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertNoMessageError(declaration.JE_OA_ImporterAddressInfo, OrganisationValidation.AddressLackOfCity);
			AssertNoMessageError(declaration.JE_OA_ImporterAddressInfo, OrganisationValidation.AddressHasInvalidCountryCode);
		}

		public void TestCheckJE_RL_NKPortOfFirstArrival()
		{
			declaration.JE_RL_NKPortOfFirstArrival = ZString.Empty;
			AssertNoNotifications(declaration.JE_RL_NKPortOfFirstArrivalInfo);

			declaration.JE_RL_NKPortOfFirstArrival = "ZZZZZ";
			AssertNoNotifications(declaration.JE_RL_NKPortOfFirstArrivalInfo);

			declaration.JE_RL_NKPortOfFirstArrival = new DeclarationTestHelper(Factory, false).AUSYD.Code;
			AssertNoNotifications(declaration.JE_RL_NKPortOfFirstArrivalInfo);
		}

		public void TestCheckJE_TotalWeightUnit()
		{
			string messageError = MandatoryValidation.YouHaveNotEntered + " a Total Weight Unit.";
			declaration.JE_TotalWeightUnit = ZString.Empty;
			AssertHasMessageError(declaration.JE_TotalWeightUnitInfo, messageError);

			declaration.JE_TotalWeightUnit = UnitOfWeightList.Codes.Kiloton;
			AssertNoMessageError(declaration.JE_TotalWeightUnitInfo, messageError);
		}

		public void TestCheckJE_VesselName()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
			string messageError = MandatoryValidation.YouHaveNotEntered + " a Vessel; a Vessel is required when the transport mode is sea.";
			declaration.JE_VesselName = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.JE_VesselNameInfo, messageError);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_VesselName = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_VesselNameInfo, messageError);

			declaration.JE_VesselName = "APL EMERALD";
			AssertNoMessageErrorContaining(declaration.JE_VesselNameInfo, messageError);
		}

		public void TestCheckJE_RL_NKFinalDestination()
		{
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory, false);
			declaration.JE_RL_NKFinalDestination = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_RL_NKFinalDestinationInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.JE_RL_NKFinalDestinationInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_RL_NKFinalDestination = "ZDSD@";
			AssertNoMessageErrorContaining(declaration.JE_RL_NKFinalDestinationInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.JE_RL_NKFinalDestinationInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_RL_NKFinalDestination = helper.USLAX.RL_Code;
			AssertNoMessageErrorContaining(declaration.JE_RL_NKFinalDestinationInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.JE_RL_NKFinalDestinationInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJE_ExportDate()
		{
			declaration.JE_ExportDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(declaration.JE_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_ExportDate = ZDateTime.Now;
			AssertNoMessageErrorContaining(declaration.JE_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_TotalNoOfPacks()
		{
			declaration.JE_TotalNoOfPacks = ZInt.Zero;
			string messageError = "No. Of Packages" + MandatoryValidation.ValueCannotBeZero + ".";
			AssertHasMessageError(declaration.JE_TotalNoOfPacksInfo, messageError);

			declaration.JE_TotalNoOfPacks = 2;
			AssertNoMessageError(declaration.JE_TotalNoOfPacksInfo, messageError);
		}

		public void TestCheckJE_TotalNoOfPacksPackType()
		{
			string messageError = MandatoryValidation.YouHaveNotEntered + " a Packages Type.";
			declaration.JE_TotalNoOfPacksPackType = ZString.Empty;
			AssertHasMessageError(declaration.JE_TotalNoOfPacksPackTypeInfo, messageError);
			AssertNoMessageErrorContaining(declaration.JE_TotalNoOfPacksPackTypeInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_TotalNoOfPacksPackType = "Z#";
			AssertNoMessageError(declaration.JE_TotalNoOfPacksPackTypeInfo, messageError);
			AssertHasMessageErrorContaining(declaration.JE_TotalNoOfPacksPackTypeInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_TotalNoOfPacksPackType = Core.Constants.PkgUnit.Box;
			AssertNoMessageError(declaration.JE_TotalNoOfPacksPackTypeInfo, messageError);
			AssertNoMessageErrorContaining(declaration.JE_TotalNoOfPacksPackTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJE_OA_SupplierAddress_PostCode()
		{
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory, false);
			helper.Consignor.OH_RL_NKClosestPort = helper.CATOR.RL_Code;
			helper.Consignor.MainAddress.OA_PostCode = ZString.Empty;
			declaration.JE_OA_SupplierAddress = helper.Consignor.MainAddress.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OA_SupplierAddress = helper.Consignor.MainAddress.PK;
			AssertHasMessageError(declaration.JE_OA_SupplierAddressInfo, CAAddressValidator.ExporterPostCodeIsRequiredCountryCanadaOrUnitedStates);
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, CAAddressValidator.CanadianExporterPostCodeFormat);

			helper.Consignor.MainAddress.OA_PostCode = "123212";
			declaration.JE_OA_SupplierAddress = helper.Consignor.MainAddress.PK;
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, CAAddressValidator.ExporterPostCodeIsRequiredCountryCanadaOrUnitedStates);
			AssertHasMessageError(declaration.JE_OA_SupplierAddressInfo, CAAddressValidator.CanadianExporterPostCodeFormat);

			helper.Consignor.MainAddress.OA_PostCode = "A2B2C2";
			declaration.JE_OA_SupplierAddress = helper.Consignor.MainAddress.PK;
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, CAAddressValidator.ExporterPostCodeIsRequiredCountryCanadaOrUnitedStates);
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, CAAddressValidator.CanadianExporterPostCodeFormat);

			helper.Consignor.OH_RL_NKClosestPort = helper.USLAX.RL_Code;
			helper.Consignor.MainAddress.OA_PostCode = ZString.Empty;
			declaration.JE_OA_SupplierAddress = helper.Consignor.MainAddress.PK;
			AssertHasMessageError(declaration.JE_OA_SupplierAddressInfo, CAAddressValidator.ExporterPostCodeIsRequiredCountryCanadaOrUnitedStates);
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, CAAddressValidator.CanadianExporterPostCodeFormat);

			helper.Consignor.MainAddress.OA_PostCode = "123212";
			declaration.JE_OA_SupplierAddress = helper.Consignor.MainAddress.PK;
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, CAAddressValidator.ExporterPostCodeIsRequiredCountryCanadaOrUnitedStates);
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, CAAddressValidator.CanadianExporterPostCodeFormat);

			helper.Consignor.OH_RL_NKClosestPort = helper.AUSYD.RL_Code;
			helper.Consignor.MainAddress.OA_PostCode = ZString.Empty;
			declaration.JE_OA_SupplierAddress = helper.Consignor.MainAddress.PK;
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, CAAddressValidator.ExporterPostCodeIsRequiredCountryCanadaOrUnitedStates);
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, CAAddressValidator.CanadianExporterPostCodeFormat);

			declaration.JE_OA_SupplierAddress = ZGuid.Empty;
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, CAAddressValidator.ExporterPostCodeIsRequiredCountryCanadaOrUnitedStates);
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, CAAddressValidator.CanadianExporterPostCodeFormat);
		}

		public void TestCheckJE_OA_SupplierAddress_ProvinceState()
		{
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory, false);
			helper.Consignor.OH_RL_NKClosestPort = helper.AUSYD.RL_Code;
			declaration.JE_OA_SupplierAddress = helper.Consignor.MainAddress.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OA_SupplierAddress = helper.Consignor.MainAddress.PK;
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, CAAddressValidator.ProvinceStateIsRequiredCountryCanadaOrUnitedStates);

			helper.Consignor.OH_RL_NKClosestPort = helper.CATOR.RL_Code;
			helper.Consignor.MainAddress.OA_State = ZString.Empty;
			declaration.JE_OA_SupplierAddress = helper.Consignor.MainAddress.PK;
			AssertHasMessageError(declaration.JE_OA_SupplierAddressInfo, CAAddressValidator.ProvinceStateIsRequiredCountryCanadaOrUnitedStates);

			helper.Consignor.MainAddress.OA_State = helper.CATOR.CountryStates.RW_Description;
			declaration.JE_OA_SupplierAddress = helper.Consignor.MainAddress.PK;
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, CAAddressValidator.ProvinceStateIsRequiredCountryCanadaOrUnitedStates);

			helper.Consignor.OH_RL_NKClosestPort = helper.USLAX.RL_Code;
			helper.Consignor.MainAddress.OA_State = ZString.Empty;
			declaration.JE_OA_SupplierAddress = helper.Consignor.MainAddress.PK;
			AssertHasMessageError(declaration.JE_OA_SupplierAddressInfo, CAAddressValidator.ProvinceStateIsRequiredCountryCanadaOrUnitedStates);

			helper.Consignor.MainAddress.OA_State = helper.USLAX.CountryStates.RW_Description;
			declaration.JE_OA_SupplierAddress = helper.Consignor.MainAddress.PK;
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, CAAddressValidator.ProvinceStateIsRequiredCountryCanadaOrUnitedStates);

			declaration.JE_OA_SupplierAddress = ZGuid.Empty;
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, CAAddressValidator.ProvinceStateIsRequiredCountryCanadaOrUnitedStates);
		}

		public void TestCheckJE_OA_SupplierAddress_BusinessNumberForExport()
		{
			OrgHeader exporter = Factory.New<OrgHeader>();
			var address = exporter.Addresses.AddNew();
			declaration.JE_OA_SupplierAddress = ZGuid.Empty;
			ZString messageError = MandatoryValidation.YouHaveNotEntered + " an Exporter.";
			AssertHasMessageError(declaration.JE_OA_SupplierAddressInfo, messageError);
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, ExportJobDeclarationValidation.ExporterBusinessNumberIsRequired);
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberForExportFormat);

			declaration.JE_OA_SupplierAddress = address.PK;
			declaration.Validation.ValidateJE_OA_SupplierAddress();
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, messageError);
			AssertHasMessageError(declaration.JE_OA_SupplierAddressInfo, ExportJobDeclarationValidation.ExporterBusinessNumberIsRequired);
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberForExportFormat);

			OrgCusCode cusCode = exporter.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForExport, "123456789RMASDF");
			declaration.Validation.ValidateJE_OA_SupplierAddress();
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, messageError);
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, ExportJobDeclarationValidation.ExporterBusinessNumberIsRequired);
			AssertHasMessageError(declaration.JE_OA_SupplierAddressInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberForExportFormat);

			cusCode.OK_CustomsRegNo = "123456789RM0001";
			declaration.Validation.ValidateJE_OA_SupplierAddress();
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, messageError);
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, ExportJobDeclarationValidation.ExporterBusinessNumberIsRequired);
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberForExportFormat);
		}

		public void TestCheckJE_OA_SupplierAddress_BusinessNumberForExportWithBusinessNumberForImportExport()
		{
			OrgHeader exporter = Factory.New<OrgHeader>();
			var address = exporter.Addresses.AddNew();
			declaration.JE_OA_SupplierAddress = ZGuid.Empty;
			ZString messageError = MandatoryValidation.YouHaveNotEntered + " an Exporter.";
			AssertHasMessageError(declaration.JE_OA_SupplierAddressInfo, messageError);
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, ExportJobDeclarationValidation.ExporterBusinessNumberIsRequired);
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberForImportExportFormat);

			declaration.JE_OA_SupplierAddress = address.PK;
			declaration.Validation.ValidateJE_OA_SupplierAddress();
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, messageError);
			AssertHasMessageError(declaration.JE_OA_SupplierAddressInfo, ExportJobDeclarationValidation.ExporterBusinessNumberIsRequired);
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberForImportExportFormat);

			OrgCusCode cusCode = exporter.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123456789RMASDF");
			declaration.Validation.ValidateJE_OA_SupplierAddress();
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, messageError);
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, ExportJobDeclarationValidation.ExporterBusinessNumberIsRequired);
			AssertHasMessageError(declaration.JE_OA_SupplierAddressInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberForImportExportFormat);

			cusCode.OK_CustomsRegNo = "123456789RM0001";
			declaration.Validation.ValidateJE_OA_SupplierAddress();
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, messageError);
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, ExportJobDeclarationValidation.ExporterBusinessNumberIsRequired);
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberForImportExportFormat);
		}

		public void TestCheckJE_OA_SupplierAddress_AutorizationID()
		{
			OrgHeader exporter = Factory.New<OrgHeader>();
			var address = exporter.Addresses.AddNew();
			declaration.JE_OA_SupplierAddress = ZGuid.Empty;
			ZString messageError = MandatoryValidation.YouHaveNotEntered + " an Exporter.";
			AssertHasMessageError(declaration.JE_OA_SupplierAddressInfo, messageError);
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, ExportJobDeclarationValidation.ExporterAuthorizationIDIsRequired);
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, CanadianCustomsCodeValidator.Constants.AuthorizationIDRightFormat);

			declaration.JE_OA_SupplierAddress = address.PK;
			declaration.Validation.ValidateJE_OA_SupplierAddress();
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, messageError);
			AssertHasMessageError(declaration.JE_OA_SupplierAddressInfo, ExportJobDeclarationValidation.ExporterAuthorizationIDIsRequired);
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, CanadianCustomsCodeValidator.Constants.AuthorizationIDRightFormat);

			OrgCusCode cusCode = exporter.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.AuthorizationID, "ASS312");
			declaration.Validation.ValidateJE_OA_SupplierAddress();
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, messageError);
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, ExportJobDeclarationValidation.ExporterAuthorizationIDIsRequired);
			AssertHasMessageError(declaration.JE_OA_SupplierAddressInfo, CanadianCustomsCodeValidator.Constants.AuthorizationIDRightFormat);

			cusCode.OK_CustomsRegNo = "AS1324";
			declaration.Validation.ValidateJE_OA_SupplierAddress();
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, messageError);
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, ExportJobDeclarationValidation.ExporterAuthorizationIDIsRequired);
			AssertNoMessageError(declaration.JE_OA_SupplierAddressInfo, CanadianCustomsCodeValidator.Constants.AuthorizationIDRightFormat);
		}

		public void TestCheckJE_OA_SupplierAddress_Special()
		{
			OrgHeader exporter = Factory.New<OrgHeader>();
			var address = exporter.Addresses.AddNew();
			exporter.OH_FullName = "123ABC";
			declaration.JE_OA_SupplierAddress = address.PK;
			AssertNoWarning(declaration.JE_OA_SupplierAddressInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");

			exporter.OH_FullName = "123ABC– ";
			declaration.Validation.ValidateJE_OA_SupplierAddress();
			AssertHasWarning(declaration.JE_OA_SupplierAddressInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
		}

		public void TestCheckJE_OH_Forwarder_Phone()
		{
			OrgHeader serviceProvider = Factory.New<OrgHeader>();
			serviceProvider.MainAddress.OA_Phone = " ";
			declaration.JE_OH_Forwarder = ZGuid.Empty;
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, ExportJobDeclarationValidation.ServiceProviderPhoneIsRequired);

			declaration.JE_OH_Forwarder = serviceProvider.PK;
			AssertHasMessageError(declaration.JE_OH_ForwarderInfo, ExportJobDeclarationValidation.ServiceProviderPhoneIsRequired);

			serviceProvider.MainAddress.OA_Phone = "99999999";
			declaration.JE_OH_Forwarder = serviceProvider.PK;
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, ExportJobDeclarationValidation.ServiceProviderPhoneIsRequired);
		}

		public void TestCheckJE_OH_Forwarder_PostCode()
		{
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory, false);
			OrgHeader serviceProvider = Factory.New<OrgHeader>();
			serviceProvider.OH_RL_NKClosestPort = helper.CATOR.RL_Code;
			serviceProvider.MainAddress.OA_PostCode = " ";
			declaration.JE_OH_Forwarder = ZGuid.Empty;
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, ExportJobDeclarationValidation.CanadianServiceProviderPostCodeFormat);

			declaration.JE_OH_Forwarder = serviceProvider.PK;
			AssertHasMessageError(declaration.JE_OH_ForwarderInfo, ExportJobDeclarationValidation.CanadianServiceProviderPostCodeFormat);

			serviceProvider.MainAddress.OA_PostCode = "A9B9C9";
			declaration.JE_OH_Forwarder = serviceProvider.PK;
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, ExportJobDeclarationValidation.CanadianServiceProviderPostCodeFormat);
		}

		public void TestCheckJE_OH_Forwarder_AuthorizationID()
		{
			OrgHeader orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			declaration.JE_OH_Supplier = orgProxy.PK;
			OrgHeader serviceProvider = Factory.New<OrgHeader>();
			declaration.JE_OH_Forwarder = ZGuid.Empty;
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, ExportJobDeclarationValidation.ServiceProviderIsRequired);
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, CanadianCustomsCodeValidator.Constants.ServiceProviderAuthorizationIDRightFormat);

			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Forwarder = ZGuid.Empty;
			AssertHasMessageError(declaration.JE_OH_ForwarderInfo, ExportJobDeclarationValidation.ServiceProviderIsRequired);
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, CanadianCustomsCodeValidator.Constants.ServiceProviderAuthorizationIDRightFormat);

			declaration.JE_OH_Forwarder = serviceProvider.PK;
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, ExportJobDeclarationValidation.ServiceProviderIsRequired);
			AssertHasMessageError(declaration.JE_OH_ForwarderInfo, CanadianCustomsCodeValidator.Constants.ServiceProviderAuthorizationIDRightFormat);

			declaration.JE_OH_Supplier = orgProxy.PK;
			declaration.JE_OH_Forwarder = serviceProvider.PK;
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, ExportJobDeclarationValidation.ServiceProviderIsRequired);
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, CanadianCustomsCodeValidator.Constants.ServiceProviderAuthorizationIDRightFormat);

			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			OrgCusCode cusCode = serviceProvider.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.AuthorizationID, "BA1231");
			declaration.JE_OH_Forwarder = serviceProvider.PK;
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, ExportJobDeclarationValidation.ServiceProviderIsRequired);
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, CanadianCustomsCodeValidator.Constants.ServiceProviderAuthorizationIDRightFormat);

			cusCode.OK_CustomsRegNo = "BBB342";
			declaration.JE_OH_Forwarder = serviceProvider.PK;
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, ExportJobDeclarationValidation.ServiceProviderIsRequired);
			AssertHasMessageError(declaration.JE_OH_ForwarderInfo, CanadianCustomsCodeValidator.Constants.ServiceProviderAuthorizationIDRightFormat);
		}

		public void TestCheckJE_MessageType_CompanyOrgProxy()
		{
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory, false);
			OrgHeader companyOrgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			companyOrgProxy.OH_RL_NKClosestPort = helper.CATOR.RL_Code;
			companyOrgProxy.MainAddress.OA_State = ZString.Empty;
			companyOrgProxy.MainAddress.OA_Phone = " ";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoMessageError(declaration.JE_MessageTypeInfo, ExportJobDeclarationValidation.CompanyOrgProxyPhoneIsRequired);
			AssertNoMessageError(declaration.JE_MessageTypeInfo, ExportJobDeclarationValidation.CompanyOrgProxyStateIsRequired);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertHasMessageError(declaration.JE_MessageTypeInfo, ExportJobDeclarationValidation.CompanyOrgProxyPhoneIsRequired);
			AssertHasMessageError(declaration.JE_MessageTypeInfo, ExportJobDeclarationValidation.CompanyOrgProxyStateIsRequired);

			companyOrgProxy.MainAddress.OA_Phone = "2342";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNoMessageError(declaration.JE_MessageTypeInfo, ExportJobDeclarationValidation.CompanyOrgProxyPhoneIsRequired);
			AssertHasMessageError(declaration.JE_MessageTypeInfo, ExportJobDeclarationValidation.CompanyOrgProxyStateIsRequired);

			companyOrgProxy.MainAddress.OA_State = helper.CATOR.CountryStates.RW_Description;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNoMessageError(declaration.JE_MessageTypeInfo, ExportJobDeclarationValidation.CompanyOrgProxyPhoneIsRequired);
			AssertNoMessageError(declaration.JE_MessageTypeInfo, ExportJobDeclarationValidation.CompanyOrgProxyStateIsRequired);

			companyOrgProxy.OH_RL_NKClosestPort = helper.USLAX.RL_Code;
			companyOrgProxy.MainAddress.OA_State = ZString.Empty;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNoMessageError(declaration.JE_MessageTypeInfo, ExportJobDeclarationValidation.CompanyOrgProxyPhoneIsRequired);
			AssertHasMessageError(declaration.JE_MessageTypeInfo, ExportJobDeclarationValidation.CompanyOrgProxyStateIsRequired);

			companyOrgProxy.MainAddress.OA_State = helper.USLAX.CountryStates.RW_Description;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNoMessageError(declaration.JE_MessageTypeInfo, ExportJobDeclarationValidation.CompanyOrgProxyPhoneIsRequired);
			AssertNoMessageError(declaration.JE_MessageTypeInfo, ExportJobDeclarationValidation.CompanyOrgProxyStateIsRequired);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}
	}
}
