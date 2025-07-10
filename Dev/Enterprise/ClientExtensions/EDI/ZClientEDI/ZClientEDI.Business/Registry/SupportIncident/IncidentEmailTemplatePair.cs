using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class IncidentEmailTemplatePair : RegistryBusinessObjectTemplate
	{
		public IncidentEmailTemplatePair() { }

		public IncidentEmailTemplatePair(Type docSourceType) : base()
		{
			this.docSourceType = docSourceType;
		}

		public IncidentEmailTemplatePair(Type docSourceType, NotificationEmailTemplate legacyAndERequestV1EmailTemplate, NotificationEmailTemplate eRequestV2EmailTemplate)
			: this(docSourceType)
		{
			this.LegacyAndERequestV1EmailTemplate.EmailSubject = legacyAndERequestV1EmailTemplate.EmailSubject;
			this.LegacyAndERequestV1EmailTemplate.EmailBody = legacyAndERequestV1EmailTemplate.EmailBody;
			this.ERequestV2EmailTemplate.EmailSubject = eRequestV2EmailTemplate.EmailSubject;
			this.ERequestV2EmailTemplate.EmailBody = eRequestV2EmailTemplate.EmailBody;
		}

		#region DocSourceType

		Type docSourceType;
		public Type DocSourceType
		{
			get { return docSourceType; }
			set
			{
				docSourceType = value;
				LegacyAndERequestV1EmailTemplate.DocSourceType = value;
				ERequestV2EmailTemplate.DocSourceType = value;
			}
		}

		IDocumentFieldDefinitionCollection documentFields;
		public IDocumentFieldDefinitionCollection DocumentFields
		{
			get { return documentFields ?? (documentFields = new DocumentFieldAttributeFinder().FindProperties(DocSourceType)); }
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new IncidentEmailTemplatePair(DocSourceType, LegacyAndERequestV1EmailTemplate, ERequestV2EmailTemplate);
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly; }
			set
			{
				base.ReadOnly = value;

				LegacyAndERequestV1EmailTemplate.ReadOnly = value;
				ERequestV2EmailTemplate.ReadOnly = value;
			}
		}

		#region Email Templates

		public NotificationEmailTemplate GetEmailTemplate(bool eRequestV2)
		{
			return eRequestV2 ? ERequestV2EmailTemplate : LegacyAndERequestV1EmailTemplate;
		}

		NotificationEmailTemplate legacyAndERequestV1EmailTemplate;
		public NotificationEmailTemplate LegacyAndERequestV1EmailTemplate
		{
			get { return legacyAndERequestV1EmailTemplate ?? (legacyAndERequestV1EmailTemplate = new NotificationEmailTemplate(DocSourceType)); }
		}

		NotificationEmailTemplate eRequestV2EmailTemplate;
		public NotificationEmailTemplate ERequestV2EmailTemplate
		{
			get { return eRequestV2EmailTemplate ?? (eRequestV2EmailTemplate = new NotificationEmailTemplate(DocSourceType)); }
		}

		#endregion

		#region XML Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			EmailTemplateSerialiser.Serialize(writer, LegacyAndERequestV1EmailTemplate);
			EmailTemplateSerialiser.Serialize(writer, ERequestV2EmailTemplate);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			legacyAndERequestV1EmailTemplate = (NotificationEmailTemplate)EmailTemplateSerialiser.Deserialize(reader);
			eRequestV2EmailTemplate = (NotificationEmailTemplate)EmailTemplateSerialiser.Deserialize(reader);
		}

		ZXmlSerializer emailTemplateSerialiser;
		ZXmlSerializer EmailTemplateSerialiser
		{
			get { return emailTemplateSerialiser ?? (emailTemplateSerialiser = ZXmlSerializer.New(typeof(NotificationEmailTemplate))); }
		}

		#endregion
	}
}

