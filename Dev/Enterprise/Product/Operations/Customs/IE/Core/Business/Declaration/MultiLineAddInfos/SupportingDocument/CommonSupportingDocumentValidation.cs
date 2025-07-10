namespace Enterprise.Customs.IE.Business.Declaration
{
	public class CommonSupportingDocumentValidation : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentValidation
	{
		public CommonSupportingDocumentValidation(SupportingDocument parent) : base(parent)
		{
		}

		protected new SupportingDocument Parent => (SupportingDocument)base.Parent;
	}
}
