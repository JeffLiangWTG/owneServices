using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Business
{
	public class EDIMessageComparer : EDIMessageComparer<EDIMessage>
	{
		public EDIMessageComparer(ListSortDirection sortDirection)
			: base(sortDirection)
		{
		}

		public static T[] GetSortedMessages<T>(IEnumerable<T> messages, ListSortDirection sortDirection) where T : EDIMessage
		{
			return messages.OrderBy(x => x, new EDIMessageComparer(sortDirection)).ToArray();
		}
	}

	public class EDIMessageComparer<T> : AutoEDIMessageComparer<T> where T : EDIMessage
	{
		public EDIMessageComparer(ListSortDirection sortDirection)
			: base(sortDirection)
		{
		}

		protected override IEnumerable<Comparison<T>> GetMessageComparers(T messageX, T messageY)
		{
			if (messageX.EM_ReceiveTransmit == messageY.EM_ReceiveTransmit)
			{
				yield return CompareInterchangeNums;
				yield return CompareMessageNumber;
			}

			yield return CompareSystemCreateTime;
			yield return CompareBusinessObjectNotInDbCreateSequence;
			yield return CompareApplicationCode;
			yield return CompareReceiveTransmit;
			yield return CompareOtherConditions;
			yield return ComparePK;
		}

		protected virtual int CompareOtherConditions(T messageX, T messageY) => 0;

		protected virtual int CompareInterchangeNums(T messageX, T messageY)
		{
			EDIInterchange messageX_Interchange = messageX.Interchange;
			EDIInterchange messageY_Interchange = messageY.Interchange;

			int result = 0;

			if (messageX_Interchange != null && messageY_Interchange != null)
			{
				result = CompareNumericOrAlphaNumeric(messageX_Interchange.EI_InterchangeNum, messageY_Interchange.EI_InterchangeNum, EDIInterchangeSchema.EI_InterchangeNum.MaxLength);
			}
			else if (messageX_Interchange == null && messageY_Interchange == null)
			{
				result = 0;
			}
			else if (messageX_Interchange == null)
			{
				result = 1;
			}
			else if (messageY_Interchange == null)
			{
				result = -1;
			}

			return result;
		}

		protected int CompareBusinessObjectNotInDbCreateSequence(T messageX, T messageY)
		{
			try
			{
				int result = CompareBusinessObjects(messageX, messageY);
				return result;
			}
			catch (InvalidOperationException)
			{
				return 0;
			}
		}

		int CompareBusinessObjects(T messageX, T messageY)
		{
			int result = 0;

			if (!messageX.IsInDatabase && !messageY.IsInDatabase)
			{
				result = messageX.FactoryNewSequenceNumber.CompareTo(messageY.FactoryNewSequenceNumber);
			}

			return result;
		}
	}

	public class AutoEDIMessageComparer<T> : IComparer<T> where T : AutoEDIMessage
	{
		public AutoEDIMessageComparer(ListSortDirection sortDirection)
		{
			SortDirection = sortDirection;
		}

		readonly ListSortDirection SortDirection;

		public int Compare(T messageX, T messageY)
		{
			int result = 0;

			if (messageX == null && messageY == null)
			{
				result = 0;
			}
			else if (messageX == null)
			{
				result = -1;
			}
			else if (messageY == null)
			{
				result = 1;
			}
			else if (messageX.PK != messageY.PK)
			{
				result = CompareCore(messageX, messageY);
			}

			if (SortDirection == ListSortDirection.Descending)
			{
				result = -result;
			}

			return result;
		}

		int CompareCore(T messageX, T messageY)
		{
			int result = 0;

			foreach (Comparison<T> comparer in GetMessageComparers(messageX, messageY))
			{
				result = comparer(messageX, messageY);

				if (result != 0)
				{
					break;
				}
			}

			return result;
		}

		protected virtual IEnumerable<Comparison<T>> GetMessageComparers(T messageX, T messageY)
		{
			if (messageX.EM_ReceiveTransmit == messageY.EM_ReceiveTransmit)
			{
				yield return CompareMessageNumber;
			}

			yield return CompareSystemCreateTime;
			yield return CompareApplicationCode;
			yield return CompareReceiveTransmit;
			yield return ComparePK;
		}

		protected int CompareMessageNumber(T messageX, T messageY)
		{
			return CompareNumericOrAlphaNumeric(messageX.EM_MessageNum, messageY.EM_MessageNum, EDIMessageSchema.EM_MessageNum.MaxLength);
		}

		protected int CompareSystemCreateTime(T messageX, T messageY)
		{
			int result = 0;

			if (messageX.EM_SystemCreateTimeUtc.IsEmpty && messageY.EM_SystemCreateTimeUtc.IsEmpty)
			{
				result = 0;
			}
			else if (messageX.EM_SystemCreateTimeUtc.IsEmpty)
			{
				result = 1;
			}
			else if (messageY.EM_SystemCreateTimeUtc.IsEmpty)
			{
				result = -1;
			}
			else
			{
				result = messageX.EM_SystemCreateTimeUtc.CompareTo(messageY.EM_SystemCreateTimeUtc);
			}

			return result;
		}

		protected int CompareApplicationCode(T messageX, T messageY)
		{
			return messageX.EM_ApplicationCode.CompareTo(messageY.EM_ApplicationCode);
		}

		protected int CompareReceiveTransmit(T messageX, T messageY)
		{
			return -messageX.EM_ReceiveTransmit.CompareTo(messageY.EM_ReceiveTransmit); // transmit, then receive
		}

		protected int ComparePK(T messageX, T messageY)
		{
			return messageX.PK.CompareTo(messageY.PK);
		}

		protected int CompareNumericOrAlphaNumeric(ZString x, ZString y, int maxLength)
		{
			ZString paddedX = x.PadLeft(maxLength, '0');
			ZString paddedY = y.PadLeft(maxLength, '0');
			return paddedX.CompareTo(paddedY);
		}
	}
}
