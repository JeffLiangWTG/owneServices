using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.Business
{
	class AcknowledgementXPathProvider
	{
		static internal string GetInterchangeXPath(string interchangeType)
		{
			switch (interchangeType)
			{
				case EDIInterchangeTypeList.Codes.XDC: return UniversalInterchangeHeaderHandler.InterchangeAcknowledgementXPath;
				case EDIInterchangeTypeList.Codes.XMS: return XmlInterchangeHandler.InterchangeAcknowledgementXPath;
				default: return string.Empty;
			}
		}
	}
}
