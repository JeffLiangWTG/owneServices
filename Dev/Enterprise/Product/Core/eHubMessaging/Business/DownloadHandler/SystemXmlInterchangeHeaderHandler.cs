using System;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.SYS)]
	class SystemXmlInterchangeHeaderHandler : XmlMessageHandler
	{
		protected override string GetApplicationCode(string payloadTypeName)
		{
			return SystemMessage.ApplicationCode;
		}

		protected override string GetInterchangeType()
		{
			return SystemMessage.InterchangeType;
		}

		protected override string GetMessageType()
		{
			return SystemMessage.MessageType;
		}

		protected override string GetMessageSubType(string payloadTypeName, string payloadSubTypeName)
		{
			string result = new SystemMessageList().GetCodeFromDescription(payloadTypeName);
			return !string.IsNullOrEmpty(result) ? result : EDIMessageSubTypeList.Codes.Unknown;
		}

		protected override string GetPayloadSubType()
		{
			return string.Empty;
		}

		protected override XPathCollection GetPayloadTypeNodes()
		{
			if (payloadTypeNodes == null)
			{
				payloadTypeNodes = new XPathCollection();
				payloadTypeNodes.Add(SystemMessage.InterchangeBodyXpathQuery);
			}
			return payloadTypeNodes;
		}
		[ThreadStatic]
		static XPathCollection payloadTypeNodes;

		protected override void ReadToMessageStartElement(XPathReader reader)
		{
		}

		protected override EDIInterchange CreateInterchange()
		{
			var result = base.CreateInterchange();
			result.EI_BodyText = ZString.Empty;
			result.NumberStrategy = new SystemMessage.InterchangeNumberStrategy(result.Factory);
			return result;
		}

		protected override EDIMessage CreateEDIMessage(EDIInterchange interchange)
		{
			var result = base.CreateEDIMessage(interchange);
			result.MessageNumberStrategy = new SystemMessage.MessageNumberStrategy(result.Factory);
			return result;
		}
	}
}