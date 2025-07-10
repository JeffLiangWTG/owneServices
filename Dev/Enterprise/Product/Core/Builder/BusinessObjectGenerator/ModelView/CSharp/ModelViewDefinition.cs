using System.Collections.Generic;
using System.Linq;

namespace Enterprise.BusinessObjectGenerator.ModelView
{
	public class ModelViewDefinition : ModelViewSourceFile
	{
		public ModelViewDefinition(ModelViewContext context) : base(context) { }

		protected override string Body
		{
			get
			{
				var lines = new List<string>
				{
					CodeForImports,
					EmptyLineString,
					CodeForNameSpace,
					BlockStartString,
					CodeForViewClass,
				};

				if (Context.HasIndex)
				{
					lines.Add(CodeForIndexedViewClass);
				}

				lines.Add(BlockEndString);

				return LinesOfCode(lines.ToArray());
			}
		}

		protected string CodeForImports
		{
			get
			{
				if (Context.HasIndex)
				{
					return LinesOfCode(
						"using System.Collections.Generic;",
						"using CargoWise.Database.Shared;",
						"using CargoWise.DbUpgrader.Scripts.Abstractions;"
					);
				}

				return LinesOfCode("using CargoWise.DbUpgrader.Scripts.Abstractions;");
			}
		}

		protected string CodeForNameSpace
		{
			get
			{
				return $"namespace {Context.DefinitionNamespace}";
			}
		}

		protected string CodeForViewClass
		{
			get
			{
				return LinesOfCode(
					CodeForViewClassDefinition,
					IndentL1 + BlockStartString,
					CodeForViewSqlResourceFileNameProperty,
					IndentL1 + BlockEndString
					);
			}
		}

		protected string CodeForIndexedViewClass
		{
			get
			{
				return LinesOfCode(
					EmptyLineString,
					CodeForIndexedViewClassDefinition,
					IndentL1 + BlockStartString,
					CodeForIndexedViewProperties,
					IndentL1 + BlockEndString
					);
			}
		}

		string CodeForDevelopmentAttribute => $"{IndentL1}[{Context.DevelopmentAttributeName}]";

		string CodeForViewClassDefinition
		{
			get
			{
				var className = CSharpHelper.GetViewClassName(Context);
				var classDefinition = $"{IndentL1}public class {className} : DbCreateViewScript";

				if (Context.ModelView.DevelopmentOnly)
				{
					return LinesOfCode(CodeForDevelopmentAttribute, classDefinition);
				}

				return classDefinition;
			}
		}

		string CodeForViewSqlResourceFileNameProperty =>
			$"{IndentL2}protected override string SqlResourceFileName => GetType().FullName + \".model\" + SqlScriptExtension;";

		string CodeForIndexedViewClassDefinition
		{
			get
			{
				var className = CSharpHelper.GetIndexedViewClassName(Context);
				var classDefinition = $"{IndentL1}public class {className} : DbCreateIndexedViewScript, IIndexedViewWithTemporaryIndexesOrColumns";

				if (Context.ModelView.DevelopmentOnly)
				{
					return LinesOfCode(CodeForDevelopmentAttribute, classDefinition);
				}

				return classDefinition;
			}
		}

		string CodeForIndexedViewSqlResourceFileNameProperty =>
			$"{IndentL2}protected override string SqlResourceFileName => GetType().FullName.Replace(\"_Idx\", \"\") + \".index\" + SqlScriptExtension;";

		string CodeForIndexesProperty
		{
			get
			{
				var lines = new List<string>
				{
					$"{IndentL2}public override List<IndexInfo> Indexes",
					$"{IndentL2}{BlockStartString}",
					$"{IndentL3}get",
					$"{IndentL3}{BlockStartString}",
					$"{IndentL4}return new List<IndexInfo>()",
					$"{IndentL4}{BlockStartString}"
				};

				var clusteredIndexName = string.Join("_",
					new[]
					{
						"NR_UC_",
						Context.ClusterKeyColumnName,
						Context.PKColumnName
					}.Where(x => !string.IsNullOrEmpty(x)));
				lines.Add(GetCodeForIndexBuilder(clusteredIndexName, isClustered: true, GetClusterKeys(Context)));

				foreach (var addInfo in Context.ModelView.AddInfos)
				{
					if (!addInfo.Indexed)
					{
						continue;
					}

					lines.Add(GetCodeForIndexBuilder(addInfo.Name, isClustered: false));
				}

				lines.Add(IndentL4 + BlockEndString + LineEndString);
				lines.Add(IndentL3 + BlockEndString);
				lines.Add(IndentL2 + BlockEndString);

				return LinesOfCode(lines.ToArray());
			}
		}

