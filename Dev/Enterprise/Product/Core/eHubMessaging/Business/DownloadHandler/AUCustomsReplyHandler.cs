using System.IO;
using System.Xml;
using CargoWise.eHub.Common.Extensions;
using Enterprise.eHubMessaging.Business.Extensions;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.AUCustoms)]
	class AUCustomsReplyHandler : MessageHandler
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Xml field name")]
		protected override EDIInterchange CreateInterchange()
		{
			Message.MessageStream.SeekBegin();
			Stream decodedInterchangeStream = null;
			string reference;

			using (var reader = XmlReader.Create(Message.MessageStream))
			{
				reference = reader.GetElementAsString("Reference", "http://cargowise.com/ehub/products/");
				using (var interchangeStream = reader.GetElementAsStream("Content", "http://cargowise.com/ehub/products/"))
				{
					interchangeStream.Position = 0;
					decodedInterchangeStream = interchangeStream.DecodeStream();
					decodedInterchangeStream.Position = 0;
				}
			}

			var interchange = base.CreateInterchange();
			interchange.EI_To = reference;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.AUCMR;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			interchange.SetEI_BodyTextSource(new TextReaderSource(decodedInterchangeStream));
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.AUCustoms;
			return interchange;
		}
	}
}
