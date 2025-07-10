using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class ETHeaderPrincipalTraderWrapper : SADEmptyTraderWrapper, IETHeaderPrincipalTrader
{
	public ZString TraderGuaranteeTaxIdentificationNumber => ZString.Empty;

	public ZString TIRHolderIdentification => ZString.Empty;

	public ZString RepresentativeGuaranteeTaxIdentificationNumber => ZString.Empty;

	public ZString RepresentativeName => ZString.Empty;

	public ZString RepresentativeType => ZString.Empty;
}
