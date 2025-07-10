using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate
{
	public sealed class DefaultNotificationEmailTemplate : BaseEDIRegistryNotificationEmailTemplate
	{
		public override NotificationEmailTemplateRegistryItem RegistryItem => null;

		public override ZString TemplateCode => "___";

		public override ZString TemplateDescription => "Empty Email Template";

		protected override NotificationEmailTemplate GetDefault()
		{
			return new NotificationEmailTemplate();
		}

		public override ZString BuildBody()
		{
			return Template.EmailBody;
		}

		public override ZString BuildSubject()
		{
			return Template.EmailSubject;
		}
	}
}
