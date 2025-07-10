using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using NUnit.Framework;

namespace CargoWise.Data.Utils.Testing
{
	sealed class GuidChunkerTest : TestCase
	{
		public void TestChunkerCanGenerateFiveBillionChunks()
		{
			BigInteger count = 0;
			foreach (var chunk in GuidChunker.GenerateChunks(1, 5000000000, null))
			{
				if (count == 100)
				{
					break;
				}
				count++;
			}
			Assert("The chunker does not have an out of memory issue when generating the bounds for potentially 5 billion chunks", true);
		}

		public void TestChunkerGeneratesCorrectAmountOfChunks_Evenly()
		{
			var count = 0;
			foreach (var chunk in GuidChunker.GenerateChunks(100000, 1000000, null))
			{
				count++;
			}
			AssertEquals("The chunker generates the correct amount of chunks when the amount fits evenly", 10, count);
		}

		public void TestChunkerGeneratesCorrectAmountOfChunks_NonEvenly()
		{
			var count = 0;
			foreach (var chunk in GuidChunker.GenerateChunks(100000, 1000001, null))
			{
				count++;
			}
			AssertEquals("The chunker generates the correct amount of chunks when the amount does not fit evenly", 11, count);
		}

		public void TestChunkerGeneratesCorrectGuidForHalfway()
		{
			var count = 0;
			foreach (var chunk in GuidChunker.GenerateChunks(1, 2, null))
			{
				if (count == 0)
				{
					AssertEquals("The Guid generated is the Guid for half way", new Guid("ffffffff-ffff-ffff-ffff-7fffffffffff"), chunk.UpperBound);
				}
				count++;
			}
			AssertEquals("There is 2 sets of guid bounds generated", 2, count);
		}

