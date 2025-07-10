using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.Schema;

namespace CargoWise.Data
{
	public sealed class VarCharChunker : IEnumerable<VarCharChunk>
	{
		readonly int chunkSize;
		readonly string lastProcessedVarChar;
		readonly string columnName;
		readonly string tableName;
		readonly string schemaName;
		readonly SqlDbType dataType;
		readonly int maxLength;

		VarCharChunker(int chunkSize, string lastProcessedVarChar, string columnName, string tableName, string schemaName, SqlDbType dataType, int maxLength) {
			this.chunkSize = chunkSize;
			this.lastProcessedVarChar = lastProcessedVarChar;
			this.columnName = columnName;
			this.tableName = tableName;
			this.schemaName = schemaName;
			this.dataType = dataType;
			this.maxLength = maxLength;
		}

		public static IEnumerable<VarCharChunk> GenerateChunks(int chunkSize, string lastProcessedVarChar, SchemaStringColumn column)
		{
			return GenerateChunks(chunkSize, lastProcessedVarChar, column.Name, column.TableName, column.TableSchema.SqlSchemaName, column.SqlDbType, column.MaxLength);
		}

		public IEnumerator<VarCharChunk> GetEnumerator()
		{
			return new VarCharChunkEnumerator(chunkSize, lastProcessedVarChar, columnName, tableName, schemaName, dataType, maxLength);
		}

		static IEnumerable<VarCharChunk> GenerateChunks(int chunkSize, string lastProcessedVarChar, string columnName, string tableName, string schemaName, SqlDbType dataType, int maxLength)
		{
			if (chunkSize <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(chunkSize), "Chunk size must be greater than 0.");
			}

			if (dataType != SqlDbType.VarChar)
			{
				throw new ArgumentException("Data type must be VarChar", nameof(dataType));
			}

			if (lastProcessedVarChar == null)
			{
				lastProcessedVarChar = GetMinVarCharValue(dataType);
			}

			return new VarCharChunker(chunkSize, lastProcessedVarChar, columnName, tableName, schemaName, dataType, maxLength);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		static string GetMinVarCharValue(SqlDbType dataType)
		{
			switch (dataType)
			{
				case SqlDbType.VarChar:
					return string.Empty;
			}
			return string.Empty;
		}

		static string GetMaxVarCharValue(SqlDbType dataType, int columnMaxLength)
		{
			switch (dataType)
			{
				case SqlDbType.VarChar:
					return new string((char)255, columnMaxLength);
			}
			return new string((char)255, columnMaxLength);
		}

		sealed class VarCharChunkEnumerator : IEnumerator<VarCharChunk>
		{
			readonly string lastProcessedVarChar;
			readonly SqlDbType dataType;
			readonly int chunkSize;

			readonly string minVarCharValue;
			readonly string maxVarCharValue;

			readonly bool createFirstChunkFromLastProcessedVarChar;
			bool firstCreated;
			VarCharChunk current;

			readonly string nextChunkSqlText;
			readonly string firstChunkSqlText;

			public VarCharChunkEnumerator(int chunkSize, string lastProcessedVarChar, string columnName, string tableName, string schemaName, SqlDbType dataType, int maxLength)
			{
				this.lastProcessedVarChar = lastProcessedVarChar;
				this.dataType = dataType;
				this.chunkSize = chunkSize;

				var columnNameQuoteName = columnName.QuoteName();

				this.minVarCharValue = GetMinVarCharValue(dataType);
				this.maxVarCharValue = GetMaxVarCharValue(dataType, maxLength);

				nextChunkSqlText = FormattableString.Invariant($@"
					WITH Strings AS (
						SELECT TOP ({chunkSize}) WITH TIES {columnNameQuoteName} AS VC
						FROM {schemaName.QuoteName()}.{tableName.QuoteName()}
						WHERE {columnNameQuoteName} > @VarChar 
						ORDER BY {columnNameQuoteName}
					)
					SELECT  MIN(VC) AS LowerBound,
							MAX(VC) AS UpperBound,
							COUNT(*) AS 'RowCount'
					FROM Strings"
				);

				firstChunkSqlText = FormattableString.Invariant($@"
					WITH Strings AS (
						SELECT TOP ({chunkSize}) WITH TIES {columnNameQuoteName} AS VC
						FROM {schemaName.QuoteName()}.{tableName.QuoteName()}
						WHERE {columnNameQuoteName} IS NOT NULL
						ORDER BY {columnNameQuoteName}
					)
					SELECT  MIN(VC) AS LowerBound,
							MAX(VC) AS UpperBound,
							COUNT(*) AS 'RowCount'
					FROM Strings"
				);

				firstCreated = false;
				createFirstChunkFromLastProcessedVarChar = !lastProcessedVarChar.IsNullOrEmpty();
			}

			public VarCharChunk Current => current;

			object IEnumerator.Current => current;

			void IDisposable.Dispose()
			{
			}

			public bool MoveNext()
			{
				if (!firstCreated)
				{
					current = GetNextChunk(lastProcessedVarChar);
					firstCreated = true;
					return true;
				}

				if (current.RowCount < chunkSize)
				{
					return false;
				}

				if (current.UpperBound.CompareTo(maxVarCharValue) >= 0)
				{
					return false;
				}

				current = GetNextChunk(current.UpperBound);

				return current.RowCount > 0;
			}

			public void Reset()
			{
				throw new NotSupportedException();
			}

			VarCharChunk GetNextChunk(string lastProcessedVarChar)
			{
				var useFirstCreatedSqlText = !createFirstChunkFromLastProcessedVarChar && !firstCreated;
				var sqlText = useFirstCreatedSqlText ? firstChunkSqlText : nextChunkSqlText;
				using (var cmd = Db.Connection.Command(sqlText))
				{
					if (!useFirstCreatedSqlText)
					{
						cmd.AddParameter("@VarChar", dataType, lastProcessedVarChar);
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
								return new VarCharChunk((string)lowerBound, (string)upperBound, (int)rowCount);
							}
						}

						return new VarCharChunk(minVarCharValue, maxVarCharValue, 0);
					}
				}
			}
		}
	}

	public struct VarCharChunk
	{
		public string LowerBound { get; }
		public string UpperBound { get; }
		public int RowCount { get; }

		public VarCharChunk(string lowerBound, string upperBound, int rowCount)
		{
			LowerBound = lowerBound;
			UpperBound = upperBound;
			this.RowCount = rowCount;
		}

		public void Deconstruct(out string lowerBound, out string upperBound)
		{
			lowerBound = LowerBound;
			upperBound = UpperBound;
		}

		public void Deconstruct(out string lowerBound, out string upperBound, out int rowCount)
		{
			lowerBound = LowerBound;
			upperBound = UpperBound;
			rowCount = RowCount;
		}

		public override string ToString()
		{
			return $@"LowerBound: {LowerBound}
					UpperBound: {UpperBound}
					RowCount: {RowCount}";
		}
	}
}
