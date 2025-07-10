using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business
{
	public class AdditionalInfoSendingObjectValidation : EU.H7.Business.AdditionalInfoSendingObjectValidation
	{
		public AdditionalInfoSendingObjectValidation(AdditionalInfoSendingObject parent) : base(parent)
		{
		}

		protected new AdditionalInfoSendingObject Parent => (AdditionalInfoSendingObject)base.Parent;

		protected override void CheckDocumentType()
		{
			if (!Parent.ReferenceNumber.IsEmpty && Parent.DocumentType.IsEmpty)
			{
				Parent.DocumentTypeInfo.AddMessageError(Parent.ValidationConfiguration.ValidationMessage.GetBR20313RuleMessage(Parent.DocumentTypeInfo));
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.DocumentTypeInfo, Parent.ValidationConfiguration.ValidationMessage.InvalidValueRuleMessage);
		}

		protected override void CheckReferenceNumber()
		{
			if (!Parent.DocumentType.IsEmpty && Parent.ReferenceNumber.IsEmpty)
			{
				Parent.ReferenceNumberInfo.AddMessageError(Parent.ValidationConfiguration.ValidationMessage.GetBR20313RuleMessage(Parent.ReferenceNumberInfo));
			}
		}
	}
}
