using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IIE818UnsatisfactoryReason
	{
		ZString ReasonCode { get; }
		ZString ComplementaryInformation { get; }
	}
}
