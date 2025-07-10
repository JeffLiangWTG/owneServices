using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate
{
	public sealed class SupportIncidentAwaitingResponseEmailContentBuilder : BaseEDIRegistryNotificationEmailTemplate
	{
		public SupportIncidentAwaitingResponseEmailContentBuilder(SupportIncident dataSource, IEnumerable<IConversationMessage> eMessages) : base()
		{
			Argument.NotNull(dataSource, nameof(dataSource));
			this.dataSource = dataSource;
			parser = new SupportIncidentParser(dataSource.Factory);
			this.eMessages = eMessages;
		}

		readonly SupportIncident dataSource;
		readonly SupportIncidentParser parser;
		readonly IEnumerable<IConversationMessage> eMessages;

		public override NotificationEmailTemplateRegistryItem RegistryItem => EDIDataRegistry.Instance.CustomerServiceAwaitingResponseNotificationMessageTemplate;

		public override ZString TemplateCode => SupportIncidentEmailTemplateConstants.Codes.AwaitingResponseNotificationMessage;

		public override ZString TemplateDescription => SupportIncidentEmailTemplateConstants.Descriptions.AwaitingResponseNotificationMessage;

		public override ZString BuildBody()
		{
			var eConversationMessagesMacro = "(*eConversationMessages*)";
			var result = parser.Parse(dataSource, BodyTemplate);
			if (result.Contains(eConversationMessagesMacro))
			{
				var formattedMessage = SupportIncidentEmailBodyGeneralControls.GetFormattedEConversationChatHistory(eMessages);
				result = formattedMessage.Length != 0 ? result.Replace(eConversationMessagesMacro, formattedMessage) : new ZString("No eConversation Messages to display.");
			}

			return result;
		}

		public override ZString BuildSubject()
		{
			var result = parser.Parse(dataSource, SubjectTemplate);
			if (result.IsEmpty)
			{
				result = parser.Parse(dataSource, SupportIncidentEmailBodyGeneralControls.GetCommonEmailSubject("Update"));
			}

			return result;
		}
	}
}
