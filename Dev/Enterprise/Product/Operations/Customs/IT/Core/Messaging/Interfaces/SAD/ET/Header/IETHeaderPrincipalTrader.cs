using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IETHeaderPrincipalTrader : ITrader
{
	ZString TraderGuaranteeTaxIdentificationNumber { get; }
	ZString TIRHolderIdentification { get; }
	ZString RepresentativeGuaranteeTaxIdentificationNumber { get; }
	ZString RepresentativeName { get; }
	ZString RepresentativeType { get; }
}
