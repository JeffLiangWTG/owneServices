using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CertificateReissueRequestCollection : NonPersistentBusinessObjectCollection<CertificateReissueRequest>
	{
		public CertificateReissueRequestCollection(BusinessObjectFactory factory, CodeDescriptionPairList certificates)
			: base(factory)
		{
			this.certificates = certificates;
		}

		readonly CodeDescriptionPairList certificates;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CertificateReissueRequest(Factory, certificates);
		}
	}
}
