#if DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CargoWise.Common;
using CargoWise.Shared;

namespace CargoWise.BuildTools.Testing
{
	public sealed class MockSourceControl : ISourceControl
	{
		#region Test Initialisers

		[SuppressMessage("CargoWiseOne", "CW1051:BaseSourcePath", Justification = "Consumed tests have SOURCE_CODE")]
		public static void Setup()
		{
			DeleteAllDirectories();
			CopyMockSourceControlFiles(MockSourceControlPath);
			CopyMockSourceControlFiles(MockWorkspacePath);

			string[] files = Directory.GetFiles(MockSourceControlPath, "*", SearchOption.AllDirectories);

			foreach (string filePath in files)
			{
				File.SetAttributes(filePath, File.GetAttributes(filePath) | FileAttributes.ReadOnly);
			}
		}

		public string WorkspaceName
		{
			get { return "Poodle"; }
		}

		public static void TearDown()
		{
			DeleteAllDirectories();
			SourceControl.ClearTestDatabaseInstances();
		}

		static void DeleteAllDirectories()
		{
			FileIO.DeleteDirectory(new DirectoryInfo(MockWorkspacePath));
			FileIO.DeleteDirectory(new DirectoryInfo(MockSourceControlPath));
		}

		#endregion

		#region ISourceControl Members

		[SuppressMessage("Microsoft.Contracts", "Requires-46-11")] //CC Static Analyzer can't prove ForAlls
		public void PendAdd(string path)
		{
			PendAdd(new string[] { path });
		}

		[SuppressMessage("Microsoft.Contracts", "Requires-46-9")] //CC Static Analyzer can't prove ForAlls
		public void PendAdd(string[] paths)
		{
			CheckSetupHasBeenRun();
			CheckOut(paths, false);
		}

		public void PendEdit(string path)
		{
			CheckSetupHasBeenRun();
			CheckOut(path, false);
		}

		#region Repository Actions

		public bool AddFile(string path)
		{
			CheckSetupHasBeenRun();
			return CheckinFileToMockSourceControlPath(path);
		}

		public bool AddNewDirectory(string path)
		{
			string serverPath = SourceControlPathsForTest.ConvertLocalPathToServerPath(path);
			if (!File.Exists(serverPath))
			{
				Directory.CreateDirectory(serverPath);
				return true;
			}
			return false;
		}

		public bool DeleteFile(string path)
		{
			CheckSetupHasBeenRun();
			string serverPath = SourceControlPathsForTest.ConvertLocalPathToServerPath(path);
			if (File.Exists(serverPath))
			{
				File.SetAttributes(serverPath, FileAttributes.Normal);
				File.Delete(serverPath);
				return true;
			}
			return false;
		}

		public bool DeleteDirectory(string path)
		{
			CheckSetupHasBeenRun();
			string serverPath = SourceControlPathsForTest.ConvertLocalPathToServerPath(path);
			if (Directory.Exists(serverPath))
			{
				Directory.Delete(serverPath);
				return true;
			}
			return false;
		}

		#endregion

		public void SubmitChanges(string staffCode, string name, string criticality, string[] items)
		{
			throw new NotImplementedException();
		}

		public void CreateShelveset(string shelfName, string comment, string[] paths)
		{
			foreach (string path in paths)
			{
				CreateShelveset(path);
			}
		}

		void CreateShelveset(string path)
		{
			Argument.NotNullOrEmpty(path, nameof(path));
			CheckSetupHasBeenRun();

			string serverPath = SourceControlPathsForTest.ConvertLocalPathToServerPath(path);
			string localPath = SourceControlPathsForTest.ConvertServerPathToLocalPath(path);

			if (!File.Exists(serverPath))
			{
				throw new SourceControlException("Not in source control");
			}
			if (File.Exists(localPath))
			{
				File.SetAttributes(serverPath, FileAttributes.Normal);
				File.Copy(localPath, serverPath, true);
				File.SetAttributes(serverPath, FileAttributes.ReadOnly);
			}
			else
			{
				throw new SourceControlException("File does not exist");
			}
		}

		public void CheckOut(string[] paths, bool recursive)
		{
			foreach (string path in paths)
			{
				CheckOut(path, recursive);
			}
		}

		public void CheckOut(string path, bool recursive)
		{
			CheckoutLatestInternal(path, recursive, true);
		}

