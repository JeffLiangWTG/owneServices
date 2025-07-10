using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate
{
	public sealed class SupportIncidentResolvedEmailContentBuilder : BaseEDIRegistryCodeDescriptionEmailTemplate
	{
		public SupportIncidentResolvedEmailContentBuilder(SupportIncident dataSource, bool eRequestV2, string product, string registryTemplateCode = "", bool needUpgrade = false) : base(eRequestV2, product, registryTemplateCode, needUpgrade)
		{
			this.dataSource = dataSource;
		}
		readonly SupportIncident dataSource;

		protected override CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem RegistryItem => EDIDataRegistry.Instance.CustomerServiceResolvedNotificationTemplates;

		public override ZString TemplateCode => SupportIncidentEmailTemplateConstants.Codes.IncidentResolvedNotificationEmail;

		public override ZString TemplateDescription => SupportIncidentEmailTemplateConstants.Descriptions.IncidentResolvedNotificationEmail;

		protected override NotificationEmailTemplate GetDefault()
		{
			return RegistryItem.Value.GetEmailTemplate(ERequestV2, "DRT", Product, NeedUpgrade);
		}

		public override ZString BuildBody()
		{
			return new SupportIncidentHtmlParser(dataSource.Factory).Parse(dataSource, BodyTemplate);
		}

		public override ZString BuildSubject()
		{
			return new SupportIncidentParser(dataSource.Factory).Parse(dataSource, SubjectTemplate);
		}
	}
}
