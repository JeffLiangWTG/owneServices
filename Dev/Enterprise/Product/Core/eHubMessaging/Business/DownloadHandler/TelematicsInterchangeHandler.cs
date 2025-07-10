using System;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.Telematics)]
	class TelematicsInterchangeHandler : XmlMessageHandler
	{
		protected override string GetApplicationCode(string payloadTypeName)
		{
			return ApplicationCodeList.Codes.Telematics;
		}

		protected override string GetInterchangeType()
		{
			return EDIInterchangeTypeList.Codes.Telematics;
		}

		protected override string GetMessageType()
		{
			return EDIMessageTypeList.Codes.XDC;
		}

		protected override string GetMessageSubType(string payloadTypeName, string payloadSubTypeName)
		{
			switch (payloadTypeName)
			{
				case nameof(TelematicsMessageList.Descriptions.ProtobufData):
					return TelematicsMessageList.Codes.ProtobufData;
				case nameof(TelematicsMessageList.Descriptions.TelematicsXmlData):
					return TelematicsMessageList.Codes.TelematicsXmlData;
				default:
					return EDIMessageSubTypeList.Codes.Unknown;
			}
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
				payloadTypeNodes.Add((NoResString)"/*[local-name()='TelematicsInterchange']/*[local-name()='Body']");
			}
			return payloadTypeNodes;
		}

		[ThreadStatic]
		static XPathCollection payloadTypeNodes;

		protected override void ReadToMessageStartElement(XPathReader reader)
		{
		}
	}
}
