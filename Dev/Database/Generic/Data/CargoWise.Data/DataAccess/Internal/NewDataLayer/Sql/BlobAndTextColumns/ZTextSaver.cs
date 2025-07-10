using System;
using System.Data;
using System.Globalization;
using System.IO;
using CargoWise.Data;
using CargoWise.IO;

namespace CargoWise.EntityFramework
{
	public class ZTextSaver : ZLargeColumnSaver, IRowFieldInfo
	{
		public ZTextSaver(string tableName, string pkColumnName, Guid rowPk, string dataColumnName, SqlDbType dataColumnType, ITextReaderSource source)
			: base(tableName, pkColumnName, rowPk, dataColumnName, dataColumnType)
		{
			this.Source = source;
		}

#if DEBUG
		public
#else
		internal
#endif
		ITextReaderSource Source { get; set; }

		public override void Save(DbConnection connection)
		{
			using (var textDataReader = Source.GetReader())
			{
				if (textDataReader is ISqlFieldSource)
				{
					using (var destinationWriter = new SqlTextFieldWriter(connection, tableName, pkColumnName, RowPk, dataColumnName, dataColumnType))
					{
						textDataReader.CopyTo(destinationWriter);
					}
				}
				else
				{
					using (var command = connection.Command(string.Format(CultureInfo.InvariantCulture, @" update {0} set [{1}] = @textData where [{2}] = @key select @@ROWCOUNT " // sql query format string
										, tableName, dataColumnName, pkColumnName)))
					{
						command.AddParameter("@key", SqlDbType.UniqueIdentifier, RowPk); // parameter name
						command.AddParameter("@textData", dataColumnType, textDataReader); // parameter name

						try
						{
							int rowCount = (int)command.ExecuteScalar();
							if (rowCount == 0)
							{
								throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "{1}={0}", RowPk, pkValueNotFoundError)); // SQL query
							}
						}
						catch (InvalidOperationException ex)
						{
							if (ex.Message.Contains(openDataReaderError))
							{
								throw new InvalidOperationException(GetExceptionMessage(textDataReader, this), ex);
							}

							throw;
						}
					}
				}
			}
		}

		static string GetExceptionMessage(TextReader sourceReader, IRowFieldInfo targetInfo)
		{
			var sourceInfo = sourceReader as IRowFieldInfo;
			var sourceDescription = sourceInfo == null ? sourceInfo.GetType().FullName : GetDescription(sourceInfo);
			var targetDescription = GetDescription(targetInfo);

			return string.Format(CultureInfo.InvariantCulture, "Failed to update {0}\r\nfrom {1}\r\nwith value [{2}]\r\nSee Inner Exception for details.", sourceDescription, targetDescription, GetFirstXCharsFrom(sourceReader, 200)); // exception message
		}

		static string GetDescription(IRowFieldInfo rowFieldInfo)
		{
			return string.Format(CultureInfo.InvariantCulture, "Table:[{0}] Column:[{1}] RowPK:[{2}]", rowFieldInfo.TableName, rowFieldInfo.ColumnName, rowFieldInfo.RowPK.ToString()); // exception message
		}

		static string GetFirstXCharsFrom(TextReader reader, int charsAllowed)
		{
			var buffer = new char[charsAllowed + 1];
			int charsRead = reader.Read(buffer, 0, charsAllowed + 1);

			return new string(buffer, 0, charsRead);
		}

		public const string openDataReaderError = "There is already an open DataReader associated with this Command which must be closed first."; // exception message
		protected const string pkValueNotFoundError = "There is no record with pkValue"; // exception message

		#region IRowFieldInfo

		string IRowFieldInfo.ColumnName
		{
			get { return dataColumnName; }
		}

		string IRowFieldInfo.PKColumnName
		{
			get { return pkColumnName; }
		}

		Guid IRowFieldInfo.RowPK
		{
			get { return RowPk; }
		}

		string IRowFieldInfo.TableName
		{
			get { return tableName; }
		}

		#endregion
	}
}
