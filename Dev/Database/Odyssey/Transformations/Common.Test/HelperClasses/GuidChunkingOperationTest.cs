using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common.HelperClasses;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.Common.Test.HelperClasses
{
	[TestedType(typeof(GuidChunkingOperation))]
	public class GuidChunkingOperationTest : TransactionedTestCase
	{
		public void TestGuidChunkingOperationCallsProcessBatchWithExpectedArguments()
		{
			var chunkSize = 1000;
			var totalRowCount = new BigInteger(100000);
			var rawGuidChunker = GuidChunker.GenerateChunks(chunkSize, totalRowCount, null);
			var rawChunkEnumerator = rawGuidChunker.GetEnumerator();
			var expectedCalls = rawGuidChunker.Count();
			var actualCalls = 0;
			Action<Guid, Guid> processChunk = (lowerBound, upperBound) =>
			{
				actualCalls++;
				rawChunkEnumerator.MoveNext();
				var rawChunk = rawChunkEnumerator.Current;
				AssertEquals("Lower bound doesn't match raw chunker", rawChunk.LowerBound, lowerBound);
				AssertEquals("Upper bound doesn't match raw chunker", rawChunk.UpperBound, upperBound);
			};
			var chunkingOperation = new GuidChunkingOperation(null, chunkSize, totalRowCount, processChunk, testLastPkPropertyName);
			chunkingOperation.DoChunking();
			AssertEquals("Process chunk wasn't called the expected number of times", expectedCalls, actualCalls);
		}

		public void TestGuidChunkingOperationRestartsFromCorrectPositionAfterCancellation()
		{
			var chunkSize = 1000;
			var totalRowCount = new BigInteger(100000);
			var callsBeforeCancellation = 5;

			var tokenSource = new CancellationTokenSource();
			var lastPk = Guid.Empty;
			Action<Guid, Guid> cancelProcessingAfterNumberOfCalls = (lowerBound, upperBound) =>
			{
				callsBeforeCancellation--;
				if (callsBeforeCancellation <= 0)
				{
					lastPk = upperBound;
					tokenSource.Cancel();
				}
			};

			var chunkingOperation = new GuidChunkingOperation(null, chunkSize, totalRowCount, cancelProcessingAfterNumberOfCalls, testLastPkPropertyName, tokenSource.Token);
			var cancelled = false;
			try
			{
				chunkingOperation.DoChunking();
			}
			catch (OperationCanceledException)
			{
				cancelled = true;
			}
			Assert("Task didn't cancel", cancelled);

			var rawChunker = GuidChunker.GenerateChunks(chunkSize, totalRowCount, lastPk);
			var rawChunkerChunk = rawChunker.First();
			var called = false;
			Action<Guid, Guid> assertProgressResumesFromLastPk = (lowerBound, upperBound) =>
			{
				if (!called)
				{
					called = true;
					AssertEquals("Chunking should start where the previous operation left off for lower bound", rawChunkerChunk.LowerBound, lowerBound);
					AssertEquals("Chunking should start where the previous operation left off for upper bound", rawChunkerChunk.UpperBound, upperBound);
				}
			};
			var newChunkingOperation = new GuidChunkingOperation(null, chunkSize, totalRowCount, assertProgressResumesFromLastPk, testLastPkPropertyName);
			newChunkingOperation.DoChunking();
		}

		public void TestGuidChunkingOperationLogsCorrectMessageWhenProcessedHalfOfChunks()
		{
			var expectedPercentage = 50;
			var chunkCountToCancel = 5;
			var chunkSize = 1000;
			var totalRowCount = 10000;
			AssertCorrectLogMessages(expectedPercentage, chunkCountToCancel, chunkSize, totalRowCount);
		}

		public void TestGuidChunkingOperationLogsCorrectMessageWhenProcessedQuarterOfChunks()
		{
			var expectedPercentage = 25;
			var chunkCountToCancel = 5;
			var chunkSize = 1000;
			var totalRowCount = 20000;
			AssertCorrectLogMessages(expectedPercentage, chunkCountToCancel, chunkSize, totalRowCount);
		}

		public void TestGuidChunkingOperationLogsCorrectMessageWhenProcessedThirdOfChunks()
		{
			var expectedPercentage = 33;
			var chunkCountToCancel = 10;
			var chunkSize = 1000;
			var totalRowCount = 30000;
			AssertCorrectLogMessages(expectedPercentage, chunkCountToCancel, chunkSize, totalRowCount);
		}

		void AssertCorrectLogMessages(int expectedPercentage, int chunkCountToCancel, int chunkSize, BigInteger totalRowCount)
		{
			var mockManager = new Mock<IUpgradeManager>();
			var messages = new List<string>();
			mockManager.Setup(m => m.ShowInfoMessage(It.IsAny<string>())).Callback<string>(str => messages.Add(str));
			var tokenSource = new CancellationTokenSource();
			var count = 0;
			Action<Guid, Guid> processChunk = (lowerBound, upperBound) =>
			{
				count++;
				if (count == chunkCountToCancel)
				{
					tokenSource.Cancel();
				}
			};
			var chunkingOperation = new GuidChunkingOperation(mockManager.Object, chunkSize, totalRowCount, processChunk, testLastPkPropertyName, tokenSource.Token);
			var cancelled = false;
			try
			{
				chunkingOperation.DoChunking();
			}
			catch (OperationCanceledException)
			{
				cancelled = true;
			}
			Assert("Task didn't cancel", cancelled);
			var messagesContents = string.Join("\n", messages);
			AssertCollectionContains($"Percent completion message should have been logged, messages logged:\n{messagesContents}", $"Percent Complete: {expectedPercentage}%", messages);
		}

		public void TestGuidChunkingOperationWorksWithSimpleTransform()
		{
			var chunkSize = 2;
			var totalRowCount = new BigInteger(10);
			var rows = new List<int>()
			{
				1,
				2,
				3,
				4,
				5,
				6,
				7,
				8,
				9,
				10,
			};
			InsertRowsIntoTestingTable(rows);
			var sqlText = @$"UPDATE {testTableName}
								SET {testColumnName} = 100
								WHERE TEST_PK > @fromPK AND TEST_PK < @toPK";
			Action<Guid, Guid> processChunk = (lowerBound, upperBound) =>
			{
				using var cmd = TestConnection.Command(sqlText);
				cmd.AddParameter("@fromPK", SqlDbType.UniqueIdentifier, lowerBound);
				cmd.AddParameter("@toPK", SqlDbType.UniqueIdentifier, upperBound);
				cmd.ExecuteNonQuery();
			};
			var chunkingOperation = new GuidChunkingOperation(null, chunkSize, totalRowCount, processChunk, testLastPkPropertyName);
			chunkingOperation.DoChunking();
			var assertionSqlText = @$"SELECT {testColumnName}
										FROM {testTableName}";
			Action<IDataRecord> assertionAction = (record) =>
			{
				AssertEquals("Record should have been updated", 100, record.GetInt32(0));
			};
			TestConnection.ExecuteReader(assertionSqlText, assertionAction);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CreateTestingTable();
		}

		protected override void TearDown()
		{
			base.TearDown();
			DropTestingTable();
		}

		readonly string testLastPkPropertyName = "GuidChunkingOperationTestLastPk";
		readonly string testTableName = "GuidChunkingOperationTestTable";
		readonly string testColumnName = "TEST_VALUE";

		void CreateTestingTable()
		{
			Db.Connection.ExecuteNonQuery(@$"CREATE TABLE [{testTableName}] (
												[TEST_PK] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
												[{testColumnName}] int
											);");
		}

		void DropTestingTable()
		{
			Db.Connection.ExecuteNonQuery(@$"DROP TABLE [{testTableName}];");
		}

		void InsertRowIntoTestingTable(int value)
		{
			var sqlText = @$"INSERT INTO [{testTableName}] ([{testColumnName}]) VALUES (@Value)";

			using (var cmd = Db.Connection.Command(sqlText))
			{
				cmd.AddParameter("@Value", SqlDbType.Int, value);
				cmd.ExecuteNonQuery();
			}
		}

		void InsertRowsIntoTestingTable(IEnumerable<int> values)
		{
			foreach (var value in values)
			{
				InsertRowIntoTestingTable(value);
			}
		}
	}
}
