using Enterprise.Customs.ES.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class CusExitHeaderLookups : EU.ExitControl.Business.CusExitHeaderLookups
	{
		public CusExitHeaderLookups(CusExitHeader parent) : base(parent)
		{
		}
		public new CusExitHeader Parent => (CusExitHeader)base.Parent;

		public CodeDescriptionPairList CertificateNames => CertificateHelper.CertificateNames(Factory, Parent.CustomsAgent, GetType().Name);
	}
}
