using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.H7.Business
{
	public class PresentationOfficeCodeLookups : EuOfficeCodeLookups
	{
		public PresentationOfficeCodeLookups(PresentationOfficeCode officeCode)
			: base(officeCode)
		{
		}

		protected new PresentationOfficeCode Parent => (PresentationOfficeCode)base.Parent;
	}
}
