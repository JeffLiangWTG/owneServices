using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ClientSpecific.EDI
{
	[TestedType(typeof(UpdateEdiIdentityApplicationForNonExistingAzureApplication))]
	class UpdateEdiIdentityApplicationForNonExistingAzureApplicationTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals("The client id for '644D4A36-07FD-43E0-A4F1-F0539E456F62' should be cleaned", string.Empty, TestConnection.ExecuteScalar<string>($"SELECT IDA_ClientID FROM dbo.EdiIdentityApplication WHERE IDA_PK = '{PK}'"));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateEdiIdentityApplicationForNonExistingAzureApplication();
		}

		protected override void AssertPreConditions()
		{
			AssertEquals("there should be one record", 1, TestConnection.ExecuteScalar<int>("SELECT count(1) FROM dbo.EdiIdentityApplication WHERE IDA_ClientID = '644D4A36-07FD-43E0-A4F1-F0539E456F62'"));
		}

		protected override void PrepareTestData()
		{
			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "EdiIdentityApplication", @"
Create Table dbo.EdiIdentityApplication
(
IDA_PK uniqueidentifier NOT NULL,
IDA_LD uniqueidentifier NULL,
IDA_ClientID varchar(36) NOT NULL DEFAULT '',
IDA_IsRollback bit NOT NULL Default 0,
IDA_ApplicationName varchar(256) NOT NULL DEFAULT '',
IDA_RedirectUrlStatus varchar(3) NOT NULL DEFAULT 'NON',
IDA_RedirectUrlLastSyncTimeUtc datetime NULL,
IDA_IsActive bit NOT NULL Default 1,
IDA_SystemCreateUser varchar(3) NOT NULL DEFAULT '',
IDA_SystemCreateTimeUtc smalldatetime NULL,
IDA_SystemLastEditUser varchar(3) NOT NULL DEFAULT '',
IDA_SystemLastEditTimeUtc datetime NULL
);");

			var sql = $@"
INSERT INTO dbo.EdiIdentityApplication (IDA_PK, IDA_ClientID, IDA_SystemCreateTimeUtc, IDA_SystemCreateUser, IDA_SystemLastEditTimeUtc, IDA_SystemLastEditUser)
VALUES
('{PK}', '644D4A36-07FD-43E0-A4F1-F0539E456F62', GETDATE(), 'E', GETDATE(), 'E')
";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		Guid PK;
	}
}
