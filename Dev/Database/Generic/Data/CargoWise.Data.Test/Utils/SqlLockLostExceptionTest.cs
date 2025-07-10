using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Data.Utils.Testing
{
	sealed class SqlLockLostExceptionTest : TestCase
	{
		public void TestIsCriticalException()
		{
			AssertEquals(true, new SqlLockLostException().IsCriticalException());
		}

		public void TestToStringHasStackTrace()
		{
			try
			{
				throw new SqlLockLostException("Test Exception", new[] { "lock" });
			}
			catch (SqlLockLostException exc)
			{
				AssertContains(exc.StackTrace, exc.ToString());
			}
		}
	}
}
