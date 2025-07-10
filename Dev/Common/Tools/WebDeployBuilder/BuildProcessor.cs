using System.Diagnostics.CodeAnalysis;
using System.Xml;

using Enterprise.ZArchitecture.Core;

namespace WebDeployBuilder
{
	/// <summary>
	/// Processes WebDeploy.Xml file and moves all files from relevant projects
	/// into a temporary deployment folder
	/// </summary>
	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer only tool")]
	public class BuildProcessor
	{
		readonly XmlNamespaceManager BuildNamespaceManager;

		public BuildProcessor(string xmlFilePath)
		{
			XmlFileName = xmlFilePath;

			BuildNamespaceManager = new XmlNamespaceManager(Document.NameTable);
			if (Document.DocumentElement != null)
			{
				BuildNamespaceManager.AddNamespace("build", Document.DocumentElement.NamespaceURI);
			}
		}

		/// <summary>
		/// Copies a directory and its contents to a new location
		/// copied from https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
		/// </summary>
		/// <param name="sourceDir">The directory to copy from</param>
		/// <param name="destinationDir">The directory to copy to</param>
		/// <param name="recursive">Whether to recurse into subdirectories</param>
		/// <exception cref="DirectoryNotFoundException"></exception>
		static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
		{
			var dir = new DirectoryInfo(sourceDir);

			if (!dir.Exists)
			{
				throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
			}

			var dirs = dir.GetDirectories();

			Directory.CreateDirectory(destinationDir);

			foreach (var file in dir.GetFiles())
			{
				if (file.LinkTarget != null)
				{
					continue;
				}

				var targetFilePath = Path.Combine(destinationDir, file.Name);
				file.CopyTo(targetFilePath);
			}

			if (recursive)
			{
				foreach (var subDir in dirs)
				{
					if (subDir.LinkTarget != null)
					{
						continue;
					}

					var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
					CopyDirectory(subDir.FullName, newDestinationDir, true);
				}
			}
		}

		/// <summary>
		/// Builds the web project deployment package based on data in Build.xml file
		/// </summary>
		/// <param name="resultFolder">Directory for Zip file</param>
		/// <returns>True is successfule and False otherwise</returns>
		public void Process(string resultFolder)
		{
			foreach (XmlNode node in Document.SelectSingleNode("//build:WebDeploy", BuildNamespaceManager)!.ChildNodes)
			{
				if (node.Name == "WebSolution")
				{
					AddSolution(node);
				}
			}

			foreach (var solution in Solutions)
			{
				var targetFolder = CreateNewTempPath();
				var sr = File.CreateText(Path.Combine(targetFolder, GetSharedFileName(solution.ClientName)));

				try
				{
					foreach (var clientSolutionDetails in solution.Solutions)
					{
						var parentFolder = clientSolutionDetails.SourceFolder;
						var newFolderName = clientSolutionDetails.DestinationName;

						var destFolder = Path.Combine(targetFolder, newFolderName);

						switch (clientSolutionDetails.SolutionType)
						{
							case SolutionType.Legacy:
								if (parentFolder != null)
								{
									foreach (var srcFileName in GetFilesInFolder(parentFolder))
									{
										ForceCopy(parentFolder, srcFileName, destFolder);
									}
								}
								break;
							case SolutionType.Published:
								CopyDirectory(parentFolder, destFolder, recursive: true);
								break;
						}

						foreach (var copyTask in clientSolutionDetails.CopyTasks)
						{
							var destination = Path.Combine(destFolder, copyTask.Destination);
							Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
							File.Copy(copyTask.Source, destination, overwrite: true);
						}

						sr.WriteLine(newFolderName);
					}
				}
				finally
				{
					sr.Close();
				}

				var resultFile = (solution.ClientName == "ALL") ? "EnterpriseWebDeploy.zip" : "ZClientWeb" + solution.ClientName + ".zip";
				resultFile = Path.Combine(resultFolder, resultFile);
				if (!ZipIt(targetFolder, resultFile))
				{
					throw new IOException("Cannot create target file " + resultFile);
				}
				if (PackageCreated != null)
				{
					PackageCreated(this, new PackageCreatedEventArgs(resultFile));
				}

				DeleteTempFolder(targetFolder);
			}
		}

