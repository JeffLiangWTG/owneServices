using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data.SqlProxy.Interface;
using CargoWise.Data.SqlProxy.Interface.Models;

namespace CargoWise.Data.HttpClient;

class HttpBulkCopyCommand : HttpCommand
{
	public DataTable Table { get; }

	public DataRow[] Rows { get; }

	public bool IsAsync { get; private set; }

	public HttpBulkCopyCommand(HttpConnection httpConnection, DataRow[] rows) : base(httpConnection)
	{
		Rows = rows;
	}

	public HttpBulkCopyCommand(HttpConnection httpConnection, DataTable table) : this(httpConnection, table, false)
	{
	}

	public HttpBulkCopyCommand(HttpConnection httpConnection, DataTable table, bool isAsync) : base(httpConnection)
	{
		Table = table;
		IsAsync = isAsync;
	}

	public int Execute(IBulkCopyClient client, string destinationTableName, int bulkCopyTimeout, int batchSize, int notifyAfter, Dictionary<string, string> columnMappings, SqlBulkCopyOptions options, Action<int> sqlRowsCopied)
	{
		Argument.NotNull(client, nameof(client));
		var request = new SqlProxyBulkCopyRequest(HttpConnection);
		request.LoadCommand(this);

		DataRow[] rows;
		if (Table != null)
		{
			rows = Table.Rows.Cast<DataRow>().ToArray();
			if (columnMappings == null || columnMappings.Count == 0)
			{
				columnMappings = Table.Columns.Cast<DataColumn>().ToDictionary(x => x.ColumnName, x => x.ColumnName);
			}
		}
		else
		{
			rows = Rows;
		}

		var numberOfRows = rows.Length;
		if (numberOfRows == 0)
		{
			return 0;
		}

		if (batchSize <= 0)
		{
			batchSize = DefaultBatchSize;
		}

		request.BatchSize = batchSize;
		request.BulkCopyTimeout = bulkCopyTimeout;
		request.DestinationTableName = destinationTableName;
		request.NotifyAfter = notifyAfter;
		request.ColumnMappings = columnMappings;
		request.Options = options;

		var tableSchemaXml = new StringBuilder();
		var table = rows[0]?.Table;
		if (string.IsNullOrWhiteSpace(table.TableName))
		{
			table.TableName = string.Format(CultureInfo.InvariantCulture, "Data{0}", Guid.NewGuid()); // data table id
		}
		using (var schemaWriter = new StringWriter(tableSchemaXml, CultureInfo.CurrentCulture))
		{
			table.WriteXmlSchema(schemaWriter);
		}
		request.TableSchema = tableSchemaXml.ToString();

		var totalRowsCopied = 0;
		for (var i = 0; i < numberOfRows; i += batchSize)
		{
			using var outputStream = new MemoryStream();
			using var serializationStream = new MemoryStream();
			using var zipStream = new GZipStream(outputStream, CompressionMode.Compress, false);

			var bufferSize = i + batchSize <= numberOfRows ? batchSize : numberOfRows - i;
			var bufferRows = new List<object[]>();
			for (var row = i; row < i + bufferSize; row++)
			{
				bufferRows.Add(rows[row].ItemArray);
			}

			using (var writer = new BinaryWriter(serializationStream))
			{
				var serializedString = JsonHelper.SerializeObjectArray(bufferRows);
				writer.Write(Encoding.UTF8.GetBytes(serializedString));

				serializationStream.Position = 0;
				serializationStream.CopyTo(zipStream);
			}

			request.Payload = outputStream.ToArray();
			var result = client.BulkCopy(request);

			sqlRowsCopied?.Invoke(result.RowsCopied);
			totalRowsCopied += result.RowsCopied;
		}

		return totalRowsCopied;
	}

	const int DefaultBatchSize = 500;
}
