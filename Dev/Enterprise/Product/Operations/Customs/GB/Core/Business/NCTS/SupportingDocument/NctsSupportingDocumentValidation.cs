namespace Enterprise.Customs.GB.Business
{
	public class NctsSupportingDocumentValidation : EU.NCTS.Business.NctsSupportingDocumentPhase4Validation
	{
		public NctsSupportingDocumentValidation(NctsSupportingDocument parent)
			: base(parent)
		{
		}

		protected new NctsSupportingDocument Parent => (NctsSupportingDocument)base.Parent;
	}
}
