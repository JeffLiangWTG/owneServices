using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class CusMAWBValidationAbstractTest : Customs.Business.Testing.CusMAWBValidationTest
	{
		public void TestCheckCM_OA_UnpackDepotAddress()
		{
			var org = Factory.New<OrgHeader>();
			MAWB.CM_OA_UnpackDepotAddress = org.MainAddress.PK;
			AssertHasMessageError(MAWB.CM_OA_UnpackDepotAddressInfo, CusMAWBValidation.NoCCP);
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "1234A", Core.Constants.CountryCodes.Australia);
			MAWB.CM_OA_UnpackDepotAddress = ZGuid.Empty;
			MAWB.CM_OA_UnpackDepotAddress = org.MainAddress.PK;
			AssertNoMessageError(MAWB.CM_OA_UnpackDepotAddressInfo, CusMAWBValidation.NoCCP);
		}

		public void TestFirstArrivalPortValidation()
		{
			MAWB.CM_RL_NKDischargePort = "SGSIN";

			MAWB.CM_RL_NKFirstArrivalPort = "AUDDD";
			AssertHasMessageErrors("Invalid", MAWB.CM_RL_NKFirstArrivalPortInfo);

			MAWB.CM_RL_NKFirstArrivalPort = "AUSYD";
			AssertNoNotifications("Valid", MAWB.CM_RL_NKFirstArrivalPortInfo);
		}

		public void TestRoutingPortValidation()
		{
			CheckRoutePort(MAWB, MAWB.CM_RL_NKRoutePort1Info);
			CheckRoutePort(MAWB, MAWB.CM_RL_NKRoutePort2Info);
			CheckRoutePort(MAWB, MAWB.CM_RL_NKRoutePort3Info);
		}

		public void TestApplicationCodeValidation()
		{
			MAWB.CM_ApplicationCode = MAWB.Lookups.ApplicationCodeList[0].Code;
			AssertNoErrors(MAWB.CM_ApplicationCodeInfo);

			MAWB.CM_ApplicationCode = "AAA";
			AssertHasErrors(MAWB.CM_ApplicationCodeInfo);

			MAWB.CM_ApplicationCode = "";
			AssertNoErrors(MAWB.CM_ApplicationCodeInfo);
		}

		public void TestMAWBNumberMatchesAirLinePrefix()
		{
			const string WarningMessage = "The Airline Prefix does not match the Airline 2 Letter Code in the Flight Number.";
			MAWB.CM_FlightNo = "";
			MAWB.CM_MAWB = "08298739457";
			AssertNoWarning(MAWB.CM_MAWBInfo, WarningMessage);
			MAWB.CM_FlightNo = "HW652";
			AssertNoWarning(MAWB.CM_MAWBInfo, WarningMessage);
			MAWB.CM_FlightNo = "QF112";
			AssertHasWarning(MAWB.CM_MAWBInfo, WarningMessage);
			MAWB.CM_MAWB = "08149837497";
			AssertNoWarning(MAWB.CM_MAWBInfo, WarningMessage);
			MAWB.CM_MAWB = "5555555LOL5";
			AssertHasWarning(MAWB.CM_MAWBInfo, WarningMessage);
		}

		public void TestResponsiblePartyIDValidation()
		{
			MAWB.CM_ResponsiblePartyID = "87003014042";
			AssertNoMessageErrors(MAWB.CM_ResponsiblePartyIDInfo);
			var responsibleParty = Factory.New<OrgHeader>();
			responsibleParty.PrimaryRegistrationNumber.Number = ZString.Empty;
			responsibleParty.OH_FullName = "Cuckoo Squeaker";
			var cusCode = responsibleParty.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "123456");
			cusCode.OK_OA_PremisesAddress = Factory.New<OrgAddress>().PK;
			MAWB.CM_ResponsiblePartyID = "INVALID";
			AssertHasMessageError(MAWB.CM_ResponsiblePartyIDInfo, "The Responsible Party ID is not a valid ABN");
			MAWB.CM_ResponsiblePartyID = ZString.Empty;
			MAWB.CM_OH_ResponsibleParty = responsibleParty.PK;
			AssertHasWarnings(MAWB.CM_ResponsiblePartyIDInfo);

			MAWB.CM_OH_ResponsibleParty = ZGuid.Empty;
			var cusHAWB = (CusHAWBBase)MAWB.ChildBills.AddNew();
			cusHAWB.Validation.ValidateCS_ResponsiblePartyID();
			AssertHasMessageError(cusHAWB.CS_ResponsiblePartyIDInfo, CusHAWBValidation.ResponsiblePartEmpty);
			cusHAWB.CS_ResponsiblePartyID = "87003014042";
			AssertNoMessageError(cusHAWB.CS_ResponsiblePartyIDInfo, CusHAWBValidation.ResponsiblePartEmpty);
		}

		public virtual void TestFlightNoValidation()
		{
			MAWB.CM_FlightNo = "F1";//too short
			Assert("Flight No too short", MAWB.CM_FlightNoInfo.HasNotifications());

			MAWB.CM_FlightNo = "1F23456";//too long
			Assert("Flight No too long", MAWB.CM_FlightNoInfo.HasNotifications());

			MAWB.CM_FlightNo = "111";
			Assert("Flight No invalid", MAWB.CM_FlightNoInfo.HasNotifications());

			MAWB.CM_FlightNo = "Q12";
			Assert("Flight no valid", !MAWB.CM_FlightNoInfo.HasNotifications());
		}

		public virtual void TestCheckCM_RL_NKDischargePort()
		{
			MAWB.CM_RL_NKDischargePort = ZString.Empty;
			AssertHasMessageErrors("when empty", MAWB.CM_RL_NKDischargePortInfo);

			MAWB.CM_RL_NKDischargePort = "AUSYD";
			AssertNoNotifications("when set", MAWB.CM_RL_NKDischargePortInfo);

			MAWB.CM_RL_NKDischargePort = GetNewPort().RL_Code;
			AssertHasMessageErrors("when invalid", MAWB.CM_RL_NKDischargePortInfo);
		}

		public void TestCheckCM_ArrivalDate()
		{
			MAWB.Validation.ValidateCM_ArrivalDate();
			AssertHasMessageErrors("by default", MAWB.CM_ArrivalDateInfo);

			MAWB.CM_ArrivalDate = ZDateTime.Now;
			AssertNoMessageErrors("when set", MAWB.CM_ArrivalDateInfo);
		}

		public void TestFirstArrivalPortOnlyRequiredIfNotDischargingInAustralia()
		{
			MAWB.CM_RL_NKLoadPort = "NZAKL";
			MAWB.CM_RL_NKDischargePort = "SGSIN";
			AssertHasMessageError(MAWB.CM_RL_NKFirstArrivalPortInfo, "Port of First Arrival is required when cargo is in transit (not discharging in Australia).");

			MAWB.CM_RL_NKFirstArrivalPort = "AUSYD";
			AssertNoNotifications(MAWB.CM_RL_NKFirstArrivalPortInfo);

			MAWB.CM_RL_NKFirstArrivalPort = "AQMCM";
			AssertHasMessageError(MAWB.CM_RL_NKFirstArrivalPortInfo, "Port of First Arrival must be an Australian port.");

			MAWB.CM_RL_NKDischargePort = "AUSYD";
			AssertHasMessageError(MAWB.CM_RL_NKFirstArrivalPortInfo, "Port of First Arrival must be blank for non-transit cargo (discharging in Australia).");

			MAWB.CM_RL_NKFirstArrivalPort = ZString.Empty;
			AssertNoNotifications(MAWB.CM_RL_NKFirstArrivalPortInfo);
		}

		protected override Customs.Business.CusMAWB GetNewMAWB() => Factory.New<CusMAWB>();

		CusMAWBBase mawb;
		CusMAWBBase MAWB => mawb ?? (mawb = (CusMAWBBase)GetNewMAWB());

		protected RefUNLOCO GetNewPort()
		{
			RefUNLOCO port = Factory.NewWithValidTestData<RefUNLOCO>();
			port.RL_Code = "CASYD";
			port.RL_HasAirport = true;
			return port;
		}

		void CheckRoutePort(CusMAWBBase masterBill, ZPropertyInfo routePortInfo)
		{
			masterBill[routePortInfo.Name] = "";
			AssertNoErrors(routePortInfo);

			masterBill[routePortInfo.Name] = "AAAAA";
			AssertHasErrors(routePortInfo);

			masterBill[routePortInfo.Name] = "USLAX";
			AssertNoErrors(routePortInfo);
		}
	}
}
