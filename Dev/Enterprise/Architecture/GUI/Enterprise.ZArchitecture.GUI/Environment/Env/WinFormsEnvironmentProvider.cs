using System.Diagnostics.CodeAnalysis;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Environment
{
	public sealed class WinFormsEnvironmentProvider : EnvProvider
	{
		#region EnvProvider Members

		BaseEnvironment fInstance;
		readonly object fInstanceLock = new object();

		public override BaseEnvironment Instance
		{
			get
			{
				if (fInstance == null)
				{
					lock (fInstanceLock)
					{
						if (fInstance == null)
						{
							fInstance = new WinFormsEnvironment();
						}
					}
				}

				return fInstance;
			}
		}

		protected override IDbEnvironment GetDbEnvironmentInstance()
		{
			return Globals.IsUserInteractive ? new WinFormsDbEnvironment() : new BaseDbEnvironment();
		}

		#endregion EnvProvider

		[SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "Has already disposed the environment instance.")]
		protected override void Dispose(bool isDisposing)
		{
			base.Dispose(isDisposing);
			fInstance?.Dispose();
		}
	}
}
