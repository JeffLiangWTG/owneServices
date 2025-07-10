using CargoWise.Common.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	/// <summary>
	/// Generates unique SQL parameter names
	/// </summary>
	public static class SqlParameterNameGenerator
	{
		public static void Reset()
		{
			lock (instanceLock)
			{
				Counter = 0;
			}
		}

		public static string Next()
		{
			lock (instanceLock)
			{
				//	string G = Guid.NewGuid().ToString().Replace("-", "");
				if (Counter > 999999)
				{
					Counter = 0;
				}
				return (NoResString)"@p" + Counter++;
			}
		}

		[SuppressThreadStaticFieldMessage]
		static long Counter = 0;

		readonly static object instanceLock = new object();
	}
}
