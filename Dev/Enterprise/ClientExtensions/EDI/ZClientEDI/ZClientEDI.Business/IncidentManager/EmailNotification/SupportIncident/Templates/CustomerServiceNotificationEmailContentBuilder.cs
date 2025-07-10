using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate
{
	public sealed class CustomerServiceNotificationEmailContentBuilder : BaseEDIRegistryCodeDescriptionEmailTemplate
	{
		public CustomerServiceNotificationEmailContentBuilder(SupportIncident dataSource, bool eRequestV2, string product, string registryTemplateCode = "", bool needUpgrade = false) : base(eRequestV2, product, registryTemplateCode, needUpgrade)
		{
			this.dataSource = dataSource;
		}

		readonly SupportIncident dataSource;
		protected override CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem RegistryItem => EDIDataRegistry.Instance.CustomerServiceNotificationEmailTemplateRaw;

		public override ZString TemplateCode => SupportIncidentEmailTemplateConstants.Codes.CustomerServiceNotificationEmail;

		public override ZString TemplateDescription => SupportIncidentEmailTemplateConstants.Descriptions.CustomerServiceNotificationEmail;

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
