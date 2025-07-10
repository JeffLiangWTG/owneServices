using CargoWise.Data;

namespace Enterprise.Environment
{
	public sealed class ServiceManagerDbEnvironmentUnpooled : BaseDbEnvironment
	{
		public override IConnectionPooling ConnectionPooling { get; } = new NoConnectionPooling();
	}
}