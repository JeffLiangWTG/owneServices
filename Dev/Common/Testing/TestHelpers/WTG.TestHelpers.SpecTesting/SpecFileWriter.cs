using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace WTG.TestHelpers.SpecTesting
{
	public static class SpecFileWriter
	{
		public static void WriteSpecFile(string relativePath, string fileName, string fileData)
		{
			var path = Path.GetFullPath(Path.Combine(FindRepositoryRoot(), relativePath, fileName));
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllText(path, fileData);

			// Log for the user to see progress
#pragma warning disable CW1106 // Do Not Leave In Debug Messages
			Console.WriteLine("Regenerated: " + path);
#pragma warning restore CW1106 // Do Not Leave In Debug Messages
		}

		public static void WriteSpecFile(string relativePath, SpecFile file)
		{
			WriteSpecFile(relativePath, file.FileName, file.Contents);
		}

		public static void WriteSpecFiles(string relativePath, IEnumerable<SpecFile> specFiles)
		{
			foreach (var specFile in specFiles)
			{
				WriteSpecFile(relativePath, specFile);
			}
		}

		public static void WriteSpecFiles(ISpecProvider provider)
		{
			WriteSpecFiles(provider.SpecFolderPath, provider.GetSpecFiles());
		}

		static string FindRepositoryRoot()
		{
			// Start at the binary location. It's likely in the repo main bin folder
			var currentDir = Assembly.GetExecutingAssembly().Location;

			// Step to parent folders until we find a Build.xml that's not in a bin folder.
			// We don't look for .git as some people might have .sl instead
			var repoRoot = currentDir;
			while (!File.Exists(Path.Combine(repoRoot, "Build.xml")) || Path.GetFileName(repoRoot).ToLower() == "bin")
			{
				repoRoot = Path.GetDirectoryName(repoRoot);

				if (string.IsNullOrEmpty(repoRoot))
				{
					throw new Exception("CargoWise repository root not found");
				}
			}

			return repoRoot;
		}
	}
}
