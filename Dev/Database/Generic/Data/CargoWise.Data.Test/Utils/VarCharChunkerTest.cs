using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Schema;
using Moq;
using NUnit.Framework;

namespace CargoWise.Data.Utils.Testing
{
	sealed class VarCharChunkerTest : TransactionedTestCase
	{
		SchemaStringColumn _varCharColumn;

		Mock<ITableSchema> _tableSchema;

		protected override void SetUp()
		{
			base.SetUp();
			CreateTestingTable();

			_tableSchema = new Mock<ITableSchema>();
			_tableSchema.Setup(x => x.SqlSchemaName).Returns("dbo");
			_tableSchema.Setup(x => x.TableName).Returns(testTableName);

			_varCharColumn = new SchemaStringColumn(_tableSchema.Object, testColumnName, 0, SqlDbType.VarChar, DBNull.Value, true, 100, isLiteralOnly: false, tvpName: "dbo.TVP_varchar");
		}

		protected override void TearDown()
		{
			base.TearDown();
			DropTestingTable();
		}

		public void TestChunkerGeneratesCorrectAmountOfBounds()
		{
			var rowCount = 1000;
			var chunkSize = 525;

			for (int i = 0; i < rowCount; i++)
			{
				InsertRowIntoTestingTable(i.ToString());
			}

			AssertEquals("Correct amount of bounds are generated", 2, VarCharChunker.GenerateChunks(chunkSize, null, _varCharColumn).Count());
		}

		public void TestChunkerGeneratesCorrectBounds_WhenChunkingOneRowAtATime()
		{
			InsertRowsIntoTestingTable(new List<string>() {
				"ABC",
				"DEF",
				"GHI",
			});

			foreach (var actualChunk in VarCharChunker.GenerateChunks(1, null, _varCharColumn))
			{
				AssertEquals("Bounds are the same", actualChunk.LowerBound, actualChunk.UpperBound);
			}
		}

		public void TestChunkerGeneratesCorrectBounds_WhenChunkingMultipleRows()
		{
			var varChars = new List<string>() {
				"AAA",
				"BBB",
				"CCC",
				"DDD",
				"EEE",
				"EEE",
				"EEE",
				"EEE",
				"EEE",
				"FFF",
				"GGG",
				"GGG",
				"GGG",
				"HHH",
			};

			var expectedVarCharChunks = new VarCharChunk[]
			{
				new VarCharChunk("AAA", "BBB", 2),
				new VarCharChunk("CCC", "DDD", 2),
				new VarCharChunk("EEE", "EEE", 5),
				new VarCharChunk("FFF", "GGG", 4),
				new VarCharChunk("HHH", "HHH", 1),
			};

			InsertRowsIntoTestingTable(varChars);

			var count = 0;

			foreach (var actualChunk in VarCharChunker.GenerateChunks(2, null, _varCharColumn))
			{
				AssertEquals(expectedVarCharChunks[count], actualChunk);
				count++;
			}
		}

		public void TestChunkerGeneratesCorrectBounds_WhenChunkingWithVerySimilarVarChars()
		{
			var varChars = new List<string>() {
				"AAA",
				"AAA!",
				"AAA,",
				"AAA-",
				"AAA.",
				"AAAa",
				"AAAb",
				"AAAc",
				"AAAz",
				"BBB",
			};

			var expectedVarCharChunks = new VarCharChunk[]
			{
				new VarCharChunk("AAA", "AAA!", 2),
				new VarCharChunk("AAA,", "AAA-", 2),
				new VarCharChunk("AAA.", "AAAa", 2),
				new VarCharChunk("AAAb", "AAAc", 2),
				new VarCharChunk("AAAz", "BBB", 2),
			};

			InsertRowsIntoTestingTable(varChars);

			var count = 0;

			foreach (var actualChunk in VarCharChunker.GenerateChunks(2, null, _varCharColumn))
			{
				AssertEquals(expectedVarCharChunks[count], actualChunk);
				count++;
			}
		}
		
