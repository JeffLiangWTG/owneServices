#if DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CargoWise.Common;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class TempFilesTestListener : BaseTestListener
	{
		public static readonly TempFilesTestListener Instance = new TempFilesTestListener(TimeSpan.Zero);

		public override void EndAllTests(DateTime endTime)
		{
			watcher.EnableRaisingEvents = false;
		}

		internal TempFilesTestListener(TimeSpan timeout)
		{
			watcher = new FileSystemWatcher(EnvProxy.Instance.TempPath);
			watcher.IncludeSubdirectories = true;
			watcher.Error += new ErrorEventHandler(watcher_Error);
			watcher.Created += new FileSystemEventHandler(watcher_Created);
			watcher.Deleted += new FileSystemEventHandler(watcher_Deleted);
			fileChangedEvent = new AutoResetEvent(false);
			this.timeout = timeout;
		}

		readonly FileSystemWatcher watcher;

		public override void StartAllTests(DateTime startTime)
		{
			foreach (string directoryName in Directory.GetDirectories(EnvProxy.Instance.TempPath))
			{
				string directorySuffix = Path.GetFileName(directoryName).ToLower();
				try
				{
					Directory.Delete(Path.Combine(EnvProxy.Instance.TempPath, directoryName), true);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}
			foreach (string fileName in Directory.GetFiles(EnvProxy.Instance.TempPath))
			{
				try
				{
					string fullPathAndName = Path.Combine(EnvProxy.Instance.TempPath, fileName);
					File.SetAttributes(fullPathAndName, FileAttributes.Normal);
					File.Delete(fullPathAndName);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}
			watcher.EnableRaisingEvents = true;
		}

		public override void StartTest(TestCase test, DateTime startTime)
		{
			if (collectorAtStart == null)
			{
				collectorAtStart = GetFileCountIncludingSubDirectories(EnvProxy.Instance.TempPath);
			}
			fileChangedEvent.Reset();
		}

		DirectoryCollector collectorAtStart;
		readonly AutoResetEvent fileChangedEvent;
		readonly TimeSpan timeout;

		public override void EndTest(TestCase test, DateTime endTime)
		{
			if (fileChangedEvent.WaitOne(timeout))
			{
				try
				{
					DirectoryCollector collectorAtEnd = GetFileCountIncludingSubDirectories(EnvProxy.Instance.TempPath);
					collectorAtEnd.Remove(FilesWithDelayedDelete);
					int fileCountChange = collectorAtEnd.Count - collectorAtStart.Count;
					if (fileCountChange > 0)
					{
						string message = (fileCountChange == 1) ? "1 new file or directory was created by this test." : fileCountChange + " new files or directories were created by this test.";
						Assertion.AssertMultilineASCIIEquals(message, collectorAtStart.FilesAndDirectories, collectorAtEnd.FilesAndDirectories);
					}
					FilesWithDelayedDelete.Clear();
				}
				finally
				{
					collectorAtStart = null;
				}
			}
		}

		public List<string> FilesWithDelayedDelete = new List<string>();

		#region Implementation

		void GetRecursiveFileCount(string directoryName, DirectoryCollector collector)
		{
			foreach (var file in Directory.GetFiles(directoryName))
			{
				collector.Add(file);
			}

			foreach (var dir in Directory.GetDirectories(directoryName))
			{
				collector.Add(dir);
				GetRecursiveFileCount(dir, collector);
			}
		}

		class DirectoryCollector
		{
			public DirectoryCollector()
			{
				filesAndDirectories = new List<string>();
			}

			public void Add(string fileOrDirectory)
			{
				filesAndDirectories.Add(fileOrDirectory);
			}

			public void Remove(List<string> filenamesToRemove)
			{
				foreach (string filename in filenamesToRemove)
				{
					filesAndDirectories.Remove(filename);
				}
			}

			public string FilesAndDirectories
			{
				get { return string.Join(System.Environment.NewLine, filesAndDirectories.ToArray()); }
			}

			public int Count
			{
				get { return filesAndDirectories.Count; }
			}

			readonly List<string> filesAndDirectories;
		}

		DirectoryCollector GetFileCountIncludingSubDirectories(string directoryName)
		{
			DirectoryCollector collector = new DirectoryCollector();
			GetRecursiveFileCount(directoryName, collector);
			return collector;
		}

		#endregion

		void MarkFileChanged()
		{
			fileChangedEvent.Set();
		}

		void watcher_Error(object sender, ErrorEventArgs e)
		{
			MarkFileChanged();
		}

		void watcher_Created(object sender, FileSystemEventArgs e)
		{
			MarkFileChanged();
		}

		void watcher_Deleted(object sender, FileSystemEventArgs e)
		{
			MarkFileChanged();
		}
	}
}
#endif
