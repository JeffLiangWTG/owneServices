using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data.Diagnostics;

namespace CargoWise.Data
{
	sealed class ReadOnlyDataAdapter : IReadOnlyDataAdapter
	{
		internal ReadOnlyDataAdapter(IDbCommand selectCommand, DbConnection connection, int timeoutSeconds = 0)
		{
			Argument.NotNull(selectCommand, nameof(selectCommand));
			Argument.NotNullOrEmpty(selectCommand.CommandText, nameof(selectCommand.CommandText));
			Argument.NotNull(connection, nameof(connection));

			runAsReader = selectCommand.CommandText.Contains(DbCommand.ExecuteAsReaderFlagComments);
			if (QueryStackTraceRecorderCore.InstanceCore.Enabled && selectCommand is SqlCommand sqlCommand)
			{
				selectCommand = QueryStackTraceRecorderCore.InstanceCore.GetCommandWithCallStackTraceAddedIfEnabled(sqlCommand);
			}

			if (timeoutSeconds > 0 && selectCommand is System.Data.Common.DbCommand dbCommand)
			{
				adapter = new DbDataAdapterWithTimeout(dbCommand, timeoutSeconds);
			}
			else
			{
				adapter = connection.DataProviderFactory.NewDataAdapter(selectCommand);
			}

			this.connection = connection;
		}
		readonly bool runAsReader;
		readonly DbDataAdapter adapter;
		readonly DbConnection connection;

#if DEBUG
		internal DbDataAdapter DbDataAdapter_Exposed => adapter;
#endif

		public int Fill(DataTable dataTable)
		{
			var sw = Stopwatch.StartNew();
			var initialRowsCount = dataTable.Rows.Count;
			var result = FillSafe(adapter.Fill, dataTable, table => CheckCanRefill(table, initialRowsCount));

			SqlEventTracker.Instance.AddSqlEvent(adapter.SelectCommand, null, sw.Elapsed);

			adapter.ProcessStringInterning(dataTable);

			return result;
		}

		public int Fill(DataSet dataSet)
		{
			var sw = Stopwatch.StartNew();
			var initialRowsCounts = dataSet.Tables.Cast<DataTable>().ToDictionary(table => table, table => table.Rows.Count);

			var result = FillSafe(adapter.Fill, dataSet, ds => CheckCanRefill(ds, initialRowsCounts));

			SqlEventTracker.Instance.AddSqlEvent(adapter.SelectCommand, null, sw.Elapsed);

			adapter.ProcessStringInterning(dataSet);

			return result;
		}

		public void Dispose()
		{
			adapter.Dispose();
		}

		int FillSafe<T>(Func<T, int> fillFunction, T dataComponentToFill, Func<T, bool> checkCanRefillAction = null) where T : IComponent
		{
			Argument.NotNull(fillFunction, nameof(fillFunction)); // Suggested By ReviewBot 
			Argument.NotNull(dataComponentToFill, nameof(dataComponentToFill)); // Suggested By ReviewBot 

			Func<object> f = () => FillSafeWrapped(fillFunction, dataComponentToFill, checkCanRefillAction);
			var result = runAsReader ? new ExecuteAsReader().Execute(connection, f) : f();
			return (int)result;
		}

		int FillSafeWrapped<T>(Func<T, int> fillFunction, T dataComponentToFill, Func<T, bool> checkCanRefillAction = null) where T : IComponent
		{
			Argument.NotNull(fillFunction, nameof(fillFunction)); // Suggested By ReviewBot 
			Argument.NotNull(dataComponentToFill, nameof(dataComponentToFill)); // Suggested By ReviewBot 

			try
			{
				var result = fillFunction(dataComponentToFill);
				return result;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (connection.ConnectionErrorHandler.HandleDisconnectionAndSecurityErrors(ex))
				{
					var canRefill = true;
					if (checkCanRefillAction != null)
					{
						canRefill = checkCanRefillAction(dataComponentToFill);
					}

					if (canRefill)
					{
						// Re-try fill function if error was properly handled
						var result = fillFunction(dataComponentToFill);
						return result;
					}
				}

				throw;
			}
		}

		bool CheckCanRefill(DataTable table, int initialRowsCount)
		{
			Argument.NotNull(table, nameof(table)); // Suggested By ReviewBot 

			if (table.Rows.Count > initialRowsCount && table.PrimaryKey.Length == 0)
			{
				if (initialRowsCount > 0)
				{
					return false;
				}

				table.Clear();
			}

			return true;
		}

		bool CheckCanRefill(DataSet dataSet, IDictionary<DataTable, int> initialRowsCounts)
		{
			Argument.NotNull(initialRowsCounts, nameof(initialRowsCounts));
			Argument.NotNull(dataSet, nameof(dataSet)); // Suggested By ReviewBot 

			foreach (DataTable table in dataSet.Tables)
			{
				int initialRowsCount;
				if (!initialRowsCounts.TryGetValue(table, out initialRowsCount))
				{
					initialRowsCount = 0;
				}
				if (!CheckCanRefill(table, initialRowsCount))
				{
					return false;
				}
			}

			return true;
		}
	}
}
