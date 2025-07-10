using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	class EHubMessageBuilderForTelematics : EHubMessageBuilder
	{
		public EHubMessageBuilderForTelematics(EDIInterchange interchange, INotifications notifier)
			: base(interchange, notifier)
		{
		}

		protected override bool FailIfNoSchemaNameFound()
		{
			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "XML element name")]
		protected override IeHubMessage BuildCore()
		{
			var bodyContent = SelectBodyContent()
				.Select(XElement.Parse);

			var interchangeXml = XDocument.Parse((NoResString)"<ns0:TelematicsInterchange xmlns:ns0=\"http://cargowise.com/ehub/products/telematics/2013/05\" />");
			interchangeXml.Root.Add(
				new XElement(
					"Header",
					new XElement("SenderID", interchange.EI_From),
					new XElement("RecipientID", interchange.EI_To)),
				new XElement(
					"Body",
					bodyContent));

			MemoryStream ms = null;
			try
			{
				ms = new MemoryStream();
				var writer = new XmlTextWriter(ms, Encoding.UTF8);
				writer.Formatting = Formatting.Indented;
				interchangeXml.WriteTo(writer);
				writer.Flush();

				ms.Seek(0, SeekOrigin.Begin);

				var hubMessage = new eHubMessage(
					interchange.EI_SessionGUID.ToGuid(),
					interchange.EI_From,
					interchange.EI_To,
					MessageSchemaType.Xml,
					ApplicationCodeList.Codes.Telematics,
					string.Empty,
					ms);

				return hubMessage;
			}
#pragma warning disable ENT0001
			catch
#pragma warning restore ENT0001
			{
				if (ms != null)
				{
					ms.Dispose();
				}

				throw;
			}

			IEnumerable<string> SelectBodyContent()
			{
				if (!string.IsNullOrWhiteSpace(interchange.EI_BodyText))
				{
					return new string[] { interchange.EI_BodyText };
				}

				return interchange
					.ContainedMessages
					.Cast<EDIMessage>()
					.Select(message => (string)message.EM_MessageText);
			}
		}
	}
}