		void CheckoutLatestInternal(string path, bool recursive, bool doGetLatest)
		{
			Argument.NotNullOrEmpty(path, nameof(path));
			CheckSetupHasBeenRun();

			string serverPath = SourceControlPathsForTest.ConvertLocalPathToServerPath(path);
			string localPath = SourceControlPathsForTest.ConvertServerPathToLocalPath(path);

			if (!File.Exists(serverPath))
			{
				return;
			}

			if (recursive)
			{
				foreach (string pathOfSomething in GetFilesAndDirectoriesForPath(path))
				{
					if (doGetLatest && File.Exists(pathOfSomething))
					{
						File.SetAttributes(localPath, FileAttributes.Normal);
						File.Copy(serverPath, localPath, true);
					}
					File.SetAttributes(localPath, FileAttributes.Normal);
					CheckoutInternal(serverPath);
				}
			}
			else
			{
				if (File.Exists(localPath))
				{
					if (doGetLatest)
					{
						File.SetAttributes(localPath, FileAttributes.Normal);
						if (serverPath != localPath)
						{
							File.Copy(serverPath, localPath, true);
						}
					}
					File.SetAttributes(localPath, FileAttributes.Normal);
					CheckoutInternal(serverPath);
				}
				else
				{
					throw new SourceControlException("File does not exist");
				}
			}
		}

		void CheckoutInternal(string path)
		{
			Argument.NotNullOrEmpty(path, nameof(path));
			string serverPath = SourceControlPathsForTest.ConvertLocalPathToServerPath(path);
			if (!ObjectsCheckedOut.Contains(serverPath))
			{
				ObjectsCheckedOut.Add(serverPath);
			}
		}

		#region GetLatestVersion

		public void GetLatestVersion(string[] paths, WritableFileAction action, bool recursive)
		{
			foreach (string path in paths)
			{
				GetLatestVersion(path, action, recursive);
			}
		}

		public void GetLatestVersion(string path, WritableFileAction action, bool recursive)
		{
			CheckSetupHasBeenRun();
			string serverPath = SourceControlPathsForTest.ConvertLocalPathToServerPath(path);
			string localPath = SourceControlPathsForTest.ConvertServerPathToLocalPath(path);

			try
			{
				if (recursive)
				{
					if (action == WritableFileAction.Merge)
					{
						throw new NotSupportedException();
					}
					else if (action == WritableFileAction.Skip)
					{
						throw new NotSupportedException();
					}
					else if (action == WritableFileAction.Replace)
					{
						foreach (string file in GetFilesForPath(path))
						{
							CopyFileFromSourceControlToLocal(serverPath);
						}
					}
				}
				else
				{
					if (action == WritableFileAction.Merge)
					{
						throw new NotSupportedException();
					}
					else if (action == WritableFileAction.Skip)
					{
						if (!File.Exists(localPath))
						{
							CopyFileFromSourceControlToLocal(serverPath);
						}
						else if ((File.GetAttributes(path) & FileAttributes.ReadOnly) != FileAttributes.ReadOnly)
						{
							return;
						}
						else
						{
							CopyFileFromSourceControlToLocal(serverPath);
						}
					}
					else if (action == WritableFileAction.Replace)
					{
						CopyFileFromSourceControlToLocal(serverPath);
					}
				}
			}
			catch (DirectoryNotFoundException ex)
			{
				throw new SourceControlException(ex.Message, ex);
			}
		}

		void CopyFileFromSourceControlToLocal(string serverPath)
		{
			Argument.NotNullOrEmpty(serverPath, nameof(serverPath)); // Suggested By ReviewBot 
			string localPath = SourceControlPathsForTest.ConvertServerPathToLocalPath(serverPath);

			bool fileExistedBeforeCopy = false;
			FileAttributes localFileAttribs = FileAttributes.Temporary;
			if (File.Exists(localPath))
			{
				localFileAttribs = File.GetAttributes(localPath);
				File.SetAttributes(localPath, FileAttributes.Normal);
				fileExistedBeforeCopy = true;
			}

			File.Copy(serverPath, localPath, true);

			if (!fileExistedBeforeCopy)
			{
				localFileAttribs = FileAttributes.ReadOnly;
			}

			File.SetAttributes(localPath, localFileAttribs);
		}

		#endregion

		#region GetLatestVersionContents

