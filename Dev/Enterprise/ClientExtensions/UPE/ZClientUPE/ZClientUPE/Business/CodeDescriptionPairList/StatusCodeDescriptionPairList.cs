using System;
using CargoWise.Types;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Business
{
	public class StatusCodeDescriptionPairList : AutoStatusCodeDescriptionPairList
	{
		public const string EmptyStatus = "__";

		public static StatusCodeDescriptionPairList GetStatusList(string reasonCode)
		{
			StatusCodeDescriptionPairList result = GetStatusList("", reasonCode);
			if (result.Count > 0)
			{
				result.Insert(0, new CodeDescriptionPair(StatusCodeDescriptionPairList.EmptyStatus, "No Status"));
			}
			return result;
		}

		public static StatusCodeDescriptionPairList GetStatusList(string queueName, string reasonCode)
		{
			StatusCodeDescriptionPairList result = GetSpecialCasesStatusList(queueName, reasonCode);
			if (result == null)
			{
				result = new StatusCodeDescriptionPairList();
				result.Clear();

				Type statusSubListType = typeof(AutoStatusCodeDescriptionPairList).GetNestedType(reasonCode);
				if (statusSubListType != null)
				{
					CodeDescriptionPairList statusSubList = (CodeDescriptionPairList)Activator.CreateInstance(statusSubListType);
					result.AddRange(statusSubList);
				}
			}

			if (result.Count > 0)
			{
				result.Insert(0, new CodeDescriptionPair(StatusCodeDescriptionPairList.EmptyStatus, "No Status"));
			}

			return result;
		}

		#region Implementation

		static StatusCodeDescriptionPairList GetSpecialCasesStatusList(ZString queueName, ZString reasonCode)
		{
			StatusCodeDescriptionPairList result = null;

			if (!queueName.IsEmpty)
			{
				if (reasonCode == ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment &&
	(queueName == CommercialQueueCodeDescriptionPairList.Codes.Finance || queueName == CommercialQueueCodeDescriptionPairList.Codes.AR ||
	queueName == CommercialQueueCodeDescriptionPairList.Codes.EIR))
				{
					result = GetEmptyStatusCodeDescriptionPairList();
				}
				else if (reasonCode == ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration)
				{
					if (queueName == DeclarationQueueCodeDescriptionPairList.Codes.Submitted)
					{
						result = GetEmptyStatusCodeDescriptionPairList();
						result.AddPair(StatusCodeDescriptionPairList.Codes.Y1_DocumentsToCustomsAQIS, StatusCodeDescriptionPairList.Descriptions.Y1_DocumentsToCustomsAQIS);
					}
					else if (queueName == DeclarationQueueCodeDescriptionPairList.Codes.Classification || queueName == DeclarationQueueCodeDescriptionPairList.Codes.Lodgement)
					{
						result = GetEmptyStatusCodeDescriptionPairList();
					}
				}
			}

			return result;
		}

		static StatusCodeDescriptionPairList GetEmptyStatusCodeDescriptionPairList()
		{
			StatusCodeDescriptionPairList result = new StatusCodeDescriptionPairList();
			result.Clear();
			return result;
		}

		#endregion
	}
}
