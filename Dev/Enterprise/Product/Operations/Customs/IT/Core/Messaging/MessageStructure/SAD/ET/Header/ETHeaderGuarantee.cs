using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETHeaderGuarantee
{
	readonly IETHeaderGuarantee iETHeaderGuarantee;

	public ETHeaderGuarantee(IETHeaderGuarantee iETHeaderGuarantee)
	{
		this.iETHeaderGuarantee = Argument.NotNull(iETHeaderGuarantee, "iETHeaderGuarantee");
	}

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 1, false)]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	public ZString GuaranteeType => iETHeaderGuarantee.Type;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 24, false)]
	[MessageFieldExportWithTransitRules("D", "C125")]
	[MessageFieldTransitRules("D", "C125")]
	public ZString Grn => iETHeaderGuarantee.Grn;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldExportWithTransitRules("D", "C130")]
	[MessageFieldTransitRules("D", "C130")]
	public ZString OtherReference => iETHeaderGuarantee.OtherReference;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 4, true)]
	[MessageFieldExportWithTransitRules("D", "C86")]
	[MessageFieldTransitRules("D", "C86")]
	public ZString AccessCode => iETHeaderGuarantee.AccessCode;

	[MessageLayout(Order = 4)]
	[MessageFieldDecimalRepresentation(10, 2, false, true)]
	[MessageFieldExportWithTransitRules("D", "C130")]
	[MessageFieldTransitRules("D", "C130")]
	public ZDecimal? Amount => iETHeaderGuarantee.Amount;

	[MessageLayout(Order = 5)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	public ZString NotValidForEC => iETHeaderGuarantee.NotValidForEC;

	[MessageLayout(Order = 6)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	public ZString NotValidForOtherContractingParties => iETHeaderGuarantee.NotValidForOtherContractingParties;
}