		protected string GetSharedFileName(string clientCode)
		{
			return (clientCode == "ALL") ? "SharedFilesList.txt" : "SharedFilesListClient.txt";
		}

		#region Event Stuff
		public event PackageCreatedEventHandler? PackageCreated;
		public delegate void PackageCreatedEventHandler(object sender, PackageCreatedEventArgs e);

		public class PackageCreatedEventArgs : EventArgs
		{
			public PackageCreatedEventArgs(string packageFile)
			{
				fPackageFile = packageFile;
			}

			public string PackageFile
			{
				get { return fPackageFile; }
			}
			readonly string fPackageFile;
		}
		#endregion

		#region Xml document-related

		readonly string XmlFileName;

		XmlDocument? fDocument;
		protected XmlDocument Document
		{
			get
			{
				if (!File.Exists(XmlFileName))
				{
					throw new ApplicationException(XmlFileName + " does not exist");
				}

				if (fDocument == null)
				{
					fDocument = new XmlDocument();
					fDocument.Load(XmlFileName);
				}
				return fDocument;
			}
		}

		#region Parsing XML file

		protected List<ClientSolutions> Solutions = new List<ClientSolutions>();

		protected ClientSolutions GetSolution(string clientName)
		{
			foreach (ClientSolutions solution in Solutions)
			{
				if (solution.ClientName == clientName)
				{
					return solution;
				}
			}
			ClientSolutions result = new ClientSolutions(clientName);
			Solutions.Add(result);
			return result;
		}

		protected void AddSolution(XmlNode solutionNode)
		{
			var clientName = "ALL"; //common solution. deployed for all clients

			if (solutionNode.Attributes!["Client"] != null)
			{
				//client-specific solution
				clientName = solutionNode.Attributes["Client"]!.Value;
			}

			var solutionType = SolutionType.Legacy;

			var sourceFolder = solutionNode.Attributes["Path"]?.Value;
			var destinationName = solutionNode.Attributes["DestFolder"]!.Value;

			var publishDirectory = solutionNode.Attributes["PublishDirectory"]?.Value;
			if (publishDirectory != null)
			{
				if (sourceFolder is not null)
				{
					throw new ApplicationException("Cannot specify both Path and PublishDirectory attributes");
				}

				solutionType = SolutionType.Published;
				sourceFolder = publishDirectory;
			}

			var copyTasks = solutionNode.SelectNodes("build:Copy", BuildNamespaceManager)!
				.Cast<XmlNode>()
				.Select(copyTaskNode => new CopyTask(copyTaskNode.Attributes!["Source"]!.Value, copyTaskNode.Attributes["Destination"]!.Value))
				.ToList();

			var solution = GetSolution(clientName!);
			solution.AddSolution(new SolutionDetails(sourceFolder!, destinationName, copyTasks, solutionType));
		}

		#endregion

		#endregion

		#region creating ZIP file

		protected bool ZipIt(string sourceFolder, string targetZipFile)
		{
			return ZipCompression.Zip(sourceFolder, targetZipFile);
		}

		#endregion

		#region File operations

		/// <summary>
		/// Removes all files in temporary folder
		/// Clear read-only file attributes prior to deletion
		/// </summary>
		/// <param name="srcFolder">Folder to delete</param>
		protected void DeleteTempFolder(string srcFolder)
		{
			string[] fileEntries = Directory.GetFiles(srcFolder);
			foreach (string fileName in fileEntries)
			{
				DeleteFile(fileName);
			}

			// Recurse into subdirectories of this directory.
			string[] subdirectoryEntries = Directory.GetDirectories(srcFolder);
			foreach (string subdirectory in subdirectoryEntries)
			{
				DeleteTempFolder(subdirectory);
			}
			Directory.Delete(srcFolder);
		}

