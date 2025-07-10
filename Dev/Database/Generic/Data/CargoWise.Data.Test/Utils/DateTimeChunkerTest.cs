using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Schema;
using Moq;
using NUnit.Framework;

namespace CargoWise.Data.Utils.Testing
{
	sealed class DateTimeChunkerTest : TransactionedTestCase
	{
		SchemaDateTimeColumn _dateTimeColumn;
		SchemaDateTimeColumn _smallDateTimeColumn;
		SchemaDateTimeColumn _dateTime2Column;
		SchemaDateTimeOffsetColumn _dateTimeOffsetColumn;

		Mock<ITableSchema> _tableSchema;

		protected override void SetUp()
		{
			base.SetUp();
			CreateTestingTable();

			_tableSchema = new Mock<ITableSchema>();
			_tableSchema.Setup(x => x.SqlSchemaName).Returns("dbo");
			_tableSchema.Setup(x => x.TableName).Returns(testTableName);

			_dateTimeColumn = new SchemaDateTimeColumn(_tableSchema.Object, testColumnName, 0, SqlDbType.DateTime, DBNull.Value, true, false, "dbo.TVP_datetime");

			_smallDateTimeColumn = new SchemaDateTimeColumn(_tableSchema.Object, testColumnName, 0, SqlDbType.SmallDateTime, DBNull.Value, true, false, "dbo.TVP_smalldatetime");

			_dateTime2Column = new SchemaDateTimeColumn(_tableSchema.Object, testColumnName, 0, SqlDbType.DateTime2, DBNull.Value, true, false, "dbo.TVP_datetime2");

			_dateTimeOffsetColumn = new SchemaDateTimeOffsetColumn(_tableSchema.Object, testColumnName, 0, SqlDbType.DateTimeOffset, DBNull.Value, true, 7, false, "dbo.TVP_datetimeoffset");
		}

		protected override void TearDown()
		{
			base.TearDown();
			DropTestingTable();
		}

		public void TestChunkerGeneratesCorrectAmountOfBounds()
		{
			ChangeTestingColumnType("DATETIME2");

			var rowCount = 1000;
			var chunkSize = 525;

			for (long i = 0; i < rowCount; i++)
			{
				InsertRowIntoTestingTable(new DateTime(i * 500));
			}

			AssertEquals("Correct amount of bounds are generated", (int)Math.Ceiling(rowCount / (double)chunkSize), DateTimeChunker.GenerateChunks(chunkSize, null, _dateTime2Column).Count());
		}

		public void TestChunkerGeneratesCorrectBounds_WhenChunkingOneRowAtATime()
		{
			InsertRowsIntoTestingTable(new List<DateTime?>() {
				new DateTime(2024, 10, 16, 12, 0, 0),
				new DateTime(2024, 10, 16, 12, 0, 1),
				new DateTime(2024, 10, 16, 12, 0, 2)
			});

			foreach (var chunk in DateTimeChunker.GenerateChunks(1, null, _dateTimeColumn))
			{
				AssertEquals("Bounds are the same", chunk.LowerBound, chunk.UpperBound);
			}
		}

		public void TestChunkerGeneratesCorrectBounds_WhenChunkingMultipleRows()
		{
			var dateTimes = new List<DateTime?>() {
				new DateTime(2024, 10, 16, 12, 0, 0),
				new DateTime(2024, 10, 16, 12, 0, 1),
				new DateTime(2024, 10, 16, 12, 0, 2),
				new DateTime(2024, 10, 16, 12, 0, 3),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 5),
				new DateTime(2024, 10, 16, 12, 0, 7),
				new DateTime(2024, 10, 16, 12, 0, 7),
				new DateTime(2024, 10, 16, 12, 0, 7),
				new DateTime(2024, 10, 16, 12, 0, 8)
			};

