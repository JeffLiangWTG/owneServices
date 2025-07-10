using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class EMCSJobDeclarationLookups : EU.EMCS.Business.EMCSJobDeclarationLookups
	{
		public EMCSJobDeclarationLookups(EMCSJobDeclaration parent) : base(parent)
		{
		}

		protected new EMCSJobDeclaration Parent => (EMCSJobDeclaration)base.Parent;

		public EMCSGlbCompanyCredentialCollection CertificateIdentifierList => Parent.EMCSGlbExternalPasswordCollection;
	}
}
