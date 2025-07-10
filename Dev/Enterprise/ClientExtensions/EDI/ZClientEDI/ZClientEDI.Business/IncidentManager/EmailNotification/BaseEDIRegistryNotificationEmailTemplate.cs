using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate
{
	public abstract class BaseEDIRegistryNotificationEmailTemplate : IEDIEmailTemplate, IEDIEmailTemplateBuilder
	{
		public ZString SubjectTemplate => Template?.EmailSubject ?? ZString.Empty;

		public ZString BodyTemplate => Template?.EmailBody ?? ZString.Empty;

		public abstract NotificationEmailTemplateRegistryItem RegistryItem { get; }

		public NotificationEmailTemplate Template
		{
			get
			{
				if (RegistryItem?.Value == null)
				{
					return GetDefault();
				}
				return RegistryItem?.Value;
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
