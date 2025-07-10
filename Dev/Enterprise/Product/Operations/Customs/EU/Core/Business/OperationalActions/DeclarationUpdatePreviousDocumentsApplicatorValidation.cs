using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.OperationalActions
{
	public class DeclarationUpdatePreviousDocumentsApplicatorValidation : AutoDeclarationUpdatePreviousDocumentsApplicatorValidation
	{
		public DeclarationUpdatePreviousDocumentsApplicatorValidation(AutoDeclarationUpdatePreviousDocumentsApplicator parent) : base(parent)
		{
		}

		protected override void CheckDocumentCode()
		{
			base.CheckDocumentCode();

			if (!Parent.ReferenceNumber.IsEmpty || !Parent.Class.IsEmpty || !Parent.LineNo.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.DocumentCodeInfo);
			}
		}

		protected override void CheckReferenceNumber()
		{
			base.CheckReferenceNumber();
			if (!Parent.DocumentCode.IsEmpty || !Parent.Class.IsEmpty || !Parent.LineNo.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ReferenceNumberInfo);
			}
		}

		protected override void CheckClass()
		{
			base.CheckClass();
			if (!Parent.DocumentCode.IsEmpty || !Parent.ReferenceNumber.IsEmpty || !Parent.LineNo.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ClassInfo);
			}
		}
	}
}
