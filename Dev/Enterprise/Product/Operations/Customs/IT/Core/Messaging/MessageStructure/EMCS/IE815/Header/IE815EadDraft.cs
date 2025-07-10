using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815EadDraft
{
	public IE815EadDraft(IEadDraft eadDraft)
	{
		this.eadDraft = Argument.NotNull(eadDraft, "eadDraft");
	}
	readonly IEadDraft eadDraft;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldRules("C", "R010")]
	public ZString InvoiceNumber => eadDraft.InvoiceNumber;

	[MessageLayout(Order = 1)]
	[MessageFieldDateYYYYMMDDRepresentation()]
	[MessageFieldRules("O")]
	public ZDate InvoiceDate => eadDraft.InvoiceDate;

	[MessageLayout(Order = 2)]
	[MessageFieldIntegerRepresentation(1, true)]
	[MessageFieldRules("R")]
	public ZInt OriginTypeCode => eadDraft.OriginTypeCode;

	[MessageLayout(Order = 3)]
	[MessageFieldDateYYYYMMDDRepresentation()]
	[MessageFieldRules("R011")]
	public ZDate DateOfDispatch => eadDraft.DateOfDispatch;

	[MessageLayout(Order = 4)]
	[MessageFieldDateHHMMSSRepresentation()]
	[MessageFieldRules("R012")]
	public ZDateTime TimeOfDispatch => eadDraft.TimeOfDispatch;
}
