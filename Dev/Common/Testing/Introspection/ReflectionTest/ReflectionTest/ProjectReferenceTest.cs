using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CargoWise.BuildTools;
using CargoWise.Common;
using Enterprise.ReflectionTest.Utilities;
using Microsoft.Build.Locator;
using Mono.Cecil;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ReflectionTest
{
	sealed class ProjectReferenceTest : TestCase
	{
		protected override void MasterSetUp()
		{
			if (MSBuildLocator.CanRegister)
			{
				var instance = DevMSBuildLocator.GetMSBuildInstance();
				MSBuildLocator.RegisterInstance(instance);
			}

			AssembliesContext.EnsureInstance(AssemblyLoader.GetBinPath(), AssembliesUnderTest.AllAssemblyPaths);

			var netCoreAssemblies = AssembliesUnderTest.AllNetCoreAssemblyPaths
				.Select(Path.GetFileName)
				.Where(name => !Path.GetExtension(name).Equals(".exe", StringComparison.Ordinal))
				.ToHashSet(StringComparer.OrdinalIgnoreCase);

#pragma warning disable CS0436 // Type conflicts with imported type
			AssembliesContext.EnsureInstance(assembliesPath: Path.Combine(AssemblyLoader.GetBinPath(), CommonAssemblyInfo.CWNetCoreSubfolder), buildOutputAssemblyPaths: netCoreAssemblies, isNetCoreInstance: true);
#pragma warning restore CS0436 // Type conflicts with imported type
		}

		[FrequentlyFailing]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoUnusedReferences()
		{
			var buildXml = BuildXml.CreateFromSourceCodeDirectory();
			CombineAssertions(
				"Unused project references detected. \n" +
					"Check that Reference Include and the name of HintPath dll are the same. \n" +
					"If you are referencing a dll for constants only, fix by adding [assembly: UsesConstants()] to tell the analyzer.",
				delegate
				{
					foreach (var solution in buildXml.GetAllSolutionFileNames())
					{
						TestNoUnusedReferencesInSolution(buildXml, solution);
					}
				});
			Assert(true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoDisallowedReferences()
		{
			var buildXml = BuildXml.CreateFromSourceCodeDirectory();
			CombineAssertions(delegate
			{
				foreach (var solution in buildXml.GetAllSolutionFileNames())
				{
					foreach (var assembly in buildXml.GetAllAssembliesInSolution(solution).Where(IsAssembly))
					{
						if (AssemblyChecker.IsNotTargetPrefix(assembly))
						{
							continue;
						}

						AssertNoDisallowedReferencesInAssembly(assembly, solution);
					}
				}
			});
			Assert(true);
		}

		// WARNING WARNING WARNING
		// This test has a ZERO EXCLUSIONS policy[^1]. Code that lives in /Database MUST NOT reference code that lives in other parts of Dev.
		// This is not like other tests where we can get away with it for a bit and little harm is done.
		// This is a hard requirement for ongoing refactoring work for the future of CargoWise One, of GLOW, and of the schema regen.
		// Adding an exclusion or exception is not acceptable and may result in your changes being indiscriminately rolled back.
		// If you think that you really really need an exception, you don't. You either need to refactor, or use alternative APIs that are
		// available already within /Database, the .NET Framework, third-party packages, or upstream source trees that Dev has a dependency on.
		// If you need help with this, see Yaakov Smith, Brian Reichle, or Bret Ehlert.
		// [^1] Except for NUnitCore as explained below.
		// WARNING WARNING WARNING
		[RequiresSoftware(RequiredSoftware.DotNetSdk)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDatabaseFolderIsSelfContained()
		{
			const string FolderScope = @"Database";
			var buildXml = BuildXml.CreateFromSourceCodeDirectory();
			var assemblyProjectLookup = GetAssemblyProjectLookup(buildXml);

			CombineAssertions(delegate
			{
				foreach (var solution in buildXml.GetAllSolutionFileNames().Where(sln => sln.StartsWith(FolderScope + Path.DirectorySeparatorChar)))
				{
					var solutionPath = Path.Combine(BaseSourcePath, solution);
					foreach (var projectPath in GetProjectsFromSolution(solutionPath))
					{
						AssertNoReferenceOutsideFolder(projectPath, FolderScope, assemblyProjectLookup, exceptions: null);
					}
				}
			});
			Assert(true);
		}

		// WARNING WARNING WARNING
		// Read the other warning block above first.
		// This test is to ensure that work in progress does not get rolled back, require rework, or that additional work is added to the project.
		// If this test fails for a reference you have added, you cannot have that assembly reference.
		// Adding an exclusion or exception is not acceptable and may result in your changes being indiscriminately reverted.
		// WARNING WARNING WARNING
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProjectsBeingRefactoredToMoveToDatabaseFolderDontHaveProgressReverted()
		{
			var buildXml = BuildXml.CreateFromSourceCodeDirectory();
			var projectsBeingRefactored = new Dictionary<string, ISet<string>>
			{
				["Enterprise.DbUpgrader.Schema.Launch"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
				{
					"CargoWise.ApplicationContext",
					"CargoWise.BI.Deployment.AnalysisServices",
					"CargoWise.Integration",
					"CargoWise.Types",
					"Enterprise.Environment",
					"Enterprise.Integration",
					"Enterprise.ZArchitecture.Core",
				},
				["Enterprise.DbUpgrader.Schema.Launch.Test"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
				{
					"CargoWise.ApplicationContext",
					"Enterprise.Environment",
					"Enterprise.Integration",
					"Enterprise.ZArchitecture.Core"
				},
				["Enterprise.DbUpgrader.Startup"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
				{
					"CargoWise.ApplicationContext",
					"CargoWise.BI.Maintenance",
					"CargoWise.BI.Product.DataLoad",
					"CargoWise.EntityFramework",
					"CargoWise.Integration",
					"CargoWise.ResourceStrings.Cache",
					"Enterprise.DbUpgrader.Schema.Launch",
					"Enterprise.Environment",
					"Enterprise.Integration",
					"Enterprise.Semaphores.Common",
					"ServiceManager.Integration.ServiceTasks.CW",
					"Enterprise.ServiceManager.Tasks.DbMaintenance",
					"Enterprise.ServiceManager.Tasks.DbSecurityAdmin",
					"Enterprise.ZArchitecture.Core",
					"Resources",
				},
				["Enterprise.DbUpgrader.Startup.Testing"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
				{
					"CargoWise.ApplicationContext",
					"CargoWise.BI.Product.DataLoad",
					"CargoWise.EntityFramework",
					"CargoWise.Integration",
					"Enterprise.DbUpgrader.Schema.Launch",
					"Enterprise.Environment",
					"Enterprise.Integration",
					"Enterprise.ZArchitecture.Business",
					"Enterprise.ZArchitecture.Business.Test",
					"Enterprise.ZArchitecture.Core",
					"Enterprise.ZArchitecture.Core.Test",
					"ServiceManager.Common.CW",
					"Resources",
				},
			};

			const string FolderScope = @"Database";
			var assemblyProjectLookup = GetAssemblyProjectLookup(buildXml);

			CombineAssertions(delegate
			{
				foreach (var kvp in projectsBeingRefactored)
				{
					var project = kvp.Key;
					var referencesYetToBeRemoved = kvp.Value;

					var projectPath = assemblyProjectLookup[project];//.Path;
					AssertNoReferenceOutsideFolder(projectPath, FolderScope, assemblyProjectLookup, exceptions: referencesYetToBeRemoved);
				}
			});
			Assert(true);
		}

		public void AssertNoReferenceOutsideFolder(string projectPath, string folderScope, IReadOnlyDictionary<string, string> assemblyProjectLookup, ISet<string> exceptions)
		{
			if (!projectPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)
				|| projectPath.Contains("\\WinCE\\")) //winCE projects cannot be evaluated with modern Build tooling)
			{
				return;
			}

			try
			{
				var projectCollection = new Microsoft.Build.Evaluation.ProjectCollection();
				var project = projectCollection.LoadProject(projectPath);
				var assemblyName = project.GetPropertyValue("AssemblyName");

				var references = project.GetItems("Reference").Select(item => item.EvaluatedInclude).ToList();
				var commaSeparator = new[] { ',' };
				var expectedProjectBaseDirectory = Path.Combine(BaseSourcePath, folderScope) + Path.DirectorySeparatorChar;

				var unusedExceptions = exceptions?.ToHashSet(StringComparer.OrdinalIgnoreCase);

				foreach (var reference in references)
				{
					var referenceAssemblyName = reference.Split(commaSeparator, 2)[0];
					if (string.IsNullOrEmpty(referenceAssemblyName))
					{
						continue;
					}

					if (IsSystemAssembly(referenceAssemblyName))
					{
						continue;
					}

					if (string.Equals(referenceAssemblyName, "NUnitCore", StringComparison.OrdinalIgnoreCase))
					{
						// Temporary exemption until we have moved enough code into /Database such that we can migrate to
						// NUnit 3 and set up Odyssey for tests that require it without using CW1 infrastructure.
						// This is the ONLY exemption, do not add your own.
						continue;
					}

					if (exceptions != null && exceptions.Contains(referenceAssemblyName))
					{
						unusedExceptions.Remove(referenceAssemblyName);
						continue;
					}

					if (!assemblyProjectLookup.TryGetValue(referenceAssemblyName, out var projectThatBuildsThisAssembly))
					{
						continue;
					}

					AssertStartsWith($"Projects under the '{folderScope}' path must only reference other projects within that path. {assemblyName} cannot reference {referenceAssemblyName}.", expectedProjectBaseDirectory, projectThatBuildsThisAssembly);
				}

				if (unusedExceptions != null && unusedExceptions.Count > 0)
				{
					Fail($"Please remove these assemblies from the exceptions list of '{assemblyName}' as they are no longer referenced: {string.Join(",", unusedExceptions)}");
				}

				projectCollection.UnloadProject(project);
			}
			catch (ArgumentException ex)
			{
				var message = $"Error processing project. Path: {projectPath}. Exception Message: {ex.Message}";
				throw new Exception(message, ex);
			}
		}

		public void TestZArchitectureDoesNotReferenceMasterFiles()
		{
			var badReferencePaths = new List<string>();
			badReferencePaths.AddRange(FindBadReferencePaths("Enterprise.ZArchitecture.Core", "ZArchitecture.GUI", "MasterFiles.Business"));
			badReferencePaths.AddRange(FindBadReferencePaths("Enterprise.ZArchitecture.Business", "ZArchitecture.Business", "MasterFiles.Business"));
			badReferencePaths.AddRange(FindBadReferencePaths("Enterprise.ZArchitecture.GUI", "ZArchitecture.GUI", "MasterFiles.Business"));
			AssertEquals("Do not reference MasterFiles.Business from ZArchitecture", "", string.Join("\r\n", badReferencePaths));
		}

		[FrequentlyFailing]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReferencesHaveHintPath()
		{
			var buildXml = BuildXml.CreateFromSourceCodeDirectory();
			CombineAssertions(delegate
			{
				foreach (var solution in buildXml.GetAllSolutionFileNames())
				{
					TestReferencesHaveHintPathInSolution(buildXml, solution);
				}
			});
			Assert(true);
		}

		[ALPOnly]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBusinessProjectsShouldNotReferenceGUIProjects()
		{
			var buildXml = BuildXml.CreateFromSourceCodeDirectory();
			var guiAssemblies = new HashSet<string>
			{
				"System.Windows.Forms",
				"WindowsBase"
			};
			var nonWinzorGuiAssemblies = GetNonWinzorGuiAssemblies();

			var nonGuiAssemblies = new List<string>();
			foreach (var assembly in buildXml.GetAllAssembliesToBuild(deployedToClientsOnly: false))
			{
				if (AssemblyChecker.IsNotTargetPrefix(assembly))
				{
					continue;
				}

				var assemblyName = Path.GetFileNameWithoutExtension(assembly);

				if (File.Exists(Path.Combine(AssemblyLoader.GetBinPath(), "winzor", assemblyName + ".dll")))
				{
					guiAssemblies.Add(assemblyName);
				}
				else if (assemblyName.EndsWith("XmlSerializers") && File.Exists(Path.Combine(AssemblyLoader.GetBinPath(), "winzor", Path.GetFileNameWithoutExtension(assemblyName) + ".dll")))
				{
					guiAssemblies.Add(assemblyName);
				}
				else if (nonWinzorGuiAssemblies.Contains(assemblyName))
				{
					guiAssemblies.Add(assemblyName);
				}
				else
				{
					nonGuiAssemblies.Add(assemblyName);
				}
			}

			var badReferencePaths = new List<string>();
			var baseline = GetBusinessProjectsShouldNotReferenceGUIProjectsBaseline();
			nonGuiAssemblies.Sort();
			foreach (var nonGuiAssembly in nonGuiAssemblies)
			{
				var assemblyPath = GetAssemblyPath(nonGuiAssembly);
				if (File.Exists(assemblyPath))
				{
					using (var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath))
					{
						foreach (var reference in assemblyDefinition.MainModule.AssemblyReferences.Select(reference => reference.Name))
						{
							if (guiAssemblies.Contains(reference))
							{
								if (baseline.Contains(nonGuiAssembly))
								{
									baseline.Remove(nonGuiAssembly);
									break;
								}
								badReferencePaths.Add(nonGuiAssembly + " > " + reference);
							}
						}
					}
				}
			}
			if (badReferencePaths.Count > 0)
			{
				Fail("All GUI projects should have a Winzor equivalent and non-GUI projects should not reference any GUI projects or winforms libraries.\r\nThe following references are invalid:\r\n\r\n" + string.Join("\r\n", badReferencePaths));
			}
			else if (baseline.Count > 0)
			{
				Fail("Remove the following from the BusinessProjectsShouldNotReferenceGUIProjectsBaseline.txt file as they no longer have invalid references:\r\n\r\n" + string.Join("\r\n", baseline));
			}
			else
			{
				Assert(true);
			}
		}

		[ALPOnly]
		public void TestWinzorProjectsDoNotReferenceNonWinzorGuiAssemblies()
		{
			var nonWinzorGuiAssemblies = GetNonWinzorGuiAssemblies();
			var badReferencePaths = new List<string>();

			// Add the runtime DLLs to ignore
			var ignoredRuntimeDlls = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				// Confluent.Kafka -> librdkafka.redist
				"libcrypto-3-x64.dll",
				"libcurl.dll",
				"librdkafka.dll",
				"librdkafkacpp.dll",
				"libssl-3-x64.dll",
				"msvcp140.dll",
				"vcruntime140.dll",
				"zlib1.dll",
				"zstd.dll",

				// Microsoft.Data.SqlClient -> Microsoft.Data.SqlClient.SNI
				"Microsoft.Data.SqlClient.SNI.dll",

				//bblanchon.PDFium.Win32
				"pdfium.dll",

				//System.Data.SqlClient -> runtime.native.System.Data.SqlClient.sni -> runtime.win-x64.runtime.native.System.Data.SqlClient.sni
				"sni.dll",

				// SqlServerSpatial160.dll is a runtime dependency of Microsoft.SqlServer.Types
				"SqlServerSpatial160.dll",
			};
			foreach (var assemblyPath in Directory.GetFiles(Path.Combine(AssemblyLoader.GetBinPath(), "winzor"), "*.dll", SearchOption.TopDirectoryOnly))
			{
				var fileName = Path.GetFileName(assemblyPath);
				if (ignoredRuntimeDlls.Contains(fileName))
				{
					continue;
				}
				try
				{
					using (var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath))
					{
						foreach (var reference in assemblyDefinition.MainModule.AssemblyReferences.Select(reference => reference.Name))
						{
							if (nonWinzorGuiAssemblies.Contains(reference))
							{
								badReferencePaths.Add("winzor\\" + Path.GetFileName(assemblyPath) + " > " + reference);
							}
						}
					}
				}
				catch (BadImageFormatException)
				{
					badReferencePaths.Add("winzor\\" + Path.GetFileName(assemblyPath));
				}
			}
			if (badReferencePaths.Count > 0)
			{
				Fail("Winzor projects should not reference non-Winzor GUI assemblies.\r\nThe following references are invalid:\r\n\r\n" + string.Join("\r\n", badReferencePaths));
			}
			else
			{
				Assert(true);
			}
		}

		HashSet<string> GetNonWinzorGuiAssemblies()
		{
			return new HashSet<string>(new[] {
				"CargoWise.Design",
				"CargoWise.WindowsDesktop",
				"CargoWiseOneX86",
				"cwAlwaysOnSetup",
				"Enterprise.BlazorWinFormsInterop",
				"Enterprise.RemoteDesktopServices.Server",
				"Enterprise.RemoteDesktopServices.Shared",
				"Enterprise.RemoteDesktopServices.Testing",
				"Enterprise.URLHandler",
				"FlexCel.Winforms",
				"OxyPlot.Wpf",
			});
		}

		HashSet<string> GetBusinessProjectsShouldNotReferenceGUIProjectsBaseline()
		{
			var baseline = new HashSet<string>();
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.ReflectionTest.BusinessProjectsShouldNotReferenceGUIProjectsBaseline.txt"))
			using (var reader = new StreamReader(stream))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					baseline.Add(line);
				}
			}
			return baseline;
		}

		[ALPOnly]
		[DatCapabilityRequirement("SOURCE_CODE")]
		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule", Justification = "Testing")]
		public void TestWinzorProjectsReferenceWinzorAssembliesWhenAvailable()
		{
			CombineAssertions(() =>
			{
				var winzorAssemblies = Directory.GetFiles(Path.Combine(AssemblyLoader.GetBinPath(), "winzor"), "*.dll", SearchOption.TopDirectoryOnly)
					.Select(f => Path.GetFileNameWithoutExtension(f)).ToHashSet();
				var winzorProjects = Directory.GetFiles(BaseSourcePath, "*Winzor*.csproj", SearchOption.AllDirectories)
					.Where(f => f.IndexOf(".git", StringComparison.OrdinalIgnoreCase) == -1);
				var referenceDictionary = new ConcurrentDictionary<string, HashSet<string>>();

				Parallel.ForEach(winzorProjects, winzorProject =>
				{
					try
					{
						ParseXml(winzorProject, out var projectElement, out var namespaceManager);

						const string xpathQuery = "//p:Reference/p:HintPath/text()";
						var references = projectElement.SelectNodes(xpathQuery, namespaceManager).Cast<XmlNode>();

						var winzorProjectFolder = Path.GetDirectoryName(winzorProject);
						var commonReferencesPath = Path.Combine(winzorProjectFolder, "Project.CommonReferences.props");

						if (File.Exists(commonReferencesPath))
						{
							ParseXml(commonReferencesPath, out var commonReferencesProjectElement, out var commonReferencesNamespaceManager);

							var commonReferences = commonReferencesProjectElement.SelectNodes(xpathQuery, commonReferencesNamespaceManager).Cast<XmlNode>();

							references = references.Concat(commonReferences).Cast<XmlNode>();
						}

						var referenceSet = references
							.Select(x => x.Value)
							.Where(reference => reference != null && !Path.GetFileName(Path.GetDirectoryName(reference)).Equals("winzor", StringComparison.OrdinalIgnoreCase))
							//exclude references to hard-coded net48 assemblies
							.Where(reference => !reference.StartsWith("$(MSBuildProgramFiles32)"))
							.ToHashSet();

						if (referenceSet.Count != 0)
						{
							referenceDictionary.TryAdd(winzorProject, referenceSet);
						}
					}
					catch (Exception ex)
					{
						throw new Exception($"Failed to analyze {winzorProject}\r\n\r\n{File.ReadAllText(winzorProject)}", ex);
					}
				});

				foreach (var project in referenceDictionary.Keys)
				{
					foreach (var reference in referenceDictionary[project])
					{
						if (winzorAssemblies.Contains(Path.GetFileNameWithoutExtension(reference)))
						{
							Fail($"Project {project} has a reference to {reference}, but a winzor version of the assembly exists. Remove the <HintPath> if the assembly is listed in Directory.CargoWiseReferences.props to allow automatic hint-path resolution. Or change the <HintPath> to point to the bin\\winzor assembly. For more information about automatic hint path resolution, see: https://github.com/WiseTechGlobal/Modernization.Content/blob/main/Controls/populate-reference-hint-path-automatically.md");
						}
					}
				}

				//ensure we have at least one assertion
				Assert(true);
			});
		}

		[ALPOnly]
		[DatCapabilityRequirement("SOURCE_CODE")]
		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule", Justification = "Testing")]
		public void TestWinzorProjectsUsePackageReferenceWhenAvailable()
		{
			CombineAssertions(() =>
			{
				var allPackageReferences = new HashSet<string>();
				try
				{
					var packageVersionFile = Path.Combine(BaseSourcePath, "Directory.Packages.props");
					ParseXml(packageVersionFile, out var xe, out var nm);
					xe.SelectNodes("//p:PackageVersion", nm)
						.Cast<XmlNode>()
						.Select(x => x.Attributes?["Include"]?.InnerText)
						.Where(x => x != null)
						.ForEach(x => allPackageReferences.Add(x));
				}
				catch (Exception ex)
				{
					throw new Exception("Failed to analyze Directory.Packages.props", ex);
				}

				var winzorProjects = Directory.GetFiles(BaseSourcePath, "*Winzor*.csproj", SearchOption.AllDirectories)
					.Where(f => f.IndexOf(".git", StringComparison.OrdinalIgnoreCase) == -1);
				var referenceDictionary = new ConcurrentDictionary<string, HashSet<string>>();
				Parallel.ForEach(winzorProjects, winzorProject =>
				{
					try
					{
						ParseXml(winzorProject, out var projectElement, out var namespaceManager);
						var referenceSet = projectElement.SelectNodes("//p:Reference", namespaceManager)
							.Cast<XmlElement>()
							.Select(x => x.SelectSingleNode("p:HintPath", namespaceManager)?.InnerText)
							.Where(x => x != null)
							.ToHashSet();
						referenceDictionary.TryAdd(winzorProject, referenceSet);
					}
					catch (Exception ex)
					{
						throw new Exception($"Failed to analyze {winzorProject}\r\n\r\n{File.ReadAllText(winzorProject)}", ex);
					}
				});

				foreach (var project in referenceDictionary.Keys)
				{
					foreach (var reference in referenceDictionary[project])
					{
						if (allPackageReferences.Contains(Path.GetFileNameWithoutExtension(reference)))
						{
							Fail($"Project {project} has a reference to {reference}, we should use PackageReference for it to ensure MSBuild works properly.");
						}
						else
						{
							Assert(true);
						}
					}
				}
			});
		}

		static void ParseXml(string filePath, out XmlElement projectElement, out XmlNamespaceManager namespaceManager)
		{
			var projectXml = new XmlDocument();
			projectXml.Load(filePath);
			projectElement = projectXml.DocumentElement;
			namespaceManager = new XmlNamespaceManager(projectXml.NameTable);
			namespaceManager.AddNamespace("p", projectElement.NamespaceURI);
		}

		[ALPOnly]
		public void TestAllWinzorTestProjectsExist()
		{
			var missingDlls = new List<string>();
			foreach (var assemblyPath in Directory.GetFiles(Path.Combine(AssemblyLoader.GetBinPath(), "winzor"), "*.dll", SearchOption.TopDirectoryOnly))
			{
				var assemblyFileName = Path.GetFileName(assemblyPath);
				if (File.Exists(Path.Combine(AssemblyLoader.GetBinPath(), assemblyFileName)))
				{
					var testAssemblyPath = Directory.GetFiles(AssemblyLoader.GetBinPath(), Path.GetFileNameWithoutExtension(assemblyPath) + ".Test*" + Path.GetExtension(assemblyPath)).SingleOrDefault();
					if (testAssemblyPath != null)
					{
						var testAssembylFileName = Path.GetFileName(testAssemblyPath);
						if (!File.Exists(Path.Combine(AssemblyLoader.GetBinPath(), "winzor", testAssembylFileName)))
						{
							missingDlls.Add(testAssembylFileName);
						}
					}
				}
			}
			if (missingDlls.Count > 0)
			{
				Fail("Winzor test projects should exist for each relevant CW1 test project.\r\nThe following winzor DLLs are missing:\r\n\r\n" + string.Join("\r\n", missingDlls));
			}
			else
			{
				Assert(true);
			}
		}

		IEnumerable<string> FindBadReferencePaths(string assemblyName, string kindOfAssembly, string shouldNotReferenceKindOfAssembly, bool analyzeSystemAssemblies = false)
		{
			var key = new Tuple<string, string, bool>(assemblyName, shouldNotReferenceKindOfAssembly, analyzeSystemAssemblies);
			if (!badReferencePathsCache.TryGetValue(key, out List<string> badReferencePaths))
			{
				badReferencePaths = new List<string>();
				var assemblyPath = GetAssemblyPath(assemblyName);
				if (File.Exists(assemblyPath))
				{
					using (var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath))
					{
						foreach (var reference in assemblyDefinition.MainModule.AssemblyReferences.Select(reference => reference.Name).Where(name => analyzeSystemAssemblies || !IsSystemAssembly(name)))
						{
							if (!reference.EndsWith(kindOfAssembly, StringComparison.OrdinalIgnoreCase) && !reference.EndsWith(".Testing", StringComparison.OrdinalIgnoreCase))
							{
								if (reference.EndsWith(shouldNotReferenceKindOfAssembly, StringComparison.OrdinalIgnoreCase))
								{
									badReferencePaths.Add(assemblyName + " > " + reference);
								}
								else
								{
									// Should pass analyzeSystemAssemblies recursivly here but adding it now will result in many failures
									// Temporarily adding analyzeSystemAssemblies to the cache key to verify items explicity added to the TestBusinessProjectsShouldNotReferenceWinformsLibraries list
									badReferencePaths.AddRange(FindBadReferencePaths(reference, kindOfAssembly, shouldNotReferenceKindOfAssembly).Select(item => assemblyName + " > " + item));
								}
							}
						}
					}
				}
				badReferencePathsCache.Add(key, badReferencePaths);
			}
			return badReferencePaths;
		}

		readonly Dictionary<Tuple<string, string, bool>, List<string>> badReferencePathsCache = new Dictionary<Tuple<string, string, bool>, List<string>>();

		[ALPOnly]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWinzorImplementationsDoNotReferenceCWAssemblies()
		{
			var buildXml = BuildXml.CreateFromSourceCodeDirectory();
			var cwAssemblies = buildXml.GetAllAssembliesToBuild(deployedToClientsOnly: false).Select(assemblyPath => Path.GetFileNameWithoutExtension(assemblyPath));
			var winzorImplementations = new string[]
			{
				"WinzorFramework",
				"WinzorFramework.Test",
			};

			var badReferencePaths = new List<string>();
			foreach (var assemblyName in winzorImplementations)
			{
				using (var assemblyDefinition = AssemblyDefinition.ReadAssembly(Path.Combine(AssemblyLoader.GetBinPath(), "winzor", assemblyName + ".dll")))
				{
					foreach (var reference in assemblyDefinition.MainModule.AssemblyReferences.Select(reference => reference.Name))
					{
						if (cwAssemblies.Contains(reference))
						{
							badReferencePaths.Add(assemblyName + " > " + reference);
						}
					}
				}
			}
			if (badReferencePaths.Count > 0)
			{
				Fail("Winzor implementations should not reference CW assemblies.\r\nThe following references are invalid:\r\n\r\n" + string.Join("\r\n", badReferencePaths));
			}
			else
			{
				Assert(true);
			}
		}

		[ALPOnly]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoUnusedWinformsDirectives()
		{
			var buildXml = BuildXml.CreateFromSourceCodeDirectory();
			var guiAssemblies = new HashSet<string>();
			guiAssemblies.Add("System.Windows.Forms");
			foreach (var assembly in buildXml.GetAllAssembliesToBuild(deployedToClientsOnly: true))
			{
				var assemblyName = Path.GetFileNameWithoutExtension(assembly);
				if (File.Exists(Path.Combine(AssemblyLoader.GetBinPath(), "winzor", assemblyName + ".dll")))
				{
					guiAssemblies.Add(assemblyName);
				}
			}

			CombineAssertions(() =>
			{
				foreach (var solution in buildXml.GetAllSolutionFileNames())
				{
					var solutionAssemblies = buildXml.GetAllAssembliesInSolution(solution);
					var solutionPath = Path.Combine(BaseSourcePath, solution);
					foreach (var projectPath in GetProjectsFromSolution(solutionPath))
					{
						if (!projectPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) || projectPath.IndexOf("Winzor", StringComparison.Ordinal) > -1)
						{
							continue;
						}

						var projectXml = new XmlDocument();
						projectXml.Load(projectPath);
						var projectElement = projectXml.DocumentElement;
						var namespaceManager = new XmlNamespaceManager(projectXml.NameTable);
						namespaceManager.AddNamespace("p", projectElement.NamespaceURI);
						var isLibrary = true;
						var outputType = projectElement.SelectSingleNode("//p:OutputType", namespaceManager);
						if (outputType != null)
						{
							isLibrary = string.Equals(outputType.InnerText, "Library", StringComparison.OrdinalIgnoreCase);
						}
						var assemblyName = projectElement.SelectSingleNode("//p:AssemblyName", namespaceManager)?.InnerText;
						assemblyName ??= Path.GetFileNameWithoutExtension(projectPath);
						var outputPath = projectElement.SelectSingleNode("//p:OutputPath", namespaceManager)?.InnerText;
						if (!string.IsNullOrEmpty(assemblyName)
							&& solutionAssemblies.Contains(assemblyName + (isLibrary ? ".dll" : ".exe"), StringComparer.OrdinalIgnoreCase))
						{
							var useWindowsFormsElement = projectElement.SelectSingleNode("//p:UseWindowsForms", namespaceManager);
							var importWindowsDesktopTargets = projectElement.SelectSingleNode("//p:ImportWindowsDesktopTargets", namespaceManager);
							if ((useWindowsFormsElement != null && useWindowsFormsElement.InnerText.Equals("true", StringComparison.OrdinalIgnoreCase))
								|| (importWindowsDesktopTargets != null && importWindowsDesktopTargets.InnerText.Equals("true", StringComparison.OrdinalIgnoreCase)))
							{
								var assemblyPath = GetAssemblyPath(assemblyName);
								using (var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath))
								{
									if (!assemblyDefinition.MainModule.AssemblyReferences.Any(reference => guiAssemblies.Contains(reference.Name)))
									{
										Fail(string.Format("Project {0} specifies UseWindowsForms and/or ImportWindowsDesktopTargets but does not contain any WinForms code", projectPath));
									}
								}
							}
						}
					}
				}
			});

			Assert(true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoUnusedNUnitReferencesInSafegaurd()
		{
			CombineAssertions(() =>
			{
				foreach (var line in File.ReadAllLines(Path.Combine(BaseSourcePath, "ProjectSafeguards", "NUnitSafeguardProjects.txt")))
				{
					if (string.IsNullOrWhiteSpace(line) || line.StartsWith("🚨") || line.StartsWith("DON'T REMOVE"))
					{
						continue;
					}

					var assemblyPath = Path.Combine(AssemblyLoader.GetBinPath(), line.Trim());
					if (File.Exists(assemblyPath + ".dll"))
					{
						assemblyPath = assemblyPath + ".dll";
					}
					else if (File.Exists(assemblyPath + ".exe"))
					{
						assemblyPath = assemblyPath + ".exe";
					}
					else
					{
						continue;
					}

					using (var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath))
					{
						if (!assemblyDefinition.MainModule.AssemblyReferences.Any(reference => reference.Name == "NUnitCore"))
						{
							Fail($"{assemblyPath} does not use NUnitCore, remove the reference from the csproj and remove the item from NUnitSafeguardProjects.txt");
						}
					}
				}
			});

			Assert(true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNonNGenedAssembliesAreNotReferencedByNGenedAssmblies()
		{
			var ngned = new Dictionary<string, bool>();
			var buildXml = BuildXml.CreateFromSourceCodeDirectory();

			foreach (var assemblyFileName in buildXml.GetAllAssembliesDeployedToClient(AssemblyLoader.GetBinPath()).Where(IsAssembly))
			{
				if (AssemblyChecker.IsNotTargetPrefix(assemblyFileName))
				{
					continue;
				}

				var assemblyName = Path.GetFileNameWithoutExtension(assemblyFileName);
				if (!ngned.ContainsKey(assemblyName))
				{
					ngned.Add(assemblyName, buildXml.NGen(assemblyFileName));
				}
			}

			CombineAssertions(() =>
			{
				foreach (var solution in buildXml.GetAllSolutionFileNames())
				{
					foreach (var assembly in buildXml.GetAllAssembliesInSolution(solution).Where(IsAssembly))
					{
						if (AssemblyChecker.IsNotTargetPrefix(assembly))
						{
							continue;
						}

						string assemblyName = Path.GetFileNameWithoutExtension(assembly);
						if (!ngned.ContainsKey(assemblyName) || !ngned[assemblyName])
						{
							continue;
						}

						var assemblyPath = Path.Combine(AssemblyLoader.GetBinPath(), assembly);
						using var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath);

						foreach (var reference in assemblyDefinition.MainModule.AssemblyReferences.Select(reference => reference.Name).Where(name => !IsSystemAssembly(name)))
						{
							if (!ngned.TryGetValue(reference, out bool isngened))
							{
								continue;
							}

							if (
								(assemblyName == "Enterprise.ServiceManager.Tasks.OnlineDataTransformation" && reference == "Enterprise.DbUpgrader.Transformation.Common") ||
								(assemblyName == "Enterprise.ServiceManager.Tasks.OnlineDataTransformation" && reference == "Enterprise.DbUpgrader.Transformations") ||
								(assemblyName == "CargoWise.Main" && reference == "Enterprise.DbUpgrader.ReferenceDatabases") ||
								(assemblyName == "CargoWise.Main" && reference == "Enterprise.DbUpgrader.Transformation.Common")
							)
							{
								// these are excluded because they are not on a hot code path, so don't need ngen
							}
							else
							{
								Assert(assemblyName + " should not reference " + reference, isngened || IsDebugOnlyReference(assemblyName, reference, solution));
							}
						}
					}
				}
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoProjectsHaveAssemblyReferencesForLocalProjects()
		{
			var buildXml = BuildXml.CreateFromSourceCodeDirectory();
			CombineAssertions(() =>
			{
				var lockObj = new object();

				var listOfFailedAssemblies = new List<string>();
				Parallel.ForEach(buildXml.GetAllSolutionFileNames(), solution =>
				{
					var solutionAssemblies = buildXml.GetAllAssembliesInSolution(solution).ConvertAll(x => x.ToUpperInvariant());
					var solutionPath = Path.Combine(BaseSourcePath, solution);
					foreach (var projectPath in GetProjectsFromSolution(solutionPath).Where(x => x.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) && !x.Contains(@".Winzor.csproj")))
					{
						var projectXml = new XmlDocument();
						projectXml.Load(projectPath);
						var projectElement = projectXml.DocumentElement;
						var namespaceManager = new XmlNamespaceManager(projectXml.NameTable);
						namespaceManager.AddNamespace("p", projectElement.NamespaceURI);
						var projectReferences = projectElement
							.SelectNodes("//p:Reference", namespaceManager)
							.Cast<XmlElement>()
							.Select(x => x.SelectSingleNode("p:HintPath", namespaceManager)?.InnerText)
							.Where(x => x != null)
							.Select(x => Path.GetFileName(x).ToUpperInvariant())
							.ToList();

						lock (lockObj)
						{
							var projectHasAssemblyDependencyForLocalSolutionProject =
								(projectReferences.Intersect(solutionAssemblies)
									//exclude SpecialFileHandling - it is not build so it has to be binary reference for SDK style projects
									.Any
									(
										item => !string.Equals
										(
											item, "Enterprise.Dat.SpecialFileHandling.dll", StringComparison.OrdinalIgnoreCase
										)
									)
								);
							if (projectHasAssemblyDependencyForLocalSolutionProject)
							{
								listOfFailedAssemblies.Add($"{Path.GetFileName(projectPath)}");
							}
						}
					}
				});

				AssertEquals($"The following projects use assembly dependencies for solution projects. Please change to project dependencies.\r\n{string.Join(System.Environment.NewLine, listOfFailedAssemblies)}",
					true, listOfFailedAssemblies.IsNullOrEmpty());
			});
		}

		#region TestNoUnusedReferences

		void TestNoUnusedReferencesInSolution(BuildXml buildXml, string solution)
		{
			var solutionAssemblies = buildXml.GetAllAssembliesInSolution(solution);
			string solutionPath = Path.Combine(BaseSourcePath, solution);

			foreach (var projectPath in GetProjectsFromSolution(solutionPath))
			{
				TestNoUnusedReferencesInProject(buildXml, projectPath, solutionAssemblies);
			}
		}

		void TestNoUnusedReferencesInProject(BuildXml buildXml, string projectPath, List<string> solutionAssemblies)
		{
			if (!projectPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) ||
				projectPath.IndexOf("Winzor", StringComparison.Ordinal) > -1)
			{
				return;
			}

			var projectXml = new XmlDocument();
			projectXml.Load(projectPath);
			var projectElement = projectXml.DocumentElement;
			var namespaceManager = new XmlNamespaceManager(projectXml.NameTable);
			namespaceManager.AddNamespace("p", projectElement.NamespaceURI);
			var isLibrary = true;
			var outputType = projectElement.SelectSingleNode("//p:OutputType", namespaceManager);
			if (outputType != null)
			{
				isLibrary = string.Equals(outputType.InnerText, "Library", StringComparison.OrdinalIgnoreCase);
			}
			var assemblyName = projectElement.SelectSingleNode("//p:AssemblyName", namespaceManager)?.InnerText;
			assemblyName ??= Path.GetFileNameWithoutExtension(projectPath);
			var outputPath = projectElement.SelectSingleNode("//p:OutputPath", namespaceManager)?.InnerText;
			if (!string.IsNullOrEmpty(assemblyName)
				&& solutionAssemblies.Contains(assemblyName + (isLibrary ? ".dll" : ".exe"), StringComparer.OrdinalIgnoreCase))
			{
				var otherBuildRequiredTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

				foreach (XmlElement element in projectElement.SelectNodes("//p:UsingTask", namespaceManager))
				{
					var usingTaskAssembly = element.GetAttribute("AssemblyFile");
					if (!string.IsNullOrEmpty(usingTaskAssembly) && usingTaskAssembly.StartsWith("$(TargetDir)") && !usingTaskAssembly.StartsWith(@"$(TargetDir)..\"))
					{
						otherBuildRequiredTypes.Add(Path.GetFileNameWithoutExtension(usingTaskAssembly.Substring("$(TargetDir)".Length)));
					}
				}

				if (!string.IsNullOrEmpty(outputPath))
				{
					foreach (XmlElement element in projectElement.SelectNodes("//p:EmbeddedResource", namespaceManager))
					{
						var inlcudePath = element.GetAttribute("Include");
						if (!string.IsNullOrEmpty(inlcudePath) && inlcudePath.StartsWith(outputPath, StringComparison.OrdinalIgnoreCase))
						{
							otherBuildRequiredTypes.Add(Path.GetFileNameWithoutExtension(inlcudePath.Substring(outputPath.Length)));
						}
					}
				}

				foreach (XmlElement element in projectElement.SelectNodes("//p:PostBuildEvent", namespaceManager))
				{
					var postBuildEventCommand = element.InnerText.Split(' ')[0];
					if (!string.IsNullOrEmpty(postBuildEventCommand) && postBuildEventCommand.StartsWith("$(TargetDir)"))
					{
						otherBuildRequiredTypes.Add(Path.GetFileNameWithoutExtension(postBuildEventCommand.Substring("$(TargetDir)".Length)));
					}
				}

				var referenced = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				foreach (XmlElement element in projectElement.SelectNodes("//p:Reference", namespaceManager))
				{
					var referenceAssemblyName = element.GetAttribute("Include");
					if (!string.IsNullOrEmpty(referenceAssemblyName))
					{
						referenceAssemblyName = referenceAssemblyName.Split(',')[0];
						if (!otherBuildRequiredTypes.Contains(referenceAssemblyName) && !IsValidReference(assemblyName, referenceAssemblyName))
						{
							Fail(string.Format("Unused reference to {0} in {1}", referenceAssemblyName, projectPath));
						}
						referenced.Add(referenceAssemblyName);
					}
				}

				foreach (XmlElement element in projectElement.SelectNodes("//p:ProjectReference", namespaceManager))
				{
					var referenceProjectPath = Path.Combine(Path.GetDirectoryName(projectPath), element.GetAttribute("Include"));
					var referenceAssemblyName = GetProjectAssemblyName(referenceProjectPath);
					if (!otherBuildRequiredTypes.Contains(referenceAssemblyName) && !IsValidReference(assemblyName, referenceAssemblyName))
					{
						Fail(string.Format("Unused reference to {0} in {1}", Path.GetFileName(referenceProjectPath), projectPath));
					}
					referenced.Add(referenceAssemblyName);
				}

				foreach (var otherRequiredType in otherBuildRequiredTypes)
				{
					if (!referenced.Contains(otherRequiredType) && !buildXml.GetOtherDeployedFilesWithCopyFrom(BaseSourcePath).Keys.Select(f => Path.GetFileNameWithoutExtension(f)).Contains(otherRequiredType))
					{
						Fail(string.Format("File {0} is used in the build process for {1} but not referenced, can result in incorrect build order", otherRequiredType, projectPath));
					}
				}
			}
		}

		string GetProjectAssemblyName(string projectPath)
		{
			var projectXml = new XmlDocument();
			projectXml.Load(projectPath);
			var projectElement = projectXml.DocumentElement;
			var namespaceManager = new XmlNamespaceManager(projectXml.NameTable);
			namespaceManager.AddNamespace("p", projectElement.NamespaceURI);
			var assemblyName = projectElement.SelectSingleNode("//p:AssemblyName", namespaceManager)?.InnerText;
			assemblyName ??= Path.GetFileNameWithoutExtension(projectPath);
			return assemblyName;
		}

		bool IsValidReference(string assemblyName, string assemblyReference)
		{
			return IsSystemAssembly(assemblyReference) || GetRealAssemblyReferences(assemblyName).Contains(assemblyReference);
		}

		HashSet<string> GetRealAssemblyReferences(string assemblyName)
		{
			if (allRealAssemblyReferences.TryGetValue(assemblyName, out HashSet<string> realAssemblyReferences))
			{
				return realAssemblyReferences;
			}

			realAssemblyReferences = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			var assembliesPath = GetAssemblyPaths(assemblyName);

			foreach (var assemblyPath in assembliesPath)
			{
				if (File.Exists(assemblyPath))
				{
#pragma warning disable CS0436 // Type conflicts with imported type
					var isNetCoreAssemblyPath = assemblyPath.Contains(CommonAssemblyInfo.CWNetCoreSubfolder);
#pragma warning restore CS0436 // Type conflicts with imported type

					var assembly = isNetCoreAssemblyPath ? AssembliesContext.NetCoreInstance.GetAssembly(assemblyName) : AssembliesContext.Instance.GetAssembly(assemblyName);

					if (!isNetCoreAssemblyPath)
					{
						foreach
						(
							CustomAttributeData usesConstantsAttribute in assembly.CustomAttributes
								.Where(attribute => attribute.AttributeType.FullName == typeof(UsesConstantsAttribute).FullName)
						)
						{
							string assemblyFullName = ((Type)usesConstantsAttribute.ConstructorArguments[0].Value).Assembly.FullName;
							string assemblySimpleName = assemblyFullName.Substring(0, assemblyFullName.IndexOf(','));

							if (!assemblySimpleName.Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
							{
								realAssemblyReferences.Add(assemblySimpleName);
							}
						}
					}

					foreach
					(
						string assemblySimpleName in assembly.GetReferencedAssemblies()
							.Select(reference => reference.Name)
							.Where(name => !IsSystemAssembly(name))
					)
					{
						realAssemblyReferences.Add(assemblySimpleName);
					}

					foreach (var realReference in realAssemblyReferences.ToArray())
					{
						foreach (var childReference in GetRealAssemblyReferences(realReference))
						{
							realAssemblyReferences.Add(childReference);
						}
					}
				}
			}

			allRealAssemblyReferences.Add(assemblyName, realAssemblyReferences);

			return realAssemblyReferences;
		}

		readonly Dictionary<string, HashSet<string>> allRealAssemblyReferences = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
		#endregion

		#region TestNoDisallowedReferences

		void AssertNoDisallowedReferencesInAssembly(string assemblyName, string solutionName)
		{
			var assemblyPath = GetAssemblyPath(assemblyName);
			using (var assembly = AssemblyDefinition.ReadAssembly(assemblyPath))
			{
				var references = assembly.MainModule.AssemblyReferences.Where(r => !IsSystemAssembly(r.Name));

				foreach (var reference in references)
				{
					var attribute = GetPreventedReferenceAttribute(reference.Name);
					if (attribute != null)
					{
						var isAllowedReference = attribute.AllowedReferencePartialPaths != null
							&& attribute.AllowedReferencePartialPaths.Any(path => assemblyName.Contains(path));

						if (!isAllowedReference)
						{
							var isAllowedDebugOnlyReference = attribute.AllowedDebugOnlyReferencePartialPaths != null && attribute.AllowedDebugOnlyReferencePartialPaths.Any(path => assemblyName.Contains(path));
							if (isAllowedDebugOnlyReference)
							{
								var isDebugOnlyReference = IsDebugOnlyReference(assemblyName, reference.Name, solutionName);
								if (!isDebugOnlyReference)
								{
									Fail(string.Format("Assembly [{0}] must not have a reference to [{1}] outside of DEBUG configuration. The Reference element in the csproj file should have the following attribute:\r\n\r\nCondition=\"'$(Configuration)'=='Debug'\"", assemblyName, reference.Name));
								}
							}
							else
							{
								Fail(string.Format("Assembly [{0}] must not have a reference to [{1}]", assemblyName, reference.Name));
							}
						}
					}
				}
			}
		}

		bool IsDebugOnlyReference(string assemblyName, string referenceName, string solutionName)
		{
			var csprojPath = GetCsprojFilePath(assemblyName, solutionName);
			var commonRefsFile = Path.Combine(Path.GetDirectoryName(csprojPath) ?? string.Empty, "Project.CommonReferences.props");

			var regex = new Regex($@"Reference Include=""{referenceName}.*Condition=.*'Debug'", RegexOptions.IgnoreCase);

			foreach (var fileName in new[] { csprojPath, commonRefsFile })
			{
				if (string.IsNullOrEmpty(fileName))
				{
					continue;
				}
				var file = File.ReadAllLines(fileName);
				if (file.Any(l => regex.IsMatch(l)))
				{
					return true;
				}
			}

			return false;
		}

		PreventAssemblyReferencesAttribute GetPreventedReferenceAttribute(string assemblyName)
		{
			if (!preventedReferencesByAssembly.ContainsKey(assemblyName))
			{
				PreventAssemblyReferencesAttribute attribute = null;
				var assemblyPath = GetAssemblyPath(assemblyName);

				if (File.Exists(assemblyPath))
				{
					var assembly = Assembly.LoadFrom(assemblyPath);
					attribute = assembly.GetCustomAttribute<PreventAssemblyReferencesAttribute>();
				}

				preventedReferencesByAssembly[assemblyName] = attribute;
			}

			return preventedReferencesByAssembly[assemblyName];
		}

		readonly Dictionary<string, PreventAssemblyReferencesAttribute> preventedReferencesByAssembly = new Dictionary<string, PreventAssemblyReferencesAttribute>();

		#endregion

		#region TestReferencesHaveHintPath

		void TestReferencesHaveHintPathInSolution(BuildXml buildXml, string solution)
		{
			var solutionAssemblies = buildXml.GetAllAssembliesInSolution(solution);
			string solutionPath = Path.Combine(BaseSourcePath, solution);
			using (var reader = File.OpenText(solutionPath))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (line.StartsWith("Project"))
					{
						string projectPath = line.Split(',')[1].Trim(' ', '\t', '"');
						TestReferencesHaveHintPathInProject(Path.Combine(Path.GetDirectoryName(solutionPath), projectPath), solutionAssemblies);
					}
				}
			}
		}

		void TestReferencesHaveHintPathInProject(string projectPath, List<string> solutionAssemblies)
		{
			if (projectPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
			{
				var projectXml = new XmlDocument();
				projectXml.Load(projectPath);
				var projectElement = projectXml.DocumentElement;
				var namespaceManager = new XmlNamespaceManager(projectXml.NameTable);
				namespaceManager.AddNamespace("p", projectElement.NamespaceURI);
				var isLibrary = false;
				var outputType = projectElement.SelectSingleNode("//p:OutputType", namespaceManager);
				if (!string.IsNullOrEmpty(outputType?.InnerText))
				{
					isLibrary = string.Equals(projectElement.SelectSingleNode("//p:OutputType", namespaceManager).InnerText, "Library", StringComparison.OrdinalIgnoreCase);
				}
				var assemblyName = projectElement.SelectSingleNode("//p:AssemblyName", namespaceManager)?.InnerText;
				if (!string.IsNullOrEmpty(assemblyName)
					&& solutionAssemblies.Contains(assemblyName + (isLibrary ? ".dll" : ".exe"), StringComparer.OrdinalIgnoreCase))
				{
					foreach (XmlElement element in projectElement.SelectNodes("//p:Reference", namespaceManager))
					{
						string referenceAssemblyName = element.GetAttribute("Include");
						if (string.IsNullOrEmpty(referenceAssemblyName))
						{
							continue;
						}

						var hintPath = element.SelectSingleNode("p:HintPath", namespaceManager)?.InnerText;
						if (hintPath == null)
						{
							referenceAssemblyName = referenceAssemblyName.Split(',')[0];
							if (!IsAssemblyToIgnoreForHintPathTest(referenceAssemblyName))
							{
								Fail(string.Format("Reference to {0} in {1} has no hint path. Please add one.", referenceAssemblyName, projectPath));
							}
						}
						else if (!Path.IsPathRooted(hintPath) && !Path.GetFullPath(Path.Combine(Path.GetDirectoryName(projectPath), hintPath)).StartsWith(BaseSourcePath, StringComparison.OrdinalIgnoreCase))
						{
							Fail(string.Format("Reference to {0} in {1} has invalid hint path. HintPath should point to a file within the current repository.", referenceAssemblyName, projectPath));
						}
					}
				}
			}
		}

		#endregion

		#region TestSGenReferences

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSGenReferences()
		{
			var buildXml = BuildXml.CreateFromSourceCodeDirectory();
			CombineAssertions("References used in the SGen command should exist in the csproj’s Reference List. The unnecessary Sgen reference should be removed.", () =>
			{
				Parallel.ForEach(buildXml.GetAllSolutionFileNames(), new ParallelOptions() { MaxDegreeOfParallelism = 2 }, (solution) =>
				{
					TestSGenReferencesInSolution(solution);
				});
			});
			Assert(true);
		}

		void TestSGenReferencesInSolution(string solution)
		{
			string solutionPath = Path.Combine(BaseSourcePath, solution);
			foreach (var projectPath in GetProjectsFromSolution(solutionPath).Where(p => p.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)))
			{
				TestSGenReferencesInProject(projectPath);
			}
		}

		void TestSGenReferencesInProject(string projectPath)
		{
			var projectXml = new XmlDocument();
			projectXml.Load(projectPath);
			var projectElement = projectXml.DocumentElement;
			var namespaceManager = new XmlNamespaceManager(projectXml.NameTable);
			namespaceManager.AddNamespace("p", projectElement.NamespaceURI);
			var commaSeparator = new[] { ',' };
			var spaceSeparator = new[] { ' ', '\t' };
			var sGenDlls = new HashSet<string>();

			foreach (XmlElement element in projectElement.SelectNodes("//p:PostBuildEvent", namespaceManager))
			{
				var sGenCommandParts = element.InnerText.ToLower().Split(spaceSeparator, StringSplitOptions.RemoveEmptyEntries);
				if (sGenCommandParts.Length == 0 || !sGenCommandParts[0].ToLower().Contains("sgen.exe"))
				{
					continue;
				}

				foreach (var sGenCommandPart in sGenCommandParts)
				{
					if (sGenCommandPart.StartsWith("/compiler:/reference:"))
					{
						sGenDlls.Add(sGenCommandPart.Replace("/compiler:/reference:$(targetdir)", "").Replace(".dll", "").Trim());
					}
				}
			}

			if (sGenDlls.Any())
			{
				var referenceDlls = new HashSet<string>();
				foreach (XmlElement element in projectElement.SelectNodes("//p:Reference", namespaceManager))
				{
					var referenceAssemblyName = element.GetAttribute("Include")?.Split(commaSeparator, 2)[0];
					if (string.IsNullOrEmpty(referenceAssemblyName))
					{
						continue;
					}

					referenceDlls.Add(referenceAssemblyName.ToLower());
				}

				var unnecessarySgenDlls = sGenDlls.Except(referenceDlls);
				if (unnecessarySgenDlls.Any())
				{
					Fail("Project: " + projectPath + " \r\nReference(s): " + string.Join(", ", unnecessarySgenDlls) + "\r\n");
				}
			}
		}

		#endregion

		#region Implementation

		static bool IsAssembly(string assemblyName)
		{
			return assemblyName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || assemblyName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);
		}

		static string GetAssemblyPath(string assemblyName)
		{
			var assemblyPath = Path.Combine(AssemblyLoader.GetBinPath(), assemblyName);
			if (File.Exists(assemblyPath + ".dll"))
			{
				assemblyPath += ".dll";
			}
			else if (File.Exists(assemblyPath + ".exe"))
			{
				assemblyPath += ".exe";
			}

			return assemblyPath;
		}

		static IEnumerable<string> GetAssemblyPaths(string assemblyName)
		{
#pragma warning disable CS0436 // Type conflicts with imported type
			IEnumerable<string> assemblyPaths = new[]
			{
				Path.Combine(AssemblyLoader.GetBinPath(), assemblyName),
				Path.Combine(AssemblyLoader.GetBinPath(), CommonAssemblyInfo.CWNetCoreSubfolder, assemblyName)
			};
#pragma warning restore CS0436 // Type conflicts with imported type

			List<string> assemblyPathsResults = [];

			foreach (var assemblyPath in assemblyPaths)
			{
				if (File.Exists(assemblyPath + ".dll"))
				{
					assemblyPathsResults.Add($"{assemblyPath}.dll");
				}
				else if (File.Exists(assemblyPath + ".exe"))
				{
					assemblyPathsResults.Add($"{assemblyPath}.exe");
				}
			}

			return assemblyPathsResults;
		}

		static string GetCsprojFilePath(string assemblyName, string solutionFileName)
		{
			if (assemblyName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || assemblyName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
			{
				assemblyName = assemblyName.Substring(0, assemblyName.Length - 4);
			}

			var solutionDirectory = Path.GetDirectoryName(Path.Combine(BaseSourcePath, solutionFileName));
			var csprojFiles = new DirectoryInfo(solutionDirectory).GetFiles("*.csproj", SearchOption.AllDirectories);

			foreach (var file in csprojFiles)
			{
				var doc = XDocument.Load(file.FullName);
				var assemblyNameElement = doc.DescendantNodes().OfType<XElement>().FirstOrDefault(n => n.Name.LocalName == "AssemblyName");

				if (assemblyNameElement != null)
				{
					if (string.Equals(assemblyNameElement.Value, assemblyName, StringComparison.OrdinalIgnoreCase))
					{
						return file.FullName;
					}
				}
				// if no AssemblyName the default is project name
				else if (string.Equals(Path.GetFileNameWithoutExtension(file.Name), assemblyName, StringComparison.OrdinalIgnoreCase))
				{
					return file.FullName;
				}
			}

			return string.Empty;
		}

		bool IsAssemblyToIgnoreForHintPathTest(string assemblyName)
		{
			return
				(IsSystemAssembly(assemblyName) || IsOtherThirdPartyAssemblyToIgnore(assemblyName)) && !AssemblyExistsInBinPath(assemblyName)
				|| IsHintAutoPopulated(assemblyName);
		}

		bool IsSystemAssembly(string assemblyName)
		{
			return systemAssemblyNamePrefixes.Any(prefix => assemblyName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
		}

		bool IsOtherThirdPartyAssemblyToIgnore(string assemblyName)
		{
			return thirdPartyAssembliesToIgnoreNamePrefixes.Any(prefix => assemblyName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
		}

		bool AssemblyExistsInBinPath(string assemblyName)
		{
			return File.Exists(Path.Combine(AssemblyLoader.GetBinPath(), assemblyName + ".dll")) || File.Exists(Path.Combine(AssemblyLoader.GetBinPath(), assemblyName + ".exe"));
		}

		bool IsHintAutoPopulated(string assemblyName) =>
			AutoPopulatedHintPathProjects.Value.Contains(assemblyName.ToLower());

		readonly string[] systemAssemblyNamePrefixes = new[] { "System", "Microsoft", "Presentation", "Windows", "WinForms", "mscorlib", "EnvDTE", "EnvDTE80", "UIAutomationProvider", "Accessibility", "CustomMarshalers", "Azure" };
		readonly string[] thirdPartyAssembliesToIgnoreNamePrefixes = new[] { "HHP.DataCollection.Common", "Intermec.DataCollection.CF2", "PsionTeklogixNet", "HandHeldProducts.Embedded.Decoding.DecodeAssembly", "HHP.DataCollection.PDTDecoding" };

		const string AutoHintPropsFilename = "Directory.CargoWiseReferences.props";

		readonly Lazy<HashSet<string>> AutoPopulatedHintPathProjects =
			new Lazy<HashSet<string>>(() =>
				XElement.Load(Path.Combine(BaseSourcePath, AutoHintPropsFilename))
					.Descendants("CWReferenceStatus")
					.Select(e => e.Attribute("Include")?.Value.ToLower())
					.Where(s => !string.IsNullOrEmpty(s))
					.ToHashSet());

		Dictionary<string, string> GetAssemblyProjectLookup(BuildXml buildXml)
		{
			var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			Parallel.ForEach(buildXml.GetAllSolutionFileNames(), new ParallelOptions() { MaxDegreeOfParallelism = 2 }, (solution) =>
			{
				var solutionPath = Path.Combine(BaseSourcePath, solution);
				foreach (var projectPath in GetProjectsFromSolution(solutionPath))
				{
					if (!projectPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}

					var projectXml = XDocument.Load(projectPath);
					var nameElement = projectXml.DescendantNodes().OfType<XElement>().FirstOrDefault(n => n.Name.LocalName == "AssemblyName");
					var nameProperty = Path.GetFileNameWithoutExtension(projectPath);
					if (nameElement != null)
					{
						nameProperty = nameElement.Value;
					}
					if (!string.IsNullOrEmpty(nameProperty))
					{
						lookup[nameProperty] = projectPath;
					}
				}
			});

			return lookup;
		}

		static IEnumerable<string> GetProjectsFromSolution(string solutionPath)
		{
			using (var reader = File.OpenText(solutionPath))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (line.StartsWith("Project"))
					{
						string projectPath = line.Split(',')[1].Trim(' ', '\t', '"');
						yield return Path.Combine(Path.GetDirectoryName(solutionPath), projectPath);
					}
				}
			}
		}

		#endregion
	}
}
