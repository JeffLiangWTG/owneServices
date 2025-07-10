using System;
using WTG.DeploymentUtils.FileSystem;

namespace Enterprise.Dat.Implementation
{
	public sealed class DeploymentProcessFactory : IDeploymentProcessFactory
	{
		public IDeploymentProcess Create(DeploymentConfiguration configuration)
		{
			_ = configuration ?? throw new ArgumentNullException(nameof(configuration));

#if DEBUG
			if (configuration.Name == "DatBackupFileUpdate")
			{
				return new DatBackupFileGenerator(configuration);
			}
			else if (configuration.Name == nameof(WinzorExplicitListUpdate))
			{
				return new WinzorExplicitListUpdate(configuration);
			}
			else if (configuration.Name == nameof(WinzorTestReport))
			{
				return new WinzorTestReport(configuration);
			}
#endif

			throw new NotImplementedException();
		}
	}
}
