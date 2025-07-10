using System;
using System.Net;
using CargoWise.Types;
using Enterprise.xTMessaging.Business;

namespace Enterprise.Customs.CH.Business;

class UniversalEventPrettyFormatter : IMessagePrettyFormatter
{
	internal UniversalEventPrettyFormatter(UniversalEventWrapper universalEventData)
	{
		this.universalEventData = universalEventData;
	}
	readonly UniversalEventWrapper universalEventData;

	public ZString GetFormattedText()
	{
		if (string.IsNullOrEmpty(universalEventData.Reason))
		{
			return FormattableString.Invariant($"<h2>{MessagePrettyFormatterHelper.MessageIsEmpty}</h2>");
		}

		var htmlBuilder = new ZStringBuilder();

		htmlBuilder.Append($"<h2>{WebUtility.HtmlEncode(Title)}</h2>");
		htmlBuilder.Append($"<p>{WebUtility.HtmlEncode(ReasonLabel)}:</p>");
		htmlBuilder.Append($"<p>{WebUtility.HtmlEncode(universalEventData.Reason).Replace("\n", "<br/>")}</p>");
		return htmlBuilder.ToString();
	}

	static string Title => Res.GetString("7FD3C202-2A13-4008-8DFB-8E2FA88D99B6", "Send Failure");
	static string ReasonLabel => Res.GetString("C21699C4-01B3-4D80-B98B-03FE388308AA", "Reason");
}
