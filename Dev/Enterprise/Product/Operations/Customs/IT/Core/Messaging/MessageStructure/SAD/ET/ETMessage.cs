using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;
using Enterprise.Customs.IT.Messaging.MessageStructure;

namespace Enterprise.Customs.IT.Messaging.SAD;

public sealed class ETMessage : SadCustomsMessage
{
	public ETMessage(IETMessageSendingObject iETMessage)
	{
		this.iETMessage = Argument.NotNull(iETMessage, nameof(iETMessage));
	}

	readonly IETMessageSendingObject iETMessage;

	[MessageLayout(Order = 0)]
	public ETMessageHeader MessageHeader => new ETMessageHeader(iETMessage.MessageHeader, iETMessage);

	[MessageLayout(Order = 1)]
	public IEnumerable<ETMessageLine> MessageLines
	{
		get
		{
			foreach (var line in iETMessage.MessageLines)
			{
				yield return new ETMessageLine(line, iETMessage.MessageHeader, iETMessage);
			}
		}
	}

	[MessageLayout(Order = 2)]
	public IEnumerable<NBMessage> NBMessages
	{
		get
		{
			int progressiveNumber = 1;
			foreach (var nbMessage in iETMessage.NBMessages)
			{
				yield return new NBMessage(nbMessage, progressiveNumber, iETMessage);
				progressiveNumber++;
			}
		}
	}
}
