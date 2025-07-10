using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Dat.Integration.SpecialFileHandling;
using Dat.Integration.VersionControl;

namespace Enterprise.Dat.SpecialFileHandling
{
	public abstract class ApplicationVersionLabelChangeRequest : VersionChangeRequest
	{
		protected ApplicationVersionLabelChangeRequest(SpecialFileHandlerContext context)
		{
			Context = context;
		}

		protected override void BumpVersion(IWorkspaceAccess workspace, string versionFileServerPath)
		{
			var localFile = workspace.GetLocalItemForServerItem(versionFileServerPath);
			var versionFileContents = File.ReadAllText(localFile);

			var currentMajorVersion = Convert.ToInt32(Regex.Match(versionFileContents, AppMajorVersionPattern).Value, CultureInfo.InvariantCulture);
			var currentMinorVersion = Convert.ToInt32(Regex.Match(versionFileContents, AppMinorVersionPattern).Value, CultureInfo.InvariantCulture);

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
				newMajor = currentMajorVersion + 1;
				newMinor = 0;
			}

			versionFileContents = Regex.Replace(versionFileContents, AppMajorVersionPattern, newMajor.ToString(CultureInfo.InvariantCulture));
			versionFileContents = Regex.Replace(versionFileContents, AppMinorVersionPattern, newMinor.ToString(CultureInfo.InvariantCulture));

			File.WriteAllText(localFile, versionFileContents);
		}

		protected SpecialFileHandlerContext Context { get; }

		const string AppMajorVersionPattern = @"(?<=\bApplication\b\s*=\s*new\s*VersionLabel\s*\(\s*)([0-9]+)(?=\s*,\s*[0-9]+\s*\)\s*;)";
		const string AppMinorVersionPattern = @"(?<=\bApplication\b\s*=\s*new\s*VersionLabel\s*\(\s*[0-9]+\s*,\s*)([0-9]+)(?=\s*\)\s*;)";
	}
}
