using System;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Common.Extensions;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.TWCustomsDeliveryNotification)]
	public class TWCustomsDeliveryResponseHandler : MessageHandler
	{
		readonly string _byteOrderMarkUtf32 =
			Encoding.UTF32.GetString(Encoding.UTF32.GetPreamble());
		protected override EDIInterchange CreateInterchange()
		{
			var interchange = base.CreateInterchange();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.TWCustoms;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;

			Message.MessageStream.SeekBegin();

			var doc = XDocument.Load(Message.MessageStream, LoadOptions.None);
			var interchangeType = doc.XPathSelectElement((NoResString)"//*[local-name()='ContextCollection']/*[local-name()='Context'][./*[local-name()='Type']='MessageType']/*[local-name()='Value']")?.Value;
			interchange.EI_InterchangeType = interchangeType ?? EDIInterchangeTypeList.Codes.TWCustomsDeliveryNotification;

			interchange.SetEI_BodyTextSource(new StreamReaderSource(Message.MessageStream));

			if (interchange.EI_BodyText.StartsWith(_byteOrderMarkUtf32, StringComparison.Ordinal))
			{
				interchange.EI_BodyText = interchange.EI_BodyText.Remove(0, _byteOrderMarkUtf32.Length);
			}

			return interchange;
		}
	}
}
