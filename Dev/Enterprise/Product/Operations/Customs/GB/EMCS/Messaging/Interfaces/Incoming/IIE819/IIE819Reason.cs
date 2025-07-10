using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IIE819Reason
	{
		ZString ReasonCode { get; }
		ZString ComplementaryInformation { get; }
	}
}
