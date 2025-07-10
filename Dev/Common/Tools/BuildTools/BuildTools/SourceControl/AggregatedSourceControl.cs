using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.BuildTools
{
	public sealed class AggregatedSourceControl : ISourceControl, IAggregatableSourceControl
	{
		public static ISourceControl New(string rootPath1, ISourceControl sourceControl1, string rootPath2, ISourceControl sourceControl2, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
		{
			if (rootPath1.Equals(rootPath2, stringComparison))
			{
				if (sourceControl2 != sourceControl1)
				{
					// as far as the caller is concerned, ownership of the sourceControl's has been passed to us, so if we don't retain it we must dispose it.
					sourceControl2.Dispose();
				}

				return sourceControl1;
			}

			return new AggregatedSourceControl(
				new[] { rootPath1, rootPath2 },
				new[] { sourceControl1, sourceControl2 },
				stringComparison);
		}

		AggregatedSourceControl(string[] rootPaths, ISourceControl[] sourceControls, StringComparison stringComparison)
		{
			this.rootPaths = rootPaths ?? throw new ArgumentNullException(nameof(rootPaths));
			this.sourceControls = sourceControls ?? throw new ArgumentNullException(nameof(sourceControls));
			this.stringComparison = stringComparison;
		}

		public bool AddFile(string path) => SelectSourceControl(path).AddFile(path);
		public bool DeleteFile(string path) => SelectSourceControl(path).DeleteFile(path);
		public bool DeleteDirectory(string path) => SelectSourceControl(path).DeleteDirectory(path);
		public void CheckOut(string path, bool recursive) => SelectSourceControl(path).CheckOut(path, recursive);
		public bool IsDifferent(string path) => SelectSourceControl(path).IsDifferent(path);
		public bool IsFileCheckedOutByMe(string path) => SelectSourceControl(path).IsFileCheckedOutByMe(path);
		public bool IsFileInSourceControl(string path) => SelectSourceControl(path).IsFileInSourceControl(path);
		public bool IsFolderInSourceControl(string path) => SelectSourceControl(path).IsFolderInSourceControl(path);
		public void UndoCheckOut(string sourceControlPath, bool recursive) => SelectSourceControl(sourceControlPath).UndoCheckOut(sourceControlPath, recursive);
		public void PendAdd(string path) => SelectSourceControl(path).PendAdd(path);
		public void PendEdit(string path) => SelectSourceControl(path).PendEdit(path);

		public void SubmitChanges(string staffCode, string name, string criticality, string[] items)
		{
			foreach (var sc in sourceControls)
			{
				sc.SubmitChanges(staffCode, name, criticality, items);
			}
		}

		public void CheckOut(string[] paths, bool recursive)
		{
			foreach (var group in GroupBySourceControl(paths))
			{
				group.Key.CheckOut(group.ToArray(), recursive);
			}
		}

		public string[] GetFilesWithPendingChanges(bool includeDeletedFiles = true)
		{
			return sourceControls.SelectMany(x => x.GetFilesWithPendingChanges(includeDeletedFiles)).ToArray();
		}

		public void UndoCheckOut(string[] paths, bool recursive)
		{
			foreach (var group in GroupBySourceControl(paths))
			{
				group.Key.UndoCheckOut(group.ToArray(), recursive);
			}
		}

		public void PendAdd(string[] paths)
		{
			foreach (var group in GroupBySourceControl(paths))
			{
				group.Key.PendAdd(group.ToArray());
			}
		}

		public string GetCurrentBranchName() => sourceControls[0].GetCurrentBranchName();

		public string[] GetFilesWithChangesInCurrentBranch(IReleaseInfo releaseInfo, bool includeDeletedFiles = true)
		{
			return sourceControls.SelectMany(x => x.GetFilesWithChangesInCurrentBranch(releaseInfo, includeDeletedFiles)).ToArray();
		}

		public void Dispose()
		{
			foreach (var sc in sourceControls)
			{
				sc.Dispose();
			}
		}

		public ISourceControl WithAdditionalRepository(ISourceControl sourceControl, string rootPath)
		{
			for (var i = 0; i < rootPaths.Length; i++)
			{
				if (string.Equals(rootPaths[i], rootPath, stringComparison))
				{
					if (sourceControls[i] != sourceControl)
					{
						sourceControl.Dispose();
					}

					// we already have the provided path, we don't need to add it again.
					return this;
				}
			}

			var n = rootPaths.Length;
			Array.Resize(ref rootPaths, n + 1);
			Array.Resize(ref sourceControls, n + 1);
			rootPaths[n] = rootPath;
			sourceControls[n] = sourceControl;
			return this;
		}

		public IEnumerable<KeyValuePair<string, ISourceControl>> Children
		{
			get
			{
				for (var i = 0; i < rootPaths.Length; i++)
				{
					yield return new KeyValuePair<string, ISourceControl>(rootPaths[i], sourceControls[i]);
				}
			}
		}

		ISourceControl SelectSourceControl(string path)
		{
			var len = 0;
			var selected = -1;

			for (var i = 0; i < rootPaths.Length; i++)
			{
				var root = rootPaths[i];
				if (path.StartsWith(root, stringComparison))
				{
					if (root.Length > len)
					{
						selected = i;
						len = root.Length;
					}
				}
			}

			if (len > 0)
			{
				return sourceControls[selected];
			}

			throw new ArgumentException("No suitable repository was found.");
		}

		ILookup<ISourceControl, string> GroupBySourceControl(string[] paths) => paths.ToLookup(SelectSourceControl);

		readonly StringComparison stringComparison;
		string[] rootPaths;
		ISourceControl[] sourceControls;
	}
}
