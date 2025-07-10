using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.ProductRegistration.Client.Test
{
	class DatabaseUniqueKeyProviderTest : TestCase
	{
		public void TestUniqueKey()
		{
			var provider = new DatabaseUniqueKeyProvider();
			var dbKey = provider.UniqueKey;
			Assert(Db.DatabaseName + " = " + dbKey.DatabaseName, 0 == string.Compare(Db.DatabaseName, dbKey.DatabaseName, true));
			Assert(Db.Connection.ServerNameReportedByDatabase + " = " + dbKey.ServerName, 0 == string.Compare(Db.Connection.ServerNameReportedByDatabase, dbKey.ServerName, true));
			AssertEquals(Db.ServerName, dbKey.ConnectionServerName);
		}

		public void TestUniqueKeyIsCachedOnSubsequentReads()
		{
			// Arrange
			var provider = new DatabaseUniqueKeyProvider();
			// Act
			var dbKey = provider.UniqueKey;
			var dbKey2 = provider.UniqueKey;
			// Assert
			Assert(ReferenceEquals(dbKey, dbKey2));
		}

		public void TestUniqueKeyChangedLoginTimeCausesUniqueKeyUpdate()
		{
			// Arrange
			var provider = new DatabaseUniqueKeyProvider();
			// Act
			var dbKey = provider.UniqueKey;
			var dbKey2 = provider.UniqueKey;
			Assert(ReferenceEquals(dbKey, dbKey2));
			Db.Connection.CloseConnection();
			Db.Connection.EnsureIsOpen();
			var dbKey3 = provider.UniqueKey;
			// Assert
			Assert(!ReferenceEquals(dbKey2, dbKey3));
		}
	}
}
