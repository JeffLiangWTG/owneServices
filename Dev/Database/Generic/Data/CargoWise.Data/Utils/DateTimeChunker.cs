using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using CargoWise.Schema;

namespace CargoWise.Data
{
	public sealed class DateTimeChunker : IEnumerable<DateTimeChunk>
	{
		readonly int chunkSize;
		readonly DateTime lastProcessedDateTime;
		readonly string columnName;
		readonly string tableName;
		readonly string schemaName;
		readonly SqlDbType dataType;
		readonly long tickDifference;
		readonly byte scale;

		DateTimeChunker(int chunkSize, DateTime lastProcessedDateTime, string columnName, string tableName, string schemaName, SqlDbType dataType, byte scale, long tickDifference) {
			this.chunkSize = chunkSize;
			this.lastProcessedDateTime = lastProcessedDateTime;
			this.columnName = columnName;
			this.tableName = tableName;
			this.schemaName = schemaName;
			this.dataType = dataType;
			this.scale = scale;
			this.tickDifference = tickDifference;
		}

		public static IEnumerable<DateTimeChunk> GenerateChunks(int chunkSize, DateTime? lastProcessedDateTime, SchemaDateTimeColumn column)
		{
			return GenerateChunks(chunkSize, lastProcessedDateTime, column.Name, column.TableName, column.TableSchema.SqlSchemaName, column.SqlDbType);
		}

		public static IEnumerable<DateTimeChunk> GenerateChunks(int chunkSize, DateTime? lastProcessedDateTime, SchemaDateTimeOffsetColumn column)
		{
			return GenerateChunks(chunkSize, lastProcessedDateTime, column.Name, column.TableName, column.TableSchema.SqlSchemaName, column.SqlDbType, column.Scale);
		}

		static IEnumerable<DateTimeChunk> GenerateChunks(int chunkSize, DateTime? lastProcessedDateTime, string columnName, string tableName, string schemaName, SqlDbType dataType, byte scale = 0)
		{
			if (chunkSize <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(chunkSize), "Chunk size must be greater than 0.");
			}

			if (dataType != SqlDbType.DateTime && dataType != SqlDbType.SmallDateTime && dataType != SqlDbType.DateTimeOffset && dataType != SqlDbType.DateTime2)
			{
				throw new ArgumentException("Data type must be either DateTime, SmalDateTime, DateTimeOffset, or DateTime2", nameof(dataType));
			}

			if (dataType == SqlDbType.DateTimeOffset)
			{
				if (scale < 0 || scale > 7)
				{
					throw new ArgumentOutOfRangeException(nameof(scale), "Scale must be between 0 and 7 inclusive.");
				}
			}

			var tickDifference = GetTickDifference(dataType, scale);

			if (lastProcessedDateTime == null)
			{
				lastProcessedDateTime = GetMinDateTimeValue(dataType);
			}
			else
			{
				lastProcessedDateTime = new DateTime(Math.Min(lastProcessedDateTime.Value.Ticks + tickDifference, GetMaxDateTimeValue(dataType).Ticks));
			}

			return new DateTimeChunker(chunkSize, lastProcessedDateTime.Value, columnName, tableName, schemaName, dataType, scale, tickDifference);
		}