			var expectedDateTimeChunks = new DateTimeChunk[]
			{
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 0), new DateTime(2024, 10, 16, 12, 0, 1), 2),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 2), new DateTime(2024, 10, 16, 12, 0, 3), 2),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 4), new DateTime(2024, 10, 16, 12, 0, 4), 5),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 5), new DateTime(2024, 10, 16, 12, 0, 7), 4),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 8), new DateTime(2024, 10, 16, 12, 0, 8), 1)
			};

			InsertRowsIntoTestingTable(dateTimes);

			var count = 0;

			foreach (var chunk in DateTimeChunker.GenerateChunks(2, null, _dateTimeColumn))
			{
				AssertEquals(expectedDateTimeChunks[count], chunk);
				count++;
			}
		}

		public void TestChunkerGeneratesCorrectBounds_WhenChunkingWithVeryCloseDateTimes()
		{
			var dateTimes = new List<DateTime?>() {
				new DateTime(2024, 10, 16, 12, 0, 0),
				new DateTime(2024, 10, 16, 12, 0, 0).AddMilliseconds(1),
				new DateTime(2024, 10, 16, 12, 0, 0).AddMilliseconds(2),
				new DateTime(2024, 10, 16, 12, 0, 0).AddMilliseconds(3),
				new DateTime(2024, 10, 16, 12, 0, 0).AddMilliseconds(7),
				new DateTime(2024, 10, 16, 12, 0, 1),
			};

			var expectedDateTimeChunks = new DateTimeChunk[]
			{
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 0), new DateTime(2024, 10, 16, 12, 0, 0), 2),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 0).AddTicks(0033333), new DateTime(2024, 10, 16, 12, 0, 0).AddTicks(0033333), 2),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 0).AddTicks(0066667), new DateTime(2024, 10, 16, 12, 0, 1), 2)
			};

			InsertRowsIntoTestingTable(dateTimes);

			var count = 0;

			foreach (var chunk in DateTimeChunker.GenerateChunks(2, null, _dateTimeColumn))
			{
				AssertEquals(expectedDateTimeChunks[count], chunk);
				count++;
			}
		}

		public void TestChunkerGeneratesCorrectBounds_WhenTableHasMinAndMax()
		{
			var dateTimes = new List<DateTime?>() {
				(DateTime)SqlDateTime.MinValue,
				new DateTime(2024, 10, 16, 12, 0, 1),
				new DateTime(2024, 10, 16, 12, 0, 2),
				new DateTime(2024, 10, 16, 12, 0, 3),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 5),
				new DateTime(2024, 10, 16, 12, 0, 6),
				new DateTime(2024, 10, 16, 12, 0, 7),
				new DateTime(2024, 10, 16, 12, 0, 7),
				new DateTime(2024, 10, 16, 12, 0, 7),
				new DateTime(2024, 10, 16, 12, 0, 8),
				(DateTime)SqlDateTime.MaxValue
			};

			var expectedDateTimeChunks = new DateTimeChunk[]
			{
				new DateTimeChunk((DateTime)SqlDateTime.MinValue, new DateTime(2024, 10, 16, 12, 0, 1), 2),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 2), new DateTime(2024, 10, 16, 12, 0, 3), 2),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 4), new DateTime(2024, 10, 16, 12, 0, 4), 5),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 5), new DateTime(2024, 10, 16, 12, 0, 6), 2),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 7), new DateTime(2024, 10, 16, 12, 0, 7), 3),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 8), new DateTime(9999, 12, 31, 23, 59, 59).AddTicks(9966667), 2)
			};

			InsertRowsIntoTestingTable(dateTimes);

			var count = 0;

			foreach (var chunk in DateTimeChunker.GenerateChunks(2, null, _dateTimeColumn))
			{
				AssertEquals(expectedDateTimeChunks[count], chunk);
				count++;
			}
		}

		public void TestChunkerGeneratesInclusiveBounds()
		{
			ChangeTestingColumnType("DATETIME2");

			var rowCount = 1000;
			var chunkSize = 22;

			for (long i = 0; i < rowCount; i++)
			{
				InsertRowIntoTestingTable(new DateTime(i * 500));
			}

			DateTime? previousBound = null;

			foreach (var chunk in DateTimeChunker.GenerateChunks(chunkSize, null, _dateTime2Column))
			{
				AssertNotEquals("Previous Upper and Current Lower Bounds are not the same", previousBound, chunk.LowerBound);
				previousBound = chunk.UpperBound;
			}
		}

		public void TestChunkerDoesNotIncludeLastProcessedDate()
		{
			var dateTimes = new List<DateTime?>() {
				new DateTime(2024, 10, 16, 12, 0, 0),
				new DateTime(2024, 10, 16, 12, 0, 1),
				new DateTime(2024, 10, 16, 12, 0, 2),
				new DateTime(2024, 10, 16, 12, 0, 3),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 5)
			};
			InsertRowsIntoTestingTable(dateTimes);

			foreach (var chunk in DateTimeChunker.GenerateChunks(3, dateTimes[0], _dateTimeColumn))
			{
				AssertNotEquals("Bounds do not contain lastProcessedDate", dateTimes[0], chunk.LowerBound);
				break;
			}
		}

		public void TestChunkerReturnsCorrectBounds_WhenAllDateTimesAreTheSameInBatchSize_AndTableIsSameSizeAsBatchSize()
		{
			var dateTime = new DateTime(2024, 10, 16, 12, 0, 0);
			var dateTimes = Enumerable.Repeat<DateTime?>(dateTime, 500);

			InsertRowsIntoTestingTable(dateTimes);

			foreach (var chunk in DateTimeChunker.GenerateChunks(500, null, _dateTimeColumn))
			{
				AssertEquals("LowerBound is correct", dateTime, chunk.LowerBound);
				AssertEquals("UpperBound is correct", dateTime, chunk.UpperBound);
			}
		}

		public void TestChunkerReturnsCorrectBounds_WhenAllDateTimesAreTheSameInBatchSize_AndTableIsBiggerThanBatchSize()
		{
			var dateTimes = new[] { new DateTime(2024, 10, 16, 12, 0, 0), new DateTime(2024, 10, 16, 12, 0, 1), new DateTime(2024, 10, 16, 12, 0, 2) };

			foreach (var dateTime in dateTimes)
			{
				InsertRowsIntoTestingTable(Enumerable.Repeat<DateTime?>(dateTime, 50));
			}

			var count = 0;

			foreach (var chunk in DateTimeChunker.GenerateChunks(50, null, _dateTimeColumn))
			{
				AssertEquals("Bounds are the same", chunk.LowerBound, chunk.UpperBound);
				AssertEquals("LowerBound is correct", dateTimes[count], chunk.LowerBound);
				AssertEquals("UpperBound is correct", dateTimes[count], chunk.UpperBound);
				count++;
			}
		}

		public void TestChunkerReturnsCorrectBounds_WhenAllDateTimesAreTheSameInBatchSize_AndTableIsSmallerThanBatchSize()
		{
			var dateTime = new DateTime(2024, 10, 16, 12, 0, 0);
			var dateTimes = Enumerable.Repeat<DateTime?>(dateTime, 500);

			InsertRowsIntoTestingTable(dateTimes);

			foreach (var chunk in DateTimeChunker.GenerateChunks(501, null, _dateTimeColumn))
			{
				AssertEquals("LowerBound is correct", dateTime, chunk.LowerBound);
				AssertEquals("UpperBound is correct", dateTime, chunk.UpperBound);
			}
		}

		public void TestChunkerGeneratesCorrectBounds_OnSmallDateTimeTable_WhenChunkingMultipleRows()
		{
			ChangeTestingColumnType("SMALLDATETIME");

			var dateTimes = new List<DateTime?>() {
				new DateTime(2024, 10, 16, 12, 0, 0),
				new DateTime(2024, 10, 16, 12, 1, 0),
				new DateTime(2024, 10, 16, 12, 2, 0),
				new DateTime(2024, 10, 16, 12, 3, 0),
				new DateTime(2024, 10, 16, 12, 4, 0),
				new DateTime(2024, 10, 16, 12, 4, 0),
				new DateTime(2024, 10, 16, 12, 4, 0),
				new DateTime(2024, 10, 16, 12, 4, 0),
				new DateTime(2024, 10, 16, 12, 4, 0),
				new DateTime(2024, 10, 16, 12, 5, 0),
				new DateTime(2024, 10, 16, 12, 6, 0),
				new DateTime(2024, 10, 16, 12, 7, 0),
				new DateTime(2024, 10, 16, 12, 7, 0),
				new DateTime(2024, 10, 16, 12, 7, 0),
				new DateTime(2024, 10, 16, 12, 8, 0)
			};

			var expectedDateTimeChunks = new DateTimeChunk[]
			{
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 0), new DateTime(2024, 10, 16, 12, 1, 0), 2),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 2, 0), new DateTime(2024, 10, 16, 12, 3, 0), 2),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 4, 0), new DateTime(2024, 10, 16, 12, 4, 0), 5),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 5, 0), new DateTime(2024, 10, 16, 12, 6, 0), 2),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 7, 0), new DateTime(2024, 10, 16, 12, 7, 0), 3),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 8, 0), new DateTime(2024, 10, 16, 12, 8, 0), 1)
			};

			InsertRowsIntoTestingTable(dateTimes);

			var count = 0;

			foreach (var chunk in DateTimeChunker.GenerateChunks(2, null, _smallDateTimeColumn))
			{
				AssertEquals(expectedDateTimeChunks[count], chunk);
				count++;
			}
		}

		public void TestChunkerGeneratesCorrectBounds_OnSmallDateTimeTable_WhenTableHasMinAndMax()
		{
			ChangeTestingColumnType("SMALLDATETIME");

			var dateTimes = new List<DateTime?>() {
				new DateTime(1900, 01, 01, 0, 0, 0),
				new DateTime(2024, 10, 16, 12, 1, 0),
				new DateTime(2024, 10, 16, 12, 2, 0),
				new DateTime(2024, 10, 16, 12, 3, 0),
				new DateTime(2024, 10, 16, 12, 4, 0),
				new DateTime(2024, 10, 16, 12, 4, 0),
				new DateTime(2024, 10, 16, 12, 4, 0),
				new DateTime(2024, 10, 16, 12, 4, 0),
				new DateTime(2024, 10, 16, 12, 4, 0),
				new DateTime(2024, 10, 16, 12, 5, 0),
				new DateTime(2024, 10, 16, 12, 6, 0),
				new DateTime(2024, 10, 16, 12, 7, 0),
				new DateTime(2024, 10, 16, 12, 7, 0),
				new DateTime(2024, 10, 16, 12, 7, 0),
				new DateTime(2024, 10, 16, 12, 8, 0),
				new DateTime(2079, 06, 06, 23, 59, 0)
			};

			var expectedDateTimeChunks = new DateTimeChunk[]
			{
				new DateTimeChunk(new DateTime(1900, 01, 01, 0, 0, 0), new DateTime(2024, 10, 16, 12, 1, 0), 2),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 2, 0), new DateTime(2024, 10, 16, 12, 3, 0), 2),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 4, 0), new DateTime(2024, 10, 16, 12, 4, 0), 5),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 5, 0), new DateTime(2024, 10, 16, 12, 6, 0), 2),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 7, 0), new DateTime(2024, 10, 16, 12, 7, 0), 3),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 8, 0), new DateTime(2079, 06, 06, 23, 59, 0), 2)
			};

			InsertRowsIntoTestingTable(dateTimes);

			var count = 0;

			foreach (var chunk in DateTimeChunker.GenerateChunks(2, null, _smallDateTimeColumn))
			{
				AssertEquals(expectedDateTimeChunks[count], chunk);
				count++;
			}
		}

		public void TestChunkerGeneratesCorrectBounds_OnDateTimeOffsetTable_WhenChunkingMultipleRows()
		{
			ChangeTestingColumnType("DATETIMEOFFSET");

			var dateTimes = new List<DateTimeOffset?>() {
				new DateTimeOffset(2024, 10, 16, 12, 0, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 1, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 2, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 3, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 4, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 4, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 4, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 4, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 4, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 5, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 6, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 7, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 7, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 7, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 8, 0, 0, TimeSpan.FromHours(2))
			};

			var expectedDateTimeChunks = new DateTimeChunk[]
			{
				new DateTimeChunk(new DateTime(2024, 10, 16, 10, 0, 0), new DateTime(2024, 10, 16, 10, 1, 0), 2),
				new DateTimeChunk(new DateTime(2024, 10, 16, 10, 2, 0), new DateTime(2024, 10, 16, 10, 3, 0), 2),
				new DateTimeChunk(new DateTime(2024, 10, 16, 10, 4, 0), new DateTime(2024, 10, 16, 10, 4, 0), 5),
				new DateTimeChunk(new DateTime(2024, 10, 16, 10, 5, 0), new DateTime(2024, 10, 16, 10, 6, 0), 2),
				new DateTimeChunk(new DateTime(2024, 10, 16, 10, 7, 0), new DateTime(2024, 10, 16, 10, 7, 0), 3),
				new DateTimeChunk(new DateTime(2024, 10, 16, 10, 8, 0), new DateTime(2024, 10, 16, 10, 8, 0), 1)
			};

			InsertRowsIntoTestingTable(dateTimes);

			var count = 0;

			foreach (var chunk in DateTimeChunker.GenerateChunks(2, null, _dateTimeOffsetColumn))
			{
				AssertEquals(expectedDateTimeChunks[count], chunk);
				count++;
			}
		}

		public void TestChunkerGeneratesCorrectBounds_OnDateTimeOffsetTable_WhenTableHasMax()
		{
			ChangeTestingColumnType("DATETIMEOFFSET");

			var dateTimes = new List<DateTimeOffset?>() {
				new DateTimeOffset(2024, 10, 16, 12, 0, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 1, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 2, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 3, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 4, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 4, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 4, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 4, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 4, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 5, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 6, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 7, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 7, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 7, 0, 0, TimeSpan.FromHours(2)),
				new DateTimeOffset(2024, 10, 16, 12, 8, 0, 0, TimeSpan.FromHours(2)),
				DateTimeOffset.MaxValue
			};

			var expectedDateTimeChunks = new DateTimeChunk[]
			{
				new DateTimeChunk(new DateTime(2024, 10, 16, 10, 0, 0), new DateTime(2024, 10, 16, 10, 1, 0), 2),
				new DateTimeChunk(new DateTime(2024, 10, 16, 10, 2, 0), new DateTime(2024, 10, 16, 10, 3, 0), 2),
				new DateTimeChunk(new DateTime(2024, 10, 16, 10, 4, 0), new DateTime(2024, 10, 16, 10, 4, 0), 5),
				new DateTimeChunk(new DateTime(2024, 10, 16, 10, 5, 0), new DateTime(2024, 10, 16, 10, 6, 0), 2),
				new DateTimeChunk(new DateTime(2024, 10, 16, 10, 7, 0), new DateTime(2024, 10, 16, 10, 7, 0), 3),
				new DateTimeChunk(new DateTime(2024, 10, 16, 10, 8, 0), DateTimeOffset.MaxValue.UtcDateTime, 2)
			};

			InsertRowsIntoTestingTable(dateTimes);

			var count = 0;

			foreach (var chunk in DateTimeChunker.GenerateChunks(2, null, _dateTimeOffsetColumn))
			{
				AssertEquals(expectedDateTimeChunks[count], chunk);
				count++;
			}
		}

		public void TestChunkerGeneratesCorrectBounds_OnDateTime2Table_WhenChunkingMultipleRows()
		{
			ChangeTestingColumnType("DATETIME2");

			var dateTimes = new List<DateTime?>() {
				new DateTime(2024, 10, 16, 12, 0, 0),
				new DateTime(2024, 10, 16, 12, 0, 1),
				new DateTime(2024, 10, 16, 12, 0, 2),
				new DateTime(2024, 10, 16, 12, 0, 3),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 5),
				new DateTime(2024, 10, 16, 12, 0, 6),
				new DateTime(2024, 10, 16, 12, 0, 7),
				new DateTime(2024, 10, 16, 12, 0, 7),
				new DateTime(2024, 10, 16, 12, 0, 7),
				new DateTime(2024, 10, 16, 12, 0, 8)
			};

			var expectedDateTimeChunks = new DateTimeChunk[]
			{
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 0), new DateTime(2024, 10, 16, 12, 0, 1), 2),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 2), new DateTime(2024, 10, 16, 12, 0, 3), 2),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 4), new DateTime(2024, 10, 16, 12, 0, 4), 5),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 5), new DateTime(2024, 10, 16, 12, 0, 6), 2),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 7), new DateTime(2024, 10, 16, 12, 0, 7), 3),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 8), new DateTime(2024, 10, 16, 12, 0, 8), 1)
			};

			InsertRowsIntoTestingTable(dateTimes);

			var count = 0;

			foreach (var chunk in DateTimeChunker.GenerateChunks(2, null, _dateTime2Column))
			{
				AssertEquals(expectedDateTimeChunks[count], chunk);
				count++;
			}
		}

		public void TestChunkerGeneratesCorrectBounds_OnDateTime2Table_WhenTableHasMinAndMax()
		{
			ChangeTestingColumnType("DATETIME2");

			var dateTimes = new List<DateTime?>() {
				DateTime.MinValue,
				new DateTime(2024, 10, 16, 12, 0, 1),
				new DateTime(2024, 10, 16, 12, 0, 2),
				new DateTime(2024, 10, 16, 12, 0, 3),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 5),
				new DateTime(2024, 10, 16, 12, 0, 6),
				new DateTime(2024, 10, 16, 12, 0, 7),
				new DateTime(2024, 10, 16, 12, 0, 7),
				new DateTime(2024, 10, 16, 12, 0, 7),
				new DateTime(2024, 10, 16, 12, 0, 8),
				DateTime.MaxValue
			};

			var expectedDateTimeChunks = new DateTimeChunk[]
			{
				new DateTimeChunk(DateTime.MinValue, new DateTime(2024, 10, 16, 12, 0, 1), 2),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 2), new DateTime(2024, 10, 16, 12, 0, 3), 2),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 4), new DateTime(2024, 10, 16, 12, 0, 4), 5),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 5), new DateTime(2024, 10, 16, 12, 0, 6), 2),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 7), new DateTime(2024, 10, 16, 12, 0, 7), 3),
				new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 8), DateTime.MaxValue, 2)
			};

			InsertRowsIntoTestingTable(dateTimes);

			var count = 0;

			foreach (var chunk in DateTimeChunker.GenerateChunks(2, null, _dateTime2Column))
			{
				AssertEquals(expectedDateTimeChunks[count], chunk);
				count++;
			}
		}

		public void TestChunkerThrowsError_WhenPassingInNegativeChunkSize()
		{
			InsertRowsIntoTestingTable(new List<DateTime?>() {
				new DateTime(2024, 10, 16, 12, 0, 0),
				new DateTime(2024, 10, 16, 12, 0, 1),
				new DateTime(2024, 10, 16, 12, 0, 2),
				new DateTime(2024, 10, 16, 12, 0, 3),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 5)
			});

			AssertExceptionThrown("GenerateChunks throws an error", typeof(ArgumentOutOfRangeException), () => DateTimeChunker.GenerateChunks(-1, null, _dateTimeColumn));
		}

		public void TestChunkerThrowsError_WhenPassingInZeroChunkSize()
		{
			InsertRowsIntoTestingTable(new List<DateTime?>() {
				new DateTime(2024, 10, 16, 12, 0, 0),
				new DateTime(2024, 10, 16, 12, 0, 1),
				new DateTime(2024, 10, 16, 12, 0, 2),
				new DateTime(2024, 10, 16, 12, 0, 3),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 5)
			});

			AssertExceptionThrown("GenerateChunks throws an error", typeof(ArgumentOutOfRangeException), () => DateTimeChunker.GenerateChunks(0, null, _dateTimeColumn));
		}

		public void TestChunkerDoesNotThrowError_WhenPassingInPositiveChunkSize()
		{
			InsertRowsIntoTestingTable(new List<DateTime?>() {
				new DateTime(2024, 10, 16, 12, 0, 0),
				new DateTime(2024, 10, 16, 12, 0, 1),
				new DateTime(2024, 10, 16, 12, 0, 2),
				new DateTime(2024, 10, 16, 12, 0, 3),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 5)
			});

			AssertNoExceptionThrown("GenerateChunks doesn't throw an error", () => DateTimeChunker.GenerateChunks(20, null, _dateTimeColumn));
		}

		public void TestChunkerThrowsError_WhenPassingNonDateTimeDataType()
		{
			InsertRowsIntoTestingTable(new List<DateTime?>() {
				new DateTime(2024, 10, 16, 12, 0, 0),
				new DateTime(2024, 10, 16, 12, 0, 1),
				new DateTime(2024, 10, 16, 12, 0, 2),
				new DateTime(2024, 10, 16, 12, 0, 3),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 5)
			});

			AssertExceptionThrown("GenerateChunks throws an error", typeof(ArgumentException), () => DateTimeChunker.GenerateChunks(20, null, new SchemaDateTimeColumn(_tableSchema.Object, testColumnName, 0, SqlDbType.Int, DBNull.Value, true, false, "dbo.TVP_int")));
		}

		public void TestChunkerDoesNotThrowError_WhenPassingDateTimeDataType()
		{
			InsertRowsIntoTestingTable(new List<DateTime?>() {
				new DateTime(2024, 10, 16, 12, 0, 0),
				new DateTime(2024, 10, 16, 12, 0, 1),
				new DateTime(2024, 10, 16, 12, 0, 2),
				new DateTime(2024, 10, 16, 12, 0, 3),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 5)
			});

			AssertNoExceptionThrown("GenerateChunks does not throw an error", () => DateTimeChunker.GenerateChunks(20, null, new SchemaDateTimeColumn(_tableSchema.Object, testColumnName, 0, SqlDbType.DateTime, DBNull.Value, true, false, "dbo.TVP_datetime")));
		}

		public void TestChunkerThrowsError_WhenPassingDateTimeOffsetWithOutOfBoundsScale()
		{
			InsertRowsIntoTestingTable(new List<DateTime?>() {
				new DateTime(2024, 10, 16, 12, 0, 0),
				new DateTime(2024, 10, 16, 12, 0, 1),
				new DateTime(2024, 10, 16, 12, 0, 2),
				new DateTime(2024, 10, 16, 12, 0, 3),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 5)
			});

			AssertExceptionThrown("GenerateChunks throws an error", typeof(ArgumentOutOfRangeException), () => DateTimeChunker.GenerateChunks(20, null, new SchemaDateTimeOffsetColumn(_tableSchema.Object, testColumnName, 0, SqlDbType.DateTimeOffset, DBNull.Value, true, 8, false, "dbo.TVP_datetimeoffset")));
		}

		public void TestChunkerDoesNotThrowError_WhenPassingDateTimeOffsetWithInBoundsScale()
		{
			InsertRowsIntoTestingTable(new List<DateTime?>() {
				new DateTime(2024, 10, 16, 12, 0, 0),
				new DateTime(2024, 10, 16, 12, 0, 1),
				new DateTime(2024, 10, 16, 12, 0, 2),
				new DateTime(2024, 10, 16, 12, 0, 3),
				new DateTime(2024, 10, 16, 12, 0, 4),
				new DateTime(2024, 10, 16, 12, 0, 5)
			});

			for (byte i = 0; i < 8; i++)
			{
				AssertNoExceptionThrown("GenerateChunks does not throw an error", () => DateTimeChunker.GenerateChunks(20, null, new SchemaDateTimeOffsetColumn(_tableSchema.Object, testColumnName, 0, SqlDbType.DateTimeOffset, DBNull.Value, true, i, false, "dbo.TVP_datetimeoffset")));
			}
		}

		public void TestDateTimeChunk_CanBeDeconstructed()
		{
			var chunk1 = new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 0), new DateTime(2024, 10, 16, 12, 0, 1), 2);
			var chunk2 = new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 2), new DateTime(2024, 10, 16, 12, 0, 3), 2);

			chunk1.Deconstruct(out var lowerBound1, out var upperBound1);
			var (lowerBound2, upperBound2) = chunk2;

			AssertEquals("Lower bound has been deconstructed properly", chunk1.LowerBound, lowerBound1);
			AssertEquals("Upper bound has been deconstructed properly", chunk1.UpperBound, upperBound1);

			AssertEquals("Lower bound has been deconstructed properly", chunk2.LowerBound, lowerBound2);
			AssertEquals("Upper bound has been deconstructed properly", chunk2.UpperBound, upperBound2);
		}

		public void TestDateTimeChunk_CanBeDeconstructed_WithRowCount()
		{
			var chunk1 = new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 0), new DateTime(2024, 10, 16, 12, 0, 1), 2);
			var chunk2 = new DateTimeChunk(new DateTime(2024, 10, 16, 12, 0, 2), new DateTime(2024, 10, 16, 12, 0, 3), 2);

			chunk1.Deconstruct(out var lowerBound1, out var upperBound1, out var rowCount1);
			var (lowerBound2, upperBound2, rowCount2) = chunk2;

			AssertEquals("Lower bound has been deconstructed properly", chunk1.LowerBound, lowerBound1);
			AssertEquals("Upper bound has been deconstructed properly", chunk1.UpperBound, upperBound1);
			AssertEquals("Row count has been deconstructed properly", chunk1.RowCount, rowCount1);

			AssertEquals("Lower bound has been deconstructed properly", chunk2.LowerBound, lowerBound2);
			AssertEquals("Upper bound has been deconstructed properly", chunk2.UpperBound, upperBound2);
			AssertEquals("Row count has been deconstructed properly", chunk2.RowCount, rowCount2);
		}

		readonly string testTableName = "DateTimeChunkerTestTable";
		readonly string testColumnName = "TEST_SystemCreateTimeUtc";

		void CreateTestingTable()
		{
			Db.Connection.ExecuteNonQuery(@$"CREATE TABLE [{testTableName}] (
												[TEST_PK] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
												[{testColumnName}] DATETIME NULL
											);");
		}
		void DropTestingTable()
		{
			Db.Connection.ExecuteNonQuery(@$"DROP TABLE [{testTableName}];");
		}

		void ChangeTestingColumnType(string dataType)
		{
			Db.Connection.ExecuteNonQuery(@$"ALTER TABLE [{testTableName}] ALTER COLUMN [{testColumnName}] {dataType};");
		}

		void InsertRowIntoTestingTable(DateTime? dateTime)
		{
			var sqlText = @$"INSERT INTO [{testTableName}] ([{testColumnName}]) VALUES (@DateTime)";

			using (var cmd = Db.Connection.Command(sqlText))
			{
				cmd.AddParameter("@DateTime", SqlDbType.DateTime2, dateTime.HasValue ? dateTime : DBNull.Value);
				cmd.ExecuteNonQuery();
			}
		}

		void InsertRowIntoTestingTable(DateTimeOffset? dateTime)
		{
			var sqlText = @$"INSERT INTO [{testTableName}] ([{testColumnName}]) VALUES (@DateTime)";

			using (var cmd = Db.Connection.Command(sqlText))
			{
				cmd.AddParameter("@DateTime", SqlDbType.DateTimeOffset, dateTime.HasValue ? dateTime : DBNull.Value);
				cmd.ExecuteNonQuery();
			}
		}

		void InsertRowsIntoTestingTable(IEnumerable<DateTime?> dateTimes)
		{
			foreach (var dateTime in dateTimes)
			{
				InsertRowIntoTestingTable(dateTime);
			}
		}

		void InsertRowsIntoTestingTable(IEnumerable<DateTimeOffset?> dateTimes)
		{
			foreach (var dateTime in dateTimes)
			{
				InsertRowIntoTestingTable(dateTime);
			}
		}
	}
}

