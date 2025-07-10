using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Dat.Integration.SpecialFileHandling;
using Dat.Integration.VersionControl;

namespace Enterprise.Dat.SpecialFileHandling
{
	public sealed class TransformVersionBump : ISpecialFileHandler
	{
		public TransformVersionBump(SpecialFileHandlerContext context)
		{
			this.context = context;
		}

		public bool ShouldUnshelveForDATCheckin(IPendingChange change)
		{
			if (change == null)
			{
				throw new ArgumentNullException(nameof(change));
			}

			return !IsVersionFile(change.ServerItem);
		}

		public string[] UpdateForDATCheckin(IWorkspaceAccess workspace, IPendingChange change)
		{
			// bump the transform version when online transforms are changed.
			const string dbUpgraderSchemaDirectory = "Enterprise/Product/Core/DbUpgrader/Schema";

			if (change.ServerItem.IndexOf(dbUpgraderSchemaDirectory, StringComparison.OrdinalIgnoreCase) > -1
				|| change.ServerItem.EndsWith(TransformVersionPath, StringComparison.OrdinalIgnoreCase))
			{
				var versionFile = context.RepositoryKey.GetServerPath(TransformVersionPath);

				if (!workspace.GetPendingChanges().Any(c => c.ServerItem.EndsWith(TransformVersionPath, StringComparison.OrdinalIgnoreCase)))
				{
					return new[]
					{
						BumpVersion(workspace, versionFile),
					};
				}

				return Array.Empty<string>();
			}
			else
			{
				return null;
			}
		}

		public bool ShouldMerge(string sourceBranch, string targetBranch, IPendingChange change)
		{
			if (string.IsNullOrEmpty(sourceBranch))
			{
				throw new ArgumentException("Value cannot be null or empty.", nameof(sourceBranch));
			}

			if (string.IsNullOrEmpty(targetBranch))
			{
				throw new ArgumentException("Value cannot be null or empty.", nameof(targetBranch));
			}

			if (change == null)
			{
				throw new ArgumentNullException(nameof(change));
			}

			return !IsVersionFile(change.ServerItem);
		}

		public string[] UpdateForMerge(IWorkspaceAccess workspace, string sourceBranch, string targetBranch, IPendingChange originalChange) => null;

		string BumpVersion(IWorkspaceAccess workspace, string versionFileServerItem)
		{
			string versionFileLocalItem = workspace.GetLocalItemForServerItem(versionFileServerItem);

			workspace.GetLatest(new string[] { versionFileServerItem }, TfsRecursionType.None, TfsGetOptions.None);
			workspace.PendEdit(versionFileServerItem);

			string versionFileContents = File.ReadAllText(versionFileLocalItem);
			var match = Regex.Match(versionFileContents, VersionPattern);
			ExtractVersion(match, out var currentMajorVersion, out var currentMinorVersion);

			BumpVersion(context.RepositoryKey.IsReleaseBranch, currentMajorVersion, currentMinorVersion, out var newMajor, out var newMinor);

			versionFileContents = UpdateVersion(versionFileContents, match, newMajor, newMinor);

			File.WriteAllText(versionFileLocalItem, versionFileContents);

			return versionFileLocalItem;
		}

		void BumpVersion(bool isReleaseBranch, int currentMajorVersion, int currentMinorVersion, out int newMajor, out int newMinor)
		{
			// MINOR VERSION MUST ALWAYS BE 0 (ZERO) IN ALPHA RELEASE (RING 0)
			// MAJOR VERSION MUST NOT BE CHANGED IN RELEASES OTHER THAN ALPHA

			if (isReleaseBranch)
			{
				newMajor = currentMajorVersion;
				newMinor = currentMinorVersion + 1;
			}
			else
			{
				newMajor = currentMajorVersion + 1;
				newMinor = 0;
			}
		}

		static void ExtractVersion(Match match, out int major, out int minor)
		{
			var majorGroup = match.Groups[MajorGroupName];
			var minorGroup = match.Groups[MinorGroupName];
			major = Convert.ToInt32(majorGroup.Value, CultureInfo.InvariantCulture);
			minor = Convert.ToInt32(minorGroup.Value, CultureInfo.InvariantCulture);
		}

		static string UpdateVersion(string versionFileContents, Match match, int newMajor, int newMinor)
		{
			var majorGroup = match.Groups[MajorGroupName];
			var minorGroup = match.Groups[MinorGroupName];
			var majorEnd = majorGroup.Index + majorGroup.Length;
			var minorEnd = minorGroup.Index + minorGroup.Length;

			versionFileContents = new StringBuilder(versionFileContents.Length + 1)
				.Append(versionFileContents, 0, majorGroup.Index)
				.Append(newMajor.ToString(CultureInfo.InvariantCulture))
				.Append(versionFileContents, majorEnd, minorGroup.Index - majorEnd)
				.Append(newMinor.ToString(CultureInfo.InvariantCulture))
				.Append(versionFileContents, minorEnd, versionFileContents.Length - minorEnd)
				.ToString();

			return versionFileContents;
		}

		bool IsVersionFile(string serverPath) => serverPath.EndsWith(TransformVersionPath);

		const string TransformVersionPath = "Database/Odyssey/Resource/Version/TransformationVersion.cs";
		const string VersionPattern = @"\bApplicationNumber\b\s*=\s*new\s+VersionLabel\s*\(\s*(?<major>[0-9]+)\s*,\s*(?<minor>[0-9])+\s*\)\s*;";
		const string MajorGroupName = "major";
		const string MinorGroupName = "minor";

		readonly SpecialFileHandlerContext context;
	}
}
