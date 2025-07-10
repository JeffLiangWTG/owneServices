using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ImportJobComInvoiceLineValidationTest : JobComInvoiceLineValidationTest
	{
		const string InvalidCode1 = "12";
		#region TestCheckJI_CustomsSecondQuantity

		public void TestCheckJI_CustomsSecondQuantity()
		{
			ValidationTestHelper.AssertValueCannotBeNegativeMessageError(invoiceLine.JI_CustomsSecondQuantityInfo);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.WarrantyRepairsRemission;
			invoiceLine.JI_ParentID = ZGuid.NewZGuid();
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			AssertHasMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, ImportJobComInvoiceLineValidation.ValueShouldBeZero);

			invoiceLine.JI_CustomsSecondUnitQty = CustomsUnitOfMeasureList.Codes.AlcoholByVolume;
			invoiceLine.JI_CustomsSecondQuantity = 0m;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsSecondQuantityInfo, MandatoryValidation.ValueCannotBeZero);
		}

		#endregion

		#region TestCheckJI_CustomsSecondUnitQty

		public void TestCheckJI_CustomsSecondUnitQty()
		{
			invoiceLine.JI_CustomsSecondQuantity = 10;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.JI_CustomsSecondUnitQtyInfo, InvalidCode1, CustomsUnitOfMeasureList.Codes.WattHour);

			var tax = invoiceLine.DutiesAndTaxes.AddNew();
			tax.C1_Override = true;
			tax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			tax.C1_Code = "E11";
			invoiceLine.JI_CustomsSecondUnitQty = CustomsUnitOfMeasureList.Codes.Bottle;
			invoiceLine.JI_CustomsThirdUnitQty = CustomsUnitOfMeasureList.Codes.Bottle;
			AssertHasMessageError(invoiceLine.JI_CustomsSecondUnitQtyInfo, HasABVUQErrorMesssage);

			invoiceLine.JI_CustomsSecondUnitQty = CustomsUnitOfMeasureList.Codes.AlcoholByVolume;
			AssertNoMessageError(invoiceLine.JI_CustomsSecondUnitQtyInfo, HasABVUQErrorMesssage);
		}

		const string HasABVUQErrorMesssage = "Alcohol By Volume (ABV) is required. At least one of the Customs QTY UQ must be ABV.";

		#endregion

		#region TestCheckJI_CustomsThirdQuantity

		public void TestCheckJI_CustomsThirdQuantity()
		{
			ValidationTestHelper.AssertValueCannotBeNegativeMessageError(invoiceLine.JI_CustomsThirdQuantityInfo);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.WarrantyRepairsRemission;
			invoiceLine.JI_ParentID = ZGuid.NewZGuid();
			invoiceLine.JI_CustomsThirdQuantity = 1m;
			AssertHasMessageError(invoiceLine.JI_CustomsThirdQuantityInfo, ImportJobComInvoiceLineValidation.ValueShouldBeZero);

			invoiceLine.JI_CustomsThirdUnitQty = CustomsUnitOfMeasureList.Codes.AlcoholByVolume;
			invoiceLine.JI_CustomsThirdQuantity = 0m;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsThirdQuantityInfo, MandatoryValidation.ValueCannotBeZero);
		}

		#endregion

		#region TestCheckJI_CustomsThirdUnitQty

		public void TestCheckJI_CustomsThirdUnitQty()
		{
			invoiceLine.JI_CustomsThirdQuantity = 10;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.JI_CustomsThirdUnitQtyInfo, InvalidCode1, CustomsUnitOfMeasureList.Codes.WattHour);

			var tax = invoiceLine.DutiesAndTaxes.AddNew();
			tax.C1_Override = true;
			tax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			tax.C1_Code = "E11";
			invoiceLine.JI_CustomsSecondUnitQty = CustomsUnitOfMeasureList.Codes.Bottle;
			invoiceLine.JI_CustomsThirdUnitQty = CustomsUnitOfMeasureList.Codes.Bottle;
			AssertHasMessageError(invoiceLine.JI_CustomsThirdUnitQtyInfo, HasABVUQErrorMesssage);

			invoiceLine.JI_CustomsThirdUnitQty = CustomsUnitOfMeasureList.Codes.AlcoholByVolume;
			AssertNoMessageError(invoiceLine.JI_CustomsThirdUnitQtyInfo, HasABVUQErrorMesssage);
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
		}

		public void TestCheckJI_Model()
		{
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = "Y";
			var ecccHeader = invoiceLine.ECCCPGAHeader;
			ecccHeader.CA_ComplianceDeclaration = true;
			ecccHeader.CA_WENProgramInd = YesNoList.Codes.Yes;
			invoiceLine.Validation.ValidateJI_Model();
			AssertHasMessageErrorContaining(invoiceLine.JI_ModelInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_Model = "AA";
			AssertNoMessageErrorContaining(invoiceLine.JI_ModelInfo, MandatoryValidation.YouHaveNotEntered);

			ecccHeader.CA_ComplianceDeclaration = false;
			ecccHeader.CA_ProcessCode = ProcessCodes.Codes.XE04;
			invoiceLine.JI_Model = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_ModelInfo, MandatoryValidation.YouHaveNotEntered);

			ecccHeader.CA_ProcessCode = ProcessCodes.Codes.XE03;
			invoiceLine.Validation.ValidateJI_Model();
			AssertNoMessageErrorContaining(invoiceLine.JI_ModelInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_OA_ManufacturerAddress_ECCCPGAHeader()
		{
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = "Y";
			var ecccHeader = invoiceLine.ECCCPGAHeader;
			ecccHeader.CA_WRMProgramInd = YesNoList.Codes.Yes;
			invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertHasMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, MandatoryValidation.YouHaveNotEntered);
			ecccHeader.CA_WRMProgramInd = YesNoList.Codes.No;
			ecccHeader.CA_ProcessCode = ProcessCodes.Codes.XE01;
			invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertHasMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_BrandName_ECCCPGAHeader()
		{
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = "Y";
			var ecccHeader = invoiceLine.ECCCPGAHeader;
			ecccHeader.CA_ProcessCode = ProcessCodes.Codes.XE04;
			invoiceLine.Validation.ValidateJI_BrandName();
			AssertHasMessageErrorContaining(invoiceLine.JI_BrandNameInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_BrandName = "ABC";
			AssertNoWarning(invoiceLine.JI_BrandNameInfo, MandatoryValidation.YouHaveNotEntered);

			ecccHeader.CA_ProcessCode = ProcessCodes.Codes.XE01;
			ecccHeader.CA_VEEProgramInd = YesNoList.Codes.Yes;
			ecccHeader.CA_Incomplete = true;
			ecccHeader.CA_VehicleClass = "ABC";
			invoiceLine.JI_BrandName = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_BrandNameInfo, MandatoryValidation.YouHaveNotEntered);

			ecccHeader.CA_ProcessCode = ProcessCodes.Codes.XE03;
			invoiceLine.Validation.ValidateJI_BrandName();
			AssertNoWarning(invoiceLine.JI_BrandNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_OA_ManufacturerAddress_DFOPGAHeader()
		{
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_DFOInd = "Y";
			var dfoHeader = invoiceLine.DFOPGAHeader;
			dfoHeader.CA_ABIProgramInd = YesNoList.Codes.Yes;
			invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, MandatoryValidation.YouHaveNotEntered);

			dfoHeader.CA_HasGeneticModification = true;
			invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertHasMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, MandatoryValidation.YouHaveNotEntered);

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.MainAddress.OA_Address1 = "Address1";
			invoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_OA_ManufacturerAddress_TCPGAHeader()
		{
			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.MainAddress.OA_Address1 = "Address1";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_TCInd = "Y";
			var pgaHeader = invoiceLine.TCPGAHeader;
			pgaHeader.CA_TPRProgramInd = YesNoList.Codes.Yes;

			invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertHasMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, MandatoryValidation.YouHaveNotEntered);

			pgaHeader.CA_TPRProgramInd = YesNoList.Codes.No;
			pgaHeader.CA_VPRProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_SubProgram = TCPGAVehicleProgramCodes.Codes.PIL;

			invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
			invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertHasMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, "Manufacturer WMI code is not found or is invalid.  WMI codes can be added under the Config > Registration Number tab of an organization.");

			var wmiCode = manufacturer.CustomsCodes.AddNew();
			wmiCode.OK_CodeType = OrgCusCode.CACodeTypes.WorldManufacturerIdentifier;
			wmiCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;
			wmiCode.OK_CustomsRegNo = "A01";

			invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, "Manufacturer WMI code is not found or is invalid.  WMI codes can be added under the Config > Registration Number tab of an organization.");
		}

		public void TestTestCheckJI_OA_ManufacturerAddress_Phone()
		{
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var manufacturer = Factory.New<OrgHeader>();
			invoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, OrganisationValidation.AddressWorkPhoneIsRequired);

			invoiceLine.CA_HCInd = YesNoList.Codes.Yes;
			var hcHeader = invoiceLine.HCPGAHeader;
			hcHeader.CA_PESProgramInd = YesNoList.Codes.Yes;
			invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertHasMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, OrganisationValidation.AddressWorkPhoneIsRequired);

			var contact = manufacturer.Contacts.AddNew();

			var allocatedContact = contact.Allocations.AddNew();
			allocatedContact.PC_Type = "CAP";

			invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, OrganisationValidation.AddressWorkPhoneIsRequired);
			AssertHasMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, OrganisationValidation.ContactPhoneNumberIsRequired);

			contact.OC_Phone = "123456";
			invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, OrganisationValidation.ContactPhoneNumberIsRequired);

			hcHeader.CA_PESProgramInd = Customs.Business.YesNoList.Codes.No;
			hcHeader.CA_CPRProgramInd = Customs.Business.YesNoList.Codes.Yes;

			invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertNoWarningContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, OrganisationValidation.ContactPhoneNumberIsRecommended);
			AssertHasWarningContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, OrganisationValidation.ContactEmailIsRecommended);

			contact.OC_Phone = "";
			contact.OC_Email = "test@test.com";

			invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertHasWarningContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, OrganisationValidation.ContactPhoneNumberIsRecommended);
			AssertNoWarningContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, OrganisationValidation.ContactEmailIsRecommended);
		}

		public void TestCheckJI_OA_ConsigneeAddress()
		{
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var consignee = Factory.New<OrgHeader>();
			var address = consignee.Addresses.MainAddress;
			address.OA_Address1 = "Address line 1";
			address.OA_City = "Sdyney";
			address.OA_RN_NKCountryCode = "AU";
			invoiceLine.JI_OA_ConsigneeAddress = address.PK;
			AssertNotNull(invoiceLine.JI_OA_ConsigneeAddress_ZAddress.OrgHeader);

			invoiceLine.JI_OA_ConsigneeAddress = ZGuid.Empty;
			AssertHasError(invoiceLine.JI_OA_ConsigneeAddressInfo, "Please enter a consignee address.");

			invoiceLine.JI_OA_ConsigneeAddress = address.PK;
			AssertNoError(invoiceLine.JI_OA_ConsigneeAddressInfo, "Please enter a consignee address.");

			invoiceLine.CA_CFIAInd = YesNoList.Codes.Yes;
			var cfiaHeader = invoiceLine.CFIAPGAHeader;
			invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertHasMessageError(invoiceLine.JI_OA_ConsigneeAddressInfo, OrganisationValidation.AddressWorkPhoneIsRequired);

			invoiceLine.CA_CFIAInd = ZString.Empty;
			invoiceLine.CA_CNSCInd = YesNoList.Codes.Yes;
			var cnscHeader = invoiceLine.CNSCPGAHeader;
			invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertHasMessageError(invoiceLine.JI_OA_ConsigneeAddressInfo, OrganisationValidation.AddressWorkPhoneIsRequired);

			invoiceLine.CA_CNSCInd = ZString.Empty;
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
			var ecccHeader = invoiceLine.ECCCPGAHeader;
			ecccHeader.CA_WRMProgramInd = YesNoList.Codes.Yes;
			invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertHasMessageError(invoiceLine.JI_OA_ConsigneeAddressInfo, OrganisationValidation.AddressWorkPhoneIsRequired);

			var contact = consignee.Contacts.AddNew();
			var allocatedContact = contact.Allocations.AddNew();
			allocatedContact.PC_Type = "CAP";

			invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertNoMessageError(invoiceLine.JI_OA_ConsigneeAddressInfo, OrganisationValidation.AddressWorkPhoneIsRequired);
			AssertHasMessageError(invoiceLine.JI_OA_ConsigneeAddressInfo, OrganisationValidation.ContactPhoneNumberIsRequired);

			contact.OC_Phone = "12345678";
			invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertNoMessageError(invoiceLine.JI_OA_ConsigneeAddressInfo, OrganisationValidation.ContactPhoneNumberIsRequired);
		}

		public void TestCheckJI_OA_ConsigneeAddress_AddrIsNotCA()
		{
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = "Y";

			var consignee = Factory.New<OrgHeader>();
			var address = consignee.Addresses.MainAddress;
			address.OA_Address1 = "Address line 1";
			address.OA_RN_NKCountryCode = "AU";

			invoiceLine.JI_OA_ConsigneeAddress = address.PK;
			AssertHasMessageErrorContaining(invoiceLine.JI_OA_ConsigneeAddressInfo, "Please select an Canada address.");

			address.OA_RN_NKCountryCode = "CA";
			invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_ConsigneeAddressInfo, "Please select an Canada address.");

			invoiceLine.CA_CFIAInd = ZString.Empty;
			invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertNoWarning(invoiceLine.JI_OA_ConsigneeAddressInfo, ImportJobComInvoiceLineValidation.ConsigneeAddrIsNotCA);

			address.OA_RN_NKCountryCode = "CN";
			invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertHasWarning(invoiceLine.JI_OA_ConsigneeAddressInfo, ImportJobComInvoiceLineValidation.ConsigneeAddrIsNotCA);
		}

		#region TestCheckJI_BrandName
		public void TestCheckJI_BrandName()
		{
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_TCInd = "Y";
			var tcpgaHeader = invoiceLine.TCPGAHeader;
			tcpgaHeader.CA_VPRProgramInd = "Y";
			tcpgaHeader.CA_SubProgram = TCPGAVehicleProgramCodes.Codes.VFS;
			invoiceLine.Validation.ValidateJI_BrandName();
			AssertHasMessageErrorContaining(invoiceLine.JI_BrandNameInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_BrandName = "ABD";
			AssertNoMessageErrorContaining(invoiceLine.JI_BrandNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_BrandName_CPRProgram()
		{
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_HCInd = YesNoList.Codes.Yes;

			var pgaHeader = invoiceLine.HCPGAHeader;
			pgaHeader.CA_CPRProgramInd = YesNoList.Codes.Yes;

			invoiceLine.Validation.ValidateJI_BrandName();
			AssertHasWarning(invoiceLine.JI_BrandNameInfo, ImportJobComInvoiceLineValidation.CPR_BrandNameShouldNotBeEmptyMessage);

			invoiceLine.JI_BrandName = "ABC";
			AssertNoWarning(invoiceLine.JI_BrandNameInfo, ImportJobComInvoiceLineValidation.CPR_BrandNameShouldNotBeEmptyMessage);
		}

		public void TestCheckJI_BrandName_CAProperties()
		{
			ZString errorMessage = "You have not entered a ";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			using (invoiceLine.SuspendValidationTesting())
			{
				CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.NRCan, "4011100000", "EEF");
				CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.TC, "4011100000", "TPR");
				invoiceLine.Declaration.CA_OGDIC = true;
				invoiceLine.CA_ModelNumber = "A";
				invoiceLine.CA_BrandNameSITTInfo.ClearAllNotifications();
				invoiceLine.Validation.ValidateJI_BrandName();
				AssertHasMessageError("CA_BrandNameSITTInfo", invoiceLine.CA_BrandNameSITTInfo, $"You have not entered a {invoiceLine.CA_BrandNameSITTInfo.HumanReadableName}.");
				invoiceLine.JI_Tariff = "4011100000";
				invoiceLine.Declaration.CA_OGDTC = true;
				invoiceLine.Declaration.CA_OGDNR = true;
				invoiceLine.CA_BrandNameTCInfo.ClearAllNotifications();
				invoiceLine.CA_BrandNameNRInfo.ClearAllNotifications();
				invoiceLine.Validation.ValidateJI_BrandName();
				AssertHasMessageError("CA_BrandNameTCInfo", invoiceLine.CA_BrandNameTCInfo, $"You have not entered a {invoiceLine.CA_BrandNameTCInfo.HumanReadableName}.");
				AssertHasMessageError("CA_BrandNameNRInfo", invoiceLine.CA_BrandNameNRInfo, $"You have not entered a {invoiceLine.CA_BrandNameNRInfo.HumanReadableName}.");
			}
		}
		#endregion

		public void TestCheckJI_Model_TC()
		{
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_TCInd = "Y";
			var tcpgaHeader = invoiceLine.TCPGAHeader;
			tcpgaHeader.CA_VPRProgramInd = "Y";
			tcpgaHeader.CA_SubProgram = TCPGAVehicleProgramCodes.Codes.VFS;
			invoiceLine.Validation.ValidateJI_Model();
			AssertHasMessageErrorContaining(invoiceLine.JI_ModelInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_Model = "ABD";
			AssertNoMessageErrorContaining(invoiceLine.JI_ModelInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_Model_DFO()
		{
			var line = CreateNewLine();
			line.CA_DFOInd = YesNoList.Codes.Yes;

			var pgaHeader = line.DFOPGAHeader;

			pgaHeader.CA_ABIProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_HasGeneticModification = true;

			Customs.Business.Testing.ValidationTestHelper.AssertYouHaveNotEnteredMessageError(line.JI_ModelInfo);

			pgaHeader.CA_ABIProgramInd = YesNoList.Codes.No;
			pgaHeader.CA_AISProgramInd = YesNoList.Codes.Yes;

			Customs.Business.Testing.ValidationTestHelper.AssertYouHaveNotEnteredMessageError(line.JI_ModelInfo);

			pgaHeader.CA_AISProgramInd = YesNoList.Codes.No;
			pgaHeader.CA_TTPProgramInd = YesNoList.Codes.Yes;

			Customs.Business.Testing.ValidationTestHelper.AssertYouHaveNotEnteredMessageError(line.JI_ModelInfo);
		}

		public void TestCheckJI_NetWeight()
		{
			invoiceLine.CA_CNSCInd = YesNoList.Codes.Yes;
			invoiceLine.CNSCPGAHeader.CA_Category = CNSCCategories.Codes.CNS;

			invoiceLine.Validation.ValidateJI_NetWeight();
			AssertHasMessageErrorContaining(invoiceLine.JI_NetWeightInfo, "Net Weight cannot be zero.");

			invoiceLine.JI_NetWeight = 7.12m;
			AssertNoMessageErrorContaining(invoiceLine.JI_NetWeightUQInfo, "Net Weight cannot be zero.");
		}

		public void TestCheckJI_NetWeightWhenHasGAC()
		{
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;
			var lpco = invoiceLine.GACPGAHeader.LPCOViews.AddNew();
			lpco.CLP_Type = LPCODocumentTypeQualifier.Codes._2006;
			invoiceLine.Validation.ValidateJI_NetWeight();
			AssertHasMessageErrorContaining(invoiceLine.JI_NetWeightInfo, "Net Weight cannot be zero.");

			invoiceLine.JI_NetWeight = 7.12m;
			invoiceLine.Validation.ValidateJI_NetWeight();
			AssertNoMessageErrorContaining(invoiceLine.JI_NetWeightUQInfo, "Net Weight cannot be zero.");
		}

		public void TestTestCheckJI_NetWeightUQ_JI_WeightUQWhenHasGAC()
		{
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;
			var lpco = invoiceLine.GACPGAHeader.LPCOViews.AddNew();
			lpco.CLP_Type = LPCODocumentTypeQualifier.Codes._2006;
			invoiceLine.JI_NetWeightUQ = ZString.Empty;
			invoiceLine.Validation.ValidateJI_NetWeightUQ();
			Customs.Business.Testing.ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_NetWeightUQInfo);

			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilotonnes;
			invoiceLine.Validation.ValidateJI_NetWeightUQ();
			AssertHasMessageErrorContaining(invoiceLine.JI_NetWeightUQInfo, "Requires unit KG or T since PGA GAC has LPCO with type 2006.");

			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.Validation.ValidateJI_NetWeightUQ();
			AssertNoMessageErrorContaining(invoiceLine.JI_NetWeightUQInfo, "Requires unit KG or T since PGA GAC has LPCO with type 2006.");

			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Tonnes;
			invoiceLine.Validation.ValidateJI_NetWeightUQ();
			AssertNoMessageErrorContaining(invoiceLine.JI_NetWeightUQInfo, "Requires unit KG or T since PGA GAC has LPCO with type 2006.");
		}

		public void TestCheckJI_NetWeightUQ_JI_WeightUQListValidation()
		{
			var message = "The code you have selected is not in the list.";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_NRCanInd = YesNoList.Codes.Yes;
			invoiceLine.NRCanPGAHeader.CA_EEFProgramInd = YesNoList.Codes.Yes;

			invoiceLine.JI_WeightUQ = "@@";
			invoiceLine.JI_NetWeightUQ = "@@";
			AssertHasMessageErrorContaining(invoiceLine.JI_WeightUQInfo, message);
			AssertHasMessageErrorContaining(invoiceLine.JI_NetWeightUQInfo, message);
		}

		JobComInvoiceLine CreateNewLine()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invocieHeader = jobDeclaration.Invoices.AddNew();

			var line = (JobComInvoiceLine)invocieHeader.InvoiceLines.AddNew();
			line.CA_TradeName = string.Empty;

			return line;
		}

		public void TestCheckJI_Model_JI_BrandName()
		{
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_NRCanInd = YesNoList.Codes.Yes;
			invoiceLine.NRCanPGAHeader.CA_EEFProgramInd = YesNoList.Codes.Yes;

			invoiceLine.JI_Model = "";
			invoiceLine.JI_BrandName = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_ModelInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(invoiceLine.JI_BrandNameInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_Model = "MODEL";
			invoiceLine.JI_BrandName = "BRAND";
			AssertNoMessageErrorContaining(invoiceLine.JI_ModelInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(invoiceLine.JI_BrandNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_StateOrRegionOfOrigin()
		{
			var message = ListValidation.InvalidCodeMessageError;

			invoiceLine.JI_CountryOfOrigin = "CN";
			invoiceLine.JI_StateOrRegionOfOrigin = "";
			AssertNoMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, message);
			invoiceLine.JI_StateOrRegionOfOrigin = "@@";
			AssertHasMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, message);

			invoiceLine.JI_CountryOfOrigin = "CA";
			invoiceLine.JI_StateOrRegionOfOrigin = "";
			AssertNoMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, message);
			invoiceLine.JI_StateOrRegionOfOrigin = "AB";
			AssertNoMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, message);
			invoiceLine.JI_StateOrRegionOfOrigin = "AL";
			AssertHasMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, message);

			invoiceLine.JI_CountryOfOrigin = "US";
			invoiceLine.JI_StateOrRegionOfOrigin = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_StateOrRegionOfOrigin = "AL";
			AssertNoMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, message);
			invoiceLine.JI_StateOrRegionOfOrigin = "AB";
			AssertHasMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, message);
		}

		public void TestCheckCA_ImportReasonCodeTC()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_ImportReasonCodeTCInfo);
			invoiceLine.Declaration.CA_OGDTC = true;
			invoiceLine.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.TC);
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.CA_ImportReasonCodeTCInfo, "99", ImportReasonCodes.Codes.Export);
		}

		public void TestCheckCA_ImportReasonCodeNR()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_ImportReasonCodeNRInfo);
			invoiceLine.Declaration.CA_OGDNR = true;
			invoiceLine.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.NRCan);
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.CA_ImportReasonCodeNRInfo, "99", ImportReasonCodes.Codes.Export);
		}

		public void TestCheckCA_ImportReasonCodeSITT()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_ImportReasonCodeSITTInfo);
			var messageError = "Import Reason Code must be 05 (Testing) if no Certification Nos are entered";
			invoiceLine.Declaration.CA_OGDIC = true;
			invoiceLine.CA_ImportReasonCodeSITT = ZString.Empty;
			invoiceLine.CA_Model = "X";
			AssertNoMessageError(invoiceLine.CA_ImportReasonCodeSITTInfo, messageError);
			invoiceLine.CA_ImportReasonCodeSITT = ImportReasonCodes.Codes.Sale;
			AssertHasMessageError(invoiceLine.CA_ImportReasonCodeSITTInfo, messageError);
			invoiceLine.CA_ImportReasonCodeSITT = ImportReasonCodes.Codes.Testing;
			AssertNoMessageError(invoiceLine.CA_ImportReasonCodeSITTInfo, messageError);
			invoiceLine.SITTCertificationNumbers.AddNew("123", "12345");
			invoiceLine.CA_ImportReasonCodeSITT = ImportReasonCodes.Codes.Sale;
			AssertNoMessageError(invoiceLine.CA_ImportReasonCodeSITTInfo, messageError);

			messageError = "At least one Certification Number must be entered if Import Reason Code is blank";
			invoiceLine.CA_Model = ZString.Empty;
			invoiceLine.SITTCertificationNumbers.RemoveAll();
			invoiceLine.CA_ImportReasonCodeSITT = ZString.Empty;
			AssertNoMessageError(invoiceLine.CA_ImportReasonCodeSITTInfo, messageError);
			invoiceLine.CA_Model = "X";
			invoiceLine.AddInfoValidation.ValidateCA_ImportReasonCode();
			AssertHasMessageError(invoiceLine.CA_ImportReasonCodeSITTInfo, messageError);
			invoiceLine.SITTCertificationNumbers.AddNew("123", "12345");
			invoiceLine.AddInfoValidation.ValidateCA_ImportReasonCode();
			AssertNoMessageErrors(invoiceLine.CA_ImportReasonCodeSITTInfo);
		}

		public void TestCheckCA_BrandName()
		{
			CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.NRCan, "7321111000", "EEF");
			CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.TC, "4011100000", "TPR");
			invoiceLine.Declaration.CA_OGDIC = true;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_BrandNameSITTInfo);
			invoiceLine.CA_Model = "X";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.CA_BrandNameSITTInfo);
			invoiceLine.Declaration.CA_OGDIC = false;
			invoiceLine.CA_Model = ZString.Empty;

			invoiceLine.Declaration.CA_OGDIC = false;
			invoiceLine.Declaration.CA_OGDNR = true;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_BrandNameNRInfo);
			invoiceLine.JI_Tariff = "7321111000";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.CA_BrandNameNRInfo);
			invoiceLine.Declaration.CA_OGDNR = false;
			invoiceLine.CA_TypeSize = ZString.Empty;

			invoiceLine.Declaration.CA_OGDTC = true;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_BrandNameTCInfo);
			invoiceLine.JI_Tariff = "4011100000";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.CA_BrandNameTCInfo);

			invoiceLine.Declaration.CA_OGDTC = false;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_BrandNameSITTInfo);
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_BrandNameNRInfo);
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_BrandNameTCInfo);
		}

		public void TestCheckCA_TypeSize()
		{
			CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.NRCan, "7321111000", "EEF");
			CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.TC, "4011100000", "TPR");
			invoiceLine.Declaration.CA_OGDTC = true;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_TypeSizeTCInfo);
			invoiceLine.JI_Tariff = "4011100000";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.CA_TypeSizeTCInfo);
			invoiceLine.Declaration.CA_OGDTC = false;

			invoiceLine.Declaration.CA_OGDNR = true;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_TypeSizeNRInfo);
			invoiceLine.JI_Tariff = "7321111000";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.CA_TypeSizeNRInfo);

			invoiceLine.Declaration.CA_OGDNR = false;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_TypeSizeNRInfo);
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_TypeSizeTCInfo);
		}

		public void TestCheckCA_Model()
		{
			invoiceLine.Declaration.CA_OGDIC = true;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_ModelSITTInfo);
			invoiceLine.CA_ModelNumber = "X";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.CA_ModelSITTInfo);
			invoiceLine.Declaration.CA_OGDIC = false;
			invoiceLine.CA_ModelNumber = ZString.Empty;

			invoiceLine.Declaration.CA_OGDNR = true;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_ModelNRInfo);
			invoiceLine.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.NRCan);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.CA_ModelNRInfo);

			invoiceLine.Declaration.CA_OGDNR = false;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_ModelNRInfo);
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_ModelSITTInfo);
		}

		#region TestCheckCA_ModelNumber

		public void TestCheckCA_ModelNumber()
		{
			invoiceLine.Declaration.CA_OGDIC = true;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_ModelNumberSITTInfo);
			invoiceLine.JI_BrandName = "X";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.CA_ModelNumberSITTInfo);
			invoiceLine.Declaration.CA_OGDIC = false;
			invoiceLine.JI_BrandName = ZString.Empty;

			invoiceLine.Declaration.CA_OGDNR = true;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_ModelNumberNRInfo);
			invoiceLine.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.NRCan);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.CA_ModelNumberNRInfo);

			invoiceLine.Declaration.CA_OGDNR = false;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_ModelNumberNRInfo);
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_ModelNumberSITTInfo);
		}

		#endregion

		[TestDate(2017, 2, 21, 10, 30, 25)]
		public void TestCheckJI_Tariff()
		{
			#region Universal Tariff Setup

			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var chinaTradeGroup = universalHelper.CreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.China, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.AddCountry(chinaTradeGroup, Core.Constants.CountryCodes.China, ZDate.Today.AddYears(-1), ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, DutyAndTaxManager.SIMATariffType);
			var antiDumpingRateType = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.AntiDumping);
			var surTaxRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.SUR, antiDumpingRateType.PK);
			Factory.Save();
			var surtaxTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "0987654321", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(surtaxTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KGM");
			var surTaxRate = universalHelper.CreateRate(surtaxTariff, surTaxRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "0.1 * VFD");
			universalHelper.CreateCusApplicability(surTaxRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			Factory.Save();
			CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.TC, "4011100000", "TPR");
			CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA, "0101210000");
			CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.NRCan, "7321111000", "EEF");
			#endregion

			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_EffectiveDate = new ZDateTime(2010, 12, 30);
			classHeader.ZA_ExpiryDate = new ZDateTime(2011, 1, 3);
			classHeader.ZA_ClassificationNumber = "1234567890";

			var classHeader2 = Factory.New<CACClassHeader>();
			classHeader2.ZA_EffectiveDate = new ZDateTime(2010, 12, 30);
			classHeader2.ZA_ExpiryDate = new ZDateTime(2017, 3, 3);
			classHeader2.ZA_ClassificationNumber = "1234567899";

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.JI_TariffInfo, "9999999999", classHeader2.ZA_ClassificationNumber, "Classification was not found or is not valid for 21-Feb-17.");

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2011, 1, 4);
			invoiceLine.JI_Tariff = "1234567890";
			AssertHasMessageError(invoiceLine.JI_TariffInfo, "Classification was not found or is not valid for 04-Jan-11.");

			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);

			invoiceLine.JI_Tariff = "4011100000";
			AssertHasMessageError(invoiceLine.JI_TariffInfo, TCTariffMessage);
			declaration.CA_OGDTC = true;
			invoiceLine.CA_ImportReasonCodeTC = "01";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageError(invoiceLine.JI_TariffInfo, TCTariffMessage);
			declaration.CA_OGDTC = false;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageError(invoiceLine.JI_TariffInfo, TCTariffMessage);
			declaration.CA_OGDTC = true;
			invoiceLine.CA_ImportReasonCode = ZString.Empty;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageError(invoiceLine.JI_TariffInfo, TCTariffMessage);
			invoiceLine.JI_Tariff = classHeader2.ZA_ClassificationNumber;
			AssertNoMessageError(invoiceLine.JI_TariffInfo, TCTariffMessage);

			invoiceLine.JI_Tariff = "0101210000";
			AssertHasMessageError(invoiceLine.JI_TariffInfo, CFIATariffMessage);
			declaration.CA_OGDCFIA = true;
			invoiceLine.CA_AirsCode = "1";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageError(invoiceLine.JI_TariffInfo, CFIATariffMessage);
			declaration.CA_OGDCFIA = false;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageError(invoiceLine.JI_TariffInfo, CFIATariffMessage);
			declaration.CA_OGDCFIA = true;
			invoiceLine.CA_AirsCode = ZString.Empty;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageError(invoiceLine.JI_TariffInfo, CFIATariffMessage);
			invoiceLine.JI_Tariff = classHeader2.ZA_ClassificationNumber;
			AssertNoMessageError(invoiceLine.JI_TariffInfo, CFIATariffMessage);

			invoiceLine.JI_Tariff = "7321111000";
			AssertHasMessageError(invoiceLine.JI_TariffInfo, NRCanTariffMessage);
			declaration.CA_OGDNR = true;
			invoiceLine.CA_ImportReasonCodeNR = "01";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageError(invoiceLine.JI_TariffInfo, NRCanTariffMessage);
			declaration.CA_OGDNR = false;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageError(invoiceLine.JI_TariffInfo, NRCanTariffMessage);
			declaration.CA_OGDNR = true;
			invoiceLine.CA_ImportReasonCode = ZString.Empty;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageError(invoiceLine.JI_TariffInfo, NRCanTariffMessage);
			invoiceLine.JI_Tariff = classHeader2.ZA_ClassificationNumber;
			AssertNoMessageError(invoiceLine.JI_TariffInfo, NRCanTariffMessage);

			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);

			invoiceLine.JI_Tariff = "4011100000";
			AssertNoMessageError(invoiceLine.JI_TariffInfo, TCTariffMessage);
			invoiceLine.JI_Tariff = "0101210000";
			AssertNoMessageError(invoiceLine.JI_TariffInfo, CFIATariffMessage);
			invoiceLine.JI_Tariff = "7321111000";
			AssertNoMessageError(invoiceLine.JI_TariffInfo, NRCanTariffMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 3, 1);
			invoiceLine.JI_Tariff = ZString.Empty;
			AssertNoMessageError(invoiceLine.JI_TariffInfo, TCTariffMessage);
			AssertNoMessageError(invoiceLine.JI_TariffInfo, CFIATariffMessage);
			AssertNoMessageError(invoiceLine.JI_TariffInfo, NRCanTariffMessage);
			invoiceLine.JI_Tariff = "4011100000";
			AssertHasWarningContaining(invoiceLine.JI_TariffInfo, TCTariffMessage);
			invoiceLine.JI_Tariff = "0101210000";
			AssertHasWarningContaining(invoiceLine.JI_TariffInfo, CFIATariffMessage);
			invoiceLine.JI_Tariff = "7321111000";
			AssertHasWarningContaining(invoiceLine.JI_TariffInfo, NRCanTariffMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.China;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			invoiceLine.JI_Tariff = "0987654321";
			AssertHasMessageError(invoiceLine.JI_TariffInfo, SIMATariffMessage);
			Factory.ClearCachedValue<ZBool>();

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.France;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageError(invoiceLine.JI_TariffInfo, SIMATariffMessage);
		}
		const string TCTariffMessage = "Transport Canada details are required for this HS code.";
		const string CFIATariffMessage = "CFIA details are required for this HS code.";
		const string NRCanTariffMessage = "Natural Resources Canada details are required for this HS code.";
		const string SIMATariffMessage = "Classification Requires SIMA – please add SIMA Code under the Duty & Tax tab, if there is no row with SIMA and no Special Authority Number entered.";

		public new void TestCheckJI_Description()
		{
			invoiceLine.Validation.ValidateJI_Description();
			AssertHasMessageErrorContaining(invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_Description = "Description";
			AssertNoMessageErrorContaining(invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			invoiceLine.JI_Description = new ZString('X', 257);
			AssertHasMessageErrorContaining(invoiceLine.JI_DescriptionInfo, ImportJobComInvoiceLineValidation.GoodsDescriptionUpTo256Chars);
			invoiceLine.JI_Description = new ZString('X', 256);
			AssertNoMessageErrorContaining(invoiceLine.JI_DescriptionInfo, ImportJobComInvoiceLineValidation.GoodsDescriptionUpTo256Chars);
		}

		public void TestCheckJI_CountryOfOriginForECCC()
		{
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = "Y";
			var ecccHeader = invoiceLine.ECCCPGAHeader;
			ecccHeader.CA_WENProgramInd = YesNoList.Codes.Yes;
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertHasMessageErrorContaining(invoiceLine.JI_CountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.JZ_RN_NKDefaultOrigin = "US";
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertNoMessageErrorContaining(invoiceLine.JI_CountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_CountryOfOrigin()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.JI_CountryOfOriginInfo, "12", Core.Constants.CountryCodes.Canada);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			invoice = declaration.Invoices[0];
			invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_CountryOfOriginInfo, "12", Core.Constants.CountryCodes.Canada);

			const string messageError = "Country/Region of Origin must be specified for LVS when tariff treatment code other than 02 and 10.";
			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			invoiceLine.CA_TreatmentCode = TariffTreatmentCodes.Codes.Iceland;
			AssertHasMessageError(invoiceLine.JI_CountryOfOriginInfo, messageError);
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_CountryOfOriginInfo, "12", Core.Constants.CountryCodes.Canada);

			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			invoiceLine.CA_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			AssertNoMessageError(invoiceLine.JI_CountryOfOriginInfo, messageError);
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_CountryOfOriginInfo, "12", Core.Constants.CountryCodes.Canada);

			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			invoiceLine.CA_TreatmentCode = ZString.Empty;
			invoice.CA_TreatmentCode = TariffTreatmentCodes.Codes.Iceland;
			AssertHasMessageError(invoiceLine.JI_CountryOfOriginInfo, messageError);
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_CountryOfOriginInfo, "12", Core.Constants.CountryCodes.Canada);

			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			invoice.CA_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			AssertNoMessageError(invoiceLine.JI_CountryOfOriginInfo, messageError);
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_CountryOfOriginInfo, "12", Core.Constants.CountryCodes.Canada);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			invoiceLine.CA_TreatmentCode = TariffTreatmentCodes.Codes.General;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			AssertHasMessageErrorContaining(invoiceLine.CA_TreatmentCodeInfo, "General Rate of Duty (TT 03) is not applicable to US origin shipments");
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Canada;
			AssertNoMessageErrorContaining(invoiceLine.CA_TreatmentCodeInfo, "General Rate of Duty (TT 03) is not applicable to US origin shipments");

			invoice.JZ_RN_NKDefaultOrigin = "CN";
			invoiceLine.JI_CountryOfOrigin = "X";
			AssertHasMessageErrorContaining(invoiceLine.JI_CountryOfOriginInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckCountryOfOriginMatchToProduct()
		{
			var msg = "Goods Origin does not match product code file.";

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";
			var supRelation = part.RelatedOrganisations.AddSupplier(Factory.New<OrgHeader>());

			var importClassification = Factory.New<BaseCusClassification>();
			importClassification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			var importPivot = part.PivotsForBinding.AddNew();
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			importPivot.CI_CC = importClassification.PK;

			invoiceLine.InvoiceHeader.JZ_OH_Supplier = supRelation.OU_OH;
			invoiceLine.JI_PartNo = part.OP_PartNum;
			using (CustomsDataRegistry.Instance.SeverityLevelOfInvoiceLineValidationAgainstProductData.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, ProductAuditActions.Codes.AddWarningValidation))
			{
				invoiceLine.JI_CountryOfOrigin = "CA";
				AssertNoWarning(invoiceLine.JI_CountryOfOriginInfo, msg);

				importPivot.CCA_RN_NKOrigin = "US";
				invoiceLine.Validation.ValidateJI_CountryOfOrigin();
				AssertHasWarning(invoiceLine.JI_CountryOfOriginInfo, msg);

				invoiceLine.InvoiceHeader.JZ_RN_NKDefaultOrigin = "";
				invoiceLine.JI_CountryOfOrigin = "";
				AssertEquals("", invoiceLine.JI_CountryOfOrigin);
				invoiceLine.InvoiceHeader.JZ_RN_NKDefaultOrigin = "CA";
				AssertHasWarning(invoiceLine.JI_CountryOfOriginInfo, msg);

				invoiceLine.InvoiceHeader.JZ_RN_NKDefaultOrigin = "";
				invoiceLine.JI_CountryOfOrigin = "US";
				invoiceLine.InvoiceHeader.JZ_RN_NKDefaultOrigin = "US";
				AssertNoWarning(invoiceLine.JI_CountryOfOriginInfo, msg);

				importPivot.CCA_RN_NKOrigin = "CA";
				invoiceLine.JI_CountryOfOrigin = "CA";
				invoiceLine.Validation.ValidateJI_CountryOfOrigin();
				AssertNoWarning(invoiceLine.JI_CountryOfOriginInfo, msg);
			}

			using (CustomsDataRegistry.Instance.SeverityLevelOfInvoiceLineValidationAgainstProductData.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, ProductAuditActions.Codes.AddMessageErrorValidation))
			{
				importPivot.CCA_RN_NKOrigin = string.Empty;
				invoiceLine.Validation.ValidateJI_CountryOfOrigin();
				AssertNoMessageError(invoiceLine.JI_CountryOfOriginInfo, msg);

				importPivot.CCA_RN_NKOrigin = "US";
				invoiceLine.Validation.ValidateJI_CountryOfOrigin();
				AssertHasMessageError(invoiceLine.JI_CountryOfOriginInfo, msg);

				invoiceLine.InvoiceHeader.JZ_RN_NKDefaultOrigin = "";
				invoiceLine.JI_CountryOfOrigin = "";
				AssertEquals("", invoiceLine.JI_CountryOfOrigin);
				invoiceLine.InvoiceHeader.JZ_RN_NKDefaultOrigin = "CA";
				AssertHasMessageError(invoiceLine.JI_CountryOfOriginInfo, msg);

				invoiceLine.InvoiceHeader.JZ_RN_NKDefaultOrigin = "";
				invoiceLine.JI_CountryOfOrigin = "US";
				invoiceLine.InvoiceHeader.JZ_RN_NKDefaultOrigin = "US";
				AssertNoMessageError(invoiceLine.JI_CountryOfOriginInfo, msg);

				importPivot.CCA_RN_NKOrigin = string.Empty;
				invoiceLine.Validation.ValidateJI_CountryOfOrigin();
				AssertNoMessageError(invoiceLine.JI_CountryOfOriginInfo, msg);
			}
		}

		public void TestCheckJI_CountryOfOrigin_LVX()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			invoice = declaration.Invoices[0];
			invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoice.JZ_RN_NKDefaultOrigin = string.Empty;
			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_CountryOfOriginInfo);

			invoice.JZ_RN_NKDefaultOrigin = string.Empty;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Canada;
			AssertNoMessageErrors(invoiceLine.JI_CountryOfOriginInfo);

			invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.Canada;
			invoiceLine.JI_CountryOfOrigin = string.Empty;
			AssertNoMessageErrors(invoiceLine.JI_CountryOfOriginInfo);
		}

		public void TestCheckJI_CustomsQuantity()
		{
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Centilitre;
			invoiceLine.JI_CustomsQuantity = 0m;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, MandatoryValidation.ValueCannotBeZero);
			invoiceLine.JI_CustomsUnitQty = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, MandatoryValidation.ValueCannotBeZero);

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.WarrantyRepairsRemission;
			invoiceLine.JI_ParentID = ZGuid.NewZGuid();
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Centilitre;
			invoiceLine.JI_CustomsQuantity = 1m;
			AssertHasMessageError(invoiceLine.JI_CustomsQuantityInfo, ImportJobComInvoiceLineValidation.ValueShouldBeZero);

			invoiceLine.CA_TCInd = YesNoList.Codes.Yes;
			invoiceLine.TCPGAHeader.CA_VPRProgramInd = YesNoList.Codes.Yes;
			invoiceLine.TCPGAHeader.CA_SubProgram = TCPGAVehicleProgramCodes.Codes.PIL;
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Number;
			invoiceLine.JI_CustomsQuantity = 2m;
			AssertHasWarning(invoiceLine.JI_CustomsQuantityInfo, ImportJobComInvoiceLineValidation.SeparateLineShouldBeCreated);
			invoiceLine.TCPGAHeader.CA_SubProgram = TCPGAVehicleProgramCodes.Codes.PIG;
			invoiceLine.Validation.ValidateJI_CustomsQuantity();
			AssertNoWarning(invoiceLine.JI_CustomsQuantityInfo, ImportJobComInvoiceLineValidation.SeparateLineShouldBeCreated);
		}

		public void TestCheckJI_CustomsUnitQty()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff3 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "1234567890", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(tariff3, "CU1", "KGM");
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_CustomsUnitQtyInfo, "???", "NMB");
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);

			CACustomsDataRegistry.Instance.EnableDebugHooks.SetTemporaryValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var tariff1 = Factory.New<CACClassHeader>();
			tariff1.ZA_ClassificationNumber = "1234567890";
			tariff1.ZA_StatisticalUOMCode = "KGM";
			tariff1.ZA_AreaCode = "AAA";
			tariff1.ZA_EffectiveDate = ZDateTime.Today.AddDays(-1);
			tariff1.ZA_ExpiryDate = ZDateTime.Today.AddDays(1);

			invoiceLine.JI_Tariff = tariff1.ZA_ClassificationNumber;
			invoiceLine.JI_InvoiceUQ = ZString.Empty;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_CustomsUnitQty = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsUnitQtyInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_CustomsQuantity = 0m;
			invoiceLine.Validation.ValidateJI_CustomsUnitQty();
			AssertNoMessageErrorContaining(invoiceLine.JI_CustomsUnitQtyInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_FormattedTariff = ZString.Empty;
			invoiceLine.JI_InvoiceUQ = "XX";
			AssertNoMessageErrorContaining(invoiceLine.JI_CustomsUnitQtyInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_InvoiceUQ = CustomsUnitOfMeasureList.Codes.Centilitre;
			AssertNoMessageErrorContaining(invoiceLine.JI_CustomsUnitQtyInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_InvoiceUQ()
		{
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			invoiceLine.JI_CustomsUnitQty = ZString.Empty;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageErrorForValidationType(invoiceLine.JI_InvoiceQuantityInfo, ImportJobComInvoiceLineValidation.InvoiceQtyRequired, ValidateForMessageType.ACROSS, declaration);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageErrorForValidationType(invoiceLine.JI_InvoiceUQInfo, ImportJobComInvoiceLineValidation.InvoiceQtyRequired, ValidateForMessageType.ACROSS, declaration);
			invoiceLine.JI_Tariff = "0000999905";
			AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, ImportJobComInvoiceLineValidation.InvoiceQtyRequired);
			AssertNoMessageError(invoiceLine.JI_InvoiceUQInfo, ImportJobComInvoiceLineValidation.InvoiceQtyRequired);
			invoiceLine.JI_Tariff = "";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageErrorForValidationType(invoiceLine.JI_InvoiceUQInfo, ImportJobComInvoiceLineValidation.InvoiceQtyRequired, ValidateForMessageType.ACROSS, declaration);
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_InvoiceQuantityInfo);
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_InvoiceUQInfo);
			invoiceLine.JI_CustomsUnitQty = "KGM";
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_InvoiceQuantityInfo);
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_InvoiceUQInfo);

			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_InvoiceUQInfo, "???", "GA");
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_InvoiceUQInfo, "???", "KGM");

			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_InvoiceUQ = "";
			invoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertHasMessageError(invoiceLine.JI_InvoiceUQInfo, ImportJobComInvoiceLineValidation.InvoiceUQRequiredForInvoiceQuantity);

			invoiceLine.JI_InvoiceUQ = "KGM";
			invoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertNoMessageError(invoiceLine.JI_InvoiceUQInfo, ImportJobComInvoiceLineValidation.InvoiceUQRequiredForInvoiceQuantity);

			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.IsEnableACROSSValidation = true;
			invoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertNoMessageError(invoiceLine.JI_InvoiceUQInfo, ImportJobComInvoiceLineValidation.InvoiceQtyRequiredForServiceIID);

			invoiceLine.JI_InvoiceUQ = "";
			invoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertHasMessageError(invoiceLine.JI_InvoiceUQInfo, ImportJobComInvoiceLineValidation.InvoiceQtyRequiredForServiceIID);
		}

		public void TestCheckJI_InvoiceUQ_TCPGA()
		{
			var message = "NO or NMB should be selected for Tires Program of PGA Transport Canada";
			invoiceLine.CA_TCInd = YesNoList.Codes.Yes;
			invoiceLine.TCPGAHeader.CA_TPRProgramInd = YesNoList.Codes.Yes;

			invoiceLine.JI_InvoiceUQ = "KG";
			AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceUQInfo, message);

			invoiceLine.JI_InvoiceUQ = "NO";
			AssertNoMessageErrorContaining(invoiceLine.JI_InvoiceUQInfo, message);

			invoiceLine.JI_InvoiceUQ = "NMB";
			AssertNoMessageErrorContaining(invoiceLine.JI_InvoiceUQInfo, message);

			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var message2 = "EA should be selected for Tires Program of PGA Transport Canada";

			invoiceLine.JI_InvoiceUQ = "NO";
			AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceUQInfo, message2);

			invoiceLine.JI_InvoiceUQ = "EA";
			AssertNoMessageErrorContaining(invoiceLine.JI_InvoiceUQInfo, message2);
			AssertNoMessageErrorContaining(invoiceLine.JI_InvoiceUQInfo, message);
		}

		public void TestCheckJI_InvoiceUQ_ForECCC()
		{
			var errorMessage = "EA should be selected for Vehicle And Engine Emissions of PGA Environment And Climate Change Canada";
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
			invoiceLine.ECCCPGAHeader.CA_VEEProgramInd = YesNoList.Codes.Yes;

			invoiceLine.JI_InvoiceUQ = "";
			invoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceUQInfo, errorMessage);

			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceUQInfo, errorMessage);

			invoiceLine.JI_InvoiceUQ = "EA";
			invoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertNoMessageErrorContaining(invoiceLine.JI_InvoiceUQInfo, errorMessage);
		}

		public void TestCheckCA_OGDStatus()
		{
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);

			using (CACustomsDataRegistry.Instance.AIRSValidationKey.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ""))
			{
				invoiceLine.CA_OGDStatus = "NOI";
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "validation has not been completed");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "goods may not be imported");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "OGD Inspection/Review may be required");
				invoiceLine.CA_OGDStatus = "NOT";
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "validation has not been completed");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "goods may not be imported");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "OGD Inspection/Review may be required");
				invoiceLine.CA_OGDStatus = "ERR";
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "validation has not been completed");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "goods may not be imported");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "OGD Inspection/Review may be required");
				invoiceLine.CA_OGDStatus = "REJ";
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "validation has not been completed");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "goods may not be imported");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "OGD Inspection/Review may be required");
				invoiceLine.CA_OGDStatus = "UNK";
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "validation has not been completed");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "goods may not be imported");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "OGD Inspection/Review may be required");
				invoiceLine.CA_OGDStatus = "OKA";
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "validation has not been completed");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "goods may not be imported");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "OGD Inspection/Review may be required");
				invoiceLine.CA_OGDStatus = "INS";
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "validation has not been completed");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "goods may not be imported");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "OGD Inspection/Review may be required");
				invoiceLine.CA_OGDStatus = "RVW";
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "validation has not been completed");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "goods may not be imported");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "OGD Inspection/Review may be required");
			}

			using (CACustomsDataRegistry.Instance.AIRSValidationKey.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "XX"))
			{
				invoiceLine.CA_OGDStatus = "NOI";
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "validation has not been completed");
				AssertHasMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "goods may not be imported");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "OGD Inspection/Review may be required");
				invoiceLine.CA_OGDStatus = "NOT";
				AssertHasMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "validation has not been completed");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "goods may not be imported");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "OGD Inspection/Review may be required");
				invoiceLine.CA_OGDStatus = "ERR";
				AssertHasMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "validation has not been completed");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "goods may not be imported");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "OGD Inspection/Review may be required");
				invoiceLine.CA_OGDStatus = "REJ";
				AssertHasMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "validation has not been completed");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "goods may not be imported");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "OGD Inspection/Review may be required");
				invoiceLine.CA_OGDStatus = "UNK";
				AssertHasMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "validation has not been completed");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "goods may not be imported");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "OGD Inspection/Review may be required");
				invoiceLine.CA_OGDStatus = "OKA";
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "validation has not been completed");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "goods may not be imported");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "OGD Inspection/Review may be required");
				invoiceLine.CA_OGDStatus = "INS";
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "validation has not been completed");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "goods may not be imported");
				AssertHasMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "OGD Inspection/Review may be required");
				invoiceLine.CA_OGDStatus = "RVW";
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "validation has not been completed");
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "goods may not be imported");
				AssertHasMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "OGD Inspection/Review may be required");
				declaration.CA_ServiceOption = ServiceOptions.Codes.IID;
				invoiceLine.CA_OGDStatus = "NOI";
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "goods may not be imported");
				invoiceLine.CA_OGDStatus = "NOT";
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "validation has not been completed");
				invoiceLine.CA_OGDStatus = "RVW";
				AssertNoMessageErrorContaining(invoiceLine.CA_OGDStatusInfo, "OGD Inspection/Review may be required");
			}
		}

		public void TestCheckJI_PreviousEntryNumberAndLineNumber()
		{
			var helper = new WhsDataTestHelper(Factory);
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = helper.Importer.PK;
			declaration.WarehouseTransactionStatus = Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated;

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
			invoiceLine.Validation.ValidateJI_PreviousEntryNumber();
			AssertNoMessageError(invoiceLine.JI_PreviousEntryNumberInfo, "Previous Tran. Number is required.");
			invoiceLine.Validation.ValidateJI_PreviousEntryLineNumber();
			AssertNoMessageError(invoiceLine.JI_PreviousEntryLineNumberInfo, "Previous Tran. Line Number is required.");
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			invoiceLine.Validation.ValidateJI_PreviousEntryNumber();
			AssertNoMessageError(invoiceLine.JI_PreviousEntryNumberInfo, "Previous Tran. Number is required.");
			invoiceLine.Validation.ValidateJI_PreviousEntryLineNumber();
			AssertNoMessageError(invoiceLine.JI_PreviousEntryLineNumberInfo, "Previous Tran. Line Number is required.");
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			invoiceLine.Validation.ValidateJI_PreviousEntryNumber();
			AssertHasMessageError(invoiceLine.JI_PreviousEntryNumberInfo, "Previous Tran. Number is required.");
			invoiceLine.Validation.ValidateJI_PreviousEntryLineNumber();
			AssertHasMessageError(invoiceLine.JI_PreviousEntryLineNumberInfo, "Previous Tran. Line Number is required.");
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.TransferOfGoods30;
			invoiceLine.Validation.ValidateJI_PreviousEntryNumber();
			AssertHasMessageError(invoiceLine.JI_PreviousEntryNumberInfo, "Previous Tran. Number is required.");
			invoiceLine.Validation.ValidateJI_PreviousEntryLineNumber();
			AssertHasMessageError(invoiceLine.JI_PreviousEntryLineNumberInfo, "Previous Tran. Line Number is required.");
			invoiceLine.JI_PreviousEntryNumber = "1234500006789";
			AssertNoMessageError(invoiceLine.JI_PreviousEntryNumberInfo, "Previous Tran. Number is required.");
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			AssertNoMessageError(invoiceLine.JI_PreviousEntryLineNumberInfo, "Previous Tran. Line Number is required.");

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

			invoiceLine.JI_PreviousEntryNumber = ZString.Empty;
			invoiceLine.JI_PreviousEntryLineNumber = 0;
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			invoiceLine.Validation.ValidateJI_PreviousEntryNumber();
			AssertNoMessageError(invoiceLine.JI_PreviousEntryNumberInfo, "Previous Tran. Number is required.");
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
			invoiceLine.Validation.ValidateJI_PreviousEntryNumber();
			AssertNoMessageError(invoiceLine.JI_PreviousEntryNumberInfo, "Previous Tran. Number is required.");
			invoiceLine.Validation.ValidateJI_PreviousEntryLineNumber();
			AssertNoMessageError(invoiceLine.JI_PreviousEntryLineNumberInfo, "Previous Tran. Line Number is required.");
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			invoiceLine.Validation.ValidateJI_PreviousEntryNumber();
			AssertHasMessageError(invoiceLine.JI_PreviousEntryNumberInfo, "Previous Tran. Number is required.");
			invoiceLine.Validation.ValidateJI_PreviousEntryLineNumber();
			AssertHasMessageError(invoiceLine.JI_PreviousEntryLineNumberInfo, "Previous Tran. Line Number is required.");
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods301;
			invoiceLine.Validation.ValidateJI_PreviousEntryNumber();
			AssertHasMessageError(invoiceLine.JI_PreviousEntryNumberInfo, "Previous Tran. Number is required.");
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods302;
			invoiceLine.Validation.ValidateJI_PreviousEntryNumber();
			AssertHasMessageError(invoiceLine.JI_PreviousEntryNumberInfo, "Previous Tran. Number is required.");
			invoiceLine.Validation.ValidateJI_PreviousEntryLineNumber();
			AssertHasMessageError(invoiceLine.JI_PreviousEntryLineNumberInfo, "Previous Tran. Line Number is required.");
			invoiceLine.JI_PreviousEntryNumber = "1234500006789";
			AssertNoMessageError(invoiceLine.JI_PreviousEntryNumberInfo, "Previous Tran. Number is required.");
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			AssertNoMessageError(invoiceLine.JI_PreviousEntryLineNumberInfo, "Previous Tran. Line Number is required.");
		}

		public void TestCheckJI_InvoiceQuantity()
		{
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = "Y";
			invoiceLine.JI_CustomsUnitQty = "ABC";
			invoiceLine.JI_CustomsQuantity = 2;
			var gacHeader = invoiceLine.GACPGAHeader;
			gacHeader.CA_AllProgramInd = "Y";
			invoiceLine.Validation.ValidateJI_InvoiceQuantity();
			AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceQuantityInfo, "GAC reporting requires Invoice Quantity");

			invoiceLine.JI_InvoiceQuantity = 2;
			AssertNoMessageErrorContaining(invoiceLine.JI_InvoiceQuantityInfo, "GAC reporting requires Invoice Quantity");

			invoiceLine.JI_InvoiceQuantity = 0;
			invoiceLine.JI_InvoiceUQ = "KGM";
			invoiceLine.Validation.ValidateJI_InvoiceQuantity();
			AssertHasMessageError(invoiceLine.JI_InvoiceQuantityInfo, ImportJobComInvoiceLineValidation.InvoiceQuantityRequiredForInvoiceUQ);

			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.Validation.ValidateJI_InvoiceQuantity();
			AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, ImportJobComInvoiceLineValidation.InvoiceQuantityRequiredForInvoiceUQ);

			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			invoiceLine.Validation.ValidateJI_InvoiceQuantity();
			AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, ImportJobComInvoiceLineValidation.InvoiceQtyRequiredForServiceIID);

			invoiceLine.JI_InvoiceQuantity = 0;
			invoiceLine.Validation.ValidateJI_InvoiceQuantity();
			AssertHasMessageError(invoiceLine.JI_InvoiceQuantityInfo, ImportJobComInvoiceLineValidation.InvoiceQtyRequiredForServiceIID);
		}

		public void TestCheckJI_OA_ConsigneeAddress_ValidateMandatory()
		{
			CreateDataForMessageSending();

			invoiceLine1.JI_OA_ConsigneeAddress = orgAddress.PK;

			AssertNoMessageErrorContaining(invoiceLine1.JI_OA_ConsigneeAddressInfo, "You have not entered a name");
			orgAddress.OA_CompanyNameOverride = ZString.Empty;
			invoiceLine1.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertNoMessageErrorContaining(invoiceLine1.JI_OA_ConsigneeAddressInfo, "You have not entered a name");

			orgAddress.OA_CompanyNameOverride = "ORGDEF";
			orgAddress.Header.OH_FullName = ZString.Empty;
			invoiceLine1.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertNoMessageErrorContaining(invoiceLine1.JI_OA_ConsigneeAddressInfo, "You have not entered a name");

			orgAddress.OA_CompanyNameOverride = ZString.Empty;
			invoiceLine1.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertHasMessageErrorContaining(invoiceLine1.JI_OA_ConsigneeAddressInfo, "You have not entered a name");

			AssertNoMessageErrorContaining(invoiceLine1.JI_OA_ConsigneeAddressInfo, "You have not entered an address line 1");
			orgAddress.OA_Address1 = ZString.Empty;
			invoiceLine1.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertHasMessageErrorContaining(invoiceLine1.JI_OA_ConsigneeAddressInfo, "You have not entered an address line 1");

			AssertNoMessageErrorContaining(invoiceLine1.JI_OA_ConsigneeAddressInfo, "You have not entered a city");
			orgAddress.OA_City = ZString.Empty;
			invoiceLine1.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertHasMessageErrorContaining(invoiceLine1.JI_OA_ConsigneeAddressInfo, "You have not entered a city");

			AssertNoMessageErrorContaining(invoiceLine1.JI_OA_ConsigneeAddressInfo, "You have not entered a country/region");
			orgAddress.OA_RN_NKCountryCode = ZString.Empty;
			invoiceLine1.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertHasMessageErrorContaining(invoiceLine1.JI_OA_ConsigneeAddressInfo, "You have not entered a country/region");
			AssertHasMessageErrorContaining(invoiceLine1.JI_OA_ConsigneeAddressInfo, "Consignee");
		}

		public void TestCheckJI_OA_ManufacturerAddress_ValidateMandatory()
		{
			CreateDataForMessageSending();

			invoiceLine1.JI_OA_ManufacturerAddress = orgAddress.PK;

			AssertNoMessageErrorContaining(invoiceLine1.JI_OA_ManufacturerAddressInfo, "You have not entered a name");
			orgAddress.OA_CompanyNameOverride = ZString.Empty;
			invoiceLine1.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertNoMessageErrorContaining(invoiceLine1.JI_OA_ManufacturerAddressInfo, "You have not entered a name");

			orgAddress.OA_CompanyNameOverride = "ORGDEF";
			orgAddress.Header.OH_FullName = ZString.Empty;
			invoiceLine1.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertNoMessageErrorContaining(invoiceLine1.JI_OA_ManufacturerAddressInfo, "You have not entered a name");

			orgAddress.OA_CompanyNameOverride = ZString.Empty;
			invoiceLine1.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertHasMessageErrorContaining(invoiceLine1.JI_OA_ManufacturerAddressInfo, "You have not entered a name");

			AssertNoMessageErrorContaining(invoiceLine1.JI_OA_ManufacturerAddressInfo, "You have not entered an address line 1");
			orgAddress.OA_Address1 = ZString.Empty;
			invoiceLine1.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertHasMessageErrorContaining(invoiceLine1.JI_OA_ManufacturerAddressInfo, "You have not entered an address line 1");

			AssertNoMessageErrorContaining(invoiceLine1.JI_OA_ManufacturerAddressInfo, "You have not entered a city");
			orgAddress.OA_City = ZString.Empty;
			invoiceLine1.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertHasMessageErrorContaining(invoiceLine1.JI_OA_ManufacturerAddressInfo, "You have not entered a city");

			AssertNoMessageErrorContaining(invoiceLine1.JI_OA_ManufacturerAddressInfo, "You have not entered a country/region");
			orgAddress.OA_RN_NKCountryCode = ZString.Empty;
			invoiceLine1.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertHasMessageErrorContaining(invoiceLine1.JI_OA_ManufacturerAddressInfo, "You have not entered a country/region");
			AssertHasMessageErrorContaining(invoiceLine1.JI_OA_ManufacturerAddressInfo, "Manufacturer");
		}

		public void TestJI_InvoiceUQAndJI_InvoiceQuantity_IsLuxuryTaxLine()
		{
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			invoiceLine.Validation.ValidateJI_InvoiceQuantity();
			invoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertHasMessageError(invoiceLine.JI_InvoiceQuantityInfo, ImportJobComInvoiceLineValidation.InvoiceQtyRequired);
			AssertHasMessageError(invoiceLine.JI_InvoiceUQInfo, ImportJobComInvoiceLineValidation.InvoiceQtyRequired);
			invoiceLine.JI_Tariff = JobComInvoiceLine.LuxuryTaxTariffCode;
			invoiceLine.JI_ParentID = Guid.NewGuid();
			invoiceLine.Validation.ValidateJI_InvoiceQuantity();
			invoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, ImportJobComInvoiceLineValidation.InvoiceQtyRequired);
			AssertNoMessageError(invoiceLine.JI_InvoiceUQInfo, ImportJobComInvoiceLineValidation.InvoiceQtyRequired);
			invoiceLine.JI_Tariff = "";
			invoiceLine.JI_ParentID = Guid.Empty;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			invoiceLine.Validation.ValidateJI_InvoiceQuantity();
			invoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertHasMessageError(invoiceLine.JI_InvoiceQuantityInfo, ImportJobComInvoiceLineValidation.InvoiceQtyRequiredForServiceIID);
			AssertHasMessageError(invoiceLine.JI_InvoiceUQInfo, ImportJobComInvoiceLineValidation.InvoiceQtyRequiredForServiceIID);
			invoiceLine.JI_Tariff = JobComInvoiceLine.LuxuryTaxTariffCode;
			invoiceLine.JI_ParentID = Guid.NewGuid();
			invoiceLine.Validation.ValidateJI_InvoiceQuantity();
			invoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, ImportJobComInvoiceLineValidation.InvoiceQtyRequiredForServiceIID);
			AssertNoMessageError(invoiceLine.JI_InvoiceUQInfo, ImportJobComInvoiceLineValidation.InvoiceQtyRequiredForServiceIID);
		}

		public void TestCheckDutyAndTaxMatchToProduct()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";
			var supRelation = part.RelatedOrganisations.AddSupplier(Factory.New<OrgHeader>());

			var importClassification = Factory.New<BaseCusClassification>();
			importClassification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			var importPivot = part.PivotsForBinding.AddNew();
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			importPivot.CI_CC = importClassification.PK;

			invoiceLine.InvoiceHeader.JZ_OH_Supplier = supRelation.OU_OH;
			invoiceLine.JI_PartNo = part.OP_PartNum;
			using (CustomsDataRegistry.Instance.SeverityLevelOfInvoiceLineValidationAgainstProductData.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, ProductAuditActions.Codes.AddWarningValidation))
			{
				AddDutyAndTax(invoiceLine.DutiesAndTaxes, DutyAndTaxTypes.Codes.ADD, "51");
				AddDutyAndTax(importPivot.DutiesAndTaxes, DutyAndTaxTypes.Codes.ADD, "52");
				AddDutyAndTax(importPivot.DutiesAndTaxes, DutyAndTaxTypes.Codes.CVD, "53");
				AddDutyAndTax(importPivot.DutiesAndTaxes, DutyAndTaxTypes.Codes.CVD, "52");
				invoiceLine.Validation.ValidateAll();
				AssertHasRowWarning(invoiceLine, "Duty & Tax (Tax Types: ADD, CVD) does not match product code file.");

				using (CustomsDataRegistry.Instance.SeverityLevelOfInvoiceLineValidationAgainstProductData.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, ProductAuditActions.Codes.AddMessageErrorValidation))
				{
					invoiceLine.Validation.ValidateAll();
					AssertHasRowMessageError(invoiceLine, "Duty & Tax (Tax Types: ADD, CVD) does not match product code file.");
					AssertNoRowWarningContaining(invoiceLine, "Duty & Tax (Tax Types: ADD, CVD) does not match product code file.");
				}

				using (CustomsDataRegistry.Instance.SeverityLevelOfInvoiceLineValidationAgainstProductData.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, ProductAuditActions.Codes.NoAction))
				{
					invoiceLine.Validation.ValidateAll();
					AssertNoRowMessageErrorContaining(invoiceLine, "Duty & Tax (Tax Types: ADD, CVD) does not match product code file.");
					AssertNoRowWarningContaining(invoiceLine, "Duty & Tax (Tax Types: ADD, CVD) does not match product code file.");
				}

				AddDutyAndTax(invoiceLine.DutiesAndTaxes, DutyAndTaxTypes.Codes.CVD, "52");
				AddDutyAndTax(invoiceLine.DutiesAndTaxes, DutyAndTaxTypes.Codes.CVD, "53");
				AddDutyAndTax(invoiceLine.DutiesAndTaxes, DutyAndTaxTypes.Codes.ADD, "52");
				invoiceLine.Validation.ValidateAll();
				AssertNoRowWarningContaining(invoiceLine, "Duty & Tax (Tax Types: ADD) does not match product code file.");

				using (CustomsDataRegistry.Instance.SeverityLevelOfInvoiceLineValidationAgainstProductData.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, ProductAuditActions.Codes.AddMessageErrorValidation))
				{
					invoiceLine.Validation.ValidateAll();
					AssertNoRowMessageErrorContaining(invoiceLine, "Duty & Tax (Tax Types: ADD) does not match product code file.");
					AssertNoRowWarningContaining(invoiceLine, "Duty & Tax (Tax Types: ADD) does not match product code file.");
				}
			}
		}

		public void TestCheckCA_SIMADumpingDesc()
		{
			#region Universal Tariff Setup

			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var chinaTradeGroup = universalHelper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.China, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.AddNewOrExistingCountry(chinaTradeGroup, Core.Constants.CountryCodes.China);
			Factory.Save();
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var simaTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, DutyAndTaxManager.SIMATariffType);
			var antiDumpingRateType = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.AntiDumping);
			var antiDumpingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.ADD, antiDumpingRateType.PK);
			var countervailingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.CVD, antiDumpingRateType.PK);
			Factory.Save();
			var antiDumpingRelTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "1234567890", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(antiDumpingRelTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NMB");
			var antiDumpingTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1407", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, relatedTariffCode: "1234567890");
			var antiDumpingRelationShip = universalHelper.CreateTariffRelationship(antiDumpingTariff.PK, harmonizedTariffType.PK, "1234567890");
			var antiDumpingRate = universalHelper.CreateRate(antiDumpingTariff, antiDumpingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "100.5 * [NMB]");
			var antiDumpingApplicability = universalHelper.CreateCusApplicability(antiDumpingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var countervailingRate = universalHelper.CreateRate(antiDumpingTariff, countervailingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "200 * [KGM]");
			var countervailingApplicability = universalHelper.CreateCusApplicability(countervailingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			Factory.Save();

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNoInfo.ClearValue();
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			AssertNoMessageError(invoiceLine.CA_SIMADumpingDescInfo, ImportJobComInvoiceLineValidation.SimaMeasureIsMissingMessage);
			invoiceLine.CA_SIMADumpingNumInfo.ClearValue();
			((ImportJobComInvoiceLineValidation)invoiceLine.Validation).ValidateCA_SIMADumpingDesc();
			AssertHasMessageError(invoiceLine.CA_SIMADumpingDescInfo, ImportJobComInvoiceLineValidation.SimaMeasureIsMissingMessage);
		}

		void AddDutyAndTax(DutyAndTaxCollection dutyAndTaxes, ZString taxType, ZString exemptCode)
		{
			var tax = Factory.New<DutyAndTax>();
			tax.C1_TaxType = taxType;
			tax.C1_ExemptCode = exemptCode;
			dutyAndTaxes.Add(tax);
		}

		void CreateDataForMessageSending()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "ORGABC";
			orgAddress = orgHeader.MainAddress;
			orgAddress.OA_CompanyNameOverride = "ORGDEF";
			orgAddress.OA_Address1 = "Address 1";
			orgAddress.OA_Address2 = "Address 2";
			orgAddress.OA_City = "HONGKONG";
			orgAddress.OA_RN_NKCountryCode = "HK";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
		}
		JobComInvoiceLine invoiceLine1;
		OrgAddress orgAddress;
	}
}
