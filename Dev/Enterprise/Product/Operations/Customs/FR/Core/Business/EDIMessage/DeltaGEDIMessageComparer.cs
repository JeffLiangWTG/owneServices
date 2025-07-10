using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class DeltaGEDIMessageComparer : EDIMessageComparer
	{
		public DeltaGEDIMessageComparer(ListSortDirection sortDirection) : base(sortDirection)
		{
		}

		protected override int CompareInterchangeNums(EDIMessage messageX, EDIMessage messageY)
		{
			var result = base.CompareInterchangeNums(messageX, messageY);
			var interchangeNumberX = messageX.EM_InterchangeNumber.Split('.');
			var interchangeNumberY = messageY.EM_InterchangeNumber.Split('.');

			if (interchangeNumberX.Length == 3 && interchangeNumberY.Length == 3 && interchangeNumberX.First() == interchangeNumberY.First())
			{
				var entryStatusX = WithoutSpaces(interchangeNumberX[1]);
				var entryStatusY = WithoutSpaces(interchangeNumberY[1]);
				if (entryStatusOrderList.Contains((entryStatusX, entryStatusY)))
				{
					result = -1;
				}
				else if (entryStatusOrderList.Contains((entryStatusY, entryStatusX)))
				{
					result = 1;
				}
			}

			return result;
		}

		static readonly List<(string, string)> entryStatusOrderList = new List<(string, string)>()
		{
			(WithoutSpaces(InboundDeltaStatusToEntryStatusMap.Descriptions.RS060_1.ToString()), WithoutSpaces(InboundDeltaStatusToEntryStatusMap.Descriptions.RS100_1.ToString())),
			(WithoutSpaces(InboundDeltaStatusToEntryStatusMap.Descriptions.RS060_2.ToString()), WithoutSpaces(InboundDeltaStatusToEntryStatusMap.Descriptions.RS100_1.ToString())),
			(WithoutSpaces(InboundDeltaStatusToEntryStatusMap.Descriptions.RS060_3.ToString()), WithoutSpaces(InboundDeltaStatusToEntryStatusMap.Descriptions.RS100_1.ToString())),
			(WithoutSpaces(InboundDeltaStatusToEntryStatusMap.Descriptions.RS061_1.ToString()), WithoutSpaces(InboundDeltaStatusToEntryStatusMap.Descriptions.RS101_1.ToString())),
			(WithoutSpaces(InboundDeltaStatusToEntryStatusMap.Descriptions.RS061_2.ToString()), WithoutSpaces(InboundDeltaStatusToEntryStatusMap.Descriptions.RS101_2.ToString())),
		};

		static string WithoutSpaces(string stringWithSpaces)
		{
			return new string(stringWithSpaces.Where(c => !char.IsWhiteSpace(c)).ToArray());
		}

		public static EDIMessage[] GetSortedMessages(IEnumerable<EDIMessage> messages, ListSortDirection sortDirection)
		{
			return messages.OrderBy((EDIMessage x) => x, new DeltaGEDIMessageComparer(sortDirection)).ToArray();
		}
	}
}
