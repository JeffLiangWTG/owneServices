using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enterprise.Client.EDI.IssueManager.Business;

static class StringBuilderExtensions
{
	public static void AddLines(this StringBuilder sb, int amount)
	{
		sb.Append(new string('\n', amount));
	}
}

public class DataFormatter : ILogFormatProvider
{
	readonly List<string> logMessages;

	readonly Dictionary<string, List<List<string>>> tableData;

	readonly Dictionary<string, (string columnName, bool ascending)> tableOrders;

	readonly Dictionary<string, List<string>> tableSchema;

	public DataFormatter()
	{
		tableSchema = new ();
		tableData = new ();
		logMessages = new ();
		tableOrders = new ();
	}

	public void AddRowToTable(string tableName, List<string> rowValues)
	{
		if (!tableSchema.TryGetValue(tableName, out var tableRows))
		{
			throw new ArgumentException($"Table, {tableName}, does not exists");
		}

		if (rowValues.Count != tableRows.Count)
		{
			throw new ArgumentException($"Number of row values does not match row count in table schema.");
		}

		var escapedValues = new List<string>(rowValues.Count);

		foreach (var value in rowValues)
		{
			escapedValues.Add(EscapeNewLines(value));
		}

		tableData[tableName].Add(escapedValues);
	}

	public string AsString()
	{
		var sb = new StringBuilder();

		GenerateTables(sb);

		sb.AddLines(2);

		GenerateLogs(sb);

		return sb.ToString();
	}

	public void CreateTable(string tableName, List<string> columnNames)
	{
		if (tableSchema.ContainsKey(tableName))
		{
			throw new ArgumentException($"Table, {tableName}, already exists.");
		}

		tableSchema[tableName] = columnNames;
		tableData[tableName] = new ();
	}

	public string EscapeNewLines(string content)
	{
		return content.Replace("\n", @"\n").Replace("\r", @"\n");
	}

	public void LogMessage(string message)
	{
		logMessages.Add(EscapeNewLines(message));
	}

	public void OrderTableBy(string tableName, string columnName, bool ascending = true)
	{
		if (!tableSchema.TryGetValue(tableName, out var tableRows))
		{
			throw new ArgumentException($"Table, {tableName}, does not exists");
		}

		if (!tableRows.Contains(columnName))
		{
			throw new ArgumentException($"Column, {columnName}, does not exist in table, {tableName}");
		}

		tableOrders[tableName] = (columnName, ascending);
	}

	void AddDashedLine(StringBuilder sb, int length)
	{
		sb.AppendLine(new string('-', length));
	}

	void CreateRowForColumnValues(StringBuilder sb, string tableName, List<string> columnValues)
	{
		sb.Append('|');

		for (var columnIndex = 0; columnIndex < columnValues.Count; columnIndex++)
		{
			sb.Append(' ');
			var columnValue = columnValues[columnIndex];
			var maxColumnWidth = GetMaxTableColumnWidth(tableName, columnIndex);
			var paddingAmount = maxColumnWidth - columnValue.Length;
			sb.Append(new string(' ', paddingAmount / 2));
			sb.Append(columnValue);
			sb.Append(new string(' ', (paddingAmount + 1) / 2));
			sb.Append(" |");
		}

		sb.AppendLine();
	}

	void GenerateLogs(StringBuilder sb)
	{
		sb.AppendLine("<=======~LOGS~=======>");

		var maxLogLength = GetMaxLogMessageLength();

		AddDashedLine(sb, maxLogLength + 4);

		foreach (var log in logMessages)
		{
			sb.Append("| ");
			sb.Append(log);
			sb.Append(new string(' ', maxLogLength - log.Length));
			sb.AppendLine(" |");
		}

		AddDashedLine(sb, maxLogLength + 4);
	}

	void GenerateTables(StringBuilder sb)
	{
		sb.AppendLine("<=======~TABLES~=======>");

		sb.AddLines(2);

		foreach (var (tableName, tableColumns) in tableSchema.Select(pair => (pair.Key, pair.Value)))
		{
			if (tableColumns.Count == 0)
			{
				continue;
			}

			sb.AppendLine($"Table - {tableName}");

			var maxRowLength = GetMaxTableRowLength(tableName);

			AddDashedLine(sb, maxRowLength);

			CreateRowForColumnValues(sb, tableName, tableColumns);

			AddDashedLine(sb, maxRowLength);

			var tableRows = tableData[tableName];

			if (tableOrders.TryGetValue(tableName, out var ordering))
			{
				var columnIndex = tableColumns.IndexOf(ordering.columnName);
				if (ordering.ascending)
				{
					tableRows.Sort((row1, row2) => row1[columnIndex].CompareTo(row2[columnIndex]));
				} else
				{
					tableRows.Sort((row1, row2) => row2[columnIndex].CompareTo(row1[columnIndex]));
				}
			}

			foreach (var columnValues in tableRows)
			{
				CreateRowForColumnValues(sb, tableName, columnValues);
			}

			AddDashedLine(sb, maxRowLength);

			sb.AddLines(1);
		}

		sb.AddLines(1);
	}

	int GetMaxLogMessageLength()
	{
		return logMessages.Max(message => message.Length);
	}

	int GetMaxTableColumnWidth(string tableName, int columnNumber)
	{
		var max = tableSchema[tableName][columnNumber].Length;

		foreach (var row in tableData[tableName])
		{
			var columnValueLength = row[columnNumber]?.Length ?? 0;

			if (columnValueLength > max)
			{
				max = columnValueLength;
			}
		}

		return max;
	}

	int GetMaxTableRowLength(string tableName)
	{
		var sumColumnWidths = 0;

		var columnCount = tableSchema[tableName].Count;

		for (var columnNumber = 0; columnNumber < columnCount; columnNumber++)
		{
			sumColumnWidths += GetMaxTableColumnWidth(tableName, columnNumber);
		}

		return TransformRowValuesLengthToDisplayLength(sumColumnWidths, columnCount);
	}

	int TransformRowValuesLengthToDisplayLength(int rowValuesLength, int rowAmount)
	{
		return rowValuesLength + rowAmount * 3 + 1;
	}
}
