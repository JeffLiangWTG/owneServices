using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business
{
	public class RF415DocumentSendingObjectValidation : AutoRF415DocumentSendingObjectValidation
	{
		public RF415DocumentSendingObjectValidation(AutoRF415DocumentSendingObject parent)
			: base(parent)
		{
		}

		new RF415DocumentSendingObject Parent => (RF415DocumentSendingObject)base.Parent;

		protected override void CheckDocumentType()
		{
			base.CheckDocumentType();
			if (Parent.MessageSendingObject.ShouldSend)
			{
				MandatoryValidation.CheckEntered(Parent.DocumentTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.DocumentTypeInfo);
			}
		}
	}
}
