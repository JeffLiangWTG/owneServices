using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;
using Enterprise.Customs.IT.Messaging.MessageStructure;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public sealed class IE815Message : SadCustomsMessage
{
	public IE815Message(IIE815EMCSMessage iE815EMCSMessage)
	{
		this.iE815EMCSMessage = Argument.NotNull(iE815EMCSMessage, "iE815EMCSMessage");
	}

	readonly IIE815EMCSMessage iE815EMCSMessage;

	[MessageLayout(Order = 0)]
	public IE815AMessageHeader Header => new IE815AMessageHeader(iE815EMCSMessage);

	[MessageLayout(Order = 1)]
	public IEnumerable<IE815CMessageContinuation> Continuations
	{
		get
		{
			int i = 0;
			foreach (var continuation in iE815EMCSMessage.Continuations)
			{
				yield return new IE815CMessageContinuation(continuation, i++);
			}
		}
	}
}
