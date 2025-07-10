namespace Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate
{
	public interface IEDIEmailTriggeringRulesProvider
	{
		IEDIEmailTriggeringRules TriggeringRules { get; }
	}
}
