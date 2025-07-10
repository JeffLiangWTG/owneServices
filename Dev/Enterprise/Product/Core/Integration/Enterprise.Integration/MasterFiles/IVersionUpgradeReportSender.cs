
namespace Enterprise.Integration
{
	/// <summary>
	/// Sends a basic version report immediately after a successful upgrade.
	/// Note defined in Enterprise.Masterfiles.Integration since it is consumed by SilentDbUpgraderDirector
	/// which did not otherwise need that dll.
	/// </summary>
	public interface IVersionUpgradeReportSender
	{
		void Send();
	}
}
