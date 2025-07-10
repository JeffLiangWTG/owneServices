using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.NCTS
{
	public class NctsMessageBuilder : INctsNativeBuilder
	{
		public ZString NativeMessage<T>(T header, NctsMessageFunctionSet messageFunction, ErrorCollector errorCollector)
		{
			var gbNctsHeader = header as Business.NctsHeader;
			ZString message = ZString.Empty;

			if (CheckForValidAccessToken(gbNctsHeader, errorCollector))
			{
				message = GetCTCBuilderXml(gbNctsHeader, messageFunction, errorCollector);
			}

			return message;
		}

		ZString GetCTCBuilderXml(Business.NctsHeader header, NctsMessageFunctionSet messageFunction, ErrorCollector errorCollector)
		{
			var messageBuilder = GetCTCXmlBuilder(header, messageFunction, errorCollector);
			var xmlMessage = ZString.Empty;
			if (messageBuilder != null)
			{
				xmlMessage = messageBuilder.GetXMLMessageWithoutNamespaces();
			}

			return xmlMessage;
		}

		protected bool CheckForValidAccessToken(Business.NctsHeader header, ErrorCollector errorCollector)
		{
			var hasToken = header.HasValidAccessToken();

			if (!hasToken)
			{
				errorCollector.AddError(NoAccessTokenError);
			}

			return hasToken;
		}
		public string NoAccessTokenError = "No valid access token for NCTS could be found. Please check the current company's Brokerage tab to ensure that valid token data exists. If a token is currently being refreshed then this is likely only a transient problem and you may retry once the token shows as valid again. Please refer to eLearning unit 1BGB054.";

		protected CTC.Interfaces.IXmlMessageBuilder GetCTCXmlBuilder(Business.NctsHeader header, NctsMessageFunctionSet messageFunction, ErrorCollector errorCollector)
		{
			if (messageFunction is NctsMessageFunctionSet.DeclarationDataMessage)
			{
				var wrapper = new CC015BDeclarationWrapper(header);
				return new CC015BXmlMessageBuilder(wrapper, errorCollector);
			}
			else if (messageFunction is NctsMessageFunctionSet.ArrivalNotificationMessage)
			{
				var wrapper = new CC007ADeclarationWrapper(header);
				return new CC007AXmlMessageBuilder(wrapper, errorCollector);
			}
			else if (messageFunction is NctsMessageFunctionSet.UnloadingRemarksMessage)
			{
				var wrapperActual = new CC044ADeclarationWrapperActual(header);
				var wrapperExpected = new CC044ADeclarationWrapperExpected(header);
				return new CC044AXmlMessageBuilder(wrapperExpected, wrapperActual, errorCollector);
			}
			else if (messageFunction is NctsMessageFunctionSet.DeclarationCancellationRequestMessage)
			{
				var wrapper = new CC014ADeclarationWrapper(header);
				return new CC014AXmlMessageBuilder(wrapper, errorCollector);
			}
			else
			{
				return null;
			}
		}
	}
}
