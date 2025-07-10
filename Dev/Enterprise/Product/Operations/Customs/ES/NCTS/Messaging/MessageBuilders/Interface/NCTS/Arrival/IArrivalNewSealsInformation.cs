using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public interface IArrivalNewSealsInformation
	{
		ZString SealId { get; }
		ZString SealIdLanguage { get; }
	}
}
