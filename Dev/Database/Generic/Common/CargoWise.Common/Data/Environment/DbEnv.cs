using System.Diagnostics.CodeAnalysis;
using CargoWise.Common.Testing;

namespace CargoWise.Data
{
	public static class DbEnv
	{
		public static void SetDbEnvironment(IDbEnvironment dbEnvironment)
		{
			instance = dbEnvironment;
		}

		public static IDbEnvironment Instance
		{
			get
			{
				return instance;
			}
		}

#if DEBUG
		public static System.IDisposable SetTemporaryDbEnvironment(IDbEnvironment dbEnvironment)
		{
			var savedInstance = Instance;
			instance = dbEnvironment;

			return new Common.DisposableAction(() => instance = savedInstance);
		}
#endif

		[SuppressThreadStaticFieldMessage]
		[SuppressMessage("CargoWiseOne", "CW1021")]
		static IDbEnvironment instance = new BaseDbEnvironment();
	}
}
