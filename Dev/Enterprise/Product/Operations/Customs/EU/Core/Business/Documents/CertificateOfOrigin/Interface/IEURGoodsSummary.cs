using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin
{
	public interface IEURGoodsSummary : IGoodsSummary
	{
		ZString InvoiceNumbers { get; }
	}
}
