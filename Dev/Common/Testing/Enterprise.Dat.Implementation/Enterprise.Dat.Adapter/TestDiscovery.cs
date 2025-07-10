using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.BuildTools;
using Dat.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.DevTools.Common.Capabilities;

namespace Enterprise.Dat.Implementation
{
	public sealed class TestDiscovery : ITestDiscovery
	{
		public TestDiscovery(TestAdapterContext adapterContext)
		{
			context = adapterContext;
		}

		public IEnumerable<Scope> GetAllScopes(string sourcePath, string binPath)
		{
			var xmlPath = Path.Combine(sourcePath, BuildConstants.SolutionXmlFileName);
			if (!File.Exists(xmlPath))
			{
				xmlPath = Path.Combine(sourcePath, BuildConstants.BuildXmlFileName);
			}
			var buildXml = new BuildXml(xmlPath, flattenSubmodules: false);
			var allScopes = buildXml
				.GetAllSolutionFileNames()
				.Select(sln =>
				{
					var assemblies = buildXml.GetAssembliesTestedInSolution(sln);

					var duplicateAssemblies = assemblies.GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
						.Where(x => x.Count() > 1)
						.Select(x => x.Key)
						.ToList();

					if (duplicateAssemblies.Count > 0)
					{
						throw new InvalidOperationException($"The following assemblies are referenced multiple times in Build.xml: {string.Join(", ", duplicateAssemblies)}. Please resolve the duplicates.");
					}

					return assemblies.ToDictionary(
						asm => asm,
						asm => new Scope(Path.GetFileNameWithoutExtension(asm), GetSourcePath(sourcePath, sln, Path.GetFileNameWithoutExtension(asm)))
					);
				}) // create dictionary with assembly name & scope for all projects in solution
				.SelectMany(x => x).GroupBy(d => d.Key).ToDictionary(x => x.Key, y => y.First().Value); // list of dictionaries to one dictionary

			return GetScopesWithTests(allScopes, binPath);
		}

		string GetSourcePath(string sourcePath, string slnPath, string assemblyName)
		{
			var slnDir = Path.GetDirectoryName(Path.Combine(sourcePath, slnPath));
			var slnFile = new WTG.DevTools.Definitions.SolutionFile(Path.Combine(sourcePath, slnPath));
			var matchingProject = slnFile.Projects.FirstOrDefault(p => p.Name == assemblyName);
			if (matchingProject == null)
			{
				foreach (var project in slnFile.Projects)
				{
					var projecPath = Path.Combine(slnDir, project.ProjectPath);
					if (File.Exists(projecPath) && File.ReadAllText(projecPath).Contains($"<AssemblyName>{assemblyName}<"))
					{
						matchingProject = project;
						break;
					}
				}
			}
			if (matchingProject != null)
			{
				return Path.GetDirectoryName(Path.Combine(slnDir, matchingProject.ProjectPath));
			}
			else
			{
				return slnDir;
			}
		}

		internal IEnumerable<Scope> GetScopesWithTests(Dictionary<string, Scope> allScopes, string binPath)
		{
			// set up load context to load assemblies later
			var frameworkPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
			var resolver = new PathAssemblyResolver
			(
				Directory.EnumerateFiles(frameworkPath, "*.dll")
					.Concat(Directory.EnumerateFiles(binPath, "*.dll"))
			);
			using (var mlc = new MetadataLoadContext(resolver))
			{
				// add all scopes that directly reference NUnit to testScopes
				var testScopes = new ConcurrentBag<Scope>();
				var testNames = new ConcurrentDictionary<string, byte>();
				// dictionary contains assembly file path and Scope
				var otherScopes = new ConcurrentDictionary<string, Scope>();
				Parallel.ForEach(allScopes, scope =>
				{
					if (scope.Key.StartsWith(CommonAssemblyInfo.CWNetCoreSubfolder, StringComparison.OrdinalIgnoreCase))
					{
						// Skip NetCore Directories, Covered by NetCoreTestAdapter with VsTest implementation
						return;
					}

					var file = Path.Combine(binPath, scope.Key);

					var assembly = mlc.LoadFromAssemblyPath(file);
					var references = assembly.GetReferencedAssemblies();

					var isTestAssembly = assembly.FullName.Contains("NUnit", StringComparison.OrdinalIgnoreCase) || references.Any(reference => reference.Name.Contains("NUnit", StringComparison.OrdinalIgnoreCase));

					if (isTestAssembly)
					{
						testScopes.Add(scope.Value);
						testNames.TryAdd(scope.Value.Name, 0);
					}
					else
					{
						otherScopes.TryAdd(file, scope.Value);
					}
				});

				// search all remaining scopes for references to test scopes
				var foundTestScope = true;
				while (foundTestScope)
				{
					foundTestScope = false;
					Parallel.ForEach(otherScopes, scope =>
					{
						var assembly = mlc.LoadFromAssemblyPath(scope.Key);
						var references = assembly.GetReferencedAssemblies();
						var referencesTestProject = references.Any(r => testNames.ContainsKey(r.Name));
						if (referencesTestProject)
						{
							testScopes.Add(scope.Value);
							testNames.TryAdd(scope.Value.Name, 0);
							foundTestScope = true;
							otherScopes.TryRemove(scope.Key, out var s);
						}
					});
				}
				return testScopes;
			}
		}

