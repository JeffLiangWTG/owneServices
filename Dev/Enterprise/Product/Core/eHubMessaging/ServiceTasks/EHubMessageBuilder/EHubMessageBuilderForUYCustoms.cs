using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.IO;
using Enterprise.Messaging.Business;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public class EHubMessageBuilderForUYCustoms : EHubMessageBuilder
	{
		public EHubMessageBuilderForUYCustoms(EDIInterchange interchange, INotifications notifier)
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

		protected override string GetSchemaName()
		{
			return EDIMessageSchemaNameList.Descriptions.UYCustomsEnvelope;
		}

		protected Stream GetMessageStream()
		{
			var namespaceManager = new XmlNamespaceManager(new NameTable());
			namespaceManager.AddNamespace("ns0", "http://cargowise.com/xhub/products/UYCustoms");
			Stream stream = new VirtualMemoryStream();
			var headers = ExtractHeaders(interchange.EI_HeaderText);
			var xmlMessage = FormattableString.Invariant($@"
			<UYCustomsEnvelope xmlns=""http://cargowise.com/xhub/products/UYCustoms"">
				<Credentials>
					<UserName>{headers[UYCustomsConstants.UserName]}</UserName>
					<Password>{headers[UYCustomsConstants.Password]}</Password>
				</Credentials>
				<InboxPK/>
				<OutboxPK/>
				<MessageTrackingID/>
				<Sender>{interchange.EI_From}</Sender>
				<Recipient>{interchange.EI_To}</Recipient>
				<MessageBodyBase64>{Base64Encode(interchange.EI_BodyText)}</MessageBodyBase64>
			</UYCustomsEnvelope>
			");
			var doc = XDocument.Parse(xmlMessage, LoadOptions.None);
			doc.Save(stream, SaveOptions.OmitDuplicateNamespaces);
			return stream;
		}

		Dictionary<string, string> ExtractHeaders(string header)
		{
			var doc = XDocument.Parse(header);
			return new Dictionary<string, string>()
			{
				{ UYCustomsConstants.UserName, doc.XPathSelectElement("//Credentials/UserName").Value },
				{ UYCustomsConstants.Password, doc.XPathSelectElement("//Credentials/Password").Value }
			};
		}

		string Base64Encode(string text)
		{
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
		}
	}

	public sealed class UYCustomsConstants
	{
		public const string UserName = nameof(UserName);
		public const string Password = nameof(Password);

		UYCustomsConstants()
		{ }
	}
}
