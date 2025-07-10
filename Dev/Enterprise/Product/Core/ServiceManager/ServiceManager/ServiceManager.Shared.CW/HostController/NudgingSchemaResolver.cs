using System;
using CargoWise.Schema;
using ServiceManager.Integration.NudgingClient.Abstractions;

namespace ServiceManager.Shared.CW
{
	public class NudgingSchemaResolver : INudgingSchemaResolver
	{
		readonly IApplicationSchemaResolver schemaResolver;

		public NudgingSchemaResolver(IApplicationSchemaResolver schemaResolver)
		{
			this.schemaResolver = schemaResolver;
		}

		public Type GetColumnType(string table, string column)
		{
			table = table ?? throw new ArgumentNullException(nameof(table));
			column = column ?? throw new ArgumentNullException(nameof(column));
			var tableSchema = schemaResolver.GetTableSchema(table) ?? throw new ArgumentException($"Failed to get table schema for table '{table}'", nameof(table));
			var columnName = tableSchema.All[column] ?? throw new ArgumentException($"Failed to get column '{column}' for table '{table}'", nameof(column));
			return columnName.DotNetType;
		}

		public bool IsParameterizable(string table, string column)
		{
			var targetColumn = schemaResolver
				.GetTableSchema(table)
				.GetSchemaColumn(column);
			var isNonBlankFilteredIndex = (targetColumn as SchemaStringColumn)?.IsNonBlankFilteredIndexParticipant ?? false;

			return !targetColumn.IsLiteralOnly && !isNonBlankFilteredIndex;
		}
	}
}
