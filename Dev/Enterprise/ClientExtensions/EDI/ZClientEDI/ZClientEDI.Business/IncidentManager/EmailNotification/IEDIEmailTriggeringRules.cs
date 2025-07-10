using Enterprise.ZArchitecture;

namespace Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate
{
	public interface IEDIEmailTriggeringRules
	{
		bool Allow(IEDIEmailTemplateBuilder emailTemplateBuilder);

		EnterpriseBusinessObject DataSource { get; }
	}
}
