using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IIMHeaderCompanyRegister
{
	ZString Number { get; }
	ZString Series { get; }
	ZDate Date { get; }
}
