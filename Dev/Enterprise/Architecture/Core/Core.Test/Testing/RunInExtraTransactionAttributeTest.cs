using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class RunInExtraTransactionAttributeTest : TestCase
	{
		[RunInExtraTransaction]
		public void TestWithAttribute()
		{
			AssertEquals(true, Db.Connection.IsInTransactionOtherThanTransactionedTestCase);
		}

		public void TestWithoutAttribute()
		{
			AssertEquals(false, Db.Connection.IsInTransactionOtherThanTransactionedTestCase);
		}
	}
}