		public string GetLatestVersionContentsAsString(string sourceControlFilePath)
		{
			CheckSetupHasBeenRun();
			sourceControlFilePath = SourceControlPathsForTest.ConvertLocalPathToServerPath(sourceControlFilePath);

			if (sourceControlFilePath.Contains("ENTERPRISE"))
			{
				sourceControlFilePath = sourceControlFilePath.Replace("ENTERPRISE", "MOCKSOURCECONTROLFILES");
			}
			return File.ReadAllText(sourceControlFilePath);
		}

		public byte[] GetLatestVersionContentsAsBytes(string sourceControlFilePath)
		{
			CheckSetupHasBeenRun();
			sourceControlFilePath = SourceControlPathsForTest.ConvertLocalPathToServerPath(sourceControlFilePath);

			if (sourceControlFilePath.Contains("ENTERPRISE"))
			{
				sourceControlFilePath = sourceControlFilePath.Replace("ENTERPRISE", "MOCKSOURCECONTROLFILES");
			}
			return File.ReadAllBytes(sourceControlFilePath);
		}

		public void DownloadLatestVersion(string serverPath, string localPath)
		{
			throw new NotImplementedException();
		}

		#endregion

		public bool IsDifferent(string path)
		{
			CheckSetupHasBeenRun();
			string serverPath = SourceControlPathsForTest.ConvertLocalPathToServerPath(path);
			string localPath = SourceControlPathsForTest.ConvertServerPathToLocalPath(path);

			return (File.ReadAllText(localPath) != File.ReadAllText(serverPath));
		}

		public bool IsFileCheckedOutByMe(string path)
		{
			CheckSetupHasBeenRun();
			string serverPath = SourceControlPathsForTest.ConvertLocalPathToServerPath(path);
			return ObjectsCheckedOut.Contains(serverPath);
		}

		[SuppressMessage("Microsoft.Contracts", "Ensures-Contract.ForAll(Contract.Result<String[]>(), file => !String.IsNullOrEmpty(file))")] //There is no code path that adds a non-empty string to ObjectsCheckedOut. I could add an invariant for it, but I think CC would blow up forever
		public string[] GetFilesWithPendingChanges(bool includeDeletedFiles = true)
		{
			return ObjectsCheckedOut.ToArray();
		}

		public bool IsFileInSourceControl(string path)
		{
			CheckSetupHasBeenRun();
			string serverPath = SourceControlPathsForTest.ConvertLocalPathToServerPath(path);
			return File.Exists(serverPath);
		}

		public bool IsFolderInSourceControl(string path)
		{
			CheckSetupHasBeenRun();
			string serverPath = SourceControlPathsForTest.ConvertLocalPathToServerPath(path);
			return Directory.Exists(serverPath);
		}

		public void UndoCheckOut(string[] paths, bool recursive)
		{
			foreach (string path in paths)
			{
				UndoCheckOut(path, recursive);
			}
		}

		public void UndoCheckOut(string sourceControlPath, bool recursive)
		{
			CheckSetupHasBeenRun();
			string serverPath = SourceControlPathsForTest.ConvertLocalPathToServerPath(sourceControlPath);
			string localPath = SourceControlPathsForTest.ConvertServerPathToLocalPath(sourceControlPath);

			while (ObjectsCheckedOut.Contains(serverPath))
			{
				ObjectsCheckedOut.Remove(serverPath);
				CopyFileFromSourceControlToLocal(serverPath);
				File.SetAttributes(localPath, File.GetAttributes(localPath) | FileAttributes.ReadOnly);
			}
		}

		public bool HasAdequatePermissions(string path)
		{
			return true;
		}

		public string GetServerPath(string path)
		{
			throw new NotImplementedException();
		}

		public List<String> GetFilesList(String tfsPath)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Implementation

		internal const string WorkspaceFolderName = "WORKSPACEFILES";
		internal const string SourceControlFolderName = "SOURCECONTROLFILES";

		[SuppressMessage("CargoWiseOne", "CW1051:BaseSourcePath", Justification = "Baseline")]
		static void CopyMockSourceControlFiles(string target)
		{
			Argument.NotNullOrEmpty(target, nameof(target)); // Suggested By ReviewBot 
			CopyMockSourceControlFiles(new DirectoryInfo(MockSourceControlFilesPath), new DirectoryInfo(target));
		}

