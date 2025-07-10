using System;
using System.Data;
using System.IO;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Interfaces;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSEDIMessage : GbEDIMessage, Integration.Customs.GB.GBCDS.IGbCDSEdiMessage
	{
		public CDSEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			MessageNumberStrategy = new GbMessageNumberStrategy(Factory, EDIMessage.ApplicationCodes.GbCustomsDeclarationServices);
		}

		public new static readonly CDSEDIMessageTypeDecider TypeDecider = new CDSEDIMessageTypeDecider();

		public CusEntryHeader LinkedEntry => EM_LinkedObject as CusEntryHeader;
		public ForwardingConsol LinkedConsol => EM_LinkedObject as ForwardingConsol;
		public IMessageAttachee LinkedMessageAttachee => EM_LinkedObject as IMessageAttachee;

		public new CDSInterchange Interchange => Factory.Load<CDSInterchange>(EM_EI);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsDeclarationServices;
		}

		protected override bool ResetToQueuedStatusPreservesMessageType => true;

		protected virtual ZGuid EHubTrackingId => ZGuid.Empty;

		public override ZString EM_MessageTextIndentedXml
		{
			get
			{
				if (emMessageTextIndentedXml == null)
				{
					emMessageTextIndentedXml = TryIndentXml(GetEM_MessageTextReader()) ?? EM_MessageText;
				}

				return emMessageTextIndentedXml;
			}
		}
		string emMessageTextIndentedXml;

		static string TryIndentXml(TextReader xmlToFormatReader)
		{
			string indentedXml;

			try
			{
				XDocument doc = XDocument.Parse(xmlToFormatReader.ReadToEnd());
				indentedXml = doc.ToString();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				indentedXml = null;
			}

			return indentedXml;
		}
	}

	public class CDSEDIMessage<TCDSMessageDataObject> : CDSEDIMessage
	where TCDSMessageDataObject : class
	{
		public CDSEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public TCDSMessageDataObject MessageDataObject => messageDataObject ?? (messageDataObject = GetMessageDataObject(EM_MessageText));

		TCDSMessageDataObject messageDataObject;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
		public static ZString Serialize(TCDSMessageDataObject messageDataObject)
		{
			return SerializationHelper.Serialize(messageDataObject);
		}

		static TCDSMessageDataObject GetMessageDataObject(ZString messageText)
		{
			try
			{
				return XmlObjectSerializer.Deserialize<TCDSMessageDataObject>(messageText);
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
