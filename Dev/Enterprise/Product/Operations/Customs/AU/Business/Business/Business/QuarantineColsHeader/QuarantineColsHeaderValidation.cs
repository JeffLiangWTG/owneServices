using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineColsHeaderValidation : AutoQuarantineColsHeaderValidation
	{
		public QuarantineColsHeaderValidation(AutoQuarantineColsHeader parent) : base(parent)
		{
		}

		protected override void CheckQCH_AlsoNotifyEmail()
		{
			base.CheckQCH_AlsoNotifyEmail();
			EmailAddressValidation.ValidateEmailAddress(Parent.QCH_AlsoNotifyEmailInfo);
		}

		protected override void CheckQCH_LateLodgementReason()
		{
			base.CheckQCH_LateLodgementReason();
			ListValidation.MessageErrorIfInvalidCode(Parent.QCH_LateLodgementReasonInfo);
		}

		protected override void CheckQCH_DeliveryClassification()
		{
			base.CheckQCH_DeliveryClassification();
			ListValidation.MessageErrorIfInvalidCode(Parent.QCH_DeliveryClassificationInfo);
			var parent = (QuarantineColsHeader)Parent;
			if (parent.LRN.IsEmpty)
			{
				CheckDeliveryClassificationIsMandatoryForSplitPostcodes(parent);
				CheckDeliveryAddressIsMandatoryForRuralPostcodes(parent);
			}
		}

		void CheckDeliveryClassificationIsMandatoryForSplitPostcodes(QuarantineColsHeader parent)
		{
			if (parent.QCH_DeliveryClassification.IsEmpty &&
				parent.DeliveryOrUnpackAddressPostcodeClassification == COLSDeliveryClassificationList.Split)
			{
				parent.QCH_DeliveryClassificationInfo.AddMessageError("Delivery Classification is mandatory for split postcodes.");
			}
		}

		void CheckDeliveryAddressIsMandatoryForRuralPostcodes(QuarantineColsHeader parent)
		{
			if (parent.QCH_DeliveryClassification == COLSDeliveryClassificationList.Codes.Rural &&
				parent.DeliveryOrUnpackAddressCompanyName.IsEmpty)
			{
				parent.QCH_DeliveryClassificationInfo.AddMessageError("Unpack location is mandatory for rural postcodes.");
			}
		}
	}
}
