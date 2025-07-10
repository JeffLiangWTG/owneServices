using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class DpsConfidenceThresholdsRegistryItemTest_UpdateRegistryTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestUpdateRegistry()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				OrganisationsDataRegistry.Instance.MatchingConfidenceThresholdsForOrganisations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DpsConfidenceThresholdsBusinessObject());
				using (var cmd = testConnection.Command("SELECT SD_BinaryValue FROM dbo.StmData WHERE SD_Name = 'MatchingConfidenceThresholdsForOrganisations'"))
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						var bytes = reader[StmDataSchema.Constants.SD_BinaryValue];
						if (bytes != DBNull.Value)
						{
							var currentValue = System.Text.Encoding.ASCII.GetString((byte[])bytes).Replace("\0", "");
							var expectedValue = "<?xml version=\"1.0\" encoding=\"utf-16\"?><DpsConfidenceThresholdsBusinessObject><MediumThreshold>65</MediumThreshold><HighThreshold>80</HighThreshold></DpsConfidenceThresholdsBusinessObject>";
							AssertEquals(expectedValue, currentValue);
						}
						else
						{
							Fail("Invalid value for MatchingConfidenceThresholdsForOrganisations registry");
						}
					}
					else
					{
						Fail("No entry for MatchingConfidenceThresholdsForOrganisations registry");
					}
				}
			}
		}
	}
}
