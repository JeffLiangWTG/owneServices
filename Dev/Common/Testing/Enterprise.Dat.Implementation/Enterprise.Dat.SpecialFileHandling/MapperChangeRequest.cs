using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Dat.Integration.SpecialFileHandling;
using Dat.Integration.VersionControl;

namespace Enterprise.Dat.SpecialFileHandling
{
	public class UpgradeMapperChangeRequest : GeneralTransformMapperChangeRequest
	{
		public UpgradeMapperChangeRequest(SpecialFileHandlerContext context)
			: base(context)
		{
		}

		protected override string ShelfCheckinMapperPath
			=> @"Database/Odyssey/Transformations/Transformations/Transforms/" + ShelfCheckinMapperTextFileName;

		protected override string ShelfCheckinMapperTextFileName => "ShelfCheckinMapper.txt";
		protected override string MapperCSharpFileName => "Mapper.cs";

		protected override void AppendMappingLine(int newMajor, int newMinor, StringBuilder mappingStrings, string shelfCheckInMapperLine)
		{
			mappingStrings
				.AppendLine()
				.Append($"				Mapping.New<{shelfCheckInMapperLine}>(new VersionLabel({newMajor},{newMinor})),");
		}
	}

	public class VisualizerDocumentDataMapperChangeRequest : MapperChangeRequest
	{
		public VisualizerDocumentDataMapperChangeRequest(SpecialFileHandlerContext context)
			: base(context)
		{
		}

		#region Overrides

		protected override string ShelfCheckinMapperPath
			=> @"DocumentVisualizer/DataTransformation/ShelfCheckinMapper.txt";

		protected override string TransformationVersionPath
			=> @"DocumentVisualizer/DataTransformation/VisualizerDocumentDataVersion.cs";

		protected override string ShelfCheckinMapperTextFileName => "ShelfCheckinMapper.txt";
		protected override string MapperCSharpFileName => "Mapper.cs";

		protected override string AppMajorVersionPattern
			=> @"(?<=\DocumentData\b\s*=\s*new\s*VersionLabel\s*\(\s*)([0-9]+)(?=\s*,\s*[0-9]+\s*\)\s*;)";

		protected override string AppMinorVersionPattern
			=> @"(?<=\DocumentData\b\s*=\s*new\s*VersionLabel\s*\(\s*[0-9]+\s*,\s*)([0-9]+)(?=\s*\)\s*;)";

		protected override void AppendMappingLine(int newMajor, int newMinor, StringBuilder mappingStrings, string shelfCheckInMapperLine)
		{
			mappingStrings
				.AppendLine()
				.Append("			yield return Mapping.New<")
				.Append(shelfCheckInMapperLine)
				.Append(">(new VersionLabel(")
				.Append(newMajor)
				.Append(",")
				.Append(newMinor)
				.Append("));");
		}

		public override bool ShouldUnshelveForDATCheckin(IPendingChange change)
			=> base.ShouldUnshelveForDATCheckin(change)
			&& !change.ServerItem.EndsWith(TransformationVersionPath);

		public override bool ShouldMerge(string sourceBranch, string targetBranch, IPendingChange change)
			=> base.ShouldMerge(sourceBranch, targetBranch, change)
			&& !change.ServerItem.EndsWith(TransformationVersionPath);

		#endregion
	}

	// Base class for special file handlers that bump TransformationVersion.cs
	public abstract class GeneralTransformMapperChangeRequest : MapperChangeRequest
	{
		protected GeneralTransformMapperChangeRequest(SpecialFileHandlerContext context)
			: base(context)
		{
		}

		protected sealed override string TransformationVersionPath
			=> @"Database/Odyssey/Resource/Version/TransformationVersion.cs";

		protected sealed override string AppMajorVersionPattern
			=> @"(?<=\bApplicationNumber\b\s*=\s*new\s*VersionLabel\s*\(\s*)([0-9]+)(?=\s*,\s*[0-9]+\s*\)\s*;)";

		protected sealed override string AppMinorVersionPattern
			=> @"(?<=\bApplicationNumber\b\s*=\s*new\s*VersionLabel\s*\(\s*[0-9]+\s*,\s*)([0-9]+)(?=\s*\)\s*;)";

		public sealed override bool ShouldUnshelveForDATCheckin(IPendingChange change)
			=> base.ShouldUnshelveForDATCheckin(change)
			&& !change.ServerItem.EndsWith(TransformationVersionPath);

		public sealed override bool ShouldMerge(string sourceBranch, string targetBranch, IPendingChange change)
			=> base.ShouldMerge(sourceBranch, targetBranch, change)
			&& !change.ServerItem.EndsWith(TransformationVersionPath);
	}

	public abstract class MapperChangeRequest : DeltaFileHandler
	{
		protected MapperChangeRequest(SpecialFileHandlerContext context)
		{
			Context = context;
		}

		// Examples Values:
		protected abstract string ShelfCheckinMapperPath { get; }           //  Transformation/DataModification/ShelfCheckinMapper.txt
		protected abstract string TransformationVersionPath { get; }        //  Resource/Version/TransformationVersion.cs
		protected abstract string ShelfCheckinMapperTextFileName { get; }   //  ShelfCheckinMapper.txt
		protected abstract string MapperCSharpFileName { get; }             //  Mapper.cs
		protected abstract string AppMajorVersionPattern { get; }
		protected abstract string AppMinorVersionPattern { get; }

		protected string MapperCSharpPath                                   // Transformation/DataModification/Mapper.cs
			=> ShelfCheckinMapperPath.Replace(ShelfCheckinMapperTextFileName, MapperCSharpFileName);

