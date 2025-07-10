using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BuildXml = CargoWise.BuildTools.BuildXml;

namespace Enterprise.Client.EDI.AutoDeploy
{
	public partial class RuntimePackageBuilder
	{
		public static IReadOnlyList<string> NetCoreFilePatterns => new[] { "net8.0" };

		static internal string NetCoreExcludeFilePattern => @"
			# Match paths that end with .test.dll, .testing.dll, .test.exe, .test.pdb, etc.
			\\                          # Match a backslash
			(.+\.)*                     # Match any characters followed by a dot, zero or more times
			(test|testing|)             # Match 'test' or 'testing'
			\.                          # Match a literal dot
			(exe|pdb|xml|dll|deps\.json|dll\.config)  # Match file extensions
			$
			| # Or
			# Match file names that contain '.Test' at the end
			\..+\.Test$                 # Match any character one or more times followed by '.Test' at the end
		";
		static internal string[] NetCoreExcludeFileNames => new[]
		{
			@"nunit.engine.api.dll",
			@"nunit.engine.core.dll",
			@"nunit.engine.dll",
			@"nunit.framework.dll",
			@"nunit.framework.legacy.dll",
			@"testhost.dll",
			@"testhost.exe",
			@"testcentric.engine.metadata.dll",
			@"testcentric.engine.dll",
			@"NUnit3.TestAdapter.dll",
			@"NUnit3.TestAdapter.pdb",
		};

		static internal bool MatchesNetCoreExcludeFilePatterns(string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
			{
				throw new ArgumentNullException(nameof(fileName));
			}

			if (Regex.IsMatch(fileName, NetCoreExcludeFilePattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace))
			{
				return true;
			}

			if (NetCoreExcludeFileNames.Any(x => fileName.EndsWith(x, StringComparison.OrdinalIgnoreCase)))
			{
				return true;
			}

			return false;
		}

		static internal HashSet<string> GetNoDeployFiles(BuildXml buildXml)
		{
			var noDeployFiles = buildXml.GetNotDeployToClientsFiles();
			foreach (var noDeployFile in noDeployFiles.ToList())
			{
				if (NetCoreFilePatterns.Any(filePattern => noDeployFile.StartsWith(filePattern, StringComparison.OrdinalIgnoreCase)))
				{
					var fileNameWithoutExtension = noDeployFile.Substring(0, noDeployFile.Length - 4);
					if (noDeployFile.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
					{
						noDeployFiles.Add($"{fileNameWithoutExtension}.deps.json");
						noDeployFiles.Add($"{fileNameWithoutExtension}.dll.config");
						noDeployFiles.Add($"{fileNameWithoutExtension}.pdb");
						noDeployFiles.Add($"{fileNameWithoutExtension}.runtimeconfig.json");
						noDeployFiles.Add($"{fileNameWithoutExtension}.xml");
					}
				}
			}

			return noDeployFiles;
		}
	}
}
