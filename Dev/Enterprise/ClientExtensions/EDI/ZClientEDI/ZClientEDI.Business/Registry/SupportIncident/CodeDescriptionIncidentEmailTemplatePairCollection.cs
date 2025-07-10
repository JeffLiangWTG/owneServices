using System;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class CodeDescriptionIncidentEmailTemplatePairCollection : RegistryBusinessObjectCollection
	{
		public CodeDescriptionIncidentEmailTemplatePairCollection() { }

		public CodeDescriptionIncidentEmailTemplatePairCollection(Type docSourceType) : base()
		{
			DocSourceType = docSourceType;
		}

		Type docSourceType;
		public Type DocSourceType
		{
			get { return docSourceType; }
			set
			{
				docSourceType = value;
				foreach (CodeDescriptionIncidentEmailTemplatePair incidentEmailTemplates in this)
				{
					incidentEmailTemplates.DocSourceType = docSourceType;
				}
			}
		}

		public NotificationEmailTemplate GetEmailTemplate(bool eRequestV2, string code, string product, bool needUpgrade = false)
		{
			var templates = this.Cast<CodeDescriptionIncidentEmailTemplatePair>();
			var codeDescriptionTemplate = new[]
				{
					templates.FirstOrDefault(t => t.Code == code && t.Product == product && t.NeedUpgrade == needUpgrade),
					templates.FirstOrDefault(t => t.Code == code && t.Product == product),
					templates.FirstOrDefault(t => t.Code == code && t.Product.IsEmpty)
				}.FirstOrDefault(x => x != null);

			if (codeDescriptionTemplate != null)
			{
				return codeDescriptionTemplate.EmailTemplates.GetEmailTemplate(eRequestV2);
			}
			else
			{
				return null;
			}
		}

		public NotificationEmailTemplate GetEmailTemplate(bool eRequestV2, string product, bool needUpgrade = false)
		{
			var templates = this.Cast<CodeDescriptionIncidentEmailTemplatePair>();
			var codeDescriptionTemplate = new[]
				{
					templates.FirstOrDefault(t => t.Product == product && t.NeedUpgrade == needUpgrade),
					templates.FirstOrDefault(t => t.Product == product),
					templates.FirstOrDefault(t => t.Product.IsEmpty)
				}.FirstOrDefault(x => x != null);

			if (codeDescriptionTemplate != null)
			{
				return codeDescriptionTemplate.EmailTemplates.GetEmailTemplate(eRequestV2);
			}
			else
			{
				return null;
			}
		}

		public new CodeDescriptionIncidentEmailTemplatePair this[int index]
		{
			get { return (CodeDescriptionIncidentEmailTemplatePair)Elements[index]; }
		}

		public new CodeDescriptionIncidentEmailTemplatePair AddNew()
		{
			return (CodeDescriptionIncidentEmailTemplatePair)base.AddNew();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((CodeDescriptionIncidentEmailTemplatePair)child).DocSourceType = DocSourceType;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var template = new CodeDescriptionIncidentEmailTemplatePair(DocSourceType);
			template.DocSourceType = DocSourceType;
			return template;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = new CodeDescriptionIncidentEmailTemplatePairCollection(DocSourceType);
			return clone;
		}
	}
}

