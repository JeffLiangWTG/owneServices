using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin
{
	public interface IATRBoxItems
	{
		void Build();
		ZString ItemsInfoBox9 { get; }
		ZString MarksNumberBox10 { get; }
		ZString GrossWeightBox11 { get; }
	}
}
