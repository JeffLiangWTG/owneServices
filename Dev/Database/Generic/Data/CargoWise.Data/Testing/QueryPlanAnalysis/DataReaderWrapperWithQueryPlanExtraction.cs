#if DEBUG

using System;
using System.Data;
using CargoWise.Common;

namespace CargoWise.Data.Testing
{
	sealed class DataReaderWrapperWithQueryPlanExtraction : IDataReader
	{
		internal DataReaderWrapperWithQueryPlanExtraction(IDbCommand command, DbCommand dbCommand, CommandBehavior behaviour)
		{
			Argument.NotNull(command, nameof(command));
			Argument.NotNull(dbCommand, nameof(dbCommand));

			dbCommand.DbConnection.ExecuteNonQuery("SET STATISTICS XML ON");

			this.wrappedDataReader = command.ExecuteReader(behaviour);
			this.dbCommand = dbCommand;
		}

		readonly IDataReader wrappedDataReader;
		readonly DbCommand dbCommand;

		#region IDataReader Members

		int IDataReader.Depth => wrappedDataReader.Depth;

		bool IDataReader.IsClosed => wrappedDataReader.IsClosed;

		int IDataReader.RecordsAffected => wrappedDataReader.RecordsAffected;

		void IDataReader.Close()
		{
			wrappedDataReader.Close();
		}

		DataTable IDataReader.GetSchemaTable()
		{
			return wrappedDataReader.GetSchemaTable();
		}

		bool IDataReader.NextResult()
		{
			var moved = NextResultCore();

			while (moved && IsReaderAtExecutionPlan)
			{
				ExtractQueryPlanResult();
				moved = NextResultCore();
			}

			return moved;
		}

		bool IDataReader.Read()
		{
			if (IsReaderAtExecutionPlan)
			{
				ExtractQueryPlanResult();
				((IDataReader)this).NextResult();
			}

			return wrappedDataReader.Read();
		}

		#endregion

		#region IDataRecord Members

		int IDataRecord.FieldCount => wrappedDataReader.FieldCount;

		object IDataRecord.this[string name] => wrappedDataReader[name];

		object IDataRecord.this[int i] => wrappedDataReader[i];

		string IDataRecord.GetName(int i)
		{
			return wrappedDataReader.GetName(i);
		}

		string IDataRecord.GetDataTypeName(int i)
		{
			return wrappedDataReader.GetDataTypeName(i);
		}

		Type IDataRecord.GetFieldType(int i)
		{
			return wrappedDataReader.GetFieldType(i);
		}

		object IDataRecord.GetValue(int i)
		{
			return wrappedDataReader.GetValue(i);
		}

		int IDataRecord.GetValues(object[] values)
		{
			return wrappedDataReader.GetValues(values);
		}

		int IDataRecord.GetOrdinal(string name)
		{
			return wrappedDataReader.GetOrdinal(name);
		}

		bool IDataRecord.GetBoolean(int i)
		{
			return wrappedDataReader.GetBoolean(i);
		}

		byte IDataRecord.GetByte(int i)
		{
			return wrappedDataReader.GetByte(i);
		}

		long IDataRecord.GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			return wrappedDataReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
		}

		char IDataRecord.GetChar(int i)
		{
			return wrappedDataReader.GetChar(i);
		}

		long IDataRecord.GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			return wrappedDataReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
		}

		Guid IDataRecord.GetGuid(int i)
		{
			return wrappedDataReader.GetGuid(i);
		}

		short IDataRecord.GetInt16(int i)
		{
			return wrappedDataReader.GetInt16(i);
		}

		int IDataRecord.GetInt32(int i)
		{
			return wrappedDataReader.GetInt32(i);
		}

		long IDataRecord.GetInt64(int i)
		{
			return wrappedDataReader.GetInt64(i);
		}

		float IDataRecord.GetFloat(int i)
		{
			return wrappedDataReader.GetFloat(i);
		}

		double IDataRecord.GetDouble(int i)
		{
			return wrappedDataReader.GetDouble(i);
		}

		string IDataRecord.GetString(int i)
		{
			return wrappedDataReader.GetString(i);
		}

		decimal IDataRecord.GetDecimal(int i)
		{
			return wrappedDataReader.GetDecimal(i);
		}

		DateTime IDataRecord.GetDateTime(int i)
		{
			return wrappedDataReader.GetDateTime(i);
		}

		IDataReader IDataRecord.GetData(int i)
		{
			return wrappedDataReader.GetData(i);
		}

		bool IDataRecord.IsDBNull(int i)
		{
			return wrappedDataReader.IsDBNull(i);
		}

		#endregion

		#region IDisposable Members

		void IDisposable.Dispose()
		{
			try
			{
				if (NextResultCore())
				{
					ExtractQueryPlanResult();
				}
			}
			finally
			{
				wrappedDataReader.Dispose();
				dbCommand.DbConnection.ExecuteNonQuery("SET STATISTICS XML OFF");
			}
		}

		#endregion

		#region Implementation

		bool IsReaderAtExecutionPlan => wrappedDataReader.FieldCount == 1 && (wrappedDataReader.GetName(0)?.Contains("XML Showplan") ?? false);

		bool NextResultCore() => wrappedDataReader.NextResult();

		void ExtractQueryPlanResult()
		{
			if (wrappedDataReader.Read())
			{
				var queryPlan = wrappedDataReader.GetString(0);
				var query = dbCommand.CommandText;

				dbCommand.DbConnection.AddExecutionPlan(query, queryPlan);
			}
		}

		#endregion
	}
}

#endif