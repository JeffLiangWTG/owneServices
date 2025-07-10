using System.Data;
using System.Data.Common;

namespace CargoWise.Data.Testing
{
	sealed class NonExecutableDbCommandForTest : DbCommand
	{
		internal NonExecutableDbCommandForTest(IDbConnection connection)
			: base("", new NullDbConnection(), connection, null, 5)
		{
		}

		public DbParameterCollection ExposedParameters
		{
			get
			{
				return InternalParameters;
			}
		}
	}
}
