using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.H7.Business
{
	public class DocumentSendingObjectValidation : SupportingDocSendingObjectValidation
	{
		public DocumentSendingObjectValidation(AutoSupportingDocSendingObject parent) : base(parent)
		{
		}

		public new DocumentSendingObject Parent => (DocumentSendingObject)base.Parent;

		protected override void ValidateAllCore()
		{
			base.ValidateAllCore();
			ValidateFileDescription();
		}

		protected override void CheckDocumentType()
		{
			MandatoryValidation.CheckEntered(Parent.DocumentTypeInfo);
		}

		protected override void CheckLocalReferenceNumber() { }

		public void ValidateFileDescription()
		{
			ValidateCalculatedProperty(Parent.FileDescriptionInfo);
		}

		protected void CheckFileDescription()
		{
			MandatoryValidation.CheckEntered(Parent.FileDescriptionInfo);
		}
	}
}
