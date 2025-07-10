using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IEMCSDocumentCert
	{
		ZString Description { get; }
		ZString Reference { get; }
		ZString Type { get; }
	}
}