		public void TestChunkerDoesNotGenerateChunksWhenStartingAtMax()
		{
			var count = 0;
			foreach (var chunk in GuidChunker.GenerateChunks(1, 2, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff")))
			{
				count++;
			}
			AssertEquals("There are no guid bounds generated", 0, count);
		}

		public void TestChunkerGeneratesBoundsEvenWhenStartingAtMin()
		{
			var count = 0;
			foreach (var chunk in GuidChunker.GenerateChunks(1000, 1, null))
			{
				count++;
				AssertEquals("The chunker generates the correct lower bound", chunk.LowerBound, new Guid("00000000-0000-0000-0000-000000000000"));
				AssertEquals("The chunker generates the correct upper bound", chunk.UpperBound, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"));
			}
			AssertEquals("There is only 1 set of guid bounds generated", 1, count);
		}

		public void TestChunkerGeneratesCorrectBoundsWhenRowsPerChunkIsEqualToTotalRows()
		{
			var count = 0;
			var rowCount = 100;
			foreach (var chunk in GuidChunker.GenerateChunks(1, rowCount, null))
			{
				count++;
			}
			AssertEquals("There are the correct amount of guid bounds generated", rowCount, count);
		}

		public void TestChunkerGeneratesCorrectBoundsWhenRowsPerChunkIsMoreThanTotalRows()
		{
			var count = 0;
			var rowCount = 100;
			foreach (var chunk in GuidChunker.GenerateChunks(rowCount + 1, rowCount, null))
			{
				count++;
				AssertEquals("The chunker generates the correct lower bound", chunk.LowerBound, new Guid("00000000-0000-0000-0000-000000000000"));
				AssertEquals("The chunker generates the correct upper bound", chunk.UpperBound, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"));
			}
			AssertEquals("There is only 1 set of guid bounds generated", 1, count);
		}

		public void TestChunkerGeneratesCorrectBoundsWhenRowsPerChunkIsEqualToTotalRowsAndRowsAreOne()
		{
			var count = 0;
			foreach (var chunk in GuidChunker.GenerateChunks(1, 1, null))
			{
				count++;
				AssertEquals("The chunker generates the correct lower bound", chunk.LowerBound, new Guid("00000000-0000-0000-0000-000000000000"));
				AssertEquals("The chunker generates the correct upper bound", chunk.UpperBound, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"));
			}
			AssertEquals("There is only 1 set of guid bounds generated", 1, count);
		}

		public void TestChunkerGeneratesCorrectBoundsWhenRowsPerChunkIsMoreThanTotalRowsAndTotalRowsAreOne()
		{
			var count = 0;
			foreach (var chunk in GuidChunker.GenerateChunks(2, 1, null))
			{
				count++;
				AssertEquals("The chunker generates the correct lower bound", chunk.LowerBound, new Guid("00000000-0000-0000-0000-000000000000"));
				AssertEquals("The chunker generates the correct upper bound", chunk.UpperBound, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"));
			}
			AssertEquals("There is only 1 set of guid bounds generated", 1, count);
		}

		public void TestChunkerGeneratesCorrectBoundsWhenTotalRowsIsZero()
		{
			var count = 0;
			foreach (var chunk in GuidChunker.GenerateChunks(1000, 0, null))
			{
				count++;
				AssertEquals("The chunker generates the correct lower bound", chunk.LowerBound, new Guid("00000000-0000-0000-0000-000000000000"));
				AssertEquals("The chunker generates the correct upper bound", chunk.UpperBound, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"));
			}
			AssertEquals("There is only 1 set of guid bounds generated", 1, count);
		}

		public void TestChunkerThrowsErrorWhenTotalRowCountIsNegative()
		{
			AssertExceptionThrown("GenerateChunks threw an error", typeof(ArgumentException), () => GuidChunker.GenerateChunks(1, -1, null));
		}

		public void TestChunkerDoesNotThrowsErrorWhenTotalRowCountIsZero()
		{
			AssertNoExceptionThrown("GenerateChunks does not throw an error", () => GuidChunker.GenerateChunks(1, 0, null));
		}

		public void TestChunkerDoesNotThrowsErrorWhenTotalRowCountIsPositive()
		{
			AssertNoExceptionThrown("GenerateChunks does not throw an error", () => GuidChunker.GenerateChunks(1, 1, null));
		}

		public void TestChunkerThrowsErrorWhenChunkSizeIsNegative()
		{
			AssertExceptionThrown("GenerateChunks threw an error", typeof(ArgumentException), () => GuidChunker.GenerateChunks(-1, 1, null));
		}

		public void TestChunkerThrowsErrorWhenChunkSizeIsZero()
		{
			AssertExceptionThrown("GenerateChunks threw an error", typeof(ArgumentException), () => GuidChunker.GenerateChunks(0, 1, null));
		}

		public void TestChunkerDoesNotThrowsErrorWhenChunkSizeIsPositive()
		{
			AssertNoExceptionThrown("GenerateChunks does not throw an error", () => GuidChunker.GenerateChunks(1, 1, null));
		}

		public void TestChunkerGeneratesInclusiveBounds()
		{
			Guid? previousBound = null;
			foreach (var chunk in GuidChunker.GenerateChunks(100, 100000, null))
			{
				AssertNotEquals("The chunker does not generate duplicate bounds", previousBound, chunk.LowerBound);
				previousBound = chunk.UpperBound;
			}
		}

		public void TestChunkerGeneratesCorrectAmountOfChunkesWithUnevenChunkSizes()
		{
			var count = 0;
			foreach (var chunk in GuidChunker.GenerateChunks(283, 500000, null))
			{
				count++;
			}
			AssertEquals("The chunker generates the correct amount of chunks when the amount does not fit evenly", 1767, count);
		}

		public void TestChunkerDoesntIncludeGuidEmptyWhenPassingGuidEmpty()
		{
			foreach (var chunk in GuidChunker.GenerateChunks(100000, 1000000, Guid.Empty))
			{
				AssertNotEquals("The first chunk's lower bound is not Guid.Empty", Guid.Empty, chunk.LowerBound);
				break;
			}
		}

		public void TestChunkerDoesIncludeGuidEmptyWhenPassingNull()
		{
			foreach (var chunk in GuidChunker.GenerateChunks(100000, 1000000, null))
			{
				AssertEquals("The first chunk's lower bound is Guid.Empty", Guid.Empty, chunk.LowerBound);
				break;
			}
		}

		public void TestGuidChunkCanBeDeconstructed()
		{
			var chunk1 = new GuidChunk(Guid.NewGuid(), Guid.NewGuid());
			var chunk2 = new GuidChunk(Guid.NewGuid(), Guid.NewGuid());

			chunk1.Deconstruct(out var lowerBound1, out var upperBound1);
			var (lowerBound2, upperBound2) = chunk2;

			AssertEquals("Lower bound has been deconstructed properly", chunk1.LowerBound, lowerBound1);
			AssertEquals("Upper bound has been deconstructed properly", chunk1.UpperBound, upperBound1);

			AssertEquals("Lower bound has been deconstructed properly", chunk2.LowerBound, lowerBound2);
			AssertEquals("Upper bound has been deconstructed properly", chunk2.UpperBound, upperBound2);
		}

		public void TestGuidChunkCanBeDeconstructedWithPercentageCompleted()
		{
			var chunk1 = new GuidChunk(Guid.NewGuid(), Guid.NewGuid());
			var chunk2 = new GuidChunk(Guid.NewGuid(), Guid.NewGuid());

			chunk1.Deconstruct(out var lowerBound1, out var upperBound1, out var percentageCompleted1);
			var (lowerBound2, upperBound2, percentageCompleted2) = chunk2;

			AssertEquals("Lower bound has been deconstructed properly", chunk1.LowerBound, lowerBound1);
			AssertEquals("Upper bound has been deconstructed properly", chunk1.UpperBound, upperBound1);

			AssertEquals("Lower bound has been deconstructed properly", chunk2.LowerBound, lowerBound2);
			AssertEquals("Upper bound has been deconstructed properly", chunk2.UpperBound, upperBound2);
		}

		public void TestWhenChunking128BitIntegerLimitRowTableOneRowAtATimeChunkerDoesNotGetStuckInInfiniteLoop()
		{
			var max128BitBigInteger = GuidChunker.BigIntegerFromReadOnlySpan(Enumerable.Repeat((byte)0b11111111, 16).ToArray());

			var count = 0;

			foreach (var chunk in GuidChunker.GenerateChunks(1, max128BitBigInteger, null))
			{
				count++;

				if (count == 100)
				{
					AssertNotEquals("100th Chunk Lower Bound is not Guid.Empty", Guid.Empty, chunk.LowerBound);
					AssertNotEquals("100th Chunk Upper Bound is not Guid.Empty", Guid.Empty, chunk.UpperBound);

					break;
				}
			}
		}

		public void TestWhenChunking128BitIntegerLimitRowTableOneRowAtATimeBoundsAreTheSame()
		{
			var max128BitBigInteger = GuidChunker.BigIntegerFromReadOnlySpan(Enumerable.Repeat((byte)0b11111111, 16).ToArray());

			var count = 0;

			foreach (var chunk in GuidChunker.GenerateChunks(1, max128BitBigInteger, null))
			{
				count++;

				AssertEquals("Upper Bound and Lower Bound are the same", chunk.LowerBound, chunk.UpperBound);

				if (count == 100)
				{
					break;
				}
			}
		}

		public void TestWhenChunking128BitIntegerLimitRowTableOneRowAtATimeItGeneratesInclusiveBounds()
		{
			var max128BitBigInteger = GuidChunker.BigIntegerFromReadOnlySpan(Enumerable.Repeat((byte)0b11111111, 16).ToArray());

			var count = 0;
			Guid? previousBound = null;

			foreach (var chunk in GuidChunker.GenerateChunks(1, max128BitBigInteger, null))
			{
				count++;

				AssertNotEquals("Previous Upper Bound and Current Lower Bound are not the same", previousBound, chunk.LowerBound);

				previousBound = chunk.UpperBound;

				if (count == 100)
				{
					break;
				}
			}
		}

		public void TestConvertGuidToPercentageCompletionWithHalf()
		{
			var guid = GuidChunker.BigIntegerToGuid(GuidChunker.BigIntegerFromReadOnlySpan(Enumerable.Repeat((byte)0b11111111, 16).ToArray()) / 2);
			var percentage = GuidChunker.ConvertGuidToPercentageCompletion(guid);

			AssertEquals(50, percentage);
		}

		public void TestGuidChunkPercentageComplete()
		{
			var guid = GuidChunker.BigIntegerToGuid(GuidChunker.BigIntegerFromReadOnlySpan(Enumerable.Repeat((byte)0b11111111, 16).ToArray()) / 2);
			var chunk = new GuidChunk(guid, guid);

			AssertEquals(50, chunk.PercentageComplete);
		}

		public void TestGuidChunkPercentComplete()
		{
			var count = 0;

			foreach(var chunk in GuidChunker.GenerateChunks(500, 100000, null))
			{
				count++;
				AssertEquals(count / 2, chunk.PercentageComplete);
			}
		}

		public void TestGuidToBigInteger()
		{
			var maxGuid = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");
			var minGuid = Guid.Empty;
			var guid20 = new Guid("14000000-0000-0000-0000-000000000000");
			var max128BitBigInteger = BigInteger.Pow(2, 128) - 1;
			var bigIntMaxGuid = GuidChunker.GuidToBigInteger(maxGuid);
			var bigIntMinGuid = GuidChunker.GuidToBigInteger(minGuid);
			var bigInt20 = GuidChunker.GuidToBigInteger(guid20);
			CombineAssertions(() =>
			{
				AssertEquals(max128BitBigInteger, bigIntMaxGuid);
				AssertEquals(new BigInteger(0), bigIntMinGuid);
				AssertEquals(new BigInteger(20), bigInt20);
			});
		}

		public void TestBigIntegerToGuid()
		{
			var max128BitBigInteger = BigInteger.Pow(2, 128) - 1;
			var bigInt0 = new BigInteger(0);
			var bigInt20 = new BigInteger(20);
			var maxGuid = GuidChunker.BigIntegerToGuid(max128BitBigInteger);
			var minGuid = GuidChunker.BigIntegerToGuid(bigInt0);
			var guid20 = GuidChunker.BigIntegerToGuid(bigInt20);
			var expectedMaxGuid = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");
			var expectedGuid20 = new Guid("14000000-0000-0000-0000-000000000000");
			CombineAssertions(() =>
			{
				AssertEquals(expectedMaxGuid, maxGuid);
				AssertEquals(Guid.Empty, minGuid);
				AssertEquals(expectedGuid20, guid20);
			});
		}

		public void TestBigIntegerFromReadOnlySpan()
		{
			var expectedMax128BitBigInteger = BigInteger.Pow(2, 128) - 1;
			var expectedBigInt0 = new BigInteger(0);
			var expectedBigInt20 = new BigInteger(20);
			var max128BitBigInteger = GuidChunker.BigIntegerFromReadOnlySpan(Enumerable.Repeat((byte)0b11111111, 16).ToArray());
			var bigInt0 = GuidChunker.BigIntegerFromReadOnlySpan(Enumerable.Repeat((byte)0, 16).ToArray());
			var zeroedArray = Enumerable.Repeat((byte)0, 16).ToArray();
			zeroedArray[0] = 20;
			var bigInt20 = GuidChunker.BigIntegerFromReadOnlySpan(zeroedArray);
			CombineAssertions(() =>
			{
				AssertEquals(expectedMax128BitBigInteger, max128BitBigInteger);
				AssertEquals(expectedBigInt0, bigInt0);
				AssertEquals(expectedBigInt20, bigInt20);
			});
		}
	}

	sealed class GuidChunkerTransactionedTest : TransactionedTestCase
	{
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

		public void TestGuidChunkDoesNotOverlapOrHaveGaps()
		{
			var expectedRowCount = 5000;
			InsertRowIntoTestingTable(expectedRowCount);

			var rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, testTableName);
			var chunkSqlText = $"SELECT {testColumnName} FROM {testTableName} WHERE {testColumnName} BETWEEN @LowerBound AND @UpperBound";

			var guidSet = new HashSet<Guid>();

			foreach (var chunk in GuidChunker.GenerateChunks(100, rowCount, null))
			{
				using (var cmd = Db.Connection.Command(chunkSqlText))
				{
					cmd.AddParameter("@LowerBound", SqlDbType.UniqueIdentifier, chunk.LowerBound);
					cmd.AddParameter("@UpperBound", SqlDbType.UniqueIdentifier, chunk.UpperBound);

					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							AssertEquals(true, guidSet.Add((Guid)reader[0]));
						}
					}
				}
			}

			AssertEquals(expectedRowCount, guidSet.Count);

			var wholeSqlText = $"SELECT {testColumnName} FROM {testTableName}";

			using (var cmd = Db.Connection.Command(wholeSqlText))
			{
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						AssertEquals(true, guidSet.Remove((Guid)reader[0]));
					}
				}
			}

			AssertEquals(0, guidSet.Count);
		}

		readonly string testTableName = "GuidChunkerTestTable";
		readonly string testColumnName = "TEST_PK";

		void CreateTestingTable()
		{
			Db.Connection.ExecuteNonQuery($"CREATE TABLE {testTableName} ({testColumnName} UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY);");
		}
		void DropTestingTable()
		{
			Db.Connection.ExecuteNonQuery($"DROP TABLE {testTableName};");
		}

		void InsertRowIntoTestingTable(int rows)
		{
			var sqlText = $"INSERT INTO {testTableName} ({testColumnName}) VALUES (@PK)";

			for (var i = 0; i < rows; i++)
			{
				using (var cmd = Db.Connection.Command(sqlText))
				{
					cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
					cmd.ExecuteNonQuery();
				}
			}
		}
	}
}
