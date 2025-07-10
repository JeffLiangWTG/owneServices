using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class IndexLoaderTest : TransactionedTestCase
	{
		public void TestGetIndexInfoReturnsNullIfNonExistingIndex()
		{
			AssertNull(IndexLoader.LoadTop1(Db.Connection, null, null, "AnInvalidIndexName"));
		}
	}
}
