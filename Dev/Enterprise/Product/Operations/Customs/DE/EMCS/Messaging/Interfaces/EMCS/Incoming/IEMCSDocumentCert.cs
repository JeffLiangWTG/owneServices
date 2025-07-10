using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging
{
	public interface IEMCSDocumentCert
	{
		ZString Description { get; }
		ZString Reference { get; }
		ZString Type { get; }
	}
}
