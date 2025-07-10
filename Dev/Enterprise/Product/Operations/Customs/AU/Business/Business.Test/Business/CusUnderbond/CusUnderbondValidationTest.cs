using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusUnderbondValidation))]
	class CusUnderbondValidationTest : Customs.Business.Testing.CusUnderbondValidationTest
	{
		public void TestC4_OriginPremiseIDCannotBeEmpty()
		{
			Underbond.C4_OriginPremiseID = ZString.Empty;
			AssertHasMessageError(Underbond.C4_OriginPremiseIDInfo, MandatoryValidation.YouHaveNotEntered + " an " + Underbond.C4_OriginPremiseIDInfo.Description + ".");
		}

		public void TestC4_OriginPremiseIDInvalidCode()
		{
			Underbond.C4_OriginPremiseID = "12C";
			AssertHasMessageErrors(Underbond.C4_OriginPremiseIDInfo);
			Underbond.C4_OriginPremiseID = "1234D";
			AssertNoMessageErrors(Underbond.C4_OriginPremiseIDInfo);
		}

		public void TestCheckC4_RL_NKTranshipDestPort()
		{
			var transhipBizo = Factory.New<CusMAWB>();
			var transhipUnderbond = transhipBizo.Underbonds.AddNew();
			transhipUnderbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.Transshipment;
			transhipUnderbond.Validation.ValidateC4_RL_NKTranshipDestPort();
			AssertHasMessageError(transhipUnderbond.C4_RL_NKTranshipDestPortInfo, MandatoryValidation.YouHaveNotEntered + " a " + transhipUnderbond.C4_RL_NKTranshipDestPortInfo.Description + ".");
			transhipUnderbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.OtherMovement;
			transhipUnderbond.Validation.ValidateC4_RL_NKTranshipDestPort();
			AssertNoMessageError(transhipUnderbond.C4_RL_NKTranshipDestPortInfo, MandatoryValidation.YouHaveNotEntered + " a " + transhipUnderbond.C4_RL_NKTranshipDestPortInfo.Description + ".");
			transhipUnderbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.Transshipment;
			transhipUnderbond.Validation.ValidateC4_RL_NKTranshipDestPort();
			AssertHasMessageError(transhipUnderbond.C4_RL_NKTranshipDestPortInfo, MandatoryValidation.YouHaveNotEntered + " a " + transhipUnderbond.C4_RL_NKTranshipDestPortInfo.Description + ".");
			transhipUnderbond.C4_ParentID = ZGuid.Empty;
			transhipUnderbond.Validation.ValidateC4_RL_NKTranshipDestPort();
			AssertNoMessageError(transhipUnderbond.C4_RL_NKTranshipDestPortInfo, MandatoryValidation.YouHaveNotEntered + " a " + transhipUnderbond.C4_RL_NKTranshipDestPortInfo.Description + ".");

			var nonTranshipBizo = Factory.New<CusSeaManOBLDetail>();
			var nonTranshipUnderbond = nonTranshipBizo.AllUnderbonds.AddNew();
			nonTranshipUnderbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.Transshipment;
			nonTranshipUnderbond.Validation.ValidateC4_RL_NKTranshipDestPort();
			AssertNoMessageError(nonTranshipUnderbond.C4_RL_NKTranshipDestPortInfo, MandatoryValidation.YouHaveNotEntered + " a " + nonTranshipUnderbond.C4_RL_NKTranshipDestPortInfo.Description + ".");
		}

		public void TestC4_DestinationPremiseIDCannotBeEmpty()
		{
			Underbond.C4_DestinationPremiseID = ZString.Empty;
			AssertHasMessageError(Underbond.C4_DestinationPremiseIDInfo, MandatoryValidation.YouHaveNotEntered + " a " + Underbond.C4_DestinationPremiseIDInfo.Description + ".");
		}

		public void TestKeyMessagingFields()
		{
			string oldPremiseID = "62342";
			Underbond.C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.Road;
			Underbond.C4_DischargePremiseID = oldPremiseID;
			Underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.DeliveryToFinalDestination;
			Underbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			Factory.Save();
			Underbond.C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.Rail;
			Assert("Should have an error", Underbond.C4_ModeOfMovementInfo.GetErrors().Contains(CustomsValidation.ErrorString.Replace("@", CMRUnderbondModeOfMovement.Codes.Road)));
			Underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.OtherMovement;
			Assert("Should have an error", Underbond.C4_MovementReasonInfo.GetErrors().Contains(CustomsValidation.ErrorString.Replace("@", CMRUnderbondRequestCodes.Codes.DeliveryToFinalDestination)));
			Underbond.C4_DischargePremiseID = "23423";
			Assert("Should have an error", Underbond.C4_DischargePremiseIDInfo.GetErrors().Contains(CustomsValidation.ErrorString.Replace("@", oldPremiseID)));
			Underbond.C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.Road;
			Assert("Should have no errors", !Underbond.C4_ModeOfMovementInfo.HasErrors());
			Underbond.C4_DischargePremiseID = oldPremiseID;
			Assert("Should have no errors", !Underbond.C4_DischargePremiseIDInfo.HasErrors());
			Underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.DeliveryToFinalDestination;
			Assert("Should have no errors", !Underbond.C4_MovementReasonInfo.HasErrors());
		}

		public void TestErrorOnDischargePremiseIDWhenDoingOutturn()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			mAWB.CM_MAWB = "08163248562";
			hAWB.CS_HAWB = "62342";
			CusUnderbond underbond = mAWB.Underbonds.AddNew();
			underbond.C4_ParentID = mAWB.PK;
			underbond.C4_ParentTableCode = CusMAWBSchema.Constants.Prefix;
			underbond.C4_IsMoveFromDischarge = false;
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_Code = "CUCSQK";
			OrgAddress address = header.Addresses.AddNew();
			address.OA_Address1 = "Cuckoo Squeaker St";
			OrgCusCode cusCode = header.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = "6623N";
			cusCode.OK_OA_PremisesAddress = address.PK;
			address.LocalControlledPremisesID = "6623N";
			underbond.C4_OA_DischargeAddress = address.PK;
			underbond.C4_OriginPremiseID = "9914N";
			underbond.C4_DestinationPremiseID = "2290P";
			AssertEquals("6623N", underbond.C4_DischargePremiseID);
			Assert(!underbond.C4_DischargePremiseIDInfo.HasErrors());
			underbond.C4_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			underbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			Factory.Save();
			underbond.Validation.ValidateC4_DischargePremiseID();
			Assert(!underbond.C4_DischargePremiseIDInfo.HasErrors());
		}

		public void TestC4_DestinationPremiseIDInvalidCode()
		{
			Underbond.C4_DestinationPremiseID = "12C";
			AssertHasMessageErrors(Underbond.C4_DestinationPremiseIDInfo);
			Underbond.C4_DestinationPremiseID = "1234D";
			AssertNoMessageErrors(Underbond.C4_DestinationPremiseIDInfo);
		}

		public void TestC4_FlightNoValidation()
		{
			Underbond.C4_ParentID = ZGuid.Empty;
			Underbond.C4_FlightNo = "";
			Assert(Underbond.C4_FlightNoInfo.HasMessageErrors());
			CusMAWB mAWB = Factory.New<CusMAWB>();
			Underbond.C4_ParentID = mAWB.PK;
			Underbond.C4_FlightNo = "";
			Assert(!Underbond.C4_FlightNoInfo.HasMessageErrors());
			Underbond.C4_FlightNo = "QF123";
			Assert(!Underbond.C4_FlightNoInfo.HasMessageErrors());
		}

		public void TestC4_ArrivalDateValidation()
		{
			Underbond.C4_ParentID = ZGuid.Empty;
			Underbond.C4_ArrivalDate = ZDateTime.Empty;
			Assert(Underbond.C4_ArrivalDateInfo.HasMessageErrors());
			CusMAWB mAWB = Factory.New<CusMAWB>();
			Underbond.C4_ParentID = mAWB.PK;
			Underbond.C4_ArrivalDate = ZDateTime.Empty;
			Assert(!Underbond.C4_ArrivalDateInfo.HasMessageErrors());
			Underbond.C4_ArrivalDate = ZDateTime.Today;
			Assert(!Underbond.C4_ArrivalDateInfo.HasMessageErrors());
		}

		public void TestC4_UnderbondBySeaVessel()
		{
			const string vesselName = "Vessel001";
			AssertEquals("Precondition: There are no Vessels named " + vesselName, 0, RefVessel.LookupVesselByName(vesselName, Factory).Length);

			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_UnderbondBySeaVessel = vesselName;
			underbond.C4_UnderbondBySeaLloydsIMONum = "998877";
			AssertHasWarning(underbond.C4_UnderbondBySeaVesselInfo, "A Vessel with this Name and Lloyds Number cannot be found.");

			var vesselA = Factory.New<RefVessel>();
			vesselA.RV_Code = vesselName;
			vesselA.RV_LloydsNumber = "";
			underbond.Validation.ValidateC4_UnderbondBySeaVessel();
			AssertHasWarning(underbond.C4_UnderbondBySeaVesselInfo, "A Vessel with this Name and Lloyds Number cannot be found.");

			var vesselB = Factory.New<RefVessel>();
			vesselB.RV_Code = vesselName;
			vesselB.RV_LloydsNumber = "998877";
			underbond.Validation.ValidateC4_UnderbondBySeaVessel();
			AssertNoWarning(underbond.C4_UnderbondBySeaVesselInfo, "A Vessel with this Name and Lloyds Number cannot be found.");
		}

		public void TestUnderbondBySeaLloydsIMONum()
		{
			Underbond.C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.SeaInternationalVessel;
			Underbond.Validation.ValidateC4_UnderbondBySeaLloydsIMONum();
			AssertEquals("Underbond by sea vessel is necessary for IVS movement", true, Underbond.C4_UnderbondBySeaLloydsIMONumInfo.HasMessageErrors());

			Underbond.C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.SeaDomesticVessel;
			Underbond.Validation.ValidateC4_UnderbondBySeaLloydsIMONum();
			AssertEquals("Underbond by sea vessel is not necessary for DVS movement", false, Underbond.C4_UnderbondBySeaLloydsIMONumInfo.HasMessageErrors());
		}

		public void TestUnderbondBySeaVoyage()
		{
			Underbond.C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.SeaInternationalVessel;
			Underbond.C4_UnderbondBySeaVoyage = "";
			AssertEquals("Underbond by sea voyage is necessary for IVS movement", true, Underbond.C4_UnderbondBySeaVoyageInfo.HasMessageErrors());

			Underbond.C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.SeaDomesticVessel;
			Underbond.C4_UnderbondBySeaVoyage = "";
			AssertEquals("Underbond by sea voyage is not necessary for DVS movement", false, Underbond.C4_UnderbondBySeaVoyageInfo.HasMessageErrors());
		}

		public void TestResponsiblePartyIDValidation()
		{
			Underbond.C4_ResponsiblePartyID = "2352321";
			AssertEquals("Should be invalid", true, Underbond.C4_ResponsiblePartyIDInfo.HasMessageErrors());
			Underbond.C4_ResponsiblePartyID = "87003014042";
			AssertEquals("Should be valid", false, Underbond.C4_ResponsiblePartyIDInfo.HasMessageErrors());

			Underbond.AU_OutturnResponsiblePartyID = "2352321";
			AssertEquals("Should be invalid", true, Underbond.AU_OutturnResponsiblePartyIDInfo.HasMessageErrors());
			Underbond.AU_OutturnResponsiblePartyID = "87003014042";
			AssertEquals("Should be valid", false, Underbond.AU_OutturnResponsiblePartyIDInfo.HasMessageErrors());
		}

		public void TestC4_MAWBValidation()
		{
			Underbond.C4_ParentID = ZGuid.Empty;
			Underbond.C4_MAWB = "";
			Assert(Underbond.C4_MAWBInfo.HasErrors());
			Underbond.C4_MAWB = "08121451124";
			Assert(!Underbond.C4_MAWBInfo.HasErrors());
			Underbond.C4_MAWB = "1";
			Assert(Underbond.C4_MAWBInfo.HasMessageError(CusUnderbondValidation.InvalidMawbFormat));
			Underbond.C4_MAWB = "123456789012";
			// Customs Error Messages:
			//	The length of MASTAIRWAYBILLNO in LINE[1] is 12 characters which exceeds the maximum field length of 11 characters
			//	The length of MASTAIRWAYBILLNO in LINE[1] is 10 characters which is less than the minimum field length of 11 characters
			AssertHasMessageError("Mawb errors at Customs if less than minimum field length of 11 characters or greater than maximum field length of 11 characters! (i.e. must be 11 characters... :)).", Underbond.C4_MAWBInfo, CusUnderbondValidation.InvalidMawbFormat);
			Underbond.C4_MAWB = "081-21451124";
			AssertNoMessageError("Validation should take into consideration standard waybill formatting.", Underbond.C4_MAWBInfo, CusUnderbondValidation.InvalidMawbFormat);

			CusMAWB mAWB = Factory.New<CusMAWB>();
			Underbond.C4_ParentID = mAWB.PK;
			Underbond.C4_MAWB = "";
			Assert(!Underbond.C4_MAWBInfo.HasErrors());
		}

		public void TestModeOfMovement()
		{
			Underbond.C4_ModeOfMovement = ZString.Empty;
			AssertHasMessageErrors("when empty", Underbond.C4_ModeOfMovementInfo);

			Underbond.C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.SeaInternationalVessel;
			AssertNoNotifications("when valid", Underbond.C4_ModeOfMovementInfo);

			Underbond.C4_ModeOfMovement = "f00";
			AssertHasErrors("when invalid", Underbond.C4_ModeOfMovementInfo);
		}

		public void TestCheckC4_DateOfArrivalIntoDestinationPremiseDoesNotErrorForStandAlone()
		{
			Underbond.C4_ParentTableCode = CusSCADepotHouseSchema.Constants.Prefix;
			Underbond.C4_ParentID = ZGuid.NewZGuid();
			var outturn = Underbond.Outturns.AddNew();
			Underbond.C4_DateOfArrivalIntoDestinationPremise = ZDateTime.Empty;
			AssertEquals("Should have a message error", true, Underbond.C4_DateOfArrivalIntoDestinationPremiseInfo.HasMessageErrors());
			Underbond.C4_ParentID = ZGuid.Empty;
			Underbond.C4_DateOfArrivalIntoDestinationPremise = ZDateTime.Empty;
			AssertEquals("Should not have a message error", false, Underbond.C4_DateOfArrivalIntoDestinationPremiseInfo.HasMessageErrors());
		}

		public void TestC4_DateOfArrivalIntoDestinationPremiseValidation()
		{
			Underbond.C4_ParentID = ZGuid.NewZGuid();
			Underbond.C4_ParentTableCode = CusSCAContainerSchema.Constants.Prefix;
			AssertEquals("Pre-Condition - No errors", false, Underbond.C4_DateOfArrivalIntoDestinationPremiseInfo.HasMessageErrors());
			CusOutturn outturn = Underbond.Outturns.AddNew();
			Underbond.C4_DateOfArrivalIntoDestinationPremise = Underbond.C4_DateOfArrivalIntoDestinationPremise;
			AssertEquals("Should have a message error", true, Underbond.C4_DateOfArrivalIntoDestinationPremiseInfo.HasMessageErrors());
			CusUnderbond airUnderbond = Factory.New<CusUnderbond>();
			airUnderbond.C4_ParentTableCode = CusHAWBSchema.Constants.Prefix;
			AssertEquals("Should not have a message error", false, airUnderbond.C4_DateOfArrivalIntoDestinationPremiseInfo.HasMessageErrors());
			CusOutturn outturn2 = airUnderbond.Outturns.AddNew();
			airUnderbond.C4_DateOfArrivalIntoDestinationPremise = airUnderbond.C4_DateOfArrivalIntoDestinationPremise;
			AssertEquals("Should not have a message error", false, airUnderbond.C4_DateOfArrivalIntoDestinationPremiseInfo.HasMessageErrors());
		}

		public void TestMovementReason()
		{
			Underbond.C4_MovementReason = ZString.Empty;
			AssertHasMessageErrors("when empty", Underbond.C4_MovementReasonInfo);

			Underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.TimeUpSendToWarehouse;
			AssertNoNotifications("when valid", Underbond.C4_MovementReasonInfo);

			Underbond.C4_MovementReason = "b4r";
			AssertHasErrors("when invalid", Underbond.C4_MovementReasonInfo);
		}

		#region Implementation
		protected new CusUnderbond Underbond => (CusUnderbond)base.Underbond;

		protected override Customs.Business.CusUnderbond CreateNewUnderbond()
		{
			var mAWB = Factory.New<CusMAWB>();
			var result = CusUnderbond.New(Factory);
			result.LinkedObject = mAWB;
			return result;
		}

		#endregion
	}
}
