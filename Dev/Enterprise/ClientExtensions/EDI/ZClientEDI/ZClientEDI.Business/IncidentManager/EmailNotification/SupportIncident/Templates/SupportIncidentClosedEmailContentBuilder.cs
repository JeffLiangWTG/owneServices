using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.Business;
using static Enterprise.Client.EDI.EDIDataRegistry;

namespace Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate
{
	public sealed class SupportIncidentClosedEmailContentBuilder : BaseEDIRegistryCodeDescriptionEmailTemplate
	{
		public SupportIncidentClosedEmailContentBuilder(SupportIncident dataSource, bool eRequestV2, string product, string registryTemplateCode = "", bool needUpgrade = false, bool shouldUseFollowUpTemplate = false)
			: base(eRequestV2, product, registryTemplateCode, needUpgrade)
		{
			this.dataSource = dataSource;
			this.shouldUseFollowUpTemplate = shouldUseFollowUpTemplate;
		}
		readonly SupportIncident dataSource;
		readonly bool shouldUseFollowUpTemplate;

		protected override CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem RegistryItem => EDIDataRegistry.Instance.CustomerServiceIncidentClosedNotificationEmailTemplates;

		public override ZString TemplateCode => SupportIncidentEmailTemplateConstants.Codes.IncidentClosedNotificationEmail;

		public override ZString TemplateDescription => SupportIncidentEmailTemplateConstants.Descriptions.IncidentClosedNotificationEmail;

		protected override NotificationEmailTemplate GetDefault()
		{
			var templateCode = shouldUseFollowUpTemplate ? CustomerServiceEmailTemplateCodes.FollowUpERequest : CustomerServiceEmailTemplateCodes.Default;
			return RegistryItem.Value.GetEmailTemplate(ERequestV2, templateCode, Product, NeedUpgrade);
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
