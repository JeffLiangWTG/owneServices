using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using CargoWise.BuildTools;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UniversalDataBuss.CodeGeneration
{
	#region SuppressResourceStringsCheckRegion
	[ExcludeFromCodeCoverage]
	public static class DataObjectGenerator
	{
		public static void Generate(string baseSourcePath = "..")
		{
			SetUpMsBuild();
			GenerateUniversalDataBussProject(baseSourcePath);
		}

		static void SetUpMsBuild()
		{
			if (MSBuildLocator.CanRegister)
			{
				var instance = DevMSBuildLocator.GetMSBuildInstance();
				MSBuildLocator.RegisterInstance(instance);
			}
		}

		static void GenerateUniversalDataBussProject(string baseSourcePath)
		{
			var projectFile = baseSourcePath + @"\Enterprise\Product\Core\UniversalDataBuss\UniversalDataBuss.DataObjects\UniversalDataBuss.DataObjects.csproj";
			var project = new Project(projectFile);

			Generate(project);
		}

		static void Generate(Project proj)
		{
			foreach (var line in proj.GetItems("Compile").ToList())
			{
				GenerateAuto(proj.DirectoryPath, line.EvaluatedInclude, proj);
			}

			foreach (var sourceFile in Directory.GetFiles(proj.DirectoryPath, "*.cs", SearchOption.AllDirectories))
			{
				var fileName = sourceFile.Substring(proj.DirectoryPath.Length + (proj.DirectoryPath.EndsWith($"{Path.DirectorySeparatorChar}") ? 0 : 1));
				GenerateAuto(proj.DirectoryPath, fileName, proj);
			}

			proj.Save();
		}

		public delegate void Intercept(bool hasChanges, string className, string fullAutoFilePath, string autoFileText);

		public static Intercept InterceptForTesting;

		static void GenerateAuto(string parentDirectory, string filePath, Project proj, bool saveChanges = true)
		{
			if (IsCSharpDocument(filePath) && !filePath.EndsWith("Auto.cs"))
			{
				var fullFilePath = Path.IsPathRooted(filePath) ? filePath : parentDirectory + "\\" + filePath;
				var fileText = File.ReadAllText(fullFilePath);
				var syntaxTree = CSharpSyntaxTree.ParseText(fileText, options: new CSharpParseOptions(preprocessorSymbols: new[] { "DEBUG" }));
				var rootNode = syntaxTree.GetRoot();
				var rootClass = FindSyntax<ClassDeclarationSyntax>(rootNode).FirstOrDefault();

				var extendsTopLevelDataObject = rootClass?.BaseList?.Types.Any(b => b.ToString().Equals("TopLevelDataObject")) ?? false;
				var extendsIDataObject = rootClass?.BaseList?.Types.Any(b => b.ToString().Equals("IDataObject")) ?? false;
				if (extendsTopLevelDataObject || extendsIDataObject)
				{
					CheckIDataObjectHasParameterlessConstructor(rootClass);
					var properties = GetCollectionProperties(rootClass);

					if (properties.Any())
					{
						var autoFileName = AppendAuto(filePath);

						var classDefinition = SyntaxFactory.ClassDeclaration(rootClass.Identifier)
							.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
							.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));

						var dataObjectWriterStrategyType = SyntaxFactory.ParseTypeName("IDataObjectWriterStrategy");
						var writerStrategyIdentifier = SyntaxFactory.Identifier("writerStrategy");
						if (!extendsTopLevelDataObject)
						{
							var field = SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(dataObjectWriterStrategyType).AddVariables(SyntaxFactory.VariableDeclarator(writerStrategyIdentifier)));
							classDefinition = classDefinition.AddMembers(field);

							classDefinition = classDefinition.AddMembers(CreateSetWriterMethod(dataObjectWriterStrategyType, writerStrategyIdentifier));
							classDefinition = classDefinition.AddMembers(CreateIsAllowSetMethod());

							classDefinition = classDefinition.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("ISettableWriterStrategy")));
						}
						foreach (var prop in properties)
						{
							var valueIdentifier = SyntaxFactory.Identifier("value");

							var assignmentExpression = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
							SyntaxFactory.IdentifierName(prop.Identifier), SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName(valueIdentifier)));

							var funcTypeDeclaration = SyntaxFactory.ParseTypeName($"Func<{prop.Type.GetText()}>");

							var check = SyntaxFactory.ParseExpression($"IsAllowSet(nameof({prop.Identifier.ToString()}))");
							var returnTrue = SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression));
							var returnFalse = SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression));
							var ifBlock = SyntaxFactory.Block(SyntaxFactory.ExpressionStatement(assignmentExpression), returnTrue);
							var elseBlock = SyntaxFactory.Block(returnFalse);
							var ifStatement = SyntaxFactory.IfStatement(check, ifBlock, SyntaxFactory.ElseClause(elseBlock));

							var method = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("bool"), "Set" + prop.Identifier.ValueText)
							.AddParameterListParameters(SyntaxFactory.Parameter(
								attributeLists: new SyntaxList<AttributeListSyntax>(),
								modifiers: new SyntaxTokenList(),
								type: funcTypeDeclaration,
								identifier: valueIdentifier,
								@default: null))
							.AddBodyStatements(ifStatement)
							.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

							classDefinition = classDefinition.AddMembers(method);
						}

						var nameSpace = SyntaxFactory.NamespaceDeclaration(rootNode.DescendantNodesAndSelf().OfType<NamespaceDeclarationSyntax>().First().Name)
							.AddMembers(classDefinition);

						var usings = syntaxTree.GetCompilationUnitRoot().Usings;
						var cu = AddUsingsInOrder(usings)

							.AddMembers(nameSpace);

						if (InterceptForTesting == null)
						{
							MakeSettersPrivateAndAddPartialModifierToExistingClass(fullFilePath, rootNode, rootClass);
							WriteAutoFile(parentDirectory, autoFileName, cu);
							AddAutoFileToProject(filePath, proj, autoFileName);
						}
						else
						{
							var hasChanges = MakeSettersPrivateAndAddPartialModifierToExistingClass(fullFilePath, rootNode, rootClass, saveChanges = false);
							InterceptForTesting(hasChanges,
								rootClass.Identifier.ToString(),
								parentDirectory + "\\" + autoFileName,
								AutoGeneratedHeaderComment + Environment.NewLine + Environment.NewLine + cu.NormalizeWhitespace(indentation: "\t").ToFullString() + Environment.NewLine);
						}
					}
				}
			}
		}

		static IEnumerable<PropertyDeclarationSyntax> GetCollectionProperties(ClassDeclarationSyntax rootClass) => rootClass.Members.OfType<PropertyDeclarationSyntax>().Where(prop => prop.Identifier.ValueText.EndsWith("Collection") && prop.AccessorList != null).ToList();

		static void CheckIDataObjectHasParameterlessConstructor(ClassDeclarationSyntax rootClass)
		{
			var constructors = rootClass.Members.OfType<ConstructorDeclarationSyntax>().ToList();
			if (constructors.Count > 0)
			{
				var hasParemeterlessConstructor = constructors.Any(constructor => constructor.ParameterList.Parameters.Count == 0);
				if (!hasParemeterlessConstructor)
				{
					throw new InvalidOperationException($"{rootClass.Identifier}.cs must have a parameterless constructor so it can be parsed from universal xml.");
				}
			}
		}

		static bool MakeSettersPrivateAndAddPartialModifierToExistingClass(string fullFilePath, SyntaxNode rootNode, ClassDeclarationSyntax rootClass, bool saveChanges = true)
		{
			bool hasChanges = false;
			var newRootClass = rootClass;

			if (!rootClass.Modifiers.Any(SyntaxKind.PartialKeyword))
			{
				newRootClass = rootClass.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword).WithTrailingTrivia(SyntaxFactory.Space));
				hasChanges = true;
			}

			var settersToReplace = new List<AccessorDeclarationSyntax>();
			foreach (var prop in GetCollectionProperties(newRootClass))
			{
				var setters = prop.AccessorList.Accessors.Where(a => a.Kind() == SyntaxKind.SetAccessorDeclaration && !a.Modifiers.Any(SyntaxKind.PrivateKeyword));
				if (setters.Any())
				{
					settersToReplace.Add(setters.First());
					hasChanges = true;
				}
			}

			if (hasChanges && saveChanges)
			{
				newRootClass = newRootClass.ReplaceNodes(settersToReplace, (a, b) => a.AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword).WithTrailingTrivia(SyntaxFactory.Space)));
				rootNode = rootNode.ReplaceNode(rootClass, newRootClass);

				using (var writer = File.CreateText(fullFilePath))
				{
					rootNode.WriteTo(writer);
				}
			}

			return hasChanges;
		}

		static CompilationUnitSyntax AddUsingsInOrder(SyntaxList<UsingDirectiveSyntax> usings)
		{
			var usingArrray = usings.ToArray();
			int i = 0, j = 0;
			var cu = new SyntaxList<UsingDirectiveSyntax>();

			while (i < usingArrray.Length && j < defaultUsings.Length)
			{
				var usingI = usingArrray[i].Name.ToFullString();
				var usingJ = defaultUsings[j].Name.ToFullString();

				if (usingI.Equals(usingJ))
				{
					cu = cu.Add(usingArrray[i]);
					i++;
					j++;
				}
				else if (usingI.StartsWith("System") && !usingJ.StartsWith("System"))
				{
					cu = cu.Add(usingArrray[i]);
					i++;
				}
				else if (usingJ.StartsWith("System") && !usingI.StartsWith("System"))
				{
					cu = cu.Add(defaultUsings[j]);
					j++;
				}
				else if (usingI.CompareTo(usingJ) == -1)
				{
					cu = cu.Add(usingArrray[i]);
					i++;
				}
				else
				{
					cu = cu.Add(defaultUsings[j]);
					j++;
				}
			}

			//only possible to enter one of the below loops
			while (i < usingArrray.Length)
			{
				cu = cu.Add(usingArrray[i]);
				i++;
			}

			while (j < defaultUsings.Length)
			{
				cu = cu.Add(defaultUsings[j]);
				j++;
			}

			return SyntaxFactory.CompilationUnit().WithUsings(cu);
		}

		static readonly UsingDirectiveSyntax[] defaultUsings = new UsingDirectiveSyntax[7]
		{
			SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System")),
			SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Collections.Generic")),
			SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Diagnostics.CodeAnalysis")),
			SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Enterprise.UniversalDataBuss.DataObjects.Core")),
			SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Enterprise.UniversalDataBuss.DataObjects.Universal")),
			SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Enterprise.UniversalDataBuss.DataObjects.Universal.Customs")),
			SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Enterprise.UniversalDataBuss.Integration"))
		};

		static void WriteAutoFile(string parentDirectory, string autoFileName, CompilationUnitSyntax cu)
		{
			var fullAutoPath = parentDirectory + "\\" + autoFileName;
			using (var writer = File.CreateText(fullAutoPath))
			{
				writer.WriteLine(AutoGeneratedHeaderComment);
				writer.WriteLine();
				cu.NormalizeWhitespace(indentation: "\t").WriteTo(writer);
				writer.WriteLine();
			}
		}

		const string AutoGeneratedHeaderComment =
