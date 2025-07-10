using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CargoWise.Data.Providers.Common
{
	/// <summary>
	/// Custom enum to replace SqlBulkCopyOptions
	/// </summary>
	[Flags]
	public enum SqlBulkCopyOptions
	{
		Default = 0,
		KeepIdentity = 1,
		CheckConstraints = 2,
		TableLock = 4,
		KeepNulls = 8,
		FireTriggers = 0x10,
		UseInternalTransaction = 0x20,
		AllowEncryptedValueModifications = 0x40
	}

	/// <summary>
	/// Custom delegate and event args to replace SqlRowsCopiedEventHandler
	/// </summary>
	public class SqlRowsCopiedEventArgs : EventArgs
	{
		public long RowsCopied { get; set; }
		public bool Abort { get; set; }
	}

	public delegate void SqlRowsCopiedEventHandler(object sender, SqlRowsCopiedEventArgs e);

	public interface ISqlBulkCopy : IDisposable
	{
		string DestinationTableName { get; set; }

		int BulkCopyTimeout { get; set; }

		int BatchSize { get; set; }

		int NotifyAfter { get; set; }

		SqlBulkCopyOptions BulkCopyOptions { get; }

		IDbTransaction Transaction { get; }

		Dictionary<string, string> ColumnMappings { get; }

		event SqlRowsCopiedEventHandler SqlRowsCopied;

		void WriteToServer(DataTable table);

		void WriteToServer(DataRow[] rows);

		Task WriteToServerAsync(DataTable table);
	}
}
