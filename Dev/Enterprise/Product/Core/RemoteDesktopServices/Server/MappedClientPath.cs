using System;
using System.IO;
using CargoWise.Application;
using Enterprise.Integration.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Server.TrackingInfo;

namespace Enterprise.RemoteDesktopServices
{
	public class MappedClientPath : IMappedClientPath
	{
		public const string RootPathMS = @"\\tsclient";
		public const string RootPathCitrix = @"\\Client";
		public const string VolumeSeparatorCharCitrix = "$";

		public virtual string GetMappedPath(string unmappedPath)
		{
			var mappingResult = TryResolveMappedPath(unmappedPath, out var mappedPath);
			if (mappingResult != DriveMappingResult.Success
				|| !CheckDirectorySafely(unmappedPath, mappedPath))
			{
				TrackingInfoLogger.Instance.NewLog(() => $"Map path [{unmappedPath}] to null");
				return null;
			}

			TrackingInfoLogger.Instance.NewLog(() => $"Map path [{unmappedPath}] to [{mappedPath}]");
			return mappedPath;

			bool CheckDirectorySafely(string unmappedDirectoryPath, string mappedDirectoryPath)
			{
				try
				{
					var checkDirectoryResult = CheckDirectory(unmappedDirectoryPath, mappedDirectoryPath);
					return checkDirectoryResult == ExistenceState.ExistingNormally ||
						checkDirectoryResult == ExistenceState.ExistingButNoPermission ||
						checkDirectoryResult == ExistenceState.UnauthorizedError;
				}
				catch (Exception e)
				{
					TrackingInfoLogger.Instance.NewLog(() => $"Directory.GetAccessControl | {ExistenceState.NotExisting}: {e}");
					return false;
				}
			}
		}

		internal string GetMappedPathOfExistingFile(string unmappedFilePath)
		{
			TrackingInfoLogger.Instance.NewLog(() => $"Resolving path [{unmappedFilePath}]");

			string mappedFilePath = null;
			if (TryResolveMappedPath(unmappedFilePath, out mappedFilePath) == DriveMappingResult.VolumeSeparatorFormatError)
			{
				TrackingInfoLogger.Instance.NewLog(() => $"Map existing file [{unmappedFilePath}] => {DriveMappingResult.VolumeSeparatorFormatError}");
				throw new FileNotFoundException($"Cannot find file. Mapping Result: {DriveMappingResult.VolumeSeparatorFormatError}");
			}

			TrackingInfoLogger.Instance.NewLog(() => $"Resolved path [{mappedFilePath}]");

			// check file's existence first in order to avoid redundant directory check
			var fileExistenceState = CheckFile(unmappedFilePath, mappedFilePath);
			TrackingInfoLogger.Instance.NewLog(() => $"Checked file existence [{mappedFilePath}] => {fileExistenceState}");

			if (fileExistenceState != ExistenceState.ExistingNormally)
			{
				var dirExistenceState = CheckDirectory(unmappedFilePath, mappedFilePath);
				TrackingInfoLogger.Instance.NewLog(() => $"Checked directory existence [{mappedFilePath}] => {dirExistenceState}");
				throw new FileNotFoundException($"File Existence State: {fileExistenceState}, Directory Existence State:{dirExistenceState}");
			}

			TrackingInfoLogger.Instance.NewLog(() => $"Path mapped to [{mappedFilePath}]");
			return mappedFilePath;
		}

		static ExistenceState CheckFile(string unmappedFilePath, string mappedFilePath)
		{
#if DEBUG
			if (DoesFileExistTestMock != null)
			{
				return DoesFileExistTestMock(mappedFilePath);
			}
#endif

			return FileSystem.FileExists(mappedFilePath);
		}

		static ExistenceState CheckDirectory(string unmappedPath, string mappedPath)
		{
			// if path is root, such as: C:\, parent directory is itself.
			var directory = Path.GetDirectoryName(unmappedPath) == null
				? mappedPath
				: Path.GetDirectoryName(mappedPath);

#if DEBUG
			if (DoesParentDirectoryExistTestMock != null)
			{
				return DoesParentDirectoryExistTestMock(directory);
			}
#endif

			return FileSystem.DirectoryExists(directory);
		}

		DriveMappingResult TryResolveMappedPath(string unmappedPath, out string mappedPath)
		{
			var volumeSeparatorIndex = unmappedPath.IndexOf(Path.VolumeSeparatorChar);
			if (volumeSeparatorIndex <= 0)
			{
				mappedPath = null;
				return DriveMappingResult.VolumeSeparatorFormatError;
			}

#if DEBUG
			if (LeaveThePathUnMappedForTesting)
			{
				mappedPath = unmappedPath;
				return DriveMappingResult.Success;
			}
#endif

			var volume = unmappedPath.Substring(0, volumeSeparatorIndex)
				+ (ObjectFactory.Get<TerminalService>().IsCitrixICA ? VolumeSeparatorCharCitrix : string.Empty);

			var mappedVolume = Path.Combine(rootPath, volume);
			mappedPath = volumeSeparatorIndex + 2 <= unmappedPath.Length
				? Path.Combine(
					mappedVolume,
					unmappedPath.Substring(volumeSeparatorIndex + 2)
						.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
				: mappedVolume;

			return DriveMappingResult.Success;
		}

#if DEBUG
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "For test only")]
		public static bool LeaveThePathUnMappedForTesting { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "For test only")]
		internal static Func<string, ExistenceState> DoesParentDirectoryExistTestMock { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "For test only")]
		internal static Func<string, ExistenceState> DoesFileExistTestMock { get; set; }
#endif

		public string GetUnmappedPath(string mappedPath)
		{
#if DEBUG
			if (LeaveThePathUnMappedForTesting)
			{
				return mappedPath;
			}
#endif
			string unmappedPath = null;
			if (mappedPath.StartsWith(rootPath, StringComparison.InvariantCultureIgnoreCase))
			{
				unmappedPath = mappedPath.Substring(rootPath.Length + 1);
				var offset = ObjectFactory.Get<TerminalService>().IsCitrixICA ? 1 : 0;
				int unmappedPathIndex = unmappedPath.IndexOf(@"\");
				string drive = unmappedPath.Substring(0, unmappedPathIndex - offset).ToUpper();
				unmappedPath = Path.Combine(drive + @":\", unmappedPath.Substring(unmappedPathIndex + 1));
			}
			return unmappedPath;
		}

		readonly string rootPath = ObjectFactory.Get<TerminalService>().IsCitrixICA ? RootPathCitrix : RootPathMS;
	}
}
