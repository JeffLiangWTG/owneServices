using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Shared;
using Newtonsoft.Json;

namespace Enterprise.xTMessaging.Business
{
	public static class Utils
	{
		public static void SetHeaderTextWithAttributeDictionary(this EDIInterchange interchange, IDictionary<string, string> messageAttributes, bool includeNonCustomMsgAttribute = false)
		{
			if (messageAttributes != null)
			{
				var attributesForHeader = messageAttributes
					.Where(kvp =>
					{
						var result = true;
						var attrKey = kvp.Key;
						result &= !attrKey.IsSystemMessageAttribute();
						result &= (includeNonCustomMsgAttribute || attrKey.StartsWith(Constants.CustomMsgAttributePrefix));
						return result;
					}).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
				var headerText = JsonConvert.SerializeObject(attributesForHeader, Formatting.None);
				interchange.EI_HeaderText = headerText;
			}
		}

		public static DateTime GetDeadline(TimeSpan messageTimeout) => ZDateTime.UtcNow.Add(messageTimeout).ToDateTime();
	}
}
