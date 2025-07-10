using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using UniversalConstants = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.CN.Business.Testing
{
	sealed class JobDeclarationValidationTest : Customs.Business.Testing.BaseJobDeclarationValidationTest<JobDeclaration>
	{
		public void TestCheckJE_MessageType_ErrorIfChangedAwaitingResponse()
		{
			void actionWatingForResponse(JobDeclaration declaration)
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			}
			AssertErrorsAfterChangingJE_MessageType(false, false, actionWatingForResponse, true);
		}

		public void TestCheckJE_MessageType_MessageErrorIfControllerChangedAwaitingResponse()
		{
			void actionWatingForResponse(JobDeclaration declaration)
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			}
			AssertErrorsAfterChangingJE_MessageType(true, false, actionWatingForResponse, false);
		}

		public void TestCheckJE_MessageType_MessageErrorIfChangedAwaitingResponse()
		{
			void actionWatingForResponse(JobDeclaration declaration)
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			}
			AssertErrorsAfterChangingJE_MessageType(false, true, actionWatingForResponse, false);
		}

		public void TestCheckJE_MessageType_MessageErrorIfChangedHasDeclarationUnifiedNumber()
		{
			void actionWatingForResponse(JobDeclaration declaration)
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.DeclarationUnifiedNumber = "I20180000123456789";
			}
			AssertErrorsAfterChangingJE_MessageType(false, true, actionWatingForResponse, false);
		}

		public void TestCheckJE_MessageType_MessageErrorIfChangedHasEntryNumber()
		{
			void actionWatingForResponse(JobDeclaration declaration)
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.EntryNumber = "ENT00000001";
			}
			AssertErrorsAfterChangingJE_MessageType(false, true, actionWatingForResponse, false);
		}

		public void TestAdditionalReferenceNumbers()
		{
			var additionalReferenceNumber1 = declaration.AdditionalReferenceNumbers.AddNew();
			additionalReferenceNumber1.CE_EntryType = "PSL";
			additionalReferenceNumber1.CE_EntryNum = "1";
			var additionalReferenceNumber2 = declaration.AdditionalReferenceNumbers.AddNew();
			additionalReferenceNumber2.CE_EntryType = "PSL";
			additionalReferenceNumber2.CE_EntryNum = "1";
			AssertHasError(additionalReferenceNumber2.CE_EntryNumInfo, "There is another entry with the same type and number.");
			AssertNoError(additionalReferenceNumber2.CE_EntryTypeInfo, "The Number Type has been duplicated and must be unique.");
			additionalReferenceNumber2.CE_EntryNum = "2";
			AssertNoError(additionalReferenceNumber2.CE_EntryNumInfo, "There is another entry with the same type and number.");
			additionalReferenceNumber1.CE_EntryType = "SLD";
			additionalReferenceNumber2.CE_EntryType = "SLD";
			AssertHasError(additionalReferenceNumber2.CE_EntryTypeInfo, "The Number Type has been duplicated and must be unique.");
		}

		public override void TestJE_ShipmentIncoTerm()
		{
			declaration.JE_ShipmentIncoTerm = "DAF";
			AssertHasMessageError(declaration.JE_ShipmentIncoTermInfo, "The code you have selected is not in the list.");
			declaration.JE_ShipmentIncoTerm = "C&I";
			AssertHasMessageError(declaration.JE_ShipmentIncoTermInfo, "The code you have selected is not in the list.");
			declaration.JE_ShipmentIncoTerm = "CFR";
			AssertNoMessageError(declaration.JE_ShipmentIncoTermInfo, "The code you have selected is not in the list.");
			declaration.JE_ShipmentIncoTerm = "CIF";
			AssertNoMessageError(declaration.JE_ShipmentIncoTermInfo, "The code you have selected is not in the list.");
			declaration.JE_ShipmentIncoTerm = "FOB";
			AssertNoMessageError(declaration.JE_ShipmentIncoTermInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckJE_RL_NKPortOfArrival()
		{
			declaration.JE_MessageSubType = "BTH";
			declaration.JE_RL_NKPortOfArrival = "CN";
			AssertNoMessageErrorContaining(declaration.JE_RL_NKPortOfArrivalInfo, "Port of Discharge should be a port in China.");
			declaration.JE_RL_NKPortOfArrival = "A";
			AssertHasMessageErrorContaining(declaration.JE_RL_NKPortOfArrivalInfo, "Port of Discharge should be a port in China.");
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			declaration.Validation.ValidateJE_RL_NKPortOfArrival();
			AssertNoMessageErrorContaining(declaration.JE_RL_NKPortOfArrivalInfo, "Port of Discharge should be a port in China.");
		}

		public void TestCheckJE_RL_NKPortOfLoading()
		{
			declaration.JE_MessageSubType = "BTH";
			declaration.JE_RL_NKPortOfLoading = "CN";
			AssertNoMessageErrorContaining(declaration.JE_RL_NKPortOfLoadingInfo, "Port of Loading should be a port in China.");
			declaration.JE_RL_NKPortOfLoading = "A";
			AssertHasMessageErrorContaining(declaration.JE_RL_NKPortOfLoadingInfo, "Port of Loading should be a port in China.");
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			declaration.Validation.ValidateJE_RL_NKPortOfLoading();
			AssertNoMessageErrorContaining(declaration.JE_RL_NKPortOfLoadingInfo, "Port of Loading should be a port in China.");
		}

		public void TestCheckJE_RL_NKFinalDestination()
		{
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.Validation.ValidateJE_RL_NKFinalDestination();
			AssertHasMessageErrorContaining(declaration.JE_RL_NKFinalDestinationInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_RL_NKOrigin()
		{
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.Validation.ValidateJE_RL_NKOrigin();
			AssertHasMessageErrorContaining(declaration.JE_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_TotalNoOfPacksPackType()
		{
			declaration.JE_TotalNoOfPacks = 20;
			declaration.JE_TotalNoOfPacksPackType = "";
			AssertHasWarning(declaration.JE_TotalNoOfPacksPackTypeInfo, "Package type is required when package is greater than 0.");
			declaration.JE_TotalNoOfPacksPackType = "XX";
			AssertNoMessageErrorContaining(declaration.JE_TotalNoOfPacksPackTypeInfo, "Package type is required when package is greater than 0.");
			AssertHasWarning(declaration.JE_TotalNoOfPacksPackTypeInfo, "You have not entered a valid code.");
			declaration.JE_TotalNoOfPacksPackType = Core.Constants.PkgUnit.Bag;
			AssertNoWarnings(declaration.JE_TotalNoOfPacksPackTypeInfo);
		}

		public void TestCheckJE_MasterBill()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.Validation.ValidateJE_MasterBill();
			AssertNoMessageErrorContaining(declaration.JE_MasterBillInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageErrorContaining(declaration.JE_MasterBillInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageErrorContaining(declaration.JE_MasterBillInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageErrorContaining(declaration.JE_MasterBillInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageErrorContaining(declaration.JE_MasterBillInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_MasterBill = "MB123456789012";
			AssertNoMessageErrorContaining(declaration.JE_MasterBillInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.JE_MasterBillInfo, "Transportation Batch Number should be 13 alphanumeric characters");
			declaration.JE_MasterBill = "MB12345678900";
			AssertNoMessageErrorContaining(declaration.JE_MasterBillInfo, "Transportation Batch Number should be 13 alphanumeric characters");
			declaration.JE_MasterBill = "MB1234567890@";
			AssertHasMessageErrorContaining(declaration.JE_MasterBillInfo, "Transportation Batch Number should be 13 alphanumeric characters");
		}

		public void TestCheckJE_CustomsOffice()
		{
			var universalDataHelper = new UniversalReferenceTestDataHelper(Factory);
			universalDataHelper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office Code");
			universalDataHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BJB", "BJB", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));
			universalDataHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "A00", "A00", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));
			Factory.Save();
			declaration.JE_CustomsOffice = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_CustomsOffice = "XXX";
			AssertHasMessageErrorContaining(declaration.JE_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_CustomsOffice = "BJB";
			AssertNoNotifications(declaration.JE_CustomsOfficeInfo);
			declaration.JE_CustomsOffice = "A00";
			AssertHasMessageErrorContaining(declaration.JE_CustomsOfficeInfo, "The Customs Office should not be a directly competent Customs (ends with 00)");
		}

		public void TestCheckAgentOrg()
		{
			var wanCCD = ValidationHelper.OrgProxyRegNumIsRequired(OrgCusCode.CodeTypes.CustomsClientCode);
			var wanUSC = ValidationHelper.OrgProxyRegNumIsRequired(OrgCusCode.ChinaCodeTypes.USC);
			var wanCIQ = ValidationHelper.OrgProxyRegNumIsRequired(OrgCusCode.ChinaCodeTypes.CIQ);
			var declaration = Factory.New<JobDeclaration>();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction1.PK;
			invoiceLine1.JI_CIQTariff = "1111";
			instruction1.CEI_CIQRequires = true;
			declaration.Validation.ValidateJE_GB();
			AssertHasMessageError(declaration.JE_GBInfo, wanCCD);
			AssertHasMessageError(declaration.JE_GBInfo, wanUSC);
			AssertHasMessageError(declaration.JE_GBInfo, wanCIQ);
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "Agent";
			var cn = Factory.Load<RefCountry>(Core.Constants.CountryGuids.China);
			orgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "Y134567891234567A71", cn);
			orgProxy.CustomsCodes.AddNew(OrgCusCode.ChinaCodeTypes.USC, "Y134567891234567A72", cn);
			orgProxy.CustomsCodes.AddNew(OrgCusCode.ChinaCodeTypes.CIQ, "Y134567891234567A72", cn);
			declaration = Factory.New<JobDeclaration>();
			instruction1 = declaration.CustomsEntryInstructions.AddNew();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction1.PK;
			invoiceLine1.JI_CIQTariff = "1111";
			instruction1.CEI_CIQRequires = true;
			declaration.Branch.GB_OH_OrgProxy = orgProxy.PK;
			declaration.Validation.ValidateJE_GB();
			AssertNoMessageErrorContaining(declaration.JE_GBInfo, wanCCD);
			AssertNoMessageErrorContaining(declaration.JE_GBInfo, wanUSC);
			AssertNoMessageErrorContaining(declaration.JE_GBInfo, wanCIQ);
		}

		public void TestCheckJE_VoyageFlightNo()
		{
			declaration.JE_TransportMode = "RAI";
			declaration.Validation.ValidateJE_VoyageFlightNo();
			AssertNoMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportMode = "AIR";
			declaration.Validation.ValidateJE_VoyageFlightNo();
			AssertHasMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportMode = "SEA";
			declaration.Validation.ValidateJE_VoyageFlightNo();
			AssertHasMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_VoyageFlightNo = "123456";
			AssertNoWarningContaining(declaration.JE_VoyageFlightNoInfo, JobDeclarationValidation.VoyageBeyondWarningLimitLength);
			declaration.JE_VoyageFlightNo = "123456789";
			AssertHasWarningContaining(declaration.JE_VoyageFlightNoInfo, JobDeclarationValidation.VoyageBeyondWarningLimitLength);
		}

		public void TestCheckJE_VesselName()
		{
			declaration.JE_TransportMode = "RAI";
			declaration.Validation.ValidateJE_VesselName();
			AssertNoMessageErrorContaining(declaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportMode = "SEA";
			declaration.Validation.ValidateJE_VesselName();
			AssertHasMessageErrorContaining(declaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_VesselName = "@0235#";
			AssertHasWarningContaining(declaration.JE_VesselNameInfo, ListValidation.InvalidCodeMessage);
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "12345678901234567890XXXXXX";
			declaration.JE_VesselName = vessel.RV_Code;
			AssertHasWarningContaining(declaration.JE_VesselNameInfo, JobDeclarationValidation.VesselBeyondWarningLimitLength);
			vessel.RV_Code = "1234567890123456789";
			declaration.JE_VesselName = vessel.RV_Code;
			AssertNoWarningContaining(declaration.JE_VesselNameInfo, JobDeclarationValidation.VesselBeyondWarningLimitLength);
		}

		public void TestCheckJE_TransportModeInland()
		{
			declaration.JE_TransitMode = TransitModeList.Codes.Transshipment;
			declaration.JE_TransportModeInland = Enterprise.Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport;
			AssertNoWarningContaining(declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportModeInland = "";
			AssertHasWarningContaining(declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransitMode = "";
			declaration.Validation.ValidateJE_TransportModeInland();
			AssertNoWarningContaining(declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransitMode = TransitModeList.Codes.Transshipment;
			declaration.Validation.ValidateJE_TransportModeInland();
			AssertHasWarningContaining("Has warning when IsCustomsTransit is true", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.Validation.ValidateJE_TransportModeInland();
			AssertHasMessageErrorContaining("Message error when EXP/T/SEA", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_MessageSubType()
		{
			var messageSubTypeInfo = declaration.JE_MessageSubTypeInfo;
			var propDescription = messageSubTypeInfo.HasHumanReadableName ? messageSubTypeInfo.HumanReadableName.ToString() : "value";
			var youHaveNotEnterd = MandatoryValidation.YouHaveNotEnteredMessage(propDescription);
			declaration.JE_MessageSubType = "";
			declaration.Validation.ValidateJE_MessageSubType();
			AssertHasMessageErrorContaining(messageSubTypeInfo, youHaveNotEnterd);
			declaration.JE_MessageSubType = "XXX";
			declaration.Validation.ValidateJE_MessageSubType();
			AssertHasMessageErrorContaining(messageSubTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			declaration.Validation.ValidateJE_MessageSubType();
			AssertNoMessageErrorContaining(messageSubTypeInfo, youHaveNotEnterd);
			AssertNoMessageErrorContaining(messageSubTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJE_ExportDate()
		{
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			declaration.JE_CNPortOfDestination = "A";
			declaration.JE_CNPortOfOrigin = "A";
			declaration.JE_ExportDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(declaration.JE_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_ExportDate = ZDateTime.Now;
			AssertNoMessageErrorContaining(declaration.JE_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_CIQRequires = true;
			declaration.JE_ExportDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(declaration.JE_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);
			instruction.CEI_CIQRequires = false;
			declaration.Validation.ValidateJE_ExportDate();
			AssertNoMessageErrorContaining(declaration.JE_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);
			instruction.CEI_CIQRequires = true;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.Validation.ValidateJE_ExportDate();
			AssertNoMessageErrorContaining(declaration.JE_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public override void TestCheckJE_DateOfArrival()
		{
			base.TestCheckJE_DateOfArrival();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_DateOfArrival = ZDateTime.Empty;
			AssertHasMessageErrorContaining(declaration.JE_DateOfArrivalInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_DateOfArrival = ZDateTime.Now;
			AssertNoMessageErrorContaining(declaration.JE_DateOfArrivalInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_LocationOfGoods()
		{
			this.declaration.Validation.ValidateJE_LocationOfGoods();
			AssertNoMessageErrorContaining(this.declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);
			this.declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			this.declaration.Validation.ValidateJE_LocationOfGoods();
			AssertHasMessageErrorContaining(this.declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);
			this.declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			this.declaration.JE_MessageSubType = "";
			this.declaration.Validation.ValidateJE_LocationOfGoods();
			AssertHasMessageErrorContaining(this.declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);
			this.declaration.JE_LocationOfGoods = "Test";
			AssertNoMessageErrors(this.declaration.JE_LocationOfGoodsInfo);
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_CIQRequires = true;
			declaration.Validation.ValidateJE_LocationOfGoods();
			AssertHasMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);
			instruction.CEI_CIQRequires = false;
			declaration.Validation.ValidateJE_LocationOfGoods();
			AssertNoMessageErrors(declaration.JE_LocationOfGoodsInfo);
		}

		public void TestCheckJE_GS_NKCusAgent()
		{
			var testInfo = declaration.JE_GS_NKCusAgentInfo;
			declaration.Validation.ValidateJE_GS_NKCusAgent();
			AssertHasWarningContaining(testInfo, MandatoryValidation.YouHaveNotEntered);
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			broker.GS_Code = "TBR";
			broker.GS_FullName = "Test Broker";
			declaration.JE_GS_NKCusAgent = "TBR";
			AssertNoWarningContaining(testInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarning(testInfo, "Broker should have an effective e-port operator card ID (CNO).");
			var cnBrk = broker.AddCert("BRK", "CN", "");
			var cnCNO = broker.AddCert("CNO", "CN", "");
			declaration.Validation.ValidateJE_GS_NKCusAgent();
			AssertHasWarning(testInfo, "Broker should have an effective e-port operator card ID (CNO).");
			AssertNoWarning(testInfo, "The broker name seems not a Chinese name. Please enter the Chinese name as Comments of Certificate BRK/CN.");
			AssertNoWarning(testInfo, "The broker name seems not a Chinese name. Please enter the Chinese name as Comments of Certificate CNO/CN.");
			cnBrk.XZ_RefNumber = "BRK001";
			cnCNO.XZ_RefNumber = "CNO001";
			declaration.Validation.ValidateJE_GS_NKCusAgent();
			AssertNoWarning(testInfo, "Broker should have an effective e-port operator card ID (CNO).");
			AssertHasWarning(testInfo, "The broker name seems not a Chinese name. Please enter the Chinese name as Comments of Certificate BRK/CN.");
			AssertHasWarning(testInfo, "The broker name seems not a Chinese name. Please enter the Chinese name as Comments of Certificate CNO/CN.");
			cnBrk.XZ_Comment = "Test Broker BRK Name";
			cnCNO.XZ_Comment = "Test Broker CNO Name";
			declaration.Validation.ValidateJE_GS_NKCusAgent();
			AssertHasWarning(testInfo, "The broker name seems not a Chinese name. Please enter the Chinese name as Comments of Certificate BRK/CN.");
			AssertHasWarning(testInfo, "The broker name seems not a Chinese name. Please enter the Chinese name as Comments of Certificate CNO/CN.");
			cnBrk.XZ_Comment = "测试名BRK";
			cnCNO.XZ_Comment = "测试名CNO";
			declaration.Validation.ValidateJE_GS_NKCusAgent();
			AssertNoWarning(testInfo, "The broker name seems not a Chinese name. Please enter the Chinese name as Comments of Certificate BRK/CN.");
			AssertNoWarning(testInfo, "The broker name seems not a Chinese name. Please enter the Chinese name as Comments of Certificate CNO/CN.");
		}

		public void TestValidationModeProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			ValidationExtensionsTest.AssertValidationModeProvider(declaration, declaration.Validation.ValidationModeProvider);
		}

		#region AddInfo Properties

		public void TestCheckJE_LicenseInvolved()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
			AssertNoMessageErrorContaining(testDeclaration.JE_LicenseInvolvedInfo, "License Involved should be ticked when any supporting documents or product qualifications entered on any Invoice Line.");
			var invoiceHeader = testDeclaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.CIQProductQualifications.AddNew();
			testDeclaration.JE_LicenseInvolved = false;
			AssertHasMessageErrorContaining(testDeclaration.JE_LicenseInvolvedInfo, "License Involved should be ticked when any supporting documents or product qualifications entered on any Invoice Line.");
			invoiceLine.CIQProductQualifications.RemoveAndDeleteAll();
			invoiceLine.CusSupportingDocuments.AddNew();
			testDeclaration.JE_LicenseInvolved = false;
			AssertHasMessageErrorContaining(testDeclaration.JE_LicenseInvolvedInfo, "License Involved should be ticked when any supporting documents or product qualifications entered on any Invoice Line.");
			testDeclaration.JE_LicenseInvolved = true;
			AssertNoMessageErrorContaining(testDeclaration.JE_LicenseInvolvedInfo, "License Involved should be ticked when any supporting documents or product qualifications entered on any Invoice Line.");
			testDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
			testDeclaration.JE_LicenseInvolved = false;
			AssertNoMessageErrorContaining(testDeclaration.JE_LicenseInvolvedInfo, "License Involved should be ticked when any supporting documents or product qualifications entered on any Invoice Line.");
		}

		public void TestCheckJE_InspectionInvolved()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
			var instruction1 = Factory.New<CusEntryInstruction>();
			instruction1.CEI_CIQRequires = true;
			var instruction2 = Factory.New<CusEntryInstruction>();
			instruction2.CEI_CIQRequires = false;
			testDeclaration.CustomsEntryInstructions.Add(instruction1);
			testDeclaration.CustomsEntryInstructions.Add(instruction2);
			testDeclaration.JE_InspectionInvolved = false;
			AssertHasMessageErrorContaining(testDeclaration.JE_InspectionInvolvedInfo, "Inspection & Quarantine Involved should be ticked when the Entry Instruction requires CIQ.");
			instruction1.CEI_CIQRequires = false;
			testDeclaration.JE_InspectionInvolved = false;
			AssertNoMessageErrorContaining(testDeclaration.JE_InspectionInvolvedInfo, "Inspection & Quarantine Involved should be ticked when the Entry Instruction requires CIQ.");
			instruction1.CEI_CIQRequires = true;
			testDeclaration.JE_InspectionInvolved = true;
			AssertNoMessageErrorContaining(testDeclaration.JE_InspectionInvolvedInfo, "Inspection & Quarantine Involved should be ticked when the Entry Instruction requires CIQ.");
			testDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
			testDeclaration.JE_InspectionInvolved = false;
			AssertNoMessageErrorContaining(testDeclaration.JE_InspectionInvolvedInfo, "Inspection & Quarantine Involved should be ticked when the Entry Instruction requires CIQ.");
		}

		public void TestCheckJE_TaxInvolved()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
			var invoiceHeader = testDeclaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_DutyMode = DutyModeList.Codes._2;
			var invoiceLine2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_DutyMode = DutyModeList.Codes._3;
			testDeclaration.JE_TaxInvolved = false;
			AssertHasMessageErrorContaining(testDeclaration.JE_TaxInvolvedInfo, $"Tax Involved should be ticked when any duty mode on any Invoice Line is not {DutyModeList.Codes._3} - {DutyModeList.Descriptions._3}.");
			invoiceLine1.JI_DutyMode = DutyModeList.Codes._3;
			testDeclaration.JE_TaxInvolved = false;
			AssertNoMessageErrorContaining(testDeclaration.JE_TaxInvolvedInfo, $"Tax Involved should be ticked when any duty mode on any Invoice Line is not {DutyModeList.Codes._3} - {DutyModeList.Descriptions._3}.");
			invoiceLine1.JI_DutyMode = DutyModeList.Codes._2;
			testDeclaration.JE_TaxInvolved = true;
			AssertNoMessageErrorContaining(testDeclaration.JE_TaxInvolvedInfo, $"Tax Involved should be ticked when any duty mode on any Invoice Line is not {DutyModeList.Codes._3} - {DutyModeList.Descriptions._3}.");
			testDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
			testDeclaration.JE_TaxInvolved = false;
			AssertNoMessageErrorContaining(testDeclaration.JE_TaxInvolvedInfo, $"Tax Involved should be ticked when any duty mode on any Invoice Line is not {DutyModeList.Codes._3} - {DutyModeList.Descriptions._3}.");
		}

		public void TestCheckJE_CNTransportMode()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_CNTransportMode = "E";
			AssertHasMessageErrorContaining(testDeclaration.JE_CNTransportModeInfo, ListValidation.InvalidCodeMessageError);
			testDeclaration.JE_CNTransportMode = "X";
			AssertNoMessageErrorContaining(testDeclaration.JE_CNTransportModeInfo, ListValidation.InvalidCodeMessageError);
			testDeclaration.JE_CNTransportMode = "";
			testDeclaration.JE_RL_NKPortOfLoading = "CN";
			testDeclaration.JE_RL_NKPortOfArrival = "";
			AssertNoMessageErrorContaining(testDeclaration.JE_CNTransportModeInfo, "Transport Flow is required for a domestic transport.");
			testDeclaration.JE_RL_NKPortOfArrival = "CN";
			testDeclaration.JE_CNTransportMode = "";
			AssertHasMessageErrorContaining(testDeclaration.JE_CNTransportModeInfo, "Transport Flow is required for a domestic transport.");
			AssertNoMessageErrorContaining(testDeclaration.JE_CNTransportModeInfo, "Transport Flow is not required for a cross-border transport.");
			testDeclaration.JE_RL_NKPortOfArrival = "";
			testDeclaration.JE_CNTransportMode = "E";
			AssertHasMessageErrorContaining(testDeclaration.JE_CNTransportModeInfo, "Transport Flow is not required for a cross-border transport.");
			testDeclaration.JE_MessageSubType = "BTH";
			testDeclaration.Validation.ValidateJE_CNTransportMode();
			AssertNoMessageErrorContaining(testDeclaration.JE_CNTransportModeInfo, "Transport Flow is not required for a cross-border transport.");
		}

		[TestDate(2017, 12, 19)]
		public void TestCheckJE_CNPortOfOrigin()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("PORT", "test code type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "PORT", "A", "test code", new ZDateTime(2017, 12, 18), new ZDateTime(2017, 12, 20));
			Factory.Save();
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = "EXP";
			testDeclaration.JE_MessageSubType = "CUS";
			testDeclaration.Validation.ValidateJE_CNPortOfOrigin();
			AssertNoMessageErrorContaining(testDeclaration.JE_CNPortOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			testDeclaration.JE_MessageSubType = "BTH";
			testDeclaration.Validation.ValidateJE_CNPortOfOrigin();
			AssertHasMessageErrorContaining(testDeclaration.JE_CNPortOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			testDeclaration.JE_MessageSubType = "CUS";
			testDeclaration.JE_MessageType = "IMP";
			testDeclaration.Validation.ValidateJE_CNPortOfOrigin();
			AssertHasMessageErrorContaining(testDeclaration.JE_CNPortOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			testDeclaration.JE_CNPortOfOrigin = "E";
			AssertHasMessageErrorContaining(testDeclaration.JE_CNPortOfOriginInfo, ListValidation.InvalidCodeMessageError);
			testDeclaration.JE_CNPortOfOrigin = "A";
			AssertNoMessageErrorContaining(testDeclaration.JE_CNPortOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(testDeclaration.JE_CNPortOfOriginInfo, ListValidation.InvalidCodeMessageError);
		}

		[TestDate(2017, 12, 19)]
		public void TestCheckJE_CNPortOfDestination()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("PORT", "test code type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "PORT", "A", "test code", new ZDateTime(2017, 12, 18), new ZDateTime(2017, 12, 20));
			Factory.Save();
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = "IMP";
			testDeclaration.JE_MessageSubType = "CUS";
			testDeclaration.Validation.ValidateJE_CNPortOfDestination();
			AssertNoMessageErrorContaining(testDeclaration.JE_CNPortOfDestinationInfo, MandatoryValidation.YouHaveNotEntered);
			testDeclaration.JE_MessageSubType = "BTH";
			testDeclaration.Validation.ValidateJE_CNPortOfDestination();
			AssertHasMessageErrorContaining(testDeclaration.JE_CNPortOfDestinationInfo, MandatoryValidation.YouHaveNotEntered);
			testDeclaration.JE_MessageType = "EXP";
			testDeclaration.JE_MessageSubType = "CUS";
			testDeclaration.Validation.ValidateJE_CNPortOfDestination();
			AssertHasMessageErrorContaining(testDeclaration.JE_CNPortOfDestinationInfo, MandatoryValidation.YouHaveNotEntered);
			testDeclaration.JE_CNPortOfDestination = "E";
			AssertHasMessageErrorContaining(testDeclaration.JE_CNPortOfDestinationInfo, ListValidation.InvalidCodeMessageError);
			testDeclaration.JE_CNPortOfDestination = "A";
			AssertNoMessageErrorContaining(testDeclaration.JE_CNPortOfDestinationInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(testDeclaration.JE_CNPortOfDestinationInfo, ListValidation.InvalidCodeMessageError);
		}

		[TestDate(2018, 8, 13)]
		public void TestCheckJE_CNLastPortBeforeEntry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("PORT", "test code type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "PORT", "A", "test code", new ZDateTime(2018, 8, 1), new ZDateTime(2018, 8, 31));
			Factory.Save();
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			testDeclaration.Validation.ValidateJE_CNLastPortBeforeEntry();
			AssertHasMessageErrorContaining(testDeclaration.JE_CNLastPortBeforeEntryInfo, MandatoryValidation.YouHaveNotEntered);
			testDeclaration.JE_CNLastPortBeforeEntry = "X";
			AssertHasMessageErrorContaining(testDeclaration.JE_CNLastPortBeforeEntryInfo, ListValidation.InvalidCodeMessageError);
			testDeclaration.JE_CNLastPortBeforeEntry = "A";
			AssertNoMessageErrors(testDeclaration.JE_CNLastPortBeforeEntryInfo);
		}

		[TestDate(2018, 8, 13)]
		public void TestCheckJE_CIQOfficeOfEntryExit()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CIQPO", "CN CIQ Ports");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "CIQPO", "CP01", "CN CIQ Port 01", new ZDateTime(2018, 8, 1), new ZDateTime(2018, 8, 31));
			Factory.Save();
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var targetInfo = testDeclaration.JE_CIQOfficeOfEntryExitInfo;
			testDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			testDeclaration.Validation.ValidateJE_CIQOfficeOfEntryExit();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			testDeclaration.JE_CIQOfficeOfEntryExit = "X";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			testDeclaration.JE_CIQOfficeOfEntryExit = "CP01";
			AssertNoMessageErrors(targetInfo);
		}

		public void TestCheckJE_RN_NKCountryOfTrade()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_RN_NKCountryOfTrade = ZString.Empty;
			var targetInfo = declaration.JE_RN_NKCountryOfTradeInfo;
			AssertHasMessageErrorContaining(targetInfo, "entered");
			declaration.JE_RN_NKCountryOfTrade = "00";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.CustomsCodes.AddNew("MMR", "MMR001", Core.Constants.CountryCodes.China);
			supplier.MainAddress.OA_RN_NKCountryCode = "US";
			declaration.JE_OH_Supplier = supplier.PK;
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew("MMR", "MMR002", Core.Constants.CountryCodes.China);
			importer.MainAddress.OA_RN_NKCountryCode = "US";

			declaration.SupplierDocumentaryAddress.OverseasPartyCodeType = OrgCusCode.ChinaCodeTypes.AEO;
			declaration.JE_RN_NKCountryOfTrade = "US";

			declaration.JE_MessageType = "IMP";
			declaration.Validation.ValidateJE_RN_NKCountryOfTrade();
			AssertNoMessageErrorContaining(targetInfo, "Country of Trade should match the Country Code of Supplier.");

			declaration.SupplierDocumentaryAddress.OverseasPartyCodeType = OrgCusCode.ChinaCodeTypes.MMR;
			declaration.Validation.ValidateJE_RN_NKCountryOfTrade();
			AssertNoMessageErrorContaining(targetInfo, "Country of Trade should match the Country Code of Supplier.");

			declaration.JE_RN_NKCountryOfTrade = "AU";
			declaration.Validation.ValidateJE_RN_NKCountryOfTrade();
			AssertHasMessageErrorContaining(targetInfo, "Country of Trade should match the Country Code of Supplier.");

			declaration.JE_MessageType = "EXP";
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_RN_NKCountryOfTrade = "AU";
			AssertHasMessageErrorContaining(targetInfo, "Country of Trade should match the Country Code of Importer.");

			declaration.JE_RN_NKCountryOfTrade = "US";
			AssertNoMessageErrorContaining(targetInfo, "Country of Trade should match the Country Code of Importer.");
		}

		public void TestCheckJE_ClearanceMode()
		{
			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(new Guid(), new Guid(), new Guid(), true))
			{
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				testDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				testDeclaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
				testDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
				AssertNoMessageErrors(testDeclaration.JE_ClearanceModeInfo);
				testDeclaration.JE_ClearanceMode = "";
				AssertHasMessageErrorContaining(testDeclaration.JE_ClearanceModeInfo, "You have not entered");
				testDeclaration.JE_ClearanceMode = "ABC";
				AssertHasMessageErrorContaining(testDeclaration.JE_ClearanceModeInfo, "The code you have selected is not in the list");
			}
		}

		public void TestCheckJE_ClearanceModeIsTwoStepShouldHaveAtMostOneEntry()
		{
			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(new Guid(), new Guid(), new Guid(), true))
			{
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				var instruction1 = Factory.New<CusEntryInstruction>();
				testDeclaration.CustomsEntryInstructions.Add(instruction1);
				testDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				AssertNoErrorContaining(testDeclaration.JE_ClearanceModeInfo, "Only one Entry Instruction is allowed for two-step declaration clearance mode.");
				var instruction2 = Factory.New<CusEntryInstruction>();
				testDeclaration.CustomsEntryInstructions.Add(instruction2);
				testDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				AssertHasErrorContaining(testDeclaration.JE_ClearanceModeInfo, "Only one Entry Instruction is allowed for two-step declaration clearance mode.");
			}
		}

		public void TestCheckJE_ClearanceModeIsTwoStepSupportingDocuments()
		{
			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(new Guid(), new Guid(), new Guid(), true))
			{
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Constants.UniversalReferenceConstants.CusCodeListAttributeName.TSDSupported, "Desc.", UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments);
				var code1 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD1", "Code 1");
				code1.Attributes.AddNew(Constants.UniversalReferenceConstants.CusCodeListAttributeName.TSDSupported, ZString.Empty);
				Factory.Save();
				var invoiceHeader = testDeclaration.Invoices.AddNew();
				var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
				var cusSupportingDocument = invoiceLine.CusSupportingDocuments.AddNew();
				cusSupportingDocument.CSI_Code = "GG";
				testDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				AssertHasMessageErrorContaining(testDeclaration.JE_ClearanceModeInfo, "There are some Supporting Documents on Invoice Lines are not supported in two-step declaration clearance mode.");
				cusSupportingDocument.CSI_Code = "CD1";
				testDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				AssertNoMessageErrorContaining(testDeclaration.JE_ClearanceModeInfo, "There are some Supporting Documents on Invoice Lines are not supported in two-step declaration clearance mode.");
				cusSupportingDocument.CSI_Code = "GG";
				testDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
				AssertNoMessageErrorContaining(testDeclaration.JE_ClearanceModeInfo, "There are some Supporting Documents on Invoice Lines are not supported in two-step declaration clearance mode.");
			}
		}

		public void TestCheckJE_ClearanceModeIsTwoStepSupportingProductQualificationCode()
		{
			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(new Guid(), new Guid(), new Guid(), true))
			{
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				var invoiceHeader = testDeclaration.Invoices.AddNew();
				var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
				var instruction1 = Factory.New<CusEntryInstruction>();
				instruction1.CEI_CIQRequires = true;
				testDeclaration.CustomsEntryInstructions.Add(instruction1);
				var cIQProductQualification = invoiceLine.CIQProductQualifications.AddNew();
				cIQProductQualification.CSI_Code = ProductQualificationCodeList.Codes._103;
				testDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				AssertHasMessageErrorContaining(testDeclaration.JE_ClearanceModeInfo, "There are some Product Qualifications on Invoice Lines are not supported in two-step declaration clearance mode.");
				instruction1.CEI_CIQRequires = false;
				testDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				AssertNoMessageErrorContaining(testDeclaration.JE_ClearanceModeInfo, "There are some Product Qualifications on Invoice Lines are not supported in two-step declaration clearance mode.");
				instruction1.CEI_CIQRequires = true;
				cIQProductQualification.CSI_Code = ProductQualificationCodeList.Codes._330;
				testDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				AssertNoMessageErrorContaining(testDeclaration.JE_ClearanceModeInfo, "There are some Product Qualifications on Invoice Lines are not supported in two-step declaration clearance mode.");
				cIQProductQualification.CSI_Code = ProductQualificationCodeList.Codes._103;
				testDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
				AssertNoMessageErrorContaining(testDeclaration.JE_ClearanceModeInfo, "There are some Product Qualifications on Invoice Lines are not supported in two-step declaration clearance mode.");
			}
		}

		public void TestCheckJE_ClearanceModeIsTwoStepAndTariffNotSupportTSD()
		{
			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(new Guid(), new Guid(), new Guid(), true))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateCustomsTariff("0000000001");
				helper.CreateCustomsTariff("0000000002");
				var tariffNotSupportTSD = helper.CreateCustomsTariff("0502103000");
				helper.CreateTariffAttribute("SupportsTSD", "N", tariffNotSupportTSD);

				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				testDeclaration.JE_MessageType = "IMP";
				testDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;

				var invoiceHeader = testDeclaration.Invoices.AddNew();
				var invoiceLine1 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
				invoiceLine1.JI_Tariff = "0000000001";
				testDeclaration.Validation.ValidateJE_ClearanceMode();
				AssertNoMessageError(testDeclaration.JE_ClearanceModeInfo, "There are some Tariffs on Invoice Lines are not supported in two-step declaration clearance mode." + System.Environment.NewLine + "(e.g. 050210)");

				var invoiceLine2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
				testDeclaration.Validation.ValidateJE_ClearanceMode();
				AssertNoMessageError(testDeclaration.JE_ClearanceModeInfo, "There are some Tariffs on Invoice Lines are not supported in two-step declaration clearance mode." + System.Environment.NewLine + "(e.g. 050210)");

				invoiceLine2.JI_Tariff = "0000000002";
				testDeclaration.Validation.ValidateJE_ClearanceMode();
				AssertNoMessageError(testDeclaration.JE_ClearanceModeInfo, "There are some Tariffs on Invoice Lines are not supported in two-step declaration clearance mode." + System.Environment.NewLine + "(e.g. 050210)");

				var invoiceLine3 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
				invoiceLine3.JI_Tariff = "0502103000";
				testDeclaration.Validation.ValidateJE_ClearanceMode();
				AssertHasWarning(testDeclaration.JE_ClearanceModeInfo, "There are some Tariffs on Invoice Lines are not supported in two-step declaration clearance mode." + System.Environment.NewLine + "(e.g. 050210)");

				testDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
				testDeclaration.Validation.ValidateJE_ClearanceMode();
				AssertNoMessageError(testDeclaration.JE_ClearanceModeInfo, "There are some Tariffs on Invoice Lines are not supported in two-step declaration clearance mode." + System.Environment.NewLine + "(e.g. 050210)");
			}
		}

		public void TestCheckJE_ClearanceModeIsTwoStepAndTariffNotSupportTSD_MoreInvoiceLines()
		{
			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(new Guid(), new Guid(), new Guid(), true))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);

				var tariff0 = helper.CreateCustomsTariff("0000000000");
				helper.CreateTariffAttribute("SupportsTSD", "N", tariff0);

				var tariff1 = helper.CreateCustomsTariff("1000000000");
				helper.CreateTariffAttribute("SupportsTSD", "N", tariff1);

				var tariff2 = helper.CreateCustomsTariff("2000000000");
				helper.CreateTariffAttribute("SupportsTSD", "N", tariff2);

				var tariff3 = helper.CreateCustomsTariff("3000000000");
				helper.CreateTariffAttribute("SupportsTSD", "N", tariff3);

				var tariff4 = helper.CreateCustomsTariff("4000000000");
				helper.CreateTariffAttribute("SupportsTSD", "N", tariff4);

				var tariff5 = helper.CreateCustomsTariff("5000000000");
				helper.CreateTariffAttribute("SupportsTSD", "N", tariff5);

				var tariff6 = helper.CreateCustomsTariff("6000000000");
				helper.CreateTariffAttribute("SupportsTSD", "N", tariff6);

				var tariff7 = helper.CreateCustomsTariff("7000000000");
				helper.CreateTariffAttribute("SupportsTSD", "N", tariff7);

				var tariff8 = helper.CreateCustomsTariff("8000000000");
				helper.CreateTariffAttribute("SupportsTSD", "N", tariff8);

				var tariff9 = helper.CreateCustomsTariff("9000000000");
				helper.CreateTariffAttribute("SupportsTSD", "N", tariff9);

				var tariff10 = helper.CreateCustomsTariff("1100000000");
				helper.CreateTariffAttribute("SupportsTSD", "N", tariff10);

				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				testDeclaration.JE_MessageType = "IMP";
				testDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;

				var invoiceHeader = testDeclaration.Invoices.AddNew();

				var invoiceLine00 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
				invoiceLine00.JI_Tariff = "0000000000";

				var invoiceLine01 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
				invoiceLine01.JI_Tariff = "1000000000";

				var invoiceLine02 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
				invoiceLine02.JI_Tariff = "2000000000";

				var invoiceLine03 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
				invoiceLine03.JI_Tariff = "3000000000";

				var invoiceLine04 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
				invoiceLine04.JI_Tariff = "4000000000";

				var invoiceLine05 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
				invoiceLine05.JI_Tariff = "5000000000";

				var invoiceLine10 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
				invoiceLine10.JI_Tariff = "6000000000";

				var invoiceLine06 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
				invoiceLine06.JI_Tariff = "7000000000";

				var invoiceLine07 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
				invoiceLine07.JI_Tariff = "8000000000";

				var invoiceLine08 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
				invoiceLine08.JI_Tariff = "9000000000";

				var invoiceLine09 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
				invoiceLine09.JI_Tariff = "1100000000";

				testDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				testDeclaration.Validation.ValidateJE_ClearanceMode();
				AssertHasWarning(testDeclaration.JE_ClearanceModeInfo, "There are some Tariffs on Invoice Lines are not supported in two-step declaration clearance mode."
					+ System.Environment.NewLine + "(e.g. 000000,100000,200000,300000,400000,500000,600000,700000,800000,900000,...)");
			}
		}

		public void TestCheckJE_TransitMode()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_TransitMode = "X";
			AssertHasMessageErrorContaining(testDeclaration.JE_TransitModeInfo, ListValidation.InvalidCodeMessageError);
			testDeclaration.JE_TransitMode = "A";
			AssertNoMessageErrorContaining(testDeclaration.JE_TransitModeInfo, ListValidation.InvalidCodeMessageError);
			testDeclaration.JE_MessageSubType = "CUS";
			testDeclaration.Validation.ValidateJE_TransitMode();
			AssertNoMessageErrorContaining(testDeclaration.JE_TransitModeInfo, "Transit Mode is not available for Declaration Type 'BTH'.");
			testDeclaration.JE_MessageSubType = "BTH";
			testDeclaration.Validation.ValidateJE_TransitMode();
			AssertHasMessageErrorContaining(testDeclaration.JE_TransitModeInfo, "Transit Mode is not available for Declaration Type 'BTH'.");
			testDeclaration.JE_TransitMode = "";
			AssertNoMessageErrorContaining(testDeclaration.JE_TransitModeInfo, "Transit Mode is not available for Declaration Type 'BTH'.");
			var dtdOrGclRequiredMessage = JobDeclarationValidation.DTDOrGclRequiredMessage;
			testDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			testDeclaration.JE_TransportMode = "";
			testDeclaration.JE_TransitMode = "A";
			AssertHasMessageErrorContaining(testDeclaration.JE_TransitModeInfo, dtdOrGclRequiredMessage);
			testDeclaration.JE_TransitMode = "D";
			AssertHasMessageErrorContaining(testDeclaration.JE_TransitModeInfo, dtdOrGclRequiredMessage);
			testDeclaration.JE_TransitMode = "";
			AssertNoMessageErrorContaining(testDeclaration.JE_TransitModeInfo, dtdOrGclRequiredMessage);
			testDeclaration.JE_TransitMode = "T";
			testDeclaration.JE_TransportMode = "ROA";
			testDeclaration.Validation.ValidateJE_TransitMode();
			AssertHasMessageErrorContaining(testDeclaration.JE_TransitModeInfo, dtdOrGclRequiredMessage);
			testDeclaration.JE_TransportMode = "SEA";
			testDeclaration.Validation.ValidateJE_TransitMode();
			AssertNoMessageErrorContaining(testDeclaration.JE_TransitModeInfo, dtdOrGclRequiredMessage);
			testDeclaration.JE_TransportMode = "RAI";
			testDeclaration.Validation.ValidateJE_TransitMode();
			AssertNoMessageErrorContaining(testDeclaration.JE_TransitModeInfo, dtdOrGclRequiredMessage);
			testDeclaration.JE_TransportMode = "AIR";
			testDeclaration.Validation.ValidateJE_TransitMode();
			AssertNoMessageErrorContaining(testDeclaration.JE_TransitModeInfo, dtdOrGclRequiredMessage);
			testDeclaration.JE_TransitMode = "";
			AssertNoMessageErrorContaining(testDeclaration.JE_TransitModeInfo, dtdOrGclRequiredMessage);
			testDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			testDeclaration.JE_TransitMode = "A";
			testDeclaration.JE_TransportMode = "SEA";
			testDeclaration.Validation.ValidateJE_TransitMode();
			AssertHasMessageErrorContaining(testDeclaration.JE_TransitModeInfo, dtdOrGclRequiredMessage);
			testDeclaration.JE_TransitMode = "T";
			AssertNoMessageErrorContaining(testDeclaration.JE_TransitModeInfo, dtdOrGclRequiredMessage);
			testDeclaration.JE_TransportMode = "AIR";
			testDeclaration.Validation.ValidateJE_TransitMode();
			AssertHasMessageErrorContaining(testDeclaration.JE_TransitModeInfo, dtdOrGclRequiredMessage);
			var referenceNumber = testDeclaration.AdditionalReferenceNumbers.AddNew();
			referenceNumber.CE_EntryType = "DTD";
			testDeclaration.Validation.ValidateJE_TransitMode();
			AssertNoMessageErrorContaining(testDeclaration.JE_TransitModeInfo, dtdOrGclRequiredMessage);
			referenceNumber.CE_EntryType = "PSL";
			testDeclaration.Validation.ValidateJE_TransitMode();
			AssertHasMessageErrorContaining(testDeclaration.JE_TransitModeInfo, dtdOrGclRequiredMessage);
			referenceNumber.CE_EntryType = "GCL";
			testDeclaration.Validation.ValidateJE_TransitMode();
			AssertNoMessageErrorContaining(testDeclaration.JE_TransitModeInfo, dtdOrGclRequiredMessage);
		}

		public void TestCheckJE_VesselInland()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			testDeclaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
			testDeclaration.JE_TransitMode = "T";
			var vehicleRoadMessage = JobDeclarationValidation.VehicleRoadMessage;
			var vesselInlandMessage = JobDeclarationValidation.VesselInlandMessage;
			var carNumberMessage = JobDeclarationValidation.CarNumberMessage;
			testDeclaration.JE_VesselInland = "A2304955";
			AssertNoWarningContaining(testDeclaration.JE_VesselInlandInfo, vehicleRoadMessage);
			testDeclaration.JE_VesselInland = "";
			AssertHasWarningContaining(testDeclaration.JE_VesselInlandInfo, vehicleRoadMessage);
			testDeclaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport;
			testDeclaration.Validation.ValidateJE_VesselInland();
			AssertNoWarningContaining(testDeclaration.JE_VesselInlandInfo, vehicleRoadMessage);
			testDeclaration.JE_TransportMode = "SEA";
			testDeclaration.Validation.ValidateJE_VesselInland();
			AssertNoMessageErrorContaining(testDeclaration.JE_VesselInlandInfo, MandatoryValidation.YouHaveNotEntered);
			testDeclaration.JE_TransitMode = "A";
			testDeclaration.Validation.ValidateJE_VesselInland();
			AssertNoMessageErrorContaining(testDeclaration.JE_VesselInlandInfo, MandatoryValidation.YouHaveNotEntered);
			testDeclaration.JE_TransitMode = "T";
			testDeclaration.JE_TransportMode = "AIR";
			testDeclaration.Validation.ValidateJE_VesselInland();
			AssertNoMessageErrorContaining(testDeclaration.JE_VesselInlandInfo, MandatoryValidation.YouHaveNotEntered);
			testDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			testDeclaration.JE_TransitMode = "A";
			testDeclaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
			testDeclaration.Validation.ValidateJE_VesselInland();
			AssertHasWarningContaining(testDeclaration.JE_VesselInlandInfo, vehicleRoadMessage);
			testDeclaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport;
			testDeclaration.Validation.ValidateJE_VesselInland();
			AssertNoWarningContaining(testDeclaration.JE_VesselInlandInfo, vehicleRoadMessage);
			testDeclaration.JE_TransitMode = "T";
			testDeclaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
			testDeclaration.Validation.ValidateJE_VesselInland();
			AssertNoWarningContaining(testDeclaration.JE_VesselInlandInfo, vehicleRoadMessage);
			testDeclaration.JE_TransportMode = "SEA";
			testDeclaration.Validation.ValidateJE_VesselInland();
			AssertHasMessageErrorContaining(testDeclaration.JE_VesselInlandInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(testDeclaration.JE_VesselInlandInfo, carNumberMessage);
			testDeclaration.JE_TransportModeInland = TransportTypeList.Codes.Rail;
			testDeclaration.Validation.ValidateJE_VesselInland();
			AssertHasMessageErrorContaining(testDeclaration.JE_VesselInlandInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(testDeclaration.JE_VesselInlandInfo, carNumberMessage);
			testDeclaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport;
			testDeclaration.Validation.ValidateJE_VesselInland();
			AssertHasMessageErrorContaining(testDeclaration.JE_VesselInlandInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(testDeclaration.JE_VesselInlandInfo, vesselInlandMessage);
			testDeclaration.JE_TransportModeInland = TransportTypeList.Codes.Sea;
			testDeclaration.Validation.ValidateJE_VesselInland();
			AssertNoMessageErrorContaining(testDeclaration.JE_VesselInlandInfo, MandatoryValidation.YouHaveNotEntered);
			testDeclaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
			testDeclaration.JE_TransitMode = "A";
			testDeclaration.Validation.ValidateJE_VesselInland();
			AssertNoMessageErrorContaining(testDeclaration.JE_VesselInlandInfo, MandatoryValidation.YouHaveNotEntered);
			testDeclaration.JE_TransitMode = "T";
			testDeclaration.JE_TransportMode = "AIR";
			testDeclaration.Validation.ValidateJE_VesselInland();
			AssertNoMessageErrorContaining(testDeclaration.JE_VesselInlandInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_VoyageInland()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_TransitMode = "T";
			testDeclaration.JE_TransportMode = "SEA";
			testDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			testDeclaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport;
			testDeclaration.JE_VoyageInland = "";
			AssertHasMessageErrorContaining(testDeclaration.JE_VoyageInlandInfo, MandatoryValidation.YouHaveNotEntered);
			testDeclaration.JE_VoyageInland = "Voyage";
			AssertNoMessageErrorContaining(testDeclaration.JE_VoyageInlandInfo, MandatoryValidation.YouHaveNotEntered);
			testDeclaration.JE_VoyageInland = "";
			testDeclaration.JE_TransitMode = "A";
			testDeclaration.Validation.ValidateJE_VoyageInland();
			AssertNoMessageErrorContaining(testDeclaration.JE_VoyageInlandInfo, MandatoryValidation.YouHaveNotEntered);
			testDeclaration.JE_TransitMode = "T";
			testDeclaration.JE_TransportMode = "AIR";
			testDeclaration.Validation.ValidateJE_VoyageInland();
			AssertNoMessageErrorContaining(testDeclaration.JE_VoyageInlandInfo, MandatoryValidation.YouHaveNotEntered);
			testDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			testDeclaration.Validation.ValidateJE_VoyageInland();
			AssertNoMessageErrorContaining(testDeclaration.JE_VoyageInlandInfo, MandatoryValidation.YouHaveNotEntered);
			testDeclaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport;
			testDeclaration.Validation.ValidateJE_VoyageInland();
			AssertNoMessageErrorContaining(testDeclaration.JE_VoyageInlandInfo, MandatoryValidation.YouHaveNotEntered);
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		new JobDeclaration declaration;

		void AssertErrorsAfterChangingJE_MessageType(bool setUserIsController, bool messageErrorOverride, Action<JobDeclaration> setupMessageTypeNotification, bool shouldBeError)
		{
			const string expectedErrorText = "You may not change the shipment type because messages have been sent or Declaration Unified Number has been set.";
			var declaration = Factory.New<JobDeclaration>();
			using (Env.CurrentUser.SetIsControllerOverrideForTesting(setUserIsController))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.BlueErrorMessageTypeAfterMessaging, declaration.GetDefaultDataGroupingCode(), ZDateTime.Today, messageErrorOverride))
			{
				CombineAssertions(() =>
				{
					declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
					AssertEquals("No expected Shipment Type notifications", true, declaration.JE_MessageTypeInfo.Notifications == null || !declaration.JE_MessageTypeInfo.Notifications.ContainsNotificationContaining(expectedErrorText));
					setupMessageTypeNotification.Invoke(declaration);
					Factory.Save();
					declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
					if (shouldBeError)
					{
						AssertHasError("Error for MessageType change", declaration.JE_MessageTypeInfo, expectedErrorText);
					}
					else
					{
						AssertHasMessageError("Message Error for MessageType change", declaration.JE_MessageTypeInfo, expectedErrorText);
					}
				});
			}
		}
	}
}
