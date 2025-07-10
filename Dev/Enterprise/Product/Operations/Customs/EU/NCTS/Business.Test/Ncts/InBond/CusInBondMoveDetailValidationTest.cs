using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class CusInBondMoveDetailValidationTest : BusinessObjectLookupsTestCase
	{
		public void TestTransportAtDepartureIDNationality_List()
		{
			var moveDetail = GetPhase5DIFROADCusInBondMoveDetail();
			ValidationTestHelper.AssertErrorIfInvalidCode(moveDetail.B9_RN_NKTransportAtDepartureIDNationalityInfo, "XX", "BE");
		}

		public void TestTransportAtDepartureIDNationality_ListNotEnabled()
		{
			var moveDetail = GetPhase5DIFROADCusInBondMoveDetail();
			moveDetail.MoveHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			moveDetail.B9_RN_NKTransportAtDepartureIDNationality = "XX";
			AssertNoErrors(moveDetail.B9_RN_NKTransportAtDepartureIDNationalityInfo);
		}

		public void TestTransportAtDepartureIDNationality_MandatoryWhenTransportID()
		{
			var moveDetail = GetPhase5DIFROADCusInBondMoveDetail();
			ValidationTestHelper.AssertErrorIfNotEnteredWhenOtherPropertyIsEntered(moveDetail.B9_RN_NKTransportAtDepartureIDNationalityInfo, moveDetail.B9_TransportAtDepartureIDInfo);
		}

		public void TestTransportAtDepartureIDNationality_MandatoryWhenAircraftID()
		{
			var moveDetail = GetPhase5DIFROADCusInBondMoveDetail();
			ValidationTestHelper.AssertErrorIfNotEnteredWhenOtherPropertyIsEntered(moveDetail.B9_RN_NKTransportAtDepartureIDNationalityInfo, moveDetail.B9_AircraftIDAtDepartureInfo);
		}

		public void TestTransportAtDepartureIDNationality_NotMandatoryWhenNotEnabled()
		{
			var moveDetail = GetPhase5DIFROADCusInBondMoveDetail();
			moveDetail.MoveHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			moveDetail.B9_AircraftIDAtDeparture = "air";
			moveDetail.B9_TransportAtDepartureID = "transport";
			AssertNoErrors(moveDetail.B9_RN_NKTransportAtDepartureIDNationalityInfo);
		}

		public void TestTransportAtDepartureTrailer1Nationality_List()
		{
			var moveDetail = GetPhase5DIFROADCusInBondMoveDetail();
			ValidationTestHelper.AssertErrorIfInvalidCode(moveDetail.B9_RN_NKTransportAtDepartureTrailer1NationalityInfo, "XX", "BE");
		}

		public void TestTransportAtDepartureTrailer1Nationality_ListWhenNotEnabled()
		{
			var moveDetail = GetPhase5DIFROADCusInBondMoveDetail();
			moveDetail.MoveHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			moveDetail.B9_RN_NKTransportAtDepartureTrailer1Nationality = "XX";
			AssertNoErrors(moveDetail.B9_RN_NKTransportAtDepartureTrailer1NationalityInfo);
		}

		public void TestTransportAtDepartureTrailer2Nationality_List()
		{
			var moveDetail = GetPhase5DIFROADCusInBondMoveDetail();
			ValidationTestHelper.AssertErrorIfInvalidCode(moveDetail.B9_RN_NKTransportAtDepartureTrailer2NationalityInfo, "XX", "BE");
		}

		public void TestTransportAtDepartureTrailer2Nationality_ListWhenNotEnabled()
		{
			var moveDetail = GetPhase5DIFROADCusInBondMoveDetail();
			moveDetail.MoveHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			moveDetail.B9_RN_NKTransportAtDepartureTrailer1Nationality = "XX";
			AssertNoErrors(moveDetail.B9_RN_NKTransportAtDepartureTrailer2NationalityInfo);
		}

		public void TestTransportAtDepartureTrailer1Nationality_Mandatory()
		{
			var moveDetail = GetPhase5DIFROADCusInBondMoveDetail();
			ValidationTestHelper.AssertErrorIfNotEnteredWhenOtherPropertyIsEntered(moveDetail.B9_RN_NKTransportAtDepartureTrailer1NationalityInfo, moveDetail.B9_TransportAtDepartureTrailer1RegNoInfo);
		}

		public void TestTransportAtDepartureTrailer1Nationality_NotMandatoryWhenNotEnabled()
		{
			var moveDetail = GetPhase5DIFROADCusInBondMoveDetail();
			moveDetail.MoveHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			moveDetail.B9_TransportAtDepartureTrailer1RegNo = "regno";
			AssertNoErrors(moveDetail.B9_RN_NKTransportAtDepartureTrailer1NationalityInfo);
		}

		public void TestTransportAtDepartureTrailer2Nationality_Mandatory()
		{
			var moveDetail = GetPhase5DIFROADCusInBondMoveDetail();
			ValidationTestHelper.AssertErrorIfNotEnteredWhenOtherPropertyIsEntered(moveDetail.B9_RN_NKTransportAtDepartureTrailer2NationalityInfo, moveDetail.B9_TransportAtDepartureTrailer2RegNoInfo);
		}

		public void TestTransportAtDepartureTrailer2Nationality_NotMandatoryWhenNotEnabled()
		{
			var moveDetail = GetPhase5DIFROADCusInBondMoveDetail();
			moveDetail.MoveHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			moveDetail.B9_TransportAtDepartureTrailer1RegNo = "regno";
			AssertNoErrors(moveDetail.B9_RN_NKTransportAtDepartureTrailer2NationalityInfo);
		}

		CusInBondMoveDetail GetPhase5DIFROADCusInBondMoveDetail()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var moveHeader = nctsHeader.ArrivalMovementHeader;
			var nctsBill = nctsHeader.Bills.AddNew();
			var moveDetail = nctsBill.MovementDetail;
			moveHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			moveDetail.B9_UnloadedState = "DIF";
			return moveDetail;
		}

		public void TestUnloadedStateValidation_Phase4()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var cusInBondMoveDetail = header.ArrivalMovementHeader.MovementDetails.AddNew();

			CombineAssertions(() =>
			{
				cusInBondMoveDetail.B9_UnloadedState = "XXX";
				AssertHasMessageErrorContaining("XXX should have a message error", cusInBondMoveDetail.B9_UnloadedStateInfo, ListValidation.InvalidCodeMessageError.ToString());

				cusInBondMoveDetail.B9_UnloadedState = "DEC";
				AssertNoNotifications("DEC should not have a message error", cusInBondMoveDetail.B9_UnloadedStateInfo);
			});
		}

		public void TestUnloadedStateValidation_Phase5Arrival_DEC()
		{
			var bill = moveDetail.Bill;
			var unloadedStateInfo = moveDetail.B9_UnloadedStateInfo;
			var message = $"The house consignment {moveDetail.B9_SeqNo} has an unloaded state DEC, while there were changes in the related documents, transport means or goods items.\nThese changes will not be sent to customs unless you change the unloaded state to DIF.";

			CombineAssertions(() =>
			{
				moveDetail.B9_UnloadedState = "DEC";
				AssertNoNotifications("State DEC without details", unloadedStateInfo);

				var supportingDoc = bill.SupportingDocuments.AddNew();
				supportingDoc.CSI_Status = "NEW";
				moveDetail.B9_UnloadedState = "DEC";
				AssertHasMessageError("Supporting Doc not state DEC", unloadedStateInfo, message);

				supportingDoc.CSI_Status = "DEC";
				moveDetail.B9_UnloadedState = "DEC";
				AssertNoMessageError("Supporting Doc state DEC", unloadedStateInfo, message);

				var additionalDoc = bill.AdditionalDocuments.AddNew();
				additionalDoc.CSI_Status = "NEW";
				moveDetail.B9_UnloadedState = "DEC";
				AssertHasMessageError("Additional Doc not state DEC", unloadedStateInfo, message);

				additionalDoc.CSI_Status = "DEC";
				moveDetail.B9_UnloadedState = "DEC";
				AssertNoMessageError("Additional Doc state DEC", unloadedStateInfo, message);

				var previousDoc = bill.PreviousDocuments.AddNew();
				previousDoc.CSI_Status = "NEW";
				moveDetail.B9_UnloadedState = "DEC";
				AssertHasMessageError("Previous Doc not state DEC", unloadedStateInfo, message);

				previousDoc.CSI_Status = "DEC";
				moveDetail.B9_UnloadedState = "DEC";
				AssertNoMessageError("Previous Doc state DEC", unloadedStateInfo, message);

				var arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
				arrivalCargoDesc.BY_UnloadedState = "NEW";
				moveDetail.B9_UnloadedState = "DEC";
				AssertHasMessageError("Good not state DEC", unloadedStateInfo, message);

				arrivalCargoDesc.BY_UnloadedState = "DEC";
				moveDetail.B9_UnloadedState = "DEC";
				AssertNoMessageError("Good state DEC", unloadedStateInfo, message);

				var transportMeans = bill.ArrivalTransportInfos.AddNew();
				transportMeans.TPM_TransportState = "NEW";
				moveDetail.B9_UnloadedState = "DEC";
				AssertHasMessageError("Transport Means state DEC", unloadedStateInfo, message);

				transportMeans.TPM_TransportState = "DEC";
				moveDetail.B9_UnloadedState = "DEC";
				AssertNoMessageError("Transport Means state DEC", unloadedStateInfo, message);
			});
		}

		public void TestUnloadedStateValidation_Phase5Arrival_DIF()
		{
			var bill = moveDetail.Bill;
			var unloadedStateInfo = moveDetail.B9_UnloadedStateInfo;
			var message = $"The house consignment {moveDetail.B9_SeqNo} has an unloaded state DIF, while there are no changes in gross mass, related documents, transport means or goods items.\nThis might potentially give an error by customs. Please change the status to DEC.";

			CombineAssertions(() =>
			{
				moveDetail.B9_UnloadedState = "DIF";
				AssertHasMessageError("State DIF without details", unloadedStateInfo, message);

				var supportingDoc = bill.SupportingDocuments.AddNew();
				supportingDoc.CSI_Status = "NEW";
				moveDetail.B9_UnloadedState = "DIF";
				AssertNoMessageError("Supporting Doc state not DEC", unloadedStateInfo, message);

				var additionalDoc = bill.AdditionalDocuments.AddNew();
				additionalDoc.CSI_Status = "NEW";
				moveDetail.B9_UnloadedState = "DIF";
				AssertNoMessageError("Additional Doc state not DEC", unloadedStateInfo, message);

				var previousDoc = bill.PreviousDocuments.AddNew();
				previousDoc.CSI_Status = "NEW";
				moveDetail.B9_UnloadedState = "DIF";
				AssertNoMessageError("Previous Doc state not DEC", unloadedStateInfo, message);

				var arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
				supportingDoc.CSI_Status = "DEC";
				additionalDoc.CSI_Status = "DEC";
				previousDoc.CSI_Status = "DEC";
				arrivalCargoDesc.BY_UnloadedState = "NEW";
				moveDetail.B9_UnloadedState = "DIF";
				AssertNoMessageError("Supporting Doc state DEC, Good state not DEC", unloadedStateInfo, message);

				var transportMeans = bill.ArrivalTransportInfos.AddNew();
				arrivalCargoDesc.BY_UnloadedState = "DEC";
				transportMeans.TPM_TransportState = "NEW";
				moveDetail.B9_UnloadedState = "DIF";
				AssertNoMessageError("Supporting Doc state DEC, Good state DEC, Transport Means state not DEC", unloadedStateInfo, message);

				transportMeans.TPM_TransportState = "DEC";
				moveDetail.B9_UnloadedState = "DIF";
				AssertHasMessageError("Supporting Doc/ Good & Transport Means have state DEC", unloadedStateInfo, message);

				var transportMeans2 = bill.ArrivalTransportInfos.AddNew();
				transportMeans2.TPM_TransportState = "NEW";
				moveDetail.B9_UnloadedState = "DIF";
				AssertNoMessageError("Supporting Doc state DEC, Good state DEC, Second Transport Means state not DEC", unloadedStateInfo, message);

				transportMeans.TPM_TransportState = "DEC";
				transportMeans2.TPM_TransportState = "DEC";
				bill.B0_Weight = 2;
				arrivalCargoDesc.BY_UnloadedState = "DEC";
				arrivalCargoDesc.BY_GrossWeight = 2;
				arrivalCargoDesc.BY_GrossWeightUnit = "KG";
				Factory.Save();

				moveDetail.B9_UnloadedState = "DIF";
				AssertEquals("DifferenceWeight when is equal ", new ZDecimal(2), moveDetail.DifferenceMoveDetail.DifferenceWeight);
				AssertHasMessageError("When no difference with the Total gross mass and gross mass Unoaded", unloadedStateInfo, message);

				arrivalCargoDesc.BY_GrossWeight = 1;
				moveDetail.B9_UnloadedState = "DIF";
				AssertEquals("DifferenceWeight when is different ", new ZDecimal(1), moveDetail.DifferenceMoveDetail.DifferenceWeight);
				AssertNoMessageError("When only difference is that the Total gross mass has changed", unloadedStateInfo, message);
			});
		}

		public void TestUnloadedStateValidation_Phase5Arrival_MIS()
		{
			var bill = moveDetail.Bill;
			var unloadedStateInfo = moveDetail.B9_UnloadedStateInfo;
			var message = $"The house consignment {moveDetail.B9_SeqNo} has an unloaded state MIS, while there are changes (NEW) in the related documents, transport means or (NEW or DIF) goods items.\nThese changes will not be sent to customs because the entire house will be sent as “Missing”.\nPlease change the unloaded state of the house consignment to DIF or delete the newly created goods items, transport means or documents or undo the differences made.";

			CombineAssertions(() =>
			{
				moveDetail.B9_UnloadedState = "MIS";
				AssertNoMessageError("State MIS without details", unloadedStateInfo, message);

				var supportingDoc = bill.SupportingDocuments.AddNew();
				supportingDoc.CSI_Status = "NEW";
				moveDetail.B9_UnloadedState = "MIS";
				AssertHasMessageError("Supporting Doc state NEW", unloadedStateInfo, message);

				supportingDoc.CSI_Status = "DEC";
				moveDetail.B9_UnloadedState = "MIS";
				AssertNoMessageError("Supporting Doc not state NEW", unloadedStateInfo, message);

				var additionalDoc = bill.AdditionalDocuments.AddNew();
				additionalDoc.CSI_Status = "NEW";
				moveDetail.B9_UnloadedState = "MIS";
				AssertHasMessageError("Additional Doc state NEW", unloadedStateInfo, message);

				additionalDoc.CSI_Status = "DEC";
				moveDetail.B9_UnloadedState = "MIS";
				AssertNoMessageError("Additional Doc not state NEW", unloadedStateInfo, message);

				var previousDoc = bill.PreviousDocuments.AddNew();
				previousDoc.CSI_Status = "NEW";
				moveDetail.B9_UnloadedState = "MIS";
				AssertHasMessageError("Previous Doc state NEW", unloadedStateInfo, message);

				previousDoc.CSI_Status = "DEC";
				moveDetail.B9_UnloadedState = "MIS";
				AssertNoMessageError("Previous Doc not state NEW", unloadedStateInfo, message);

				var arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
				arrivalCargoDesc.BY_UnloadedState = "NEW";
				moveDetail.B9_UnloadedState = "MIS";
				AssertHasMessageError("Good state NEW", unloadedStateInfo, message);

				arrivalCargoDesc.BY_UnloadedState = "DIF";
				moveDetail.B9_UnloadedState = "MIS";
				AssertHasMessageError("Good state DIF", unloadedStateInfo, message);

				arrivalCargoDesc.BY_UnloadedState = "DEC";
				moveDetail.B9_UnloadedState = "MIS";
				AssertNoMessageError("Good state not NEW or DIF", unloadedStateInfo, message);

				var transportMeans = bill.ArrivalTransportInfos.AddNew();
				transportMeans.TPM_TransportState = "NEW";
				moveDetail.B9_UnloadedState = "MIS";
				AssertHasMessageError("Transport Means state NEW", unloadedStateInfo, message);

				transportMeans.TPM_TransportState = "DEC";
				moveDetail.B9_UnloadedState = "MIS";
				AssertNoMessageError("Transport Means not state NEW", unloadedStateInfo, message);
			});
		}

		public void TestUnloadedStateValidation_Phase5Arrival()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var cusInBondMoveDetail = nctsHeader.ArrivalMovementHeader.MovementDetails.AddNew();

			CombineAssertions(() =>
			{
				cusInBondMoveDetail.B9_UnloadedState = ZString.Empty;
				AssertHasErrorContaining("Empty value should have an error", cusInBondMoveDetail.B9_UnloadedStateInfo, MandatoryValidation.MustBeEntered);

				cusInBondMoveDetail.B9_UnloadedState = "XXX";
				AssertHasErrorContaining("XXX should have an error", cusInBondMoveDetail.B9_UnloadedStateInfo, ListValidation.InvalidCodeError);

				cusInBondMoveDetail.B9_UnloadedState = "DEC";
				AssertNoNotifications("DEC should not have an error", cusInBondMoveDetail.B9_UnloadedStateInfo);

				cusInBondMoveDetail.B9_UnloadedState = "NEW";
				AssertNoNotifications("NEW should not have an error", cusInBondMoveDetail.B9_UnloadedStateInfo);
			});
		}

		public void TestCheckB9_B0_NotMandatory()
		{
			const string message = "Bill number cannot be empty.";
			var cusInBondMoveDetail = Factory.New<CusInBondMoveDetail>();
			cusInBondMoveDetail.B9_B0 = ZGuid.Empty;
			AssertEquals(false, cusInBondMoveDetail.B9_B0Info.HasError(message));
		}

		public void TestCheckB9_TransportAtDepartureID_MaxLength()
		{
			CombineAssertions(() =>
			{
				UniversalValidationHelperTest.AssertMaxLengthDuringTransitionPeriod(moveDetail.B9_TransportAtDepartureIDInfo, 27);
			});
		}

		public void TestCheckB9_TransportAtDepartureTrailer1RegNo_MaxLength()
		{
			CombineAssertions(() =>
			{
				UniversalValidationHelperTest.AssertMaxLengthDuringTransitionPeriod(moveDetail.B9_TransportAtDepartureTrailer1RegNoInfo, 27);
			});
		}

		public void TestCheckB9_TransportAtDepartureTrailer2RegNo_MaxLength()
		{
			CombineAssertions(() =>
			{
				UniversalValidationHelperTest.AssertMaxLengthDuringTransitionPeriod(moveDetail.B9_TransportAtDepartureTrailer2RegNoInfo, 27);
			});
		}

		public void TestCheckB9_AircraftIDatDeparture_MaxLength()
		{
			CombineAssertions(() =>
			{
				UniversalValidationHelperTest.AssertMaxLengthDuringTransitionPeriod(moveDetail.B9_AircraftIDAtDepartureInfo, 27);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			moveDetail = header.Bills.AddNew().MovementDetail;
		}
		CusInBondMoveDetail moveDetail;
	}
}
