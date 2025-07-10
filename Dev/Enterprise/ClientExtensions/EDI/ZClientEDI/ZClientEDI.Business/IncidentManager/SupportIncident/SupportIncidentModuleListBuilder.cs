namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentModuleListBuilder : ModuleListBuilder
	{
		protected internal override IEnterpriseMenuSectionListBuilder GetNewEnterpriseMenuSectionListBuilder()
		{
			return new SupportIncidentEnterpriseMenuSectionListBuilder();
		}
	}
}

