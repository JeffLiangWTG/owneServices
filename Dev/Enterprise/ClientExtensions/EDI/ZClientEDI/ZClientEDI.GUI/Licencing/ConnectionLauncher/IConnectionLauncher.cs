using Enterprise.Upgrades;

namespace Enterprise.Client.EDI.Licencing.GUI
{
	interface IConnectionLauncher
	{
		bool ShowProgressForm { get; }
		void Launch(Progress progress);
	}
}
