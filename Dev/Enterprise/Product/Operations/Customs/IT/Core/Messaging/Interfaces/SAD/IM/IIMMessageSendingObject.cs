using System.Collections.Generic;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IIMMessageSendingObject : ISadMessageSendingObject
{
	IIMHeader MessageHeader { get; }
	IEnumerable<IIMLine> MessageLines { get; }
	IEnumerable<INBMessageSendingObject> NBMessages { get; }
}
