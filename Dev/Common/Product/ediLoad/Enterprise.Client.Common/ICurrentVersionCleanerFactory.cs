using Enterprise.Upgrades;

namespace Enterprise.Client.Common
{
	public interface ICurrentVersionCleanerFactory
	{
		ICurrentVersionCleaner GetCurrentVersionCleaner(ICurrentVersionCleanerConfig config);
	}
}
