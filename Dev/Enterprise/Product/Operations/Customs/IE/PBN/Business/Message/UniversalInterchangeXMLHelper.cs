using System.Linq;
using System.Xml;

namespace Enterprise.Customs.IE.PBN.Business;

public static class UniversalInterchangeXMLHelper
{
	public static string TryGetReasonNodeFromUniversalInterchangeTypeXML(string messageText)
	{
		string reasonNodeValue;
		try
		{
			reasonNodeValue = GetReasonNodeFromUniversalInterchangeTypeXML(messageText);
		}
		catch
		{
			reasonNodeValue = null;
		}
		return reasonNodeValue;
	}
	static string GetReasonNodeFromUniversalInterchangeTypeXML(string messageText)
	{
		const string reasonNodeName = "UniversalInterchange/Body/UniversalEvent/Event/EventParameters/Reason";
		var xPathReason = ConvertToXPath(reasonNodeName);
		var xmldoc = new XmlDocument();
		xmldoc.LoadXml(messageText);

		return xmldoc.SelectSingleNode(xPathReason)?.InnerXml.Trim();
	}

	static string ConvertToXPath(string xmlPath)
	{
		var pathsSplit = xmlPath.Split('/');
		return "//" + string.Join("/", pathsSplit.Select(s => $"*[local-name()='{s}']"));
	}
}