@"//------------------------------------------------------------------------------
// <auto-generated>
//     This file was auto-generated using the UniversalDataBuss.CodeGeneration.exe.
//     Do not edit this file, because changes will be lost next time it is generated.
// </auto-generated>
//------------------------------------------------------------------------------";

		static void AddAutoFileToProject(string filePath, Project proj, string autoFileName)
		{
			if (!proj.Xml.Items.Any(p => p.ItemType == "Compile" && p.Update == autoFileName))
			{
				var list = proj.AddItemFast("Compile", autoFileName, new[] { new KeyValuePair<string, string>("DependentUpon", Path.GetFileName(filePath)) });
				list[0].Xml.Include = null;
				list[0].Xml.Update = autoFileName;
			}
		}

		static MethodDeclarationSyntax CreateIsAllowSetMethod()
		{
			var propertyNameIdentifier = SyntaxFactory.Identifier("propertyName");
			var isAllowSetMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("bool"), "IsAllowSet")
				.AddParameterListParameters(SyntaxFactory.Parameter(
					attributeLists: new SyntaxList<AttributeListSyntax>(),
					modifiers: new SyntaxTokenList(),
					type: SyntaxFactory.ParseTypeName("string"),
					identifier: propertyNameIdentifier,
					@default: null))
					.AddBodyStatements(SyntaxFactory.ReturnStatement(SyntaxFactory.ParseExpression("writerStrategy.IsAllowSet(propertyName)")));
			return isAllowSetMethod;
		}

		static MethodDeclarationSyntax CreateSetWriterMethod(TypeSyntax dataObjectWriterStrategyType, SyntaxToken writerStrategyIdentifier)
		{
			var strategyParamIdentifier = SyntaxFactory.Identifier("strategy");
			var setWriterStrategy = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "SetWriterStrategy")
				.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
				.AddParameterListParameters(SyntaxFactory.Parameter(
					attributeLists: new SyntaxList<AttributeListSyntax>(),
					modifiers: new SyntaxTokenList(),
					type: dataObjectWriterStrategyType,
					identifier: strategyParamIdentifier,
					@default: null))
					.AddBodyStatements(SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
					SyntaxFactory.IdentifierName(writerStrategyIdentifier), SyntaxFactory.IdentifierName(strategyParamIdentifier))));
			return setWriterStrategy;
		}

		static string AppendAuto(string fileName)
		{
			return fileName.Replace(".cs", "Auto.cs");
		}

		static IEnumerable<T> FindSyntax<T>(SyntaxNode node)
		{
			if (node is T r)
			{
				yield return r;
			}
			else
			{
				foreach (var child in node.ChildNodes().SelectMany(res => FindSyntax<T>(res)))
				{
					yield return child;
				}
			}
		}

		internal static bool IsCSharpDocument(string filePath)
		{
			return filePath.EndsWith(".CS", StringComparison.OrdinalIgnoreCase);
		}
	}

	#endregion
}
