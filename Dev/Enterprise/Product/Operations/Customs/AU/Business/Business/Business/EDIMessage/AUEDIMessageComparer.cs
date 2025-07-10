using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class AUEDIMessageComparer : AUEDIMessageComparer<EDIMessage>
	{
		public AUEDIMessageComparer(ListSortDirection sortDirection)
			: base(sortDirection)
		{
		}

		public static EDIMessage[] GetSortedMessages(IEnumerable<EDIMessage> messages, ListSortDirection sortDirection)
		{
			return messages.OrderBy(x => x, new AUEDIMessageComparer(sortDirection)).ToArray();
		}
	}

	class AUEDIMessageComparer<T> : EDIMessageComparer<T> where T : EDIMessage
	{
		public AUEDIMessageComparer(ListSortDirection sortDirection)
			: base(sortDirection)
		{
		}

		protected override IEnumerable<Comparison<T>> GetMessageComparers(T messageX, T messageY)
		{
			var isCUSRESMessageX = messageX is CMRCUSRESMessage;
			var isCUSRESMessageY = messageY is CMRCUSRESMessage;

			if (isCUSRESMessageX && isCUSRESMessageY)
			{
				yield return CompareTransactionDateTime;

				if (messageX.EM_ReceiveTransmit == messageY.EM_ReceiveTransmit)
				{
					yield return CompareInterchangeNums;
				}
			}

			foreach (var messageComparer in base.GetMessageComparers(messageX, messageY))
			{
				yield return messageComparer;
			}
		}

		int CompareTransactionDateTime(T messageX, T messageY)
		{
			var incomingMessageX = messageX as CMRCUSRESMessage;
			var incomingMessageY = messageY as CMRCUSRESMessage;

			var messageX_ProcessingDate = incomingMessageX.ProcessingDate;
			var messageY_ProcessingDate = incomingMessageY.ProcessingDate;

			int result = 0;

			if (!messageX_ProcessingDate.IsEmpty && !messageY_ProcessingDate.IsEmpty)
			{
				result = messageX_ProcessingDate.CompareTo(messageY_ProcessingDate);
			}

			return result;
		}
	}
}
