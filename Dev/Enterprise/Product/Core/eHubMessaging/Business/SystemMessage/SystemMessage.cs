using Enterprise.ZArchitecture.Core.Encryption;

namespace Enterprise.eHubMessaging.Business
{
	using System;
	using System.IO;
	using System.Text;
	using System.Xml;
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Messaging.Business;
	using Enterprise.Messaging.Integration;
	using Enterprise.Registry.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.Xml;

	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public class SystemMessage : IOutgoingSystemMessage
	{
		public static readonly string SchemaName = EDIInterchangeTypeList.Descriptions.SYS.Substring(0, EDIInterchangeTypeList.Descriptions.SYS.IndexOf('#'));
		public const string ApplicationCode = ApplicationCodeList.Codes.SYS;
		public const string InterchangeType = EDIInterchangeTypeList.Codes.SYS;
		public const string MessageType = EDIMessageTypeList.Codes.XMS;
		const string ZipFileName = "SystemMessage.xml";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "It's an XPath Query so not translated")]
		internal const string InterchangeBodyXpathQuery = "/*[local-name()='SystemInterchange']/*[local-name()='Body']";

		void IOutgoingSystemMessage.Create(BusinessObjectFactory factory, string xmlMessageBody, string recipientId)
		{
			SystemMessage.CreateInterchange(factory, xmlMessageBody, recipientId);
		}

		void IOutgoingSystemMessage.CreateSecure(BusinessObjectFactory factory, string messageName, Stream inputStream, string recipientId)
		{
			SystemMessage.CreateSecureInterchange(factory, messageName, inputStream, recipientId);
		}

		#region Create Interchange

		public static EDIInterchange CreateSecureInterchange(BusinessObjectFactory factory, string messageName, ZXmlSerializer serializer, object valueObject, string recipientId = null)
		{
			using (MemoryStream xmlStream = new MemoryStream())
			{
				serializer.Serialize(xmlStream, valueObject);
				xmlStream.Position = 0;

				return CreateSecureInterchange(factory, messageName, xmlStream, recipientId);
			}
		}

		public static EDIInterchange CreateSecureInterchange(BusinessObjectFactory factory, string messageName, Stream inputStream, string recipientId = null)
		{
			recipientId = recipientId ?? SystemDataRegistry.Instance.EdiProdLicenceIdentifier.Value;
			var interchange = CreateInterchange(factory, recipientId);
			string messageText = Pack(inputStream, ZipFileName);
			interchange.EI_BodyText = CreateSecureInterchangeXml(recipientId, messageName, messageText);
			return interchange;
		}

		public static EDIInterchange CreateInterchange(BusinessObjectFactory factory, string xmlMessageBody, string recipientId = null)
		{
			recipientId = recipientId ?? SystemDataRegistry.Instance.EdiProdLicenceIdentifier.Value;
			var interchange = CreateInterchange(factory, recipientId);
			interchange.EI_BodyText = CreateInterchangeXml(recipientId, xmlMessageBody);
			return interchange;
		}

		#endregion

		/// <summary>
		/// Get an XML reader for the body of a message, positioned on the first element.
		/// Will automatically unencrypt the message if it was from an interchange created by CreateSecure.
		/// </summary>
		public static XmlTextReader GetXmlReader(EDIMessage message)
		{
			var textReader = new StringReader(message.EM_MessageText); // No point using a reader just to read the whole thing into a string anyway.
			XmlTextReader reader = new XmlTextReader(textReader);
			if (reader.ReadState == ReadState.Initial)
			{
				reader.Read();
				reader.MoveToContent();
			}

			if (reader.GetAttribute(CompressedAttributeName) == "1")
			{
				string xml = Unpack(reader.ReadString());
				reader.Close();
				textReader.Dispose();
				reader = new XmlTextReader(new StringReader(xml));
				if (reader.ReadState == ReadState.Initial)
				{
					reader.Read();
					reader.MoveToContent();
				}
			}

			return reader;
		}

		#region Deserialize

		/// <summary>
		/// Deserialize a received message that has been downloaded from an interchange.
		/// The interchange should have been created via CreateSecureInterchange/CreateInterchange by the sender.
		/// </summary>
		public static object Deserialize(EDIMessage message, ZXmlSerializer serializer)
		{
			using (var reader = GetXmlReader(message))
			{
				return serializer.Deserialize(reader);
			}
		}

		#endregion

		#region CreateInterchange implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "It's an XML attribute name so not translated")]
		const string CompressedAttributeName = "compressed";

		static string CreateInterchangeXml(string recipientId, string xmlMessageBody)
		{
			StringBuilder stringBuilder = new StringBuilder(xmlMessageBody.Length + 1000);
			using (var reader = XmlReader.Create(new StringReader(xmlMessageBody), new XmlReaderSettings() { CloseInput = true }))
			using (var writer = CreateWriter(stringBuilder, recipientId))
			{
				reader.MoveToContent();
				writer.WriteNode(reader, false);
			}
			return stringBuilder.ToString();
		}

		static string CreateSecureInterchangeXml(string recipientId, string xmlElementName, string messageText)
		{
			StringBuilder stringBuilder = new StringBuilder(messageText.Length + 1000);
			using (var writer = CreateWriter(stringBuilder, recipientId))
			{
				writer.WriteStartElement(xmlElementName);
				writer.WriteAttributeString(CompressedAttributeName, "1");
				writer.WriteString(messageText);
			}
			return stringBuilder.ToString();
		}

		static XmlWriter CreateWriter(StringBuilder stringBuilder, string recipientId)
		{
			var writer = XmlWriter.Create(stringBuilder, new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true });
			writer.WriteStartElement("SystemInterchange", SchemaName);
			writer.WriteAttributeString("xmlns", SchemaName);
			writer.WriteStartElement("Header");
			writer.WriteElementString("SenderID", GlbCompany.CurrentCompany.LicenceKeyIdentifier);
			writer.WriteElementString("RecipientID", recipientId);
			writer.WriteEndElement();
			writer.WriteStartElement("Body");
			return writer;
		}

		static EDIInterchange CreateInterchange(BusinessObjectFactory factory, string recipientId)
		{
			var interchange = factory.New<EDIInterchange>();

			interchange.EI_ApplicationCode = ApplicationCode;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.eHubQueued;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_From = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			interchange.EI_To = recipientId;
			interchange.EI_SessionGUID = interchange.PK;
			interchange.NumberStrategy = new InterchangeNumberStrategy(factory);

			// Not actually used for outgoing interchanges, but lets make it consistent with incoming ones
			interchange.EI_InterchangeType = InterchangeType;

			return interchange;
		}

		#endregion

		#region Compress and encrypt

		/// <summary>
		/// Compress, encrypt and convert to Base64 text
		/// </summary>
		public static string Pack(ZXmlSerializer serializer, object valueObject, string fileName)
		{
			using (MemoryStream xmlStream = new MemoryStream())
			{
				serializer.Serialize(xmlStream, valueObject);
				xmlStream.Position = 0;

				return Pack(xmlStream, fileName);
			}
		}

		public static string Pack(Stream stream, string fileName)
		{
			byte[] compressedData = Compress(stream, fileName);
			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			return Convert.ToBase64String(encoder.Encrypt(compressedData), Base64FormattingOptions.InsertLineBreaks);
		}

		public static string Pack(Stream stream)
		{
			return Pack(stream, ZipFileName);
		}

		static byte[] Compress(Stream stream, string fileName)
		{
			using (MemoryStream zipStream = new MemoryStream())
			{
				ZipCreator creator = new ZipCreator();
				creator.ZipStream(fileName, stream, zipStream);
				return zipStream.ToArray();
			}
		}

		/// <summary>
		/// Reverse a Pack
		/// </summary>
		public static string Unpack(string base64Text)
		{
			return Unpack(base64Text, ZipFileName);
		}

		public static string Unpack(string base64Text, string fileName)
		{
			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			return ExtractZipped(fileName, encoder.Decrypt(Convert.FromBase64String(base64Text)));
		}

		static string ExtractZipped(string fileName, byte[] data)
		{
			string result = string.Empty;
			using (Stream inStream = new MemoryStream(data))
			using (MemoryStream outStream = new MemoryStream())
			{
				ZipExtractor extractor = new ZipExtractor();
				extractor.ExtractZipStream(inStream, outStream, fileName);
				result = Encoding.UTF8.GetString(outStream.ToArray());
			}
			return result;
		}

		#endregion

		#region Number fountain

		internal class InterchangeNumberStrategy : IMessageNumberStrategy
		{
			public InterchangeNumberStrategy(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}
			readonly BusinessObjectFactory factory;

			public string GetMessageReferenceNumber()
			{
				return Enterprise.Environment.Env.Instance.NumberFountains.SystemEDIInterchangeNumber.GetNextFormatted(this.factory);
			}
		}

		internal class MessageNumberStrategy : IMessageNumberStrategy
		{
			public MessageNumberStrategy(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}
			readonly BusinessObjectFactory factory;

			public string GetMessageReferenceNumber()
			{
				return Enterprise.Environment.Env.Instance.NumberFountains.SystemEDIMessageNumber.GetNextFormatted(this.factory);
			}
		}

		#endregion

#if DEBUG
		public static EDIMessage DebugOnlyCreateDownloadedMessage(EDIInterchange interchange)
		{
			var reader = new XmlTextReader(interchange.GetEI_BodyTextReader());
			reader.ReadToFollowing("Body");
			reader.Read();
			reader.MoveToContent();
			if (reader.GetAttribute(CompressedAttributeName) == "1")
			{
				string xml = Unpack(reader.ReadString());
				reader = new XmlTextReader(new StringReader(xml));
				if (reader.ReadState == ReadState.Initial)
				{
					reader.Read();
					reader.MoveToContent();
				}
			}

			var message = interchange.Factory.New<Enterprise.Messaging.Business.XmlMessaging.XmlEDIMessage>();
			message.EM_ApplicationCode = SystemMessage.ApplicationCode;
			message.EM_IsTestMessage = false;
			message.EM_MessageType = SystemMessage.MessageType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_EI = interchange.PK;
			message.EM_GB = interchange.EI_GB;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;
			message.SetEM_MessageTextSource(new TextReaderSource(LargeMessageHelper.GetStreamFromNode(reader)));
			return message;
		}
#endif
	}
}
