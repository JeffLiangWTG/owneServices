using CargoWise.Types;
using Enterprise.CustomerService.Business;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate
{
	public sealed class ERequestPendingApprovalNotificationMessageContentBuilder : BaseEDIRegistryNotificationEmailTemplate
	{
		public ERequestPendingApprovalNotificationMessageContentBuilder(IncidentRequest dataSource) : base()
		{
			this.dataSource = dataSource;
			parser = new DocumentParser<IncidentRequest, DocIncidentRequest>(dataSource.Factory);
		}

		readonly IncidentRequest dataSource;

		readonly DocumentParser<IncidentRequest, DocIncidentRequest> parser;

		public override NotificationEmailTemplateRegistryItem RegistryItem => EDIDataRegistry.Instance.ERequestPendingApprovalNotificationMessageTemplate;

		public override ZString TemplateCode => SupportIncidentEmailTemplateConstants.Codes.PendingApprovalNotificationMessage;

		public override ZString TemplateDescription => SupportIncidentEmailTemplateConstants.Descriptions.PendingApprovalNotificationMessage;

		public override ZString BuildBody()
		{
			return parser.Parse(dataSource, BodyTemplate);
		}

		public override ZString BuildSubject()
		{
			return parser.Parse(dataSource, SubjectTemplate);
		}
	}
}
