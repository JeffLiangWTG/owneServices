using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.Common;

namespace CargoWise.BuildTools
{
	/// <summary>
	/// The "Build.xml" file that specifies all of the solutions, assemblies and business objects.
	/// </summary>
	[SuppressMessage("CargoWiseOne", "CW1051:BaseSourcePath", Justification = "On DAT read from the Bin")]
	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer only tool")]
	public class BuildXml
	{
		const string BusinessObjectRootNode = "generator";
		const string SolutionsRootNode = "build";

		public BuildXml(string buildXmlFilePath, bool flattenSubmodules = true)
			: this(flattenSubmodules)
		{
			Argument.NotNullOrEmpty(buildXmlFilePath, nameof(buildXmlFilePath));

			if (!File.Exists(buildXmlFilePath))
			{
				throw new FileNotFoundException("buildXmlFilePath", buildXmlFilePath);
			}

#if DEBUG
			if (buildXmlFilePath.StartsWith(@"\\") && Path.GetFileName(buildXmlFilePath).Equals(BuildConstants.BuildXmlFileName, StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidOperationException($"Attempted to read Build.xml over a network share: {buildXmlFilePath}. Use BuildXml.Instance instead to load from the bin directory when running on DAT.");
			}
#endif

			this.buildXmlFilePath = buildXmlFilePath;
		}

		readonly string buildXmlFilePath = "";

		internal BuildXml(bool flattenSubmodules = true)
		{
			lazyNotDeployToClientsFiles = new Lazy<HashSet<string>>(GetNotDeployToClientsFiles);
			this.flattenSubmodules = flattenSubmodules;

			HashSet<string> GetNotDeployToClientsFiles()
			{
				var xpath = new[]
				{
					"//build:Solutions/build:Solution/build:Bin[(@DeployToClients='false')]",
					"//build:OtherFiles/build:Filename[(@DeployToClients='false')]",
				};

				return new HashSet<string>(SelectNodesWithNamespace(string.Join("|", xpath))
					.Cast<XmlNode>()
					.Select(node => node.InnerText)
					.Concat(
						SelectNodesWithNamespace("//build:Dependencies/build:Dependency/build:Copy[(@DeployToClients='false') and not (@Target)]")
						.Cast<XmlNode>()
						.Select(node => Path.GetFileName(node.Attributes["Source"].Value)))
					.Concat(GetOtherFilesDirectoryCopyNotDeployToClients())
					.Select(value => value.Trim()));
			}

			IEnumerable<string> GetOtherFilesDirectoryCopyNotDeployToClients()
			{
				var xpath = new[]
				{
					"//build:OtherFiles/build:Directory[(@CopyFrom and @DeployToClients='false' and text()='.')]",
					"//build:OtherFiles/build:Directory[(@CopyFrom and not(text()='.'))]",
				};

				var sourcePath = Path.GetDirectoryName(BuildXmlFileName);
				var directoryNodes = SelectNodesWithNamespace(string.Join("|", xpath)).Cast<XmlNode>();
				foreach (var directoryNode in directoryNodes)
				{
					foreach (var file in GetFilesFromCopyFromDirectory(directoryNode.Attributes["CopyFrom"].Value, sourcePath))
					{
						yield return Path.GetFileName(file);
					}
				}
			}
		}

		#region Instance

		public void Release()
		{
			instance = null;
		}

		public static BuildXml Instance
		{
			[SuppressMessage("Microsoft.Contracts", "Ensures-Contract.Result<BuildXml>() != null")] //When DEBUG is fined, CC Static Analyzer thinks ensure is unreachable
			get
			{
#if DEBUG
				if (instanceForTest != null)
				{
					return instanceForTest;
				}
#endif
				if (instance == null)
				{
					instance = new BuildXml();
				}
				return instance;
			}
		}
		[ThreadStatic]
		static BuildXml instance;

#if DEBUG
		[ThreadStatic]
		static BuildXml instanceForTest;

		public static bool HasInstanceForTest
		{
			get { return instanceForTest != null; }
		}

		public static void SetInstanceForTesting(BuildXml instanceForTest)
		{
			if (BuildXml.instanceForTest != null)
			{
				throw new InvalidOperationException("You cannot call SetInstanceForTesting more than once for a testcase.");
			}

			BuildXml.instanceForTest = instanceForTest;
		}

		public static void RemoveTestingInstance()
		{
			if (BuildXml.instanceForTest == null)
			{
				throw new InvalidOperationException("You must call SetInstanceForTesting before calling this method.");
			}

			instanceForTest = null;
		}

		/// <summary>
		/// Initialises a <see cref="BuildXml"/> instance using the content of the Build.xml file in the source code directory, as opposed to the bin directory.
		/// The version in the source code directory is capable of recursing through the nested submodules to include their content.
		/// </summary>
		/// <param name="flattenSubmodules">When true, the contents of the &lt;Submodules&gt; element will be used to recursively load the Build.xml files for each submodule and flatten their contents into this <see cref="BuildXml"/> instance.</param>
		public static BuildXml CreateFromSourceCodeDirectory(bool flattenSubmodules = true)
		{
			return new(SourceCodeBuildXmlFilePath, flattenSubmodules);
		}

		static string SourceCodeBuildXmlFilePath => Path.Combine(WTG.TestHelpers.TestCase.BaseSourcePath, BuildConstants.BuildXmlFileName);

#endif
		#endregion

		public bool NGen(string outputFileName)
		{
			Argument.NotNull(outputFileName, nameof(outputFileName));
			return
				!outputFileName.StartsWith("ZClient", StringComparison.OrdinalIgnoreCase)
				&& GetFlag(
					outputFileName,
					"NGen",
					!lazyNotDeployToClientsFiles.Value.Contains(outputFileName));
		}

		public bool UnitTest(string outputFileName)
		{
			Argument.NotNull(outputFileName, nameof(outputFileName));
			return GetFlag(outputFileName, "UnitTest", true);
		}

		public bool IsDuplicatedCodeAssembly(string outputFileName)
		{
			Argument.NotNull(outputFileName, nameof(outputFileName));
			return GetFlag(outputFileName, "DuplicatedCodeAssembly", false);
		}

		bool GetFlag(string outputFileName, string attributeName, bool defaultValue)
		{
			Argument.NotNull(outputFileName, nameof(outputFileName));
			Argument.NotNull(attributeName, nameof(attributeName));
			return GetFlag(outputFileName, attributeName) ?? defaultValue;
		}

		bool? GetFlag(string outputFileName, string attributeName)
		{
			Argument.NotNull(outputFileName, nameof(outputFileName));
			Argument.NotNull(attributeName, nameof(attributeName));
			bool? result = null;
			XmlNodeList elements = SolutionNodes("/build:Bin[.='" + outputFileName + "']");
			if (elements.Count > 0)
			{
				XmlElement element = elements.Item(0) as XmlElement;
				if (element != null)
				{
					string attribute = element.GetAttribute(attributeName);
					if (!string.IsNullOrEmpty(attribute))
					{
						result = bool.Parse(attribute);
					}
				}
			}
			return result;
		}

		public string GetMSBuildVersion()
		{
			return Document.FirstChild?.Attributes["MSBuild"]?.Value;
		}

		#region AllAssembliesDeployedToClient

		public string[] GetAllAssembliesDeployedToClient(string sourceDirectory)
		{
			Argument.NotNullOrEmpty(sourceDirectory, nameof(sourceDirectory));
			return GetAllAssemblies(true, sourceDirectory);
		}

		internal string[] GetAllAssemblies(bool deployedToClientsOnly, string sourceDirectory)
		{
			Argument.NotNullOrEmpty(sourceDirectory, nameof(sourceDirectory));
			List<string> result = new List<string>();
			result.AddRange(GetAllAssembliesToBuild(deployedToClientsOnly));
			result.AddRange(GetOtherDeployedFiles(sourceDirectory));
			result.AddRange(GetDependencyFiles(deployedToClientsOnly));
			result.AddRange(GetWebPackageFiles());
			return result.ToArray();
		}

		public string[] GetAllAssembliesToBuild(bool deployedToClientsOnly)
		{
			List<string> result = new List<string>();
			foreach (string solutionName in GetAllSolutionFileNames().Where(x => !string.IsNullOrEmpty(x)))
			{
				result.AddRange(GetAssembliesInSolution(solutionName, deployedToClientsOnly));
			}
			return result.ToArray();
		}

		internal List<string> GetAssembliesDeployedInSolution(string solutionFileName)
		{
			Argument.NotNull(solutionFileName, nameof(solutionFileName));
			return GetAssembliesInSolution(solutionFileName, true);
		}

		public List<string> GetAllAssembliesInSolution(string solutionFileName)
		{
			Argument.NotNull(solutionFileName, nameof(solutionFileName));
			return GetAssembliesInSolution(solutionFileName, false);
		}

		List<string> GetAssembliesInSolution(string solutionFileName, bool deployedToClientsOnly)
		{
			Argument.NotNull(solutionFileName, nameof(solutionFileName));
			string xpath = "/build:Bin";
			if (deployedToClientsOnly)
			{
				xpath += "[not(@DeployToClients = 'false')]";
			}
			return GetMatchingAssemblyNames(xpath, solutionFileName);
		}

		internal List<string> GetOtherDeployedFiles(string sourceDirectory)
		{
			Argument.NotNullOrEmpty(sourceDirectory, nameof(sourceDirectory));
			List<string> result = new List<string>();

			foreach (XmlElement node in SelectNodesWithNamespace("//build:OtherFiles/build:Filename[not(@DeployToClients = 'false')]"))
			{
				result.Add(node.InnerText.Trim());
			}

			foreach (XmlElement node in SelectNodesWithNamespace("//build:OtherFiles/build:Directory[not(@DeployToClients = 'false')]"))
			{
				string copyFromDirectory = node.GetAttribute("CopyFrom");
				foreach (var fileName in GetFilesFromCopyFromDirectory(copyFromDirectory, sourceDirectory))
				{
					result.Add(fileName);
				}
			}

			return result;
		}

		public HashSet<string> GetNotDeployToClientsFiles()
		{
			var xpath = new[]
			{
				"//build:Solutions/build:Solution/build:Bin[(@DeployToClients = 'false')]",
				"//build:OtherFiles/build:Filename[(@DeployToClients = 'false')]",
			};

			return SelectNodesWithNamespace(string.Join("|", xpath))
				.Cast<XmlNode>()
				.Select(node => node.InnerText)
				.Concat(SelectNodesWithNamespace("//build:Dependencies/build:Dependency/build:Copy[(@DeployToClients = 'false')]")
					.Cast<XmlNode>()
					.Select(node => Path.GetFileName(node.Attributes["Source"].Value)))
				.Select(value => value.Trim())
				.ToHashSet(StringComparer.OrdinalIgnoreCase);
		}

		public Dictionary<string, string> GetOtherDeployedFilesWithCopyFrom(string sourceDirectory)
		{
			Argument.NotNullOrEmpty(sourceDirectory, nameof(sourceDirectory));
			Dictionary<string, string> result = new Dictionary<string, string>();

			foreach (XmlElement node in SelectNodesWithNamespace("//build:OtherFiles/build:Filename"))
			{
				string copyFrom = GetCopyFromPath(node);
				if (!string.IsNullOrEmpty(copyFrom))
				{
					result.Add(node.InnerText.Trim(), Path.Combine(copyFrom, Path.GetFileName(node.InnerText.Trim())));
				}
			}

			foreach (XmlElement node in SelectNodesWithNamespace("//build:OtherFiles/build:Directory"))
			{
				string copyFromDirectory = GetCopyFromPath(node);
				string copyToDirectory = node.GetAttribute("CopyTo");
				if (Path.IsPathRooted(copyToDirectory))
				{
					throw new ArgumentException("Rooted paths are not allowed for destination folders");
				}

				foreach (var fileName in GetFilesFromCopyFromDirectory(copyFromDirectory, sourceDirectory))
				{
					if (!Path.IsPathRooted(copyFromDirectory))
					{
						copyFromDirectory = Path.Combine(sourceDirectory, copyFromDirectory);
					}

					result.Add(Path.Combine(copyToDirectory, fileName.Replace(copyFromDirectory + "\\", "")), fileName);
				}
			}

			return result;
		}

		public IEnumerable<string> GetDependencyFiles(bool deployedToClientsOnly)
		{
			var dependencyFiles = new List<string>();
			string xpath = "//build:Dependencies/build:Dependency/build:Copy";
			if (deployedToClientsOnly)
			{
				xpath += "[not(@DeployToClients = 'false')]";
			}
			foreach (XmlElement element in SelectNodesWithNamespace(xpath))
			{
				var file = Path.GetFileName(element.GetAttribute("Source"));
				var target = element.GetAttribute("Target");
				if (!string.IsNullOrEmpty(target))
				{
					file = Path.Combine(target, file);
				}
				dependencyFiles.Add(file);
			}
			return dependencyFiles;
		}

		public IEnumerable<KeyValuePair<string, string>> GetDependencies()
		{
			var result = SelectNodesWithNamespace("//build:Dependencies/build:Dependency")
				.Cast<XmlElement>()
				.Select(element => new
				{
					repository = element.GetAttribute("Repository"),
					path = element.GetAttribute("Path"),
				})
				.Select(arg => new KeyValuePair<string, string>(arg.repository, arg.path))
				.ToArray();
			return result;
		}

		[SuppressMessage("Microsoft.Contracts", "Ensures-Contract.ForAll(Contract.Result<IEnumerable<String>>(), x => x != null)")] //CC static analyzer can't prove ForAlls
		public IEnumerable<string> GetAllRedirectionFiles(string sourceDirectory)
		{
			Argument.NotNull(sourceDirectory, nameof(sourceDirectory));
			HashSet<string> redirctionFiles = new HashSet<string>();
			foreach (XmlElement node in SelectNodesWithNamespace("//build:OtherFiles/build:Filename|//build:OtherFiles/build:Directory"))
			{
				string copyFrom = node.GetAttribute("CopyFrom");
				if (!string.IsNullOrEmpty(copyFrom))
				{
					var match = redirectionRegex.Match(copyFrom);
					if (match.Success)
					{
						redirctionFiles.Add(match.Groups["path"].Value);
					}
				}
			}
			return redirctionFiles;
		}

		string GetCopyFromPath(XmlElement node)
		{
			Argument.NotNull(node, nameof(node));
			string copyFrom = node.GetAttribute("CopyFrom");
			if (!string.IsNullOrEmpty(copyFrom))
			{
				copyFrom = redirectionRegex.Replace(copyFrom, (Match match) =>
					{
						return File.ReadAllText(Path.Combine(Path.GetDirectoryName(BuildXmlFileName), match.Groups["path"].Value));
					});
			}
			return copyFrom;
		}
		readonly Regex redirectionRegex = new Regex(@"\@\/(?<path>.*?)\/");

		public IList<KeyValuePair<string, Version>> GetStrictlyVersionedOtherFiles()
		{
			var result = new List<KeyValuePair<string, Version>>();

			foreach (XmlElement node in SelectNodesWithNamespace("//build:OtherFiles/build:Filename[@Version]"))
			{
				var versionString = node.GetAttribute("Version");
				var fileName = node.InnerText.Trim();
				Version version;

				if (!Version.TryParse(versionString, out version))
				{
					throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Version specifier '{0}' is invalid. (File: {1})", versionString, fileName));
				}

				result.Add(new KeyValuePair<string, Version>(fileName, version));
			}

			return result;
		}

		[SuppressMessage("Microsoft.Contracts", "Ensures-Contract.ForAll(Contract.Result<String[]>(), fileName => !String.IsNullOrEmpty(fileName))")] //I tried with assumes but they don't work for some reason.
		string[] GetFilesFromCopyFromDirectory(string copyFromDirectory, string sourceDirectory)
		{
			Argument.NotNullOrEmpty(sourceDirectory, nameof(sourceDirectory));
			Argument.NotNullOrEmpty(copyFromDirectory, nameof(copyFromDirectory));
			if (!Path.IsPathRooted(copyFromDirectory))
			{
				copyFromDirectory = Path.Combine(sourceDirectory, copyFromDirectory);
			}

			var result = Directory.GetFiles(copyFromDirectory, "*", SearchOption.AllDirectories);
			return result;
		}

		[SuppressMessage("Microsoft.Contracts", "Ensures-Contract.ForAll(Contract.Result<List<String>>(), fileName => !string.IsNullOrEmpty(fileName))")] //CC static analyzer can't prove this.
		public List<string> GetWebPackageFiles()
		{
			List<string> result = new List<string>();
			foreach (XmlElement node in SelectNodesWithNamespace("//build:WebSolution"))
			{
				if (node.Attributes["Client"] != null && node.Attributes["Client"].Value != "ALL")
				{
					string fileName = "ZClientWeb" + node.Attributes["Client"].Value + ".zip";
					if (result.IndexOf(fileName) == -1)
					{
						result.Add(fileName.Trim());
					}
				}
			}
			return result;
		}

		public IEnumerable<string> GetWebDestFolders()
		{
			foreach (XmlElement node in SelectNodesWithNamespace("//build:WebSolution"))
			{
				if (string.IsNullOrEmpty(node.GetAttribute("Client")))
				{
					yield return node.GetAttribute("DestFolder");
				}
			}
		}

		#endregion

		public IEnumerable<string> ExcludedTables
		{
			get
			{
				return SelectNodesWithNamespace("//build:ExcludedTables/build:ExcludedTable").OfType<XmlElement>().Select(x => x.Attributes["Table"].Value);
			}
		}

		public IEnumerable<string> ExcludedColumns
		{
			get
			{
				return SelectNodesWithNamespace("//build:ExcludedColumns/build:ExcludedColumn").OfType<XmlElement>().Select(x => x.Attributes["Column"].Value);
			}
		}

		public IEnumerable<string> SchemaOnlyViews
			=> SelectNodesWithNamespace("//build:SchemaOnlyEntities/build:SchemaOnly").OfType<XmlElement>().Select(x => x.Attributes["View"].Value);

		public Dictionary<string, (string SourceTable, string SourceColumn)> BackingColumns
		{
			get
			{
				if (backingColumns == null)
				{
					backingColumns = new Dictionary<string, (string SourceTable, string SourceColumn)>();

					foreach (var node in SelectNodesWithNamespace("//build:BackingColumns/build:BackingColumn").OfType<XmlElement>())
					{
						var column = node.Attributes["Column"]?.Value;
						var sourceTable = node.Attributes["SourceTable"]?.Value;
						var sourceColumn = node.Attributes["SourceColumn"]?.Value;
						backingColumns.Add(column, (sourceTable, sourceColumn));
					}
				}

				return backingColumns;
			}
		}
		Dictionary<string, (string SourceTable, string SourceColumn)> backingColumns;

		#region All Business Objects

		public BuildXmlBizOEntryCollection AllBusinessObjects
		{
			get
			{
				if (allBusinessObjects == null)
				{
					allBusinessObjects = new BuildXmlBizOEntryCollection();

					foreach (XmlElement node in SelectNodesWithNamespace("//build:BusinessObject"))
					{
						XmlAttribute refDbCountryNode = node.Attributes["ReferenceDatabaseCountry"];
						XmlAttribute refDbTypeNode = node.Attributes["ReferenceDatabaseType"];
						XmlAttribute tableNode = node.Attributes["Table"];
						XmlAttribute solutionNode = node.Attributes["Solution"];
						XmlAttribute preventDeleteNode = node.Attributes["PreventDelete"];
						XmlAttribute masterFileReference = node.Attributes["MasterFileReference"];
						XmlAttribute convertZStringToWesternEuropeanCharactersNode = node.Attributes["ConvertZStringToWesternEuropeanCharacters"];

						string refDbCountry = (refDbCountryNode == null) ? null : refDbCountryNode.Value;
						string refDbType = (refDbTypeNode == null) ? null : refDbTypeNode.Value;
						bool autoGenerate = (masterFileReference != null) && GetBooleanValueFromXmlNode(masterFileReference);
						bool preventDelete = preventDeleteNode == null || GetBooleanValueFromXmlNode(preventDeleteNode);
						bool convertZStringToWesternEuropeanCharacters = convertZStringToWesternEuropeanCharactersNode != null && GetBooleanValueFromXmlNode(convertZStringToWesternEuropeanCharactersNode);

						if (tableNode == null || string.IsNullOrEmpty(tableNode.Value))
						{
							throw new ArgumentException(string.Format("Attribute 'Table' was null or empty."));
						}

						if (solutionNode == null || string.IsNullOrEmpty(solutionNode.Value))
						{
							throw new ArgumentException(string.Format("Attribute 'Solution' was null or empty."));
						}

						var addInfos = SelectNodesWithNamespace("build:AddInfo", node).Cast<XmlElement>().Select(addInfoNode =>
						{
							var addInfoView = addInfoNode.Attributes["View"];
							var addInfoSolutionNode = addInfoNode.Attributes["Solution"];
							var oldPrefix = addInfoNode.Attributes["OldPrefix"];
							var parentSchema = addInfoNode.Attributes["ParentSchema"];
							var childTableName = addInfoNode.Attributes["ChildTableName"];
							var childForeignKey = addInfoNode.Attributes["ChildForeignKey"];
							return new BuildXmlAddInfoEntry(addInfoView?.Value, addInfoSolutionNode?.Value, oldPrefix?.Value, parentSchema?.Value, childTableName?.Value, childForeignKey?.Value);
						}).ToList();

						allBusinessObjects.Add(
							refDbCountry, refDbType, tableNode.Value, solutionNode.Value,
							autoGenerate, preventDelete, convertZStringToWesternEuropeanCharacters, addInfos);
					}
				}

				return allBusinessObjects;
			}
		}

		bool GetBooleanValueFromXmlNode(XmlNode node)
		{
			Argument.NotNull(node, nameof(node)); // Suggested By ReviewBot
			bool result;
			string nodeValue = node.Value != null ? node.Value.ToLower() : "null";
			if (nodeValue == "true" || nodeValue == "t" || nodeValue == "y")
			{
				result = true;
			}
			else if (nodeValue == "false" || nodeValue == "f" || nodeValue == "n")
			{
				result = false;
			}
			else
			{
				throw new ArgumentException(string.Format(
					"The node '{0}' for has the invalid value {1} (must be true or false).", node.Name, node.Value));
			}

			return result;
		}

		BuildXmlBizOEntryCollection allBusinessObjects;

		#endregion

		public string[] GetAllTestedAssemblies()
		{
			List<string> allDlls = new List<string>();
			foreach (string solutionName in GetAllSolutionFileNames())
			{
				allDlls.AddRange(GetAssembliesTestedInSolution(solutionName));
			}

			return allDlls.ToArray();
		}

		public string[] GetAllSolutionFileNames()
		{
			return SolutionFileNames("").ToArray();
		}

		public string[] GetSniffedSolutionFileNames()
		{
			return SolutionFileNames("[not(@Sniff = 'false')]").ToArray();
		}

		public string[] GetSolutionFileNamesNotIncludingClientSpecific()
		{
			return SolutionFileNames("[not(contains(@Filename,'ZClient'))]").ToArray();
		}

		public string GetFileNameOfSolution(string solutionName)
		{
			string result = "";
			if (solutionName != null)
			{
				solutionName = solutionName.ToLower();

				foreach (string solutionFileName in GetAllSolutionFileNames())
				{
					if (string.Equals(Path.GetFileNameWithoutExtension(solutionFileName), solutionName, StringComparison.OrdinalIgnoreCase))
					{
						result = solutionFileName;
						break;
					}
				}
			}
			return result;
		}

		public string[] GetAssembliesTestedInSolution(string solutionFileName)
		{
			return GetMatchingAssemblyNames("/build:Bin[not(@UnitTest = 'false')]", solutionFileName).ToArray();
		}

		public bool HasRuntimePackageDefinition()
		{
			return SelectNodesWithNamespace("//build:RuntimePackage") != null;
		}

		public string[] GetRuntimePackageDistributionInstallDirectories()
		{
			List<string> result = new List<string>();
			foreach (XmlElement node in SelectNodesWithNamespace("//build:RuntimePackage/build:DistributionInstallDirectory"))
			{
				result.Add(node.InnerText.Trim());
			}
			return result.ToArray();
		}

		public string BuildXmlFileName
		{
			get
			{
				if (buildXmlFilePath.Length > 0)
				{
					return buildXmlFilePath;
				}
				else
				{
#if DEBUG
					if (WTG.TestHelpers.TestingState.IsRunningOnDAT)
					{
						var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
#if NET
						var rootBin = Directory.GetParent(assemblyLocation)!.FullName;
						return Path.Combine(rootBin, BuildConstants.BuildXmlFileName);
#else
						return Path.Combine(assemblyLocation, BuildConstants.BuildXmlFileName);
#endif
					}
					else
					{
						return SourceCodeBuildXmlFilePath;
					}
#else
					return BuildConstants.GetLocalPath(BuildConstants.BuildXmlFileName);
#endif
				}
			}
		}

		#region Implementation

		internal XmlNodeList SelectNodesWithNamespace(string xpathExpression, XmlNode node = null)
		{
			Argument.NotNull(xpathExpression, nameof(xpathExpression));
			var buildNamespaceManager = NewXmlNamespaceManager(Document);
			var result = (node ?? Document).SelectNodes(xpathExpression, buildNamespaceManager);
			return result;
		}

		XmlNamespaceManager NewXmlNamespaceManager(XmlDocument document, string nameSpace = null)
		{
			var buildNamespaceManager = new XmlNamespaceManager(document.NameTable);
			var documentElement = document.DocumentElement;
			buildNamespaceManager.AddNamespace(nameSpace ?? "build", documentElement.NamespaceURI);
			return buildNamespaceManager;
		}

		List<string> SolutionFileNames(string xpathSuffix)
		{
			Argument.NotNull(xpathSuffix, nameof(xpathSuffix));
			List<string> result = new List<string>();
			foreach (XmlElement node in SolutionNodes(xpathSuffix))
			{
				result.Add(node.Attributes["Filename"].Value);
			}
			return result;
		}

		List<string> GetMatchingAssemblyNames(string xPath, string solutionFileName)
		{
			Argument.NotNull(xPath, nameof(xPath));
			List<string> result = new List<string>();

			if (solutionFileName != null && !string.IsNullOrEmpty(solutionFileName))
			{
				xPath = "[contains(@Filename,'" + solutionFileName + "')]" + xPath;
			}

			foreach (XmlElement node in SolutionNodes(xPath))
			{
				result.Add(node.InnerText.Trim());
			}

			return result;
		}

		XmlNodeList SolutionNodes(string xpathSuffix)
		{
			Argument.NotNull(xpathSuffix, nameof(xpathSuffix));
			return SelectNodesWithNamespace("//build:Solution" + xpathSuffix);
		}

		XmlDocument Document
		{
			get
			{
				if (document == null)
				{
					if (!File.Exists(BuildXmlFileName))
					{
						throw new ApplicationException(BuildXmlFileName + " does not exist");
					}
					document = new XmlDocument();
					document.Load(BuildXmlFileName);

					// load from Solutions.xml if exists
					LoadElementsFromExternalXml(document, BuildConstants.SolutionXmlFileName, new string[] { "Solutions" }, SolutionsRootNode);
					// load from BusinessObjects.xml if exists
					LoadElementsFromExternalXml(
						document,
						BuildConstants.BusinessObjectsXmlFileName,
						new string[] {
							"ExcludedTables",
							"SchemaOnlyEntities",
							"ExcludedColumns",
							"BackingColumns",
							"BusinessObjects" },
						BusinessObjectRootNode);

					LoadFromSubModules(document);
				}

				return document;
			}
		}

		XmlDocument document;

		void LoadElementsFromExternalXml(XmlDocument document, string fileName, string[] nodeTypes, string rootNode = null)
		{
			// do not handle the same file
			if (Path.GetFileName(BuildXmlFileName) == fileName)
			{
				return;
			}

			var xmlPath = Path.Combine(Path.GetDirectoryName(BuildXmlFileName), fileName);

			if (!File.Exists(xmlPath))
			{
				return;
			}

			var externalDocument = new XmlDocument();
			externalDocument.Load(xmlPath);

			var buildNamespaceManager = NewXmlNamespaceManager(document);
			var externalNamespaceManager = NewXmlNamespaceManager(externalDocument, rootNode);

			var buildNode = document.SelectSingleNode("/build:Build", buildNamespaceManager);
			if (buildNode == null)
			{
				return;
			}

			ProcessNodeTypes(nodeTypes, buildNode, externalDocument, buildNamespaceManager, externalNamespaceManager, rootNode, fileName);
		}

		void ProcessNodeTypes(
			string[] nodeTypes,
			XmlNode buildNode,
			XmlDocument externalDocument,
			XmlNamespaceManager buildNamespaceManager,
			XmlNamespaceManager externalNamespaceManager,
			string rootNode,
			string fileName)
		{
			// Process each requested node type (e.g., Solutions, BusinessObjects, etc.)
			foreach (var nodeType in nodeTypes)
			{
				// Find this node type in the external XML document
				var externalNode = externalDocument.SelectSingleNode($"//{rootNode}:{nodeType}", externalNamespaceManager);
				if (externalNode == null)
				{
					continue;
				}

				var destinationNode = buildNode.SelectSingleNode($"build:{nodeType}", buildNamespaceManager);
				if (destinationNode != null)
				{
					throw new InvalidOperationException(
						$"Found '{nodeType}' node in Build.xml when it should only exist in its dedicated external XML file. " +
						$"Please remove this section from Build.xml as it's now managed in {fileName}.");
				}

				var correctedNode = ImportNodeWithNamespaceCorrection(document, externalNode, buildNode.NamespaceURI);
				buildNode.AppendChild(correctedNode);
			}
		}

		XmlNode ImportNodeWithNamespaceCorrection(XmlDocument targetDoc, XmlNode sourceNode, string targetNamespace)
		{
			var importedNode = targetDoc.ImportNode(sourceNode, true);

			// Create a new element with the correct namespace
			if (importedNode is XmlElement importedElement)
			{
				var newElement = targetDoc.CreateElement(importedElement.LocalName, targetNamespace);

				foreach (XmlAttribute attr in importedElement.Attributes.Cast<XmlAttribute>().ToList())
				{
					if (!attr.Name.StartsWith("xmlns"))
					{
						newElement.SetAttribute(attr.LocalName, attr.Value);
					}
				}

				// Set inner XML to copy all content with namespace correction
				if (!string.IsNullOrEmpty(importedElement.InnerXml))
				{
					newElement.InnerXml = importedElement.InnerXml;
					FixChildElementNamespaces(newElement, targetNamespace);
				}

				return newElement;
			}

			return importedNode;
		}

		void FixChildElementNamespaces(XmlElement parentElement, string targetNamespace)
		{
			var elementsToFix = new List<XmlElement>();

			// Collect all child elements that need namespace fixing
			foreach (XmlNode child in parentElement.ChildNodes)
			{
				if (child is XmlElement childElement && string.IsNullOrEmpty(childElement.NamespaceURI))
				{
					elementsToFix.Add(childElement);
				}
			}

			foreach (var element in elementsToFix)
			{
				var newElement = parentElement.OwnerDocument.CreateElement(element.LocalName, targetNamespace);

				foreach (XmlAttribute attr in element.Attributes.Cast<XmlAttribute>().ToList())
				{
					if (!attr.Name.StartsWith("xmlns"))
					{
						newElement.SetAttribute(attr.LocalName, attr.Value);
					}
				}

				newElement.InnerXml = element.InnerXml;
				parentElement.ReplaceChild(newElement, element);

				FixChildElementNamespaces(newElement, targetNamespace);
			}
		}

		IEnumerable<(string subModulePath, BuildXml subModuleBuildXml)> GetSubModuleBuildXmls(XmlDocument document, string mainPath)
		{
			var buildNamespaceManager = NewXmlNamespaceManager(document);
			if (document.SelectSingleNode("//build:Submodules", buildNamespaceManager) is XmlNode submodulesNode)
			{
				foreach (XmlNode subModuleNode in submodulesNode.SelectNodes("build:Submodule", buildNamespaceManager))
				{
					var subModuleBuildXmlPath = subModuleNode.InnerText.Trim();
					if (!string.IsNullOrEmpty(subModuleBuildXmlPath))
					{
						var subModuleBuildXmlFileName = Path.Combine(Path.Combine(mainPath, subModuleBuildXmlPath), BuildConstants.BuildXmlFileName);
						if (File.Exists(subModuleBuildXmlFileName))
						{
							yield return (subModuleBuildXmlPath, new BuildXml(subModuleBuildXmlFileName));
						}
					}
				}
			}
		}

		void LoadFromSubModules(XmlDocument document)
		{
			if (!flattenSubmodules)
			{
				return;
			}
			var buildNamespaceManager = NewXmlNamespaceManager(document);
			if (document.SelectSingleNode("/build:Build", buildNamespaceManager) is XmlNode buildNode)
			{
				XmlNode solutionsNode = null;
				XmlNode businessObjectsNode = null;
				XmlNode excludedTablesNode = null;
				XmlNode schemaOnlyEntities = null;
				CopyNodesFromSubModules(document, GetSubModuleBuildXmls(document, Path.GetDirectoryName(BuildXmlFileName)), buildNamespaceManager, buildNode, ref solutionsNode, ref businessObjectsNode, ref excludedTablesNode, ref schemaOnlyEntities, string.Empty);
			}
		}

		void CopyNodesFromSubModules(XmlDocument document, IEnumerable<(string subModulePath, BuildXml subModuleBuildXml)> subModulesData, XmlNamespaceManager buildNamespaceManager, XmlNode buildNode, ref XmlNode solutionsNode, ref XmlNode businessObjectsNode, ref XmlNode excludedTablesNode, ref XmlNode schemaOnlyEntities, string filenamePrefixPath)
		{
			foreach ((string subModulePath, BuildXml subModuleBuildXml) in subModulesData)
			{
				var subModuleDocument = subModuleBuildXml.Document;
				var subModuleBuildNamespaceManager = NewXmlNamespaceManager(subModuleDocument);
				if (subModuleDocument.SelectSingleNode("/build:Build", subModuleBuildNamespaceManager) is XmlNode subModuleBuildNode)
				{
					solutionsNode = CopyNodes("Solutions", "Solution", document, buildNamespaceManager, buildNode, solutionsNode, subModuleBuildNamespaceManager, subModuleBuildNode, (newSolutionNode) =>
					{
						var filenameAttribute = newSolutionNode.Attributes["Filename"];
						filenameAttribute.Value = Path.Combine(Path.Combine(filenamePrefixPath, subModulePath), filenameAttribute.Value);
					});
					businessObjectsNode = CopyNodes("BusinessObjects", "BusinessObject", document, buildNamespaceManager, buildNode, businessObjectsNode, subModuleBuildNamespaceManager, subModuleBuildNode);
					excludedTablesNode = CopyNodes("ExcludedTables", "ExcludedTable", document, buildNamespaceManager, buildNode, excludedTablesNode, subModuleBuildNamespaceManager, subModuleBuildNode);
					schemaOnlyEntities = CopyNodes("SchemaOnlyEntities", "SchemaOnly", document, buildNamespaceManager, buildNode, schemaOnlyEntities, subModuleBuildNamespaceManager, subModuleBuildNode);
				}
			}
		}

		static XmlNode CopyNodes(string parentNodeName, string childNodeName, XmlDocument document, XmlNamespaceManager buildNamespaceManager, XmlNode buildNode, XmlNode destinationNodes, XmlNamespaceManager subModuleBuildNamespaceManager, XmlNode subModuleBuildNode, Action<XmlNode> setAdditionalData = null)
		{
			if (subModuleBuildNode.SelectSingleNode("build:" + parentNodeName, subModuleBuildNamespaceManager) is XmlNode subModuleSolutionsNode)
			{
				foreach (XmlNode subModuleSolutionNode in subModuleSolutionsNode.SelectNodes("build:" + childNodeName, subModuleBuildNamespaceManager))
				{
					if (destinationNodes == null)
					{
						destinationNodes = buildNode.SelectSingleNode("build:" + parentNodeName, buildNamespaceManager);
						if (destinationNodes == null)
						{
							destinationNodes = buildNode.OwnerDocument.CreateElement(subModuleSolutionsNode.Name);
							buildNode.AppendChild(destinationNodes);
						}
					}
					var newDestinationNode = document.ImportNode(subModuleSolutionNode, true);
					setAdditionalData?.Invoke(newDestinationNode);
					destinationNodes.AppendChild(newDestinationNode);
				}
			}
			return destinationNodes;
		}

		readonly Lazy<HashSet<string>> lazyNotDeployToClientsFiles;
		readonly bool flattenSubmodules;

		#endregion
	}
}
