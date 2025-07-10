using System;
using System.IO;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class VerboseLoggerTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestCanLog()
		{
			var logger = new VerboseLogger("");
			Assert(!logger.CanLog);
			logger = new VerboseLogger(null);
			Assert(!logger.CanLog);
			logger = new VerboseLogger(FilenameForTesting);
			Assert(logger.CanLog);
		}

		public void TestFileIsCreated()
		{
			Assert(!File.Exists(FilenameForTesting));
			var logger = new VerboseLogger(FilenameForTesting);
			Assert(File.Exists(FilenameForTesting));
		}

		public void TestLogIsAdded()
		{
			Assert(!File.Exists(FilenameForTesting));
			var logger = new VerboseLogger(FilenameForTesting);
			Assert(string.IsNullOrEmpty(File.ReadAllText(FilenameForTesting)));
			logger.Log(LogType.Warning, "hello");
			string text = File.ReadAllText(FilenameForTesting);
			Assert(text.Contains("Warning"));
			Assert(text.Contains("hello"));

			logger.Log(LogType.Error, "bye", new Exception("muhaha"));
			text = File.ReadAllText(FilenameForTesting);
			Assert(text.Contains("Error"));
			Assert(text.Contains("bye"));
			Assert(text.Contains("muhaha"));
		}

		protected override void SetUp()
		{
			File.Delete(FilenameForTesting);
		}

		protected override void TearDown()
		{
			base.TearDown();
			string err = "";
			TempFile.TryDelete(FilenameForTesting, out err, false);
		}

		readonly string FilenameForTesting = Path.Combine(EnvProxy.Instance.TempPath, "forverbosetesting.txt");
	}
}
