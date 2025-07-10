using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.H7.Business
{
	public class PresentationOfficeCodeValidation : EuOfficeCodeValidation
	{
		public PresentationOfficeCodeValidation(PresentationOfficeCode parent)
			: base(parent)
		{
		}

		protected new PresentationOfficeCode Parent => (PresentationOfficeCode)base.Parent;
	}
}
