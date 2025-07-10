using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ClientSpecific.EDI
{
	[TestedType(typeof(UpdateClientChargeableUsageSubCodeToUpperCaseTransform))]
	public class UpdateClientChargeableUsageSubCodeToUpperCaseTransformTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var clientChargeableUsageTestDataQuery = $"SELECT COUNT(*) FROM ClientChargeableUsage WHERE U1_SubCode COLLATE Latin1_General_BIN = 'ABC';";
			AssertEquals("3 records in clientChargeableUsages should be set to upper case", 3, TestConnection.ExecuteScalar<int>(clientChargeableUsageTestDataQuery));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateClientChargeableUsageSubCodeToUpperCaseTransform();
		}

		readonly List<Guid> clientChargeableUsageGuids = new List<Guid>
		{
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
		};

		protected override void PrepareTestData()
		{
			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "ClientChargeableUsage", @"
CREATE TABLE dbo.ClientChargeableUsage(
	[U1_PK] [uniqueidentifier] NOT NULL,
	[U1_Code] [varchar](3) NOT NULL DEFAULT(''),
	[U1_SubCode] [varchar](50) NOT NULL DEFAULT(''),
	[U1_PeriodStart] [smalldatetime] NOT NULL,
	[U1_UnitCount] [decimal](14, 4) NOT NULL DEFAULT(0),
	[U1_AH_Invoice] [uniqueidentifier] NULL,
	[U1_UnitPrice] [money] NOT NULL DEFAULT(0),
	[U1_LC] [uniqueidentifier] NULL,
	[U1_UpdateTime] [smalldatetime] NULL,
	[U1_InvoicedUnitCount] [decimal](14, 4) NOT NULL DEFAULT(0),
	[U1_Parent] [uniqueidentifier] NULL,
	[U1_Reference1] [varchar](50) NOT NULL DEFAULT(''),
	[U1_Reference2] [varchar](50) NOT NULL DEFAULT(''),
	[U1_Reference3] [varchar](50) NOT NULL DEFAULT(''),
	[U1_Reference4] [varchar](50) NOT NULL DEFAULT(''),
	[U1_LD] uniqueidentifier NULL,
	[U1_LCC] uniqueidentifier NULL,
	[U1_ManuallyProcessed] bit NOT NULL default(0),
	[U1_SystemCreateTimeUtc] [smalldatetime] NOT NULL DEFAULT GetUtcDate()
);");

			var dataQuery = $@"
INSERT INTO dbo.ClientChargeableUsage(U1_PK, U1_SubCode, U1_PeriodStart)
VALUES
('{clientChargeableUsageGuids[0]}', 'abc', '2024-7-15 00:00:00'),
('{clientChargeableUsageGuids[1]}', 'Abc', '2024-7-16 00:00:00'),
('{clientChargeableUsageGuids[2]}', 'ABC', '2024-7-17 00:00:00'),
('{clientChargeableUsageGuids[3]}', 'abc', '2024-6-15 00:00:00')
";
			using (var cmd = Db.Connection.Command(dataQuery))
			{
				cmd.ExecuteNonQuery();
			}
		}
	}
}
