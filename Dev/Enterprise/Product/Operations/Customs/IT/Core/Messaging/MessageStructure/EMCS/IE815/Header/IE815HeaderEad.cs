using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815HeaderEad
{
	public IE815HeaderEad(IHeaderEad headerEad)
	{
		this.headerEad = Argument.NotNull(headerEad, "headerEad");
	}
	readonly IHeaderEad headerEad;

	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(1, true)]
	[MessageFieldRules("R")]
	public ZInt DestinationTypeCode => headerEad.DestinationTypeCode;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, true)]
	[MessageFieldRules("R", "R001")]
	public ZString DurationTransportUnitMeasure => headerEad.DurationTransportUnitMeasure;

	[MessageLayout(Order = 2)]
	[MessageFieldIntegerRepresentation(2, true)]
	[MessageFieldRules("R", "R002")]
	public ZInt JourneyTime => headerEad.JourneyTime;

	[MessageLayout(Order = 3)]
	[MessageFieldIntegerRepresentation(1, true)]
	[MessageFieldRules("R")]
	public ZInt TransportArrangement => headerEad.TransportArrangement;

	[MessageLayout(Order = 4)]
	[MessageFieldBoolRepresentation()]
	[MessageFieldRules("R", "D001", "R003")]
	public ZBool SendFlagDeferred => headerEad.SendFlagDeferred;
}
