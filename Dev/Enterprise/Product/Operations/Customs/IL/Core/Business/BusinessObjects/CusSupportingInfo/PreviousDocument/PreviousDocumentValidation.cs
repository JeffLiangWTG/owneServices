using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business
{
	public class PreviousDocumentValidation : CusSupportingInfoValidation
	{
		public PreviousDocumentValidation(AutoCusSupportingInfo parent) : base(parent)
		{
		}

		protected new PreviousDocument Parent => (PreviousDocument)base.Parent;
	}
}
