using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	abstract class CusTempStorageJobHeaderValidationAbstractTest<T> : BusinessObjectValidationTestCase
		where T : CusTempStorageJobHeader
	{
		public void TestCheckSJH_ReferenceNumber_Mandatory() => CombineAssertions(() =>
		{
			const string warningWithJobNumber =
				"If Customer Reference is empty, Job Number (TS000002) will be determined as Local Reference Number and sent to Customs.";

			const string warningWithoutJobNumber =
				"If Customer Reference is empty, Job Number will be determined as Local Reference Number and sent to Customs.";

			var header = GetCusTempStorageJobHeaderToTest();
			var propertyInfo = header.SJH_ReferenceNumberInfo;
			header.SJH_JobReference = "";

			header.SJH_ReferenceNumber = "TS000001";
			AssertNoWarning(propertyInfo, warningWithoutJobNumber);
			header.SJH_ReferenceNumber = "";
			AssertHasWarning(propertyInfo, warningWithoutJobNumber);

			header.SJH_JobReference = "TS000002";

			AssertHasWarning(propertyInfo, warningWithJobNumber);
			header.SJH_ReferenceNumber = "TS000001";
			AssertNoWarning(propertyInfo, warningWithJobNumber);
		});

		public void TestCheckSJH_TransportMode_Mandatory()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			CombineAssertions(() =>
			{
				header.SJH_PreviousReferenceType = PreviousReferenceType.Codes._ESUMA;
				header.Validation.ValidateSJH_TransportMode();
				AssertHasMessageErrorContaining("ESUMA Empty", header.SJH_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);

				header.SJH_PreviousReferenceType = PreviousReferenceType.Codes._ENST2L;
				header.Validation.ValidateSJH_TransportMode();
				AssertHasMessageErrorContaining("ENST2L Empty", header.SJH_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);

				header.SJH_TransportMode = "BLA";
				AssertNoMessageErrorContaining("Entered", header.SJH_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckSJH_TransportMode_Lastkraftwagen()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Truck;
			ValidationTestHelper.AssertInvalidCodeMessageError(header.SJH_TransportModeInfo, TransportTypeList.Codes.Air, TransportTypeList.Codes.Road);
		}

		public void TestCheckSJH_TransportMode_Schiff()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Vessel;
			ValidationTestHelper.AssertInvalidCodeMessageError(header.SJH_TransportModeInfo, TransportTypeList.Codes.Air, TransportTypeList.Codes.Sea);
		}

		public void TestCheckSJH_TransportMode_Waggon()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Wagon;
			ValidationTestHelper.AssertInvalidCodeMessageError(header.SJH_TransportModeInfo, TransportTypeList.Codes.Air, TransportTypeList.Codes.Rail);
		}

		public void TestCheckSJH_TransportMode_Flugzeug()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Aircraft;
			ValidationTestHelper.AssertInvalidCodeMessageError(header.SJH_TransportModeInfo, TransportTypeList.Codes.Mail, TransportTypeList.Codes.Air);
		}

		public void TestCheckSJH_TransportMode_Pkw()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Car;
			ValidationTestHelper.AssertInvalidCodeMessageError(header.SJH_TransportModeInfo, TransportTypeList.Codes.Air, TransportTypeList.Codes.Road);
		}

		public void TestCheckSJH_TransportMode_Ohne()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Without;
			ValidationTestHelper.AssertInvalidCodeMessageError(header.SJH_TransportModeInfo, TransportTypeList.Codes.Air, TransportTypeList.Codes.FixedTransportInstallations);
		}

		public void TestCheckSJH_TransportMode_Andere()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Other;
			ValidationTestHelper.AssertInvalidCodeMessageError(header.SJH_TransportModeInfo, TransportTypeList.Codes.Air, TransportTypeList.Codes.OwnPropulsion);
		}

		public void TestCheckSJH_TransportMeansCode_List()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			ValidationTestHelper.AssertInvalidCodeMessageError(header.SJH_TransportMeansCodeInfo, "BLA", TemporaryStorageTransportMeansList.Codes.Other);
		}

		public void TestCheckSJH_TransportRegNo()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "FAIRSTAR";
			vessel.RV_LloydsNumber = "1";
			vessel.RV_RadioCallSign = "1";
			vessel.RV_VesselType = "CV";
			vessel.RV_RN_NKCountryOfReg = "DE";

			//Transport Means 01
			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Truck;
			header.SJH_TransportRegNo = ZString.Empty;
			header.Validation.ValidateSJH_TransportRegNo();
			AssertHasMessageError(header.SJH_TransportRegNoInfo, MandatoryValidation.YouHaveNotEnteredMessage("Transport Registration Number"));
			header.SJH_TransportRegNo = "BJW61Z";
			AssertNoMessageError(header.SJH_TransportRegNoInfo, MandatoryValidation.YouHaveNotEnteredMessage("Transport Registration Number"));

			//Transport Means 02
			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Vessel;
			header.SJH_TransportRegNo = ZString.Empty;
			AssertHasMessageError(header.SJH_TransportRegNoInfo, MandatoryValidation.YouHaveNotEnteredMessage("Vessel"));
			header.SJH_TransportRegNo = "FAIRSTAR";
			AssertNoMessageError(header.SJH_TransportRegNoInfo, MandatoryValidation.YouHaveNotEnteredMessage("Vessel"));

			//Transport Means 03
			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Wagon;
			header.SJH_TransportRegNo = ZString.Empty;
			AssertHasMessageError(header.SJH_TransportRegNoInfo, MandatoryValidation.YouHaveNotEnteredMessage("Transport Registration Number"));
			header.SJH_TransportRegNo = "CHOO CHOO";
			AssertNoMessageError(header.SJH_TransportRegNoInfo, MandatoryValidation.YouHaveNotEnteredMessage("Transport Registration Number"));

			//Transport Means 04
			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Aircraft;
			header.SJH_TransportRegNo = ZString.Empty;
			AssertHasMessageError(header.SJH_TransportRegNoInfo, MandatoryValidation.YouHaveNotEnteredMessage("Flight Number"));
			header.SJH_TransportRegNo = "QF81";
			AssertNoMessageError(header.SJH_TransportRegNoInfo, MandatoryValidation.YouHaveNotEnteredMessage("Flight Number"));

			//Transport Means 05
			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Car;
			header.SJH_TransportRegNo = ZString.Empty;
			AssertHasMessageError(header.SJH_TransportRegNoInfo, MandatoryValidation.YouHaveNotEnteredMessage("Transport Registration Number"));
			header.SJH_TransportRegNo = "TV668";
			AssertNoMessageError(header.SJH_TransportRegNoInfo, MandatoryValidation.YouHaveNotEnteredMessage("Transport Registration Number"));

			//Allow any characters
			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Without;
			header.SJH_TransportRegNo = "CODE!123";
			AssertNoMessageErrors(header.SJH_TransportRegNoInfo);
		}

		public void TestFlightNumber()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Aircraft;
			header.SJH_TransportRegNo = "SAW1234";
			AssertHasWarningContaining(header.SJH_TransportRegNoInfo, "Flight numbers have a specific set of rules which are followed by all airlines.");
			header.SJH_TransportRegNo = "QF98";
			AssertNoWarningContaining(header.SJH_TransportRegNoInfo, "Flight numbers have a specific set of rules which are followed by all airlines.");
			header.SJH_TransportRegNo = "QF98Z";
			AssertNoWarningContaining(header.SJH_TransportRegNoInfo, "Flight numbers have a specific set of rules which are followed by all airlines.");
			header.SJH_TransportRegNo = "QF98123";
			AssertHasWarningContaining(header.SJH_TransportRegNoInfo, "Flight numbers have a specific set of rules which are followed by all airlines.");
			header.SJH_TransportRegNo = "QF6969S";
			AssertNoWarningContaining(header.SJH_TransportRegNoInfo, "Flight numbers have a specific set of rules which are followed by all airlines.");
		}

		public void TestCheckSJH_RL_NKLoading()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			var humandReadableName = header.SJH_RL_NKLoadingInfo.GetHumanReadableName();
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "!ZZ";

			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Aircraft;
			header.SJH_RL_NKLoading = ZString.Empty;
			AssertHasMessageError(header.SJH_RL_NKLoadingInfo, MandatoryValidation.YouHaveNotEnteredMessage(humandReadableName));
			header.SJH_RL_NKLoading = "!ZZ";
			AssertNoMessageError(header.SJH_RL_NKLoadingInfo, MandatoryValidation.YouHaveNotEnteredMessage(humandReadableName));
			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Other;
			header.SJH_RL_NKLoading = ZString.Empty;
			AssertNoMessageError(header.SJH_RL_NKLoadingInfo, MandatoryValidation.YouHaveNotEnteredMessage(humandReadableName));

			header.SJH_RL_NKLoading = "ZZ";
			AssertHasMessageErrorContaining(header.SJH_RL_NKLoadingInfo, ListValidation.InvalidCodeMessageError);
			header.SJH_RL_NKLoading = "!ZZ";
			AssertNoMessageErrorContaining(header.SJH_RL_NKLoadingInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckSJH_CustomsOffice()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			var humanReadableName = header.SJH_CustomsOfficeInfo.HumanReadableName;
			SetupCustomsOffices();
			header.Validation.ValidateSJH_CustomsOffice();
			AssertHasMessageError(header.SJH_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEnteredMessage(humanReadableName));
			header.SJH_CustomsOffice = "GB1";
			AssertHasMessageError(header.SJH_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);
			header.SJH_CustomsOffice = "DE";
			AssertHasMessageError(header.SJH_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);
			header.SJH_CustomsOffice = "DE1";
			AssertNoMessageErrors(header.SJH_CustomsOfficeInfo);
		}

		public void TestCheckSJH_OA_Presenter()
		{
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			const string missingEORINumberAndBranchMessage = "Address, Country/Region, City and Post Code must be entered as Presenter is missing EORI number and branch.";
			const string missingEORIBranchMessage = "Address, Country/Region, City and Post Code must be entered as Presenter is missing EORI branch.";
			const string missingEORINumberMessage = "Address, Country/Region, City and Post Code must be entered as Presenter is missing EORI number.";
			var header = GetCusTempStorageJobHeaderToTest();
			var presenter = Factory.NewWithValidTestData<OrgHeader>();
			presenter.OH_Code = "SUMATST";
			var presenterAddress = presenter.Addresses.AddNew();

			var representative = Factory.NewWithValidTestData<OrgHeader>();
			representative.OH_Code = "REPTST";
			var representativeAddress = representative.Addresses.AddNew();
			header.SJH_OA_Representative = representativeAddress.PK;

			header.SJH_OA_Presenter = ZGuid.Empty;
			AssertNoErrors(header.SJH_OA_PresenterInfo);
			AssertHasMessageError(header.SJH_OA_PresenterInfo, MandatoryValidation.YouHaveNotEnteredMessage(header.SJH_OA_PresenterInfo.GetHumanReadableName()));

			header.SJH_OA_Presenter = presenterAddress.PK;
			AssertHasMessageError(header.SJH_OA_PresenterInfo, missingEORINumberAndBranchMessage);

			AddEoriNumber(presenter);
			header.Validation.ValidateSJH_OA_Presenter();
			AssertHasMessageError(header.SJH_OA_PresenterInfo, missingEORIBranchMessage);

			presenter.CustomsCodes.RemoveAndDeleteAll();
			var cusCode = presenterAddress.CustomsCodes.AddNew();
			cusCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			cusCode.OK_CustomsRegNo = "TESTBRANCHADDR";
			header.Validation.ValidateSJH_OA_Presenter();
			AssertHasMessageError(header.SJH_OA_PresenterInfo, missingEORINumberMessage);

			AddEoriNumber(presenter);
			header.Validation.ValidateSJH_OA_Presenter();
			AssertNoMessageErrors(header.SJH_OA_PresenterInfo);

			presenterAddress.CustomsCodes.DeleteAll();
			header.Validation.ValidateSJH_OA_Presenter();
			AssertHasMessageError(header.SJH_OA_PresenterInfo, missingEORIBranchMessage);

			var newpresenterAddress = presenter.Addresses.AddNew();
			var newAddressBranchCusCode = newpresenterAddress.CustomsCodes.AddNew();
			newAddressBranchCusCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix;
			newAddressBranchCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			newAddressBranchCusCode.OK_CustomsRegNo = "NOTPICKEDUPINFALLBACK";

			header.Validation.ValidateSJH_OA_Presenter();
			AssertHasMessageError(header.SJH_OA_PresenterInfo, missingEORIBranchMessage);

			var branchCode = presenter.CustomsCodes.AddNew();
			branchCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix;
			branchCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			branchCode.OK_CustomsRegNo = "TESTBRANCHHEADER";
			branchCode.OK_OA_PremisesAddress = presenterAddress.PK;

			header.Validation.ValidateSJH_OA_Presenter();
			AssertNoMessageErrors(header.SJH_OA_PresenterInfo);

			presenter.CustomsCodes.RemoveAndDeleteAll();
			presenterAddress.CustomsCodes.DeleteAll();
			header.Validation.ValidateSJH_OA_Presenter();
			AssertHasMessageError(header.SJH_OA_PresenterInfo, missingEORINumberAndBranchMessage);

			presenterAddress.OA_Address1 = "TEST PHYSICAL LOCATION";
			presenterAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			presenterAddress.OA_PostCode = "02627";
			presenterAddress.OA_City = "BERLIN";
			header.Validation.ValidateSJH_OA_Presenter();
			AssertNoMessageErrors(header.SJH_OA_PresenterInfo);
		}

		public void TestPresenterAtlasValidation()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			var presenter = Factory.NewWithValidTestData<OrgHeader>();
			presenter.OH_Code = "SUMATST";
			var presenterAddress = presenter.Addresses.AddNew();

			var representative = Factory.NewWithValidTestData<OrgHeader>();
			representative.OH_Code = "REPTST";
			var representativeAddress = representative.Addresses.AddNew();

			header.SJH_OA_Presenter = presenterAddress.PK;
			header.SJH_OA_Representative = representativeAddress.PK;
			header.Validation.ValidateSJH_OA_Presenter();
			AssertNoMessageError(header.SJH_OA_PresenterInfo, "Presenter must have an ATLAS Participant Identification Number when Representative is empty.");
			header.SJH_OA_Representative = ZGuid.Empty;
			header.Validation.ValidateSJH_OA_Presenter();
			AssertHasMessageError(header.SJH_OA_PresenterInfo, "Presenter must have an ATLAS Participant Identification Number when Representative is empty.");
			var atlasCode = presenter.CustomsCodes.AddNew();
			atlasCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			atlasCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber;
			atlasCode.OK_CustomsRegNo = "08970987987";
			header.Validation.ValidateSJH_OA_Presenter();
			AssertNoMessageError(header.SJH_OA_PresenterInfo, "Presenter must have an ATLAS Participant Identification Number when Representative is empty.");
		}

		public void TestCheckSJH_OA_PresenterShouldBeOrgProxy()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			var presenter = Factory.NewWithValidTestData<OrgHeader>();
			presenter.OH_Code = "SUMATST";
			var presenterAddress = presenter.Addresses.AddNew();

			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.OH_Code = "OrgProxy";
			var orgProxyAddress = orgProxy.Addresses.AddNew();

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = orgProxy.PK;

			var representative = Factory.NewWithValidTestData<OrgHeader>();
			representative.OH_Code = "REPTST";
			var representativeAddress = representative.Addresses.AddNew();
			CombineAssertions(() =>
			{
				header.SJH_OA_Presenter = presenterAddress.PK;
				header.SJH_OA_Representative = representativeAddress.PK;
				header.Validation.ValidateSJH_OA_Presenter();
				AssertNoMessageError(header.SJH_OA_PresenterInfo, "Presenter must equal your own Organization when Representative is empty.");

				header.SJH_OA_Representative = ZGuid.Empty;
				header.Validation.ValidateSJH_OA_Presenter();
				AssertHasMessageError(header.SJH_OA_PresenterInfo, "Presenter must equal your own Organization when Representative is empty.");

				header.SJH_OA_Presenter = orgProxyAddress.PK;
				header.SJH_OA_Representative = ZGuid.Empty;
				header.Validation.ValidateSJH_OA_Presenter();
				AssertNoMessageError(header.SJH_OA_PresenterInfo, "Presenter must equal your own Organization when Representative is empty.");
			});
		}

		public void TestCheckSJH_OA_Representative()
		{
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			var header = GetCusTempStorageJobHeaderToTest();
			var presenter = Factory.NewWithValidTestData<OrgHeader>();
			presenter.OH_Code = "PRSTST";
			var presenterAddress = presenter.Addresses.AddNew();
			var representative = Factory.NewWithValidTestData<OrgHeader>();
			representative.OH_Code = "REPTEST";
			var representativeAddress = representative.Addresses.AddNew();

			var atlasCode = Factory.New<OrgCusCode>();
			atlasCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			atlasCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber;
			atlasCode.OK_CustomsRegNo = "08970987987";
			representative.CustomsCodes.Add(atlasCode);

			header.SJH_OA_Presenter = presenterAddress.PK;
			header.SJH_OA_Representative = ZGuid.Empty;
			AssertNoErrors(header.SJH_OA_RepresentativeInfo);
			AssertHasMessageError(header.SJH_OA_RepresentativeInfo, "Representative is required to be entered as the Presenter is missing EORI number or branch.");

			AddEoriNumber(presenter);
			header.Validation.ValidateSJH_OA_Representative();
			AssertHasMessageError(header.SJH_OA_RepresentativeInfo, "Representative is required to be entered as the Presenter is missing EORI number or branch.");

			var eoriBranch = presenterAddress.CustomsCodes.AddNew();
			eoriBranch.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix;
			eoriBranch.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			eoriBranch.OK_CustomsRegNo = "BRANCH1234";
			header.Validation.ValidateSJH_OA_Representative();
			AssertNoMessageError(header.SJH_OA_RepresentativeInfo, "Representative is required to be entered as the Presenter is missing EORI number or branch.");

			presenterAddress.CustomsCodes.DeleteAll();
			presenter.CustomsCodes.RemoveAndDeleteAll();

			representative.CustomsCodes.Remove(atlasCode);
			header.SJH_OA_Representative = representativeAddress.PK;
			AssertHasMessageError(header.SJH_OA_RepresentativeInfo, "Representative is missing EORI number and branch.");
			AssertHasMessageError(header.SJH_OA_RepresentativeInfo, "Representative must have an ATLAS Participant Identification Number.");

			AddEoriNumber(representative);
			header.Validation.ValidateSJH_OA_Representative();
			AssertHasMessageError(header.SJH_OA_RepresentativeInfo, "Representative is missing EORI branch.");
			AssertHasMessageError(header.SJH_OA_RepresentativeInfo, "Representative must have an ATLAS Participant Identification Number.");

			var presenterEoriCode = AddEoriNumber(presenter);
			header.Validation.ValidateSJH_OA_Representative();
			AssertHasMessageError(header.SJH_OA_RepresentativeInfo, "Representative is missing EORI branch.");
			AssertHasMessageError(header.SJH_OA_RepresentativeInfo, "Representative must have an ATLAS Participant Identification Number.");
			AssertHasMessageError(header.SJH_OA_RepresentativeInfo, "Presenter and Representative may not have the same EORI number.");

			presenterEoriCode.OK_CustomsRegNo = "DIFFTEST";
			header.Validation.ValidateSJH_OA_Representative();
			AssertNoMessageError(header.SJH_OA_RepresentativeInfo, "Presenter and Representative may not have the same EORI number.");
			AssertHasMessageError(header.SJH_OA_RepresentativeInfo, "Representative is missing EORI branch.");
			AssertHasMessageError(header.SJH_OA_RepresentativeInfo, "Representative must have an ATLAS Participant Identification Number.");

			representative.CustomsCodes.Add(atlasCode);
			header.Validation.ValidateSJH_OA_Representative();
			AssertNoMessageError(header.SJH_OA_RepresentativeInfo, "Representative must have an ATLAS Participant Identification Number.");
			AssertNoMessageError(header.SJH_OA_RepresentativeInfo, "Presenter and Representative may not have the same EORI number.");
			AssertHasMessageError(header.SJH_OA_RepresentativeInfo, "Representative is missing EORI branch.");

			var branchCode = representativeAddress.CustomsCodes.AddNew();
			branchCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			branchCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix;
			branchCode.OK_CustomsRegNo = "BRANCHTEST";
			header.Validation.ValidateSJH_OA_Representative();
			AssertNoMessageError(header.SJH_OA_RepresentativeInfo, "Representative is required to be entered as the Presenter is missing EORI number or branch.");
			AssertNoMessageError(header.SJH_OA_RepresentativeInfo, "Representative must have an ATLAS Participant Identification Number.");
			AssertNoMessageError(header.SJH_OA_RepresentativeInfo, "Presenter and Representative may not have the same EORI number.");
			AssertNoMessageError(header.SJH_OA_RepresentativeInfo, "Representative is missing EORI branch.");
		}

		public void TestCheckSJH_OA_RepresentativeShouldBeOrgProxy()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			var representative = Factory.NewWithValidTestData<OrgHeader>();
			representative.OH_Code = "SUMATST";
			var representativeAddress = representative.Addresses.AddNew();

			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.OH_Code = "OrgProxy";
			var orgProxyAddress = orgProxy.Addresses.AddNew();

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = orgProxy.PK;

			CombineAssertions(() =>
			{
				header.SJH_OA_Representative = representativeAddress.PK;
				header.Validation.ValidateSJH_OA_Representative();
				AssertHasMessageError(header.SJH_OA_RepresentativeInfo, "Representative must equal your own Organization.");

				header.SJH_OA_Representative = orgProxyAddress.PK;
				header.Validation.ValidateSJH_OA_Representative();
				AssertNoMessageError(header.SJH_OA_RepresentativeInfo, "Representative must equal your own Organization.");
			});
		}

		protected abstract T GetCusTempStorageJobHeaderToTest();

		protected void SetupCustomsOffices()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: euDataGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, parent: euDataGrouping);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "MainCustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE1", "Germany", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "GB1", "United Kingdom", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}

		OrgCusCode AddEoriNumber(OrgHeader orgHeader)
		{
			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Greece;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "TEST";
			return cusCode;
		}
	}
}
