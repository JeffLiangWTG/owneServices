using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class JPAFRHeaderValidationTest : BusinessObjectValidationTestCase
	{
		[TestDate(2017, 5, 3)]
		public void TestCheckJPH_VesselDetailsChanged()
		{
			using (JPAFRRegistry.Instance.AFR2017EffectiveLiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2017, 10, 06)))
			{
				Header.JPH_IsShippingLineEntry = false;
				Header.JPH_VesselDetailsChanged = true;
				AssertHasMessageError(Header.JPH_VesselDetailsChangedInfo, ValidationConstants.Header.VesselDetailsChangedIsNotActive);

				Header.JPH_IsShippingLineEntry = true;
				Header.JPH_VesselDetailsChanged = true;
				AssertNoMessageError(Header.JPH_VesselDetailsChangedInfo, ValidationConstants.Header.VesselDetailsChangedIsNotActive);
			}

			using (JPAFRRegistry.Instance.AFR2017EffectiveLiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2017, 1, 1)))
			{
				Header.JPH_IsShippingLineEntry = false;
				Header.JPH_VesselDetailsChanged = true;
				AssertNoMessageError(Header.JPH_VesselDetailsChangedInfo, ValidationConstants.Header.VesselDetailsChangedIsNotActive);
			}
		}

		public void TestCheckJPH_Voyage()
		{
			this.header = null;

			Header.JPH_Voyage = string.Empty;
			AssertNoErrors(Header.JPH_VoyageInfo);
			AssertHasMessageErrorContaining(Header.JPH_VoyageInfo, MandatoryValidation.YouHaveNotEntered);

			Header.JPH_Voyage = "00[N";
			AssertHasMessageErrorContaining(Header.JPH_VoyageInfo, ValidationConstants.Shared.InvalidNACCSChar(Header.JPH_VoyageInfo.HumanReadableName));
			AssertNoErrors(Header.JPH_VoyageInfo);

			Header.JPH_Voyage = "009N";
			AssertNoErrors(Header.JPH_VoyageInfo);
			AssertNoMessageErrors(Header.JPH_VoyageInfo);

			var testBill = Header.Bills.AddNew();
			Factory.Save();

			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_Voyage = ZString.Empty; }, Header.JPH_VoyageInfo, "009N", ZString.Empty);
			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_Voyage = "009M"; }, Header.JPH_VoyageInfo, "009N", "009M");
		}

		public void TestCheckJPH_VesselName()
		{
			Header.JPH_VesselName = ZString.Empty;
			Header.RunPreSaveValidation();
			AssertHasMessageErrorContaining(Header.JPH_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);

			Header.JPH_VesselName = ZString.Empty;
			Header.JPH_VesselName = "XXXX";
			AssertHasMessageErrorContaining(Header.JPH_VesselNameInfo, ListValidation.InvalidCodeMessageError);

			var testCountry = Factory.NewWithValidTestData<RefCountry>();
			testCountry.Code = "A]";
			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_Code = "XX[X";
			testVessel.RV_RadioCallSign = "YY[Y";
			testVessel.RV_RN_NKCountryOfReg = "A]";

			Header.JPH_VesselName = ZString.Empty;
			Header.JPH_VesselName = "XX[X";
			AssertHasMessageErrorContaining(Header.JPH_VesselNameInfo, ValidationConstants.Shared.InvalidNACCSChar(testVessel.RV_CodeInfo.HumanReadableName));
			AssertHasMessageErrorContaining(Header.JPH_RadioCallSignInfo, ValidationConstants.Shared.InvalidNACCSChar(header.JPH_RadioCallSignInfo.HumanReadableName));
			AssertHasMessageErrorContaining(Header.JPH_RN_NKCountryOfRegInfo, ValidationConstants.Shared.InvalidNACCSChar(header.JPH_RN_NKCountryOfRegInfo.HumanReadableName));

			testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_RadioCallSign = "";
			testVessel.RV_RN_NKCountryOfReg = "";
			Header.JPH_VesselName = ZString.Empty;
			Header.JPH_VesselName = testVessel.RV_Code;
			AssertNoMessageErrorContaining(Header.JPH_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Header.JPH_RadioCallSignInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Header.JPH_RN_NKCountryOfRegInfo, MandatoryValidation.YouHaveNotEntered);

			testVessel.RV_RadioCallSign = "123321";
			Header.JPH_VesselName = ZString.Empty;
			Header.JPH_VesselName = testVessel.RV_Code;
			AssertNoMessageErrorContaining(Header.JPH_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(Header.JPH_RadioCallSignInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Header.JPH_RN_NKCountryOfRegInfo, MandatoryValidation.YouHaveNotEntered);

			testVessel.RV_RadioCallSign = "ABCDEFGHIJ";
			Header.JPH_VesselName = ZString.Empty;
			Header.JPH_VesselName = testVessel.RV_Code;
			AssertNoMessageErrorContaining(Header.JPH_RadioCallSignInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Header.JPH_RadioCallSignInfo, ValidationConstants.Header.VesselCallSignReachingMaxAllowed);
			AssertHasMessageErrorContaining(Header.JPH_RN_NKCountryOfRegInfo, MandatoryValidation.YouHaveNotEntered);

			testVessel.RV_RN_NKCountryOfReg = "AU";
			testVessel.RV_RadioCallSign = "123321";
			Header.JPH_VesselName = ZString.Empty;
			Header.JPH_VesselName = testVessel.RV_Code;
			AssertNoMessageErrors(Header.JPH_VesselNameInfo);
			AssertNoMessageErrors(Header.JPH_RadioCallSignInfo);
			AssertNoMessageErrors(Header.JPH_RN_NKCountryOfRegInfo);

			testVessel.RV_RadioCallSign = "N/A";
			Header.JPH_VesselName = ZString.Empty;
			Header.JPH_VesselName = testVessel.RV_Code;
			AssertNoMessageErrors(Header.JPH_RadioCallSignInfo);
			AssertHasWarning(Header.JPH_RadioCallSignInfo, ValidationConstants.Header.InappropriateVesselCode("N/A"));

			testVessel.RV_RadioCallSign = "3T";
			Header.JPH_VesselName = ZString.Empty;
			Header.JPH_VesselName = testVessel.RV_Code;
			AssertNoMessageErrors(Header.JPH_RadioCallSignInfo);
			AssertHasWarning(header.JPH_RadioCallSignInfo, ValidationConstants.Header.InappropriateVesselCode("3T"));

			testVessel.RV_RadioCallSign = "TTTTT";
			Header.JPH_VesselName = ZString.Empty;
			Header.JPH_VesselName = testVessel.RV_Code;
			AssertNoMessageErrors(Header.JPH_RadioCallSignInfo);
			AssertHasWarning(Header.JPH_RadioCallSignInfo, ValidationConstants.Header.InappropriateVesselCode("TTTTT"));

			testVessel.RV_RadioCallSign = "123321";
			Header.JPH_VesselName = ZString.Empty;
			Header.JPH_VesselName = testVessel.RV_Code;
			AssertNoMessageErrors(Header.JPH_RadioCallSignInfo);
			AssertNoWarnings(Header.JPH_RadioCallSignInfo);

			var testBill = Header.Bills.AddNew();
			Factory.Save();

			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_VesselName = ZString.Empty; Header.RunPreSaveValidation(); }, Header.JPH_VesselNameInfo, testVessel.RV_Code, ZString.Empty);
			var newVessel = Factory.NewWithValidTestData<RefVessel>();
			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_VesselName = newVessel.RV_Code; Header.RunPreSaveValidation(); }, Header.JPH_VesselNameInfo, testVessel.RV_Code, newVessel.RV_Code);
		}

		public void TestCheckJPH_CarrierCode()
		{
			this.header = null;

			Header.JPH_CarrierCode = string.Empty;
			AssertNoErrors(Header.JPH_CarrierCodeInfo);
			AssertHasMessageErrorContaining(Header.JPH_CarrierCodeInfo, MandatoryValidation.YouHaveNotEntered);

			Header.JPH_CarrierCode = "12";
			AssertNoErrors(Header.JPH_CarrierCodeInfo);
			AssertHasMessageErrorContaining(Header.JPH_CarrierCodeInfo, ValidationConstants.Header.CarrierCodeInvalid);

			Header.JPH_CarrierCode = "12]4";
			AssertHasMessageErrorContaining(Header.JPH_CarrierCodeInfo, ValidationConstants.Shared.InvalidNACCSChar(Header.JPH_CarrierCodeInfo.HumanReadableName));
			AssertHasMessageErrorContaining(Header.JPH_CarrierCodeInfo, ValidationConstants.Header.CarrierCodeInvalid);

			Header.JPH_CarrierCode = "12,4";
			AssertNoErrors(Header.JPH_CarrierCodeInfo);
			AssertHasMessageErrorContaining(Header.JPH_CarrierCodeInfo, ValidationConstants.Header.CarrierCodeInvalid);

			Header.JPH_CarrierCode = "123";
			AssertNoMessageErrors(Header.JPH_CarrierCodeInfo);
			AssertNoErrors(Header.JPH_CarrierCodeInfo);
			AssertNoWarnings(Header.JPH_CarrierCodeInfo);

			Header.JPH_CarrierCode = "1234";
			AssertNoMessageErrors(Header.JPH_CarrierCodeInfo);
			AssertNoErrors(Header.JPH_CarrierCodeInfo);
			AssertNoWarnings(Header.JPH_CarrierCodeInfo);

			var testBill = Header.Bills.AddNew();
			Factory.Save();

			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_CarrierCode = ZString.Empty; }, Header.JPH_CarrierCodeInfo, "1234", ZString.Empty);
			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_CarrierCode = "4321"; }, Header.JPH_CarrierCodeInfo, "1234", "4321");
		}

		public void TestCheckJPH_RL_NKLoading()
		{
			this.header = null;

			Header.JPH_RL_NKLoading = string.Empty;
			AssertNoErrors(Header.JPH_RL_NKLoadingInfo);
			AssertHasMessageErrorContaining(Header.JPH_RL_NKLoadingInfo, MandatoryValidation.YouHaveNotEntered);

			Header.JPH_RL_NKLoading = "XXX[X";
			AssertHasMessageErrorContaining(Header.JPH_RL_NKLoadingInfo, ValidationConstants.Shared.InvalidNACCSChar(Header.JPH_RL_NKLoadingInfo.HumanReadableName));
			AssertHasMessageErrorContaining(Header.JPH_RL_NKLoadingInfo, ListValidation.InvalidCodeMessageError);

			Header.JPH_RL_NKLoading = "XXXXX";
			AssertNoErrors(Header.JPH_RL_NKLoadingInfo);
			AssertHasMessageErrorContaining(Header.JPH_RL_NKLoadingInfo, ListValidation.InvalidCodeMessageError);

			var testUNLOCO = Factory.NewWithValidTestData<RefUNLOCO>();
			testUNLOCO.RL_PortName = "LOCOLOCOLOCOLOCOLOCOLOCOLOCO";
			Header.JPH_RL_NKLoading = testUNLOCO.Code;
			AssertNoErrors(Header.JPH_RL_NKLoadingInfo);
			AssertHasWarningContaining(Header.JPH_RL_NKLoadingInfo, ValidationConstants.Shared.PortNameLengthExceeded);

			testUNLOCO.RL_PortName = "LOCOLOCOLOCOLOCOLOCOLOCOLOCO";
			testUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			Header.JPH_RL_NKLoading = testUNLOCO.Code;
			AssertNoErrors(Header.JPH_RL_NKLoadingInfo);
			AssertHasMessageErrorContaining(Header.JPH_RL_NKLoadingInfo, ValidationConstants.Header.LoadingPortCannotBeInJP);

			testUNLOCO.RL_PortName = "LOCO";
			testUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			Header.JPH_RL_NKLoading = testUNLOCO.Code;
			AssertNoErrors(Header.JPH_RL_NKLoadingInfo);
			AssertHasMessageErrorContaining(Header.JPH_RL_NKLoadingInfo, ValidationConstants.Header.LoadingPortCannotBeInJP);

			testUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			Header.JPH_RL_NKLoading = testUNLOCO.Code;
			AssertNoErrors(Header.JPH_RL_NKLoadingInfo);
			AssertNoMessageErrors(Header.JPH_RL_NKLoadingInfo);
			AssertNoWarnings(Header.JPH_RL_NKLoadingInfo);

			var testBill = Header.Bills.AddNew();
			var testUNLOCO2 = Factory.NewWithValidTestData<RefUNLOCO>();
			Factory.Save();

			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_RL_NKLoading = string.Empty; }, Header.JPH_RL_NKLoadingInfo, testUNLOCO.Code, ZString.Empty);
			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_RL_NKLoading = "XXXXX"; }, Header.JPH_RL_NKLoadingInfo, testUNLOCO.Code, "XXXXX");
			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_RL_NKLoading = testUNLOCO2.Code; }, Header.JPH_RL_NKLoadingInfo, testUNLOCO.Code, testUNLOCO2.Code);
			testUNLOCO2.RL_PortName = "LOCOLOCOLOCOLOCOLOCOLOCOLOCO";
			testUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_RL_NKLoading = testUNLOCO2.Code; }, Header.JPH_RL_NKLoadingInfo, testUNLOCO.Code, testUNLOCO2.Code);
			testUNLOCO2.RL_PortName = "LOCOLOCOLOCOLOCOLOCOLOCOLOCO";
			testUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_RL_NKLoading = testUNLOCO2.Code; }, Header.JPH_RL_NKLoadingInfo, testUNLOCO.Code, testUNLOCO2.Code);
			testUNLOCO2.RL_PortName = "LOCO";
			testUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_RL_NKLoading = testUNLOCO2.Code; }, Header.JPH_RL_NKLoadingInfo, testUNLOCO.Code, testUNLOCO2.Code);
		}

		public void TestCheckJPH_RL_NKDischarge()
		{
			this.header = null;

			Header.JPH_RL_NKDischarge = string.Empty;
			AssertHasMessageErrorContaining(Header.JPH_RL_NKDischargeInfo, MandatoryValidation.YouHaveNotEntered);

			Header.JPH_RL_NKDischarge = "XXX[X";
			AssertHasMessageErrorContaining(Header.JPH_RL_NKDischargeInfo, ValidationConstants.Shared.InvalidNACCSChar(Header.JPH_RL_NKDischargeInfo.HumanReadableName));
			AssertHasMessageErrorContaining(Header.JPH_RL_NKDischargeInfo, ListValidation.InvalidCodeMessageError);

			Header.JPH_RL_NKDischarge = "XXXXX";
			AssertHasMessageErrorContaining(Header.JPH_RL_NKDischargeInfo, ListValidation.InvalidCodeMessageError);

			var testUNLOCO = Factory.NewWithValidTestData<RefUNLOCO>();
			testUNLOCO.RL_PortName = "LOCOLOCOLOCOLOCOLOCOLOCOLOCO";
			Header.JPH_RL_NKDischarge = testUNLOCO.Code;
			AssertNoMessageErrors(Header.JPH_RL_NKDischargeInfo);
			AssertNoWarnings(Header.JPH_RL_NKDischargeInfo);

			testUNLOCO.RL_PortName = "LOCO";
			Header.JPH_RL_NKDischarge = testUNLOCO.Code;
			AssertNoMessageErrors(Header.JPH_RL_NKDischargeInfo);

			var testBill = Header.Bills.AddNew();
			var testUNLOCO2 = Factory.NewWithValidTestData<RefUNLOCO>();
			Factory.Save();

			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_RL_NKDischarge = string.Empty; }, Header.JPH_RL_NKDischargeInfo, testUNLOCO.Code, ZString.Empty);
			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_RL_NKDischarge = "XXXXX"; }, Header.JPH_RL_NKDischargeInfo, testUNLOCO.Code, "XXXXX");
			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_RL_NKDischarge = testUNLOCO2.Code; }, Header.JPH_RL_NKDischargeInfo, testUNLOCO.Code, testUNLOCO2.Code);
			testUNLOCO2.RL_PortName = "LOCOLOCOLOCOLOCOLOCOLOCOLOCO";
			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_RL_NKDischarge = testUNLOCO2.Code; }, Header.JPH_RL_NKDischargeInfo, testUNLOCO.Code, testUNLOCO2.Code);
		}

		public void TestCheckJPH_LoadingPortSuffix()
		{
			this.header = null;

			Header.JPH_LoadingPortSuffix = "A";
			AssertHasMessageErrorContaining(Header.JPH_LoadingPortSuffixInfo, ValidationConstants.Header.InvalidPortSuffix);

			Header.JPH_LoadingPortSuffix = "1";
			AssertNoMessageErrors(Header.JPH_LoadingPortSuffixInfo);

			var testBill = Header.Bills.AddNew();
			Factory.Save();

			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_LoadingPortSuffix = string.Empty; }, Header.JPH_LoadingPortSuffixInfo, "1", ZString.Empty);
			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_LoadingPortSuffix = "A"; }, Header.JPH_LoadingPortSuffixInfo, "1", "A");
			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_LoadingPortSuffix = "2"; }, Header.JPH_LoadingPortSuffixInfo, "1", "2");
		}

		public void TestCheckJPH_RelaxedAppId()
		{
			this.header = null;
			Header.JPH_RelaxedAppId = false;
			AssertNoErrors(Header.JPH_LoadingPortSuffixInfo);
			AssertNoMessageErrors(Header.JPH_LoadingPortSuffixInfo);
			AssertNoWarnings(Header.JPH_LoadingPortSuffixInfo);

			Header.JPH_RelaxedAppId = true;
			AssertNoErrors(Header.JPH_LoadingPortSuffixInfo);
			AssertNoMessageErrors(Header.JPH_LoadingPortSuffixInfo);
			AssertNoWarnings(Header.JPH_LoadingPortSuffixInfo);

			var testBill = Header.Bills.AddNew();
			Factory.Save();
			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_RelaxedAppId = false; }, Header.JPH_RelaxedAppIdInfo, "Y", "N");
		}

		public void TestCheckJPH_ETD()
		{
			this.header = null;
			var testTime = ValidationUtils.GetCurrentJPDate;

			Header.JPH_ETD = ZDateTime.Empty;
			AssertHasMessageErrorContaining(Header.JPH_ETDInfo, MandatoryValidation.YouHaveNotEntered);

			Header.JPH_ETD = testTime;
			AssertNoMessageErrors(Header.JPH_ETDInfo);

			var testBill = Header.Bills.AddNew();
			Factory.Save();

			var newTestTime = testTime.AddMinutes(10);
			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_ETD = ZDate.Empty; }, Header.JPH_ETDInfo, testTime.ToString(), ZString.Empty);
			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_ETD = newTestTime; }, Header.JPH_ETDInfo, testTime.ToString(), newTestTime.ToString());
		}

		public void TestCheckJPH_ETA()
		{
			this.header = null;
			var testTime = ValidationUtils.GetCurrentJPDate;

			Header.JPH_ETA = ZDateTime.Empty;
			AssertHasMessageErrorContaining(Header.JPH_ETAInfo, MandatoryValidation.YouHaveNotEntered);

			Header.JPH_ETA = testTime.AddDays(-1);
			AssertHasMessageErrorContaining(Header.JPH_ETAInfo, ValidationConstants.Header.PastDateNotAllowedForETA);

			Factory.Save();
			Header.JPH_ETA = testTime.AddDays(-2);
			AssertNoMessageErrors(Header.JPH_ETAInfo);
			AssertNoErrors(Header.JPH_ETAInfo);
			AssertNoWarnings(Header.JPH_ETAInfo);

			Header.JPH_ETA = testTime;
			AssertNoMessageErrors(Header.JPH_ETAInfo);
			AssertNoErrors(Header.JPH_ETAInfo);
			AssertNoWarnings(Header.JPH_ETAInfo);

			Header.JPH_ETD = testTime.AddDays(10);
			AssertHasMessageErrorContaining(Header.JPH_ETAInfo, ValidationConstants.Header.ETAShouldBeNoEarlierThanETD);

			var testBill = Header.Bills.AddNew();
			Factory.Save();
			var newTestTime = testTime.AddMinutes(10);

			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_ETA = ZDate.Empty; }, Header.JPH_ETAInfo, testTime.ToString(), ZString.Empty);
			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_ETA = newTestTime; }, Header.JPH_ETAInfo, testTime.ToString(), newTestTime.ToString());
		}

		public void TestCheckJPH_MasterBillNumber_NVOCC()
		{
			var testBill = Header.Bills.AddNew();

			Header.JPH_MasterBillNumber = string.Empty;
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, MandatoryValidation.YouHaveNotEntered);

			Header.JPH_MasterBillNumber = "12345";
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Shared.BOLNumMissingCarrierCode);

			Header.JPH_MasterBillNumber = "123456";
			AssertNoMessageErrors(Header.JPH_MasterBillNumberInfo);

			Header.JPH_MasterBillNumber = "12345,6";
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);

			Header.JPH_MasterBillNumber = "12,3456";
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Header.MBOLStartWithInvalidCarrierCode);

			Header.JPH_MasterBillNumber = "12|3456";
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Header.MBOLStartWithInvalidCarrierCode);

			Header.JPH_MasterBillNumber = "12[3456";
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Header.MBOLStartWithInvalidCarrierCode);

			Header.JPH_MasterBillNumber = "12]3456";
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Header.MBOLStartWithInvalidCarrierCode);

			Header.JPH_MasterBillNumber = "12{3456";
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Header.MBOLStartWithInvalidCarrierCode);

			Header.JPH_MasterBillNumber = "12}3456";
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Header.MBOLStartWithInvalidCarrierCode);

			Header.JPH_MasterBillNumber = "12~3456";
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Header.MBOLStartWithInvalidCarrierCode);

			Header.JPH_MasterBillNumber = "12`3456";
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Header.MBOLStartWithInvalidCarrierCode);

			Header.JPH_MasterBillNumber = "12^3456";
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Header.MBOLStartWithInvalidCarrierCode);

			Header.JPH_MasterBillNumber = "12_3456";
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Header.MBOLStartWithInvalidCarrierCode);

			Header.JPH_MasterBillNumber = "A---TEST002";
			AssertNoErrors(Header.JPH_MasterBillNumberInfo);
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Header.MBOLStartWithInvalidCarrierCode);
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Header.CarrierCodeInvalid);

			Header.JPH_MasterBillNumber = "AA--TEST002";
			AssertNoErrors(Header.JPH_MasterBillNumberInfo);
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Header.MBOLStartWithInvalidCarrierCode);
			AssertHasMessageErrorContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Header.CarrierCodeInvalid);

			Header.JPH_MasterBillNumber = "AAA-TEST002";
			AssertNoMessageErrors(Header.JPH_MasterBillNumberInfo);
			AssertNoErrors(Header.JPH_MasterBillNumberInfo);

			Header.JPH_MasterBillNumber = "AAAATEST002";
			AssertNoMessageErrors(Header.JPH_MasterBillNumberInfo);
			AssertNoErrors(Header.JPH_MasterBillNumberInfo);

			Header.JPH_CarrierCode = "SPQA";
			Header.JPH_MasterBillNumber = "SPQB56";
			AssertHasWarningContaining(Header.JPH_MasterBillNumberInfo, ValidationConstants.Header.MBOLNumDoesntMatchCarrierCode);
			AssertNoMessageErrors(Header.JPH_MasterBillNumberInfo);

			Header.JPH_CarrierCode = "SPQA";
			Header.JPH_MasterBillNumber = "SPQA56";
			AssertNoWarnings(Header.JPH_MasterBillNumberInfo);
			AssertNoMessageErrors(Header.JPH_MasterBillNumberInfo);
			AssertNoErrors(Header.JPH_MasterBillNumberInfo);

			Factory.Save();

			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_MasterBillNumber = "12345"; }, Header.JPH_MasterBillNumberInfo, "SPQA56", "12345");
			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_MasterBillNumber = "1234,56"; }, Header.JPH_MasterBillNumberInfo, "SPQA56", "1234,56");
			CheckValidationWhenBillStatusChanged(testBill, () => { Header.JPH_MasterBillNumber = "123456"; }, Header.JPH_MasterBillNumberInfo, "SPQA56", "123456");
		}

		public void TestCheckJPH_MasterBillNumber_VOCC()
		{
			var testBill = Header.Bills.AddNew();
			Header.JPH_IsShippingLineEntry = true;

			Header.JPH_MasterBillNumber = string.Empty;
			AssertNoMessageErrors(Header.JPH_MasterBillNumberInfo);
			AssertNoErrors(Header.JPH_MasterBillNumberInfo);

			Header.JPH_MasterBillNumber = "12345";
			AssertNoMessageErrors(Header.JPH_MasterBillNumberInfo);
			AssertNoErrors(Header.JPH_MasterBillNumberInfo);

			Header.JPH_MasterBillNumber = "123456";
			AssertNoMessageErrors(Header.JPH_MasterBillNumberInfo);
			AssertNoErrors(Header.JPH_MasterBillNumberInfo);

			Header.JPH_MasterBillNumber = "12345,6";
			AssertNoMessageErrors(Header.JPH_MasterBillNumberInfo);
			AssertNoErrors(Header.JPH_MasterBillNumberInfo);

			Header.JPH_MasterBillNumber = "12,3456";
			AssertNoMessageErrors(Header.JPH_MasterBillNumberInfo);
			AssertNoErrors(Header.JPH_MasterBillNumberInfo);

			Header.JPH_MasterBillNumber = "12|3456";
			AssertNoMessageErrors(Header.JPH_MasterBillNumberInfo);
			AssertNoErrors(Header.JPH_MasterBillNumberInfo);

			Header.JPH_MasterBillNumber = "12[3456";
			AssertNoMessageErrors(Header.JPH_MasterBillNumberInfo);
			AssertNoErrors(Header.JPH_MasterBillNumberInfo);

			Header.JPH_MasterBillNumber = "12]3456";
			AssertNoMessageErrors(Header.JPH_MasterBillNumberInfo);
			AssertNoErrors(Header.JPH_MasterBillNumberInfo);

			Header.JPH_MasterBillNumber = "12{3456";
			AssertNoMessageErrors(Header.JPH_MasterBillNumberInfo);
			AssertNoErrors(Header.JPH_MasterBillNumberInfo);

			Header.JPH_MasterBillNumber = "12}3456";
			AssertNoMessageErrors(Header.JPH_MasterBillNumberInfo);
			AssertNoErrors(Header.JPH_MasterBillNumberInfo);

			Header.JPH_MasterBillNumber = "12~3456";
			AssertNoMessageErrors(Header.JPH_MasterBillNumberInfo);
			AssertNoErrors(Header.JPH_MasterBillNumberInfo);

			Header.JPH_MasterBillNumber = "12`3456";
			AssertNoMessageErrors(Header.JPH_MasterBillNumberInfo);
			AssertNoErrors(Header.JPH_MasterBillNumberInfo);

			Header.JPH_MasterBillNumber = "12^3456";
			AssertNoMessageErrors(Header.JPH_MasterBillNumberInfo);
			AssertNoErrors(Header.JPH_MasterBillNumberInfo);

			Header.JPH_MasterBillNumber = "12_3456";
			AssertNoMessageErrors(Header.JPH_MasterBillNumberInfo);
			AssertNoErrors(Header.JPH_MasterBillNumberInfo);

			Header.JPH_MasterBillNumber = "A---TEST002";
			AssertNoMessageErrors(Header.JPH_MasterBillNumberInfo);
			AssertNoErrors(Header.JPH_MasterBillNumberInfo);

			Header.JPH_MasterBillNumber = "AA--TEST002";
			AssertNoMessageErrors(Header.JPH_MasterBillNumberInfo);
			AssertNoErrors(Header.JPH_MasterBillNumberInfo);

			Header.JPH_MasterBillNumber = "AAA-TEST002";
			AssertNoMessageErrors(Header.JPH_MasterBillNumberInfo);
			AssertNoErrors(Header.JPH_MasterBillNumberInfo);

			Header.JPH_MasterBillNumber = "AAAATEST002";
			AssertNoMessageErrors(Header.JPH_MasterBillNumberInfo);
			AssertNoErrors(Header.JPH_MasterBillNumberInfo);

			Header.JPH_CarrierCode = "SPQA";
			Header.JPH_MasterBillNumber = "SPQB56";
			AssertNoWarnings(Header.JPH_MasterBillNumberInfo);
			AssertNoMessageErrors(Header.JPH_MasterBillNumberInfo);
			AssertNoErrors(Header.JPH_MasterBillNumberInfo);

			Header.JPH_CarrierCode = "SPQA";
			Header.JPH_MasterBillNumber = "SPQA56";
			AssertNoWarnings(Header.JPH_MasterBillNumberInfo);
			AssertNoMessageErrors(Header.JPH_MasterBillNumberInfo);
			AssertNoErrors(Header.JPH_MasterBillNumberInfo);
		}

		public void TestCarrierCodeInbDocAddress()
		{
			var testOrganization = Factory.New<OrgHeader>();
			var testOrgAddress = Factory.NewWithValidTestData<OrgAddress>();
			testOrgAddress.OA_OH = testOrganization.PK;
			Header.Carrier.E2_OA_Address = testOrgAddress.PK;
			Assert(Header.JPH_CarrierCode.IsEmpty);
			AssertHasWarning(Header.Carrier.OrganisationPKInfo, ValidationConstants.Header.CarrierOrgHasNoJPCarrierCode);

			testOrganization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SPQA", Core.Constants.CountryCodes.Japan);
			Header.Carrier.E2_OA_Address = ZGuid.Empty;
			Header.Carrier.E2_OA_Address = testOrgAddress.PK;
			AssertNoNotifications(Header.Carrier.OrganisationPKInfo);

			testOrganization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, string.Empty, Core.Constants.CountryCodes.Japan);
			Header.Carrier.E2_OA_Address = ZGuid.Empty;
			Header.Carrier.E2_OA_Address = testOrgAddress.PK;
			AssertEquals(string.Empty, Header.JPH_CarrierCode);
			AssertHasWarning(Header.Carrier.OrganisationPKInfo, ValidationConstants.Header.CarrierOrgHasNoJPCarrierCode);

			testOrganization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SPQBA", Core.Constants.CountryCodes.Japan);
			Header.Carrier.E2_OA_Address = ZGuid.Empty;
			Header.Carrier.E2_OA_Address = testOrgAddress.PK;
			AssertEquals("SPQB", Header.JPH_CarrierCode);
			AssertHasWarningContaining(Header.Carrier.OrganisationPKInfo, ValidationConstants.Header.CarrierOrgHaveJPCarrierCodeReachedMaxAllowed);

			testOrganization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SPQA", Core.Constants.CountryCodes.Japan);
			Header.Carrier.E2_OA_Address = ZGuid.Empty;
			Header.Carrier.E2_OA_Address = testOrgAddress.PK;
			AssertEquals("SPQA", Header.JPH_CarrierCode);
			AssertNoNotifications(Header.Carrier.OrganisationPKInfo);
		}

		public void TestCarrierNotOnTransportsGenerateMessageError()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_IsShippingLine = true;
			carrier1.OH_FullName = "McLaren";
			carrier1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SPQA", Core.Constants.CountryCodes.Japan);

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingLine = true;
			carrier2.OH_FullName = "Button";
			var carrier2Address2 = Factory.NewWithValidTestData<OrgAddress>();
			carrier2Address2.OA_OH = carrier2.PK;
			carrier2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SPQA", Core.Constants.CountryCodes.Japan);

			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();
			carrier3.OH_IsShippingLine = true;
			carrier3.OH_FullName = "Perez";
			var carrier3Address2 = Factory.NewWithValidTestData<OrgAddress>();
			carrier3Address2.OA_OH = carrier3.PK;
			carrier3.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SPQA", Core.Constants.CountryCodes.Japan);

			var invalidAddress = Factory.New<OrgAddress>();
			invalidAddress.OA_OH = ZGuid.Empty;

			var consol = Factory.New<CommonConsol>();
			var leg1 = consol.Transports.AddNew();
			leg1.JW_OA_CarrierAddress = carrier1.MainAddress.PK;
			var leg2 = consol.Transports.AddNew();
			leg2.JW_OA_CarrierAddress = carrier2.MainAddress.PK;

			Header.JPH_ParentId = consol.PK;
			Header.JPH_ParentTableCode = consol.TablePrefix;
			Header.Carrier.E2_OA_Address = carrier3.MainAddress.PK;
			Header.Validation.ValidateJPH_CarrierCode();
			AssertHasMessageError("Expected a message error as carrier3 is unrelated to the carriers on both transport legs", Header.Carrier.OrganisationPKInfo, ValidationConstants.Header.CarrierNotOnRouting);

			Header.Carrier.E2_OA_Address = carrier2.MainAddress.PK;
			Header.Validation.ValidateJPH_CarrierCode();
			AssertNoMessageError("Expected no message error as the carrier matches a carrier from a rounting leg", Header.Carrier.OrganisationPKInfo, ValidationConstants.Header.CarrierNotOnRouting);

			Header.Carrier.E2_OA_Address = carrier2Address2.PK;
			Header.Validation.ValidateJPH_CarrierCode();
			AssertNoMessageError("Expected no message error as it carrier is still the same carrier as the second routing leg, just with a different address", Header.Carrier.OrganisationPKInfo, ValidationConstants.Header.CarrierNotOnRouting);

			Header.Carrier.E2_OA_Address = invalidAddress.PK;
			Header.Validation.ValidateJPH_CarrierCode();
			AssertNoMessageError("Expected no message error as the carrier address is invalid, not being linked to a carrier organisation", Header.Carrier.OrganisationPKInfo, ValidationConstants.Header.CarrierNotOnRouting);
		}

		#region Implementation

		void CheckValidationWhenBillStatusChanged(JPAFRBills testBill, Action changeAction, ZPropertyInfo targetInfo, ZString oldValue, ZString newValue)
		{
			testBill.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillRegistration;
			testBill.JPB_ReleaseStatus = ZString.Empty;
			changeAction();
			AssertHasErrorContaining(targetInfo, ValidationConstants.Shared.ChangingFieldWhenMessagingIsInProgress(targetInfo.HumanReadableName, oldValue, newValue));
			AssertNoErrorContaining(targetInfo, ValidationConstants.Shared.ChangingFieldWhenBillIsRegistered(targetInfo.HumanReadableName, oldValue, newValue));
			AssertNoWarnings(targetInfo);
			AssertNoMessageErrors(targetInfo);

			testBill.JPB_MessageStatus = ZString.Empty;
			testBill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			changeAction();
			AssertNoErrorContaining(targetInfo, ValidationConstants.Shared.ChangingFieldWhenMessagingIsInProgress(targetInfo.HumanReadableName, oldValue, newValue));
			AssertHasErrorContaining(targetInfo, ValidationConstants.Shared.ChangingFieldWhenBillIsRegistered(targetInfo.HumanReadableName, oldValue, newValue));
			AssertNoWarnings(targetInfo);
			AssertNoMessageErrors(targetInfo);
		}

		#endregion

		#region Preperation

		JPAFRHeader Header
		{
			get { return header ?? (header = Factory.New<JPAFRHeader>()); }
		}
		JPAFRHeader header;

		#endregion
	}
}
