using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IIMLineSpecialMentionGroup : ISpecialMentionGroup
{
	IPreviousAdministrativeReference PreviousProcedure { get; }
	ZString SteelType { get; }
}
