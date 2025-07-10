using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.MX.Manifest.Business
{
	class MXMessageHelper
	{
		internal static DateTime SafeDateTime(ZDateTime dateTime) => dateTime.IsValid ? Convert.ToDateTime(dateTime.ToString("yyyy-MM-ddTHH:mm:ss")) : default;

		internal static ZString EnvelopeSignedMessage(ZString xMLMessage, ZString username, ZString password, ZString url, ZString messageType)
		{
			if (messageType == MessageTypes.Codes.MXA)
			{
				var messageWithOutXMlDeclaration = xMLMessage.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", ZString.Empty).Replace("<Parametros>", ZString.Empty).Replace("</Parametros>", ZString.Empty);
				messageWithOutXMlDeclaration = messageWithOutXMlDeclaration.Replace("<ISA02 />", "<ISA02>          </ISA02>").Replace("<ISA04 />", "<ISA04>          </ISA04>");

				using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.MX.Manifest.Business.Message.Templates.MXSeaManifestFullRequest.xml"))
				using (StreamReader reader = new StreamReader(stream))
				{
					return reader.ReadToEnd().Replace("{BodyText}", messageWithOutXMlDeclaration).Replace("{Username}", username).Replace("{Password}", password);
				}
			}
			else
			{
				using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.MX.Manifest.Business.Message.Templates.MXAirManifestFullRequest.xml"))
				using (StreamReader reader = new StreamReader(stream))
				{
					return reader.ReadToEnd().Replace("{Username}", username).Replace("{Password}", password).Replace("{URL}", url).Replace("{BodyText}", GetXMLMessageWithEnvelope(xMLMessage));
				}
			}
		}

		static ZString GetXMLMessageWithEnvelope(ZString messageText) => Convert.ToBase64String(GetMessageBinary(messageText));

		static byte[] GetMessageBinary(string dataString)
		{
			var bytes = Encoding.UTF8.GetBytes(dataString);
			using (var cms = new MemoryStream())
			{
				using (var gzip = new GZipStream(cms, CompressionMode.Compress))
				{
					gzip.Write(bytes, 0, bytes.Length);
				}
				return cms.ToArray();
			}
		}

		internal static ZString GetInterchangeMessageTo(ZString messageType, ZBool isTestMode)
		{
			switch (messageType)
			{
				case MessageTypes.Codes.MXA:
				case MessageTypes.Codes.MXD:
				case MXMessageConstants.XER:
					return isTestMode ? (ZString)MXMessageConstants.MXCustomsForSeaModeTesting : (ZString)MXMessageConstants.MXCustomsForSeaMode;
				case MessageTypes.Codes.MXE:
				case MessageTypes.Codes.MXF:
				case MessageTypes.Codes.MXG:
					return MXMessageConstants.MXCustomsForAirMode;
				default:
					return ZString.Empty;
			}
		}

		internal static IMessageAttachee FindRelevantBusinessObjectFirstSeaResponse(ZString message, EDIInterchange interchange)
		{
			AsycudaBill bill = null;
			var response = CargoWise.Customs.MX.MessageContracts.MXHelper.GetSeaFirstResponseInformation(message, ZString.Empty);
			if (response != null)
			{
				var messageReference = response.Isa13;
				bill = MXMessageProcessorHelper.LookForAsycudaBill(interchange.Factory, messageReference);
			}
			return bill;
		}

		internal static IMessageAttachee FindRelevantBusinessObjectFinalSeaResponse(ZString message, EDIInterchange interchange)
		{
			AsycudaBill bill = null;
			var response = CargoWise.Customs.MX.MessageContracts.MXHelper.GetSeaFinalResponseInformation(message);
			if (response != null)
			{
				var messageReference = response.Isa13;
				bill = MXMessageProcessorHelper.LookForAsycudaBill(interchange.Factory, messageReference);
			}
			return bill;
		}
	}
}
