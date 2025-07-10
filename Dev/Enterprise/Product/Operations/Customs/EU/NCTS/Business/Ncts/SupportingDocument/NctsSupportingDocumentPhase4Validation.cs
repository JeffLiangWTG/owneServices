namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsSupportingDocumentPhase4Validation : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentValidation
	{
		public NctsSupportingDocumentPhase4Validation(NctsSupportingDocument parent)
			: base(parent)
		{
		}

		protected new NctsSupportingDocument Parent => (NctsSupportingDocument)base.Parent;

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			if (ShouldCheckConditionC902)
			{
				var header = Parent.Header;
				if (header != null)
				{
					header.CheckConditionC902(Parent, Parent.CSI_ReferenceNumberInfo);
				}
			}
		}

		protected virtual bool ShouldCheckConditionC902 => true;
	}
}
