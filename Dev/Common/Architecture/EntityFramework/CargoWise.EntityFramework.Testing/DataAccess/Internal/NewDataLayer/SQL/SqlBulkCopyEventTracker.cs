using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;

namespace CargoWise.EntityFramework.Testing;

public class SqlBulkCopyEventTracker : IDisposable
{
	public SqlBulkCopyEventTracker()
	{
		bulkCopyEvents = new List<string>();
		SqlEventTracker.Instance.SqlCommandExecutedEvent += AddSqlCommandsEvent;
	}

	void AddSqlCommandsEvent(SqlCommandExecutedEventArgs args)
	{
		if (args.Text.StartsWith("insert bulk"))
		{
			bulkCopyEvents.Add(args.Text);
		}
	}

	public bool HasBulkCopyEvent(string tableName, IList<string> bulkCopyOptions = null)
	{
		var bulkCopyOptionsString = !bulkCopyOptions.IsNullOrEmpty()
			? string.Join(", ", bulkCopyOptions.Where(b => !b.IsNullOrEmpty()))
			: "Default";

		var statement = $"insert bulk {tableName} with ({bulkCopyOptionsString})";

		return bulkCopyEvents.Contains(statement);
	}

	public void Dispose()
	{
		SqlEventTracker.Instance.SqlCommandExecutedEvent -= AddSqlCommandsEvent;
	}

	readonly IList<string> bulkCopyEvents;
}
