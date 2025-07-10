using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging
{
	public interface IED881 : IEmcsDataProvider
	{
		ZString MessageSender { get; }

		ZBool RequestAccepted { get; }

		ZString RejectionReason { get; }

		ZString RejectionComplement { get; }

		IEMCSEvent ResponseAttributes { get; }
	}
}
