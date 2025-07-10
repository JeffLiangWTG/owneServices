using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	[UseSnapshotProtection]
	sealed class PTKeyUpgraderTestCase : TestCase
	{
		public void TestName()
		{
			AssertEquals("PT Key creation", upgrader.Name);
		}

		public void TestEstimatedNumberOfTasks()
		{
			AssertEquals(1, upgrader.EstimatedNumberOfTasks);
		}

		public void TestCreatesNonexistentKeys()
		{
			DeleteKey("PortugalCertificationKey");
			upgrader.RunUpgrade();
			AssertKeyExists("PortugalCertificationKey");
		}

		public void TestWhenKeyExists()
		{
			SetKey("PortugalCertificationKey", new byte[] { 0x12, 0x34, 0x56, 0x78 });
			upgrader.RunUpgrade();
			AssertKeyExists("PortugalCertificationKey");
			AssertKeyEquals("PortugalCertificationKey", new byte[] { 0x12, 0x34, 0x56, 0x78 });
		}

		PTKeyUpgrader upgrader;

		protected override void SetUp()
		{
			base.SetUp();

			upgrader = new PTKeyUpgrader(new DummyUpgradeManager(), Db.Connection);
		}

		protected override void TearDown()
		{
			upgrader = null;

			base.TearDown();
		}

		void DeleteKey(string name)
		{
			using (var command = Db.Connection.Command("DELETE dbo.StmData WHERE SD_Name = @Name"))
			{
				command.AddParameter("@Name", SqlDbType.VarChar, name);
				command.ExecuteNonQuery();
			}
		}

		void SetKey(string name, byte[] value)
		{
			using (var command = Db.Connection.Command("DELETE dbo.StmData WHERE SD_Name = @Name; INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_BinaryValue) VALUES (NEWID(), @Name, @Value);"))
			{
				command.AddParameter("@Name", SqlDbType.VarChar, name);
				command.AddParameter("@Value", SqlDbType.VarBinary, value);

				command.ExecuteNonQuery();
			}
		}

		byte[] ReadKeyValue(string name)
		{
			using (var command = Db.Connection.Command("SELECT SD_BinaryValue FROM dbo.StmData WHERE SD_Name = @Name"))
			{
				command.AddParameter("@Name", SqlDbType.VarChar, name);

				using (var reader = command.ExecuteReader())
				{
					var read = reader.Read();
					if (!read)
					{
						return null;
					}

					return (byte[])reader.GetValue(0);
				}
			}
		}

		void AssertKeyEquals(string name, byte[] valueToCompare)
		{
			var value = ReadKeyValue(name);
			var areEqual = valueToCompare.SequenceEqual(value);
			Assert(string.Format(CultureInfo.InvariantCulture, "Key '{0}' should be equal to value to compare '{1}'", name, BitConverter.ToString(valueToCompare).Replace("-", string.Empty)), areEqual);
		}

		void AssertKeyExists(string name)
		{
			var key = ReadKeyValue(name);
			Assert(string.Format(CultureInfo.InvariantCulture, "Key '{0}' should exist", name), key != null);
		}
	}
}
