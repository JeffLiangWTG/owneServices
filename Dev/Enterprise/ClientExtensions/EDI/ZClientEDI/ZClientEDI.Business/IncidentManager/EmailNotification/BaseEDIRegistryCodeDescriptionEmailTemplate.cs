using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate
{
	public abstract class BaseEDIRegistryCodeDescriptionEmailTemplate : IEDIEmailTemplate, IEDIEmailTemplateBuilder
	{
		public BaseEDIRegistryCodeDescriptionEmailTemplate(bool eRequestV2, string product, string registryTemplateCode = "", bool needUpgrade = false)
		{
			ERequestV2 = eRequestV2;
			Product = product;
			NeedUpgrade = needUpgrade;
			this.RegistryTemplateCode = registryTemplateCode;
		}
		protected string RegistryTemplateCode { get; set; }

		public ZString SubjectTemplate => Template?.EmailSubject ?? ZString.Empty;

		public ZString BodyTemplate => Template?.EmailBody ?? ZString.Empty;

		protected virtual bool ERequestV2 { get; set; }
		protected virtual string Product { get; set; }
		protected virtual bool NeedUpgrade { get; set; }

		protected abstract CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem RegistryItem { get; }

		public NotificationEmailTemplate Template
		{
			get
			{
				NotificationEmailTemplate result;
				if (string.IsNullOrEmpty(RegistryTemplateCode))
				{
					result = RegistryItem?.Value.GetEmailTemplate(ERequestV2, Product, NeedUpgrade);
				}
				else
				{
					result = RegistryItem?.Value.GetEmailTemplate(ERequestV2, RegistryTemplateCode, Product, NeedUpgrade);
				}

				if (result == null)
				{
					result = GetDefault();
				}

				return result;
			}
		}

		protected virtual NotificationEmailTemplate GetDefault() => null;

		public abstract ZString BuildBody();

		public abstract ZString BuildSubject();

		public IEDIEmailTemplate GetIEDIEmailTemplate()
		{
			return this;
		}

		public bool IsEmpty => SubjectTemplate.IsEmpty && BodyTemplate.IsEmpty;

		public abstract ZString TemplateCode { get; }

		public abstract ZString TemplateDescription { get; }
	}
}
