using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin
{
	public interface IGoodsSummary
	{
		ZString Description { get; }
		ZString WeightAndVolume { get; }
	}
}
