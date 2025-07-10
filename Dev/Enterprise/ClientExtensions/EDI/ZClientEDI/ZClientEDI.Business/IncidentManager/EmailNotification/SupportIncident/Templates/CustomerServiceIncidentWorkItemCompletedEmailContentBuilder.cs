using System.Globalization;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate
{
	public sealed class CustomerServiceIncidentWorkItemCompletedEmailContentBuilder : BaseEDIRegistryCodeDescriptionEmailTemplate
	{
		public CustomerServiceIncidentWorkItemCompletedEmailContentBuilder(SupportIncident dataSource, bool eRequestV2, string product, string registryTemplateCode = "", bool needUpgrade = false) : base(eRequestV2, product, registryTemplateCode, needUpgrade)
		{
			this.dataSource = dataSource;
		}
		readonly SupportIncident dataSource;

		protected override CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem RegistryItem => EDIDataRegistry.Instance.CustomerServiceIncidentWorkItemCompletedEmailTemplates;

		public override ZString TemplateCode => SupportIncidentEmailTemplateConstants.Codes.WorkItemCompletedEmail;

		public override ZString TemplateDescription => SupportIncidentEmailTemplateConstants.Descriptions.WorkItemCompletedEmail;

		public override ZString BuildBody()
		{
			return new SupportIncidentHtmlParser(dataSource.Factory).Parse(dataSource, BodyTemplate);
		}

		public override ZString BuildSubject()
		{
			return new SupportIncidentParser(dataSource.Factory).Parse(dataSource, SubjectTemplate);
		}
	}

	public sealed class CustomerServiceIncidentWorkItemCompletedDefaultEmailContentBuilder : IEDIEmailTemplate, IEDIEmailTemplateBuilder
	{
		public CustomerServiceIncidentWorkItemCompletedDefaultEmailContentBuilder(SupportIncident dataSource)
		{
			this.dataSource = dataSource;
		}

		readonly SupportIncident dataSource;

		public ZString TemplateCode => SupportIncidentEmailTemplateConstants.Codes.WorkItemCompletedEmail;

		public ZString TemplateDescription => SupportIncidentEmailTemplateConstants.Descriptions.WorkItemCompletedEmail;

		public ZString SubjectTemplate
		{
			get
			{
				if (dataSource.IM_Category == SupportIncidentCategoriesList.Codes.ContentDevelopment)
				{
					return "Content Development Work Completed email for {0} could not be sent to the client";
				}
				else
				{
					return "Development Work Completed email for {0} could not be sent to the client";
				}
			}
		}

		public ZString BodyTemplate => "Please notify the client manually.";

		public bool IsEmpty => dataSource == null;

		public ZString BuildBody()
		{
			return BodyTemplate;
		}

		public ZString BuildSubject()
		{
			return string.Format(CultureInfo.InvariantCulture, SubjectTemplate, dataSource.IM_IncidentNumber);
		}

		public IEDIEmailTemplate GetIEDIEmailTemplate()
		{
			return this;
		}
	}
}
