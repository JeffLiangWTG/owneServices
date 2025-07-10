using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging
{
	public interface IEMCSDocumentCert
	{
		ZString Description { get; }
		ZString Reference { get; }
		ZString Type { get; }
	}
}
