using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin
{
	public interface IATRCertificateItem
	{
		ZString LineNumber { get; }
		ZString GoodsDescription { get; }
		ZString Weight { get; }
		ZString Volume { get; }
	}
}
