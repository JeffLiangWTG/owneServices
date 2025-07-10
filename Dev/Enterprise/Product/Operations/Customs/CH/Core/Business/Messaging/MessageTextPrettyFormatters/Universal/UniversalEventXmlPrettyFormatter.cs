using System.Net;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class UniversalEventXmlPrettyFormatter : IMessagePrettyFormatter
{
	internal UniversalEventXmlPrettyFormatter(UniversalEventWrapper universalEventData)
	{
		this.universalEventData = universalEventData;
	}
	readonly UniversalEventWrapper universalEventData;

	public ZString GetFormattedText()
	{
		ZString message = universalEventData?.GetResponseMessage()?.Trim() ?? ZString.Empty;

		if (!message.IsEmpty)
		{
			try
			{
				var xml = XDocument.Parse(message);
				message = xml.ToString().Trim();

				if (!message.StartsWith((NoResString)"<?xml"))
				{
					message = message.Insert(0, (NoResString)"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n");
				}
				else
				{
					const string xmlDeclEndTag = "?>";
					var endOfXmlDecl = message.IndexOf(xmlDeclEndTag);
					if (endOfXmlDecl >= 0 && message.Length > endOfXmlDecl + 2)
					{
						endOfXmlDecl += xmlDeclEndTag.Length;
						if (message.SubstringSafe(endOfXmlDecl).StartsWith("\n"))
						{
							message = message.InsertSafe(endOfXmlDecl, "\r");
						}
						else if (!message.SubstringSafe(endOfXmlDecl).StartsWith("\r\n"))
						{
							message = message.InsertSafe(endOfXmlDecl, "\r\n");
						}
					}
				}
			}
			catch (XmlException e)
			{
				var htmlBuilder = new ZStringBuilder();
				htmlBuilder.Append($"<h2>{WebUtility.HtmlEncode(Title)}</h2>");
				htmlBuilder.Append($"<p>{WebUtility.HtmlEncode(Message)}</p>");
				htmlBuilder.Append($"<p>{WebUtility.HtmlEncode(ReasonLabel)}:{WebUtility.HtmlEncode(e.Message)}</p>");
				return htmlBuilder.ToString();
			}
		}

		return message;
	}

	static string Title => Res.GetString("9939EEA2-2C58-44A4-AD85-A750BF9E0AEE", "The message content cannot be shown");
	static string Message => Res.GetString("66787492-1779-4D91-9490-9A548070A91F", "Please look at Text tab for the raw message response.");
	static string ReasonLabel => Res.GetString("D994C322-F102-4AC3-845C-DDD98D21574E", "Reason");
}
