using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class CusExitControlHeaderLookups : EU.Business.CusExitControlHeaderLookups
	{
		public CusExitControlHeaderLookups(CusExitControlHeader parent) : base(parent)
		{
		}
		public new CusExitControlHeader Parent => (CusExitControlHeader)base.Parent;

		public CodeDescriptionPairList CertificateNames => CertificateHelper.CertificateNames(Factory, Parent.CustomsAgent, GetType().Name);
	}
}
