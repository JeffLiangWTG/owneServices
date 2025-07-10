namespace Enterprise.Registry.Business.Web
{
	public interface IAccessControlled
	{
		AccessControlRegistryItem SuppressionItem { get; }
		string GetRegistryCaption(string boundPropertyName);
		bool LoggedInOrgIs(string role);
		bool IsInTextSuppressionMode { get; set; }
	}
}