		public void TestChunkerGeneratesCorrectBounds_WhenTableHasMinAndMax()
		{
			var varChars = new List<string>() {
				string.Empty,
				"AAA",
				"BBB",
				"CCC",
				"EEE",
				"EEE",
				"EEE",
				"EEE",
				"EEE",
				"FFF",
				"GGG",
				"HHH",
				"HHH",
				"HHH",
				"III",
				new string('ÿ', 100), //largest possible value of VARCHAR in our test table
			};

			var expectedVarCharChunks = new VarCharChunk[]
			{
				new VarCharChunk(string.Empty, "AAA", 2),
				new VarCharChunk("BBB", "CCC", 2),
				new VarCharChunk("EEE", "EEE", 5),
				new VarCharChunk("FFF", "GGG", 2),
				new VarCharChunk("HHH", "HHH", 3),
				new VarCharChunk("III", new string('ÿ', 100), 2),
			};

			InsertRowsIntoTestingTable(varChars);

			var count = 0;

			foreach (var actualChunk in VarCharChunker.GenerateChunks(2, null, _varCharColumn))
			{
				AssertEquals(expectedVarCharChunks[count], actualChunk);
				count++;
			}
		}

		public void TestChunkerGeneratesInclusiveBounds()
		{
			var rowCount = 1000;
			var chunkSize = 22;

			for (int i = 0; i < rowCount; i++)
			{
				InsertRowIntoTestingTable(i.ToString());
			}

			string previousBound = null;

			foreach (var actualChunk in VarCharChunker.GenerateChunks(chunkSize, null, _varCharColumn))
			{
				AssertNotEquals("Previous Upper and Current Lower Bounds are not the same", previousBound, actualChunk.LowerBound);
				AssertGreaterThan("Current Lower Bound should be greater than the Previous Upper Bound", actualChunk.LowerBound, previousBound);
				previousBound = actualChunk.UpperBound;
			}
		}

		public void TestChunkerDoesNotIncludeLastProcessedVarChar()
		{
			var varChars = new List<string>() {
				"AAA",
				"BBB",
				"CCC",
				"DDD",
				"EEE",
				"FFF",
			};
			InsertRowsIntoTestingTable(varChars);

			var expectedVarCharChunks = new VarCharChunk[]
			{
				new VarCharChunk("CCC", "EEE", 3),
				new VarCharChunk("FFF", "FFF", 1),
			};

			int count = 0;

			foreach (var actualChunk in VarCharChunker.GenerateChunks(3, varChars[1], _varCharColumn))
			{
				if (count == 0)
				{
					AssertNotEquals("Bounds do not contain lastProcessedVarChar", varChars[1], actualChunk.LowerBound);
				}
				AssertEquals(expectedVarCharChunks[count], actualChunk);
				count++;
			}
		}

		public void TestChunkerReturnsCorrectBounds_WhenAllVarCharsAreTheSameInBatchSize_AndTableIsSameSizeAsBatchSize()
		{
			var varChar = "ABC";
			var varChars = Enumerable.Repeat(varChar, 500);

			InsertRowsIntoTestingTable(varChars);

			int count = 0;

			foreach (var actualChunk in VarCharChunker.GenerateChunks(500, null, _varCharColumn))
			{
				AssertEquals("LowerBound is correct", varChar, actualChunk.LowerBound);
				AssertEquals("UpperBound is correct", varChar, actualChunk.UpperBound);
				count++;
			}

			AssertEquals("Should only generate one chunk", 1, count);
		}

