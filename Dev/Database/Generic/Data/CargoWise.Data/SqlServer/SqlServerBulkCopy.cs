using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using CargoWise.Data.Providers.Common;
using SqlBulkCopyOptions = CargoWise.Data.Providers.Common.SqlBulkCopyOptions;
using SqlRowsCopiedEventArgs = CargoWise.Data.Providers.Common.SqlRowsCopiedEventArgs;
using SqlRowsCopiedEventHandler = CargoWise.Data.Providers.Common.SqlRowsCopiedEventHandler;

namespace CargoWise.Data.SqlServer
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Fullname needed to identify SqlClient types")]
	public class SqlServerBulkCopy : ISqlBulkCopy
	{
		public string DestinationTableName
		{
			get
			{
#if NET
				if (sqlBulkCopyMS != null)
				{
					return sqlBulkCopyMS.DestinationTableName;
				}
#endif
				return sqlBulkCopySys.DestinationTableName;
			}
			set
			{
#if NET
				if (sqlBulkCopyMS != null)
				{
					sqlBulkCopyMS.DestinationTableName = value;
					return;
				}
#endif
				sqlBulkCopySys.DestinationTableName = value;
			}
		}

		public int BulkCopyTimeout
		{
			get
			{
#if NET
				if (sqlBulkCopyMS != null)
				{
					return sqlBulkCopyMS.BulkCopyTimeout;
				}
#endif
				return sqlBulkCopySys.BulkCopyTimeout;
			}
			set
			{
#if NET
				if (sqlBulkCopyMS != null)
				{
					sqlBulkCopyMS.BulkCopyTimeout = value;
					return;
				}
#endif
				sqlBulkCopySys.BulkCopyTimeout = value;
			}
		}

		public int BatchSize
		{
			get
			{
#if NET
				if (sqlBulkCopyMS != null)
				{
					return sqlBulkCopyMS.BatchSize;
				}
#endif
				return sqlBulkCopySys.BatchSize;
			}
			set
			{
#if NET
				if (sqlBulkCopyMS != null)
				{
					sqlBulkCopyMS.BatchSize = value;
					return;
				}
#endif
				sqlBulkCopySys.BatchSize = value;
			}
		}

		public int NotifyAfter
		{
			get
			{
#if NET
				if (sqlBulkCopyMS != null)
				{
					return sqlBulkCopyMS.NotifyAfter;
				}
#endif
				return sqlBulkCopySys.NotifyAfter;
			}
			set
			{
#if NET
				if (sqlBulkCopyMS != null)
				{
					sqlBulkCopyMS.NotifyAfter = value;
					return;
				}
#endif
				sqlBulkCopySys.NotifyAfter = value;
			}
		}

		public SqlBulkCopyOptions BulkCopyOptions => sqlBulkCopyOptions;

		public IDbTransaction Transaction => transaction;

		public Dictionary<string, string> ColumnMappings { get { return columnMappings; } }

		public event SqlRowsCopiedEventHandler SqlRowsCopied;
		public void WriteToServer(DataTable table)
		{
			Exception exception = null;
			Prepare();
			stopWatch.Start();
			try
			{
#if NET
				if (sqlBulkCopyMS != null)
				{
					sqlBulkCopyMS.WriteToServer(table);
					return;
				}
#endif
				sqlBulkCopySys.WriteToServer(table);
			}
			catch (Exception ex)
			{
				exception = ex;
				throw;
			}
			finally
			{
				SqlEventTracker.Instance.AddSqlEvent(this, exception, stopWatch.Elapsed);
				stopWatch.Stop();
			}
		}

		public void WriteToServer(DataRow[] rows)
		{
			Exception exception = null;
			Prepare();
			stopWatch.Start();
			try
			{
#if NET
				if (sqlBulkCopyMS != null)
				{
					sqlBulkCopyMS.WriteToServer(rows);
					return;
				}
#endif
				sqlBulkCopySys.WriteToServer(rows);
			}
			catch (Exception ex)
			{
				exception = ex;
				throw;
			}
			finally
			{
				SqlEventTracker.Instance.AddSqlEvent(this, exception, stopWatch.Elapsed);
				stopWatch.Stop();
			}
		}

		public void WriteToServer(IDataReader dataReader)
		{
			Exception exception = null;
			Prepare();
			stopWatch.Start();
			try
			{
#if NET
				if (sqlBulkCopyMS != null)
				{
					sqlBulkCopyMS.WriteToServer(dataReader);
					return;
				}
#endif
				sqlBulkCopySys.WriteToServer(dataReader);
			}
			catch (Exception ex)
			{
				exception = ex;
				throw;
			}
			finally
			{
				SqlEventTracker.Instance.AddSqlEvent(this, exception, stopWatch.Elapsed);
				stopWatch.Stop();
			}
		}

		public async Task WriteToServerAsync(DataTable table)
		{
			Exception exception = null;
			Prepare();
			stopWatch.Start();
			try
			{
#if NET
				if (sqlBulkCopyMS != null)
				{
					await sqlBulkCopyMS.WriteToServerAsync(table);
					return;
				}
#endif
				await sqlBulkCopySys.WriteToServerAsync(table);
			}
			catch (Exception ex)
			{
				exception = ex;
				throw;
			}
			finally
			{
				SqlEventTracker.Instance.AddSqlEvent(this, exception, stopWatch.Elapsed);
				stopWatch.Stop();
			}
		}

		public SqlServerBulkCopy(System.Data.Common.DbConnection connection) : this(connection, SqlBulkCopyOptions.Default, null)
		{
		}

		public SqlServerBulkCopy(System.Data.Common.DbConnection connection, SqlBulkCopyOptions options, System.Data.Common.DbTransaction transaction)
		{
			if (connection == null)
			{
				throw new ArgumentNullException(nameof(connection));
			}

			columnMappings = new Dictionary<string, string>();
			sqlBulkCopyOptions = options;
			this.transaction = transaction;
			stopWatch = new Stopwatch();

			if (connection is System.Data.SqlClient.SqlConnection sqlConnectionSys)
			{
				sqlBulkCopySys = new System.Data.SqlClient.SqlBulkCopy(
				sqlConnectionSys,
				(System.Data.SqlClient.SqlBulkCopyOptions)options,
				transaction as System.Data.SqlClient.SqlTransaction);
				sqlBulkCopySys.SqlRowsCopied += (s, e) =>
				{
					SqlRowsCopied?.Invoke(s, new SqlRowsCopiedEventArgs
					{
						Abort = e.Abort,
						RowsCopied = e.RowsCopied,
					});
				};
			}
#if NET
			else if (connection is Microsoft.Data.SqlClient.SqlConnection sqlConnectionMS)
			{
				sqlBulkCopyMS = new Microsoft.Data.SqlClient.SqlBulkCopy(
					sqlConnectionMS,
					(Microsoft.Data.SqlClient.SqlBulkCopyOptions)options,
					transaction as Microsoft.Data.SqlClient.SqlTransaction);
				sqlBulkCopyMS.SqlRowsCopied += (s, e) =>
				{
					SqlRowsCopied?.Invoke(s, new SqlRowsCopiedEventArgs
					{
						Abort = e.Abort,
						RowsCopied = e.RowsCopied,
					});
				};
			}
#endif
			else
			{
				throw new NotSupportedException($"Unsupported connection type: {connection.GetType()}");
			}
		}

		void Prepare()
		{
#if NET
			if (sqlBulkCopyMS != null)
			{
				foreach (var item in columnMappings)
				{
					sqlBulkCopyMS.ColumnMappings.Add(item.Key, item.Value);
				}
				return;
			}
#endif
			foreach (var item in columnMappings)
			{
				sqlBulkCopySys.ColumnMappings.Add(item.Key, item.Value);
			}
		}

		#region IDisposable Support
		public bool IsDisposed { get; private set; }

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					sqlBulkCopySys?.Close();
#if NET
					sqlBulkCopyMS?.Close();
#endif
				}

				IsDisposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

#if NET
		readonly Microsoft.Data.SqlClient.SqlBulkCopy sqlBulkCopyMS;
#endif
		readonly System.Data.SqlClient.SqlBulkCopy sqlBulkCopySys;

		readonly SqlBulkCopyOptions sqlBulkCopyOptions;
		readonly IDbTransaction transaction;
		readonly Dictionary<string, string> columnMappings;
		readonly Stopwatch stopWatch;
	}
}
