using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ImmediatePrimaryKeyFetchHintTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			ZGuid pK = ZGuid.NewZGuid();
			ImmediateFetchHint fetchHint = new ImmediateFetchHint(typeof(DummyBusinessObject), DummyBizoSchema.PK, pK);
			string tableName = BusinessObjectFactory.GetTableNameFromType(typeof(DummyBusinessObject));
			AssertEquals(tableName, fetchHint.TableName);
			AssertEquals(DummyBizoSchema.PK, fetchHint.Column);
			AssertEquals(pK, fetchHint.Value);
		}

		public void TestGetHashString()
		{
			ZGuid pK = ZGuid.NewZGuid();
			FetchHint fetchHint = new ImmediateFetchHint(typeof(DummyBusinessObject), DummyBizoSchema.PK, pK);
			string key = "DummyBizoZ0_PK" + pK.ToString() + "CargoWise.EntityFramework.Testing.DummyBusinessObject";
			AssertEquals(key, fetchHint.GetHashKeyObject().ToString());
		}
	}
}
