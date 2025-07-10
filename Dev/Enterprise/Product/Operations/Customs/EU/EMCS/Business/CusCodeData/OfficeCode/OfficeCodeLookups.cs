using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class OfficeCodeLookups : EuOfficeCodeLookups
	{
		public OfficeCodeLookups(OfficeCode officeCode) : base(officeCode)
		{
		}

		protected new OfficeCode Parent => (OfficeCode)base.Parent;

		protected EMCSJobDeclaration Declaration => (EMCSJobDeclaration)Parent.Parent;
	}
}
