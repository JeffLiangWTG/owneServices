using System.Collections.Generic;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Shared;

namespace Enterprise.xTMessaging.Business
{
	public static class MetaDataHelperExtensions
	{
		public static bool IsValidForSavingToEDIInterchange(this MetaDataHelper helper)
		{
			return HasValidValue(Constants.CustomMsgAttributes.ApplicationCode) &&
					HasValidValue(Constants.CustomMsgAttributes.MessageTrackingID) &&
					HasValidValue(Constants.CustomMsgAttributes.MessageType) &&
					HasValidValue(Constants.CustomMsgAttributes.SourceParty) &&
					HasValidValue(Constants.CustomMsgAttributes.DestinationParty);

			bool HasValidValue(string key)
			{
				return helper.MetaData.TryGetValue(key, out var readValue) && !string.IsNullOrEmpty(readValue);
			}
		}

		public static void MergeMessageAttributesFromOriginalInterchange(this MetaDataHelper helper, IEDIInterchange interchange)
		{
			var interchangeAttributes = new Dictionary<string, string>();
			interchangeAttributes.Add(Constants.CustomMsgAttributes.ApplicationCode, interchange.EI_ApplicationCode);
			interchangeAttributes.Add(Constants.CustomMsgAttributes.MessageTrackingID, interchange.EI_SessionGUID.ToString());
			interchangeAttributes.Add(Constants.CustomMsgAttributes.SourceParty, interchange.EI_From);
			interchangeAttributes.Add(Constants.CustomMsgAttributes.DestinationParty, interchange.EI_To);
			interchangeAttributes.Add(Constants.CustomMsgAttributes.MessageType, interchange.EI_InterchangeType);

			helper.MergeMessageAttributesFromOriginalInfo(interchangeAttributes);
		}
	}
}
