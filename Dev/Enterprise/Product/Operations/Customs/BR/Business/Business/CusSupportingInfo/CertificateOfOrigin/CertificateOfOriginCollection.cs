using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class CertificateOfOriginCollection : Customs.Business.CusSupportingInfoCollection<CertificateOfOrigin>
	{
		public CertificateOfOriginCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.CertificateOfOrigin)
		{
		}
	}
}
