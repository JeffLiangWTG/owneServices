using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.BuildTools;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.MSBuild;
using NUnit.Framework;

[assembly: AssemblyTitle("Analyzers Unit Test Runner")]

namespace AnalyzersRunner
{
	public static class Runner
	{
		public static async Task Main(string[] argv)
		{
			TestingState.IsRunningTests = true;
			if (argv.Length >= 4 && bool.TryParse(argv[3], out bool isRunningOnDat) && isRunningOnDat)
			{
				TestingState.IsRunningOnDAT = true;
			}

			var instance = DevMSBuildLocator.GetMSBuildInstance();
			MSBuildLocator.RegisterInstance(instance);

			await ActualRunnerAfterMSBuildLocatorIsRegistered.Main(argv, instance);
		}
	}

	/// <remarks>
	/// The reason this is a separate class is that MSBuildLocator needs to be run before any class that has a reference to any Microsoft.Build.* types. Otherwise the CLR will load
	/// Microsoft.Build.* assemblies when it loads the type. This means that MSBuildLocator cannot get the chance to intercept that load and redirect it to the proper location.
	/// </remarks>
	[SuppressMessage("CargoWiseOne", "CW1106:Do Not Leave In Debug Messages", Justification = "This console app uses output to report back to calling unit tests, as well as verbose output on request.")]
	[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStatic")]
	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer only tool")]
	static class ActualRunnerAfterMSBuildLocatorIsRegistered
	{
		static readonly ImmutableList<ReportDiagnostic> DiagnosticsToAnalyze = ImmutableList.Create(ReportDiagnostic.Error, ReportDiagnostic.Warn);
		static readonly ImmutableList<DiagnosticSeverity> SevertitiesToReport = ImmutableList.Create(DiagnosticSeverity.Error, DiagnosticSeverity.Warning);

		static bool mustShowVerboseOutput;

		static void LogVerbose(string message)
		{
			if (mustShowVerboseOutput)
			{
				Console.WriteLine(message);
			}
		}

		public static async Task Main(string[] argv, VisualStudioInstance visualStudioInstance)
		{
			if (argv.Length < 2)
			{
				Console.WriteLine("Error: this program expects at least two arguments. The first is the path to the source code folder. The second is the relative path to the .csproj file which is to be analyzed. If the optional third parameter is 'true' then this program will print out verbose logging.");
				return;
			}

			var totalTimeStopWatch = Stopwatch.StartNew();

			var sourcePath = argv[0];
			var projectRelativePath = argv[1];

			if (argv.Length >= 3)
			{
				bool.TryParse(argv[2], out mustShowVerboseOutput);
			}

			var projectFullPath = Path.Combine(sourcePath, projectRelativePath);

			var exePath = Assembly.GetExecutingAssembly().Location;
			var binariesFolderWhereExeIsRunning = Path.GetDirectoryName(exePath);

			var expectedBinariesFolderUnderSourcePath = Path.Combine(sourcePath, "Bin");

			var isBinariesFolderNotInExpectedLocation = !binariesFolderWhereExeIsRunning.Equals(expectedBinariesFolderUnderSourcePath, StringComparison.OrdinalIgnoreCase);
			var didCreateSymbolicLink = false;

			LogVerbose($"using Visual Studio instance '{visualStudioInstance.Name}'. MSBuild path is {visualStudioInstance.MSBuildPath}");
			LogVerbose($"analyzing {projectFullPath}...");

			try
			{
				if (isBinariesFolderNotInExpectedLocation)
				{
					//when running on DAT, the binaries are in a completely separate folder to the source code.
					//in order for analyzers to run properly, the binaries need to be in the [source]\bin folder.
					//in that case, we need to create a symbolic link from the source code folder to the actual binaries location.
					CreateSymbolicLinkToBinariesFolder(expectedBinariesFolderUnderSourcePath, binariesFolderWhereExeIsRunning);

					didCreateSymbolicLink = true;
				}

				var operationStopWatch = Stopwatch.StartNew();

				LogVerbose("loading project into MSBuildWorkspace...");

				using (var workspace = MSBuildWorkspace.Create())
				{
					workspace.AssociateFileExtensionWithLanguage("sqlproj", "C#");
					var project = await workspace.OpenProjectAsync(projectFullPath, progress: new LogProjectLoad());

					var diagnosticFailures = workspace.Diagnostics.Where(d => d.Kind == WorkspaceDiagnosticKind.Failure).ToList();
					diagnosticFailures.RemoveAll(d => d.Message.Contains("Project does not contain 'Compile' target."));

					if (diagnosticFailures.Count != 0)
					{
						if (workspace.Diagnostics.Any(d => d.Message.Contains(@"Microsoft.CompactFramework.CSharp.targets"" was not found")))
						{
							return;
						}
						throw new InvalidOperationException(string.Join(Environment.NewLine, diagnosticFailures.Select(d => d.Message)));
					}

					if (!project.HasDocuments)
					{
						throw new InvalidOperationException("Project contains no documents after it has been loaded. project path = " + projectFullPath);
					}

					if (mustShowVerboseOutput)
					{
						operationStopWatch.Stop();
						LogVerbose($"\ttook {operationStopWatch.Elapsed} ({operationStopWatch.ElapsedMilliseconds:#,##0}ms)");

						var diagnosticIdsToReport = project.CompilationOptions.SpecificDiagnosticOptions.Where(x => DiagnosticsToAnalyze.Contains(x.Value)).Select(x => x.Key).OrderBy(x => x).ToList();

						LogVerbose($"the project has {diagnosticIdsToReport.Count:#,##0} specific diagnostic(s) set to warn or error:");

						foreach (var diagnosticId in diagnosticIdsToReport)
						{
							LogVerbose($"\t{diagnosticId} = {project.CompilationOptions.SpecificDiagnosticOptions[diagnosticId]}");
						}

						operationStopWatch.Restart();
					}

					LogVerbose("compiling project...");
					var comp = await project.GetCompilationAsync();

					operationStopWatch.Stop();
					LogVerbose($"\ttook {operationStopWatch.Elapsed} ({operationStopWatch.ElapsedMilliseconds:#,##0}ms)");

					operationStopWatch.Restart();
					var analyzers = LoadAnalyzersFromProjectAnalyzerReferences(workspace, project, comp, out var diagnosticReportById);

					operationStopWatch.Stop();
					LogVerbose($"\ttook {operationStopWatch.Elapsed} ({operationStopWatch.ElapsedMilliseconds:#,##0}ms)");

					if (mustShowVerboseOutput)
					{
						LogVerbose($"the project has {diagnosticReportById.Count:#,##0} diagnostic(s) set to warn or error:");

#pragma warning disable CW1024 // Bad Concurrent Collection Access - not actually concurrent here
						foreach (var diagnosticReportEntry in diagnosticReportById.OrderBy(x => x.Key))
#pragma warning restore CW1024 // Bad Concurrent Collection Access - not actually concurrent here
						{
							LogVerbose($"\t{diagnosticReportEntry.Key}:{diagnosticReportEntry.Value.Title} = {diagnosticReportEntry.Value.Severity}");
						}
					}

					operationStopWatch.Restart();
					LogVerbose("starting analysis...");

					if (analyzers.Length != 0)
					{
						var analyzerCompilation = comp
							.WithAnalyzers(analyzers, new CompilationWithAnalyzersOptions(project.AnalyzerOptions, null, concurrentAnalysis: true, logAnalyzerExecutionTime: mustShowVerboseOutput, reportSuppressedDiagnostics: false));

						var analysisResults = await analyzerCompilation.GetAnalysisResultAsync(CancellationToken.None);
						var allDiagnostics = analysisResults.GetAllDiagnostics();
						var filteredDiagnosticList = allDiagnostics.Where(x => SevertitiesToReport.Contains(x.Severity)).ToList();

						LogVerbose($"{allDiagnostics.Length:#,##0} diagnostic issues reported, of which {filteredDiagnosticList.Count:#,##0} are warn or error");
						operationStopWatch.Stop();
						LogVerbose($"\ttook {operationStopWatch.Elapsed} ({operationStopWatch.ElapsedMilliseconds:#,##0}ms)");
						operationStopWatch.Restart();

						if (filteredDiagnosticList.Count > 0)
						{
							LogVerbose("warn/error diagnostics:");

							var diagnosticHtmlReports = filteredDiagnosticList
								.OrderBy(d => d.Location.GetLineSpan().Path).ThenBy(d => d.Location.GetLineSpan().StartLinePosition.Line).ThenBy(d => d.Location.GetLineSpan().StartLinePosition.Character)
								.Select(d => GetDiagnosticHtml(d));

							Console.WriteLine(string.Join("<br/>\r\n", diagnosticHtmlReports));
						}

						if (mustShowVerboseOutput)
						{
							LogVerbose($"\n\n=== Analyzer Telemetry ({analysisResults.AnalyzerTelemetryInfo.Count:#,##0} total) ===\n");

							foreach (var telemPair in analysisResults.AnalyzerTelemetryInfo.OrderByDescending(x => x.Value.ExecutionTime))
							{
								var analyzer = telemPair.Key;
								var telem = telemPair.Value;

								LogVerbose($"{analyzer.GetType().FullName}");
								LogVerbose($"\ttook {telem.ExecutionTime} ({telem.ExecutionTime.TotalMilliseconds:#,##0}ms)");
								LogVerbose($"\tsupported diagnostics");

								foreach (var supportedDiagnosticGroupedById in analyzer.SupportedDiagnostics.GroupBy(x => x.Id).OrderBy(x => x.Key))
								{
									var firstDiagnostic = supportedDiagnosticGroupedById.First();

									ReportDiagnostic? severity = null;

#pragma warning disable CW1024 // Bad Concurrent Collection Access - not actually concurrent here
									if (diagnosticReportById.TryGetValue(firstDiagnostic.Id, out var specificSeverity))
#pragma warning restore CW1024 // Bad Concurrent Collection Access
									{
										severity = specificSeverity.Severity;
									}

									LogVerbose($"\t\t[{(severity == null ? "not configured" : severity.ToString())}] {firstDiagnostic.Id} - {firstDiagnostic.Title}");
								}
							}
						}
					}
					else
					{
						throw new InvalidOperationException("zero analyzers set to 'warn' or 'error' were found. project path = " + projectFullPath);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("<pre>");

				if (ex is AggregateException aggEx)
				{
					foreach (var innerEx in aggEx.InnerExceptions)
					{
						HandleException(innerEx);
					}
				}
				else
				{
					HandleException(ex);
				}

				void HandleException(Exception exception)
				{
					Console.WriteLine(exception.ToString());

					if (exception is ReflectionTypeLoadException rtlx)
					{
						Console.WriteLine("Loader Exceptions:");

						foreach (var lx in rtlx.LoaderExceptions)
						{
							Console.WriteLine();
							Console.WriteLine(lx.ToString());
						}
					}
				}

				Console.WriteLine("</pre>");
			}
			finally
			{
				if (didCreateSymbolicLink)
				{
					//clean up the symbolic link
					Directory.Delete(expectedBinariesFolderUnderSourcePath);
				}

				if (mustShowVerboseOutput)
				{
					totalTimeStopWatch.Stop();

					Console.WriteLine($"total time: {totalTimeStopWatch.Elapsed}");
					Console.WriteLine($"peak memory working set: {Process.GetCurrentProcess().PeakWorkingSet64 / 1024.0 / 1024.0:#,##0.00}mb");
				}
			}
		}

		static void CreateSymbolicLinkToBinariesFolder(string expectedBinariesFolderUnderSourcePath, string binariesFolderWhereExeIsRunning)
		{
			var mustCreateSymbolicLink = true;

			if (Directory.Exists(expectedBinariesFolderUnderSourcePath))
			{
				//occassionally the binaries folder already exists under the source code folder in DAT.
				//if this is just an empty folder, or a symbolic link that was not cleaned up properly
				//then we can proceed. Otherwise, we need to output more information to figure out why this is
				//occurring.

				LogVerbose($"binaries folder already exists at {expectedBinariesFolderUnderSourcePath}");

				var didHandleBinariesFolder = false;
				var canDeleteBinariesFolder = false;

				var isSymbolicLink = SymbolicLink.TryGetTarget(expectedBinariesFolderUnderSourcePath, out var linkTarget);

				if (isSymbolicLink)
				{
					if (linkTarget == binariesFolderWhereExeIsRunning)
					{
						//symoblic link already exists to the target folder. this should have been cleaned up, but we
						//can reuse it
						mustCreateSymbolicLink = false;
					}
					else
					{
						//symbolic link to another folder. this should have been cleaned up. we can delete it
						canDeleteBinariesFolder = true;
					}

					didHandleBinariesFolder = true;
				}
				else if (!Directory.EnumerateFiles(expectedBinariesFolderUnderSourcePath, "*.*", SearchOption.AllDirectories).Any())
				{
					//binaries folder contains no files (it is empty or contains empty folders), we can remove it
					canDeleteBinariesFolder = true;
					didHandleBinariesFolder = true;
				}

				if (didHandleBinariesFolder)
				{
					if (canDeleteBinariesFolder)
					{
						LogVerbose($"deleting existing folder at {expectedBinariesFolderUnderSourcePath}...");

						Directory.Delete(expectedBinariesFolderUnderSourcePath);
					}
				}
				else
				{
					//unexpected binaries folder. output some logging information so we can diagnose it.
					//writing to the console will fail the unit test that is calling AnalyzersRunner and we
					//can find see the output in DAT failed test

					Console.WriteLine($"Bin folder already exists under source path. Please run AnalyzersRunner from there. source path binaries folder = {expectedBinariesFolderUnderSourcePath}. folder {nameof(AnalyzersRunner)} is running from = {binariesFolderWhereExeIsRunning}");

					Console.WriteLine($"is symbolic link? {isSymbolicLink}. symbolic link target = {linkTarget}");
					Console.WriteLine($"comparison of {expectedBinariesFolderUnderSourcePath} to {binariesFolderWhereExeIsRunning}:");

					var comparison = FolderComparer.CompareFolders(expectedBinariesFolderUnderSourcePath, binariesFolderWhereExeIsRunning);

					Console.WriteLine($"files in {expectedBinariesFolderUnderSourcePath} only ({comparison.OnlyInSource.Length:#,##0}):");

					foreach (var file in comparison.OnlyInSource)
					{
						Console.WriteLine($"\t{file.RelativePath} ({file.Size:#,##0} bytes)");
					}

					Console.WriteLine($"files in {binariesFolderWhereExeIsRunning} only ({comparison.OnlyInTarget.Length:#,##0}):");

					foreach (var file in comparison.OnlyInTarget)
					{
						Console.WriteLine($"\t{file.RelativePath} ({file.Size:#,##0} bytes)");
					}

					Console.WriteLine($"files with different contents ({comparison.DifferentFiles.Length:#,##0}):");

					foreach (var files in comparison.DifferentFiles)
					{
						Console.WriteLine($"\t{files.source.RelativePath}");
						Console.WriteLine($"\t\t source Size = {files.source.Size}. Date Modified = {files.source.DateModified}");
						Console.WriteLine($"\t\t target Size = {files.target.Size}. Date Modified = {files.target.DateModified}");
					}

					Console.WriteLine($"identical files ({comparison.SameFiles.Length:#,##0}):");

					foreach (var file in comparison.SameFiles)
					{
						Console.WriteLine($"\t{file.RelativePath} ({file.Size:#,##0} bytes)");
					}

					throw new InvalidOperationException("binaries folder already exists. could not create symbolic link");
				}
			}

			if (mustCreateSymbolicLink)
			{
				LogVerbose($"creating a symbolic link from {expectedBinariesFolderUnderSourcePath} to {binariesFolderWhereExeIsRunning}...");

				if (!SymbolicLink.CreateSymbolicLink(expectedBinariesFolderUnderSourcePath, binariesFolderWhereExeIsRunning, SymbolicLink.SYMBOLIC_LINK_FLAG.Directory))
				{
					var lastErrorCode = Marshal.GetLastWin32Error();

					var message = $"error: could not create symbolic link. source folder = {expectedBinariesFolderUnderSourcePath}. dest folder = {binariesFolderWhereExeIsRunning}. error code = {lastErrorCode}";

					//see: https://learn.microsoft.com/en-us/windows/win32/debug/system-error-codes--1300-1699-
					if (lastErrorCode == 1314)
					{
						message += ". 1314 = ERROR_PRIVILEGE_NOT_HELD";
					}

					throw new InvalidOperationException(message);
				}
			}
		}

		static ImmutableArray<DiagnosticAnalyzer> LoadAnalyzersFromProjectAnalyzerReferences(Workspace workspace, Project project, Compilation compilation, out ConcurrentDictionary<string, (string Title, ReportDiagnostic Severity)> analyzerDiagnotics)
		{
			//Note: DiagnosticAnalyzer.GetEffectiveSeverity reports different results than expected and different from what this function
			//actually returns. it also does not match up with what Visual Studio reports.

			LogVerbose($"scanning the project's {project.AnalyzerReferences.Count:#,##0} analyzer references in parallel for diagnostic analyzer classes...");

			var analyzers = new ConcurrentBag<DiagnosticAnalyzer>();
			var analyzerDiagnoticsInternal = analyzerDiagnotics = new ConcurrentDictionary<string, (string Title, ReportDiagnostic severity)>();

			var countGrandTotalAnalyzers = 0;

			Parallel.ForEach(project.AnalyzerReferences, analyzerReference =>
			{
				var stopWatchAssemblyScanning = Stopwatch.StartNew();

				var countTotalAnalyzers = 0;
				var countIncludedAnalyzers = 0;

				var assembly = LoadAnalyzerAssemblyForReference(workspace, analyzerReference);

				foreach (var analyzer in analyzerReference.GetAnalyzers(project.Language))
				{
					countTotalAnalyzers++;
					Interlocked.Increment(ref countGrandTotalAnalyzers);

					var mustIncludeAnalyzer = false;

					foreach (var diagnostic in analyzer.SupportedDiagnostics)
					{
						var severity = CalculateEffectiveSeverity(diagnostic, project, compilation);

						if (DiagnosticsToAnalyze.Contains(severity))
						{
							mustIncludeAnalyzer = true;
							analyzerDiagnoticsInternal.TryAdd(diagnostic.Id, (diagnostic.Title.ToString(), severity));
						}
					}

					if (mustIncludeAnalyzer)
					{
						countIncludedAnalyzers++;
						analyzers.Add(analyzer);
					}
				}

				stopWatchAssemblyScanning.Stop();

				LogVerbose($"\t{analyzerReference.FullPath} contains {countTotalAnalyzers:#,##0} analyzer classes. {countIncludedAnalyzers:#,##0} of these are set to warn or error.\n\t\ttook {stopWatchAssemblyScanning.Elapsed} ({stopWatchAssemblyScanning.ElapsedMilliseconds:#,##0}ms)");
			});

#pragma warning disable CW1024 // Bad Concurrent Collection Access - read only collection at this stage
			LogVerbose($"across all analyzer references, there are {countGrandTotalAnalyzers:#,##0} possible analyzers. after filtering down to those analyzers that are set to warn or error only, we will be running {analyzers.Count:#,##0} analyzers.");

			return analyzers.ToImmutableArray();
#pragma warning restore CW1024 // Bad Concurrent Collection Access
		}

		static Assembly LoadAnalyzerAssemblyForReference(Workspace workspace, AnalyzerReference reference)
		{
			if (reference is AnalyzerFileReference analyzerFileReference)
			{
				return analyzerFileReference.GetAssembly();
			}
			else
			{
				var analyzerService = workspace.Services.GetRequiredService<IAnalyzerService>();
				var loader = analyzerService.GetLoader();
				return loader.LoadFromPath(reference.FullPath);
			}
		}

		//inspired from https://github.com/dotnet/roslyn/blob/9aa3370949571eed5a7385764a1fcc4e29b16559/src/Compilers/Core/Portable/DiagnosticAnalyzer/AnalyzerManager.cs#L292
		//however, does it in reverse order. most specific first to least specific, so we can short-cut early
		static ReportDiagnostic CalculateEffectiveSeverity(DiagnosticDescriptor diagnostic, Project project, Compilation compilation)
		{
			var highestDiagnosticSettingInEditorConfig = ReportDiagnostic.Default;

			var hasFoundDiagnosticSettingInEditorConfig = false;
			var doesAnySyntaxTreeHaveDefaultSetting = false;

			//for every file in the project, check diagnostic settings from .editorconfig files, either for a specific diagnostic, category-specific setting or
			//a general setting at the category level or global level
			//see: https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-options
			foreach (var syntaxTree in compilation.SyntaxTrees)
			{
				if (compilation.Options.SyntaxTreeOptionsProvider.TryGetDiagnosticValue(syntaxTree, diagnostic.Id, CancellationToken.None, out var severityInSyntaxTree))
				{
					if (severityInSyntaxTree.IsMoreSevereThan(highestDiagnosticSettingInEditorConfig))
					{
						highestDiagnosticSettingInEditorConfig = severityInSyntaxTree;
						hasFoundDiagnosticSettingInEditorConfig = true;
					}
				}
				else if (project.AnalyzerOptions.TryGetSeverityFromBulkConfiguration(syntaxTree, compilation, diagnostic, out severityInSyntaxTree))
				{
					if (severityInSyntaxTree.IsMoreSevereThan(highestDiagnosticSettingInEditorConfig))
					{
						highestDiagnosticSettingInEditorConfig = severityInSyntaxTree;
						hasFoundDiagnosticSettingInEditorConfig = true;
					}
				}

				if (!doesAnySyntaxTreeHaveDefaultSetting && severityInSyntaxTree == ReportDiagnostic.Default)
				{
					doesAnySyntaxTreeHaveDefaultSetting = true;
				}
			}

			if (hasFoundDiagnosticSettingInEditorConfig && !doesAnySyntaxTreeHaveDefaultSetting)
			{
				return highestDiagnosticSettingInEditorConfig;
			}

			var defaultSeverity = ReportDiagnostic.Default;

			//check diagnostic settings from warnAsError property and any code analysis ruleset xml file
			if (compilation.Options.SpecificDiagnosticOptions.TryGetValue(diagnostic.Id, out var severity))
			{
				defaultSeverity = severity;
			}
			//check global configuration
			else if (compilation.Options.SyntaxTreeOptionsProvider.TryGetGlobalDiagnosticValue(diagnostic.Id, CancellationToken.None, out severity))
			{
				defaultSeverity = severity;
			}
			//finally, use the diagnostic default
			else if (diagnostic.IsEnabledByDefault)
			{
				defaultSeverity = diagnostic.DefaultSeverity.MapToReportDiagnostic();
			}
			else
			{
				defaultSeverity = ReportDiagnostic.Suppress;
			}

			if (hasFoundDiagnosticSettingInEditorConfig && doesAnySyntaxTreeHaveDefaultSetting)
			{
				return highestDiagnosticSettingInEditorConfig.IsMoreSevereThan(defaultSeverity) ? highestDiagnosticSettingInEditorConfig : defaultSeverity;
			}
			else
			{
				return defaultSeverity;
			}
		}

		static string GetDiagnosticHtml(Diagnostic diagnostic)
		{
			var lineSpan = diagnostic.Location.GetLineSpan();

			//StartLinePosition is zero-based. see: https://learn.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.text.lineposition.line?view=roslyn-dotnet#microsoft-codeanalysis-text-lineposition-line
			var lineNumber = lineSpan.StartLinePosition.Line + 1;
			var columnNumber = lineSpan.StartLinePosition.Character + 1;

			return FormattableString.Invariant(
				$"<a href='vsnet:{lineSpan.Path}#{lineNumber}'>{lineSpan.Path}({lineNumber},{columnNumber})</a>: [{diagnostic.Severity}] {diagnostic.Descriptor.Id}: {diagnostic.GetMessage()}");
		}

		struct LogProjectLoad : IProgress<ProjectLoadProgress>
		{
			public void Report(ProjectLoadProgress value)
			{
				LogVerbose($"\toperation = {value.Operation} ({value.ElapsedTime.TotalMilliseconds:#,##0}ms)");
			}
		}

		#region folder contents diff

		static class FolderComparer
		{
			public struct FileDetails
			{
				public string RelativePath;
				public long Size;
				public DateTime DateModified;
			}

			public class ComparisonResults
			{
				public FileDetails[] OnlyInSource;
				public FileDetails[] OnlyInTarget;

				public FileDetails[] SameFiles;
				public (FileDetails source, FileDetails target)[] DifferentFiles;
			}

			public static ComparisonResults CompareFolders(string source, string target)
			{
				if (!Directory.Exists(source))
				{
					throw new ArgumentException("source folder does not exist. folder = " + source, nameof(source));
				}

				if (!Directory.Exists(target))
				{
					throw new ArgumentException("target folder does not exist. folder = " + target, nameof(target));
				}

				var result = new ComparisonResults();

				//read source files
				var sourceFilesByRelativePath = new Dictionary<string, FileDetails>(StringComparer.OrdinalIgnoreCase);

				var sourceDirInfo = new DirectoryInfo(source);

				foreach (var sourceFileInfo in sourceDirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories))
				{
					var relativePath = sourceFileInfo.FullName.Substring(source.Length + 1);

					sourceFilesByRelativePath.Add(relativePath, new FileDetails
					{
						RelativePath = relativePath,
						Size = sourceFileInfo.Length,
						DateModified = sourceFileInfo.LastWriteTime
					});
				}

				//read target files
				var targetFilesByRelativePath = new Dictionary<string, FileDetails>(StringComparer.OrdinalIgnoreCase);

				var targetDirInfo = new DirectoryInfo(target);

				foreach (var targetFileInfo in targetDirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories))
				{
					var relativePath = targetFileInfo.FullName.Substring(target.Length + 1);

					targetFilesByRelativePath.Add(relativePath, new FileDetails
					{
						RelativePath = relativePath,
						Size = targetFileInfo.Length,
						DateModified = targetFileInfo.LastWriteTime
					});
				}

				//compare files
				result.OnlyInSource = sourceFilesByRelativePath.Values.Where(x => !targetFilesByRelativePath.ContainsKey(x.RelativePath)).OrderBy(x => x.RelativePath).ToArray();
				result.OnlyInTarget = targetFilesByRelativePath.Values.Where(x => !sourceFilesByRelativePath.ContainsKey(x.RelativePath)).OrderBy(x => x.RelativePath).ToArray();

				var matchingFileDetails = new List<(FileDetails source, FileDetails target)>();

				foreach (var sourceFile in sourceFilesByRelativePath.Values)
				{
					if (!targetFilesByRelativePath.TryGetValue(sourceFile.RelativePath, out var targetFile))
					{
						continue;
					}

					matchingFileDetails.Add((sourceFile, targetFile));
				}

				result.SameFiles = matchingFileDetails.Where(x => x.source.Size == x.target.Size && x.source.DateModified == x.target.DateModified).Select(x => x.source).OrderBy(x => x.RelativePath).ToArray();
				result.DifferentFiles = matchingFileDetails.Where(x => x.source.Size != x.target.Size || x.source.DateModified != x.target.DateModified).OrderBy(x => x.source.RelativePath).ToArray();

				return result;
			}
		}

		#endregion folder contents diff
	}
}
