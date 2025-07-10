using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IDeclarantTrader : ITrader
{
	ZString RepresentativeType { get; }
}
