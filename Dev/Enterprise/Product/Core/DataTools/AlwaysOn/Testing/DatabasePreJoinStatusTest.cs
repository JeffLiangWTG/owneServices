using System;
using Enterprise.AlwaysOn.Setup;
using NUnit.Framework;

namespace Enterprise.AlwaysOn.Testing
{
	class DatabasePreJoinStatusTest : TestCase
	{
		public void TestAttributes()
		{
			var primaryDbFiles = new DatabaseFileCollection();
			primaryDbFiles.Add(new DatabaseFile("DataFile", "D:\\Folder\\Data.mdf", "D"));
			primaryDbFiles.Add(new DatabaseFile("LogFile", "L:\\Folder\\Log.ldf", "L"));
			var secondaryDbFiles = new DatabaseFileCollection();
			secondaryDbFiles.Add(new DatabaseFile("DataFile".ToUpperInvariant(), "D:\\Folder\\Data.mdf".ToUpperInvariant(), "D"));
			secondaryDbFiles.Add(new DatabaseFile("LogFile".ToUpperInvariant(), "L:\\Folder\\Log.ldf".ToUpperInvariant(), "L"));
			var differentSecondaryDbFiles = new DatabaseFileCollection();
			differentSecondaryDbFiles.Add(new DatabaseFile("DataFile", "D:\\Folder\\Data.mdf", "D"));
			differentSecondaryDbFiles.Add(new DatabaseFile("LogFile", "M:\\Folder\\Log.ldf", "L"));

			// No PRIMARY or SECONDARY file information
			AssertAttributes(
				primaryDbFileInfo: null,
				secondaryDbFileInfo: null,
				secondaryState: -1,
				secondaryStateDescription: "-",
				groupId: Guid.Empty, currentGroupId: Guid.Empty,
				expectedFilesMatch: false, expectedJoinLevel: PreJoinLevel.Cannot_be_joined);

			// PRIMARY but not SECONDARY file information
			AssertAttributes(
				primaryDbFileInfo: new DbFileAndTransactionLogInfo("SomeDb", 60004000035700003m, primaryDbFiles),
				secondaryDbFileInfo: null,
				secondaryState: -1,
				secondaryStateDescription: "-",
				groupId: Guid.Empty, currentGroupId: Guid.Empty,
				expectedFilesMatch: false, expectedJoinLevel: PreJoinLevel.Full_restore_required);

			// PRIMARY and SECONDARY file lists do not match
			AssertAttributes(
				primaryDbFileInfo: new DbFileAndTransactionLogInfo("SomeDb", 60004000035700003m, primaryDbFiles),
				secondaryDbFileInfo: new DbFileAndTransactionLogInfo("SomeDb", 50010000090800020m, differentSecondaryDbFiles),
				secondaryState: 1,
				secondaryStateDescription: "RESTORING",
				groupId: Guid.Empty, currentGroupId: Guid.Empty,
				expectedFilesMatch: false, expectedJoinLevel: PreJoinLevel.Full_restore_required);

			// PRIMARY and SECONDARY file lists match, LSNs differs
			AssertAttributes(
				primaryDbFileInfo: new DbFileAndTransactionLogInfo("SomeDb", 00000000000000001m, primaryDbFiles),
				secondaryDbFileInfo: new DbFileAndTransactionLogInfo("SomeDb", 00000000000000002m, secondaryDbFiles),
				secondaryState: 1,
				secondaryStateDescription: "RESTORING",
				groupId: Guid.Empty, currentGroupId: Guid.Empty,
				expectedFilesMatch: true, expectedJoinLevel: PreJoinLevel.Partial_restore_required);

			// PRIMARY and SECONDARY file lists and LSNs match
			AssertAttributes(
				primaryDbFileInfo: new DbFileAndTransactionLogInfo("SomeDb", 00000000000000003m, primaryDbFiles),
				secondaryDbFileInfo: new DbFileAndTransactionLogInfo("SomeDb", 00000000000000003m, secondaryDbFiles),
				secondaryState: 1,
				secondaryStateDescription: "RESTORING",
				groupId: Guid.Empty, currentGroupId: Guid.Empty,
				expectedFilesMatch: true, expectedJoinLevel: PreJoinLevel.Ready_to_be_joined);

			// PRIMARY and SECONDARY file lists and LSNs match, but SECONDARY database is not in recovery mode
			AssertAttributes(
				primaryDbFileInfo: new DbFileAndTransactionLogInfo("SomeDb", 00000000000000003m, primaryDbFiles),
				secondaryDbFileInfo: new DbFileAndTransactionLogInfo("SomeDb", 00000000000000003m, secondaryDbFiles),
				secondaryState: -1,
				secondaryStateDescription: "-",
				groupId: Guid.Empty, currentGroupId: Guid.Empty,
				expectedFilesMatch: true, expectedJoinLevel: PreJoinLevel.Full_restore_required);

			// PRIMARY and SECONDARY file lists and LSNs match, but database is already part of another availability group
			AssertAttributes(
				primaryDbFileInfo: new DbFileAndTransactionLogInfo("SomeDb", 00000000000000003m, primaryDbFiles),
				secondaryDbFileInfo: new DbFileAndTransactionLogInfo("SomeDb", 00000000000000003m, secondaryDbFiles),
				secondaryState: 1,
				secondaryStateDescription: "RESTORING",
				groupId: Guid.NewGuid(), currentGroupId: Guid.NewGuid(),
				expectedFilesMatch: true, expectedJoinLevel: PreJoinLevel.Cannot_be_joined);

			// PRIMARY and SECONDARY file lists and LSNs match, but database is already part of current availability group
			Guid groupId = Guid.NewGuid();
			AssertAttributes(
				primaryDbFileInfo: new DbFileAndTransactionLogInfo("SomeDb", 00000000000000003m, primaryDbFiles),
				secondaryDbFileInfo: new DbFileAndTransactionLogInfo("SomeDb", 00000000000000003m, secondaryDbFiles),
				secondaryState: 1,
				secondaryStateDescription: "RESTORING",
				groupId: groupId, currentGroupId: groupId,
				expectedFilesMatch: true, expectedJoinLevel: PreJoinLevel.Already_joined);
		}

