using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate
{
	public sealed class SendUnsubscribedNotificationEmailContentBuilder : BaseEDIRegistryNotificationEmailTemplate
	{
		public SendUnsubscribedNotificationEmailContentBuilder(SupportIncident dataSource)
		{
			this.dataSource = dataSource;
			parser = new DocumentParser<SupportIncident, DocSupportIncident>(dataSource.Factory);
		}
		readonly SupportIncident dataSource;

		readonly DocumentParser<SupportIncident, DocSupportIncident> parser;

		public override NotificationEmailTemplateRegistryItem RegistryItem => EDIDataRegistry.Instance.UnsubscriberEmailTemplate;

		public override ZString TemplateCode => SupportIncidentEmailTemplateConstants.Codes.IncidentEConversationUnsubscribedEmailNotification;

		public override ZString TemplateDescription => SupportIncidentEmailTemplateConstants.Descriptions.IncidentEConversationUnsubscribedEmailNotification;

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
