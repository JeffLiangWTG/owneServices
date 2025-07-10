using System;
using CargoWise.Data;

namespace Enterprise.Environment
{
	public sealed class ServiceManagerEnvProvider : EnvProvider
	{
		public ServiceManagerEnvProvider(bool usePooledConnection)
		{
			this.usePooledConnection = usePooledConnection;
		}

		public override BaseEnvironment Instance => instanceLazy.Value;

		protected override IDbEnvironment GetDbEnvironmentInstance()
		{
			return usePooledConnection
				? new ServiceManagerDbEnvironmentPooled()
				: new ServiceManagerDbEnvironmentUnpooled();
		}

		protected override void Dispose(bool isDisposing)
		{
			if (instanceLazy.IsValueCreated)
			{
				instanceLazy.Value.Dispose();
			}

			base.Dispose(isDisposing);
		}

		readonly Lazy<BaseEnvironment> instanceLazy = new Lazy<BaseEnvironment>(() => new ServiceTaskEnvironment());
		readonly bool usePooledConnection;
	}
}
