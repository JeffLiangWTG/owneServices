using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Messaging
{
	public interface IEMCSMessageHeader
	{
		ZString MessageSender { get; set; }

		ZString MessageRecipient { get; set; }

		ZDate DateOfPreparation { get; set; }

		ZDateTime TimeOfPreparation { get; set; }

		ZString MessageIdentifier { get; set; }

		ZString CorrelationIdentifier { get; set; }
	}
}
