using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CertificateReissueHeader : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CertificateReissueHeader(BusinessObjectFactory factory, CodeDescriptionPairList certificates)
		{
			this.certificates = certificates;
		}
		readonly CodeDescriptionPairList certificates;

		public bool IsValid => CertificateReissueRequests.Count > 0 && CertificateReissueRequests.Cast<CertificateReissueRequest>().All(x => x.IsValid);

		public CertificateReissueRequestCollection CertificateReissueRequests
		{
			get
			{
				if (certificateReissueRequests == null)
				{
					certificateReissueRequests = new CertificateReissueRequestCollection(Factory, certificates);
					RegisterEditableChildObject(certificateReissueRequests);
				}

				return certificateReissueRequests;
			}
		}
		CertificateReissueRequestCollection certificateReissueRequests;
	}
}
