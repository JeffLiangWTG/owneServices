using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class CodeDescriptionEmailTemplateCollection : CodeDescriptionBoolCollection
	{
		public CodeDescriptionEmailTemplateCollection()
			: base()
		{ }

		public CodeDescriptionEmailTemplateCollection(Type docSourceType)
			: base()
		{
			this.docSourceType = docSourceType;
		}

		public CodeDescriptionEmailTemplateCollection(Type docSourceType, FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
			this.docSourceType = docSourceType;
		}

		readonly Type docSourceType;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((CodeDescriptionEmailTemplate)child).EmailTemplate.DocSourceType = docSourceType;
		}

		public new CodeDescriptionEmailTemplate this[int index]
		{
			get { return (CodeDescriptionEmailTemplate)Elements[index]; }
		}

		public new CodeDescriptionEmailTemplate AddNew()
		{
			return (CodeDescriptionEmailTemplate)base.AddNew();
		}

		public NotificationEmailTemplate GetEmailTemplate(string code)
		{
			NotificationEmailTemplate result;
			CodeDescriptionEmailTemplate template = FindByCode(code) as CodeDescriptionEmailTemplate;

			if (template != null)
			{
				result = template.EmailTemplate;
			}
			else
			{
				result = new NotificationEmailTemplate();
			}

			return result;
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new CodeDescriptionEmailTemplateCollection(docSourceType);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			CodeDescriptionEmailTemplate template = new CodeDescriptionEmailTemplate();
			template.EmailTemplate.DocSourceType = docSourceType;
			return template;
		}
	}
}

