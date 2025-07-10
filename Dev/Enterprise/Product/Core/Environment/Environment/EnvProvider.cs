using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.Environment
{
	/// <summary>
	/// Base class for Environment providers.
	/// Providers provide and manage Environment Instances.
	/// Providers could provide singleton or pooled environments.
	/// In order to enable a provider you need to create a provider instance and call the Enable() method.
	/// </summary>
	public abstract class EnvProvider : Disposable
	{
		public abstract BaseEnvironment Instance { get; }
		protected abstract IDbEnvironment GetDbEnvironmentInstance();

		public void Enable()
		{
			Env.Provider = this;
			DbEnv.SetDbEnvironment(GetDbEnvironmentInstance());
		}

		protected override void Dispose(bool isDisposing)
		{
		}
	}
}
