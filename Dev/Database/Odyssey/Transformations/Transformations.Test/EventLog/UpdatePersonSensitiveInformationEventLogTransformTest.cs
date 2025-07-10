using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.EventLog;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.EventLog
{
	[TestedType(typeof(UpdatePersonSensitiveInformationEventLogTransform))]
	class UpdatePersonSensitiveInformationEventLogTransformTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			var transformation = new UpdatePersonSensitiveInformationEventLogTransform();
			transformation.Initialise(null, new DummyUpgradeManager());
			return transformation;
		}

		protected override void AssertTransformationResults()
		{
			var fullNameCount = $"SELECT COUNT(*) FROM dbo.StmALog WHERE SL_Reference = 'Changed PER_FullName.';";
			var legalNameCount = $"SELECT COUNT(*) FROM dbo.StmALog WHERE SL_Reference = 'Changed PER_LegalName.';";
			var birthDateCount = $"SELECT COUNT(*) FROM dbo.StmALog WHERE SL_Reference = 'Changed PER_BirthDate.';";
			var emailAddressCount = $"SELECT COUNT(*) FROM dbo.StmALog WHERE SL_Reference = 'Changed PER_EmailAddress.';";
			var mobilePhoneCount = $"SELECT COUNT(*) FROM dbo.StmALog WHERE SL_Reference = 'Changed PER_MobilePhone.';";
			var homePhoneCount = $"SELECT COUNT(*) FROM dbo.StmALog WHERE SL_Reference = 'Changed PER_HomePhone.';";
			AssertEquals(1, TestConnection.ExecuteScalar<int>(fullNameCount));
			AssertEquals(1, TestConnection.ExecuteScalar<int>(legalNameCount));
			AssertEquals(2, TestConnection.ExecuteScalar<int>(birthDateCount));
			AssertEquals(2, TestConnection.ExecuteScalar<int>(emailAddressCount));
			AssertEquals(2, TestConnection.ExecuteScalar<int>(mobilePhoneCount));
			AssertEquals(1, TestConnection.ExecuteScalar<int>(homePhoneCount));
		}

		/*protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdatePersonSensitiveInformationEventLogTransform();
		}*/

		protected override void PrepareTestData()
		{
			Db.Connection.ExecuteNonQuery($@"
INSERT INTO dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_IsEstimate, SL_IsCancelled, SL_Reference, SL_PostedTimeUtc, SL_EventTime, SL_GS_NKUser, SL_SE_NKEvent, SL_GB_NKBranch, SL_GE_NKDepartment, SL_FireWorkflow, SL_DataSource, SL_EventTimeUtc) VALUES
('{Guid.NewGuid()}', 'GlbStaff', '{Guid.NewGuid()}', 'N', 'N', 'Changed PER_FullName from:AAA to BBB.', '2023-12-01 00:00:00', '2023-12-01 10:00:00', '~BP', 'EDT', 'BNE', 'BRN', 0, 'C', null),
('{Guid.NewGuid()}', 'GlbPerson', '{Guid.NewGuid()}', 'N', 'N', 'Changed PER_LegalName from:CCC to DDD.', '2018-04-01 00:00:00', '2018-04-01 10:00:00', '~BP', 'EDT', 'BNE', 'BRN', 0, 'C', null),
('{Guid.NewGuid()}', 'GlbPerson', '{Guid.NewGuid()}', 'N', 'N', 'Changed PER_BirthDate from: to 26-Apr-81 00:00:00. from:AAA to BBB.', '2023-12-01 00:00:00', '2023-12-01 10:00:00', '~BP', 'EDT', 'BNE', 'BRN', 0, 'C', null),
('{Guid.NewGuid()}', 'GlbPerson', '{Guid.NewGuid()}', 'N', 'N', 'Changed PER_EmailAddress from:EEE@test.com to FFF@test.com.', '2023-12-01 00:00:00', '2023-12-01 10:00:00', '~BP', 'EDT', 'BNE', 'BRN', 0, 'C', null),
('{Guid.NewGuid()}', 'GlbPerson', '{Guid.NewGuid()}', 'N', 'N', 'Changed PER_MobilePhone from:+44 89652214 to .', '2023-12-01 00:00:00', '2023-12-01 10:00:00', '~BP', 'EDT', 'BNE', 'BRN', 0, 'C', null),
('{Guid.NewGuid()}', 'GlbPerson', '{Guid.NewGuid()}', 'N', 'N', 'Changed PYN PER_HomePhone from:666 to 999.', '2023-12-01 00:00:00', '2023-12-01 10:00:00', '~BP', 'EDT', 'BNE', 'BRN', 0, 'C', null),
('{Guid.NewGuid()}', 'GlbPerson', '{Guid.NewGuid()}', 'N', 'N', 'Changed PER_FullName.', GETUTCDATE(), GETDATE(), '~BP', 'EDT', 'BNE', 'BRN', 0, 'C', null),
('{Guid.NewGuid()}', 'GlbPerson', '{Guid.NewGuid()}', 'N', 'N', 'Changed PER_LegalName.', GETUTCDATE(), GETDATE(), '~BP', 'EDT', 'BNE', 'BRN', 0, 'C', null),
('{Guid.NewGuid()}', 'GlbPerson', '{Guid.NewGuid()}', 'N', 'N', 'Changed PER_BirthDate.', GETUTCDATE(), GETDATE(), '~BP', 'EDT', 'BNE', 'BRN', 0, 'C', null),
('{Guid.NewGuid()}', 'GlbPerson', '{Guid.NewGuid()}', 'N', 'N', 'Changed PER_EmailAddress.', GETUTCDATE(), GETDATE(), '~BP', 'EDT', 'BNE', 'BRN', 0, 'C', null),
('{Guid.NewGuid()}', 'GlbPerson', '{Guid.NewGuid()}', 'N', 'N', 'Changed PER_MobilePhone.', GETUTCDATE(), GETDATE(), '~BP', 'EDT', 'BNE', 'BRN', 0, 'C', null),
('{Guid.NewGuid()}', 'GlbPerson', '{Guid.NewGuid()}', 'N', 'N', 'Changed PER_HomePhone.', GETUTCDATE(), GETDATE(), '~BP', 'EDT', 'BNE', 'BRN', 0, 'C', null)
");
		}

		public void TestBatchingAndLogging()
		{
			AssertEquals(0, StmALog.ShallowLoadFromDB(TestConnection, d => d.SL_Reference == "Changed PER_FullName.").Length);

			var insertSql = @"
				INSERT INTO dbo.StmALog 
				(SL_PK, SL_Table, SL_Parent, SL_IsEstimate, SL_IsCancelled, SL_Reference, SL_PostedTimeUtc, SL_EventTime, SL_GS_NKUser, SL_SE_NKEvent, SL_GB_NKBranch, SL_GE_NKDepartment, SL_FireWorkflow, SL_DataSource, SL_EventTimeUtc) 
				VALUES 
				(@SL_PK, 'GlbPerson', @SL_Parent, 'N', 'N', 'Changed PER_FullName from:AAA to BBB.', @SL_PostedTimeUtc, @SL_EventTime, '~BP', 'EDT', 'BNE', 'BRN', 0, 'C', NULL)";
			var startTime = new DateTime(2018, 5, 1, 12, 0, 0);
			var eventTime = startTime;
			for (var i = 0; i < 1000; i++)
			{
				using (var cmd = Db.Connection.Command(insertSql))
				{
					cmd.AddParameter("@SL_PK", System.Data.SqlDbType.UniqueIdentifier, Guid.NewGuid());
					cmd.AddParameter("@SL_Parent", System.Data.SqlDbType.UniqueIdentifier, Guid.NewGuid());
					cmd.AddParameter("@SL_PostedTimeUtc", System.Data.SqlDbType.DateTime, eventTime);
					cmd.AddParameter("@SL_EventTime", System.Data.SqlDbType.DateTime, eventTime);
					cmd.ExecuteNonQuery();
				}
				eventTime = eventTime.AddHours(12);
			}

			var logger = new List<string>();
			var transformation = (IOnlineTransformation)GetNewTestTransformationInstance();

			for (var i = 1; i <= 100; i++)
			{
				AssertExceptionThrown<OperationCanceledException>(() => transformation.Run(s => logger.Add(s), new CancellationToken(canceled: true)));
				AssertEquals(Math.Min(1000, i * 14), StmALog.ShallowLoadFromDB(TestConnection, d => d.SL_Reference == "Changed PER_FullName.").Length);
			}

			var startDate = new DateTime(2018, 5, 8);
			var expectedMessages = new List<string>();
			for (var i = 0; i < 100; i++)
			{
				var message = $"Last processed batch endDate: {startDate.AddDays(i * 7):yyyy-MM-dd} 00:00:00.000.";
				expectedMessages.Add(message);
			}
			AssertContainsExactElementsInExactOrder(expectedMessages.ToArray(), logger);

			AssertEquals(1000, StmALog.ShallowLoadFromDB(TestConnection, d => d.SL_Reference == "Changed PER_FullName.").Length);
		}
	}
}
