using System.Collections.Generic;
using System.Linq;

namespace Enterprise.BusinessObjectGenerator.ModelView
{
	public class SqlView : ModelViewSourceFile
	{
		public SqlView(ModelViewContext context) : base(context)
		{ }

		public override string SourceCode => !NeedGeneration() ? string.Empty : base.SourceCode;

		protected override string Body
		{
			get
			{
				if (!NeedGeneration())
				{
					return string.Empty;
				}

				var lines = new List<string>
				{
					$"CREATE VIEW {ViewName}",
					"WITH SCHEMABINDING",
					"AS",
					"SELECT",
					$"{IndentL1}{GetColumnsClause()}",
					$"FROM [dbo].[{TableName}] {TableAlias}",
				};

				var joinClause = GetJoinClause();
				if (!string.IsNullOrWhiteSpace(joinClause))
				{
					lines.Add(joinClause);
				}

				lines.Add($"{IndentL1}{GetWhereClause(Context.ModelView)}");

				return LinesOfCode(lines.ToArray());
			}
		}

		internal string GetColumnsClause()
		{
			var addInfos = AddInfosToAdd(Context.ModelView).ToList();
			var addInfoNames = addInfos.Select(a => a.Name).ToHashSet();
			var columnPrefix = QualifyColumns ? $"{TableAlias}." : string.Empty;
			var addedColumns = new HashSet<string>();
			var columnLines = new List<string>();

			if (IncludeUnderlyingColumns)
			{
				foreach (var column in Context.Columns)
				{
					if (addInfoNames.Contains(column))
					{
						continue;
					}

					columnLines.Add($"{columnPrefix}{column}");
					addedColumns.Add(column);
				}
			}

			if (!string.IsNullOrWhiteSpace(Context.PKColumnName)
				&& !addedColumns.Contains(Context.PKColumnName))
			{
				columnLines.Add($"{columnPrefix}{Context.PKColumnName}");
				addedColumns.Add(Context.PKColumnName);
			}

			if (!string.IsNullOrWhiteSpace(Context.ClusterKeyColumnName)
				&& !addedColumns.Contains(Context.ClusterKeyColumnName))
			{
				columnLines.Add($"{columnPrefix}{Context.ClusterKeyColumnName}");
				addedColumns.Add(Context.ClusterKeyColumnName);
			}

			foreach (var addInfo in addInfos)
			{
				columnLines.Add(GetColumnDefinition(addInfo, addInfo.IsUnicode ? Context.NAddInfoColumnName : Context.AddInfoColumnName));
			}

			var columnSeparator = $",{System.Environment.NewLine}{IndentL1}";

			return string.Join(columnSeparator, columnLines);
		}

		internal virtual IEnumerable<AddInfo> AddInfosToAdd(View view)
		{
			return (view.ParentContext?.ModelView?.AddInfos.Select(a => {
				a.Indexed = false;
				return a;
			}) ?? new List<AddInfo>()).Union(view.AddInfos);
		}

		internal virtual bool NeedGeneration()
		{
			return Context.ModelView.AddInfos.Count > 0;
		}

		internal virtual string GetJoinClause()
		{
			if (!Context.ModelView.AddInfos.Any(a => a.Indexed))
			{
				return string.Empty;
			}

			var joinLines = new List<string>
			{
				$"JOIN [dbo].[{IndexedViewName}] {IndexedViewAlias} WITH (NOEXPAND)",
				$"{IndentL2}ON {TableAlias}.{Context.PKColumnName} = {IndexedViewAlias}.{Context.PKColumnName}"
			};

			if (!string.IsNullOrWhiteSpace(Context.ClusterKeyColumnName))
			{
				joinLines.Add($"{IndentL2}AND {TableAlias}.{Context.ClusterKeyColumnName} " +
							  $"= {IndexedViewAlias}.{Context.ClusterKeyColumnName}");
			}

			return $"{IndentL1}{string.Join(System.Environment.NewLine, joinLines)}";
		}

		internal string GetWhereClause(View view)
		{
			if (string.IsNullOrWhiteSpace(view.SearchCondition))
			{
				return string.Empty;
			}

			return $"WHERE {view.SearchCondition}";
		}

		internal virtual string GetColumnDefinition(AddInfo addInfo, string addInfoColumnName)
		{
			return addInfo.Indexed
				? $"{IndexedViewAlias}.{addInfo.Name}"
				: SqlHelper.GetColumnDefinition(addInfo, addInfoColumnName);
		}

		internal virtual string ViewName => Context.ModelName;

		internal string TableName => Context.ModelView.Table;

		internal string IndexedViewName => $"{Context.ModelName}_Idx";

		internal virtual bool IncludeUnderlyingColumns => true;

		internal virtual bool QualifyColumns => true;

		internal const string TableAlias = "T";

		internal const string IndexedViewAlias = "I";

		protected override string CommentString => "--";
	}
}
