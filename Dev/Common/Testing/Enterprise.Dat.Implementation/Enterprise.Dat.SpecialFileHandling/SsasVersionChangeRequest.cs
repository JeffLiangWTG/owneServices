using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Dat.Integration.SpecialFileHandling;
using Dat.Integration.VersionControl;

namespace Enterprise.Dat.SpecialFileHandling
{
	public class SsasVersionChangeRequest : VersionChangeRequest
	{
		public SsasVersionChangeRequest(SpecialFileHandlerContext context)
		{
			this.context = context;
		}

		protected override void BumpVersion(IWorkspaceAccess workspace, string versionFileServerPath)
		{
			var ssasBimFiles = new Dictionary<string, TfsChangeType>();
			foreach (var change in workspace.GetPendingChanges())
			{
				if (IsSsasBimFilePath(change.ServerItem))
				{
					var cubeName = change.FileName.Replace(ssasBimFileExt, "");
					ssasBimFiles.Add(cubeName, change.ChangeType);
				}
			}

			string localFile = workspace.GetLocalItemForServerItem(versionFileServerPath);
			string versionFileContents = File.ReadAllText(localFile);
			var formattedDate = DateTime.UtcNow.ToString("yyMMdd", CultureInfo.InvariantCulture);

			var ssasVersionList = new List<string>();
			var existingVersionLabelMatches = Regex.Matches(versionFileContents, ssasVersionLabelPattern);

			foreach (Match versionLabelMatch in existingVersionLabelMatches)
			{
				var cubeName = versionLabelMatch.Groups["cubeName"].Value;
				var majorVersion = Convert.ToInt32(versionLabelMatch.Groups["majorVersion"].Value, CultureInfo.InvariantCulture);
				var minorVersion = Convert.ToInt32(versionLabelMatch.Groups["minorVersion"].Value, CultureInfo.InvariantCulture);

				TfsChangeType changeType;
				if (ssasBimFiles.TryGetValue(cubeName, out changeType))
				{
					if (changeType != TfsChangeType.Delete)
					{
						// MINOR VERSION MUST ALWAYS BE 0 (ZERO) IN ALPHA RELEASE (RING 0)
						// MAJOR VERSION MUST NOT BE CHANGED IN RELEASES OTHER THAN ALPHA
						int newMajor;
						int newMinor;

						if (context.RepositoryKey.IsReleaseBranch)
						{
							newMajor = majorVersion;
							newMinor = minorVersion + 1;
						}
						else
						{
							if (majorVersion.ToString(CultureInfo.InvariantCulture).StartsWith(formattedDate, StringComparison.OrdinalIgnoreCase))
							{
								newMajor = majorVersion + 1;
							}
							else
							{
								newMajor = Convert.ToInt32(formattedDate + "00", CultureInfo.InvariantCulture);
							}
							newMinor = 0;
						}

						ssasVersionList.Add(string.Format(CultureInfo.InvariantCulture, @"{{ ""{0}"", new VersionLabel({1}, {2}) }}", cubeName, newMajor, newMinor));
					}
				}
				else
				{
					ssasVersionList.Add(string.Format(CultureInfo.InvariantCulture, @"{{ ""{0}"", new VersionLabel({1}, {2}) }}", cubeName, majorVersion, minorVersion));
				}
			}

			foreach (var ssasBimFile in ssasBimFiles.Where(f => f.Value != TfsChangeType.Delete && !ssasVersionList.Any(v => v.Contains(f.Key))))
			{
				var cubeName = ssasBimFile.Key;
				var majorVersion = Convert.ToInt32(formattedDate + "00", CultureInfo.InvariantCulture);
				var minorVersion = 0;

				ssasVersionList.Add(string.Format(CultureInfo.InvariantCulture, @"{{ ""{0}"", new VersionLabel({1}, {2}) }}", cubeName, majorVersion, minorVersion));
			}

			versionFileContents = Regex.Replace(versionFileContents, ssasVersionDictionaryPattern, string.Join(",\r\n\t\t\t\t\t", ssasVersionList));

			File.WriteAllText(localFile, versionFileContents);
		}

		protected override bool TriggersVersionChange(string serverPath)
		{
			return IsSsasBimFilePath(serverPath);
		}

		bool IsSsasBimFilePath(string serverPath)
		{
			var ssasBimFilePathRegex = new Regex(@"\/BusinessIntelligence\/CargoWiseBi\/CargoWiseBi.Models\/", RegexOptions.IgnoreCase);

			return ssasBimFilePathRegex.IsMatch(serverPath) &&
				serverPath.EndsWith(ssasBimFileExt, StringComparison.OrdinalIgnoreCase);
		}

		protected override string GetVersionFileServerPath(string serverPath)
		{
			return context.RepositoryKey.GetServerPath("BusinessIntelligence/BiIntegration/Deployment/CargoWiseBiDeployment/AnalysisServices/SsasProjectVersion.cs");
		}

		readonly SpecialFileHandlerContext context;

		const string ssasVersionDictionaryPattern = @"(?<=\s*new\s*Dictionary\s*<\s*string\s*,\s*VersionLabel\s*>\s*\{\s*)([^\s](.|\s)*?)(?=\s*\}\s*\)\s*;)";
		const string ssasVersionLabelPattern = @"\{\s*""(?<cubeName>.+?)""\s*,\s*new\s*VersionLabel\s*\(\s*(?<majorVersion>[0-9]+)\s*,\s*(?<minorVersion>[0-9]+)";
		const string ssasBimFileExt = ".bim";
	}
}