		public IEnumerable<TestDescriptor> GetTests(string sourcePath, string binPath, string scopeName)
		{
			var testCaseRetriever = new SubClassRetriever(binPath, new string[] { scopeName }, typeof(TestCase));
			testCaseRetriever.IncludeTestClasses = true;
			testCaseRetriever.IncludeAbstractClasses = false;
			testCaseRetriever.ExcludedAttributes = new[] { typeof(DoNotAddToTestTreeAttribute) };
			var testCaseTypes = testCaseRetriever.Retrieve();

			var includeDeveloperOnlyTests = context.Options.ContainsKey(TestDiscoveryOptions.IncludeDeveloperOnlyTests);
			var isAlp = ReleaseInfo.Instance.ReleaseRing == WTG.DevTools.Definitions.ReleaseRings.Codes.ALP;

			return FindTestMethods(testCaseTypes,
				new FindTestMethodArguments(scopeName, TestCase.TestMethodPrefix, includeDeveloperOnlyTests, isAlp));
		}

		static IEnumerable<TestDescriptor> FindTestMethods(Type[] testCaseTypes, FindTestMethodArguments args)
		{
			var tests = new ConcurrentBag<TestDescriptor>();
			Parallel.ForEach(testCaseTypes, testCaseType =>
			{
				foreach (var test in FindTestMethodsInType(testCaseType, args))
				{
					tests.Add(test);
				}
			});
			return tests
				.Distinct(comparer: new TestIdentifierEqualityComparer())
				.OrderBy(t => t.Identifier.ElementName + "." + t.Identifier.TargetName);
		}

		static List<TestDescriptor> FindTestMethodsInType(Type testCaseType, FindTestMethodArguments args)
		{
			var testCaseAndAssemblyRequiresSoftware = testCaseType.Assembly.GetCustomAttributesCached<RequiresSoftwareAttribute>()
								.Concat(testCaseType.GetCustomAttributesCached<RequiresSoftwareAttribute>(true))
								.Select(x => x.Software)
								.Aggregate((RequiredSoftware)0, UnionRequiredSoftware);

			var testCaseDatCapabilityRequirements = testCaseType.GetCustomAttributesCached<DatCapabilityRequirementAttribute>(true);

			var testList = new List<TestDescriptor>();
			var addedMethods = new HashSet<string>();  //This is to ensure the first method with same signature will get added (Where the first on is the one actually get run), to ensure inheritance detection always work
			foreach (var method in testCaseType.GetMethods())
			{
				if (addedMethods.Contains(method.ToString()))
				{
					continue;
				}

				var isMatchedTestMethod = method.Name.StartsWith(args.SearchPrefix, StringComparison.OrdinalIgnoreCase) && method.ReturnType == typeof(void) && method.GetParameters().Length == 0;
				if (!isMatchedTestMethod)
				{
					continue;
				}

				var runOnDat = args.IncludeDeveloperOnlyTests || !testCaseType.HasAttribute<DeveloperOnlyTestAttribute>(method);
				runOnDat = runOnDat && (args.IsAlp || !testCaseType.HasAttribute<ALPOnlyAttribute>(method));
				if (!runOnDat)
				{
					continue;
				}

				var requiresSoftware = method.GetCustomAttributes<RequiresSoftwareAttribute>(true)
					.Select(x => x.Software)
					.Aggregate(testCaseAndAssemblyRequiresSoftware, UnionRequiredSoftware);

				var datTestFlags = DatTestFlags.Default;
				if (GUITestDetection.IsGuiTest(method))
				{
					datTestFlags |= DatTestFlags.GUITest;
					requiresSoftware |= RequiredSoftware.CanRunGUITests;
				}
				if (testCaseType.HasAttribute<TestRequiresAdministrativePrivilegesAttribute>(method))
				{
					datTestFlags |= DatTestFlags.RequiresAdminPrivileges;
				}
				if (testCaseType.HasAttribute<FrequentlyFailingAttribute>(method))
				{
					datTestFlags |= DatTestFlags.FrequentlyFailing;
				}

				var capabilityRequirements = GetCapabilityRequirements(testCaseDatCapabilityRequirements, method, ref requiresSoftware);

				var addNormaltest = true;
				var targetFrameworksAttribute = testCaseType.GetTargetFrameworksAttribute(method);
				if (targetFrameworksAttribute != null)
				{
					if (args.IsAlp && targetFrameworksAttribute.TargetFrameworks.HasFlag(TargetFramework.NetCore))
					{
						var net8capabilityRequirements = capabilityRequirements.Concat(new[] { "DOTNET_RUNTIME_8.0_COMPAT" }).ToArray();
						addedMethods.Add(method.ToString());
						testList.Add(
							new TestDescriptor(args.ScopeName, testCaseType.FullName, method.Name + $"[{nameof(TargetFramework.NetCore)}]", (int)requiresSoftware, datTestFlags, net8capabilityRequirements));
					}

					addNormaltest = targetFrameworksAttribute.TargetFrameworks.HasFlag(TargetFramework.NetFramework);
				}

				if (addNormaltest)
				{
					addedMethods.Add(method.ToString());
					testList.Add(
						new TestDescriptor(args.ScopeName, testCaseType.FullName, method.Name, (int)requiresSoftware, datTestFlags, capabilityRequirements));
				}
			}

			return testList;
		}

