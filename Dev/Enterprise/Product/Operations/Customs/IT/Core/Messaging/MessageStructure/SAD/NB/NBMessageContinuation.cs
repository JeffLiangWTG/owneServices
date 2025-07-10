using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;
using Enterprise.Customs.IT.Messaging.MessageStructure;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class NBMessageContinuation : IMessageContinuation
{
	public NBMessageContinuation(IEnumerable<IPreviousOperationInfo> nBDataBlocks, ZString annualProgressiveNumber, ZInt progressiveNumber, ZBool continuation, ISadMessageSendingObject sadMessageSendingObject)
	{
		this.nBDataBlocks = Argument.NotNull(nBDataBlocks, nameof(nBDataBlocks));
		this.annualProgressiveNumber = Argument.NotNullOrEmpty(annualProgressiveNumber, nameof(annualProgressiveNumber));
		this.progressiveNumber = Argument.GreaterThanOrEqualToZero(progressiveNumber, nameof(progressiveNumber));
		this.continuation = continuation;
		this.sadMessageSendingObject = Argument.NotNull(sadMessageSendingObject, nameof(sadMessageSendingObject));
	}

	readonly IEnumerable<IPreviousOperationInfo> nBDataBlocks;
	readonly ZString annualProgressiveNumber;
	readonly ZInt progressiveNumber;
	readonly ZBool continuation;
	readonly ISadMessageSendingObject sadMessageSendingObject;

	[MessageLayout(Order = 0)]
	public ISadMessageFixedPart FixedPart => SadMessageFixedPartFactory.GetSadMessageFixedPart
		(isHeader: false
		, sadMessageSendingObject: sadMessageSendingObject
		, messageCode: SadMessageFixedPart.Constants.MessageCode.NB.Continuation
		, annualProgressiveNumber: annualProgressiveNumber
		, progressiveNumber: progressiveNumber);

	[MessageLayout(Order = 1)]
	public NBDataBlock HeaderBlock => new NBDataBlock(nBDataBlocks, continuation);
}
