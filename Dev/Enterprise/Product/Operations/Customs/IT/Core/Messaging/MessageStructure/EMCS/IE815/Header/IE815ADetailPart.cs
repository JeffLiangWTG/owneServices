using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815ADetailPart
{
	public IE815ADetailPart(IIE815MessageHeaderDetailPart messageHeaderDetailPart, ZInt messageContinuationsCount)
	{
		this.messageHeaderDetailPart = Argument.NotNull(messageHeaderDetailPart, "messageHeaderDetailPart");
		TotalRecordsTypeC = Argument.GreaterThanOrEqualToZero(messageContinuationsCount, "messageContinuationsCount");
	}
	readonly IIE815MessageHeaderDetailPart messageHeaderDetailPart;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, true)]
	[MessageFieldRules("R")]
	public ZString RecordType => "A";

	[MessageLayout(Order = 1)]
	[MessageFieldIntegerRepresentation(4, true)]
	[MessageFieldRules("R")]
	public ZInt ProgressiveNumberRecord => 1;

	[MessageLayout(Order = 2)]
	[MessageFieldIntegerRepresentation(4, true)]
	[MessageFieldRules("R")]
	public ZInt ProgressiveNumberRecordsTypeA => 1;

	[MessageLayout(Order = 3)]
	[MessageFieldIntegerRepresentation(2, true)]
	[MessageFieldRules("R")]
	public ZInt TotalRecordsTypeB => ZInt.Zero;

	[MessageLayout(Order = 4)]
	[MessageFieldIntegerRepresentation(3, true)]
	[MessageFieldRules("R", "R037")]
	public ZInt TotalRecordsTypeC { get; }

	[MessageLayout(Order = 5)]
	[MessageFieldIntegerRepresentation(1, true)]
	[MessageFieldRules("R")]
	public ZInt MessageType => messageHeaderDetailPart.MessageType;
}
