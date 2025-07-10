using System;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.IO;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.Environment.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class TempFileCleanupTaskTest : BackgroundApplicationStartupTaskTest<TempFileCleanupTask>
	{
		public void TestCleanTempFolder()
		{
			string baseTempDirectory = new DirectoryInfo(Temp.TempPath).Parent.FullName;
			string oldTemp = Path.Combine(baseTempDirectory, "bla");
			Directory.CreateDirectory(oldTemp);
			File.WriteAllText(Path.Combine(oldTemp, "one.txt"), "one");
			File.WriteAllText(Path.Combine(oldTemp, "two.txt"), "two");
			File.SetLastWriteTimeUtc(Path.Combine(oldTemp, "one.txt"), DateTime.UtcNow.AddMinutes(-4));
			File.SetLastWriteTimeUtc(Path.Combine(oldTemp, "two.txt"), DateTime.UtcNow.AddMinutes(-4));
			int oldPid = 0;
			var processes = ProcessLocator.Instance.GetProcessIDs();
			for (int i = 55555; i < 99999; i++)
			{
				if (!processes.Any(id => id == i))
				{
					oldPid = i;
					break;
				}
			}
			string anotherOldTemp = Path.Combine(baseTempDirectory, oldPid.ToString());
			Directory.CreateDirectory(anotherOldTemp);
			File.WriteAllText(Path.Combine(anotherOldTemp, "three.txt"), "three");
			File.SetLastWriteTimeUtc(Path.Combine(anotherOldTemp, "three.txt"), DateTime.UtcNow.AddMinutes(-4));
			File.WriteAllText(Path.Combine(anotherOldTemp, "four.txt"), "four");
			File.SetLastWriteTimeUtc(Path.Combine(anotherOldTemp, "four.txt"), DateTime.UtcNow.AddMinutes(-4));

			string recursiveDirectory = Path.Combine(anotherOldTemp, "bla");
			Directory.CreateDirectory(recursiveDirectory);
			File.WriteAllText(Path.Combine(recursiveDirectory, "five.txt"), "five");
			File.SetLastWriteTimeUtc(Path.Combine(recursiveDirectory, "five.txt"), DateTime.UtcNow.AddMinutes(-4));
			File.WriteAllText(Path.Combine(baseTempDirectory, "foo.txt"), "foo");
			File.SetLastWriteTimeUtc(Path.Combine(baseTempDirectory, "foo.txt"), DateTime.UtcNow.AddMinutes(-4));
			Directory.SetLastWriteTimeUtc(recursiveDirectory, DateTime.UtcNow.AddMinutes(-4));
			Directory.SetLastWriteTimeUtc(anotherOldTemp, DateTime.UtcNow.AddMinutes(-4));
			Directory.SetLastWriteTimeUtc(oldTemp, DateTime.UtcNow.AddMinutes(-4));

			using (TempFile fileForCurrentProcess = TempFile.New())
			{
				new TempFileCleanupTask().CleanTempFolder();

				var fpath = Path.Combine(baseTempDirectory, "foo.txt");
				if (Directory.GetFiles(baseTempDirectory).Contains(fpath))
				{
					File.Delete(fpath);
					Fail("foo.txt should have been deleted");
				}
				AssertCollectionContains(oldTemp, Directory.GetDirectories(baseTempDirectory));
				AssertCollectionNotContains(anotherOldTemp, Directory.GetDirectories(baseTempDirectory));
				Assert(File.Exists(fileForCurrentProcess.Filename));
			}
		}

		public void TestShouldExecute()
		{
			var task = new TempFileCleanupTask();
			Assert(task.ShouldExecute(new ApplicationArguments(Array.Empty<string>())));
			Assert(task.ShouldExecute(new ApplicationArguments(new string[] { "-IAmDoingTheWrongThingByRunningEnterpriseWithoutLoader" })));
			Assert(task.ShouldExecute(new ApplicationArguments(new string[] { "-SkipVersionCheck" })));
			Assert(task.ShouldExecute(new ApplicationArguments(new string[] { "-IAmDoingTheWrongThingByRunningEnterpriseWithoutLoader", "-SkipVersionCheck" })));
		}

		[ExpectNoExceptions]
		public void TestExecute_ShouldCleanOldTemplateCacheRecords()
		{
			var mocks = new MockRepository(MockBehavior.Default);
			var cache = mocks.Create<ITemplateCache>();
			cache.Setup(m => m.CacheTimeout).Returns(TemplateCache.Instance.CacheTimeout);
			cache.Setup(m => m.PurgeOldRecords());
			TemplateCache.Instance = cache.Object;
			try
			{
				using (InstallationEnvironmentForTest.TempDirectoryForTest())
				{
					new TempFileCleanupTask().DoExecute();
				}
			}
			finally
			{
				TemplateCache.Instance = null;
			}
		}

		public void TestExecute_ShouldNotPurgeTemplateCache()
		{
			var templateFile = Path.Combine(TemplateCache.CacheBasePath, "something.xls");

			Func<int> getTemplateCacheCount = () => Directory.GetFiles(TemplateCache.CacheBasePath).Length;

			Directory.CreateDirectory(TemplateCache.CacheBasePath);
			TemplateCache.Instance.Clear();

			var startingCacheItemCount = getTemplateCacheCount();

			File.WriteAllText(templateFile, "blah blah blah");

			AssertEquals(startingCacheItemCount + 1, getTemplateCacheCount());

			using (InstallationEnvironmentForTest.TempDirectoryForTest())
			{
				new TempFileCleanupTask().DoExecute();
			}
			AssertEquals("Task should not wipe template cache", startingCacheItemCount + 1, getTemplateCacheCount());

			TemplateCache.Instance.Clear();
		}

		public override int DefaultErrorExitCode => ExitCodes.TempFileCleanupTaskError;
	}
}
