using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusUnderbondValidation : Customs.Business.CusUnderbondValidation
	{
		public CusUnderbondValidation(CusUnderbond underbond)
			: base(underbond)
		{
		}

		CusUnderbond Underbond
		{
			get { return Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAU_OutturnResponsiblePartyID();
		}

		public void ValidateAU_OutturnResponsiblePartyID()
		{
			ValidateCalculatedProperty(Underbond.AU_OutturnResponsiblePartyIDInfo);
		}

		protected void CheckAU_OutturnResponsiblePartyID()
		{
			ValidateResponsiblePartyID(Parent.AU_OutturnResponsiblePartyID, Parent.AU_OutturnResponsiblePartyIDInfo);
		}

		protected override void CheckC4_RL_NKTranshipDestPort()
		{
			base.CheckC4_RL_NKTranshipDestPort();
			if (Parent.C4_MovementReason == CMRUnderbondRequestCodes.Codes.Transshipment && Parent.LinkedObject != null && Parent.LinkedObject.UsesTranshipmentPortOnUnderbond)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.C4_RL_NKTranshipDestPortInfo);
			}
		}

		protected override void CheckC4_DischargePremiseID()
		{
			base.CheckC4_DischargePremiseID();
			if (Parent.C4_OA_DischargeAddress.IsEmpty)
			{
				new CustomsValidation(Parent.C4_DischargePremiseIDInfo).ErrorOnKeyDataWithNoChildren(Underbond.UnderbondStatus.Code);
			}
		}

		protected override void CheckC4_DestinationPremiseID()
		{
			base.CheckC4_DestinationPremiseID();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.C4_DestinationPremiseIDInfo);
			new EstablishmentCodeValidation(Parent).ValidateEstablishmentCode(Parent.C4_DestinationPremiseIDInfo);
		}

		protected override void CheckC4_ResponsiblePartyID()
		{
			base.CheckC4_ResponsiblePartyID();
			ValidateResponsiblePartyID(Parent.C4_ResponsiblePartyID, Parent.C4_ResponsiblePartyIDInfo);
		}

		void ValidateResponsiblePartyID(ZString partyID, ZPropertyInfo targetInfo)
		{
			if (!partyID.IsEmpty && !ABNValidation.CheckValidABN(partyID))
			{
				targetInfo.AddMessageError("The Responsible Party ID is not a valid ABN");
			}
		}

		protected override void CheckC4_MAWB()
		{
			base.CheckC4_MAWB();
			if (Underbond.IsStandAloneUnderbond)
			{
				MandatoryValidation.CheckEntered(Parent.C4_MAWBInfo, "MAWB Number");
				if (!Parent.C4_MAWB.IsEmpty)
				{
					if (Parent.C4_MAWB.KeepAlphanumericCharacters().Length != 11)
					{
						Parent.C4_MAWBInfo.AddMessageError(InvalidMawbFormat);
					}
				}
			}

			if (Underbond.IsChangingUniqueIdentifier)
			{
				Parent.C4_MAWBInfo.AddError(WithdrawalResendRequired);
			}
		}
		internal const string InvalidMawbFormat = "MAWB Number must be 11 characters. (Format: x(11) or xxx-xxxxxxxx)";
		internal const string WithdrawalResendRequired = "You cannot change the MAWB Number after an Outturn report has been submitted, you must first withdraw the current Outturn, then change the MAWB & re-submit as an original.";

		protected override void CheckC4_OriginPremiseID()
		{
			base.CheckC4_OriginPremiseID();
			if (!Underbond.IsStandAloneUnderbond)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.C4_OriginPremiseIDInfo);
			}
			new EstablishmentCodeValidation(Parent).ValidateEstablishmentCode(Parent.C4_OriginPremiseIDInfo);
		}

		protected override void CheckC4_ArrivalDate()
		{
			base.CheckC4_ArrivalDate();
			if (Underbond.IsStandAloneUnderbond)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.C4_ArrivalDateInfo);
			}

			if (Underbond.IsChangingCustomsIdentifier)
			{
				Parent.C4_ArrivalDateInfo.AddError(CustomsIdentifiersChanged);
			}
		}
		internal const string CustomsIdentifiersChanged = "You cannot change the Flight Number, Arrival Date or Date of Outturn after the Outturn report has been submitted, you must first withdraw the current Outturn, then change the relevant field required & re-submit the Outturn as an original.";

		protected override void CheckC4_FlightNo()
		{
			base.CheckC4_FlightNo();
			if (Underbond.IsStandAloneUnderbond)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.C4_FlightNoInfo);
			}

			if (Underbond.IsChangingCustomsIdentifier)
			{
				Parent.C4_FlightNoInfo.AddError(CustomsIdentifiersChanged);
			}
		}

		protected override void CheckC4_Outurned()
		{
			base.CheckC4_Outurned();

			if (Underbond.IsChangingCustomsIdentifier)
			{
				Parent.C4_OuturnedInfo.AddError(CustomsIdentifiersChanged);
			}
		}

		protected override void CheckC4_DateOfArrivalIntoDestinationPremise()
		{
			base.CheckC4_DateOfArrivalIntoDestinationPremise();

			if (Underbond.C4_DateOfArrivalIntoDestinationPremise.IsEmpty && Underbond.Outturns.Count > 0 && !Underbond.IsAirCargo && !Underbond.C4_ParentID.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Underbond.C4_DateOfArrivalIntoDestinationPremiseInfo);
			}

			if (Underbond.IsChangingCustomsIdentifier)
			{
				Parent.C4_DateOfArrivalIntoDestinationPremiseInfo.AddError(CustomsIdentifiersChanged);
			}
		}

		protected override void CheckC4_UnderbondBySeaVessel()
		{
			base.CheckC4_UnderbondBySeaVessel();

			if (!RefVessel.LookupVesselsByNameAndLloyds(Parent.C4_UnderbondBySeaVessel, Parent.C4_UnderbondBySeaLloydsIMONum, Parent.Factory).Any())
			{
				Parent.C4_UnderbondBySeaVesselInfo.AddWarning("A Vessel with this Name and Lloyds Number cannot be found.");
			}
		}

		protected override void CheckC4_UnderbondBySeaLloydsIMONum()
		{
			base.CheckC4_UnderbondBySeaLloydsIMONum();
			if (Parent.C4_ModeOfMovement == CMRUnderbondModeOfMovement.Codes.SeaInternationalVessel && Parent.C4_UnderbondBySeaLloydsIMONum.IsEmpty)
			{
				Parent.C4_UnderbondBySeaLloydsIMONumInfo.AddMessageError("By IVS, you should enter a valid vessel.");
			}
		}

		protected override void CheckC4_UnderbondBySeaVoyage()
		{
			base.CheckC4_UnderbondBySeaVoyage();
			if (Parent.C4_ModeOfMovement == CMRUnderbondModeOfMovement.Codes.SeaInternationalVessel && Parent.C4_UnderbondBySeaVoyage.IsEmpty)
			{
				Parent.C4_UnderbondBySeaVoyageInfo.AddMessageError("By IVS, you should enter a valid voyage number.");
			}
		}

		protected override void CheckC4_ModeOfMovement()
		{
			base.CheckC4_ModeOfMovement();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.C4_ModeOfMovementInfo);
			ListValidation.ErrorIfInvalidCode(Parent.C4_ModeOfMovementInfo, Parent.Lookups.ModeOfTransportList);
			new CustomsValidation(Parent.C4_ModeOfMovementInfo).ErrorOnKeyDataWithNoChildren(Underbond.UnderbondStatus.Code);
		}

		protected override void CheckC4_MovementReason()
		{
			base.CheckC4_MovementReason();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.C4_MovementReasonInfo);
			ListValidation.ErrorIfInvalidCode(Parent.C4_MovementReasonInfo, Parent.Lookups.RequestReasonList);
			new CustomsValidation(Parent.C4_MovementReasonInfo).ErrorOnKeyDataWithNoChildren(Underbond.UnderbondStatus.Code);
		}

		#region Implementation

		protected new CusUnderbond Parent { get { return (CusUnderbond)base.Parent; } }

		#endregion
	}
}
