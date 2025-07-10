using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin
{
	public interface IEURCertificateOfOrigin : ICertificateOfOrigin
	{
		IEURGoodsSummary GoodsSummary { get; }
		ZString OriginCountry { get; }
		ZString OriginGroup { get; }
		ZString DestinationGroup { get; }
	}
}
