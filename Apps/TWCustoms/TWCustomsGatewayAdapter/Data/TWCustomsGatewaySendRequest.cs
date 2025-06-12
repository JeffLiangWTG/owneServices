using System;
using System.Xml.Serialization;

namespace CargoWise.eHub.Products.TWCustoms.TWCustomsGatewayAdapter.Data
{
	[Serializable]
	[XmlType(AnonymousType = true, Namespace = "http://cargowise.com/ehub/products/TWCPluginRequest")]
	[XmlRoot(Namespace = "http://cargowise.com/ehub/products/TWCPluginRequest", IsNullable = false, ElementName = "TWCPluginServiceSendRequest")]
	public class TWCustomsGatewaySendRequest : ITWCustomsRequest
	{
		[XmlArray("attachments")]
		[XmlArrayItem("attachment", typeof(TWCustomsSendRequestAttachment))]
		public TWCustomsSendRequestAttachment[] Attachments { get; set; }

		[XmlElement(ElementName = "clientRegistrationId")]
		public string ClientRegistrationId { get; set; }

		[XmlElement(ElementName = "entryNumber")]
		public string EntryNumber { get; set; }

		[XmlElement(ElementName = "entryNumberType")]
		public string EntryNumberType { get; set; }

		[XmlElement(ElementName = "mailbox")]
		public string Mailbox { get; set; }

		[XmlElement(ElementName = "messageBodyBase64")]
		public string MessageBodyBase64 { get; set; }

		[XmlElement(ElementName = "messageFormat")]
		public string MessageFormat { get; set; }

		[XmlElement(ElementName = "messageId")]
		public string MessageId { get; set; }

		[XmlElement(ElementName = "interchangeNum")]
		public string InterchangeNum { get; set; }

		[XmlElement(ElementName = "messageType")]
		public string MessageType { get; set; }

		[XmlElement(ElementName = "registrationConfiguration")]
		public string RegistrationConfiguration { get; set; }

		[XmlElement(ElementName = "staffCode")]
		public string StaffCode { get; set; }

		[XmlElement(ElementName = "systemId")]
		public string SystemId { get; set; }

		[XmlElement(ElementName = "companyId")]
		public string CompanyId { get; set; }

		[XmlElement(ElementName = "passwordType")]
		public string PasswordType { get; set; }

		public class TWCustomsSendRequestAttachment
		{
			[XmlElement(ElementName = "attachmentDataBase64")]
			public string AttachmentDataBase64 { get; set; }

			[XmlElement(ElementName = "attachmentFileType")]
			public string AttachmentFileType { get; set; }

			[XmlElement(ElementName = "attachmentName")]
			public string AttachmentName { get; set; }
		}
	}
}