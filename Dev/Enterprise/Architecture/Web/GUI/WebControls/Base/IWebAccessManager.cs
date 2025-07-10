using Enterprise.Integration.Licensing;

namespace Enterprise.ZArchitecture.Web.GUI
{
	public interface IWebAccessManager
	{
		ILicenceCheckpoint[] LicenceCheckpoints(string pageRelativePage);
		ZGlobal AppInstance { get; }
		bool IsReportsPage(string pageRelativePath);
	}
}
