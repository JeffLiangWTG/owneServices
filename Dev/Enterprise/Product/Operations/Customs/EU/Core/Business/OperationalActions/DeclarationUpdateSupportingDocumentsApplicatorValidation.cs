using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.OperationalActions
{
	public class DeclarationUpdateSupportingDocumentsApplicatorValidation : AutoDeclarationUpdateSupportingDocumentsApplicatorValidation
	{
		public DeclarationUpdateSupportingDocumentsApplicatorValidation(AutoDeclarationUpdateSupportingDocumentsApplicator parent) : base(parent)
		{
		}

		protected override void CheckDocumentCode()
		{
			base.CheckDocumentCode();
			if (!Parent.ReferenceNumber.IsEmpty || !Parent.Date.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.DocumentCodeInfo);
			}
		}

		protected override void CheckReferenceNumber()
		{
			base.CheckReferenceNumber();
			if (!Parent.DocumentCode.IsEmpty || !Parent.Date.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ReferenceNumberInfo);
			}
		}
	}
}