		public IEnumerator<DateTimeChunk> GetEnumerator()
		{
			return new DateTimeChunkEnumerator(chunkSize, lastProcessedDateTime, columnName, tableName, schemaName, dataType, scale, tickDifference);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		static long GetTickDifference(SqlDbType dataType, byte scale)
		{
			if (dataType == SqlDbType.DateTimeOffset)
			{
				switch (scale)
				{
					case 0:
						return TimeSpan.TicksPerSecond;
					case 1:
						return TimeSpan.TicksPerMillisecond * 100;
					case 2:
						return TimeSpan.TicksPerMillisecond * 10;
					case 3:
						return TimeSpan.TicksPerMillisecond;
					case 4:
						return 1000;
					case 5:
						return 100;
					case 6:
						return 10;
					case 7:
						return 1;
				}
			}

			switch (dataType)
			{
				case SqlDbType.DateTime:
					return TimeSpan.TicksPerMillisecond * 3;
				case SqlDbType.SmallDateTime:
					return TimeSpan.TicksPerMinute;
				case SqlDbType.DateTime2:
					return 1;
			}

			return 0;
		}

		static DateTime GetMinDateTimeValue(SqlDbType dataType)
		{
			switch (dataType)
			{
				case SqlDbType.DateTime:
					return (DateTime)SqlDateTime.MinValue;
				case SqlDbType.SmallDateTime:
					return new DateTime(1900, 1, 1);
				case SqlDbType.DateTimeOffset:
				case SqlDbType.DateTime2:
					return DateTime.MinValue;
			}
			return DateTime.MinValue;
		}

		static DateTime GetMaxDateTimeValue(SqlDbType dataType)
		{
			switch (dataType)
			{
				case SqlDbType.DateTime:
					return (DateTime)SqlDateTime.MaxValue;
				case SqlDbType.SmallDateTime:
					return new DateTime(2079, 6, 6, 23, 59, 0);
				case SqlDbType.DateTimeOffset:
				case SqlDbType.DateTime2:
					return DateTime.MaxValue;
			}
			return DateTime.MaxValue;
		}

		sealed class DateTimeChunkEnumerator : IEnumerator<DateTimeChunk>
		{
			readonly DateTime dateTimeToContinueFrom;
			readonly SqlDbType dataType;
			readonly long tickDifference;
			readonly int chunkSize;
			readonly byte scale;

			readonly DateTime minDateTimeValue;
			readonly DateTime maxDateTimeValue;

			bool firstCreated;
			DateTimeChunk current;

			readonly string sqlText;

			public DateTimeChunkEnumerator(int chunkSize, DateTime dateTimeToContinueFrom, string columnName, string tableName, string schemaName, SqlDbType dataType, byte scale, long tickDifference)
			{
				this.dateTimeToContinueFrom = dateTimeToContinueFrom;
				this.dataType = dataType;
				this.tickDifference = tickDifference;
				this.chunkSize = chunkSize;
				this.scale = scale;

				this.minDateTimeValue = GetMinDateTimeValue(dataType);
				this.maxDateTimeValue = GetMaxDateTimeValue(dataType);

				sqlText = $@"WITH Times AS (
								SELECT TOP ({chunkSize}) WITH TIES {columnName} AS DT
								FROM {schemaName}.{tableName}
								WHERE {columnName} IS NOT NULL AND {columnName} >= @DateTime
								ORDER BY {columnName}
							)
							SELECT	CAST( CAST( MIN(DT) AS DATETIMEOFFSET ) AT TIME ZONE 'UTC' AS DATETIME2 ) AS LowerBound,
									CAST( CAST( MAX(DT) AS DATETIMEOFFSET ) AT TIME ZONE 'UTC' AS DATETIME2 ) AS UpperBound,
									COUNT(*) AS 'RowCount'
							FROM	Times";

				firstCreated = false;
			}

			public DateTimeChunk Current => current;

			object IEnumerator.Current => current;

			void IDisposable.Dispose()
			{
			}

			public bool MoveNext()
			{
				if (!firstCreated)
				{
					firstCreated = true;
					current = GetNextChunk(dateTimeToContinueFrom);
					return true;
				}

				if (current.RowCount < chunkSize)
				{
					return false;
				}

				if (current.UpperBound.Ticks + tickDifference >= maxDateTimeValue.Ticks)
				{
					return false;
				}

				current = GetNextChunk(current.UpperBound.AddTicks(tickDifference));

				return current.RowCount > 0;
			}

			public void Reset()
			{
				firstCreated = false;
			}

			DateTimeChunk GetNextChunk(DateTime lastProcessedDateTime)
			{
				using (var cmd = Db.Connection.Command(sqlText))
				{
					lastProcessedDateTime = DateTime.SpecifyKind(lastProcessedDateTime, DateTimeKind.Utc);

					if (dataType == SqlDbType.DateTimeOffset)
					{
						cmd.AddParameter("@DateTime", dataType, scale, lastProcessedDateTime);
					}
					else
					{
						cmd.AddParameter("@DateTime", dataType, lastProcessedDateTime);
					}

					using (var reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
					{
						if (reader.Read())
						{
							var lowerBound = reader["LowerBound"];
							var upperBound = reader["UpperBound"];
							var rowCount = reader["RowCount"];

							if (lowerBound != null && lowerBound != DBNull.Value && upperBound != null && upperBound != DBNull.Value && rowCount != DBNull.Value)
							{
								return new DateTimeChunk((DateTime)lowerBound, (DateTime)upperBound, (int)rowCount);
							}
						}

						return new DateTimeChunk(minDateTimeValue, maxDateTimeValue, 0);
					}
				}
			}
		}
	}

	public struct DateTimeChunk
	{
		public DateTime LowerBound { get; }
		public DateTime UpperBound { get; }
		public int RowCount { get; }

		public DateTimeChunk(DateTime lowerBound, DateTime upperBound, int rowCount)
		{
			LowerBound = lowerBound;
			UpperBound = upperBound;
			this.RowCount = rowCount;
		}

		public void Deconstruct(out DateTime lowerBound, out DateTime upperBound)
		{
			lowerBound = LowerBound;
			upperBound = UpperBound;
		}

		public void Deconstruct(out DateTime lowerBound, out DateTime upperBound, out int rowCount)
		{
			lowerBound = LowerBound;
			upperBound = UpperBound;
			rowCount = RowCount;
		}

		public override string ToString()
		{
			return $@"LowerBound: {LowerBound.ToString("O")}
					UpperBound: {UpperBound.ToString("O")}
					RowCount: {RowCount}";
		}
	}
}
