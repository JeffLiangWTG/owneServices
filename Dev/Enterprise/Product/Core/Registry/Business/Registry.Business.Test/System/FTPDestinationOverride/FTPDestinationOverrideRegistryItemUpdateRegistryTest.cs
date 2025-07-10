using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class FTPDestinationOverrideRegistryItemUpdateRegistryTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestUpdateRegistry()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				SystemDataRegistry.Instance.FTPDestinationOverride.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new FTPDestinationOverrideInfo("FtpAddressTest", "UserNameTest", "PasswordTest"));
				using (var cmd = testConnection.Command("SELECT SD_BinaryValue FROM dbo.StmData WHERE SD_Name = 'FTPDestinationOverride'"))
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						var bytes = reader[StmDataSchema.Constants.SD_BinaryValue];
						if (bytes != DBNull.Value)
						{
							var currentValue = System.Text.Encoding.ASCII.GetString((byte[])bytes).Replace("\0", "");
							var expectedValue = "<?xml version=\"1.0\" encoding=\"utf-16\"?><FTPDestinationOverrideInfo><FtpAddress>FtpAddressTest</FtpAddress><UserName>UserNameTest</UserName><Password>PasswordTest</Password></FTPDestinationOverrideInfo>";
							AssertEquals(expectedValue, currentValue);
						}
						else
						{
							Fail("Invalid value for FTPDestinationOverride registry");
						}
					}
					else
					{
						Fail("No entry for FTPDestinationOverride registry");
					}
				}
			}
		}
	}
}