		public void TestChunkerReturnsCorrectBounds_WhenAllVarCharsAreTheSameInBatchSize_AndTableIsBiggerThanBatchSize()
		{
			var varChars = new[] { "ABC", "DEF", "GHI" };

			foreach (var varChar in varChars)
			{
				InsertRowsIntoTestingTable(Enumerable.Repeat(varChar, 50));
			}

			var count = 0;

			foreach (var actualChunk in VarCharChunker.GenerateChunks(50, null, _varCharColumn))
			{
				AssertEquals("Bounds are the same", actualChunk.LowerBound, actualChunk.UpperBound);
				AssertEquals("LowerBound is correct", varChars[count], actualChunk.LowerBound);
				AssertEquals("UpperBound is correct", varChars[count], actualChunk.UpperBound);
				count++;
			}
		}

		public void TestChunkerReturnsCorrectBounds_WhenAllVarCharsAreTheSameInBatchSize_AndTableIsSmallerThanBatchSize()
		{
			var varChar = "ABC";
			var varChars = Enumerable.Repeat(varChar, 500);

			InsertRowsIntoTestingTable(varChars);

			var count = 0;

			foreach (var actualChunk in VarCharChunker.GenerateChunks(501, null, _varCharColumn))
			{
				AssertEquals("LowerBound is correct", varChar, actualChunk.LowerBound);
				AssertEquals("UpperBound is correct", varChar, actualChunk.UpperBound);
				AssertEquals("RowCount is 500", 500, actualChunk.RowCount);
				count++;
			}

			AssertEquals("Should only generate one chunk", 1, count);
		}

		public void TestChunkerReturnsCorrectBounds_WhenThereAreNoVarCharsInTable()
		{
			var count = 0;

			foreach (var actualChunk in VarCharChunker.GenerateChunks(1, null, _varCharColumn))
			{
				AssertEquals("LowerBound is correct", string.Empty, actualChunk.LowerBound);
				AssertEquals("UpperBound is correct", new string('ÿ', 100), actualChunk.UpperBound);
				AssertEquals("RowCount is 0", 0, actualChunk.RowCount);
				count++;
			}

			AssertEquals("Should only generate one chunk", 1, count);
		}

		public void TestChunkerThrowsError_WhenPassingInNegativeChunkSize()
		{
			InsertRowsIntoTestingTable(new List<string>() {
				"AAA",
				"BBB",
				"CCC",
				"DDD",
				"EEE",
				"FFF",
			});

			AssertExceptionThrown("GenerateChunks throws an error", typeof(ArgumentOutOfRangeException), () => VarCharChunker.GenerateChunks(-1, null, _varCharColumn));
		}

		public void TestChunkerThrowsError_WhenPassingInZeroChunkSize()
		{
			InsertRowsIntoTestingTable(new List<string>() {
				"AAA",
				"BBB",
				"CCC",
				"DDD",
				"EEE",
				"FFF",
			});

			AssertExceptionThrown("GenerateChunks throws an error", typeof(ArgumentOutOfRangeException), () => VarCharChunker.GenerateChunks(0, null, _varCharColumn));
		}

		public void TestChunkerDoesNotThrowError_WhenPassingInPositiveChunkSize()
		{
			InsertRowsIntoTestingTable(new List<string>() {
				"AAA",
				"BBB",
				"CCC",
				"DDD",
				"EEE",
				"FFF",
			});

			AssertNoExceptionThrown("GenerateChunks doesn't throw an error", () => VarCharChunker.GenerateChunks(20, null, _varCharColumn));
		}

		public void TestChunkerThrowsError_WhenPassingNonVarCharDataType()
		{
			InsertRowsIntoTestingTable(new List<string>() {
				"AAA",
				"BBB",
				"CCC",
				"DDD",
				"EEE",
				"FFF",
			});

			AssertExceptionThrown("GenerateChunks throws an error", typeof(ArgumentException), () => VarCharChunker.GenerateChunks(20, null, new SchemaStringColumn(_tableSchema.Object, testColumnName, 0, SqlDbType.Int, DBNull.Value, true, 10, isLiteralOnly: false, tvpName: "dbo.TVP_int")));
		}

