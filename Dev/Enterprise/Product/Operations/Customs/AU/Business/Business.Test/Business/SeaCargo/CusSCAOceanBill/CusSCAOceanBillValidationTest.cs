using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class CusSCAOceanBillValidationTest : BusinessObjectValidationTestCase
	{
		public void TestVesselName()
		{
			var oceanBill = GetNewOceanBill();
			oceanBill.CB_LloydsIMO = "foo";
			var refVessel = Factory.New<RefVessel>();
			refVessel.RV_Code = "Code1";
			refVessel.RV_LloydsNumber = "foo";
			Factory.Save();

			oceanBill.Validation.ValidateCB_VesselName();
			AssertNoWarning(oceanBill.CB_VesselNameInfo, "Vessel is not on file.");

			oceanBill.CB_VesselName = "Code1";
			AssertNoWarning(oceanBill.CB_VesselNameInfo, "Vessel is not on file.");

			oceanBill.CB_LloydsIMO = "";
			AssertNoWarning(oceanBill.CB_VesselNameInfo, "Vessel is not on file.");

			oceanBill.CB_LloydsIMO = "foo2";
			AssertHasWarning(oceanBill.CB_VesselNameInfo, "Vessel is not on file.");

			oceanBill.CB_LloydsIMO = "foo";
			oceanBill.CB_VesselName = "Code2";
			AssertHasWarning(oceanBill.CB_VesselNameInfo, "Vessel is not on file.");
		}

		public void TestCheckFilter()
		{
			var oceanBill = GetNewOceanBill();

			oceanBill.CustomsShipmentStatusFilter = "~~~";
			AssertHasMessageErrorContaining(oceanBill.CustomsShipmentStatusFilterInfo, ListValidation.InvalidCodeMessageError);

			oceanBill.CustomsShipmentStatusFilter = GetValidCustomsShipmentStatus(oceanBill);
			AssertNoMessageErrorContaining(oceanBill.CustomsShipmentStatusFilterInfo, ListValidation.InvalidCodeMessageError);

			oceanBill.CustomsMessageStatusFilter = "~~~";
			AssertHasMessageErrorContaining(oceanBill.CustomsMessageStatusFilterInfo, ListValidation.InvalidCodeMessageError);

			oceanBill.CustomsMessageStatusFilter = GetValidCustomsMessageStatus(oceanBill);
			AssertNoMessageErrorContaining(oceanBill.CustomsMessageStatusFilterInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestOceanBillValidation()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.Validation.ValidateCB_OceanBill();
			Assert("Pre-Condition, Oceanbill Number initially has Messages Errors", oceanBill.CB_OceanBillInfo.HasMessageErrors());
			oceanBill.CB_OceanBill = "TEST123";
			Assert("Oceanbill Number should have no message errors", !oceanBill.CB_OceanBillInfo.HasMessageErrors());
		}

		public void TestPortOfLoadingValidation()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.Validation.ValidateCB_RL_NKPortOfLoading();
			Assert("Pre-Condition, Port of Loading initially has Messages Errors", oceanBill.CB_RL_NKPortOfLoadingInfo.HasMessageErrors() && !oceanBill.CB_RL_NKPortOfLoadingInfo.HasErrors());
			oceanBill.CB_RL_NKPortOfLoading = "TEST1";
			Assert("Port of Loading should have errors as TEST1 is not a valid UNLOCO", oceanBill.CB_RL_NKPortOfLoadingInfo.HasMessageErrors());
			oceanBill.CB_RL_NKPortOfLoading = "USLAX";
			Assert("Port of Loading should have no message errors", !oceanBill.CB_RL_NKPortOfLoadingInfo.HasMessageErrors() && !oceanBill.CB_RL_NKPortOfLoadingInfo.HasErrors());
			oceanBill.CB_RL_NKPortOfLoading = "FKMPN";
			Assert("Port of Loading should have message errors as port is not sea port", !oceanBill.CB_RL_NKPortOfLoadingInfo.HasMessageErrors() && !oceanBill.CB_RL_NKPortOfLoadingInfo.HasErrors() && oceanBill.CB_RL_NKPortOfLoadingInfo.HasWarnings());
			oceanBill.CB_RL_NKPortOfLoading = "FK///";
			Assert("Port of Loading should have no message errors as it is the override is the port is not known as a port for sea cargo", !oceanBill.CB_RL_NKPortOfLoadingInfo.HasMessageErrors() && !oceanBill.CB_RL_NKPortOfLoadingInfo.HasErrors() && !oceanBill.CB_RL_NKPortOfLoadingInfo.HasWarnings());
		}

		public void TestPortOfDischargeValidation()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_RL_NKPortOfDischarge = "";
			oceanBill.Validation.ValidateCB_RL_NKPortOfDischarge();
			Assert("Pre-Condition, Port of Discharge initially has Messages Errors", oceanBill.CB_RL_NKPortOfDischargeInfo.HasMessageErrors() && !oceanBill.CB_RL_NKPortOfDischargeInfo.HasErrors());
			oceanBill.CB_RL_NKPortOfDischarge = "TEST2";
			Assert("Port of Discharge should have message errors as Port as not a valid UNLOCO", !oceanBill.CB_RL_NKPortOfDischargeInfo.HasErrors() && oceanBill.CB_RL_NKPortOfDischargeInfo.HasMessageErrors());
			oceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
			Assert("Port of Discharge should have no message errors", !oceanBill.CB_RL_NKPortOfDischargeInfo.HasMessageErrors() && !oceanBill.CB_RL_NKPortOfDischargeInfo.HasErrors());
		}

		public void TestVoyageNumberValidation()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.Validation.ValidateCB_Voyage();
			Assert("Pre-Condition, Voayge Number initially has Messages Errors", oceanBill.CB_VoyageInfo.HasMessageErrors());
			oceanBill.CB_Voyage = "12E";
			Assert("Voyage Number should have no message errors", !oceanBill.CB_VoyageInfo.HasMessageErrors());
			oceanBill.CB_Voyage = "1234567";
			Assert("Voyage number should not have more than 6 chars", oceanBill.CB_VoyageInfo.HasMessageErrors());
		}

		public void TestAUADLAcceptedAsPortOfDischarge()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_RL_NKPortOfDischarge = "";
			oceanBill.Validation.ValidateCB_RL_NKPortOfDischarge();
			Assert("Pre-Condition, Port of Discharge initially has Messages Errors", oceanBill.CB_RL_NKPortOfDischargeInfo.HasMessageErrors() && !oceanBill.CB_RL_NKPortOfDischargeInfo.HasErrors());
			oceanBill.CB_RL_NKPortOfDischarge = "AUADL";
			Assert("Port of Discharge should have no message errors", !oceanBill.CB_RL_NKPortOfDischargeInfo.HasMessageErrors() && !oceanBill.CB_RL_NKPortOfDischargeInfo.HasErrors());
			Assert("Port of Discharge should have no warningss", !oceanBill.CB_RL_NKPortOfDischargeInfo.HasWarnings());
		}

		public void TestCheckCB_ApplicationCode()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ApplicationCode = "JNK";
			AssertHasMessageErrors("Ocean Bill Application Code - \"JNK\"", oceanBill.CB_ApplicationCodeInfo);

			oceanBill.CB_ApplicationCode = oceanBill.Lookups.ApplicationCodeList[0].Code;
			AssertNoMessageErrors("Ocean Bill Application Code - from Code List", oceanBill.CB_ApplicationCodeInfo);

			oceanBill.CB_ApplicationCode = ZString.Empty;
			AssertNoMessageErrors("Ocean Bill Application Code - \"\"", oceanBill.CB_ApplicationCodeInfo);
		}

		public void TestVesselValidation()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();

			oceanBill.CB_LloydsIMO = "";
			AssertNoMessageErrors("Blank Lloyds, blank vessel is OK", oceanBill.CB_VesselNameInfo);

			oceanBill.CB_VesselName = "ABC";
			AssertHasMessageErrors("Blank Lloyds, incorrect vessel is NOT ok", oceanBill.CB_VesselNameInfo);

			oceanBill.CB_LloydsIMO = "324";
			AssertNoMessageErrors("Specified Lloyds, incorrect vessel is OK", oceanBill.CB_VesselNameInfo);

			oceanBill.CB_LloydsIMO = "";
			oceanBill.CB_VesselName = "ABC";
			AssertHasMessageErrors("Blank Lloyds, incorrect vessel is NOT ok", oceanBill.CB_VesselNameInfo);

			oceanBill.CB_LloydsIMO = "";
			oceanBill.CB_VesselName = Factory.NewWithValidTestData<RefVessel>().RV_Code;
			AssertNoMessageErrors("Blank Lloyds, valid vessel is OK", oceanBill.CB_VesselNameInfo);
		}

		public void TestVesselValidation_VesselNotFound()
		{
			const string vesselName = "Vessel001";
			AssertEquals("Precondition: There are no Vessels named " + vesselName, 0, RefVessel.LookupVesselByName(vesselName, Factory).Length);

			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_VesselName = vesselName;
			oceanBill.CB_LloydsIMO = "998877";
			AssertHasWarning(oceanBill.CB_VesselNameInfo, "A Vessel with this Name and Lloyds Number cannot be found.");

			var vesselA = Factory.New<RefVessel>();
			vesselA.RV_Code = vesselName;
			vesselA.RV_LloydsNumber = "";
			oceanBill.Validation.ValidateCB_VesselName();
			AssertHasWarning(oceanBill.CB_VesselNameInfo, "A Vessel with this Name and Lloyds Number cannot be found.");

			var vesselB = Factory.New<RefVessel>();
			vesselB.RV_Code = vesselName;
			vesselB.RV_LloydsNumber = "998877";
			oceanBill.Validation.ValidateCB_VesselName();
			AssertNoWarning(oceanBill.CB_VesselNameInfo, "A Vessel with this Name and Lloyds Number cannot be found.");
		}

		public void TestLloydsValidation()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.Validation.ValidateCB_LloydsIMO();
			Assert("Pre-Condition, Vessel initially has Messages Errors", oceanBill.CB_LloydsIMOInfo.HasMessageErrors());
			oceanBill.CB_VesselName = "TESTWRONGVESSEL";
			Assert("Vessel should have message errors as it is not a valid vessel", oceanBill.CB_VesselNameInfo.HasMessageErrors());
			var vessel = Factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, TestVessel);
			AssertEquals("Vessel should have the following Lloyds Number", vessel.RV_LloydsNumber, TestLloyds);
			vessel.RV_LloydsNumber = "1234";
			Assert("Vessel should have message errors as the lloyds is not 7 in length", oceanBill.CB_VesselNameInfo.HasMessageErrors());
			vessel.RV_LloydsNumber = "8610033";
			Assert("Vessel should not have message errors", oceanBill.CB_VesselNameInfo.HasMessageErrors());
		}

		public void TestCB_RL_NKPortOfFirstArrival()
		{
			oceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
			oceanBill.CB_RL_NKPortOfFirstArrival = "";
			AssertEquals("If cargo is discharged in AU, First arrival is not mandatory", false, oceanBill.CB_RL_NKPortOfFirstArrivalInfo.HasNotifications());

			oceanBill.CB_RL_NKPortOfDischarge = "NZAKL";
			oceanBill.CB_RL_NKPortOfFirstArrival = "";
			AssertEquals("If cargo is discharged overseas, first arrival is mandatory", true, oceanBill.CB_RL_NKPortOfFirstArrivalInfo.HasMessageErrors());

			oceanBill.CB_RL_NKPortOfFirstArrival = "USLAX";
			AssertEquals("If cargo is discharged overseas, first arrival should be an AU port", true, oceanBill.CB_RL_NKPortOfFirstArrivalInfo.HasMessageErrors());

			oceanBill.CB_RL_NKPortOfFirstArrival = "AUSYD";
			AssertEquals("If cargo is discharged overseas, first arrival should be an AU port", false, oceanBill.CB_RL_NKPortOfFirstArrivalInfo.HasMessageErrors());
		}

		public void TestPrincipalIDValidation()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.Validation.ValidateCB_PrincipalID();
			Assert("Pre-Condition, Principal ID initially has Messages Errors", oceanBill.CB_PrincipalIDInfo.HasMessageErrors());
			oceanBill.CB_PrincipalID = ABN;
			Assert("Principal ID is Valid ABN - no Messages Errors expected", !oceanBill.CB_PrincipalIDInfo.HasMessageErrors());
		}

		public void TestShippingLineValidation()
		{
			oceanBill.Validation.ValidateCB_PrincipalID();
			AssertHasMessageErrors("by default", oceanBill.CB_PrincipalIDInfo);

			OrgHeader org = OrgHeader.New(Factory);
			org.OH_IsShippingLine = true;
			oceanBill.CB_OH_ShippingLine = org.PK;
			AssertHasMessageErrors("when set with no abn", oceanBill.CB_PrincipalIDInfo);

			org = OrgHeader.New(Factory);
			org.OH_IsShippingLine = true;
			org.PrimaryRegistrationNumber.Number = ABN;

			oceanBill.CB_OH_ShippingLine = ZGuid.Empty;
			oceanBill.CB_OH_ShippingLine = org.PK;

			oceanBill.Validation.ValidateCB_PrincipalID();
			AssertNoNotifications("when set with abn", oceanBill.CB_PrincipalIDInfo);
		}

		public void TestShippingLineABNLength()
		{
			OrgHeader org = OrgHeader.New(Factory);
			org.OH_IsShippingLine = true;
			org.PrimaryRegistrationNumber.Number = "12345678901234";
			oceanBill.CB_OH_ShippingLine = org.PK;
			oceanBill.Validation.ValidateCB_OH_ShippingLine();
			AssertHasMessageErrors("ABN too long", oceanBill.CB_PrincipalIDInfo);
			org.PrimaryRegistrationNumber.Number = "12345678901";
			oceanBill.Validation.ValidateCB_OH_ShippingLine();
			AssertNoNotifications("abn should be okay now", oceanBill.CB_OH_ShippingLineInfo);
		}
		public void TestKeyMessagingFields()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();

			string masterNumber = "08162424242";
			string lloyds = "9879464";
			string voyage = "92382690";
			string principalID = "32566";
			string responsibleParty = "23562342";

			oceanBill.CB_OceanBill = masterNumber;
			oceanBill.CB_LloydsIMO = lloyds;
			oceanBill.CB_ResponsiblePartyID = responsibleParty;
			oceanBill.CB_PrincipalID = principalID;
			oceanBill.CB_Voyage = voyage;

			CusSCAHouse house = oceanBill.HouseBills.AddNew();
			house.CA_HouseBill = "23423";
			house.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			Factory.Save();

			oceanBill.CB_OceanBill = "234";
			Assert("Should have an error", oceanBill.CB_OceanBillInfo.GetErrors().Contains(CustomsValidation.ErrorString.Replace("@", masterNumber)));
			oceanBill.CB_LloydsIMO = "TT324";
			Assert("Should have an error", oceanBill.CB_LloydsIMOInfo.GetErrors().Contains(CustomsValidation.ErrorString.Replace("@", lloyds)));
			oceanBill.CB_ResponsiblePartyID = "123";
			Assert("Should have an error", oceanBill.CB_ResponsiblePartyIDInfo.GetErrors().Contains(CustomsValidation.ErrorString.Replace("@", responsibleParty)));
			oceanBill.CB_PrincipalID = "23456";
			Assert("Should have an error", oceanBill.CB_PrincipalIDInfo.GetErrors().Contains(CustomsValidation.ErrorString.Replace("@", principalID)));
			oceanBill.CB_Voyage = "1";
			Assert("Should have an error", oceanBill.CB_VoyageInfo.GetErrors().Contains(CustomsValidation.ErrorString.Replace("@", voyage)));

			oceanBill.CB_OceanBill = masterNumber;
			Assert("Should have no errors", !oceanBill.CB_OceanBillInfo.HasErrors());
			oceanBill.CB_LloydsIMO = lloyds;
			Assert("Should have no errors", !oceanBill.CB_LloydsIMOInfo.HasErrors());
			oceanBill.CB_ResponsiblePartyID = responsibleParty;
			Assert("Should have no errors", !oceanBill.CB_ResponsiblePartyIDInfo.HasErrors());
			oceanBill.CB_PrincipalID = principalID;
			Assert("Should have no errors", !oceanBill.CB_PrincipalIDInfo.HasErrors());
			oceanBill.CB_Voyage = voyage;
			Assert("Should have no errors", !oceanBill.CB_VoyageInfo.HasErrors());
		}

		#region Implementation

		CusSCAOceanBill oceanBill;

		protected CusSCAOceanBill GetNewOceanBill()
		{
			return Factory.New<CusSCAOceanBill>();
		}

		protected string GetValidCustomsShipmentStatus(CusSCAOceanBill oceanBill)
		{
			var list = oceanBill.Lookups.CustomsShipmentStatusList;
			return list.Count > 0 ? list[0].Code : "";
		}

		protected string GetValidCustomsMessageStatus(CusSCAOceanBill oceanBill)
		{
			var list = oceanBill.Lookups.CustomsMessageStatusList;
			return list.Count > 0 ? list[0].Code : "";
		}

		protected override void SetUp()
		{
			base.SetUp();
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			oceanBill = Factory.New<CusSCAOceanBill>();
		}

		#endregion

		#region Contants

		public const string ABN = "21008057734";
		public const string TestVessel = "ADMIRALENGRACHT";
		public const string TestLloyds = "8811924";

		#endregion

	}
}
