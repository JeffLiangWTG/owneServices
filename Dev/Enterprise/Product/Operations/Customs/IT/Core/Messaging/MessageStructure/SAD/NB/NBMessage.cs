using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;
using Enterprise.Customs.IT.Messaging.MessageStructure;

namespace Enterprise.Customs.IT.Messaging.SAD;

public sealed class NBMessage : SadCustomsMessage
{
	internal const int MaxIterationNumberEachRow = 4;

	public NBMessage(INBMessageSendingObject nBMessage, ISadMessageSendingObject sadMessageSendingObject)
	{
		this.nBMessage = Argument.NotNull(nBMessage, nameof(nBMessage));
		this.sadMessageSendingObject = Argument.NotNull(sadMessageSendingObject, nameof(sadMessageSendingObject));
	}

	public NBMessage(INBMessageSendingObject nBMessage, ZInt progressiveNumber, ISadMessageSendingObject sadMessageSendingObject)
		: this(nBMessage, sadMessageSendingObject)
	{
		this.progressiveNumber = Argument.GreaterThanOrEqualToZero(progressiveNumber, nameof(progressiveNumber));
	}

	readonly INBMessageSendingObject nBMessage;
	readonly ZInt progressiveNumber;
	readonly ISadMessageSendingObject sadMessageSendingObject;

	[MessageLayout(Order = 0)]
	public NBMessageHeader Header => new NBMessageHeader(nBMessage.Header, nBMessage.DataBlocks, nBMessage.AnnualProgressiveNumber, progressiveNumber, sadMessageSendingObject);

	[MessageLayout(Order = 1)]
	public IEnumerable<NBMessageContinuation> Continuations
	{
		get
		{
			var nBDataBlockTotalCount = nBMessage.DataBlocks.Count();

			for (int i = MaxIterationNumberEachRow; i < nBDataBlockTotalCount; i += MaxIterationNumberEachRow)
			{
				var dataBlocks = nBMessage.DataBlocks.Skip(i).Take(MaxIterationNumberEachRow);
				yield return new NBMessageContinuation(dataBlocks, nBMessage.AnnualProgressiveNumber, progressiveNumber, i > nBDataBlockTotalCount, sadMessageSendingObject);
			}
		}
	}
}
