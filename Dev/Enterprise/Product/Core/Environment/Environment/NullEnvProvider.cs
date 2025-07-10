#if DEBUG
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Environment.Testing
{
	/// <summary>
	/// An EnvProvider that does nothing/returns null.
	/// </summary>
	public sealed class NullEnvProvider : EnvProvider, Enterprise.Integration.Environment.INullEnvProvider
	{
		public override BaseEnvironment Instance
		{
			get { return instance ?? (instance = new NullEnv()); }
		}
		NullEnv instance;

		protected override IDbEnvironment GetDbEnvironmentInstance()
		{
			return new BaseDbEnvironment();
		}

		protected override void Dispose(bool isDisposing)
		{
			base.Dispose(isDisposing);
			instance?.Dispose();
		}

		#region NullEnv

		public class NullEnv : BaseEnvironment
		{
			public NullEnv()
				: base(new MultiThreadUserContextManager())
			{
			}

			public override string ApplicationStartupPath
			{
				get { return null; }
			}

			public override IUserLoginController LoginController
			{
				get { return null; }
			}

			public override void ExitApplication()
			{
			}

			protected override Enterprise.Semaphores.Common.ISemaphoreProvider EnvironmentSpecificSemaphoreProvider
			{
				get { return null; }
			}
		}

		#endregion
	}
}
#endif
