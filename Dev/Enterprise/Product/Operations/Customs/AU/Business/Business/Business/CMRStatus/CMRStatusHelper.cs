using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class CMRStatusHelper
	{
		public static bool NoMessageIsCurrent(ZString msgStatusCode)
		{
			return (msgStatusCode.IsEmpty ||
							msgStatusCode == CMRBaseStatuses.Codes.WithdrawalAccepted ||
							msgStatusCode == CMRBaseStatuses.Codes.NotSent ||
							msgStatusCode == CMRBaseStatuses.Codes.OriginalRejected);
		}

		public static bool CanDelete(ZString msgStatusCode)
		{
			return (NoMessageIsCurrent(msgStatusCode) || !(new CMRBaseStatuses().ContainsCode(msgStatusCode)));
		}

		public static bool NoMessagesSent(ZString statusCode)
		{
			return statusCode.IsEmpty || statusCode == CMRBaseStatuses.Codes.NotSent;
		}

		public static CodeDescriptionPairList GetAcceptableStatusesForKeyValueChange(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("CMRAcceptableStatusesForKeyValueChange", GetMessagingBeingNotLodgedList);
		}

		static CodeDescriptionPairList GetMessagingBeingNotLodgedList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CMRBaseStatuses.Codes.WithdrawalAccepted, CMRBaseStatuses.Descriptions.WithdrawalAccepted);
			result.AddPair(CMRBaseStatuses.Codes.OriginalRejected, CMRBaseStatuses.Descriptions.OriginalRejected);
			result.AddPair(CMRBaseStatuses.Codes.NotSent, CMRBaseStatuses.Descriptions.NotSent);
			return result;
		}

		public static bool IsAcceptableStatusesForKeyValueChange(BusinessObjectFactory factory, string status)
		{
			return string.IsNullOrEmpty(status) || GetAcceptableStatusesForKeyValueChange(factory).ContainsCode(status);
		}

		public static bool IsMessagingBeingNotLodged(string status)
		{
			return string.IsNullOrEmpty(status) || GetMessagingBeingNotLodgedList().ContainsCode(status);
		}
	}
}
