using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRCusMAWBValidationTest : AirCargoCusMAWBValidationTest
	{
		public void TestKeyMessagingFields()
		{
			string mAWBNo = "08162424242";
			string flightNo = "QF123";
			string responsibleParty = "23562342";
			ZDateTime arrivalDate = ZDateTime.Now;
			string masterHouseBill = "92382690";

			MAWB.CM_MAWB = mAWBNo;
			MAWB.CM_FlightNo = flightNo;
			MAWB.CM_ResponsiblePartyID = responsibleParty;
			MAWB.CM_ArrivalDate = arrivalDate;
			MAWB.CM_MasterHouseBill = masterHouseBill;

			CusHAWB hAWB = MAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "23423";
			hAWB.CMRMessageStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			hAWB.CS_CM = MAWB.PK;
			Factory.Save();

			MAWB.CM_MAWB = "234";
			Assert("Should have an error", MAWB.CM_MAWBInfo.GetErrors().Contains(CustomsValidation.ErrorString.Replace("@", mAWBNo)));
			MAWB.CM_FlightNo = "TT324";
			Assert("Should have an error", MAWB.CM_FlightNoInfo.GetErrors().Contains(CustomsValidation.ErrorString.Replace("@", flightNo)));
			MAWB.CM_ResponsiblePartyID = "123";
			Assert("Should have an error", MAWB.CM_ResponsiblePartyIDInfo.GetErrors().Contains(CustomsValidation.ErrorString.Replace("@", responsibleParty)));
			MAWB.CM_ArrivalDate = new ZDateTime(2006, 01, 01);
			Assert("Should have an error", MAWB.CM_ArrivalDateInfo.GetErrors().Contains(CustomsValidation.ErrorString.Replace("@", arrivalDate.ToString())));
			MAWB.CM_MasterHouseBill = "1";
			Assert("Should not have an error", !MAWB.CM_MasterHouseBillInfo.GetErrors().Contains(CustomsValidation.ErrorString.Replace("@", masterHouseBill)));

			MAWB.CM_MAWB = mAWBNo;
			Assert("Should have no errors", !MAWB.CM_MAWBInfo.HasErrors());
			MAWB.CM_FlightNo = flightNo;
			Assert("Should have no errors", !MAWB.CM_FlightNoInfo.HasErrors());
			MAWB.CM_ResponsiblePartyID = responsibleParty;
			Assert("Should have no errors", !MAWB.CM_ResponsiblePartyIDInfo.HasErrors());
			MAWB.CM_ArrivalDate = arrivalDate;
			Assert("Should have no errors", !MAWB.CM_ArrivalDateInfo.HasErrors());
			MAWB.CM_MasterHouseBill = masterHouseBill;
			Assert("Should have no errors", !MAWB.CM_MasterHouseBillInfo.HasErrors());
		}

		public override void TestFlightNoValidation()
		{
			MAWB.CM_FlightNo = "QF";//too short
			Assert("Flight No too short", MAWB.CM_FlightNoInfo.HasNotifications());

			MAWB.CM_FlightNo = "QF23456";//too long
			Assert("Flight No too long", MAWB.CM_FlightNoInfo.HasNotifications());

			MAWB.CM_FlightNo = "111";
			Assert("Flight No invalid", MAWB.CM_FlightNoInfo.HasNotifications());

			MAWB.CM_FlightNo = "QF1";
			AssertNoMessageError(MAWB.CM_FlightNoInfo, "The flight number prefix is not a valid Airline code.");
			MAWB.CM_FlightNo = "XX1";
			AssertHasMessageError(MAWB.CM_FlightNoInfo, "The flight number prefix is not a valid Airline code.");
			MAWB.CM_FlightNo = "XX";
			AssertHasMessageError(MAWB.CM_FlightNoInfo, "The flight number prefix is not a valid Airline code.");
			MAWB.CM_FlightNo = "X";
			AssertHasMessageError(MAWB.CM_FlightNoInfo, "The flight number prefix is not a valid Airline code.");
			MAWB.CM_FlightNo = "";
			AssertNoMessageError(MAWB.CM_FlightNoInfo, "The flight number prefix is not a valid Airline code.");
		}

		public void TestMultipleAirlineRecordsForSameAWBPrefix()
		{
			MAWB.CM_FlightNo = "QF1";
			AssertNoMessageError(MAWB.CM_FlightNoInfo, "The flight number prefix is not a valid Airline code.");
			MAWB.CM_FlightNo = "XX1";
			AssertHasMessageError(MAWB.CM_FlightNoInfo, "The flight number prefix is not a valid Airline code.");

			MAWB.CM_FlightNo = "SQ410";
			AssertNoMessageError(MAWB.CM_FlightNoInfo, "The flight number prefix is not a valid Airline code.");

			var sqFreight = Factory.NewWithValidTestData<RefAirline>();
			sqFreight.RM_TwoCharacterCode = "SQ";
			sqFreight.RM_ThreeLetterCode = "SQC";
			sqFreight.RM_AirlineName1 = "Singapore Airlines Cargo Pte. Ltd.";
			sqFreight.RM_AddressLine1 = "30 Airline Road";
			sqFreight.RM_AddressLine2 = "05-J SATS Airfreight Terminal 5";
			sqFreight.RM_AirlineCity = "Singapore";
			sqFreight.RM_EagleAddedAirlinePrefixOrAccountingCode = "618";

			MAWB.CM_FlightNo = "SQ350";
			AssertNoMessageError("Airlines with multiple records should not error on valid Airline code prefix", MAWB.CM_FlightNoInfo, "The flight number prefix is not a valid Airline code.");
		}

		protected override Customs.Business.CusMAWB GetNewMAWB()
		{
			var result = Factory.New<CusMAWB>();
			result.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			return result;
		}
	}
}