		public void TestChunkerDoesNotThrowError_WhenPassingVarCharDataType()
		{
			InsertRowsIntoTestingTable(new List<string>() {
				"AAA",
				"BBB",
				"CCC",
				"DDD",
				"EEE",
				"FFF",
			});

			AssertNoExceptionThrown("GenerateChunks does not throw an error", () => VarCharChunker.GenerateChunks(20, null, new SchemaStringColumn(_tableSchema.Object, testColumnName, 0, SqlDbType.VarChar, DBNull.Value, true, 100, isLiteralOnly: false, tvpName: "dbo.TVP_varchar")));
		}

		public void TestChunkerThrowsError_WhenResetIsCalled()
		{
			var chunker = VarCharChunker.GenerateChunks(20, null, new SchemaStringColumn(_tableSchema.Object, testColumnName, 0, SqlDbType.VarChar, DBNull.Value, true, 100, isLiteralOnly: false, tvpName: "dbo.TVP_varchar"));
			var enumerator = chunker.GetEnumerator();
			AssertExceptionThrown("Calling Reset on the enumerator throws an error", typeof(NotSupportedException), () => enumerator.Reset());
		}

		public void TestVarCharChunk_CanBeDeconstructed()
		{
			var chunk1 = new VarCharChunk("AAA", "BBB", 2);
			var chunk2 = new VarCharChunk("CCC", "DDD", 2);

			chunk1.Deconstruct(out var lowerBound1, out var upperBound1);
			var (lowerBound2, upperBound2) = chunk2;

			AssertEquals("Lower bound has been deconstructed properly", chunk1.LowerBound, lowerBound1);
			AssertEquals("Upper bound has been deconstructed properly", chunk1.UpperBound, upperBound1);

			AssertEquals("Lower bound has been deconstructed properly", chunk2.LowerBound, lowerBound2);
			AssertEquals("Upper bound has been deconstructed properly", chunk2.UpperBound, upperBound2);
		}

		public void TestVarCharChunk_CanBeDeconstructed_WithRowCount()
		{
			var chunk1 = new VarCharChunk("AAA", "BBB", 2);
			var chunk2 = new VarCharChunk("CCC", "DDD", 2);

			chunk1.Deconstruct(out var lowerBound1, out var upperBound1, out var rowCount1);
			var (lowerBound2, upperBound2, rowCount2) = chunk2;

			AssertEquals("Lower bound has been deconstructed properly", chunk1.LowerBound, lowerBound1);
			AssertEquals("Upper bound has been deconstructed properly", chunk1.UpperBound, upperBound1);
			AssertEquals("Row count has been deconstructed properly", chunk1.RowCount, rowCount1);

			AssertEquals("Lower bound has been deconstructed properly", chunk2.LowerBound, lowerBound2);
			AssertEquals("Upper bound has been deconstructed properly", chunk2.UpperBound, upperBound2);
			AssertEquals("Row count has been deconstructed properly", chunk2.RowCount, rowCount2);
		}

		readonly string testTableName = "VarCharChunkerTestTable";
		readonly string testColumnName = "TEST_JobId";

		void CreateTestingTable()
		{
			Db.Connection.ExecuteNonQuery(@$"CREATE TABLE [{testTableName}] (
												[TEST_PK] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
												[{testColumnName}] VARCHAR(100) NULL
											);");
		}

		void DropTestingTable()
		{
			Db.Connection.ExecuteNonQuery(@$"DROP TABLE [{testTableName}];");
		}

		void InsertRowIntoTestingTable(string varChar)
		{
			var sqlText = @$"INSERT INTO [{testTableName}] ([{testColumnName}]) VALUES (@VarChar)";

			using (var cmd = Db.Connection.Command(sqlText))
			{
				cmd.AddParameter("@VarChar", SqlDbType.VarChar, varChar != null ? varChar : DBNull.Value);
				cmd.ExecuteNonQuery();
			}
		}

		void InsertRowsIntoTestingTable(IEnumerable<string> varChars)
		{
			foreach (var varChar in varChars)
			{
				InsertRowIntoTestingTable(varChar);
			}
		}
	}
}

