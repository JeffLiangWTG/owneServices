using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageStructure;

namespace Enterprise.Customs.IT.Messaging;

public static class CustomsMessageExtensions
{
	public static ZString SerializeWithTabSeparator(this IEnumerable<ISadCustomsMessage> customsMessages)
	{
		Argument.NotNull(customsMessages, nameof(customsMessages));

		var stringBuilder = new ZStringBuilder();
		foreach (var customsMessage in customsMessages)
		{
			stringBuilder.Append(customsMessage.Serialize());
		}
		return stringBuilder.ToString();
	}
}
