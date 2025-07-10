using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IETHeader : IHeaderCommon
{
	ZBool HeaderDataDeclaredOnItems { get; }
	ZBool? SecurityData { get; }
	IMeansOfTransport MeansOfTransportAtDeparture { get; }
	IETHeaderMeansOfTransportCrossingBorder MeansOfTransportCrossingBorder { get; }
	ZString DialogLanguageIndicatorAtDeparture { get; }
	IETHeaderSecurityBlock SecurityBlock { get; }
	ZString ExitCustomsOffice { get; }
	IETHeaderAgreedLocationOfGoods AgreedLocationOfGoods { get; }
	IETHeaderPrincipalTrader PrincipalTrader { get; }
	IEnumerable<IETHeaderTransitCustomsOffice> TransitCustomsOffices { get; }
	IEnumerable<IETHeaderGuarantee> Guarantees { get; }
	ZString DestinationCustomsOffice { get; }
	IEnumerable<ZString> Seals { get; }
	IETHeaderControlResult ControlResult { get; }
}
