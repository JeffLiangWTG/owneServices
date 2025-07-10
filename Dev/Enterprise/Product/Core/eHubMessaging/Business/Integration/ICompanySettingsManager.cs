using Enterprise.MasterFiles.Business;

namespace Enterprise.eHubMessaging.Business
{
	public interface ICompanySettingsManager
	{
		GlbCompany[] Companies { get; }
		ICompanySettings GetSetting(GlbCompany company);

		void ClearCache();
	}
}
