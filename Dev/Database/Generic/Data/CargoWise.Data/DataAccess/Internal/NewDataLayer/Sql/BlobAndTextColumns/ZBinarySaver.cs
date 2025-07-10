using System;
using System.Data;
using System.IO;
using CargoWise.Data;

namespace CargoWise.EntityFramework
{
	public class ZBinarySaver : ZLargeColumnSaver
	{
		public ZBinarySaver(string tableName, string pkColumnName, Guid rowPk, string dataColumnName, SqlDbType dataColumnType, IStreamSource source, bool compress)
			: base(tableName, pkColumnName, rowPk, dataColumnName, dataColumnType)
		{
			this.Source = source;
			this.compress = compress;
		}

		internal IStreamSource Source { get; set; }

		readonly bool compress;

		public override void Save(DbConnection connection)
		{
			using (var sourceStream = Source.GetStream())
			using (var destinationStream = GetDestination(connection, sourceStream, compress && sourceStream.Length > MaxChunkSize))
			{
				sourceStream.Position = 0;
				sourceStream.CopyTo(destinationStream);
			}
		}

		Stream GetDestination(DbConnection connection, Stream sourceStream, bool zcompress)
		{
			Stream destination = SqlBinaryFieldStream.OpenWriter(connection, tableName, pkColumnName, RowPk, dataColumnName, dataColumnType);
			if (zcompress)
			{
				destination = ZCompressor.GetCompressedVersion(sourceStream, destination, dataColumnName);
			}
			return destination;
		}
	}
}
