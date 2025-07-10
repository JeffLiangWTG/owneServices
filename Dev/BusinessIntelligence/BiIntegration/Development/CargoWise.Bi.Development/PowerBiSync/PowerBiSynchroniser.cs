using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Bi.Development.Common;
using CargoWise.BuildTools;
using CargoWise.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Bi.Development.PowerBiSync
{
	public static class PowerBiSynchroniser
	{
		#region SuppressResourceStringsCheckRegion

		public static void Sync()
		{
			if (IsSyncRequired())
			{
				ExtractCustomVisualFiles();
				UpdateReportVisuals();
				ExtractReportFiles();
				AddReportLayoutsToRegistrationsProject();

				BiLogger.Complete("Power BI synchronisation completed");
			}
			else
			{
				BiLogger.StartTask("Power BI extraction is not required. No files are checked out.");
			}
		}

		public static void AddReportLayoutsToRegistrationsProject()
		{
			BiLogger.StartTask("Adding Report Layouts to Registration.csproj as embedded resources");

			var embeddedResourceTags = new List<string>();
			foreach (var reportFile in PowerBiReportFiles)
			{
				embeddedResourceTags.Add(
					ReportLayoutsEmbeddedResourceTag(reportFile)
				);
			}
			AddLayoutTagsToRegistrationsProject(embeddedResourceTags, RegistrationProjectFilePath);
		}

		#region File Extraction

		static void ExtractReportFiles()
		{
			if (PowerBiReportFiles_CheckedOut.Any())
			{
				BiLogger.StartTask("Extracting Power BI report file contents");
				foreach (var reportFile in PowerBiReportFiles_CheckedOut)
				{
					ExtractPowerBiReportFile(reportFile);
				}
			}
		}

		static string ReportLayoutsEmbeddedResourceTag(string reportFile)
		{
			var parent = new DirectoryInfo(reportFile).Parent.Name;
			var reportFileName = Path.GetFileNameWithoutExtension(reportFile);
			var targetDirectory = Path.Combine(@"..\..\..\..\CargoWiseBi\CargoWiseBi.PBIRS\Reports", parent, reportFileName);
			return $@"
		<EmbeddedResource Include=""{targetDirectory}\Report\Layout"">
			<Link>Test\PowerBi\Reports\{parent}\{reportFileName}\Report\Layout</Link>
		</EmbeddedResource>";
		}

		public static void AddLayoutTagsToRegistrationsProject(List<string> embeddedResourceTags, string projectFilePath)
		{
			var embeddedResourceGroup = $@"<!--Report Layout Files-->
	<ItemGroup Condition=""$(Configuration) == 'Debug'"">{string.Join("", embeddedResourceTags)}
	</ItemGroup>
	<!--Report Layout Files-->";
			var projectContent = File.ReadAllText(projectFilePath);

			var reportLayoutRegex = @"<!--Report Layout Files-->(.|\s)*?<!--Report Layout Files-->";
			var regex = new Regex(reportLayoutRegex, RegexOptions.IgnoreCase);
			projectContent = regex.Replace(projectContent, embeddedResourceGroup);
			BiFiles.SaveFile(projectFilePath, projectContent);
		}

		static void ExtractCustomVisualFiles()
		{
			if (CustomVisualFiles.Any())
			{
				BiLogger.StartTask("Extracting Power BI custom visual file contents");
				foreach (var customVisual in CustomVisualFiles)
				{
					ExtractCustomVisualFile(customVisual);
				}
			}
		}

		static void ExtractPowerBiReportFile(string reportFile)
		{
			var parent = new DirectoryInfo(reportFile).Parent.Name;
			var reportFileName = Path.GetFileNameWithoutExtension(reportFile);
			var targetDirectory = Path.Combine(PowerBiReportFileDirectory, parent, reportFileName);
			BiLogger.StartSubtask($"{parent}/{reportFileName}");
			ExtractFile(reportFile, targetDirectory);
		}

		static void ExtractCustomVisualFile(string visualFile)
		{
			var visualFileName = Path.GetFileNameWithoutExtension(visualFile);
			BiLogger.StartSubtask($"CustomVisualization/{visualFileName}");

			var targetDirectory = Path.Combine(CustomVisualFileDirectory, visualFileName);
			ExtractFile(visualFile, targetDirectory);
		}

		static void ExtractFile(string powerBiFile, string targetDirectory)
		{
			if (Directory.Exists(targetDirectory))
			{
				using (var sourceControl = SourceControlFactory.Instance.GetSourceControl())
				{
					foreach (var reportFile in Directory.EnumerateFiles(targetDirectory, "*", SearchOption.AllDirectories))
					{
						if (sourceControl.IsFileCheckedOutByMe(reportFile))
						{
							sourceControl.UndoCheckOut(reportFile, true);
						}
					}
				}
				DeleteDirectory(targetDirectory);
			}
			ZipFile.ExtractToDirectory(powerBiFile, targetDirectory);
			var visualGuidDictionary = CreateVisualGuidDictionary(targetDirectory);

			foreach (var directory in Directory.EnumerateDirectories(targetDirectory, "*", SearchOption.AllDirectories))
			{
				RenameDirectoryIfVisual(directory, visualGuidDictionary);
			}
		}

		static void RenameDirectoryIfVisual(string directory, Dictionary<string, string> visualGuidDictionary)
		{
			foreach (var visualName in visualGuidDictionary)
			{
				var dirInfo = new DirectoryInfo(directory);
				if (dirInfo.Name.Contains(visualName.Key))
				{
					var newDirectoryName = Path.Combine(dirInfo.Parent.FullName, dirInfo.Name.Replace(visualName.Key, visualName.Value));
					Directory.Move(directory, newDirectoryName);
					return;
				}
			}
		}

		#endregion

		#region Updating Report Visuals

		static void UpdateReportVisuals()
		{
			if (CustomVisualFiles.Any())
			{
				BiLogger.StartTask("Updating Power BI reports with checked out custom visuals.");
				foreach (var customVisual in CustomVisualFiles)
				{
					var visualGuidName = GetVisualGuidName(customVisual);
					if (!string.IsNullOrEmpty(visualGuidName))
					{
						UpdateReports(customVisual, visualGuidName);
					}
				}
			}
		}

		static string GetVisualGuidName(string customVisual)
		{
			var packageInfoFile = Path.Combine(customVisual.Replace(".pbiviz", ""), "package.json");
			if (File.Exists(packageInfoFile))
			{
				using (var fileStream = File.OpenRead(packageInfoFile))
				using (var reader = new StreamReader(fileStream))
				{
					string json = reader.ReadToEnd();
					var packageObj = JsonConvert.DeserializeObject<JObject>(json);

					return packageObj.SelectToken("visual.guid").ToString();
				}
			}
			else
			{
				return null;
			}
		}

		static void UpdateReports(string customVisual, string visualGuidName)
		{
			var pbivizJsonFileName = visualGuidName + ".pbiviz.json";
			foreach (var reportFile in Directory.EnumerateFiles(PowerBiReportFileDirectory, "*.pbix", SearchOption.AllDirectories))
			{
				var extractedReportFileDirectory = reportFile.Replace(".pbix", "");
				if (Directory.EnumerateFiles(extractedReportFileDirectory, pbivizJsonFileName, SearchOption.AllDirectories).Any())
				{
					UpdateReportVisual(reportFile, customVisual, visualGuidName);
				}
			}
		}

		static void UpdateReportVisual(string reportFile, string customVisual, string visualGuidName)
		{
			BiLogger.StartSubtask($"Updating '{reportFile}' custom visual version for '{visualGuidName}'");

			using (var tempDir = new TempDirectory())
			{
				var reportName = Path.GetFileNameWithoutExtension(reportFile);
				var targetExtractDirectory = Path.Combine(tempDir.DirectoryName, reportName);
				ZipFile.ExtractToDirectory(reportFile, targetExtractDirectory);

				var visualExtractDirectory = Path.Combine(targetExtractDirectory, "Report", "CustomVisuals", visualGuidName);
				DeleteDirectory(visualExtractDirectory);
				ZipFile.ExtractToDirectory(customVisual, visualExtractDirectory);

				var newReportFile = Path.Combine(tempDir.DirectoryName, reportName + ".pbix");

				ZipFile.CreateFromDirectory(targetExtractDirectory, newReportFile);

				File.Copy(newReportFile, reportFile, true);
			}
		}

		#endregion

		static Dictionary<string, string> CreateVisualGuidDictionary(string targetDirectory)
		{
			var dict = new Dictionary<string, string>();

			foreach (var packageInfoFile in Directory.EnumerateFiles(targetDirectory, "package.json", SearchOption.AllDirectories))
			{
				using (var fileStream = File.OpenRead(packageInfoFile))
				using (var reader = new StreamReader(fileStream))
				{
					string json = reader.ReadToEnd();
					var packageObj = JsonConvert.DeserializeObject<JObject>(json);

					var guid = packageObj.SelectToken("visual.guid").ToString();
					var displayName = packageObj.SelectToken("visual.name").ToString();

					dict[guid] = displayName;
				}
			}

			return dict;
		}

		static void DeleteDirectory(string targetDirectory)
		{
			foreach (var file in Directory.GetFiles(targetDirectory))
			{
				File.SetAttributes(file, FileAttributes.Normal);
				File.Delete(file);
			}

			foreach (var dir in Directory.GetDirectories(targetDirectory))
			{
				DeleteDirectory(dir);
			}

			Directory.Delete(targetDirectory, false);
		}

		#region Files

		static bool IsSyncRequired()
		{
			return PowerBiReportFiles_CheckedOut.Any() || CustomVisualFiles.Any();
		}

		static IEnumerable<string> PowerBiReportFiles
		{
			get
			{
				return powerBiReportFiles ??
					(powerBiReportFiles = Directory.EnumerateFiles(PowerBiReportFileDirectory, ".", SearchOption.AllDirectories).Where(r => r.EndsWith(".pbix") || r.EndsWith(".rdl")));
			}
		}
		[ThreadSafe]
		static IEnumerable<string> powerBiReportFiles;

		static IEnumerable<string> PowerBiReportFiles_CheckedOut
		{
			get
			{
				using (var sourceControl = SourceControlFactory.Instance.GetSourceControl())
				{
					return powerBiReportFiles_CheckedOut ??
						(powerBiReportFiles_CheckedOut = sourceControl.GetFilesWithPendingChanges().Where(f => PowerBiReportFiles.Contains(f)));
				}
			}
		}
		[ThreadSafe]
		static IEnumerable<string> powerBiReportFiles_CheckedOut;

		static IEnumerable<string> CustomVisualFiles
		{
			get
			{
				using (var sourceControl = SourceControlFactory.Instance.GetSourceControl())
				{
					var allCustomVisualFiles = Directory.EnumerateFiles(CustomVisualFileDirectory, "*.pbiviz", SearchOption.AllDirectories);
					return customVisualFiles ??
						(customVisualFiles = sourceControl.GetFilesWithPendingChanges().Where(f => allCustomVisualFiles.Contains(f)));
				}
			}
		}
		[ThreadSafe]
		static IEnumerable<string> customVisualFiles;

		static string PowerBiReportFileDirectory
		{
			get
			{
				return powerBiReportFileDirectory ??
					(powerBiReportFileDirectory = Path.Combine(BuildConstants.LocalEnterprisePath, @"BusinessIntelligence\CargoWiseBi\CargoWiseBi.PBIRS\Reports"));
			}
		}
		[ThreadSafe]
		static string powerBiReportFileDirectory;

		static string CustomVisualFileDirectory
		{
			get
			{
				return customVisualFileDirectory ??
					(customVisualFileDirectory = Path.Combine(BuildConstants.LocalEnterprisePath, @"BusinessIntelligence\CargoWiseBi\CargoWiseBi.PBIRS\CustomVisualization"));
			}
		}
		[ThreadSafe]
		static string customVisualFileDirectory;

		static string RegistrationProjectFilePath
		{
			get
			{
				return registrationProjectFilePath ??
					(registrationProjectFilePath = Path.Combine(BuildConstants.LocalEnterprisePath, @"BusinessIntelligence\BiIntegration\Deployment\CargoWiseBiDeployment\Registration\Registration.csproj"));
			}
		}
		[ThreadSafe]
		static string registrationProjectFilePath;

		#endregion

		#endregion
	}
}
