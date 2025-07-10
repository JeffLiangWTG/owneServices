using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.EU.Manifest.Business
{
	public class ICSAsycudaBillValidationForRegularBill : AsycudaBillValidationForRegularBill
	{
		public ICSAsycudaBillValidationForRegularBill(AsycudaBill parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateSpecialMentions();
		}

		public const string NegotiableBOL = "10600";

		protected override void CheckABL_OA_Consignee()
		{
			var header = Parent.Header;
			if (header != null && header.SpecialMentions != NegotiableBOL && header.Consol == null)
			{
				if (Parent.ABL_OA_Consignee.IsEmpty)
				{
					Parent.ABL_OA_ConsigneeInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Parent.ABL_OA_ConsigneeInfo.HumanReadableName));
				}
				else if (header.SpecificCircumstanceIndicator == SpecificCircumstanceList.Codes.E
					&& (!ValidationHelper.HasEORIOrTCUCustomsCode(Parent.Consignee?.Header, header.AMA_RN_NKCountry)))
				{
					Parent.ABL_OA_ConsigneeInfo.AddMessageError(Res.GetString("669C9F1E-13CF-4177-A2D6-3738EBB5FD6C", "An EORI or TCUIN number must be set up against the organization"));
				}
			}
		}

		protected override void CheckABL_OA_Shipper()
		{
			var header = Parent.Header;
			if (header != null && header.Consol == null)
			{
				if (Parent.ABL_OA_Shipper.IsEmpty)
				{
					Parent.ABL_OA_ShipperInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Parent.ABL_OA_ShipperInfo.HumanReadableName));
				}
				else if (!ValidationHelper.HasEORIOrTCUCustomsCode(Parent.Shipper?.Header, header.AMA_RN_NKCountry))
				{
					if (header.SpecificCircumstanceIndicator == SpecificCircumstanceList.Codes.E)
					{
						Parent.ABL_OA_ShipperInfo.AddMessageError(Res.GetString("53048B53-1AEA-4CF0-8BF7-661854971FB9", "Organization must have a valid EORI Trader Identification number or a Third Country Unique Identification Number(TCUIN)"));
					}
					else
					{
						Parent.ABL_OA_ShipperInfo.AddWarning(Res.GetString("0CFA6030-E970-4156-964E-A457984AA526", "If the organization has a valid EORI Trader Identification number or a Third Country Unique Identification Number (TCUIN) which has been made available to the Union by the third country concerned, then the EORI or TCUIN must be supplied"));
					}
				}
			}
		}

		protected override void CheckABL_OA_NotifyParty()
		{
			var header = Parent.Header;
			if (header != null && header.SpecialMentions == NegotiableBOL && header.Consol == null)
			{
				if (Parent.ABL_OA_NotifyParty.IsEmpty)
				{
					Parent.ABL_OA_NotifyPartyInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Parent.ABL_OA_NotifyPartyInfo.HumanReadableName));
				}
				else if (header.SpecificCircumstanceIndicator == SpecificCircumstanceList.Codes.E
					&& (!ValidationHelper.HasEORIOrTCUCustomsCode(Parent.NotifyParty?.Header, header.AMA_RN_NKCountry)))
				{
					Parent.ABL_OA_NotifyPartyInfo.AddMessageError(Res.GetString("1F5F341A-2196-440D-8D8C-C1E862DC06BC", "An EORI or TCUIN number must be set up against the organization"));
				}
			}
		}

		#region ValidateSpecialMentions

		public void ValidateSpecialMentions()
		{
			ValidateCalculatedProperty(Parent.SpecialMentionsInfo);
		}

		protected virtual void CheckSpecialMentions()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.SpecialMentionsInfo);
		}

		#endregion
	}
}
