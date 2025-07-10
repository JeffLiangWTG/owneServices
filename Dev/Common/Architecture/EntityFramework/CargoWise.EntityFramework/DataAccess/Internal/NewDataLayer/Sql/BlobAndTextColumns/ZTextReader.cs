using System;
using System.IO;
using CargoWise.Data;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	class ZTextReader : TextReader
	{
		internal ZTextReader(ZSqlConnectionInfo connectionInfo, SchemaStringColumn textColumn, ZGuid pk)
		{
			this.connectionInfo = connectionInfo;
			this.textColumn = textColumn;
			this.pk = pk;
		}

		public override int Read()
		{
			char[] chararray = new char[1];
			if (Read(chararray, 0, 1) == 1)
			{
				return chararray[0];
			}
			else
			{
				return -1;
			}
		}

		int position;

		public override int Read(char[] buffer, int index, int count)
		{
			// SQL substring returns only as many characters as are available so no need to check if count is too large
			count = Math.Max(count, 0);
			if (count > 0)
			{
				string commandText = String.Format(@"SELECT SUBSTRING({1}, {3}, {4}) FROM {0} WHERE {2} = @PK",
					TableName, textColumn.Name, textColumn.TableSchema.PK.Name, position + 1, count);
				using (DbCommand command = connectionInfo.DbConnection.Command(commandText))
				{
					command.AddParameterBasedOnDbColumn("@PK", pk.ToGuid(), textColumn.TableSchema.PK);
					string result = (string)command.ExecuteScalar();
					if (result != null)
					{
						count = result.Length;
						result.CopyTo(0, buffer, index, count);
					}
					else
					{
						count = 0;
					}
				}
				position += count;
			}
			return count;
		}

		public string TableName => textColumn.TableName;

		readonly ZSqlConnectionInfo connectionInfo;
		readonly SchemaStringColumn textColumn;
		readonly ZGuid pk;
	}
}
