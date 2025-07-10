using System;

namespace CargoWise.BuildTools
{
	public interface ISourceControl : IDisposable
	{
		bool AddFile(string path);
		bool DeleteFile(string path);
		bool DeleteDirectory(string path);

		void CheckOut(string path, bool recursive);
		void CheckOut(string[] paths, bool recursive);

		bool IsDifferent(string path);
		bool IsFileCheckedOutByMe(string path);
		string[] GetFilesWithPendingChanges(bool includeDeletedFiles = true);
		bool IsFileInSourceControl(string path);
		bool IsFolderInSourceControl(string path);

		void UndoCheckOut(string sourceControlPath, bool recursive);
		void UndoCheckOut(string[] paths, bool recursive);

		void PendAdd(string path);
		void PendAdd(string[] paths);

		void PendEdit(string path);

		void SubmitChanges(string staffCode, string name, string criticality, string[] items);

		string GetCurrentBranchName();
		string[] GetFilesWithChangesInCurrentBranch(IReleaseInfo releaseInfo, bool includeDeletedFiles = true);
	}
}