		static void CopyMockSourceControlFiles(DirectoryInfo source, DirectoryInfo target)
		{
			Argument.NotNull(target, nameof(target)); // Suggested By ReviewBot 
			Argument.NotNull(source, nameof(source)); // Suggested By ReviewBot 
			if (!target.Exists)
			{
				target.Create();
			}
			foreach (FileInfo sourceFile in source.GetFiles())
			{
				string destFileName = Path.Combine(target.FullName, sourceFile.Name);
				sourceFile.CopyTo(destFileName);
			}
			foreach (DirectoryInfo sourceSubDir in source.GetDirectories())
			{
				string fullPath = Path.Combine(target.FullName, sourceSubDir.Name);
				DirectoryInfo targetSubDir = new DirectoryInfo(fullPath);
				CopyMockSourceControlFiles(sourceSubDir, targetSubDir);
			}
		}

		public static string MockSourceControlPath
		{
			get
			{
				return Path.Combine(WTG.TestHelpers.TestingState.TempPath, SourceControlFolderName);
			}
		}

		public static string MockWorkspacePath
		{
			get
			{
				return Path.Combine(WTG.TestHelpers.TestingState.TempPath, WorkspaceFolderName);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1051:BaseSourcePath", Justification = "Baseline")]
		static string MockSourceControlFilesPath
		{
			get
			{
				string path = Path.Combine(WTG.TestHelpers.TestCase.BaseSourcePath, @"Common\Tools\BuildTools\BuildTools\SourceControl\Testing\MockSourceControlFiles");

				Directory.CreateDirectory(path);

				return path;
			}
		}

		void CheckSetupHasBeenRun()
		{
			if (!Directory.Exists(MockWorkspacePath))
			{
				throw new NotImplementedException("Please call MockSourceControl.Setup() and MockSourceControl.TearDown() in your SetUp and TearDown for this testcase");
			}
		}

		bool CheckinFileToMockSourceControlPath(string path)
		{
			Argument.NotNullOrEmpty(path, nameof(path));
			string localPath = SourceControlPathsForTest.ConvertServerPathToLocalPath(path);
			string serverPath = SourceControlPathsForTest.ConvertLocalPathToServerPath(path);

			if (path.Contains(MockSourceControlPath))
			{
				return false;
			}

			if (File.Exists(serverPath))
			{
				File.SetAttributes(serverPath, FileAttributes.Normal);
			}
			if (localPath != serverPath)
			{
				Directory.CreateDirectory(Path.GetDirectoryName(serverPath));
				File.Copy(localPath, serverPath, true);
				File.SetAttributes(serverPath, FileAttributes.ReadOnly);
			}
			return true;
		}

		string[] GetFilesAndDirectoriesForPath(string path)
		{
			Argument.NotNullOrEmpty(path, nameof(path));
			List<string> result = new List<string>();
			result.AddRange(GetFilesForPath(path));

			string[] directoryList = Directory.GetDirectories(path, String.Empty, SearchOption.AllDirectories);

			foreach (string directory in directoryList)
			{
				result.Add(directory);
			}
			return result.ToArray();
		}

		string[] GetFilesForPath(string path)
		{
			Argument.NotNullOrEmpty(path, nameof(path));
			List<string> result = new List<string>();
			string[] files = Directory.GetFiles(path, String.Empty, SearchOption.AllDirectories);

			foreach (string file in files)
			{
				result.Add(file);
			}
			return result.ToArray();
		}

		public void Dispose()
		{
		}

		public string GetCurrentBranchName()
		{
			throw new NotImplementedException();
		}

		public string[] GetFilesWithChangesInCurrentBranch(IReleaseInfo releaseInfo, bool includeDeletedFiles = true)
		{
			throw new NotImplementedException();
		}

		public class CaseInsensitiveStringList
		{
			public CaseInsensitiveStringList()
			{
				inner = new List<string>();
			}

			public void Add(string item)
			{
				Argument.NotNull(item, nameof(item));
				inner.Add(item);
			}

			public void Remove(string item)
			{
				Argument.NotNull(item, nameof(item));
				inner.Remove(item);
			}

			public bool Contains(string item)
			{
				Argument.NotNull(item, nameof(item));
				foreach (string i in inner)
				{
					if (i.Equals(item, StringComparison.InvariantCultureIgnoreCase))
					{
						return true;
					}
				}
				return false;
			}

			public string[] ToArray()
			{
				return inner.ToArray();
			}

			readonly List<string> inner;
		}

		readonly internal CaseInsensitiveStringList ObjectsCheckedOut = new CaseInsensitiveStringList();

		#endregion
	}
}
#endif