		string CodeForTemporaryIndexesProperty
		{
			get
			{
				var lines = new List<string>
				{
					$"{IndentL2}public List<IndexInfo> TemporaryIndexes",
					$"{IndentL2}{BlockStartString}",
					$"{IndentL3}get",
					$"{IndentL3}{BlockStartString}",
					$"{IndentL4}return new List<IndexInfo>()",
					$"{IndentL4}{BlockStartString}",
					$"{IndentL5}IndexInfo.Builder.New(WellKnownSqlNames.DbOwnerSchema, {Quote(Context.ModelView.Table)}," +
								$" IndexInfo.MANUALLY_CREATED_WTG_INDEX_PREFIX + {Quote(Context.ModelName)})",
					$"{IndentL6}.Unique(true)",
					$"{IndentL6}.Clustered(false)"
				};

				foreach (var clusterKey in GetClusterKeys(Context))
				{
					lines.Add($"{IndentL6}.Key({Quote(clusterKey)})");
				}

				lines.Add(CodeForTemporaryIndexIncludes);

				if (!string.IsNullOrWhiteSpace(Context.ModelView.SearchCondition))
				{
					lines.Add($"{IndentL6}.Where(@{Quote(Context.ModelView.SearchCondition)})");
				}

				lines.Add($"{IndentL6}.Option(IndexOptions.ONLINE, value: true)");
				lines.Add($"{IndentL6}.GetInfo(),");
				lines.Add(IndentL4 + BlockEndString + LineEndString);
				lines.Add(IndentL3 + BlockEndString);
				lines.Add(IndentL2 + BlockEndString);

				return LinesOfCode(lines.ToArray());
			}
		}

		string CodeForTemporaryIndexIncludes
		{
			get
			{
				var lines = new List<string>
				{
					$"{IndentL6}.Include(",
					$"{IndentL7}{Quote(Context.AddInfoColumnName)},",
				};

				var addInfosToAdd = Context.ModelView.AddInfos.Where(a => a.Indexed).ToArray();
				for (var i = 0; i < addInfosToAdd.Length; i++)
				{
					var addInfo = addInfosToAdd[i];

					var addInfoName = addInfo.Name.Substring(addInfo.Name.IndexOf("_") + 1);
					var lineEnd = i + 1 < addInfosToAdd.Length ? "," : string.Empty;
					lines.Add($"{IndentL7}{Quote($"CW!!{Context.ModelName}_{addInfoName}")}{lineEnd}");
				}

				lines.Add(IndentL6 + ")");

				return LinesOfCode(lines.ToArray());
			}
		}

		string CodeForTemporaryComputedColumnsProperty
		{
			get
			{
				var lines = new List<string>
				{
					$"{IndentL2}public List<ComputedColumnInfo> TemporaryComputedColumns",
					$"{IndentL2}{BlockStartString}",
					$"{IndentL3}get",
					$"{IndentL3}{BlockStartString}",
					$"{IndentL4}var builder = ComputedColumnInfo.Builder.New(WellKnownSqlNames.DbOwnerSchema, {Quote(Context.ModelView.Table)});",
					$"{IndentL4}return new List<ComputedColumnInfo>()",
					$"{IndentL4}{BlockStartString}",
				};

				foreach (var addInfo in Context.ModelView.AddInfos)
				{
					if (!addInfo.Indexed)
					{
						continue;
					}

					lines.Add(GetCodeForComputedColumnInfo(addInfo));
				}

				lines.Add(IndentL4 + BlockEndString + LineEndString);
				lines.Add(IndentL3 + BlockEndString);
				lines.Add(IndentL2 + BlockEndString);

				return LinesOfCode(lines.ToArray());
			}
		}

		string GetCodeForComputedColumnInfo(AddInfo addInfo)
		{
			return LinesOfCode(
				$"{IndentL5}builder.Name({Quote($"CW!!{Context.ModelName}_{addInfo.NormalizedName}")})",
				$"{IndentL6}.ComputedExpression({Quote(SqlHelper.GetColumnScript(addInfo, addInfo.IsUnicode ? Context.NAddInfoColumnName : Context.AddInfoColumnName))})",
				$"{IndentL6}.GetInfo(),");
		}

		string CodeForIndexedViewProperties =>
			LinesOfCode(
				CodeForIndexedViewSqlResourceFileNameProperty,
				EmptyLineString,
				CodeForIndexesProperty,
				EmptyLineString,
				CodeForTemporaryIndexesProperty,
				EmptyLineString,
				CodeForTemporaryComputedColumnsProperty);

		string GetCodeForIndexBuilder(string indexName, bool isClustered, List<string> keys = null)
		{
			var lines = new List<string>();

			if (keys == null)
			{
				keys = new List<string>(1);
			}

			if (keys.Count == 0)
			{
				keys.Add(indexName);
			}

			var indexNameExpr = isClustered ? Quote(indexName) : $"NonclusteredIndexName({Quote(indexName)})";

			lines.Add($"{IndentL5}IndexInfo.Builder.New(WellKnownSqlNames.DbOwnerSchema, Name, {indexNameExpr})");
			lines.Add($"{IndentL6}.Unique({isClustered.ToString().ToLowerInvariant()})");
			lines.Add($"{IndentL6}.Clustered({isClustered.ToString().ToLowerInvariant()})");

			foreach (var key in keys)
			{
				lines.Add($"{IndentL6}.Key({Quote(key)})");
			}
			lines.Add($"{IndentL6}.Option(IndexOptions.ONLINE, value: false)");
			lines.Add($"{IndentL6}.GetInfo(),");

			return LinesOfCode(lines.ToArray());
		}

		static List<string> GetClusterKeys(ModelViewContext generatorContext)
		{
			var clusterKeys = new List<string>();
			if (!string.IsNullOrWhiteSpace(generatorContext.ClusterKeyColumnName))
			{
				clusterKeys.Add(generatorContext.ClusterKeyColumnName);
			}

			if (!string.IsNullOrWhiteSpace(generatorContext.PKColumnName))
			{
				clusterKeys.Add(generatorContext.PKColumnName);
			}

			return clusterKeys;
		}

		string Quote(string textToQuote)
		{
			return QuoteString + textToQuote + QuoteString;
		}

		const string QuoteString = "\"";
	}
}