		/// <summary>
		/// Deletes all files, including read-only
		/// </summary>
		/// <param name="fileName">File to delete</param>
		protected void DeleteFile(string fileName)
		{
			ClearReadOnlyAttribute(fileName);
			File.Delete(fileName);
		}

		/// <summary>
		/// Removes read-only attribute is present
		/// </summary>
		/// <param name="fileName">File to clear attribute</param>
		protected void ClearReadOnlyAttribute(string fileName)
		{
			FileAttributes attrib = File.GetAttributes(fileName);
			if ((attrib & FileAttributes.ReadOnly) != 0)
			{
				attrib -= FileAttributes.ReadOnly;
			}
			File.SetAttributes(fileName, attrib);
		}

		/// <summary>
		/// Generates a list of files that belong to a web project
		/// It is one of .aspx, .asax, .ascx, .dll and the file shouldn't be found in
		/// C:\Dev\Bin folder.
		/// </summary>
		/// <param name="FilesToCompress">Collection of file names</param>
		/// <param name="srcFolder">Folder to scan</param>
		protected IEnumerable<string> GetFilesInFolder(string srcFolder)
		{
			var filesToCompress = new List<string>();
			string[] projectFiles1 = Directory.GetFiles(srcFolder, "*.csproj");
			if (projectFiles1.Length == 1)
			{
				var parser = new ProjectFileParser(projectFiles1[0]);
				var projectFiles = parser.GetDeployableFiles();
				foreach (string file in projectFiles)
				{
					filesToCompress.Add(file);
				}
			}
			return filesToCompress;
		}

		/// <summary>
		/// Copies a file into another location, creating all the required folders
		/// Sample:
		///		BaseSrcFolder is C:\Dev\Tracking\Tracking.Web
		///		SourceFile is C:\Dev\Tracking\Tracking.Web\Base\login.aspx
		///		Destination folder is C:\Temp
		///		Result should be C:\Temp\Base\login.aspx and directory C:\Temp\Base will be created automatically
		/// </summary>
		/// <param name="baseSrcFolder">Base Folder where source file is located</param>
		/// <param name="sourceFile">Source File name</param>
		/// <param name="baseDestFolder">Base Destination </param>
		protected void ForceCopy(string baseSrcFolder, string sourceFile, string baseDestFolder)
		{
			string baseFolder = Path.GetDirectoryName(sourceFile)!.Substring(baseSrcFolder.Length);
			if (baseFolder.StartsWith(@"\"))
			{
				baseFolder = baseFolder.Substring(1);
			}

			string destFolderPath = Path.Combine(baseDestFolder, baseFolder);
			if (!Directory.Exists(destFolderPath))
			{
				Directory.CreateDirectory(destFolderPath);
			}

			string destFilePath = Path.Combine(destFolderPath, GetDestinationFile(sourceFile));
			File.Copy(sourceFile, destFilePath, true);
		}

		/// <summary>
		/// Renames Web.Config into a Sample file so it won't override
		/// existing live Web.Config file on the web server
		/// </summary>
		/// <param name="sourceFile">Source file name including path</param>
		/// <returns>File name without path</returns>
		string GetDestinationFile(string sourceFile)
		{
			string sourceFileName = Path.GetFileName(sourceFile);
			return (sourceFileName.ToLower() == "web.config") ? "Web.Config.Sample" : sourceFileName;
		}

		/// <summary>
		/// Creates a new temp folder directory
		/// </summary>
		/// <returns>Name of created temp directory</returns>
		[SuppressMessage("Enterprise", "EDI011:TempPathRule", Justification = "Outside of Enterprise")]
		protected string CreateNewTempPath()
		{
			string path = Path.Combine(Path.GetTempPath(), "WebDeployBuilder", Guid.NewGuid().ToString());
			Directory.CreateDirectory(path);
			return path;
		}

		#endregion
	}
}
