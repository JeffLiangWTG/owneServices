namespace Enterprise.Integration
{
	public interface IOrganizationRegistryProvider
	{
		bool EnableImportFromCreditReports
		{
			get;
#if DEBUG
			set;
#endif
		}
	}
}
