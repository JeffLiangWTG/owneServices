using System;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ArchiveManager;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ArchiveManager
{
	[TestedType(typeof(DeleteCusOutturnForDeletedJobShipment))]
	public class DeleteCusOutturnForDeletedJobShipmentTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new DeleteCusOutturnForDeletedJobShipment();

		Guid outturnHeaderPK = Guid.NewGuid();
		Guid otherOutturnHeaderPK = Guid.NewGuid();
		Guid nonExistentShipmentPK = Guid.NewGuid();
		Guid underbondPK = Guid.NewGuid();
		Guid outturnNotLinkedToHeaderPKButLinkedToDeletedUnderbond = Guid.NewGuid();
		Guid unrelatedHeaderPK = Guid.NewGuid();
		Guid nonExistentInvoiceLinePK = Guid.NewGuid();
		Guid unrelatedUnderbondPK = Guid.NewGuid();
		Guid unrelatedUnderbondWithNoHeaderParent = Guid.NewGuid();
		readonly DateTime outturnTime = new DateTime(2020, 5, 20, 10, 0, 0);

		protected override void PrepareTestData()
		{
			const int numberOfOutturnsToCreate = 123;

			_ = TestConnection.ExecuteNonQuery($"INSERT INTO dbo.CusOutturnHeader (C6_PK, C6_SystemCreateTimeUtc, C6_SystemCreateUser, C6_SystemLastEditTimeUtc, C6_SystemLastEditUser)" +
				$"VALUES ('{outturnHeaderPK}', GETDATE(), '~BP', GETDATE(), '~BP')");

			_ = TestConnection.ExecuteNonQuery($"INSERT INTO dbo.CusOutturnHeader (C6_PK, C6_VoyageNum, C6_SystemCreateTimeUtc, C6_SystemCreateUser, C6_SystemLastEditTimeUtc, C6_SystemLastEditUser)" +
				$"VALUES ('{otherOutturnHeaderPK}', '1', GETDATE(), '~BP', GETDATE(), '~BP')");

			_ = TestConnection.ExecuteNonQuery("DISABLE TRIGGER TG_CusOutturn_AuditDetailsAreNotMissing_Insert on CusOutturn");

			for (var i = 0; i < numberOfOutturnsToCreate; i++)
			{
				_ = TestConnection.ExecuteNonQuery($"INSERT INTO dbo.CusOutturn (C5_PK, C5_ParentID, C5_C6, C5_ParentTableCode, C5_SystemCreateTimeUtc, C5_SystemCreateUser, C5_SystemLastEditTimeUtc, C5_SystemLastEditUser) " +
					$"VALUES (NEWID(), '{nonExistentShipmentPK}', '{outturnHeaderPK}', 'JS', DATEADD(DAY, -{i}, '{SqlFormatInfo.ToSqlDateString(outturnTime)}'), '~BP', GETDATE(), '~BP')");
				_ = TestConnection.ExecuteNonQuery($"INSERT INTO dbo.CusOutturn (C5_PK, C5_ParentID, C5_C6, C5_ParentTableCode, C5_SystemCreateTimeUtc, C5_SystemCreateUser, C5_SystemLastEditTimeUtc, C5_SystemLastEditUser) " +
					$"VALUES (NEWID(), '{nonExistentShipmentPK}', '{otherOutturnHeaderPK}', 'JS', null, '~BP', GETDATE(), '~BP')");
				_ = TestConnection.ExecuteNonQuery($"INSERT INTO dbo.CusOutturn (C5_PK, C5_ParentID, C5_C6, C5_ParentTableCode, C5_SystemCreateTimeUtc, C5_SystemCreateUser, C5_SystemLastEditTimeUtc, C5_SystemLastEditUser) " +
					$"VALUES (NEWID(), '{nonExistentShipmentPK}', '{otherOutturnHeaderPK}', 'JS', DATEADD(DAY, -{i}, '{SqlFormatInfo.ToSqlDateString(outturnTime)}'),	'~BP', GETDATE(), '~BP')");
				_ = TestConnection.ExecuteNonQuery($"INSERT INTO dbo.CusOutturn (C5_PK, C5_ParentID, C5_C6, C5_ParentTableCode, C5_SystemCreateTimeUtc, C5_SystemCreateUser, C5_SystemLastEditTimeUtc, C5_SystemLastEditUser) " +
					$"VALUES (NEWID(), '{nonExistentShipmentPK}', NULL, 'JS', DATEADD(DAY, -{i}, '{SqlFormatInfo.ToSqlDateString(outturnTime)}'),	'~BP', GETDATE(), '~BP')");
				_ = TestConnection.ExecuteNonQuery($"INSERT INTO dbo.CusOutturn (C5_PK, C5_ParentID, C5_C6, C5_ParentTableCode, C5_SystemCreateTimeUtc, C5_SystemCreateUser, C5_SystemLastEditTimeUtc, C5_SystemLastEditUser) " +
					$"VALUES (NEWID(), '{nonExistentShipmentPK}', NULL, 'JS', null, '~BP', GETDATE(), '~BP')");
			}

			_ = TestConnection.ExecuteNonQuery("ENABLE TRIGGER TG_CusOutturn_AuditDetailsAreNotMissing_Insert on CusOutturn");

			_ = TestConnection.ExecuteNonQuery($"INSERT INTO dbo.CusUnderbond (C4_PK, C4_C6, C4_ApplicationCode, C4_SystemCreateTimeUtc, C4_SystemCreateUser, C4_SystemLastEditTimeUtc, C4_SystemLastEditUser)" +
				$"VALUES ('{unrelatedUnderbondWithNoHeaderParent}', NULL, 'CW1', GETDATE(), '~BP', GETDATE(), '~BP')");
			_ = TestConnection.ExecuteNonQuery($"INSERT INTO dbo.CusUnderbond (C4_PK, C4_SendersMessageReference, C4_C6, C4_ApplicationCode, C4_SystemCreateTimeUtc, C4_SystemCreateUser, C4_SystemLastEditTimeUtc, C4_SystemLastEditUser)" +
				$"VALUES ('{underbondPK}', 'B', '{outturnHeaderPK}', 'CW1', GETDATE(), '~BP', GETDATE(), '~BP')");
			_ = TestConnection.ExecuteNonQuery($"INSERT INTO dbo.CusOutturn (C5_PK, C5_C4_Underbond, C5_SystemCreateTimeUtc, C5_SystemCreateUser, C5_SystemLastEditTimeUtc, C5_SystemLastEditUser) " +
				$"VALUES ('{outturnNotLinkedToHeaderPKButLinkedToDeletedUnderbond}', '{underbondPK}', GETDATE(), '~BP', GETDATE(), '~BP')");

			_ = TestConnection.ExecuteNonQuery($"INSERT INTO dbo.CusOutturnHeader (C6_PK, C6_VoyageNum, C6_SystemCreateTimeUtc, C6_SystemCreateUser, C6_SystemLastEditTimeUtc, C6_SystemLastEditUser)" +
				$"VALUES ('{unrelatedHeaderPK}', 'A', GETDATE(), '~BP', GETDATE(), '~BP')");
			_ = TestConnection.ExecuteNonQuery($"INSERT INTO dbo.CusOutturn (C5_PK, C5_ParentID, C5_C6, C5_ParentTableCode, C5_SystemCreateTimeUtc, C5_SystemCreateUser, C5_SystemLastEditTimeUtc, C5_SystemLastEditUser) " +
				$"VALUES (NEWID(), '{nonExistentInvoiceLinePK}', '{unrelatedHeaderPK}', 'JI', GETDATE(), '~BP', GETDATE(), '~BP')");
			_ = TestConnection.ExecuteNonQuery($"INSERT INTO dbo.CusUnderbond (C4_PK, C4_SendersMessageReference, C4_C6, C4_ApplicationCode, C4_SystemCreateTimeUtc, C4_SystemCreateUser, C4_SystemLastEditTimeUtc, C4_SystemLastEditUser)" +
				$"VALUES ('{unrelatedUnderbondPK}', 'A', '{unrelatedHeaderPK}', 'CW1', GETDATE(), '~BP', GETDATE(), '~BP')");
		}

		protected override void AssertTransformationResults()
		{
			Assert(!TestConnection.Exists($"FROM dbo.CusOutturnHeader WHERE C6_PK = '{outturnHeaderPK}'"));
			Assert(!TestConnection.Exists($"FROM dbo.CusOutturnHeader WHERE C6_PK = '{otherOutturnHeaderPK}'"));
			Assert(!TestConnection.Exists($"FROM dbo.CusOutturn WHERE C5_ParentID = '{nonExistentShipmentPK}' OR C5_PK = '{outturnNotLinkedToHeaderPKButLinkedToDeletedUnderbond}'"));
			Assert(!TestConnection.Exists($"FROM dbo.CusUnderbond WHERE C4_PK = '{underbondPK}'"));
			Assert(TestConnection.Exists($"FROM dbo.CusOutturnHeader WHERE C6_PK = '{unrelatedHeaderPK}'"));
			Assert(TestConnection.Exists($"FROM dbo.CusOutturn WHERE C5_ParentID = '{nonExistentInvoiceLinePK}'"));
			Assert(TestConnection.Exists($"FROM dbo.CusUnderbond WHERE C4_PK = '{unrelatedUnderbondPK}'"));
			Assert(TestConnection.Exists($"FROM dbo.CusUnderbond WHERE C4_PK = '{unrelatedUnderbondWithNoHeaderParent}'"));

			var watermarkString = ExtProperty.Database.Select(Db.Connection, "DeleteCusOutturnForDeletedJobShipmentWatermark");

			AssertNull("Watermark should be no longer exist", watermarkString);
		}
	}
}
