using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor
{
	public class MessageBatch
	{
		public MessageBatch(List<ZGuid> messagePKs)
		{
			MessagePKs = messagePKs;
			NumberOfSavedMessagesInBatch = 0;
		}

		public int NumberOfSavedMessagesInBatch { get; set; }

		public List<ZGuid> MessagePKs { get; private set; }

		public bool IsEmpty
		{
			get
			{
				return MessagePKs == null || MessagePKs.Count == 0;
			}
		}
	}
}
