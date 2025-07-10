using System.Collections.Generic;

namespace Enterprise.BusinessObjectGenerator.ModelView
{
	public class ModelViews : ModelViewSourceFile
	{
		readonly List<ModelViewContext> allContexts;

		public ModelViews(List<ModelViewContext> allContexts) : base(null)
		{
			this.allContexts = allContexts;
		}

		protected override string Body
		{
			get
			{
				return LinesOfCode(CodeForImports, EmptyLineString, CodeForNameSpace, BlockStartString, CodeForBodyOfClass, BlockEndString);
			}
		}

		protected string CodeForImports
		{
			get
			{
				return LinesOfCode(
					"using System.Collections.Generic;",
					"using CargoWise.DbUpgrader.Scripts.Abstractions;"
					);
			}
		}

		protected string CodeForNameSpace
		{
			get
			{
				return $"namespace {ModelViewConstants.NamespacePrefix}";
			}
		}

		protected string CodeForBodyOfClass
		{
			get
			{
				return LinesOfCode(
					CodeForClassDefinition,
					IndentL1 + BlockStartString,
					CodeForViews,
					IndentL1 + BlockEndString
					);
			}
		}

		protected string CodeForClassDefinition => $"{IndentL1}public static class {ClassName}";

		protected string CodeForViews
		{
			get
			{
				if (allContexts.Count == 0)
				{
					return string.Empty;
				}

				var linesOfCode = new List<string>
				{
					$"{IndentL2}public static IEnumerable<DbCreateViewScript> GetViews()",
					IndentL2 + BlockStartString
				};

				foreach (var singleContext in allContexts)
				{
					if (singleContext.HasIndex)
					{
						linesOfCode.Add(GetYieldStatement(CSharpHelper.GetFullIndexedViewClassName(singleContext)));
					}

					linesOfCode.Add(GetYieldStatement(CSharpHelper.GetFullViewClassName(singleContext)));
				}

				linesOfCode.Add(IndentL2 + BlockEndString);

				return LinesOfCode(linesOfCode.ToArray());
			}
		}

		string GetYieldStatement(string fqClassName) => $"{IndentL3}yield return new {fqClassName}();";

		const string ClassName = "ModelViews";
	}
}
