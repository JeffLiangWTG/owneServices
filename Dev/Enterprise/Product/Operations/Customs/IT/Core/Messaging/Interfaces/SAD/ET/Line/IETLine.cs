using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IETLine : ILineCommon
{
	ZString DeclarationType { get; }
	ITrader Consignor { get; }
	ITrader Consignee { get; }
	ZString DispatchCountryCode { get; }
	ZString DestinationCountryCode { get; }
	IETLineSecurityBlock SecurityBlock { get; }
	IEnumerable<IPackage> Packages { get; }
	IETLineSpecialMentionGroup SpecialMentionGroup { get; }
	ZString ComplementOfInformation { get; }
	ZString ComplementOfInformationLng { get; }
}
