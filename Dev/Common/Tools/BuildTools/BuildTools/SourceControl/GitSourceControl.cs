using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Common;
using LibGit2Sharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WTG.DevTools.Definitions;
using WTG.DevTools.SourceControl;

namespace CargoWise.BuildTools
{
	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer only tool")]
	public sealed class GitSourceControl : ISourceControl, IAggregatableSourceControl
	{
		internal GitSourceControl(string gitPath)
		{
			repository = new Repository(gitPath);
		}

		public string WorkingDirectory => repository.Info.WorkingDirectory;

		public bool AddFile(string path)
		{
			return File.Exists(path);
		}

		public void CheckOut(string path, bool recursive)
		{
		}

		public void CheckOut(string[] paths, bool recursive)
		{
		}

		public bool DeleteDirectory(string path)
		{
			if (Directory.Exists(path))
			{
				Directory.Delete(path, true);
				return true;
			}
			return false;
		}

		public bool DeleteFile(string path)
		{
			if (File.Exists(path))
			{
				File.Delete(path);
				return true;
			}
			return false;
		}

		public string[] GetFilesWithPendingChanges(bool includeDeletedFiles = true)
		{
			return GitCli.StatusAsync(repository.Info.WorkingDirectory).GetAwaiter().GetResult()
				.Where(item => includeDeletedFiles || item.Status != GitCliDiffStatus.Deleted)
				.Select(item => Path.GetFullPath(Path.Combine(repository.Info.WorkingDirectory, item.Path)))
				.ToArray();
		}

		bool IsChanged(FileStatus status, bool includeDeletedFiles = true)
		{
			return (includeDeletedFiles && status.HasFlag(FileStatus.DeletedFromIndex))
				|| (includeDeletedFiles && status.HasFlag(FileStatus.DeletedFromWorkdir))
				|| status.HasFlag(FileStatus.ModifiedInIndex)
				|| status.HasFlag(FileStatus.ModifiedInWorkdir)
				|| status.HasFlag(FileStatus.NewInIndex)
				|| status.HasFlag(FileStatus.NewInWorkdir);
		}

		public bool IsDifferent(string path)
		{
			return IsChanged(repository.RetrieveStatus(path));
		}

		public bool IsFileCheckedOutByMe(string path)
		{
			return IsChanged(repository.RetrieveStatus(path));
		}

		public bool IsFileInSourceControl(string path)
		{
			try
			{
				return repository.RetrieveStatus(path) != FileStatus.Ignored;
			}
			catch (AmbiguousSpecificationException)
			{
				return true;
			}
		}

		public bool IsFolderInSourceControl(string path)
		{
			try
			{
				return repository.RetrieveStatus(path) != FileStatus.Ignored;
			}
			catch (AmbiguousSpecificationException)
			{
				return true;
			}
		}

		public void PendAdd(string path)
		{
		}

		public void PendAdd(string[] paths)
		{
		}

		public void PendEdit(string path)
		{
		}

		public void UndoCheckOut(string sourceControlPath, bool recursive)
		{
			UndoCheckOut(new[] { sourceControlPath }, recursive);
		}

		public void UndoCheckOut(string[] paths, bool recursive)
		{
			var newFiles = paths.Where(p => IsNew(repository.RetrieveStatus(p))).ToArray();
			foreach (var newFile in newFiles)
			{
				File.Delete(newFile);
			}
			var options = new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force };
			repository.CheckoutPaths(repository.Head.FriendlyName, paths.Except(newFiles), options);
		}

		bool IsNew(FileStatus status)
		{
			return status.HasFlag(FileStatus.NewInIndex)
				|| status.HasFlag(FileStatus.NewInWorkdir);
		}

		public void SubmitChanges(string staffCode, string name, string criticality, string[] items)
		{
			SubmitChanges(staffCode, name, criticality, items, Environment.UserDomainName + "\\" + Environment.UserName, true);
		}

		public void SubmitChanges(string staffCode, string name, string criticality, string[] items, string datOwnerName, bool mergeChanges = true)
		{
			var startingBranch = repository.Head;
			try
			{
				var branchName = staffCode + "/" + GitExtensions.SafeBranchName(name);
				var branch = repository.CreateBranch(branchName);
				GitCli.CheckoutAsync(repository.Info.WorkingDirectory, branchName).GetAwaiter().GetResult();
				Commands.Stage(repository, items);
				var signature = GetSignature();
				repository.Commit(name, signature, signature);
				var pullRequestUrl = GitExtensions.CreatePullRequestFromLocalBranch(repository, new PullRequestFactory(), branch, startingBranch.FriendlyName, name, string.Empty);
				if (mergeChanges)
				{
					SubmitShelvesetToDAT(null, datOwnerName, pullRequestUrl, criticality, Guid.Empty, SubmissionType.Checkin);
				}
				else
				{
					SubmitShelvesetToDAT(null, datOwnerName, pullRequestUrl, criticality, Guid.Empty, SubmissionType.TestRun);
				}
			}
			finally
			{
				Commands.Checkout(repository, startingBranch);
			}
		}

