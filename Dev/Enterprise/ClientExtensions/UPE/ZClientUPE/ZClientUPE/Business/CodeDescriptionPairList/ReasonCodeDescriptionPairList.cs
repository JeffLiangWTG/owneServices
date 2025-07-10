using System;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Business
{
	public class ReasonCodeDescriptionPairList : AutoReasonCodeDescriptionPairList
	{
		public static ReasonCodeDescriptionPairList GetReasonList(DefaultQueueCodeDescriptionPairList queueList)
		{
			ReasonCodeDescriptionPairList result = new ReasonCodeDescriptionPairList();
			result.Clear();

			foreach (CodeDescriptionPair queue in queueList)
			{
				result.AddRange(GetReasonList(queueList, queue.Code));
			}

			return result;
		}

		public static ReasonCodeDescriptionPairList GetReasonList(DefaultQueueCodeDescriptionPairList queueList, string queueName)
		{
			string queueCode = GetQueueCodeFromQueueList(queueList) + queueName;

			ReasonCodeDescriptionPairList result = new ReasonCodeDescriptionPairList();
			result.Clear();

			Type reasonSubListType = typeof(AutoReasonCodeDescriptionPairList).GetNestedType(queueCode);
			if (reasonSubListType != null)
			{
				CodeDescriptionPairList reasonSubList = (CodeDescriptionPairList)Activator.CreateInstance(reasonSubListType);
				result.AddRange(reasonSubList);
			}
			return result;
		}

		#region Implementation

		static string GetQueueCodeFromQueueList(DefaultQueueCodeDescriptionPairList queueList)
		{
			string result = "";

			if (queueList is CommercialQueueCodeDescriptionPairList)
			{
				result = "C";
			}
			else if (queueList is CargoReportQueueCodeDescriptionPairList)
			{
				result = "A";
			}
			else if (queueList is DeclarationQueueCodeDescriptionPairList)
			{
				result = "D";
			}

			return result;
		}

		#endregion
	}
}
