using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815ComplementConsigneeTrader
{
	public IE815ComplementConsigneeTrader(IComplementConsigneeTrader complementConsigneeTrader)
	{
		this.complementConsigneeTrader = Argument.NotNull(complementConsigneeTrader, "complementConsigneeTrader");
	}
	readonly IComplementConsigneeTrader complementConsigneeTrader;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, true)]
	[MessageFieldRules("C", "C007", "C034")]
	public ZString MemberStateCode => complementConsigneeTrader.MemberStateCode;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 255, false)]
	[MessageFieldRules("C", "C008", "C034", "D002")]
	public ZString SerialNumberOfCertificateOfExemption => complementConsigneeTrader.SerialNumberOfCertificateOfExemption;
}