		static string[] GetCapabilityRequirements(IEnumerable<DatCapabilityRequirementAttribute> testCaseDatCapabilityRequirements, MethodInfo method, ref RequiredSoftware requiresSoftware)
		{
			var methodCapabilityRequirements = method.GetCustomAttributes<DatCapabilityRequirementAttribute>(true).ToArray();
			var classCapabilityRequirements = testCaseDatCapabilityRequirements.ToArray();
			var hasNamedCapabilityRequirements = (requiresSoftware & RequiredSoftware.AllNamedCapabilities) != 0;

			if (!hasNamedCapabilityRequirements && methodCapabilityRequirements.Length == 0 && classCapabilityRequirements.Length == 0)
			{
				return GetBaseCapabilityRequirements();
			}

			var capabilityRequirementsSet = new HashSet<string>();

			if (requiresSoftware.HasFlag(RequiredSoftware.IsVM))
			{
				capabilityRequirementsSet.Add("VM");
			}
			if (requiresSoftware.HasFlag(RequiredSoftware.VisualStudio))
			{
				capabilityRequirementsSet.Add("VS2022");
			}
			if (requiresSoftware.HasFlag(RequiredSoftware.Fonts))
			{
				capabilityRequirementsSet.Add("FONTS");
			}
			if (requiresSoftware.HasFlag(RequiredSoftware.OfficeFonts))
			{
				capabilityRequirementsSet.Add("OFFICE_FONTS");
			}
			if (requiresSoftware.HasFlag(RequiredSoftware.WinSdk))
			{
				capabilityRequirementsSet.Add("WINSDK");
			}
			if (requiresSoftware.HasFlag(RequiredSoftware.DotNetSdk))
			{
				capabilityRequirementsSet.Add("DOTNETSDK8.0");
			}

			requiresSoftware = requiresSoftware & ~RequiredSoftware.AllNamedCapabilities;

			var hasAddedSqlCapability = false;
			AddToCapabilitiesExcludingSqlIfAlreadyAdded(methodCapabilityRequirements.Select(c => c.DatCapabilityRequirement));
			AddToCapabilitiesExcludingSqlIfAlreadyAdded(classCapabilityRequirements.Select(c => c.DatCapabilityRequirement));
			AddToCapabilitiesExcludingSqlIfAlreadyAdded(GetBaseCapabilityRequirements());

			return capabilityRequirementsSet.ToArray();

			void AddToCapabilitiesExcludingSqlIfAlreadyAdded(IEnumerable<string> capabilities)
			{
				var foundSQLCapabilityInThisScope = false;
				foreach (var capability in capabilities)
				{
					if (SqlCapability.IsSqlCapability(capability))
					{
						foundSQLCapabilityInThisScope = true;
						if (!hasAddedSqlCapability)
						{
							var sqlCapability = SqlCapability.Parse(capability);
							capabilityRequirementsSet.Add(minimumSupportedSqlVersion.IsCompatibleWith(sqlCapability) ? MinimumSupportedSqlCapability : capability);
						}
					}
					else
					{
						capabilityRequirementsSet.Add(capability);
					}
				}
				hasAddedSqlCapability = hasAddedSqlCapability || foundSQLCapabilityInThisScope;
			}
		}

		const string MinimumSupportedSqlCapability = "SQL2019";
		static readonly SqlVersion minimumSupportedSqlVersion = SqlVersion.Parse("SQL2019");
		static string[] GetBaseCapabilityRequirements() => new[] { "NET48", MinimumSupportedSqlCapability };

		static RequiredSoftware UnionRequiredSoftware(RequiredSoftware a, RequiredSoftware b)
		{
			return a | b;
		}

		class TestIdentifierEqualityComparer : IEqualityComparer<TestDescriptor>
		{
			public bool Equals(TestDescriptor x, TestDescriptor y)
			{
				return x.Identifier.Equals(y.Identifier);
			}

			public int GetHashCode(TestDescriptor obj)
			{
				return obj.Identifier.GetHashCode();
			}
		}

		readonly TestAdapterContext context;

		struct FindTestMethodArguments
		{
			public FindTestMethodArguments(string scopeName, string searchPrefix, bool includeDeveloperOnlyTests, bool isAlp)
			{
				ScopeName = scopeName;
				SearchPrefix = searchPrefix;
				IncludeDeveloperOnlyTests = includeDeveloperOnlyTests;
				IsAlp = isAlp;
			}

			public string ScopeName { get; }
			public string SearchPrefix { get; }
			public bool IncludeDeveloperOnlyTests { get; }
			public bool IsAlp { get; }
		}
	}
}
