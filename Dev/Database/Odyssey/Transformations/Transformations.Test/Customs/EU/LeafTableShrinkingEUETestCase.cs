using System;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Testing
{
	abstract class LeafTableShrinkingEUETestCase : DataTransformationTestCase
	{
		protected override IDisposable TransformationRunTwiceTestContextSetupAndDispose()
		{
			DbObjectCreator.CreateTableIfNotExists(Db.Connection, Db.DatabaseName, "CusEUEntryInstruction", CreateOldTableSql);
			return DisposableAction.NoAction;
		}

		const string CreateOldTableSql = @"
CREATE TABLE [CusEUEntryInstruction] (
	[EUE_PK] [uniqueidentifier] NOT NULL,
	[EUE_AutoVersion] [smallint] NOT NULL,
	[EUE_ClusterKey] [int] NOT NULL,
	[EUE_CEI] [uniqueidentifier] NOT NULL,
	[EUE_IsValid] [bit] NOT NULL,
	[EUE_ExportUnionSecretaryCode] [varchar](3) NOT NULL,
	[EUE_ExportUnionCode] [varchar](3) NOT NULL,
	[EUE_SystemCreateTimeUtc] [smalldatetime] NOT NULL,
	[EUE_SystemCreateUser] [varchar](3) NOT NULL,
	[EUE_SystemLastEditTimeUtc] [smalldatetime] NOT NULL,
	[EUE_SystemLastEditUser] [varchar](3) NOT NULL,
	[EUE_DedicatedGuaranteeAmount] [money] NOT NULL,
	[EUE_GuaranteeRatio] [decimal](5, 2) NOT NULL,
);";

		protected void AssertAddInfoField(string message, Guid instructionPk, string expectedAddInfo)
		{
			var script = $"SELECT CEI_AddInfo FROM dbo.CusEntryInstruction WHERE CEI_PK = '{instructionPk}'";
			using (var reader = TestConnection.Command(string.Format(script, instructionPk)).ExecuteReader())
			{
				reader.Read();
				var addInfo = reader.GetString(0);
				AssertEquals(message, expectedAddInfo, addInfo);
			}
		}
	}
}
