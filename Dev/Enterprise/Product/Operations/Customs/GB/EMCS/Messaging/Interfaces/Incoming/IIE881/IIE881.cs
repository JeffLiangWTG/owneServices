using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IIE881 : IEMCSInboundProvider
	{
		ZString MessageSender { get; }
		ZString MessageRecepient { get; }
		ZDateTime DateOfPreparation { get; }
		ZDateTime TimeOfPreparation { get; }
		ZString MessageIdentifier { get; }
		IIE881ManualClosureResponse ManualClosureResponse { get; }
	}
}
