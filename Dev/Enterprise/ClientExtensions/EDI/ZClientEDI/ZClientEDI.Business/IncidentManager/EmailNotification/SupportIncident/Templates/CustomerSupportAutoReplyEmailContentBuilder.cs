using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;
using Enterprise.MailManager.Business;

namespace ZClientEDI.Business.IncidentManager.EmailNotification.SupportIncident.Templates
{
	public sealed class CustomerSupportAutoReplyEmailContentBuilder : IEDIEmailTemplate, IEDIEmailTemplateBuilder
	{
		public CustomerSupportAutoReplyEmailContentBuilder(MailItem needRepliedEmail)
		{
			this.needRepliedEmail = needRepliedEmail;
		}

		readonly MailItem needRepliedEmail;

		public ZString TemplateCode => SupportIncidentEmailTemplateConstants.Codes.AutoReplyEmail;

		public ZString TemplateDescription => SupportIncidentEmailTemplateConstants.Descriptions.AutoReplyEmail;

		const string subjectFlag = "(*ReplySubject*)";

		public ZString SubjectTemplate => $"Re: {subjectFlag}";

		public ZString BodyTemplate => EDIDataRegistry.Instance.CustomerSupportAutoReplyEmailTemplate ?? ZString.Empty;

		public bool IsEmpty => needRepliedEmail == null || BodyTemplate == ZString.Empty;

		ZString IEDIEmailTemplateBuilder.BuildSubject()
		{
			return SubjectTemplate.Replace(subjectFlag, needRepliedEmail?.MI_Subject ?? ZString.Empty);
		}

		ZString IEDIEmailTemplateBuilder.BuildBody()
		{
			return BodyTemplate;
		}

		public IEDIEmailTemplate GetIEDIEmailTemplate()
		{
			return this;
		}
	}
}
