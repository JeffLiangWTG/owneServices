using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.BuildTools;
using CargoWise.Common;
using NUnit.Framework;

namespace Enterprise.ReflectionTest
{
	[FrequentlyFailing]
	sealed class DeadCodeTest : TestCase
	{
		Lazy<string[]> allSourceFiles;
		Lazy<string[]> allProjectFiles;
		Lazy<string[]> allSolutionFiles;
		Lazy<string[]> allSolutionFilterFiles;

		readonly (string Solution, string Project)[] externalProjectsExceptions =
		[
			// PL.All.sln os development-only solution that combines related projects together.
			// It has a references to project from other repository, that makes it easier to work with them.
			("PL.All.sln", "CargoWise.Customs.PL.MessageContracts.csproj"),
			("PL.All.sln", "CargoWise.Customs.PL.MessageContracts.Test.csproj"),
			("PL.All.sln", "CargoWise.Customs.PL.MessageDefinitions.csproj"),
			("PL.All.sln", "CargoWise.Customs.PL.MessageDefinitions.Test.csproj"),
		];

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Unit tests require source code")]
		protected override void SetUp()
		{
			base.SetUp();
			allSourceFiles = new Lazy<string[]>(() =>
			{
				try
				{
					return GetAllFiles(new DirectoryInfo(BaseSourcePath));
				}
				catch (AggregateException ex)
				{
					throw ex.Flatten();
				}
			});
			allProjectFiles = new Lazy<string[]>(() => allSourceFiles.Value.Where(s => s.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) || s.EndsWith(".sqlproj", StringComparison.OrdinalIgnoreCase)).ToArray());
			allSolutionFiles = new Lazy<string[]>(() => allSourceFiles.Value.Where(s => s.EndsWith(".sln", StringComparison.OrdinalIgnoreCase)).ToArray());
			allSolutionFilterFiles = new Lazy<string[]>(() => allSourceFiles.Value.Where(s => s.EndsWith(".slnf", StringComparison.OrdinalIgnoreCase)).ToArray());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoDeadCode()
		{
			var analyzer = new DeadCodeAnalyzer();
			var buildXml = BuildXml.CreateFromSourceCodeDirectory();

			Parallel.ForEach(buildXml.GetAllSolutionFileNames().Where(solution => !solution.Contains("ThirdParty")).SelectMany(solution => buildXml.GetAllAssembliesInSolution(solution)),
				new ParallelOptions() { MaxDegreeOfParallelism = 2 },
				(string assemblyName) => analyzer.AnalyzeAssembly(assemblyName, buildXml));

			var deadTypes = analyzer.DefinedTypes.Where(t => !analyzer.UsedTypes.Contains(t)).OrderBy(t => t).ToArray();
			var baseline = new HashSet<string>(File.ReadAllLines(DeadCodeBaselineFile), StringComparer.OrdinalIgnoreCase);
			if (TestingState.IsRunningOnDAT)
			{
				deadTypes = deadTypes.Where(t => !baseline.Contains(t)).ToArray();
			}
			else
			{
				deadTypes = deadTypes.OrderBy(t => baseline.Contains(t)).ToArray();
			}
			if (deadTypes.Length > 0)
			{
				var message = new StringBuilder();
				message.AppendLine("<b>The following types appear to be unused and should be deleted.</b><br/><br/>");
				message.AppendLine("For sets of types are loaded dynamically through a factory using reflection add a [TypeFactoryAnnotationMethod] to the factory.<br/>");
				message.AppendLine("For types that are used in some other way that cannot be detected in the compiled code, use the [CodeAlive] attribute on the type and explain how the type is used on the reason field.");
				message.AppendLine("<br/><br/>");
				foreach (var deadType in deadTypes)
				{
					bool isBaseline = baseline.Contains(deadType);
					if (isBaseline)
					{
						message.Append("<span style='color=grey'>(BASELIN" +
							"E - You don't need to do anything) ");
					}
					message.Append(deadType);
					if (isBaseline)
					{
						message.Append("</span>");
					}
					message.Append("<br/>");
				}
				HtmlFail(message.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoUnusedSolutions()
		{
			var exceptions = new string[]
			{
				// These solutions are BI solutions, not C# solutions
				@"\BusinessIntelligence\BiIntegration\Development\StlAnalysis\StlAnalysisOlap\StlAnalysisOlap.sln",
				@"\BusinessIntelligence\BiIntegration\Monitoring\Reports\GlobalMonitoring.sln",
				@"\BusinessIntelligence\BillingBi\BillingBi.sln",
				@"\BusinessIntelligence\CargoWiseBi\CargoWiseBi.Models\CargoWiseBi.Models.sln",
				@"\BusinessIntelligence\CargoWiseBi\CargoWiseBi.SSRS\CargoWiseBi.SSRS.sln",
				@"\BusinessIntelligence\eServicesBi\AirlineMessageAnalysis\AirlineMessageAnalysis.sln",

				// all following solutions should be checked manually later. Each of these solutions should:
				// (1) have a reason not to be built through build.xml
				// or (2) be removed
				@"\Common\Tools\BuildTools\BuildTools\SourceControl\Testing\MockSourceControlFiles\Accounting\Data\TestSolution.sln",
				@"\Common\Tools\BuildTools\BuildTools\SourceControl\Testing\MockSourceControlFiles\BuildTools\SourceSafeTestSolution\SourceSafeTestSolution.sln",
				@"\Common\Tools\BuildTools\BuildTools\SourceControl\Testing\MockSourceControlFiles\SGCustoms\SGCustoms.sln",
				@"\Common\Tools\BuildTools\BuildTools\SourceControl\Testing\MockSourceControlFiles\ZArchitecture\ZArchitecture.sln",

				//Used to bring some related projects together. Makes it easier to work with them.
				@"\Enterprise\Product\Core\UMI Service Tasks.sln",
				@"\Enterprise\Product\Operations\Customs\PL\PL.All.sln",

				//Winzor Samples solution for development
				@"\Winzor\WinzorFramework.Samples.sln",
			};

			var buildXml = BuildXml.CreateFromSourceCodeDirectory();
			var solutionsInBuildXml = buildXml.GetAllSolutionFileNames().Select(fileName => BuildConstants.GetLocalPath(fileName)).ToArray();
			var solutionsNotInBuildXml = allSolutionFiles.Value.Except(solutionsInBuildXml, (IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase).ToArray();

			var solutionsUnused = solutionsNotInBuildXml.Where(solution => exceptions.All(exception => solution.IndexOf(exception, StringComparison.OrdinalIgnoreCase) == -1)).ToArray();
			if (solutionsUnused.Length > 0)
			{
				var message = new StringBuilder("These solutions are not used and should be deleted: ");
				foreach (var solution in solutionsUnused)
				{
					message.AppendLine();
					message.Append(solution);
				}
				message.AppendLine();
				message.AppendLine();
				message.Append("All detected solutions:");
				foreach (var solution in solutionsInBuildXml)
				{
					message.AppendLine();
					message.Append(solution);
				}
				Fail(message.ToString());
			}

			var exceptionsUnused = exceptions.Where(exception => solutionsNotInBuildXml.All(solution => solution.IndexOf(exception, StringComparison.OrdinalIgnoreCase) == -1));
			AssertEquals("These items are not used any more. Please remove them from array exceptions", "", string.Join("\n", exceptionsUnused));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoUnusedProjs()
		{
			var exceptions = new []
			{
				@"\packages\",
			};

			var projsFromSolutions = GetSolutionProjects()
				.Where(x => !externalProjectsExceptions.Contains((Path.GetFileName(x.Solution), Path.GetFileName(x.Project))))
				.Select(x => x.Project)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToArray();

			var projsNotExist = projsFromSolutions.Except(allProjectFiles.Value, (IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase).ToArray();
			AssertEquals("These projects are specified in solutions, but they do not exist.", "", string.Join("\n", projsNotExist));

			var projsNotFromSolutions = allProjectFiles.Value.Except(projsFromSolutions, (IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase).ToArray();

			var projsUnused = projsNotFromSolutions.Where(proj => exceptions.All(exception => proj.IndexOf(exception, StringComparison.OrdinalIgnoreCase) == -1));
			AssertEquals("These project files are not used by any solution and should be deleted: ", "", string.Join("\n", projsUnused));

			var exceptionsUnused = exceptions.Skip(1).Where(exception => projsNotFromSolutions.All(proj => proj.IndexOf(exception, StringComparison.OrdinalIgnoreCase) == -1));
			AssertEquals("These exceptions are not needed any more. Please remove them from exceptions array.", "", string.Join("\n", exceptionsUnused));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoUnlistedAssemblies()
		{
			var skipSolutions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)          {
				"Winzor.sln",
				"CWNetCoreTestAdapter.sln"
			};

			var skipOutputs = new []
			{
				// Intentionally excluded assembly. Source code is used for functional tests of AnalyzersRunner, but the assembly is
				// never actually built (contains analyzer errors that would stop compilation)
				"AnalyzersRunner.FunctionalTestingTarget.dll",
				$@"{InternalCommonAssemblyInfo.CWNetCoreSubfolder}\AnalyzersRunner.FunctionalTestingTarget.dll",
				"AppDomainWrappers.Net48.exe"
			};

			var exceptionsUsed = new List<string>();

			var errMsg = "";
			var buildXml = BuildXml.CreateFromSourceCodeDirectory();

			foreach (var solution in buildXml.GetAllSolutionFileNames())
			{
				if (skipSolutions.Contains(Path.GetFileName(solution)))
				{
					continue;
				}

				var assembliesFromBuildXml = buildXml.GetAllAssembliesInSolution(solution);
				var solutionFilePath = BuildConstants.GetLocalPath(solution);
				var assembliesFromSolution = new List<string>();

				foreach (var proj in GetProjsFromSolution(solutionFilePath))
				{
					if (proj.IndexOf(".Winzor", StringComparison.OrdinalIgnoreCase) > 0)
					{
						continue;
					}

					assembliesFromSolution.AddRange(ReflectionTestHelper.GetOutputAssemblies(proj));
				}

				var assembliesNotExist = assembliesFromBuildXml
					.Except(assembliesFromSolution, (IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase)
					.Where(x => !x.EndsWith(".XmlSerializers.dll", StringComparison.OrdinalIgnoreCase) && !buildXml.IsDuplicatedCodeAssembly(x)).ToArray();

				if (assembliesNotExist.Length > 0)
				{
					errMsg += string.Format("\nThese assemblies are in Build.xml, but not in solution {0}.\n\t{1}\n", solution, string.Join("\n\t", assembliesNotExist));
				}

				var assembliesToBeChecked = assembliesFromSolution
					.Except(assembliesFromBuildXml, (IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase)
					.ToArray();
				var assembliesNotListed = assembliesToBeChecked.Except(skipOutputs, (IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase).ToArray();

				if (assembliesNotListed.Length > 0)
				{
					errMsg += string.Format("\nThese assemblies are in solution {0}, but not in Build.xml.\n\t{1}\n", solution, string.Join("\n\t", assembliesNotListed));
				}

				exceptionsUsed.AddRange(assembliesToBeChecked.Except(assembliesNotListed, (IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase));
			}

			var exceptionsUnused = skipOutputs.Except(exceptionsUsed, (IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase).ToArray();

			if (exceptionsUnused.Length > 0)
			{
				errMsg += string.Format("\nThese items are not used any more. Please remove them from array exceptions\n\t{0}\n", string.Join("\n\t", exceptionsUnused));
			}

			AssertEquals("", errMsg);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoUnusedCsfiles()
		{
			var exceptions = new[]
			{
				@"\packages\",
				@"\Winzor\Infrastructure\TestSuppressions.cs", // Defined in Directory.Build.props using MsBuild properties,
				@"\NetCore\NetCoreAssemblyResolver.cs", // Included in NetCore projects as linked compilation file
				@"IsExternalInit.cs", // Required for C# 9.0 record types
			};

			var csfiles = allSourceFiles.Value.Where(s => s.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)).ToArray();
			if (ZArchitecture.Environment.Globals.IsUserInteractive)
			{
				csfiles = csfiles.Where(f => !InObjFolder(f.ToLowerInvariant())).ToArray();
			}

			var csfilesFromProjs = GetFilesFromCsProjs();
			var csfilesNotFromProjs = csfiles.Except(csfilesFromProjs, (IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase).ToArray();

			var csfilesUnused = csfilesNotFromProjs.Where(csfile => exceptions.All(exception => csfile.IndexOf(exception, StringComparison.OrdinalIgnoreCase) == -1));
			AssertEquals("These .cs files are not used and should be deleted: ", "", string.Join("\n", csfilesUnused));

			var exceptionsUnused = exceptions.Skip(1).Where(exception => csfilesNotFromProjs.All(csfile => csfile.IndexOf(exception, StringComparison.OrdinalIgnoreCase) == -1));
			AssertEquals("These items are not used any more. Please remove them from array exceptions", "", string.Join("\n", exceptionsUnused));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Changes BaseSourcePath")]
		public void TestGetFullPathCarefully()
		{
			var savedBaseSourcePath = BaseSourcePath;
			var path = new string('A', MaxSourcePathLength + 2);

			try
			{
				BaseSourcePath = @"\\SYDCO-SDTS-1.wtg.zone\DW\ER20150624-1";
				AssertExceptionThrown(typeof(PathTooLongException), () => GetFullPathCarefully(BaseSourcePath, path));

				BaseSourcePath = @"\\SYDCO-SDTS-1.wtg.zone\DW\Dev-0";
				AssertExceptionThrown(typeof(PathTooLongException), () => GetFullPathCarefully(BaseSourcePath, path));
			}
			finally
			{
				BaseSourcePath = savedBaseSourcePath;
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProjectPathMaxLength()
		{
			var tooLongPaths = new List<string>();
			foreach (var project in allProjectFiles.Value)
			{
				var projectRelativePath = project.Substring(BaseSourcePath.Length);
				if (Path.Combine(@"__\BS\CWReleases\CWYYYYMMDD\Dev", Path.GetDirectoryName(projectRelativePath), @"obj\RELEASE", Path.GetFileName(projectRelativePath) + ".cwResolveAssemblyReference.cache").Length >= 260)
				{
					tooLongPaths.Add(projectRelativePath);
				}
			}
			if (tooLongPaths.Any())
			{
				Fail("The following project paths will result in build failures on DAT build servers in release branches, please shorten:\r\n" + string.Join("\r\n", tooLongPaths));
			}
			else
			{
				Assert(true);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSLNFilters_IncludeValidPathsToFiles()
		{
			var baseSourcePathLength = BaseSourcePath.Length;

			var validSolutionPathSet = GetSetOfValidPaths(allSolutionFiles.Value, baseSourcePathLength);
			var validProjectPathSet = GetSetOfValidPaths(allProjectFiles.Value, baseSourcePathLength);

			var filterList = new List<string>();
			foreach (var filterFile in allSolutionFilterFiles.Value)
			{
				var filterPath = filterFile;
				filterList.Add(filterPath);
			}

			var filterSolutionPathList = new List<(string Filter, string Solution)>();
			var filterProjectPathList = new List<(string Filter, string Solution, string Project)>();
			foreach (var filter in filterList)
			{
				var filterDirectoryPath = Path.GetDirectoryName(filter);
				var filterDirectoryRelPath = filterDirectoryPath.Substring(baseSourcePathLength);
				var solutionPath = string.Empty;

				string[] lines = File.ReadAllLines(filter);
				foreach (var line in lines)
				{
					var unescapedLine = Regex.Unescape(line);
					if (unescapedLine.Contains(".sln"))
					{
						var substring = unescapedLine.Substring(unescapedLine.IndexOf(":"));
						solutionPath = ParseToPath(filterDirectoryRelPath, substring);
						filterSolutionPathList.Add((filter, solutionPath));
					}
					else if (unescapedLine.Contains(".csproj"))
					{
						var projectPath = ParseToPath(filterDirectoryRelPath, unescapedLine);
						filterProjectPathList.Add((filter, solutionPath, projectPath));
					}
				}
			}

			foreach (var (filter, solution) in filterSolutionPathList)
			{
				Assert($"Solution path {solution} in {filter} is not a valid path", validSolutionPathSet.Contains(solution));
			}

			foreach (var (filter, solution, project) in filterProjectPathList)
			{
				if (externalProjectsExceptions.Contains((Path.GetFileName(solution), Path.GetFileName(project))))
				{
					continue;
				}
				Assert($"Project path {project} in {filter} is not a valid path", validProjectPathSet.Contains(project));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDebugFast_IsAppliedConsistentlyWithinASolution()
		{
			var inconsistentSolutions = new List<string>();
			var consistentSolutionsToRemoveFromTheWhiteList = new List<string>();

			foreach (var slnFilePath in allSolutionFiles.Value)
			{
				var lines = File.ReadAllLines(slnFilePath, Encoding.UTF8);
				if (!HasDebugFastConfiguration(lines))
				{
					continue;
				}

				if (HasInconsistentBuildConfiguration(lines))
				{
					inconsistentSolutions.Add(slnFilePath);
				}
			}

			AssertContainsExactElementsInAnyOrder($@"{inconsistentSolutions.Count:N0} solution(s) use DEBUGFAST but project(s) are configured to build in Debug config.
For consistency of behavior, if any one project in a solution uses DEBUGFAST then all must.
Either a) remove all usage of DEBUGFAST, or b) ensure all projects build with DEBUGFAST in Visual Studio > Configuration Manager.",
				Array.Empty<string>(),
				inconsistentSolutions
			);

			AssertContainsExactElementsInAnyOrder($"{consistentSolutionsToRemoveFromTheWhiteList.Count:N0} solution(s) are correctly using DEBUGFAST but on the whitelist. Time to remove them!",
				Array.Empty<string>(),
				consistentSolutionsToRemoveFromTheWhiteList
			);

			#region Local Helpers

			// Note: not properly parsing SLN file; if the internal format changes we will need to do better.
			bool HasDebugFastConfiguration(string[] lines)
				=> lines.Any(l => l.IndexOf("DEBUGFAST", 0, StringComparison.OrdinalIgnoreCase) >= 0);

			bool HasInconsistentBuildConfiguration(string[] lines)
				=> lines.Any(l => l.IndexOf(".DEBUGFAST|Any CPU.", 0, StringComparison.OrdinalIgnoreCase) >= 0
							   && l.IndexOf("= Debug|Any CPU", 0, StringComparison.OrdinalIgnoreCase) >= 0);

			#endregion
		}

		#region Helpers

		static IEnumerable<string> GetProjsFromSolution(string solutionFilePath)
		{
			try
			{
				var solution = new CSharpSolution(solutionFilePath);
				return solution.Projects.Cast<string>().Select(Path.GetFullPath);
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("Error when finding *.csproj file names from {0}", solutionFilePath), ex);
			}
		}

		IEnumerable<(string Solution, string Project)> GetSolutionProjects()
			=> allSolutionFiles.Value.SelectMany(GetProjsFromSolution, (solution, project) => (solution, project));

		static string[] GetAllFiles(DirectoryInfo directoryInfo)
		{
			var files = new ConcurrentHashSet<string>(StringComparer.OrdinalIgnoreCase);
			FileSystemInfo[] fileSystemInfos = null;
			try
			{
				fileSystemInfos = directoryInfo.GetFileSystemInfos().Where(f => !f.FullName.Contains(@"\.git\")).ToArray();
			}
			catch (DirectoryNotFoundException)
			{
				// A build may occur on the network path while we are running the test, and the temporary directories may be deleted.  Ignore.
				return Array.Empty<string>();
			}

			//Getting the files in parallel is significantly faster when accessing files across the network (eg when DAT runs the test)
			Parallel.ForEach(fileSystemInfos, new ParallelOptions() { MaxDegreeOfParallelism = 3 }, (entry) =>
			{
				if (entry is FileInfo file)
				{
					files.TryAdd(entry.FullName);
				}
				else
				{
					GetAllFiles(new DirectoryInfo(entry.FullName)).ForEach((s) => files.TryAdd(s));
				}
			});

			return files.ToArray();
		}

		bool InObjFolder(string fileName) =>
			   fileName.Contains(@"\obj\debug\")
			|| fileName.Contains(@"\obj\release\")
			|| fileName.Contains(@"\obj\x86\")
			|| fileName.Contains(@"\obj\x64\")
			|| fileName.Contains(@"\obj\any cpu\")
		;

		const int MaxSourcePathLength = 214;

		static string GetFullPathCarefully(string path1, string path2)
		{
			string result = null;

			try
			{
				result = Path.GetFullPath(Path.Combine(path1, path2));
			}
			catch (PathTooLongException ex)
			{
				var errMsg = "GetFullPathCarefully error\n";
				errMsg += "path1=" + path1 + "\n";
				errMsg += "path2=" + path2 + "\n";
				throw new PathTooLongException(errMsg, ex);
			}

			var sourcePath = result.Substring(BaseSourcePath.Length);

			if (sourcePath.Length > MaxSourcePathLength)
			{
				var errMsg = "GetFullPathCarefully error\n";
				errMsg += "path1=" + path1 + "\n";
				errMsg += "path2=" + path2 + "\n";
				errMsg += "sourcePath=" + sourcePath + "\n";
				errMsg += string.Format("The sourcePath length ({0}) is too big (> {1})\n", sourcePath.Length, MaxSourcePathLength);
				throw new PathTooLongException(errMsg);
			}

			return result;
		}

		IEnumerable<string> GetFilesFromCsProjs()
		{
			var csfiles = new ConcurrentHashSet<string>(StringComparer.OrdinalIgnoreCase);

			Parallel.ForEach(allProjectFiles.Value, new ParallelOptions() { MaxDegreeOfParallelism = 2 }, (proj) =>
			{
				var projectFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				string lastFile = string.Empty;
				try
				{
					var xmlDoc = new XmlDocument();
					xmlDoc.Load(proj);
					var root = xmlDoc.DocumentElement;
					var path = Path.GetDirectoryName(proj);

					//WI00483710 created to update this when library to read project files using MSBuild tools
					var isSDKProject = root.HasAttribute("Sdk");
					if (isSDKProject)
					{
						var defaultIncludeAllCsFiles = GetAllFiles(new DirectoryInfo(Path.GetDirectoryName(proj))).Where(name => name.EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase));

						defaultIncludeAllCsFiles.ForEach(name => projectFiles.Add(name));
					}

					foreach (XmlNode node in root.ChildNodes)
					{
						if (!node.Name.Equals("ItemGroup", StringComparison.OrdinalIgnoreCase))
						{
							continue;
						}

						foreach (XmlNode subNode in node.ChildNodes)
						{
							var names = new string[] { "Compile", "None", "Content" };

							if (names.Any(x => subNode.Name.Equals(x, StringComparison.OrdinalIgnoreCase)))
							{
								var includeNode = subNode.Attributes["Include"];
								var removeNode = subNode.Attributes["Remove"];
								//Globbing is not fully supported. WI00483710 should fix this
								if (includeNode != null && !includeNode.InnerText.Contains("*"))
								{
									lastFile = includeNode.InnerText;
									projectFiles.Add(GetFullPathCarefully(path, includeNode.InnerText));
								}
								else if (removeNode != null && !isSDKProject)
								{
									// This supports limited globbing. For more complex exclusions, consider reworking the dead code test
									// to be more aligned with modern SDK-style MSBuild projects.
									if (removeNode.InnerText.EndsWith("**"))
									{
										var directory = GetFullPathCarefully(path, Path.GetDirectoryName(removeNode.InnerText));
										projectFiles.RemoveWhere(c => c.StartsWith(directory));
									}
									else
									{
										projectFiles.Remove(GetFullPathCarefully(path, removeNode.InnerText));
									}
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					throw new Exception(string.Format("Error when adding '{0}' file name from {1}", lastFile, proj), ex);
				}

				projectFiles.ForEach(f => csfiles.TryAdd(f));
			});

			return csfiles;
		}

		string DeadCodeBaselineFile
		{
			get { return Path.Combine(BaseSourcePath, "Common", "Testing", "Introspection", "ReflectionTest", "ReflectionTest", "DeadCodeBaseline.txt"); }
		}

		HashSet<string> GetSetOfValidPaths(string[] allPaths, int baseSourcePathLength)
		{
			var validPathSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (var path in allPaths)
			{
				var validRelativePath = path.Substring(baseSourcePathLength);
				validPathSet.Add(validRelativePath);
			}
			return validPathSet;
		}

		public void TestParseToPath()
		{
			AssertEquals("Common\\Architecture\\Windows.UI\\CargoWise.Windows.UI.Winzor.csproj", ParseToPath("Winzor", "\"..\\Common\\Architecture\\Windows.UI\\CargoWise.Windows.UI.Winzor.csproj\","));
			AssertEquals("Enterprise\\Architecture\\GUI.UserControls\\Aga.Controls\\Aga.Controls.Winzor.csproj", ParseToPath("Winzor\\BArchitecture.GUI", "\"..\\..\\Enterprise\\Architecture\\GUI.UserControls\\Aga.Controls\\Aga.Controls.Winzor.csproj\","));
			AssertEquals("Winzor\\BArchitecture.GUI.Test\\BArchitecture.GUI.Test.csproj", ParseToPath("Winzor", "\"BArchitecture.GUI.Test\\BArchitecture.GUI.Test.csproj\","));
			AssertEquals("Winzor\\Infrastructure\\CargoWise.Winzor.AppServer.Test\\CargoWise.Winzor.AppServer.Test.csproj", ParseToPath("Winzor\\BArchitecture.GUI", "\"..\\Infrastructure\\CargoWise.Winzor.AppServer.Test\\CargoWise.Winzor.AppServer.Test.csproj\","));
		}

		static string ParseToPath(string directoryPath, string line)
		{
			var pathStartSearch = "\"";
			var pathStartIndex = line.IndexOf(pathStartSearch) + pathStartSearch.Length;
			var pathEndSearch = "\"";
			var pathEndCount = line.LastIndexOf(pathEndSearch) - pathStartIndex;
			var path = line.Substring(pathStartIndex, pathEndCount);

			while (path.StartsWith("..\\"))
			{
				path = path.Substring(3);
				var index = directoryPath.LastIndexOf("\\");
				if (index == -1)
				{
					return path;
				}
				directoryPath = directoryPath.Substring(0, index);
			}

			return $"{directoryPath}\\{path}";
		}

		#endregion
	}
}