		protected override bool IsDeltaFile(string serverPath) => serverPath.EndsWith(ShelfCheckinMapperPath);

		protected override bool IsMasterFile(string serverPath)
			=> serverPath.EndsWith(TransformationVersionPath) || serverPath.EndsWith(MapperCSharpPath);

		protected override bool ShouldUnshelveMaster(string serverPath) => true;

		protected override string[] UpdateForDATCheckinOnDeltaFile(IWorkspaceAccess workspace, IPendingChange change, string localTempFile)
		{
			var updatedFiles = new string[2];
			updatedFiles[0] = BumpVersion(workspace, GetVersionFileFromMapperChange(change), out var newMajor, out var newMinor);
			updatedFiles[1] = UpdateMapper(workspace, change, localTempFile, newMajor, newMinor);
			return updatedFiles;
		}

		public override bool ShouldUnshelveForDATCheckin(IPendingChange change)
		{
			if (IsDeltaFile(change.ServerItem)
				&& (change.ChangeType.HasFlag(TfsChangeType.Add) || change.ChangeType.HasFlag(TfsChangeType.Delete)))
			{
				return true;
			}

			return base.ShouldUnshelveForDATCheckin(change);
		}

		protected virtual string GetVersionFileFromMapperChange(IPendingChange change) => change.ServerItem.Replace(ShelfCheckinMapperPath, TransformationVersionPath);

		protected virtual string BumpVersion(IWorkspaceAccess workspace, string versionFileServerItem, out int newMajor, out int newMinor)
		{
			string versionFileLocalItem = workspace.GetLocalItemForServerItem(versionFileServerItem);

			workspace.GetLatest(new string[] { versionFileServerItem }, TfsRecursionType.None, TfsGetOptions.None);
			workspace.PendEdit(versionFileServerItem);

			string versionFileContents = File.ReadAllText(versionFileLocalItem);
			int currentMajorVersion = Convert.ToInt32(Regex.Match(versionFileContents, AppMajorVersionPattern).Value, CultureInfo.InvariantCulture);
			int currentMinorVersion = Convert.ToInt32(Regex.Match(versionFileContents, AppMinorVersionPattern).Value, CultureInfo.InvariantCulture);

			BumpVersion(Context.RepositoryKey.IsReleaseBranch, currentMajorVersion, currentMinorVersion, out newMajor, out newMinor);

			versionFileContents = Regex.Replace(versionFileContents, AppMajorVersionPattern, newMajor.ToString());
			versionFileContents = Regex.Replace(versionFileContents, AppMinorVersionPattern, newMinor.ToString());

			File.WriteAllText(versionFileLocalItem, versionFileContents);

			return versionFileLocalItem;
		}

		protected virtual void BumpVersion(bool isReleaseBranch, int currentMajorVersion, int currentMinorVersion, out int newMajor, out int newMinor)
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

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		protected void GetVersion(IWorkspaceAccess workspace, string versionFileServerItem, out int major, out int minor)
		{
			workspace.GetLatest(new string[] { versionFileServerItem }, TfsRecursionType.None, TfsGetOptions.None);
			string versionFileLocalItem = workspace.GetLocalItemForServerItem(versionFileServerItem);
			string versionFileContents = File.ReadAllText(versionFileLocalItem);
			major = Convert.ToInt32(Regex.Match(versionFileContents, AppMajorVersionPattern).Value, CultureInfo.InvariantCulture);
			minor = Convert.ToInt32(Regex.Match(versionFileContents, AppMinorVersionPattern).Value, CultureInfo.InvariantCulture);
		}

		protected virtual string UpdateMapper(IWorkspaceAccess workspace, IPendingChange change, string localTempFile, int newMajor, int newMinor)
		{
			const string DATInsertTag = @"//DO_NOT_CHANGE_THIS_LINE_DAT_WILL_MAP_TRANSFORMATIONS_BELOW";
			StringBuilder mappingStrings = new StringBuilder();
			mappingStrings.Append(DATInsertTag);

			using (TextReader streamReader = new StreamReader(localTempFile))
			{
				string line = string.Empty;
				do
				{
					line = line.Trim();
					if (!string.IsNullOrEmpty(line) && !line.StartsWith("//"))
					{
						AppendMappingLine(newMajor, newMinor, mappingStrings, line);
					}
					line = streamReader.ReadLine();
				}
				while (line != null);
			}

			string mapperFileServerItem = change.ServerItem.Replace(ShelfCheckinMapperTextFileName, MapperCSharpFileName);
			string mapperFileLocalItem = workspace.GetLocalItemForServerItem(mapperFileServerItem);
			workspace.GetLatest(new string[] { mapperFileServerItem }, TfsRecursionType.None, TfsGetOptions.None);
			workspace.PendEdit(mapperFileServerItem);

			string mapperFileCurrentContents = File.ReadAllText(mapperFileLocalItem);
			if (!mapperFileCurrentContents.Contains(DATInsertTag))
			{
				throw new Exception(string.Format("DAT cannot auto map transformation, because required string is missing in mapper file or its format is incorrect.\r\nMissing string: {0}\r\nMapper file: {1}", DATInsertTag, mapperFileLocalItem));
			}
			string newMapperContent = mapperFileCurrentContents.Replace(DATInsertTag, mappingStrings.ToString());
			File.WriteAllText(mapperFileLocalItem, newMapperContent);

			return mapperFileLocalItem;
		}

		protected abstract void AppendMappingLine(int newMajor, int newMinor, StringBuilder mappingStrings, string shelfCheckInMapperLine);

		protected SpecialFileHandlerContext Context { get; }
	}
}
