using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IETHeaderSecurityBlock : IETSecurityBlockCommon
{
	ZString SpecificCircumstanceIndicator { get; }
	ZString PlaceOfLoadingCode { get; }
	ZString ConveyanceReferenceNumber { get; }
	ZString PlaceOfUnloadingCode { get; }
	IEnumerable<ZString> TransitCountries { get; }
	ITrader Carrier { get; }
	ZInt SealsNumber { get; }
}