		Signature GetSignature()
		{
			if (identity != null)
			{
				return new Signature(identity, DateTimeOffset.Now);
			}

			var signature = repository.Config.BuildSignature(DateTimeOffset.Now);
			return signature ?? throw new IOException("Please set a user name and email address in your git global settings or local repository");
		}

		public void UseIdentity(string name, string email)
		{
			identity = new Identity(name, email);
		}
		Identity identity;

		public string GetCurrentBranchName()
		{
			return repository.Head.FriendlyName;
		}

		public string[] GetFilesWithChangesInCurrentBranch(IReleaseInfo releaseInfo, bool includeDeletedFiles = true)
		{
			var sourceBranch = releaseInfo.ReleaseRing == ReleaseRings.Codes.ALP ? "master" : $"releases/CW{releaseInfo.ReleaseDate.ToString("yyyyMMdd")}";
			if (repository.Head.FriendlyName != sourceBranch)
			{
				var mergeBase = GitCli.MergeBaseAsync(repository.Info.WorkingDirectory, sourceBranch, repository.Head.FriendlyName).GetAwaiter().GetResult();
				return GitCli.DiffNameStatusAsync(repository.Info.WorkingDirectory, mergeBase, repository.Head.FriendlyName).GetAwaiter().GetResult()
					.Where(item => includeDeletedFiles || item.Status != GitCliDiffStatus.Deleted)
					.Select(item => Path.GetFullPath(Path.Combine(repository.Info.WorkingDirectory, item.Path)))
					.Concat(GetFilesWithPendingChanges(includeDeletedFiles))
					.Distinct(StringComparer.OrdinalIgnoreCase)
					.ToArray();
			}
			else
			{
				return GetFilesWithPendingChanges(includeDeletedFiles);
			}
		}

		public void Dispose()
		{
			repository.Dispose();
		}

		public ISourceControl WithAdditionalRepository(ISourceControl sourceControl, string rootPath)
		{
			return AggregatedSourceControl.New(
				WorkingDirectory,
				this,
				rootPath,
				sourceControl);
		}

		static void SubmitShelvesetToDAT(string shelfName, string ownerName, string pullRequestUrl, string criticality, Guid processTaskPK, SubmissionType submissionType)
		{
			Argument.NotNull(ownerName, nameof(ownerName));
			Argument.NotNull(criticality, nameof(criticality));

			var apiClient = new SubmissionsApiClient();
			try
			{
				apiClient.CreateNewSubmission(submissionType, ownerName, pullRequestUrl, processTaskPK, criticality);
			}
			catch (Exception ex)
			{
				throw new IOException(string.Format(CultureInfo.CurrentCulture, "Failed to submit {0} for {1}.\r\n\r\n{2}", shelfName ?? pullRequestUrl, ownerName, ex.Message), ex);
			}
		}

		public void UpdateAppsettingProperties(List<string> files, IDictionary<string, object> keyValueToAdd, List<string> keysToDelete)
		{
			foreach (var fileName in files)
			{
				var json = File.ReadAllText($"{WorkingDirectory}/{fileName}");
				var jsonObj = JsonConvert.DeserializeObject<JObject>(json);

				// Update properties
				foreach (var pair in keyValueToAdd)
				{
					var keys = pair.Key.Split(':');
					var currentObj = jsonObj;

					for (var i = 0; i < keys.Length - 1; i++)
					{
						if (currentObj[keys[i]] is JObject)
						{
							currentObj = (JObject)currentObj[keys[i]];
						}
					}

					if (currentObj.ContainsKey(keys.Last()))
					{
						currentObj[keys.Last()] = JToken.FromObject(pair.Value);
					}
				}

				// Delete properties
				foreach (var key in keysToDelete)
				{
					var keys = key.Split(':');
					var currentObj = jsonObj;

					for (var i = 0; i < keys.Length - 1; i++)
					{
						if (currentObj[keys[i]] is JObject)
						{
							currentObj = (JObject)currentObj[keys[i]];
						}
					}

					currentObj.Property(keys.Last())?.Remove();
				}

				var output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
				File.WriteAllText($"{WorkingDirectory}/{fileName}", output);
				AddFile($"{WorkingDirectory}/{fileName}");
			}
		}

		readonly Repository repository;
	}
}
