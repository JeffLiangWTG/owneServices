using System.Xml;
using System.Xml.Serialization;

using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class CodeDescriptionEmailTemplate : CodeDescriptionBool
	{
		public CodeDescriptionEmailTemplate() { }

		public CodeDescriptionEmailTemplate(FallbackLevel fallbackLevel)
			: base(fallbackLevel) { }

		public CodeDescriptionEmailTemplate(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			CodeDescriptionEmailTemplate result = new CodeDescriptionEmailTemplate(fallbackLevel, factory);
			result.emailTemplate = (NotificationEmailTemplate)EmailTemplate.Clone(fallbackLevel, factory);
			result.emailTemplate.DocSourceType = EmailTemplate.DocSourceType;
			return result;
		}

		#region Notification Email Template

		public NotificationEmailTemplate EmailTemplate
		{
			get { return emailTemplate ?? (emailTemplate = new NotificationEmailTemplate()); }
			set { emailTemplate = value; }
		}

		NotificationEmailTemplate emailTemplate;

		ZXmlSerializer EmailTemplateSerialiser
		{
			get { return emailTemplateSerialiser ?? (emailTemplateSerialiser = ZXmlSerializer.New(typeof(NotificationEmailTemplate))); }
		}

		ZXmlSerializer emailTemplateSerialiser;

		#endregion

		#region XML Serialisation

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			EmailTemplateSerialiser.Serialize(writer, EmailTemplate);
		}

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			emailTemplate = (NotificationEmailTemplate)EmailTemplateSerialiser.Deserialize(reader);
		}

		#endregion
	}
}

