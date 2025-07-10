using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815CMessageContinuation
{
	const int MaxIterationNumber = 99;
	public IE815CMessageContinuation(IIE815EMCSMessageContinuation emcsMessageContinuation, int progressiveNumberRecord)
	{
		this.emcsMessageContinuation = Argument.NotNull(emcsMessageContinuation, "emcsMessageContinuation");
		var progressiveNumberRecordName = nameof(progressiveNumberRecord);
		ProgressiveNumberRecord = Argument.GreaterThanOrEqualToZero(progressiveNumberRecord, progressiveNumberRecordName);
		if (progressiveNumberRecord > MaxIterationNumber)
		{
			throw new ArgumentOutOfRangeException(progressiveNumberRecordName, FormattableString.Invariant($"{progressiveNumberRecordName} must be less than {MaxIterationNumber}"));
		}
	}
	readonly IIE815EMCSMessageContinuation emcsMessageContinuation;

	[MessageLayout(Order = 0)]
	public IE815FixedPart Attributes => new IE815FixedPart(emcsMessageContinuation.Attributes);

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 1, true)]
	[MessageFieldRules("R")]
	public ZString RecordType => "C";

	[MessageLayout(Order = 2)]
	[MessageFieldIntegerRepresentation(4, true)]
	[MessageFieldRules("R")]
	public ZInt ProgressiveNumberRecord { get; }

	[MessageLayout(Order = 3)]
	public IE815BodyEad BodyEad => new IE815BodyEad(emcsMessageContinuation.BodyEad);
}
