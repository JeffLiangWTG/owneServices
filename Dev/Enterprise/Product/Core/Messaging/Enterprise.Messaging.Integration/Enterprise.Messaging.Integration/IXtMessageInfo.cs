using System.Collections.Generic;
using System.IO;

namespace Enterprise.Messaging.Integration
{
	public interface IXtMessageInfo
	{
		string ApplicationCode { get; }
		string MessageType { get; }
		string DestinationParty { get; }
		string SourceParty { get; }
		string MessageTrackingID { get; }

		BinaryReader GetMessageData();

		Dictionary<string, string> XTMessageAttributes { get; }
	}
}
