using System;
using System.Data;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Shipment;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Shipment
{
	[TestedType(typeof(JobShipmentContainerAvailableStorage))]
	sealed class JobShipmentContainerAvailableStorageTest : DbCreateScriptTest
	{
		const string sql = "SELECT ShipmentPK, AllContainersFCLAvailableStorageOverridden, AllContainersLCLAvailableStorageOverridden FROM JobShipmentContainerAvailableStorage(@JS_PK)";
		readonly Guid clientPk = TestDbHelper.DefaultCompanyOrgProxyPK;
		readonly Guid orgAddressPk = Guid.NewGuid();
		readonly Guid fclContainerPk1 = Guid.NewGuid();
		readonly Guid fclContainerPk2a = Guid.NewGuid();
		readonly Guid fclContainerPk2b = Guid.NewGuid();
		Guid refContainerPk1;
		Guid refContainerPk2a;
		Guid refContainerPk2b;

		static readonly Guid masterJobShipmentPk1 = Guid.NewGuid();
		static readonly Guid masterPackLinePk1 = Guid.NewGuid();

		static readonly Guid subJobShipmentPk1 = Guid.NewGuid();
		static readonly Guid subPackLinePk1 = Guid.NewGuid();

		static readonly Guid masterJobShipmentPk2 = Guid.NewGuid();
		static readonly Guid masterPackLinePk2 = Guid.NewGuid();

		static readonly Guid subJobShipmentPk2a = Guid.NewGuid();
		static readonly Guid subPackLinePk2a = Guid.NewGuid();
		static readonly Guid subJobShipmentPk2b = Guid.NewGuid();
		static readonly Guid subPackLinePk2b = Guid.NewGuid();

		protected override void SetUp()
		{
			refContainerPk1 = (Guid)TestConnection.ExecuteScalar($"SELECT TOP 1 RC_PK FROM dbo.RefContainer WHERE RC_StorageClass = '40F'");
			refContainerPk2a = (Guid)TestConnection.ExecuteScalar($"SELECT TOP 1 RC_PK FROM dbo.RefContainer WHERE RC_StorageClass = '20F'");
			refContainerPk2b = (Guid)TestConnection.ExecuteScalar($"SELECT TOP 1 RC_PK FROM dbo.RefContainer WHERE RC_StorageClass = '40R'");

			TestConnection.ExecuteNonQuery($@"
			INSERT INTO dbo.OrgAddress
				([OA_PK],
				[OA_OH],
				[OA_Address1])
			VALUES
				('{orgAddressPk}',
				'{clientPk}',
				'Address 1')");

			TestConnection.ExecuteNonQuery($@"
			INSERT INTO dbo.JobShipment
				([JS_PK],
				[JS_UniqueConsignRef],
				[JS_IsCancelled],
				[JS_IsShipping],
				[JS_TransportMode],
				[JS_PackingMode])
			VALUES
				('{masterJobShipmentPk1}',
				'S00001005',
				0,
				0,
				'SEA',
				'FCL')");

			TestConnection.ExecuteNonQuery($@"
			INSERT INTO dbo.JobShipment
				([JS_PK],
				[JS_UniqueConsignRef],
				[JS_IsCancelled],
				[JS_IsShipping],
				[JS_TransportMode],
				[JS_PackingMode],
				[JS_JS_ColoadMasterShipment])
			VALUES
				('{subJobShipmentPk1}',
				'S00001006',
				0,
				0,
				'SEA',
				'FCL',
				'{masterJobShipmentPk1}')");

			TestConnection.ExecuteNonQuery($@"
			INSERT INTO dbo.JobContainer
				([JC_PK]
				,[JC_ContainerNum]
				,[JC_OverrideFCLAvailableStorage]
				,[JC_OverrideLCLAvailableStorage]
				,[JC_RC]
				,[JC_FCLAvailable])
			VALUES
				('{fclContainerPk1}',
				1,
				1,
				1,
				'{refContainerPk1}',
				'2024/06/15')");

			TestConnection.ExecuteNonQuery($@"
			INSERT INTO dbo.JobPackLines
				([JL_PK]
				,[JL_JS])
			VALUES
				('{masterPackLinePk1}',
				'{masterJobShipmentPk1}')");

			TestConnection.ExecuteNonQuery($@"
			INSERT INTO dbo.JobPackLines
				([JL_PK]
				,[JL_JS])
			VALUES
				('{subPackLinePk1}',
				'{subJobShipmentPk1}')");

			TestConnection.ExecuteNonQuery($@"
			INSERT INTO dbo.JobContainerPackPivot
				([J6_PK]
				,[J6_JC]
				,[J6_JL])
			VALUES
				(NEWID(),
				'{fclContainerPk1}',
				'{subPackLinePk1}')");

			TestConnection.ExecuteNonQuery($@"
			INSERT INTO dbo.JobShipment
				([JS_PK],
				[JS_UniqueConsignRef],
				[JS_IsCancelled],
				[JS_IsShipping],
				[JS_TransportMode],
				[JS_PackingMode])
			VALUES
				('{masterJobShipmentPk2}',
				'S00001007',
				0,
				0,
				'SEA',
				'FCL')");

			TestConnection.ExecuteNonQuery($@"
			INSERT INTO dbo.JobShipment
				([JS_PK],
				[JS_UniqueConsignRef],
				[JS_IsCancelled],
				[JS_IsShipping],
				[JS_TransportMode],
				[JS_PackingMode],
				[JS_JS_ColoadMasterShipment])
			VALUES
				('{subJobShipmentPk2a}',
				'S00001008',
				0,
				0,
				'SEA',
				'FCL',
				'{masterJobShipmentPk2}')");

			TestConnection.ExecuteNonQuery($@"
			INSERT INTO dbo.JobShipment
				([JS_PK],
				[JS_UniqueConsignRef],
				[JS_IsCancelled],
				[JS_IsShipping],
				[JS_TransportMode],
				[JS_PackingMode],
				[JS_JS_ColoadMasterShipment])
			VALUES
				('{subJobShipmentPk2b}',
				'S00001009',
				0,
				0,
				'SEA',
				'FCL',
				'{masterJobShipmentPk2}')");

			TestConnection.ExecuteNonQuery($@"
			INSERT INTO dbo.JobContainer
				([JC_PK]
				,[JC_ContainerNum]
				,[JC_OverrideFCLAvailableStorage]
				,[JC_OverrideLCLAvailableStorage]
				,[JC_RC]
				,[JC_FCLAvailable])
			VALUES
				('{fclContainerPk2a}',
				1,
				1,
				1,
				'{refContainerPk2a}',
				'2024/06/15')");

			TestConnection.ExecuteNonQuery($@"
			INSERT INTO dbo.JobContainer
				([JC_PK]
				,[JC_ContainerNum]
				,[JC_OverrideFCLAvailableStorage]
				,[JC_OverrideLCLAvailableStorage]
				,[JC_RC]
				,[JC_FCLAvailable])
			VALUES
				('{fclContainerPk2b}',
				1,
				0,
				0,
				'{refContainerPk2b}',
				'2024/06/15')");

			TestConnection.ExecuteNonQuery($@"
			INSERT INTO dbo.JobPackLines
				([JL_PK]
				,[JL_JS])
			VALUES
				('{masterPackLinePk2}',
				'{masterJobShipmentPk2}')");

			TestConnection.ExecuteNonQuery($@"
			INSERT INTO dbo.JobPackLines
				([JL_PK]
				,[JL_JS])
			VALUES
				('{subPackLinePk2a}',
				'{subJobShipmentPk2a}')");

			TestConnection.ExecuteNonQuery($@"
			INSERT INTO dbo.JobPackLines
				([JL_PK]
				,[JL_JS])
			VALUES
				('{subPackLinePk2b}',
				'{subJobShipmentPk2b}')");

			TestConnection.ExecuteNonQuery($@"
			INSERT INTO dbo.JobContainerPackPivot
				([J6_PK]
				,[J6_JC]
				,[J6_JL])
			VALUES
				(NEWID(),
				'{fclContainerPk2a}',
				'{subPackLinePk2a}')");

			TestConnection.ExecuteNonQuery($@"
			INSERT INTO dbo.JobContainerPackPivot
				([J6_PK]
				,[J6_JC]
				,[J6_JL])
			VALUES
				(NEWID(),
				'{fclContainerPk2b}',
				'{subPackLinePk2b}')");
		}

		public void TestAllContainersFCLAvailableStorageOverridden()
		{
			TestContainerStorageOverridden(masterJobShipmentPk1, "AllContainersFCLAvailableStorageOverridden", "1");
			TestContainerStorageOverridden(masterJobShipmentPk2, "AllContainersFCLAvailableStorageOverridden", "0");
			TestContainerStorageOverridden(subJobShipmentPk1, "AllContainersFCLAvailableStorageOverridden", "1");
			TestContainerStorageOverridden(subJobShipmentPk2b, "AllContainersFCLAvailableStorageOverridden", "0");
		}

		public void TestAllContainersLCLAvailableStorageOverridden()
		{
			TestContainerStorageOverridden(masterJobShipmentPk1, "AllContainersLCLAvailableStorageOverridden", "1");
			TestContainerStorageOverridden(masterJobShipmentPk2, "AllContainersLCLAvailableStorageOverridden", "0");
			TestContainerStorageOverridden(subJobShipmentPk1, "AllContainersLCLAvailableStorageOverridden", "1");
			TestContainerStorageOverridden(subJobShipmentPk2b, "AllContainersLCLAvailableStorageOverridden", "0");
		}

		void TestContainerStorageOverridden(Guid shipmentPk, string columnName, string expectedValue)
		{
			var builder = new StringBuilder();
			var value = string.Empty;

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@JS_PK", SqlDbType.UniqueIdentifier, shipmentPk);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						value = reader[columnName].ToString();
						builder.Append(value ?? "null");
					}
				}
			}

			AssertMultilineASCIIEquals($"{columnName} is incorrect", expectedValue, builder.ToString());
		}
	}
}
