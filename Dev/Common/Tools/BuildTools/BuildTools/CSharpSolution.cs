using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.Common;

namespace CargoWise.BuildTools
{
	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer only tool")]
	public class CSharpSolution
	{
		public CSharpSolution(string solutionFileName)
			: this(solutionFileName, "")
		{
			Argument.NotNullOrEmpty(solutionFileName, nameof(solutionFileName));
		}

		public CSharpSolution(string solutionFileName, string solutionSourceSafePath)
		{
			Argument.NotNullOrEmpty(solutionFileName, nameof(solutionFileName));
			Argument.NotNull(solutionSourceSafePath, nameof(solutionSourceSafePath));
			this.SolutionFileName = solutionFileName;
			this.SolutionSourceSafePath = solutionSourceSafePath;
			LoadFromFile();
		}

		public readonly string SolutionFileName;

		public StringCollection Projects
		{
			get { return fProjects; }
		}

		#region Implementation

		protected StringCollection fProjects;
		protected readonly string SolutionSourceSafePath;

		protected void LoadFromFile()
		{
			fProjects = new StringCollection();
			string[] solutionTextLines;

			using (StreamReader reader = File.OpenText(SolutionFileName))
			{
				solutionTextLines = reader.ReadToEnd().Split('\n');
			}

			string baseDir = new FileInfo(SolutionFileName).DirectoryName + @"\";

			foreach (string line in solutionTextLines)
			{
				if (line.StartsWith("Project"))
				{
					string[] lineParts = Regex.Split(line, "\",\\s+\"");
					if (lineParts.Length != 3)
					{
						throw new InvalidOperationException(string.Format("Expected '{0}' to have a guid, an assembly, a path and a guid in that order (see TestSolutionFile.txt for example).", line));
					}
					string projectPath = lineParts[1];
					if (string.IsNullOrEmpty(projectPath))
					{
						throw new InvalidOperationException(string.Format("Expected '{0}' to have a guid, an assembly, a path and a guid in that order (see TestSolutionFile.txt for example).", line));
					}

					if (projectPath.StartsWith("http://"))
					{
						int lastForwardSlashPos = projectPath.LastIndexOf("/") + 1;
						string projectName = projectPath.Substring(lastForwardSlashPos);
						string[] dirs = Directory.GetDirectories(baseDir);
						for (int i = 0; i < dirs.Length; i++)
						{
							string[] files = Directory.GetFiles(dirs[i], projectName);
							if (files.Length == 1)
							{
								projectPath = files[0];
							}
						}
					}
					else
					{
						projectPath = baseDir + projectPath;
					}

					if (projectPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)
						|| projectPath.EndsWith(".vdproj", StringComparison.OrdinalIgnoreCase)
						|| projectPath.EndsWith(".sqlproj", StringComparison.OrdinalIgnoreCase))
					{
						fProjects.Add(projectPath);
					}
				}
			}
		}

		protected string ConvertSourceSafeRelativePathToAbsolute(string projectRelativePath)
		{
			Argument.NotNullOrEmpty(projectRelativePath, nameof(projectRelativePath));
			if (projectRelativePath.StartsWith(@"./") && !string.IsNullOrEmpty(SolutionSourceSafePath))
			{
				int lastIndexOfSlash = SolutionSourceSafePath.LastIndexOf(@"/");
				return SolutionSourceSafePath.Substring(0, lastIndexOfSlash) + @"/" + projectRelativePath.Substring(2);
			}
			else
			{
				return projectRelativePath;
			}
		}

		#endregion
	}
}
