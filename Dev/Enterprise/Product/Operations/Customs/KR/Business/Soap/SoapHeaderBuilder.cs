using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.Customs.KR.MessageDefinitions.SoapEnvelope.Committees;
using CargoWise.Customs.KR.MessageDefinitions.SoapEnvelope.SOAP;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class SoapHeaderBuilder : MessageBuilder<Envelope>
	{
		public static class Constants
		{
			public static class Namespaces
			{
				public const string xlink = @"http://www.w3.org/1999/xlink";
				public const string xsi = @"http://www.w3.org/2001/XMLSchema-instance";
				public const string eb = @"http://www.oasis-open.org/committees/ebxml-msg/schema/msg-header-2_0.xsd";
				public const string actor = @"http://schemas.xmlsoap.org/soap/actor/next";
				public const string SOAP = @"http://schemas.xmlsoap.org/soap/envelope/";
				public const string ds = @"http://www.w3.org/2000/09/xmldsig#";
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Prefix strings")]
			public static class Prefixes
			{
				public const string xlink = "xlink";
				public const string xsi = "xsi";
				public const string eb = "eb";
				public const string actor = "actor";
				public const string SOAP = "SOAP";
				public const string ds = "ds";
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Http elements")]
			public static class HttpElements
			{
				public const string SoapAction = "SOAPACTION: \"ebXML\"";
				public const string ContentTypeText = "Content-Type: text/xml; charset=UTF-8";
				public const string ContentTypeMultipart = "Content-Type: multipart/related; type=\"text/xml\";";
				public const string ContentIdPayload = "Content-Id: <payload-1>";
				public const string ContentIdSoapPart = "Content-Id: <SOAPPART>";
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Xml elements")]
			public static class SignedXmlElementNames
			{
				public const string SignedInfo = "SignedInfo";
				public const string SignatureValue = "SignatureValue";
				public const string Modulus = "Modulus";
				public const string X509Certificate = "X509Certificate";
			}

			public static class SoapObjectLocalNames
			{
				public static class SOAP
				{
					public const string Header = nameof(Header);
					public const string Body = nameof(Body);
				}

#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
				public static class eb
				{
					public const string Manifest = nameof(Manifest);
					public const string Reference = nameof(Reference);
					public const string SyncReply = nameof(SyncReply);
				}

				public static class xlink
				{
					public const string href = nameof(href);
				}
				public const string XPath = nameof(XPath);
#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
			}
		}

		readonly Enterprise.Messaging.Business.EDIInterchange interChange;
		public SoapHeaderBuilder(Enterprise.Messaging.Business.EDIInterchange interChange)
		{
			this.interChange = interChange;
		}

		public override Envelope GenerateMessage()
		{
			return new Envelope
			{
				Header = PopulateHeader(),
				Body = PopulateBody()
			};
		}

		public static XmlElement SerializeToXmlElement<T>(T o)
		{
			XmlDocument doc = new XmlDocument();

			var namespaces = new XmlSerializerNamespaces();
			namespaces.Add(Constants.Prefixes.xlink, Constants.Namespaces.xlink);
			namespaces.Add(Constants.Prefixes.xsi, Constants.Namespaces.xsi);
			namespaces.Add(Constants.Prefixes.eb, Constants.Namespaces.eb);
			namespaces.Add(Constants.Prefixes.actor, Constants.Namespaces.actor);

			var settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };

			using (var stream = SerializeAsStream(o, settings, namespaces))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				doc.LoadXml(serialisedXml);
			}

			return doc.DocumentElement;
		}

		public static Stream SerializeAsStream<T>(T xmlObject, XmlWriterSettings settings, XmlSerializerNamespaces namespaces)
		{
			SubStreamableStream subStreamableStream = new SubStreamableStream(EmptyDisposableLeakListener.Instance);
			using (XmlWriter xmlWriter = XmlWriter.Create(subStreamableStream, settings))
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
				xmlSerializer.Serialize(xmlWriter, xmlObject, namespaces);
			}

			subStreamableStream.Position = 0L;
			return subStreamableStream;
		}

		Header PopulateHeader()
		{
			var header = new Header();
			header.Any.Add(SerializeToXmlElement(PopulateMessageHeader()));
			header.Any.Add(SerializeToXmlElement(PopulateSyncReply()));

			return header;
		}

		Body PopulateBody()
		{
			var body = new Body();
			body.Any.Add(SerializeToXmlElement(PopulateManifest()));
			return body;
		}

		MessageHeader PopulateMessageHeader()
		{
			return new MessageHeader
			{
				MustUnderstand = 1,
				Id = "MessageHeader",
				Version = "2.0",
				From = PopulateFrom(),
				To = PopulateTo(),
				CpaId = "KCSIPTJXAO001",
				ConversationId = PopulateConversationId(),
				Service = PopulateService(),
				Action = PopulateAction(),
				MessageData = PopulateMessageData(),
				Description = PopulateMessageHeaderDescription()
			};
		}

		From PopulateFrom()
		{
			return new From
			{
				PartyId = PopulateFromPartyId(),
				Role = "http://www.ok-customs.go.kr/ebMSH#sender"
			};
		}

		Collection<PartyId> PopulateFromPartyId()
		{
			ZString mailboxid = ZString.Empty;

			var factory = interChange.Factory;
			GlbBranch branch = factory.Load<GlbBranch>(interChange.EI_GB);

			if (branch != null)
			{
				var wrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(branch.Company);
				var glbExternalPassword = wrapper.GetGlbExternalPassword<GlbExternalPassword>(PasswordTypesList.Codes.KRB);
				if (glbExternalPassword != null)
				{
					mailboxid = glbExternalPassword.GP_MailBoxID;
				}
			}
			return PopulateToPartyId(mailboxid);
		}

		To PopulateTo()
		{
			return new To
			{
				PartyId = PopulateToPartyId("OK-CUSTOMS"),
				Role = "http://www.ok-customs.go.kr/ebMSH#seller"
			};
		}

		Collection<PartyId> PopulateToPartyId(ZString mailboxid)
		{
			var partid = new Collection<PartyId>();
			partid.Add(new PartyId { Type = "ok-customs.dtm1", Value = mailboxid });
			return partid;
		}

		string PopulateConversationId()
		{
			string result = "";
			if (interChange.ContainedMessages.Count > 0)
			{
				var message = interChange.ContainedMessages[interChange.ContainedMessages.Count - 1];
				var parent = message.EM_LinkedObject as IEDIMessageCollectionProviderWithID;
				result = parent?.IDNumber ?? ZString.Empty;
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Id name")]
		Service PopulateService()
		{
			return new Service
			{
				Type = "anyURI",
				Value = "urn:Ok-Customs-Service:order"
			};
		}

		string PopulateAction()
		{
			string result;
			switch (interChange.EI_InterchangeType)
			{
				case EDIInterchangeType.DLT:
					result = "KCSListReqAction";
					break;
				case EDIInterchangeType.DOC:
					result = "KCSFileReqAction";
					break;
				default:
					result = "KCSSingleAction";
					break;
			}
			return result;
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Baseline")]
		MessageData PopulateMessageData()
		{
			return new MessageData { MessageId = interChange.EI_SessionGUID.ToString(), Timestamp = DateTime.Now };
		}

		Collection<Description> PopulateMessageHeaderDescription()
		{
			var description = new Collection<Description>();
			description.Add(new Description { Lang = "en-US", Value = PopulateDescriptionValue() });
			return description;
		}

		string PopulateDescriptionValue()
		{
			string result;
			switch (interChange.EI_InterchangeType)
			{
				case EDIInterchangeType.DLT:
					result = "REQLST-000";
					break;
				case EDIInterchangeType.DOC:
					result = "DOCFND-000";
					break;
				default:
					result = "GOVCBR" + interChange.EI_InterchangeType;
					break;
			}
			return result;
		}

		SyncReply PopulateSyncReply()
		{
			return new SyncReply
			{
				Actor = "http://schemas.xmlsoap.org/soap/actor/next",
				MustUnderstand = 1,
				Version = "2.0"
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Id name")]
		Manifest PopulateManifest()
		{
			Manifest result = new Manifest();
			result.Id = "Manifest";
			result.Version = "2.0";

			if (interChange.EI_InterchangeType != EDIInterchangeType.DLT && interChange.EI_InterchangeType != EDIInterchangeType.DOC)
			{
				result.Reference = PopulateReference();
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Html element")]
		Collection<Reference> PopulateReference()
		{
			var reference = new Collection<Reference>();
			reference.Add(new Reference { Id = "Reference-1", Href = "cid:payload-1", Type = CargoWise.Customs.KR.MessageDefinitions.SoapEnvelope.XLINK.Type.Simple, Description = PopulateManifestDescription() });
			return reference;
		}

		Collection<Description> PopulateManifestDescription()
		{
			var description = new Collection<Description>();
			description.Add(new Description { Lang = "ko-kr", Value = interChange.EI_SessionGUID.ToString() });
			return description;
		}
	}
}
