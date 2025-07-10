using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsHeaderValidation : EU.NCTS.Business.NctsHeaderPhase5Validation
	{
		public NctsHeaderValidation(NctsHeader parent) : base(parent)
		{
		}

		public new NctsHeader Parent => (NctsHeader)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateEventCancellationReason();
		}

		public void ValidateEventCancellationReason()
		{
			ValidateCalculatedProperty(Parent.EventCancellationReasonInfo);
		}

		protected virtual void CheckEventCancellationReason()
		{
		}

		protected virtual void CheckTransportMeans()
		{
		}

		protected override void CheckArrivalMrnFromUserCore()
		{
			base.CheckArrivalMrnFromUserCore();
			var mrnError = MRNFormatValidator.CheckMRNFormat(Parent.ArrivalMrnFromUser, Parent.ArrivalMrnFromUserInfo.BizObj.Factory, Res.GetString("6f1e6406-3def-4444-811e-67c383534713", "For NCTS, "));
			if (!mrnError.IsEmpty)
			{
				Parent.ArrivalMrnFromUserInfo.AddMessageError(mrnError);
			}
		}

		public void CheckArrivalDestinationTraderAPIRegistrationNumber()
		{
			if (Parent?.DestinationTrader?.Organisation != null)
			{
				var identificationNumber = Parent.DestinationTrader.Organisation.CustomsCodes.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, Core.Constants.CountryCodes.Germany);
				if (identificationNumber.IsEmpty)
				{
					var senderDetails = EORIHelper.GetSenderDetailsFromRegistry();
					if (senderDetails.Sender.EoriBranchSuffix.IsEmpty() || senderDetails.Sender.EoriNumber.IsEmpty() || senderDetails.Bin.IsEmpty)
					{
						Parent.DestinationTrader.OrganisationPKInfo.AddMessageError(Res.GetString("31A90FE1-01E6-49EC-ACFE-C861902CCC64",
							"[NR0057] If Destination Trader is not Message Sender then EORI-Number, Branch and Participant Identification Number of sending Company must be maintained in Registry Setting: Customs/Country or Region Specific/ATLAS."));
					}
				}
			}
		}

		internal virtual void CheckDestinationTraderContact(ZPropertyInfo propertyInfo, OrgContact contact)
		{
		}

		protected override void CheckExplanation()
		{
		}

		protected override void CheckHeaderUnloadingNotes()
		{
		}

		protected override void CheckBH_ExportFlag()
		{
			base.CheckBH_ExportFlag();
			if (Parent.IsArrivalMovement)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BH_ExportFlagInfo);
			}
		}

		protected override bool EitherDispatchCountryOnGoodsItemOrOnHeaderMustBeFilled => false;
	}
}
