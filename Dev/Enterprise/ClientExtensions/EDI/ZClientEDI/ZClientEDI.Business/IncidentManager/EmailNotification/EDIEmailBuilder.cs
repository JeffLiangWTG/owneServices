using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate
{
	public class EDIEmailBuilder
	{
		EDIEmailBuilder(IEDIEmailTriggeringRules triggeringRules)
		{
			this.TriggeringRules = triggeringRules;
		}

		IEDIEmailTriggeringRules TriggeringRules { get; }

		public static EDIEmailBuilder GetInstance(IEDIEmailTriggeringRulesProvider rulesProvider)
		{
			return new EDIEmailBuilder(rulesProvider?.TriggeringRules);
		}

		public static EDIEmailBuilder GetInstance(IEDIEmailTriggeringRules rules)
		{
			return new EDIEmailBuilder(rules);
		}

		public static void AddLogIfGenerationFailed(EnterpriseBusinessObject dataSource, IEDIEmailTemplateBuilder template)
		{
			if (dataSource != null)
			{
				var templateInfo = template.GetIEDIEmailTemplate();
				if (templateInfo != null)
				{
					dataSource.Logs.AddNew(Enterprise.ZArchitecture.Business.Events.EmailSent, string.Format(LogReference, templateInfo.TemplateDescription, templateInfo.TemplateCode));
				}
			}
		}
		public static string LogReference => "{0} suppressed due to {1} tag";

		public EmailDef BuildEmailDefByTemplate(IEDIEmailTemplateBuilder template)
		{
			if (template != null && !template.IsEmpty)
			{
				if (TriggeringRules == null || TriggeringRules.Allow(template))
				{
					var email = new EmailDef();
					email.Body = template.BuildBody();
					email.Subject = template.BuildSubject();
					email.Headers[SupportIncidentEmailHeaderConstants.Headers.XAutoResponseSuppress] = SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll;
					return email;
				}
				AddLogIfGenerationFailed(TriggeringRules?.DataSource, template);
			}
			return null;
		}

		public HtmlEmailDef BuildHtmlEmailDefByTemplate(IEDIEmailTemplateBuilder template)
		{
			if (template != null && !template.IsEmpty)
			{
				if (TriggeringRules == null || TriggeringRules.Allow(template))
				{
					var email = new HtmlEmailDef();
					email.Body = template.BuildBody();
					email.Subject = template.BuildSubject();
					email.Headers[SupportIncidentEmailHeaderConstants.Headers.XAutoResponseSuppress] = SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll;
					return email;
				}
				AddLogIfGenerationFailed(TriggeringRules?.DataSource, template);
			}
			return null;
		}
	}
}
