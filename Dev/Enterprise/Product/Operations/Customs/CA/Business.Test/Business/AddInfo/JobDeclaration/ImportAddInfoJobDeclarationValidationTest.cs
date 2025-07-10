using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ImportAddInfoJobDeclarationValidationTest : AddInfoJobDeclarationValidationTest
	{
		public void TestCheckCA_SubLocationName()
		{
			declaration.CA_SubLocationName = "123ABC";
			AssertNoWarning(declaration.CA_SubLocationNameInfo, "The Sub-Location has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");

			declaration.CA_SubLocationName = "123ABC– ";
			AssertHasWarning(declaration.CA_SubLocationNameInfo, "The Sub-Location has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
		}

		#region 
		public void TestCheckCA_MergeBy()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.CA_MergeByInfo, "XXX", B3MergeByList.Codes.ClassificationTariff);
		}
		#endregion

		#region TestCheckCA_BondType

		public void TestCheckCA_BondType()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var bondCollection = new CusBondDetailCollection(org);
			var bondData1 = bondCollection.AddNew();
			bondData1.PW_BondType = BondTypeList.Codes.NotOnPortal;
			bondData1.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-10);
			bondData1.PW_BondNumber = "00001";
			bondData1.PW_SuretyCode = "001";
			var bondData2 = bondCollection.AddNew();
			bondData2.PW_BondType = BondTypeList.Codes.OnPortal;
			bondData2.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-10);
			bondData2.PW_BondNumber = "00002";
			bondData2.PW_SuretyCode = "002";

			var bondNotOnPortal = ImportAddInfoJobDeclarationValidation.ImporterIsNotOnCARMPortal;
			var bondWillBRequired = ImportAddInfoJobDeclarationValidation.ClinetIsInCARMPortalButNoBondOnFile;

			AssertEquals(ZString.Empty, declaration.CA_BondType);
			declaration.AddInfoValidation.ValidateCA_BondType();
			AssertHasMessageError(declaration.CA_BondTypeInfo, bondNotOnPortal);
			AssertNoWarning(declaration.CA_BondTypeInfo, bondWillBRequired);

			declaration.JE_OH_Importer = org.PK;
			declaration.AddInfoValidation.ValidateCA_BondType();
			AssertHasMessageError(declaration.CA_BondTypeInfo, bondNotOnPortal);
			AssertNoWarning(declaration.CA_BondTypeInfo, bondWillBRequired);

			declaration.CA_BondType = BondTypeList.Codes.OnPortal;
			declaration.AddInfoValidation.ValidateCA_BondType();
			AssertNoMessageError(declaration.CA_BondTypeInfo, bondNotOnPortal);
			AssertHasWarning(declaration.CA_BondTypeInfo, bondWillBRequired);

			declaration.JE_OH_Importer = ZGuid.Empty;
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var bondCollection2 = new CusBondDetailCollection(org2);
			var bondData3 = bondCollection2.AddNew();
			bondData3.PW_BondType = BondTypeList.Codes.SingleTransactionBond;
			bondData3.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-10);
			bondData3.PW_BondNumber = "00003";
			bondData3.PW_SuretyCode = "003";

			declaration.JE_OH_Importer = org2.PK;
			declaration.AddInfoValidation.ValidateCA_BondType();
			AssertNoMessageError(declaration.CA_BondTypeInfo, bondNotOnPortal);
			AssertNoWarning(declaration.CA_BondTypeInfo, bondWillBRequired);
		}

		#endregion

		#region TestCheckCA_AnySightDepositAmount

		public void TestCheckCA_AnySightDepositAmount()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
			declaration.AddInfoValidation.ValidateCA_AnySightDepositAmount();
			AssertNoMessageErrorContaining(declaration.CA_AnySightDepositAmountInfo, "Deposit amounts are mandatory on Sight Type D Jobs.");
			AssertNoMessageErrorContaining(declaration.CA_AnySightDepositAmountInfo, "Deposit amounts are mandatory on Confirming Sight Type AD Jobs.");

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.CashD;
			declaration.AddInfoValidation.ValidateCA_AnySightDepositAmount();
			AssertHasMessageErrorContaining(declaration.CA_AnySightDepositAmountInfo, "Deposit amounts are mandatory on Sight Type D Jobs.");

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ConfirmingSight;
			declaration.AddInfoValidation.ValidateCA_AnySightDepositAmount();
			AssertHasMessageErrorContaining(declaration.CA_AnySightDepositAmountInfo, "Deposit amounts are mandatory on Confirming Sight Type AD Jobs.");

			declaration.CA_AnySightDepositAmount = 42.0m;

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.CashD;
			declaration.AddInfoValidation.ValidateCA_AnySightDepositAmount();
			AssertNoMessageErrorContaining(declaration.CA_AnySightDepositAmountInfo, "Deposit amounts are mandatory on Sight Type D Jobs.");

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ConfirmingSight;
			declaration.AddInfoValidation.ValidateCA_AnySightDepositAmount();
			AssertNoMessageErrorContaining(declaration.CA_AnySightDepositAmountInfo, "Deposit amounts are mandatory on Confirming Sight Type AD Jobs.");
		}

		#endregion

		#region TestCheckCA_PriorityInd

		public void TestCheckCA_PriorityInd()
		{
			ValidationTestHelper.AssertInvalidCodeMessageErrorForValidationType(declaration.CA_PriorityIndInfo, "5", PriorityIndicators.Codes.DriverWaitingOrRUSH, ValidateForMessageType.ACROSS, declaration);
		}

		#endregion

		#region TestCA_UnladingOffice

		public void TestCA_UnladingOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "X234", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			ValidationTestHelper.SetUpHasUSPlaceOfExportInvoice(declaration);
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.AddInfoValidation.ValidateCA_UnladingOffice();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageErrorForValidationType(declaration.CA_UnladingOfficeInfo, ValidateForMessageType.B3CUSDEC, declaration);

			declaration.CA_UnladingOffice = ZString.Empty;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.AddInfoValidation.ValidateCA_UnladingOffice();
			AssertNoMessageErrors(declaration.CA_UnladingOfficeInfo);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.AddInfoValidation.ValidateCA_UnladingOffice();
			AssertNoMessageErrors(declaration.CA_UnladingOfficeInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			declaration.AddInfoValidation.ValidateCA_UnladingOffice();
			AssertNoMessageErrors(declaration.CA_UnladingOfficeInfo);

			declaration.SuppressShipmentRelatedFields = false;
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.CA_UnladingOfficeInfo, "BLA", "X234");
		}

		#endregion

		#region CheckCA_ServiceOption

		public void TestCheckCA_ServiceOption()
		{
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, "CFIA", "3824600001");
			ValidationTestHelper.AssertInvalidCodeMessageErrorForValidationType(declaration.CA_ServiceOptionInfo, "12", "125", ValidateForMessageType.ACROSS, declaration);
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			declaration.CA_AssesmentOption = AssessmentOptions.Codes.AQtoFollow;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.ReplaceRMDwithAQ;
			AssertHasMessageError(declaration.CA_ServiceOptionInfo, InvalidCombinationOFServiceOptionAndAssessmentOption);
			declaration.CA_ServiceOption = "";
			AssertHasMessageError(declaration.CA_ServiceOptionInfo, NotAllOptionsSpecified);
			ValidationTestHelper.ReSetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			declaration.CA_AssesmentOption = AssessmentOptions.Codes.AQtoFollow;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.ReplaceRMDwithAQ;
			AssertNoMessageError(declaration.CA_ServiceOptionInfo, InvalidCombinationOFServiceOptionAndAssessmentOption);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			AssertEquals(false, declaration.ClassificationsHasPGARequirements);
			invoiceLine1.JI_Tariff = "3824600001";
			AssertEquals(true, declaration.ClassificationsHasPGARequirements);
			declaration.AddInfoValidation.ValidateCA_ServiceOption();
			AssertHasMessageError(declaration.CA_ServiceOptionInfo, ImportAddInfoJobDeclarationValidation.ClassificationsRequirePGAs);
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			AssertNoMessageError(declaration.CA_ServiceOptionInfo, ImportAddInfoJobDeclarationValidation.ClassificationsRequirePGAs);
		}

		const string InvalidCombinationOFServiceOptionAndAssessmentOption = "Invalid combination of Service Option and Assessment Option.";
		const string NotAllOptionsSpecified = "Both a valid Service Option and Assessment Option should be specified.";

		#endregion

		#region CheckCA_AssesmentOption

		public void TestCheckCA_AssesmentOption()
		{
			ValidationTestHelper.AssertInvalidCodeMessageErrorForValidationType(declaration.CA_AssesmentOptionInfo, "3", "2", ValidateForMessageType.ACROSS, declaration);
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.ReplaceRMDwithAQ;
			declaration.CA_AssesmentOption = AssessmentOptions.Codes.AQtoFollow;
			AssertHasMessageError(declaration.CA_ServiceOptionInfo, InvalidCombinationOFServiceOptionAndAssessmentOption);
			declaration.CA_AssesmentOption = AssessmentOptions.Codes.AppraisalQualityData;
			AssertNoMessageError(declaration.CA_ServiceOptionInfo, InvalidCombinationOFServiceOptionAndAssessmentOption);
			ValidationTestHelper.ReSetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.ReplaceRMDwithAQ;
			declaration.CA_AssesmentOption = AssessmentOptions.Codes.AQtoFollow;
			AssertNoMessageError(declaration.CA_ServiceOptionInfo, InvalidCombinationOFServiceOptionAndAssessmentOption);
			declaration.CA_AssesmentOption = AssessmentOptions.Codes.AppraisalQualityData;
			AssertNoMessageError(declaration.CA_ServiceOptionInfo, InvalidCombinationOFServiceOptionAndAssessmentOption);
		}

		#endregion

		#region TestCheckCA_NetWeight

		public void TestCheckCA_NetWeight()
		{
			declaration.CA_NetWeight = -1m;
			AssertHasError(declaration.CA_NetWeightInfo, "Please enter a 'Net Weight' greater than or equal to 0.");
			declaration.CA_NetWeight = 1m;
			AssertNoError(declaration.CA_NetWeightInfo, "Please enter a 'Net Weight' greater than or equal to 0.");
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.CA_NetWeightUQInfo, "??", "KG");
			declaration.CA_NetWeightUQ = ZString.Empty;
			AssertHasMessageError(declaration.CA_NetWeightUQInfo, "Net weight unit must be entered.");
			declaration.CA_NetWeightUQ = "T";
			AssertNoMessageError(declaration.CA_NetWeightUQInfo, "Net weight unit must be entered.");
			declaration.CA_NetWeight = 0m;
			declaration.CA_NetWeightUQ = ZString.Empty;
			AssertNoMessageError(declaration.CA_NetWeightUQInfo, "Net weight unit must be entered.");
		}

		#endregion

		#region Test Check OGD Indicators

		public void TestCheckOGDIndicators()
		{
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			AssertOGDIndicators(ServiceOptions.Codes.PARS, true);
			AssertOGDIndicators(ServiceOptions.Codes.PARSOGD, false);
			AssertOGDIndicators(ServiceOptions.Codes.RMDOGD, false);
			AssertOGDIndicators(ServiceOptions.Codes.ReplaceRMDwithAQ, true);
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			AssertOGDIndicators(ServiceOptions.Codes.PARS, false);
			AssertOGDIndicators(ServiceOptions.Codes.PARSOGD, false);
			AssertOGDIndicators(ServiceOptions.Codes.RMDOGD, false);
			AssertOGDIndicators(ServiceOptions.Codes.ReplaceRMDwithAQ, false);
		}

		void AssertOGDIndicators(string serviceOption, bool expectNotification)
		{
			declaration.CA_ServiceOption = serviceOption;
			declaration.CA_OGDCFIA = true;
			declaration.CA_OGDIC = true;
			declaration.CA_OGDNR = true;
			declaration.CA_OGDTC = true;
			if (expectNotification)
			{
				AssertHasMessageError(declaration.CA_OGDCFIAInfo, ImportAddInfoJobDeclarationValidation.notAnOGDSO);
				AssertHasMessageError(declaration.CA_OGDICInfo, ImportAddInfoJobDeclarationValidation.notAnOGDSO);
				AssertHasMessageError(declaration.CA_OGDNRInfo, ImportAddInfoJobDeclarationValidation.notAnOGDSO);
				AssertHasMessageError(declaration.CA_OGDTCInfo, ImportAddInfoJobDeclarationValidation.notAnOGDSO);
			}
			else
			{
				AssertNoMessageError(declaration.CA_OGDCFIAInfo, ImportAddInfoJobDeclarationValidation.notAnOGDSO);
				AssertNoMessageError(declaration.CA_OGDICInfo, ImportAddInfoJobDeclarationValidation.notAnOGDSO);
				AssertNoMessageError(declaration.CA_OGDNRInfo, ImportAddInfoJobDeclarationValidation.notAnOGDSO);
				AssertNoMessageError(declaration.CA_OGDTCInfo, ImportAddInfoJobDeclarationValidation.notAnOGDSO);
			}
			declaration.CA_OGDCFIA = false;
			declaration.CA_OGDIC = false;
			declaration.CA_OGDNR = false;
			declaration.CA_OGDTC = false;
			AssertNoMessageError(declaration.CA_OGDCFIAInfo, ImportAddInfoJobDeclarationValidation.notAnOGDSO);
			AssertNoMessageError(declaration.CA_OGDICInfo, ImportAddInfoJobDeclarationValidation.notAnOGDSO);
			AssertNoMessageError(declaration.CA_OGDNRInfo, ImportAddInfoJobDeclarationValidation.notAnOGDSO);
			AssertNoMessageError(declaration.CA_OGDTCInfo, ImportAddInfoJobDeclarationValidation.notAnOGDSO);
		}

		#endregion

		#region TestCheckCA_ATDExCode

		public void TestCheckCA_ATDExCode()
		{
			declaration.CA_ATDExCode = ZString.Empty;
			declaration.AddInfoValidation.ValidateCA_ATDExCode();
			AssertNoErrors(declaration.CA_ATDExCodeInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(declaration.CA_ATDExCodeInfo, "??", ATDExemptionCodes.Codes.A1);
		}

		#endregion

		#region TestCheckCA_AmendReasonCode

		public void TestCheckCA_AmendReasonCode()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.CA_AmendReasonCodeInfo, "??", EManifestAmendmentReasonCodes.Codes.ClientOutage);
		}

		#endregion

		#region TestCheckCA_ProvinceOfClearance

		public void TestCheckCA_ProvinceOfClearance()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.CA_ProvinceOfClearanceInfo, "XX", CanadianProvinceList.Codes.Alberta);
		}

		#endregion

		#region CheckCA_ExamLocationCode
		public void TestCheckCA_ExamLocationCode()
		{
			var examLocation1 = CACSubLocationTest.CreateSubLocation(Factory, "XXX3", port: "00X1");
			var examLocation2 = CACSubLocationTest.CreateSubLocation(Factory, "XXX4", port: "00X2");
			var examLocation3 = CACSubLocationTest.CreateSubLocation(Factory, "4570", port: "0497");
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.CA_ExamLocationCodeInfo, "AAAA", "4570");
			declaration.CA_ExamLocationCode = ZString.Empty;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			declaration.AddInfoValidation.ValidateCA_ExamLocationCode();
			AssertNoMessageError(declaration.CA_ExamLocationCodeInfo, ImportAddInfoJobDeclarationValidation.ExamLocationMustBeEntered);
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.AddInfoValidation.ValidateCA_ExamLocationCode();
			AssertHasMessageError(declaration.CA_ExamLocationCodeInfo, ImportAddInfoJobDeclarationValidation.ExamLocationMustBeEntered);
			declaration.CA_ExamLocationCode = "XX";
			declaration.AddInfoValidation.ValidateCA_ExamLocationCode();
			AssertNoMessageError(declaration.CA_ExamLocationCodeInfo, ImportAddInfoJobDeclarationValidation.ExamLocationMustBeEntered);

			declaration.JE_CustomsOffice = "0X2";
			declaration.CA_ExamLocationCode = "XXX3";
			AssertHasMessageErrorContaining(declaration.CA_ExamLocationCodeInfo, "The Customs Port of Clearance is not valid for this exam location. Either the exam location code is incorrect or the Port of Clearance should be X1");
			declaration.CA_ExamLocationCode = "XXX4";
			AssertNoMessageErrorContaining(declaration.CA_ExamLocationCodeInfo, "The Customs Port of Clearance is not valid for this exam location. Either the exam location code is incorrect or the Port of Clearance should be X1");
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.CA_ExamLocationCode = "XXX3";
			AssertNoMessageErrorContaining(declaration.CA_ExamLocationCodeInfo, "The Customs Port of Clearance is not valid for this exam location. Either the exam location code is incorrect or the Port of Clearance should be X1");
		}
		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		}

		#endregion
	}
}
