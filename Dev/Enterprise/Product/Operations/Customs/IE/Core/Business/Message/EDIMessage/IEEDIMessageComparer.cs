using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class IEEDIMessageComparer : Enterprise.Messaging.Business.EDIMessageComparer
	{
		public IEEDIMessageComparer(ListSortDirection sortDirection) : base(sortDirection)
		{
		}

		protected override int CompareOtherConditions(Enterprise.Messaging.Business.EDIMessage messageX, Enterprise.Messaging.Business.EDIMessage messageY)
		{
			int result = 0;

			var baseResult = base.CompareOtherConditions(messageX, messageY);
			if (baseResult != 0)
			{
				result = baseResult;
			}
			else
			{
				var messageTypeX = messageX.EM_MessageType;
				var messageTypeY = messageY.EM_MessageType;
				if (
					messageX.EM_ApplicationCode.EqualsIgnoringCase(EDIMessage.ApplicationCodes.IECustomsNCTS)
					&& messageY.EM_ApplicationCode.EqualsIgnoringCase(EDIMessage.ApplicationCodes.IECustomsNCTS)
				)
				{
					result = ResponseMessageDetails.NCTSResponseMessageDetails.CompareMessageType(messageTypeX, messageTypeY);
				}
				else if (messageTypeX != messageTypeY)
				{
					if (messageTypeX == AESIncomingMessageTypeList.Codes.IE525)
					{
						result = 1;
					}
					else if (messageTypeY == AESIncomingMessageTypeList.Codes.IE525)
					{
						result = -1;
					}
					else if (messageTypeX == AESIncomingMessageTypeList.Codes.IE528)
					{
						result = -1;
					}
					else if (messageTypeY == AESIncomingMessageTypeList.Codes.IE528)
					{
						result = 1;
					}
				}
			}

			return result;
		}

		public static EDIMessage[] GetSortedMessages(IEnumerable<EDIMessage> messages, ListSortDirection sortDirection)
		{
			return messages.OrderBy((EDIMessage x) => x, new IEEDIMessageComparer(sortDirection)).ToArray();
		}
	}
}
