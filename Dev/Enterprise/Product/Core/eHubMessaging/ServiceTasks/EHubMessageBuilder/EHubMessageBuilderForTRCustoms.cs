using System;
using System.IO;
using System.Xml.Linq;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.IO;
using Enterprise.Messaging.Business;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public class EHubMessageBuilderForTRCustoms : EHubMessageBuilder
	{
		public EHubMessageBuilderForTRCustoms(EDIInterchange interchange, INotifications notifier)
			: base(interchange, notifier)
		{
		}

		protected override IeHubMessage BuildCore()
		{
			return new eHubMessage(
				interchange.EI_SessionGUID.ToGuid(),
				interchange.EI_From,
				interchange.EI_To,
				MessageSchemaType.Xml,
				interchange.EI_ApplicationCode,
				schemaName,
				GetMessageStream());
		}

		protected Stream GetMessageStream()
		{
			const string ns = "http://cargowise.com/xhub/products/TRCustoms";

			var xmlMessage = new XDocument();

			var envelope = new XElement(XName.Get("TRCustomsEnvelope", ns));
			xmlMessage.Add(envelope);

			var messageBody = new XElement(XName.Get("MessageBodyBase64", ns));
			envelope.Add(messageBody);
			messageBody.Add(new XText(Convert.ToBase64String(interchange.EI_BodyData)));

			Stream stream = new VirtualMemoryStream();
			xmlMessage.Save(stream);
			return stream;
		}

		void SetEmptyNamespaceTo(XElement el, string ns)
		{
			if (el.Name.Namespace == string.Empty)
			{
				el.Name = XName.Get(el.Name.LocalName, ns);
			}

			foreach (var child in el.Elements())
			{
				if (child is XElement childElement)
				{
					SetEmptyNamespaceTo(childElement, ns);
				}
			}
		}
	}

	public static class TRCustomsConstants
	{
		public const string UserName = nameof(UserName);
		public const string Password = nameof(Password);
	}
}
