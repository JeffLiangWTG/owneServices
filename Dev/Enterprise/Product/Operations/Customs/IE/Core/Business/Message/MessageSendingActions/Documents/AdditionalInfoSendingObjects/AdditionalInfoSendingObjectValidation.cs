using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class AdditionalInfoSendingObjectValidation : AutoAdditionalInfoSendingObjectValidation
	{
		public AdditionalInfoSendingObjectValidation(AutoAdditionalInfoSendingObject parent) : base(parent)
		{
		}

		public new AdditionalInfoSendingObject Parent => (AdditionalInfoSendingObject)base.Parent;

		protected override void CheckDocumentType()
		{
			if (Parent.Action?.ShouldSend == true)
			{
				base.CheckDocumentType();
				MandatoryValidation.CheckEntered(Parent.DocumentTypeInfo);
			}
		}

		protected override void CheckDocumentInformation()
		{
			if (Parent.Action?.ShouldSend == true && ShouldValidateDocumentInformation)
			{
				base.CheckDocumentInformation();
				MandatoryValidation.CheckEntered(Parent.DocumentInformationInfo);
			}
		}

		bool ShouldValidateDocumentInformation
		{
			get
			{
				var action = Parent.Action;
				return !(action is UploadDocumentsSendingAction)
						|| action is UploadDocumentsSendingAction uploadDocumentsSendingAction && uploadDocumentsSendingAction.MessageType == AISUploadDocumentsMessageTypeList.Codes.IM483;
			}
		}

		protected override void CheckCCQualifier()
		{
			if (Parent.Action?.ShouldSend == true)
			{
				base.CheckCCQualifier();
				ListValidation.MessageErrorIfInvalidCode(Parent.CCQualifierInfo);
			}
		}
	}
}
