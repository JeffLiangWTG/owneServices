using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DataTransfer.Native.Business.Requests
{
	public class MessageNumberCollectionWrapper
	{
		MessageNumberCollectionWrapper()
		{
			MessageNumberCollection = new List<MessageNumberWrapper>();
		}

		public List<MessageNumberWrapper> MessageNumberCollection { get; set; }

		public static MessageNumberCollectionWrapper New(IEnumerable<IMessageNumber> messageNumberCollection)
		{
			MessageNumberCollectionWrapper result = new MessageNumberCollectionWrapper();
			if (messageNumberCollection != null)
			{
				foreach (var messageNumber in messageNumberCollection)
				{
					result.MessageNumberCollection.Add(MessageNumberWrapper.New((MessageNumber)messageNumber));
				}
			}
			else
			{
				result.MessageNumberCollection = null;
			}
			return result;
		}
	}
}
