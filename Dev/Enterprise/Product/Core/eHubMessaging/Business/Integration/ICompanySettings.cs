namespace Enterprise.eHubMessaging.Business
{
	public interface ICompanySettings
	{
		bool PasswordExists { get; }
		string GetPassword();
	}
}