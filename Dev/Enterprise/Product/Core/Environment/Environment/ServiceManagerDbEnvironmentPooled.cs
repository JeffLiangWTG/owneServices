using CargoWise.Data;

namespace Enterprise.Environment
{
	public sealed class ServiceManagerDbEnvironmentPooled : BaseDbEnvironment
	{
		public override IConnectionPooling ConnectionPooling { get; } = new PooledConnection();

		class PooledConnection : DefaultConnectionPooling
		{
			public override int MaxPoolSize => 1000;

			public override int MinPoolSize => 0;
		}
	}
}
