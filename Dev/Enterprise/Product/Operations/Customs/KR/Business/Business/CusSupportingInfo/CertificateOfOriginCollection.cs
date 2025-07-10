using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class CertificateOfOriginCollection : CusSupportingInfoCollection<CertificateOfOrigin>
	{
		public CertificateOfOriginCollection(ISupportingDocumentParent parent)
			: base((BusinessObject)parent, CusSupportingInfoTypeList.Codes.CertificateOfOrigin)
		{
		}
	}
}
