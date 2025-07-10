using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class SADDeclarantTraderWrapper : IDeclarantTrader
{
	public SADDeclarantTraderWrapper(IDeclarantProvider declarantProvider)
	{
		this.declarantProvider = Argument.NotNull(declarantProvider, nameof(declarantProvider));
		traderWrapper = new SADTraderWrapper(declarantProvider?.DeclarantAddress);
	}
	readonly IDeclarantProvider declarantProvider;
	readonly SADTraderWrapper traderWrapper;

	public ZString RepresentativeType => CustomsRulesProvider.ConvertRepresentativeTypeToItalianCustomsFormat(declarantProvider.RepresentativeType);

	public ZString IdCountryCode => GetValueOrEmptyWhenSelfRepresentativeType(traderWrapper.IdCountryCode);

	public ZString ID => GetValueOrEmptyWhenSelfRepresentativeType(traderWrapper.ID);

	public ZString Name => GetValueOrEmptyWhenSelfRepresentativeType(traderWrapper.Name);

	public ZString Address => GetValueOrEmptyWhenSelfRepresentativeType(traderWrapper.Address);

	public ZString Postcode => GetValueOrEmptyWhenSelfRepresentativeType(traderWrapper.Postcode);

	public ZString City => GetValueOrEmptyWhenSelfRepresentativeType(traderWrapper.City);

	public ZString CountryCode => GetValueOrEmptyWhenSelfRepresentativeType(traderWrapper.CountryCode);

	ZString GetValueOrEmptyWhenSelfRepresentativeType(ZString value) => RepresentativeType != SADConstants.RepresentationTypeList.Self ? value : ZString.Empty;
}
