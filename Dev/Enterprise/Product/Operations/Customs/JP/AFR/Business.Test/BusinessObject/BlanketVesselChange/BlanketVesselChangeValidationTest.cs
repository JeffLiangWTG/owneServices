using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class BlanketVesselChangeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJPM_CarrierCodeNew()
		{
			var header = Factory.New<JPAFRHeader>();
			var blanketVesselChange = new BlanketVesselChange(header) { JPM_CarrierCodeNew = string.Empty };

			AssertHasMessageErrorContaining(blanketVesselChange.JPM_CarrierCodeNewInfo, MandatoryValidation.YouHaveNotEntered);

			blanketVesselChange.JPM_CarrierCodeNew = "12";
			AssertHasMessageErrorContaining(blanketVesselChange.JPM_CarrierCodeNewInfo, ValidationConstants.Header.CarrierCodeInvalid);

			blanketVesselChange.JPM_CarrierCodeNew = "12]4";
			AssertHasMessageErrorContaining(blanketVesselChange.JPM_CarrierCodeNewInfo, ValidationConstants.Shared.InvalidNACCSChar(blanketVesselChange.JPM_CarrierCodeNewInfo.HumanReadableName));
			AssertHasMessageErrorContaining(blanketVesselChange.JPM_CarrierCodeNewInfo, ValidationConstants.Header.CarrierCodeInvalid);

			AssertHasMessageErrorContaining(blanketVesselChange.JPM_CarrierCodeNewInfo, ValidationConstants.Header.CarrierCodeInvalid);

			blanketVesselChange.JPM_CarrierCodeNew = "123";
			AssertNoNotifications(blanketVesselChange.JPM_CarrierCodeNewInfo);
		}

		public void TestCheckJPM_VesselNameNew()
		{
			var header = Factory.New<JPAFRHeader>();
			var blanketVesselChange = new BlanketVesselChange(header) { JPM_VesselNameNew = string.Empty };
			blanketVesselChange.RunPreSaveValidation();

			AssertHasMessageErrorContaining(blanketVesselChange.JPM_VesselNameNewInfo, MandatoryValidation.YouHaveNotEntered);

			blanketVesselChange.JPM_VesselNameNew = "XXXX";
			AssertHasMessageErrorContaining(blanketVesselChange.JPM_VesselNameNewInfo, ListValidation.InvalidCodeMessageError);

			var testCountry = blanketVesselChange.Factory.NewWithValidTestData<RefCountry>();
			testCountry.Code = "A]";
			var testVessel = blanketVesselChange.Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_Code = "XX[X";
			testVessel.RV_RadioCallSign = "YY[Y";
			testVessel.RV_RN_NKCountryOfReg = "A]";

			blanketVesselChange.JPM_VesselNameNew = "XX[X";
			AssertHasMessageErrorContaining(blanketVesselChange.JPM_VesselNameNewInfo, ValidationConstants.Shared.InvalidNACCSChar("Vessel Name New"));
			AssertHasMessageErrorContaining(blanketVesselChange.JPM_RadioCallSignNewInfo, ValidationConstants.Shared.InvalidNACCSChar(blanketVesselChange.JPM_RadioCallSignNewInfo.HumanReadableName));
			AssertHasMessageErrorContaining(blanketVesselChange.JPM_RN_NKCountryOfRegNewInfo, ValidationConstants.Shared.InvalidNACCSChar(blanketVesselChange.JPM_RN_NKCountryOfRegNewInfo.HumanReadableName));

			testVessel = blanketVesselChange.Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_RadioCallSign = "";
			testVessel.RV_RN_NKCountryOfReg = "";
			blanketVesselChange.JPM_VesselNameNew = testVessel.RV_Code;
			AssertNoMessageErrorContaining(blanketVesselChange.JPM_VesselNameNewInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(blanketVesselChange.JPM_RadioCallSignNewInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(blanketVesselChange.JPM_RN_NKCountryOfRegNewInfo, MandatoryValidation.YouHaveNotEntered);

			testVessel.RV_RadioCallSign = "123321";
			blanketVesselChange.JPM_VesselNameNew = ZString.Empty;
			blanketVesselChange.JPM_VesselNameNew = testVessel.RV_Code;
			AssertNoMessageErrorContaining(blanketVesselChange.JPM_VesselNameNewInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(blanketVesselChange.JPM_RadioCallSignNewInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(blanketVesselChange.JPM_RN_NKCountryOfRegNewInfo, MandatoryValidation.YouHaveNotEntered);

			testVessel.RV_RadioCallSign = "ABCDEFGHIJ";
			blanketVesselChange.JPM_VesselNameNew = ZString.Empty;
			blanketVesselChange.JPM_VesselNameNew = testVessel.RV_Code;
			AssertNoMessageErrorContaining(blanketVesselChange.JPM_RadioCallSignNewInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(blanketVesselChange.JPM_RadioCallSignNewInfo, ValidationConstants.Header.VesselCallSignReachingMaxAllowed);
			AssertHasMessageErrorContaining(blanketVesselChange.JPM_RN_NKCountryOfRegNewInfo, MandatoryValidation.YouHaveNotEntered);

			testVessel.RV_RN_NKCountryOfReg = "AU";
			testVessel.RV_RadioCallSign = "123321";
			blanketVesselChange.JPM_VesselNameNew = ZString.Empty;
			blanketVesselChange.JPM_VesselNameNew = testVessel.RV_Code;
			AssertNoMessageErrors(blanketVesselChange.JPM_VesselNameNewInfo);
			AssertNoMessageErrors(blanketVesselChange.JPM_RadioCallSignNewInfo);
			AssertNoMessageErrors(blanketVesselChange.JPM_RN_NKCountryOfRegNewInfo);

			testVessel.RV_RadioCallSign = "N/A";
			blanketVesselChange.JPM_VesselNameNew = ZString.Empty;
			blanketVesselChange.JPM_VesselNameNew = testVessel.RV_Code;
			AssertNoMessageErrors(blanketVesselChange.JPM_RadioCallSignNewInfo);
			AssertHasWarning(blanketVesselChange.JPM_RadioCallSignNewInfo, ValidationConstants.Header.InappropriateVesselCode("N/A"));

			testVessel.RV_RadioCallSign = "3T";
			blanketVesselChange.JPM_VesselNameNew = ZString.Empty;
			blanketVesselChange.JPM_VesselNameNew = testVessel.RV_Code;
			AssertNoMessageErrors(blanketVesselChange.JPM_RadioCallSignNewInfo);
			AssertHasWarning(blanketVesselChange.JPM_RadioCallSignNewInfo, ValidationConstants.Header.InappropriateVesselCode("3T"));

			testVessel.RV_RadioCallSign = "TTTTT";
			blanketVesselChange.JPM_VesselNameNew = ZString.Empty;
			blanketVesselChange.JPM_VesselNameNew = testVessel.RV_Code;
			AssertNoMessageErrors(blanketVesselChange.JPM_RadioCallSignNewInfo);
			AssertHasWarning(blanketVesselChange.JPM_RadioCallSignNewInfo, ValidationConstants.Header.InappropriateVesselCode("TTTTT"));

			testVessel.RV_RadioCallSign = "123321";
			blanketVesselChange.JPM_VesselNameNew = ZString.Empty;
			blanketVesselChange.JPM_VesselNameNew = testVessel.RV_Code;
			AssertNoMessageErrors(blanketVesselChange.JPM_RadioCallSignNewInfo);
			AssertNoWarnings(blanketVesselChange.JPM_RadioCallSignNewInfo);
		}

		public void TestCheckJPM_VoyageNumberNew()
		{
			var header = Factory.New<JPAFRHeader>();
			var blanketVesselChange = new BlanketVesselChange(header) { JPM_VoyageNumberNew = string.Empty };

			AssertNoErrors(blanketVesselChange.JPM_VoyageNumberNewInfo);
			AssertHasMessageErrorContaining(blanketVesselChange.JPM_VoyageNumberNewInfo, MandatoryValidation.YouHaveNotEntered);

			blanketVesselChange.JPM_VoyageNumberNew = "00[N";
			AssertHasMessageErrorContaining(blanketVesselChange.JPM_VoyageNumberNewInfo, ValidationConstants.Shared.InvalidNACCSChar(blanketVesselChange.JPM_VoyageNumberNewInfo.HumanReadableName));
			AssertNoErrors(blanketVesselChange.JPM_VoyageNumberNewInfo);

			blanketVesselChange.JPM_VoyageNumberNew = "009N";
			AssertNoErrors(blanketVesselChange.JPM_VoyageNumberNewInfo);
			AssertNoMessageErrors(blanketVesselChange.JPM_VoyageNumberNewInfo);
		}

		public void CheckJPM_PortOfLoadingCodeNew()
		{
			var header = Factory.New<JPAFRHeader>();
			var blanketVesselChange = new BlanketVesselChange(header) { JPM_PortOfLoadingCodeNew = string.Empty };

			AssertNoErrors(blanketVesselChange.JPM_PortOfLoadingCodeNewInfo);
			AssertHasMessageErrorContaining(blanketVesselChange.JPM_PortOfLoadingCodeNewInfo, MandatoryValidation.YouHaveNotEntered);

			blanketVesselChange.JPM_PortOfLoadingCodeNew = "XXX[X";
			AssertHasMessageErrorContaining(blanketVesselChange.JPM_PortOfLoadingCodeNewInfo, ValidationConstants.Shared.InvalidNACCSChar(blanketVesselChange.JPM_PortOfLoadingCodeNewInfo.HumanReadableName));
			AssertHasMessageErrorContaining(blanketVesselChange.JPM_PortOfLoadingCodeNewInfo, ListValidation.InvalidCodeMessageError);

			blanketVesselChange.JPM_PortOfLoadingCodeNew = "XXXXX";
			AssertNoErrors(blanketVesselChange.JPM_PortOfLoadingCodeNewInfo);
			AssertHasMessageErrorContaining(blanketVesselChange.JPM_PortOfLoadingCodeNewInfo, ListValidation.InvalidCodeMessageError);

			var testUNLOCO = blanketVesselChange.Factory.NewWithValidTestData<RefUNLOCO>();
			testUNLOCO.RL_PortName = "LOCOLOCOLOCOLOCOLOCOLOCOLOCO";
			blanketVesselChange.JPM_PortOfLoadingCodeNew = testUNLOCO.Code;
			AssertNoErrors(blanketVesselChange.JPM_PortOfLoadingCodeNewInfo);
			AssertHasWarningContaining(blanketVesselChange.JPM_PortOfLoadingCodeNewInfo, ValidationConstants.Shared.PortNameLengthExceeded);

			testUNLOCO.RL_PortName = "LOCOLOCOLOCOLOCOLOCOLOCOLOCO";
			testUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			blanketVesselChange.JPM_PortOfLoadingCodeNew = testUNLOCO.Code;
			AssertNoErrors(blanketVesselChange.JPM_PortOfLoadingCodeNewInfo);
			AssertHasMessageErrorContaining(blanketVesselChange.JPM_PortOfLoadingCodeNewInfo, ValidationConstants.Header.LoadingPortCannotBeInJP);

			testUNLOCO.RL_PortName = "LOCO";
			testUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			blanketVesselChange.JPM_PortOfLoadingCodeNew = testUNLOCO.Code;
			AssertNoErrors(blanketVesselChange.JPM_PortOfLoadingCodeNewInfo);
			AssertHasMessageErrorContaining(blanketVesselChange.JPM_PortOfLoadingCodeNewInfo, ValidationConstants.Header.LoadingPortCannotBeInJP);

			testUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			blanketVesselChange.JPM_PortOfLoadingCodeNew = testUNLOCO.Code;
			AssertNoErrors(blanketVesselChange.JPM_PortOfLoadingCodeNewInfo);
			AssertNoMessageErrors(blanketVesselChange.JPM_PortOfLoadingCodeNewInfo);
			AssertNoWarnings(blanketVesselChange.JPM_PortOfLoadingCodeNewInfo);
		}

		public void CheckJPM_PortOfLoadingSuffixNew()
		{
			var header = Factory.New<JPAFRHeader>();
			var blanketVesselChange = new BlanketVesselChange(header) { JPM_PortOfLoadingSuffixNew = "A" };

			AssertHasMessageErrorContaining(blanketVesselChange.JPM_PortOfLoadingSuffixNewInfo, ValidationConstants.Header.InvalidPortSuffix);

			blanketVesselChange.JPM_PortOfLoadingSuffixNew = "1";
			AssertNoMessageErrors(blanketVesselChange.JPM_PortOfLoadingSuffixNewInfo);
		}

		public void CheckJPM_ETDNew()
		{
			var testTime = ValidationUtils.GetCurrentJPDate;
			var header = Factory.New<JPAFRHeader>();
			var blanketVesselChange = new BlanketVesselChange(header) { JPM_ETDNew = ZDateTime.Empty };

			AssertHasMessageErrorContaining(blanketVesselChange.JPM_ETDNewInfo, MandatoryValidation.YouHaveNotEntered);

			blanketVesselChange.JPM_ETDNew = testTime;
			AssertNoMessageErrors(blanketVesselChange.JPM_ETDNewInfo);
		}

		public void TestValidateNoVesselDetailsChange()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "VESSEL";
			vessel.RV_RadioCallSign = "12345";
			vessel.RV_RN_NKCountryOfReg = "CN";

			var portOfLoading = Factory.NewWithValidTestData<RefUNLOCO>();
			portOfLoading.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			portOfLoading.RL_PortName = "LN123";
			portOfLoading.Code = "L123";

			var portOfDischarge = Factory.NewWithValidTestData<RefUNLOCO>();
			portOfDischarge.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			portOfDischarge.RL_PortName = "DN123";
			portOfDischarge.Code = "D123";

			Factory.Save();

			var header = Factory.New<JPAFRHeader>();
			header.JPH_IsShippingLineEntry = ZBool.True;
			header.JPH_CarrierCode = "C123";
			header.JPH_VesselName = vessel.RV_Code;
			header.JPH_RadioCallSign = vessel.RV_RadioCallSign;
			header.JPH_Voyage = "V123";
			header.JPH_OperationalCarrierVoyageNo = "O123";
			header.JPH_RL_NKLoading = portOfLoading.Code;
			header.JPH_LoadingPortSuffix = "1";
			header.JPH_RelaxedAppId = ZBool.True;
			header.JPH_RL_NKDischarge = portOfDischarge.Code;
			header.JPH_DischargePortSuffix = "1";
			header.JPH_ETD = new ZDateTime(2017, 5, 3);
			header.JPH_ETA = new ZDateTime(2017, 5, 3);

			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			AssertEquals(2, header.Bills.Count);

			var blanketVesselChange = new BlanketVesselChange(header);
			AssertEquals(2, blanketVesselChange.BlanketVesselChangeBills.Count);

			var msgObj1 = blanketVesselChange.BlanketVesselChangeBills[0];
			var msgObj2 = blanketVesselChange.BlanketVesselChangeBills[1];

			blanketVesselChange.JPM_CarrierCodeNew = "C123";
			blanketVesselChange.JPM_VesselNameNew = vessel.RV_Code;
			AssertEquals(vessel.RV_RadioCallSign, blanketVesselChange.JPM_RadioCallSignNew);
			blanketVesselChange.JPM_VoyageNumberNew = "V123";
			blanketVesselChange.JPM_OperatorVoyageNew = "O123";
			blanketVesselChange.JPM_PortOfLoadingCodeNew = portOfLoading.Code;
			blanketVesselChange.JPM_PortOfLoadingSuffixNew = "1";
			blanketVesselChange.JPM_IsDepartureFromRelaxedAreaNew = ZBool.True;
			blanketVesselChange.JPM_ETDNew = new ZDateTime(2017, 5, 3);

			blanketVesselChange.Validation.ValidateAll();
			AssertHasMessageError(blanketVesselChange.JPM_CarrierCodeNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertHasMessageError(blanketVesselChange.JPM_VesselNameNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertHasMessageError(blanketVesselChange.JPM_OperatorVoyageNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertHasMessageError(blanketVesselChange.JPM_VoyageNumberNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertHasMessageError(blanketVesselChange.JPM_IsDepartureFromRelaxedAreaNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertHasMessageError(blanketVesselChange.JPM_PortOfLoadingCodeNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertHasMessageError(blanketVesselChange.JPM_PortOfLoadingSuffixNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertHasMessageError(blanketVesselChange.JPM_ETDNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);

			blanketVesselChange.JPM_ETDNew = new ZDateTime(2017, 5, 4);
			blanketVesselChange.Validation.ValidateAll();
			AssertNoMessageError(blanketVesselChange.JPM_CarrierCodeNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertNoMessageError(blanketVesselChange.JPM_VesselNameNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertNoMessageError(blanketVesselChange.JPM_OperatorVoyageNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertNoMessageError(blanketVesselChange.JPM_VoyageNumberNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertNoMessageError(blanketVesselChange.JPM_IsDepartureFromRelaxedAreaNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertNoMessageError(blanketVesselChange.JPM_PortOfLoadingCodeNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertNoMessageError(blanketVesselChange.JPM_PortOfLoadingSuffixNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertNoMessageError(blanketVesselChange.JPM_ETDNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);

			header.JPH_IsShippingLineEntry = ZBool.False;
			blanketVesselChange = new BlanketVesselChange(header);
			AssertEquals(2, blanketVesselChange.BlanketVesselChangeBills.Count);

			blanketVesselChange.JPM_CarrierCodeNew = "C123";
			blanketVesselChange.JPM_VesselNameNew = vessel.RV_Code;
			AssertEquals(vessel.RV_RadioCallSign, blanketVesselChange.JPM_RadioCallSignNew);
			blanketVesselChange.JPM_VoyageNumberNew = "V123";
			blanketVesselChange.JPM_OperatorVoyageNew = "O123";
			blanketVesselChange.JPM_PortOfLoadingCodeNew = portOfLoading.Code;
			blanketVesselChange.JPM_PortOfLoadingSuffixNew = "1";
			blanketVesselChange.JPM_IsDepartureFromRelaxedAreaNew = ZBool.True;
			blanketVesselChange.JPM_ETDNew = new ZDateTime(2017, 5, 3);

			blanketVesselChange.Validation.ValidateAll();
			AssertHasMessageError(blanketVesselChange.JPM_CarrierCodeNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertHasMessageError(blanketVesselChange.JPM_VesselNameNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertHasMessageError(blanketVesselChange.JPM_OperatorVoyageNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertHasMessageError(blanketVesselChange.JPM_VoyageNumberNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertHasMessageError(blanketVesselChange.JPM_IsDepartureFromRelaxedAreaNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertHasMessageError(blanketVesselChange.JPM_PortOfLoadingCodeNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertHasMessageError(blanketVesselChange.JPM_PortOfLoadingSuffixNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertHasMessageError(blanketVesselChange.JPM_ETDNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);

			blanketVesselChange.JPM_ETDNew = new ZDateTime(2017, 5, 4);
			blanketVesselChange.Validation.ValidateAll();
			AssertNoMessageError(blanketVesselChange.JPM_CarrierCodeNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertNoMessageError(blanketVesselChange.JPM_VesselNameNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertNoMessageError(blanketVesselChange.JPM_OperatorVoyageNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertNoMessageError(blanketVesselChange.JPM_VoyageNumberNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertNoMessageError(blanketVesselChange.JPM_IsDepartureFromRelaxedAreaNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertNoMessageError(blanketVesselChange.JPM_PortOfLoadingCodeNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertNoMessageError(blanketVesselChange.JPM_PortOfLoadingSuffixNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			AssertNoMessageError(blanketVesselChange.JPM_ETDNewInfo, ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
		}
	}
}
