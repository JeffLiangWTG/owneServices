using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
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
	public class EHubMessageBuilderForTWCustoms : EHubMessageBuilder
	{
		public EHubMessageBuilderForTWCustoms(EDIInterchange interchange, INotifications notifier)
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
			return "http://cargowise.com/ehub/products/TWCPluginRequest#TWCPluginServiceSendRequest";
		}

		protected Stream GetMessageStream()
		{
			var namespaceManager = new XmlNamespaceManager(new NameTable());
			namespaceManager.AddNamespace("ns0", "http://cargowise.com/ehub/products/TWCPluginRequest");
			Stream stream = new VirtualMemoryStream();
			var headers = ExtractHeaders(interchange.EI_HeaderText);
			var xmlMessage = $@"
			<TWCPluginServiceSendRequest xmlns=""http://cargowise.com/ehub/products/TWCPluginRequest"">
				<systemId>{ExtractSystemID(interchange.EI_From)}</systemId>
				<companyId>{headers[TWCustomsConstants.CompanyID]}</companyId>
				<staffCode>{headers[TWCustomsConstants.StaffCode]}</staffCode>
				<mailbox>{headers[TWCustomsConstants.MailBox]}</mailbox>
				<passwordType>{headers[TWCustomsConstants.PasswordType]}</passwordType>
				<messageType>{headers[TWCustomsConstants.MessageType]}</messageType>
				<entryNumber>{headers[TWCustomsConstants.EntryNumber]}</entryNumber>
				<interchangeNum>{string.Format(CultureInfo.InvariantCulture, interchange.EI_InterchangeNum)}</interchangeNum>
				<entryNumberType>{headers[TWCustomsConstants.EntryNumberType]}</entryNumberType>
				<messageFormat>{GetMessageFormat(interchange.EI_BodyText)}</messageFormat>
				<messageId/>
				<messageBodyBase64>{Base64Encode(interchange.EI_BodyData)}</messageBodyBase64>
				<attachments/>
				<clientRegistrationId/> 
				<registrationConfiguration/> 
			</TWCPluginServiceSendRequest>
			";
			var doc = XDocument.Parse(xmlMessage, LoadOptions.None);
			var attachments = doc.XPathSelectElement("//ns0:attachments", namespaceManager);
			var message = interchange.ContainedMessages[0];
			foreach (EDIMessageAttach attachment in message.MessageAttachments)
			{
				var storageDocs = attachment.GetAttachment();
				if (storageDocs != null)
				{
					var attachmentXml = $@"
						<attachment xmlns=""http://cargowise.com/ehub/products/TWCPluginRequest"">
							<attachmentName>{ExtractFileName(attachment.EG_FileName)}</attachmentName>
            					<attachmentDataBase64>{Base64Encode(storageDocs.ImageData)}</attachmentDataBase64>
            					<attachmentFileType>{ExtractFileType(attachment.EG_FileName)}</attachmentFileType>
						</attachment>
						";
					attachments.Add(XElement.Parse(attachmentXml, LoadOptions.None));
				}
			}
			doc.Save(stream, SaveOptions.OmitDuplicateNamespaces);
			return stream;
		}

		Dictionary<string, string> ExtractHeaders(string header)
		{
			var doc = XDocument.Parse(header);
			var staffCodeValue = string.Empty;
			if (doc.XPathSelectElement("//StaffCode") != null)
			{
				staffCodeValue = doc.XPathSelectElement("//StaffCode").Value;
			}

			return new Dictionary<string, string>()
			{
				{ TWCustomsConstants.CompanyID, doc.XPathSelectElement("//CompanyID").Value },
				{ TWCustomsConstants.StaffCode, staffCodeValue },
				{ TWCustomsConstants.MailBox, doc.XPathSelectElement("//MailBox").Value },
				{ TWCustomsConstants.PasswordType, doc.XPathSelectElement("//PasswordType").Value },
				{ TWCustomsConstants.MessageType, doc.XPathSelectElement("//MessageType").Value },
				{ TWCustomsConstants.EntryNumber, doc.XPathSelectElement("//EntryNumber").Value },
				{ TWCustomsConstants.EntryNumberType, doc.XPathSelectElement("//EntryNumberType").Value }
			};
		}

		string ExtractSystemID(string clientID)
		{
			return Regex.Replace(clientID, "(.{3}).{3}(.{3})", "$1$2");
		}

		string ExtractFileName(string name)
		{
			return name.Split(new[] { '.' })[0];
		}

		string ExtractFileType(string name)
		{
			return name.Split(new[] { '.' })[1];
		}

		string Base64Encode(byte[] blob)
		{
			return Convert.ToBase64String(blob);
		}

		string GetMessageFormat(string textMessage)
		{
			return Regex.Match(textMessage, ".*?urn:wco:datamodel:TW:(.*?):.*").Groups?[1].Value;
		}
	}

	public sealed class TWCustomsConstants
	{
		public const string SystemID = "SystemID";
		public const string CompanyID = "CompanyID";
		public const string StaffCode = "StaffCode";
		public const string MailBox = "MailBox";
		public const string PasswordType = "PasswordType";
		public const string MessageType = "MessageType";
		public const string EntryNumber = "EntryNumber";
		public const string EntryNumberType = "EntryNumberType";

		TWCustomsConstants()
		{ }
	}
}
