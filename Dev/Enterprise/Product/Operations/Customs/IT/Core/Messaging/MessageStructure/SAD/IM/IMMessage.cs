using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;
using Enterprise.Customs.IT.Messaging.MessageStructure;

namespace Enterprise.Customs.IT.Messaging.SAD;

public sealed class IMMessage : SadCustomsMessage
{
	public IMMessage(IIMMessageSendingObject iMMessage)
	{
		this.iMMessage = Argument.NotNull(iMMessage, nameof(iMMessage));
	}
	readonly IIMMessageSendingObject iMMessage;

	[MessageLayout(Order = 0)]
	public IMMessageHeader MessageHeader => new IMMessageHeader(iMMessage.MessageHeader, iMMessage);

	[MessageLayout(Order = 1)]
	public IEnumerable<IMMessageLine> MessageLines => iMMessage.MessageLines.Select(line => new IMMessageLine(line, iMMessage.MessageHeader, iMMessage));

	[MessageLayout(Order = 2)]
	public IEnumerable<NBMessage> NBMessages
	{
		get
		{
			int progressiveNumber = 1;
			foreach (var nbMessage in iMMessage.NBMessages)
			{
				yield return new NBMessage(nbMessage, progressiveNumber, iMMessage);
				progressiveNumber++;
			}
		}
	}
}
