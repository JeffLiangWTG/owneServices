using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public class ZBulkUpdateCommandBuilderBase : ZSqlCommandBuilder
	{
		public ZBulkUpdateCommandBuilderBase(IEnumerable<DataRow> rowsToUpdate, string tableName, bool compress, ITableSchema schema)
			: base(null, tableName, compress, schema)
		{
			this.rowsToUpdate = new List<DataRow>(rowsToUpdate);
			this.useParameters = UseParameters;
		}

		List<ZUpdateCommandBuilderBase> builders;
		readonly List<DataRow> rowsToUpdate;
		readonly bool useParameters;

		protected int UpdateRowsCount
		{
			get
			{
				return rowsToUpdate.Count;
			}
		}

		public override bool IsConcurrencyCheckRequired
		{
			get
			{
				return true;
			}
		}

		#region SuppressResourceStringsCheckRegion

		public virtual ZUpdateCommandBuilderBase GetZUpdateCommandBuilder(DataRow row, string tableName, bool compress, int statementIndex, ITableSchema schema, bool isBulk, StringBuilder commandText, List<ILargeColumnSaver> largeColumnSavers)
		{
			return new ZUpdateCommandBuilderBase(row, tableName, compress, statementIndex, schema, isBulk, commandText, largeColumnSavers);
		}

		protected override void Build()
		{
			builders = new List<ZUpdateCommandBuilderBase>();
			var updateStatementIndex = 0;
			foreach (var row in rowsToUpdate)
			{
				var statementBuilder = GetZUpdateCommandBuilder(row, TableName, Compress, updateStatementIndex++, Schema, isBulk: true, commandText: CommandText, LargeColumnSavers);
				builders.Add(statementBuilder);
			}

			CreateNamesAndValues();

			var fromCaluse = GetFromClause();

			if (useParameters)
			{
				CommandText.Append(String.Format(CultureInfo.InvariantCulture,
@"EXEC sys.sp_executesql N'UPDATE {0} SET
{1}
{2}WHERE
{3};
{4}
'
, N'{5}'
, {6};
"
					, AddSchemaName(TableName) // 0
					, SetClause               // 1
					, fromCaluse              // 2
					, WhereClause             // 3
					, SetRowCountClause       // 4
					, ParameterDeclarations   // 5
					, ParameterValues         // 6
					));
			}
			else
			{
				CommandText.Append(String.Format(CultureInfo.InvariantCulture,
@"UPDATE {0} SET
{1}
{2}WHERE 1=1
{3};
"
					, AddSchemaName(TableName) // 0
					, SetClause               // 1
					, fromCaluse              // 2
					, WhereClause             // 3
					));
			}
		}

		internal override void AddConcurrencyCheck(StringBuilder currentCommandText, string concurrencyErrorText)
		{
			if (IsConcurrencyCheckRequired)
			{
				if (useParameters)
				{
					currentCommandText.AppendLine(String.Format(CultureInfo.InvariantCulture, "if (@row_count <> {1}) RAISERROR('{0}', 16, 1);", concurrencyErrorText, UpdateRowsCount)); // part of sql command
				}
				else
				{
					currentCommandText.AppendLine(String.Format(CultureInfo.InvariantCulture, "if @@ROWCOUNT <> {1} RAISERROR('{0}', 16, 1);", concurrencyErrorText, UpdateRowsCount)); // sql command part
				}
			}
		}

		#endregion // SuppressResourceStringsCheckRegion

		#region NamesAndValues

		internal class NameAndValue
		{
			public string Name;
			public string TypeDeclaration;
			public string Value;
			public bool IsDeclaration;
		}

		List<NameAndValue> namesAndValues;

		void CreateNamesAndValues()
		{
			namesAndValues = new List<NameAndValue>();
			if (IsConcurrencyCheckRequired)
			{
				namesAndValues.Add(new NameAndValue { Name = "@0", TypeDeclaration = "int out", Value = "@row_count out", IsDeclaration = true }); // part of sql procedure declaration
			}

			AddSetClauseAndConcurrencyCheckWhereClause();
		}

		string GetFromClause()
		{
			var fromCaluse = "";
			if (builders.Count > 0)
			{
				fromCaluse = builders[0].GetFromClauseForBulk();
			}

			return fromCaluse;
		}

		protected void AddSetClauseAndConcurrencyCheckWhereClause()
		{
			foreach (var builder in builders)
			{
				builder.CreateNamesAndValuesForBulk();
			}
		}

		class SetNamesAndValues
		{
			public string ColumnName;
			public string Value;
			public string PK;
		}

		#region SuppressResourceStringsCheckRegion

		public string SetClause
		{
			get
			{
				var setColumns = new Dictionary<string, List<SetNamesAndValues>>();
				var setClauseBuilder = new StringBuilder();

				if (Schema != null)
				{
					var pkName = Schema.PK.Name;

					var builderCount = builders.Count;
					for (var i = 0; i < builderCount; i++)
					{
						var builder = builders[i];
						var namesAndValuesForBulk = builder.GetSetNamesAndValuesForBulk();
						var pkValue = builder.GetPkValueForBulk();

						foreach (var nameAndValue in namesAndValuesForBulk)
						{
							var setNamesAndValues = new SetNamesAndValues();
							setNamesAndValues.ColumnName = nameAndValue.Key;
							setNamesAndValues.Value = nameAndValue.Value;
							setNamesAndValues.PK = pkValue;

							if (!setColumns.ContainsKey(setNamesAndValues.ColumnName))
							{
								setColumns.Add(setNamesAndValues.ColumnName, new List<SetNamesAndValues>());
							}
							setColumns[setNamesAndValues.ColumnName].Add(setNamesAndValues);
						}
					}

					foreach (var setColum in setColumns)
					{
						if (0 < setClauseBuilder.Length)
						{
							_ = setClauseBuilder.AppendLine(",");
						}
						_ = setClauseBuilder.AppendFormat("\t{0} = CASE {1}", setColum.Key, pkName).AppendLine();
						foreach (var setNameAndValue in setColum.Value)
						{
							_ = setClauseBuilder.AppendFormat("\tWHEN {0} THEN {1}", setNameAndValue.PK, setNameAndValue.Value).AppendLine();
						}
						if (setColum.Value.Count < UpdateRowsCount)
						{
							_ = setClauseBuilder.AppendFormat("\tELSE {0}", setColum.Key).AppendLine();
						}
						_ = setClauseBuilder.Append("\tEND");
					}
				}

				return setClauseBuilder.ToString();
			}
		}

		public string WhereClause
		{
			get
			{
				var whereClauseBuilder = new StringBuilder();
				var builderCount = builders.Count;

				var pkValues = builders.Select(b => b.GetPkValueForBulk());
				var pkbrackets = string.Join(",", pkValues);

				var firstPart = new StringBuilder();
				firstPart.AppendFormat("\t{0} in ({1})", Schema.PK.Name, pkbrackets);
				firstPart.AppendLine("\n\tAND\n\t(");

				for (var i = 0; i < builderCount; i++)
				{
					var builder = builders[i];
					if (builderCount > 1)
					{
						_ = firstPart.AppendLine(i > 0 ? "\t\tOR\n\t\t(" : "\t\t(");
					}
					_ = firstPart.Append("\t\t\t1=1");
					_ = firstPart.AppendFormat("\n\t\t\t{0}", builder.WhereClause);

					if (builderCount > 1)
					{
						_ = firstPart.AppendLine("\n\t\t)");
					}
				}

				if (builderCount > 0)
				{
					_ = whereClauseBuilder.AppendLine("\t)");
				}

				var final = new StringBuilder();
				_ = final.AppendLine(firstPart.ToString().TrimEnd());
				_ = final.AppendLine(whereClauseBuilder.ToString().TrimEnd());
				return final.ToString().TrimEnd();
			}
		}
		#endregion // SuppressResourceStringsCheckRegion

		public string SetRowCountClause
		{
			get { return (IsConcurrencyCheckRequired) ? "SET @0 = @@ROWCOUNT;" : String.Empty; } // sql command part
		}

		public string ParameterDeclarations
		{
			get
			{
				var parameterDeclarationsBuilder = new StringBuilder();
				_ = parameterDeclarationsBuilder.AppendLine(String.Join(", ", namesAndValues.Where(p => p.IsDeclaration).Select(p => String.Format(CultureInfo.InvariantCulture, "{0} {1}", p.Name, p.TypeDeclaration))));
				var builderCount = builders.Count;
				for (var i = 0; i < builderCount; i++)
				{
					var builder = builders[i];
					if (parameterDeclarationsBuilder.Length > 0)
					{
						_ = parameterDeclarationsBuilder.Append(",");
					}
					_ = parameterDeclarationsBuilder.AppendLine(builder.ParameterDeclarations);
				}

				return parameterDeclarationsBuilder.ToString();
			}
		}

		public string ParameterValues
		{
			get
			{
				var parameterValuesBuilder = new StringBuilder();
				parameterValuesBuilder.AppendLine(String.Join(", ", namesAndValues.Where(p => p.IsDeclaration).Select(p => String.Format(CultureInfo.InvariantCulture, "{0} = {1}", p.Name, p.Value))));
				var builderCount = builders.Count;
				for (var i = 0; i < builderCount; i++)
				{
					var builder = builders[i];
					if (parameterValuesBuilder.Length > 0)
					{
						_ = parameterValuesBuilder.Append(",");
					}
					_ = parameterValuesBuilder.AppendLine(builder.ParameterValues);
				}

				return parameterValuesBuilder.ToString();
			}
		}

		#endregion // NamesAndValues
	}
}
