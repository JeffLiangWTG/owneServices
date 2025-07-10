using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IIE807Event : IEMCSEvent
	{
		ZString Reason { get; }
		ZString ComplementaryInformation { get; }
	}
}
