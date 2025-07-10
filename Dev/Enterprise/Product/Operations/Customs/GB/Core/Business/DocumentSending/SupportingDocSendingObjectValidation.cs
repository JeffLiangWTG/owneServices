using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Business.DocumentSending
{
	public class SupportingDocSendingObjectValidation : Customs.Business.SupportingDocSendingObjectValidation
	{
		public SupportingDocSendingObjectValidation(SupportingDocSendingObject parent) : base(parent)
		{
		}

		protected override void CheckDocumentType()
		{
			MandatoryValidation.CheckEntered(Parent.DocumentTypeInfo);
		}

		public override char[] InvalidFileNameCharsForSending => new char[] { '{', '}' };

		public override ZInt MaxEDocFileSizeInBytes => 10_000_000;
	}
}
