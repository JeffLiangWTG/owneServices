using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusInBondMoveDetailValidation : Customs.Business.CusInBondMoveDetailValidation
	{
		public CusInBondMoveDetailValidation(CusInBondMoveDetail parent)
			: base(parent)
		{
		}

		protected new CusInBondMoveDetail Parent => (CusInBondMoveDetail)base.Parent;

		protected override void CheckB9_UnloadedState()
		{
			base.CheckB9_UnloadedState();

			var parent = Parent;
			var unloadedStateInfo = parent.B9_UnloadedStateInfo;

			if (parent.IsPhase5)
			{
				MandatoryValidation.CheckEntered(unloadedStateInfo);
				var unloadedState = parent.B9_UnloadedState;
				if (unloadedState != NctsUnloadedStateList.Codes.NEW || unloadedStateInfo.OriginalValue.ToString() != NctsUnloadedStateList.Codes.NEW)
				{
					ListValidation.ErrorIfInvalidCode(unloadedStateInfo);
				}

				if (parent.Bill is NctsBill bill)
				{
					switch (unloadedState)
					{
						case NctsUnloadedStateList.Codes.DEC:
							if (bill.SupportingDocuments.Any(x => x.CSI_Status != NctsUnloadedStateList.Codes.DEC) ||
								bill.AdditionalDocuments.Cast<NctsBillAdditionalDocument>().Any(x => x.CSI_Status != NctsUnloadedStateList.Codes.DEC) ||
								bill.PreviousDocuments.Cast<CommonPreviousDocument>().Any(x => x.CSI_Status != NctsUnloadedStateList.Codes.DEC) ||
								bill.ArrivalGoodsItems.Cast<NctsArrivalCargoDesc>().Any(x => x.BY_UnloadedState != NctsUnloadedStateList.Codes.DEC) ||
								bill.ArrivalTransportInfos.Cast<ArrivalCusTransportMeans>().Any(x => x.TPM_TransportState != NctsUnloadedStateList.Codes.DEC))
							{
								unloadedStateInfo.AddMessageError(Res.GetString("AA6C7626-FFE4-4754-9E9E-3A8B75BA118B", "The house consignment {0} has an unloaded state DEC, while there were changes in the related documents, transport means or goods items.\nThese changes will not be sent to customs unless you change the unloaded state to DIF.", parent.B9_SeqNo));
							}
							break;
						case NctsUnloadedStateList.Codes.DIF:
							var differenceWeight = parent.DifferenceMoveDetail?.DifferenceWeight ?? 0;
							if (!bill.SupportingDocuments.Any(x => x.CSI_Status != NctsUnloadedStateList.Codes.DEC) &&
								!bill.AdditionalDocuments.Cast<NctsBillAdditionalDocument>().Any(x => x.CSI_Status != NctsUnloadedStateList.Codes.DEC) &&
								!bill.PreviousDocuments.Cast<CommonPreviousDocument>().Any(x => x.CSI_Status != NctsUnloadedStateList.Codes.DEC) &&
								!bill.ArrivalGoodsItems.Cast<NctsArrivalCargoDesc>().Any(x => x.BY_UnloadedState != NctsUnloadedStateList.Codes.DEC) &&
								!bill.ArrivalTransportInfos.Cast<ArrivalCusTransportMeans>().Any(x => x.TPM_TransportState != NctsUnloadedStateList.Codes.DEC) &&
								(differenceWeight == bill.B0_Weight))
							{
								unloadedStateInfo.AddMessageError(Res.GetString("6E1B7DEE-2306-44F8-9B3E-523EF5B1AE3B", "The house consignment {0} has an unloaded state DIF, while there are no changes in gross mass, related documents, transport means or goods items.\nThis might potentially give an error by customs. Please change the status to DEC.", parent.B9_SeqNo));
							}
							break;
						case NctsUnloadedStateList.Codes.MIS:
							if (bill.SupportingDocuments.Any(x => x.CSI_Status == NctsUnloadedStateList.Codes.NEW) ||
								bill.AdditionalDocuments.Cast<NctsBillAdditionalDocument>().Any(x => x.CSI_Status == NctsUnloadedStateList.Codes.NEW) ||
								bill.PreviousDocuments.Cast<CommonPreviousDocument>().Any(x => x.CSI_Status == NctsUnloadedStateList.Codes.NEW) ||
								bill.ArrivalGoodsItems.Cast<NctsArrivalCargoDesc>().Any(x => x.BY_UnloadedState == NctsUnloadedStateList.Codes.NEW || x.BY_UnloadedState == NctsUnloadedStateList.Codes.DIF) ||
								bill.ArrivalTransportInfos.Cast<ArrivalCusTransportMeans>().Any(x => x.TPM_TransportState == NctsUnloadedStateList.Codes.NEW))
							{
								unloadedStateInfo.AddMessageError(Res.GetString("1B8A108D-A11C-46ED-972B-47E3CCE4E923", "The house consignment {0} has an unloaded state MIS, while there are changes (NEW) in the related documents, transport means or (NEW or DIF) goods items.\nThese changes will not be sent to customs because the entire house will be sent as “Missing”.\nPlease change the unloaded state of the house consignment to DIF or delete the newly created goods items, transport means or documents or undo the differences made.", parent.B9_SeqNo));
							}
							break;
					}
				}
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(unloadedStateInfo);
			}
		}

		protected override void CheckB9_RN_NKTransportAtDepartureIDNationality()
		{
			base.CheckB9_RN_NKTransportAtDepartureIDNationality();

			if (Parent.IsPhase5 && Parent.IsRoad)
			{
				ListValidation.ErrorIfInvalidCode(Parent.B9_RN_NKTransportAtDepartureIDNationalityInfo);
				MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.B9_RN_NKTransportAtDepartureIDNationalityInfo, Parent.B9_TransportAtDepartureIDInfo);
				MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.B9_RN_NKTransportAtDepartureIDNationalityInfo, Parent.B9_AircraftIDAtDepartureInfo);
			}
		}

		protected override void CheckB9_RN_NKTransportAtDepartureTrailer1Nationality()
		{
			base.CheckB9_RN_NKTransportAtDepartureTrailer1Nationality();

			if (Parent.IsPhase5 && Parent.IsRoad)
			{
				ListValidation.ErrorIfInvalidCode(Parent.B9_RN_NKTransportAtDepartureTrailer1NationalityInfo);

				MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.B9_RN_NKTransportAtDepartureTrailer1NationalityInfo, Parent.B9_TransportAtDepartureTrailer1RegNoInfo);
			}
		}

		protected override void CheckB9_RN_NKTransportAtDepartureTrailer2Nationality()
		{
			base.CheckB9_RN_NKTransportAtDepartureTrailer2Nationality();

			if (Parent.IsPhase5 && Parent.IsRoad)
			{
				ListValidation.ErrorIfInvalidCode(Parent.B9_RN_NKTransportAtDepartureTrailer2NationalityInfo);

				MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.B9_RN_NKTransportAtDepartureTrailer2NationalityInfo, Parent.B9_TransportAtDepartureTrailer2RegNoInfo);
			}
		}

		protected override void CheckB9_AircraftIDAtDeparture()
		{
			base.CheckB9_AircraftIDAtDeparture();
			var parent = Parent;
			UniversalValidationHelper.CheckMaxLengthIfPhase5TransitionPeriod(parent.IsInPhase5TransitionPeriod, parent.B9_AircraftIDAtDepartureInfo, 27);
		}

		protected override void CheckB9_TransportAtDepartureID()
		{
			base.CheckB9_TransportAtDepartureID();
			var parent = Parent;
			UniversalValidationHelper.CheckMaxLengthIfPhase5TransitionPeriod(parent.IsInPhase5TransitionPeriod, parent.B9_TransportAtDepartureIDInfo, 27);
		}

		protected override void CheckB9_TransportAtDepartureTrailer1RegNo()
		{
			base.CheckB9_TransportAtDepartureTrailer1RegNo();
			var parent = Parent;
			UniversalValidationHelper.CheckMaxLengthIfPhase5TransitionPeriod(parent.IsInPhase5TransitionPeriod, parent.B9_TransportAtDepartureTrailer1RegNoInfo, 27);
		}

		protected override void CheckB9_TransportAtDepartureTrailer2RegNo()
		{
			base.CheckB9_TransportAtDepartureTrailer2RegNo();
			var parent = Parent;
			UniversalValidationHelper.CheckMaxLengthIfPhase5TransitionPeriod(parent.IsInPhase5TransitionPeriod, parent.B9_TransportAtDepartureTrailer2RegNoInfo, 27);
		}

		protected override bool IsBillNumberMandatory => false;
	}
}
