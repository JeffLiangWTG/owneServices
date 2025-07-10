using System;
using System.Data;
using System.Text;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.OceanCarrier;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.OceanCarrier;

[UseSnapshotProtection]
[TestedType(typeof(RenameCarrierShipmentToCarrierShipmentHeaderInGlowUseIndexingForModuleList))]
sealed class RenameCarrierShipmentToCarrierShipmentHeaderInGlowUseIndexingForModuleListTest : DataTransformationTestCase
{
	const string Separator = "◄◘►";

	protected override DataTransformation GetNewTestTransformationInstance() => new RenameCarrierShipmentToCarrierShipmentHeaderInGlowUseIndexingForModuleList();

	protected override void PrepareTestData()
	{
		var registrySetting = new[]
		{
			"CarrierShipment",
			"DtbConsignment",
			"JobConsol",
			"Containers",
			"WhsPickLine",
			"CusDec",
			"WhsItemDispatchLoadList",
			"WhsItemDispatchTransportationUnit",
			"HVLVConsignment",
			"CYDYardUnitState",
			"Orders",
			"Organisation",
			"CYDReceiveAdvice",
			"Project",
			"WhsItemReceiveASN",
			"WhsItemReceiveTransportationUnit",
			"CYDReleaseAdvice",
			"DtbConsignmentRunSheet",
			"ShipmentReceival",
			"JobShipment",
			"GlbStaff",
			"WhsItemTransferHeader",
			"CYDTransportationUnit",
			"WhsVASOrder",
			"BMBoard",
			"WorkItem",
		};
		TestConnection.ExecuteNonQuery("""
									   IF EXISTS (SELECT 1 FROM dbo.StmData WHERE SD_Name = 'GlowUseIndexingForModuleList')
									       UPDATE dbo.StmData SET SD_BinaryValue=@binaryParameter WHERE SD_Name = 'GlowUseIndexingForModuleList'
									   ELSE
									       INSERT INTO dbo.StmData VALUES(NEWID(), 'GlowUseIndexingForModuleList', NULL, NULL, 'SAR', 1, @binaryParameter, NULL, 0, 0, CURRENT_TIMESTAMP, 'E', CURRENT_TIMESTAMP, 'E')
									   """,
			cmd => cmd.AddParameter("@binaryParameter", SqlDbType.Binary, Encoding.Unicode.GetBytes(string.Join(Separator, registrySetting))));
	}

	protected override void AssertTransformationResults()
	{
		var setting = (byte[])Db.Connection.ExecuteScalar("SELECT SD_BinaryValue FROM dbo.StmData WHERE SD_Name = 'GlowUseIndexingForModuleList'");

		var settingCollection = Encoding.Unicode.GetString(setting)
			.Split([Separator,], StringSplitOptions.RemoveEmptyEntries);
		AssertCollectionNotContains("CarrierShipment", settingCollection);
		AssertCollectionContains("CarrierShipmentHeader", settingCollection);
	}

	public void TestSettingDoesNotExist()
	{
		TestRunAndAssertResultsTwice<object>(() =>
			{
				TestConnection.ExecuteNonQuery("DELETE FROM dbo.StmData WHERE SD_Name = 'GlowUseIndexingForModuleList'");
				return null;
			},
			_ => { },
			_ =>
			{
				var setting = Db.Connection.ExecuteScalar("SELECT SD_BinaryValue FROM dbo.StmData WHERE SD_Name = 'GlowUseIndexingForModuleList'");
				AssertNull(setting);
			});
	}

	public void TestSettingExistsAndIsNull()
	{
		TestRunAndAssertResultsTwice<object>(() =>
			{
				TestConnection.ExecuteNonQuery("""
											   IF EXISTS(SELECT 1 FROM dbo.StmData WHERE SD_Name = 'GlowUseIndexingForModuleList')
											       UPDATE dbo.StmData SET SD_BinaryValue = NULL WHERE SD_Name = 'GlowUseIndexingForModuleList'
											   ELSE
											       INSERT INTO dbo.StmData VALUES(NEWID(), 'GlowUseIndexingForModuleList', NULL, NULL, 'SAR', 1, NULL, NULL, 0, 0, CURRENT_TIMESTAMP, 'E', CURRENT_TIMESTAMP, 'E')
											   """);
				return null;
			},
			_ => { },
			_ =>
			{
				var setting = Db.Connection.ExecuteScalar("SELECT SD_BinaryValue FROM dbo.StmData WHERE SD_Name = 'GlowUseIndexingForModuleList'");
				AssertEquals(setting, DBNull.Value);
			});
	}

	public void TestSettingExistsAndDoesNotContainCarrierShipment()
	{
		TestRunAndAssertResultsTwice<object>(() =>
			{
				var registrySetting = new[] { "JobShipment", "GlbStaff", };
				TestConnection.ExecuteNonQuery("""
											   IF EXISTS(SELECT 1 FROM dbo.StmData WHERE SD_Name = 'GlowUseIndexingForModuleList')
											       UPDATE dbo.StmData SET SD_BinaryValue=@binaryParameter WHERE SD_Name = 'GlowUseIndexingForModuleList'
											   ELSE
											       INSERT INTO dbo.StmData VALUES(NEWID(), 'GlowUseIndexingForModuleList', NULL, NULL, 'SAR', 1, @binaryParameter, NULL, 0, 0, CURRENT_TIMESTAMP, 'E', CURRENT_TIMESTAMP, 'E')
											   """,
					(cmd) => cmd.AddParameter("@binaryParameter", SqlDbType.Binary, Encoding.Unicode.GetBytes(string.Join(Separator, registrySetting))));

				return null;
			},
			_ => { },
			_ =>
			{
				var setting = (byte[])Db.Connection.ExecuteScalar("SELECT SD_BinaryValue FROM dbo.StmData WHERE SD_Name = 'GlowUseIndexingForModuleList'");

				var settingCollection = Encoding.Unicode.GetString(setting)
					.Split([Separator,], StringSplitOptions.RemoveEmptyEntries);
				AssertCollectionNotContains("CarrierShipment", settingCollection);
				AssertCollectionNotContains("CarrierShipmentHeader", settingCollection);
			});
	}
}
