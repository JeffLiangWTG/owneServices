using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Dat.Integration.SpecialFileHandling;
using Dat.Integration.VersionControl;

namespace Enterprise.Dat.SpecialFileHandling
{
	public abstract class BiVersionChangeRequest : VersionChangeRequest
	{
		protected BiVersionChangeRequest(SpecialFileHandlerContext context)
		{
			Context = context;
		}

		protected override void BumpVersion(IWorkspaceAccess workspace, string versionFileServerPath)
		{
			string localFile = workspace.GetLocalItemForServerItem(versionFileServerPath);
			string versionFileContents = File.ReadAllText(localFile);

			int currentMajorVersion = Convert.ToInt32(Regex.Match(versionFileContents, appMajorVersionPattern).Value, CultureInfo.InvariantCulture);
			int currentMinorVersion = Convert.ToInt32(Regex.Match(versionFileContents, appMinorVersionPattern).Value, CultureInfo.InvariantCulture);

			// MINOR VERSION MUST ALWAYS BE 0 (ZERO) IN ALPHA RELEASE (RING 0)
			// MAJOR VERSION MUST NOT BE CHANGED IN RELEASES OTHER THAN ALPHA

			int newMajor;
			int newMinor;

			if (Context.RepositoryKey.IsReleaseBranch)
			{
				newMajor = currentMajorVersion;
				newMinor = currentMinorVersion + 1;
			}
			else
			{
				var formattedDate = DateTime.UtcNow.ToString("yyMMdd", CultureInfo.InvariantCulture);
				if (currentMajorVersion.ToString(CultureInfo.InvariantCulture).StartsWith(formattedDate, StringComparison.OrdinalIgnoreCase))
				{
					newMajor = currentMajorVersion + 1;
				}
				else
				{
					newMajor = Convert.ToInt32(formattedDate + "00", CultureInfo.InvariantCulture);
				}
				newMinor = 0;
			}

			versionFileContents = Regex.Replace(versionFileContents, appMajorVersionPattern, newMajor.ToString(CultureInfo.InvariantCulture));
			versionFileContents = Regex.Replace(versionFileContents, appMinorVersionPattern, newMinor.ToString(CultureInfo.InvariantCulture));

			File.WriteAllText(localFile, versionFileContents);
		}

		protected SpecialFileHandlerContext Context { get; }

		const string appMajorVersionPattern = @"(?<=\bApplication\b\s*=\s*new\s*VersionLabel\s*\(\s*)([0-9]+)(?=\s*,\s*[0-9]+\s*\)\s*;)";
		const string appMinorVersionPattern = @"(?<=\bApplication\b\s*=\s*new\s*VersionLabel\s*\(\s*[0-9]+\s*,\s*)([0-9]+)(?=\s*\)\s*;)";
	}

	public class AnalyticsReportVersionChangeRequest : BiVersionChangeRequest
	{
		public AnalyticsReportVersionChangeRequest(SpecialFileHandlerContext context)
			: base(context)
		{
		}

		const string biReportFileExt = ".pbix";

		protected override bool TriggersVersionChange(string serverPath)
		{
			var powerBiPackageFilePathRegex = new Regex(@"\/BusinessIntelligence\/CargoWiseBi\/CargoWiseBi.PBIRS\/Reports\/(?!Audit)", RegexOptions.IgnoreCase);

			return powerBiPackageFilePathRegex.IsMatch(serverPath) &&
				serverPath.EndsWith(biReportFileExt, StringComparison.OrdinalIgnoreCase);
		}

		protected override string GetVersionFileServerPath(string serverPath)
		{
			return Context.RepositoryKey.GetServerPath("BusinessIntelligence/BiIntegration/Deployment/CargoWiseBiDeployment/ReportingServices/AnalyticsReport/AnalyticsReportProjectVersion.cs");
		}
	}
}