		void AssertAttributes(DbFileAndTransactionLogInfo primaryDbFileInfo, DbFileAndTransactionLogInfo secondaryDbFileInfo, sbyte secondaryState, string secondaryStateDescription, Guid groupId, Guid currentGroupId, bool expectedFilesMatch, PreJoinLevel expectedJoinLevel)
		{
			IDatabasePreJoinStatus dbJoinStatus = new DatabasePreJoinStatus("SomeDb", secondaryDbFileInfo, primaryDbFileInfo, false, secondaryState, secondaryStateDescription, groupId, currentGroupId);

			AssertEquals("Name", "SomeDb", dbJoinStatus.Name);

			AssertLogSequenceNumber("LastPrimaryBackupLsn", primaryDbFileInfo, dbJoinStatus.LastPrimaryBackupLsn);

			AssertEquals("SecondaryExists?", (secondaryDbFileInfo != null), dbJoinStatus.SecondaryExists);
			AssertLogSequenceNumber("SecondaryRedoLsn", secondaryDbFileInfo, dbJoinStatus.SecondaryRedoLsn);
			AssertEquals("SecondaryState", secondaryState, dbJoinStatus.SecondaryState);
			AssertEquals("SecondaryStateDescription", secondaryStateDescription, dbJoinStatus.SecondaryStateDescription);

			AssertEquals("DbFilesMatch?", expectedFilesMatch, dbJoinStatus.DbFilesMatch);
			AssertEquals("JoinLevel", expectedJoinLevel, dbJoinStatus.JoinLevel);
		}

		void AssertLogSequenceNumber(string assertionMessage, DbFileAndTransactionLogInfo dbFileInfo, decimal actualLsn)
		{
			AssertEquals(assertionMessage, (dbFileInfo == null) ? -1m : dbFileInfo.LogSequenceNumber, actualLsn);
		}
	}
}
