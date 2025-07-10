
namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ImportPreviousDocumentValidation : CommonPreviousDocumentValidation
	{
		public ImportPreviousDocumentValidation(PreviousDocument parent) : base(parent)
		{
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();

			var parent = Parent;
			if (parent.Declaration is JobDeclaration declaration && declaration.IsUCC5 && parent.CSI_ReferenceNumber.Length > PreviousDocument.CSI_ReferenceNumberMaxLength_AISUCC5)
			{
				parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("5AB42EC7-23B0-4356-9D5C-4337E1CC2CA7", "Previous Document Reference Number can have up to {0} alpha numeric characters.", PreviousDocument.CSI_ReferenceNumberMaxLength_AISUCC5));
			}
		}
	}
}
