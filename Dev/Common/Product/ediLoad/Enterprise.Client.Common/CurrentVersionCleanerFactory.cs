using CargoWise.Common;
using Enterprise.Upgrades;

namespace Enterprise.Client.Common
{
	public class CurrentVersionCleanerFactory : ICurrentVersionCleanerFactory
	{
		public ICurrentVersionCleaner GetCurrentVersionCleaner(ICurrentVersionCleanerConfig config)
		{
			Argument.NotNull(config, nameof(config));

			return new CurrentVersionCleaner(config);
		}
	}
}
