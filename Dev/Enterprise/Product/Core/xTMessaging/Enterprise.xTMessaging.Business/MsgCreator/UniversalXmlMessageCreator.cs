using System;
using Enterprise.eHubMessaging.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;

namespace Enterprise.xTMessaging.Business
{
	public class UniversalXmlMessageCreator : XMLMessageCreator
	{
		public UniversalXmlMessageCreator(EDIInterchange interchange, ILogger logger) : base(interchange, logger) { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected override string GetApplicationCode(string payloadTypeName)
		{
			switch (payloadTypeName)
			{
				case "Native":
					return ApplicationCodeList.Codes.NativeDataMessaging;
				default:
					return ApplicationCodeList.Codes.UniversalDataMessaging;
			}
		}
		protected override string GetMessageType(string payloadTypeName, string payloadSubTypeName) => EDIMessageTypeList.Codes.XDC;

		protected override string GetMessageSubType(string payloadTypeName, string payloadSubTypeName)
		{
			return XmlMessageHelper.GetMessageSubType(payloadTypeName, payloadSubTypeName);
		}

		protected override bool CheckSupportedType(string applicationCode, string incomingTag)
		{
			if (applicationCode == EDIInterchange.ApplicationCodes.UniversalDataMessaging)
			{
				return !EdiMessageTags.UniversalMessage.RootTags.IsRequestTag(incomingTag);
			}
			return true;
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected override XPathCollection XPathsForPayload()
		{
			if (xpathsForPayload == null)
			{
				xpathsForPayload = new XPathCollection()
				{
					"/*[local-name()='UniversalInterchange']/*[local-name()='Body']"
				};
			}
			return xpathsForPayload;
		}
		[ThreadStatic]
		static XPathCollection xpathsForPayload;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected override XPathCollection XPathsForPayloadSubType()
		{
			if (xpathsForPayloadSubType == null)
			{
				xpathsForPayloadSubType = new XPathCollection()
				{
					"/*[local-name()='ReferenceData']/*[local-name()='Body']/*",
					"/*[local-name()='Native']/*[local-name()='Body']/*",
					"/*[local-name()='Body']/*[local-name()='ReadRexResponse']/*",
					"/*[local-name()='Body']/*[local-name()='CMD']/*"
				};
			}
			return xpathsForPayloadSubType;
		}
		[ThreadStatic]
		static XPathCollection xpathsForPayloadSubType;
	}
}
