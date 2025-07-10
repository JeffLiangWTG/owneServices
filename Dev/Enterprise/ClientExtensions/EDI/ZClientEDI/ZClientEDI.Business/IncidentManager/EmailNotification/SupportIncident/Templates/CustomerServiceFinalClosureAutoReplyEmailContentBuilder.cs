
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.Business;
using static Enterprise.Client.EDI.EDIDataRegistry;

namespace Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate
{
	public class CustomerServiceFinalClosureAutoReplyEmailContentBuilder : BaseEDIRegistryCodeDescriptionEmailTemplate
	{
		public CustomerServiceFinalClosureAutoReplyEmailContentBuilder(SupportIncident dataSource, bool eRequestV2, string product, string registryTemplateCode = "", bool needUpgrade = false) : base(eRequestV2, product, registryTemplateCode, needUpgrade)
		{
			this.dataSource = dataSource;
		}

		readonly SupportIncident dataSource;

		public override ZString TemplateCode => SupportIncidentEmailTemplateConstants.Codes.CustomerServiceFinalClosureAutoReplyEmail;

		public override ZString TemplateDescription => SupportIncidentEmailTemplateConstants.Descriptions.CustomerServiceFinalClosureAutoReplyEmail;

		protected override CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem RegistryItem => EDIDataRegistry.Instance.CustomerServiceFinalClosureAutoReplyEmailTemplate;

		protected override NotificationEmailTemplate GetDefault()
		{
			return RegistryItem.Value.GetEmailTemplate(ERequestV2, CustomerServiceEmailTemplateCodes.Default, Product, NeedUpgrade);
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
