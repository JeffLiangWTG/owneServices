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
	sealed class KeyCyclingUpgraderTestCase : TestCase
	{
		public void TestName()
		{
			AssertEquals("Key Cycler", upgrader.Name);
		}

		public void TestEstimatedNumberOfTasks()
		{
			AssertEquals(2, upgrader.EstimatedNumberOfTasks);
		}

		public void TestCreatesNonexistentKeys()
		{
			DeleteKey("GlowAuthEncryptionKey");
			DeleteKey("GlowAuthHmacKey");
			DeleteKey("LoginFailureAttemptSecretKey");
			DeleteKey("GlowOidcCodeVerifierSecretKey");

			upgrader.RunUpgrade();

			AssertKeyExistsWithLength("GlowAuthEncryptionKey", 32);
			AssertKeyExistsWithLength("GlowAuthHmacKey", 64);
			AssertKeyExistsWithLength("LoginFailureAttemptSecretKey", 64);
			AssertKeyExistsWithLength("GlowOidcCodeVerifierSecretKey", 64);
		}

		public void TestCyclesExistingKeys()
		{
			SetKey("GlowAuthEncryptionKey", new byte[] { 0x12, 0x34, 0x56, 0x78 });
			SetKey("GlowAuthHmacKey", new byte[] { 0x12, 0x34, 0x56, 0x78 });

			upgrader.RunUpgrade();

			AssertKeyExistsWithLength("GlowAuthEncryptionKey", 32);
			AssertKeyNotEquals("GlowAuthEncryptionKey", new byte[] { 0x12, 0x34, 0x56, 0x78 });

			AssertKeyExistsWithLength("GlowAuthHmacKey", 64);
			AssertKeyNotEquals("GlowAuthHmacKey", new byte[] { 0x12, 0x34, 0x56, 0x78 });
		}

		public void TestDoNotCycleLoginFailureAttemptSecretKey()
		{
			SetKey("LoginFailureAttemptSecretKey", new byte[] { 0x12, 0x34, 0x56, 0x78 });

			upgrader.RunUpgrade();

			AssertKeyEquals("LoginFailureAttemptSecretKey", new byte[] { 0x12, 0x34, 0x56, 0x78 });
		}

		public void TestSetLoginFailureAttemptSecretKeyWhenEmpty()
		{
			SetKey("LoginFailureAttemptSecretKey", Array.Empty<byte>());

			upgrader.RunUpgrade();

			AssertKeyExistsWithLength("LoginFailureAttemptSecretKey", 64);
		}

		public void TestSetLoginFailureAttemptSecretKeyWhenNull()
		{
			SetKey("LoginFailureAttemptSecretKey", null);

			upgrader.RunUpgrade();

			AssertKeyExistsWithLength("LoginFailureAttemptSecretKey", 64);
		}

		public void TestSetGlowOidcCodeVerifierSecretKeyWhenEmpty()
		{
			SetKey("GlowOidcCodeVerifierSecretKey", Array.Empty<byte>());

			upgrader.RunUpgrade();

			AssertKeyExistsWithLength("GlowOidcCodeVerifierSecretKey", 64);
		}

		public void TestSetGlowOidcCodeVerifierSecretKeyWhenNull()
		{
			SetKey("GlowOidcCodeVerifierSecretKey", null);

			upgrader.RunUpgrade();

			AssertKeyExistsWithLength("GlowOidcCodeVerifierSecretKey", 64);
		}

		KeyCyclingUpgrader upgrader;

		protected override void SetUp()
		{
			base.SetUp();

			upgrader = new KeyCyclingUpgrader(new DummyUpgradeManager(), Db.Connection);
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
				command.AddParameter("@Value", SqlDbType.VarBinary, value ?? (object)DBNull.Value);

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

		void AssertKeyNotEquals(string name, byte[] evilValue)
		{
			var value = ReadKeyValue(name);
			var areEqual = evilValue.SequenceEqual(value);
			Assert(string.Format(CultureInfo.InvariantCulture, "Key '{0}' should not be equal to evil value '{1}'", name, BitConverter.ToString(evilValue).Replace("-", string.Empty)), !areEqual);
		}

		void AssertKeyEquals(string name, byte[] expectedValue)
		{
			var value = ReadKeyValue(name);
			var areEqual = expectedValue.SequenceEqual(value);
			Assert(string.Format(CultureInfo.InvariantCulture, "Key '{0}' should be equal to value '{1}'", name, BitConverter.ToString(expectedValue).Replace("-", string.Empty)), areEqual);
		}

		void AssertKeyExistsWithLength(string name, int length)
		{
			var key = ReadKeyValue(name);
			Assert(string.Format(CultureInfo.InvariantCulture, "Key '{0}' should exist", name), key != null);
			Assert(string.Format(CultureInfo.InvariantCulture, "Key '{0}' should be {1} bytes long ({2} bits)", name, length, length * 8), key.Length == length);
		}
	}
}
