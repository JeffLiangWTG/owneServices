using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CargoWise.Data.Providers.Common;

namespace CargoWise.Data.HttpClient
{
	public sealed class HttpBulkCopy : ISqlBulkCopy
	{
		public string DestinationTableName { get; set; }

		public int BulkCopyTimeout { get; set; } = 30;

		public int BatchSize { get; set; }

		public int NotifyAfter { get; set; }

		public Providers.Common.SqlBulkCopyOptions BulkCopyOptions => Providers.Common.SqlBulkCopyOptions.Default;

		public IDbTransaction Transaction => null;

		public Dictionary<string, string> ColumnMappings { get; }

		public event Providers.Common.SqlRowsCopiedEventHandler SqlRowsCopied;

		public HttpBulkCopy(HttpConnection connection) : this(connection, Providers.Common.SqlBulkCopyOptions.Default, null)
		{
		}

		public HttpBulkCopy(HttpConnection connection, Providers.Common.SqlBulkCopyOptions options, IDbTransaction transaction)
		{
		}

		public void WriteToServer(DataTable table)
		{
			SqlRowsCopied?.Invoke(null, null);
		}

		public void WriteToServer(DataRow[] rows)
		{
		}

		public Task WriteToServerAsync(DataTable table)
		{
			return Task.CompletedTask;
		}

		public void Dispose()
		{
		}
	}
}
