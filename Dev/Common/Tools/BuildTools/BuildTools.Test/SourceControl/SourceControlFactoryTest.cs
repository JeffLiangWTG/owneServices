using System.IO;
using System.Linq;
using CargoWise.IO;
using LibGit2Sharp;
using NUnit.Framework;

namespace CargoWise.BuildTools.Testing
{
	sealed class SourceControlFactoryTest : TestCase
	{
		public void TestGitRepository()
		{
			using (var tempDirectory = new TempDirectory())
			{
				Repository.Init(tempDirectory.DirectoryName);
				using (var sourceControl = SourceControlFactory.Instance.GetSourceControl(tempDirectory.DirectoryName))
				{
					AssertType<GitSourceControl>(sourceControl);
				}
			}
		}

		public void TestFileInSubdirectoryOfGitRepository()
		{
			using (var tempDirectory = new TempDirectory())
			{
				Repository.Init(tempDirectory.DirectoryName);
				Directory.CreateDirectory(Path.Combine(tempDirectory.DirectoryName, "Foo"));
				var path = Path.Combine(tempDirectory.DirectoryName, "Foo", "Bar.txt");
				File.WriteAllText(path, "whatever");
				using (var sourceControl = SourceControlFactory.Instance.GetSourceControl(path))
				{
					AssertType<GitSourceControl>(sourceControl);
				}
			}
		}

		public void TestWithAdditionalRepository()
		{
			using (var tempDirectory1 = new TempDirectory())
			using (var tempDirectory2 = new TempDirectory())
			{
				Repository.Init(tempDirectory1.DirectoryName);
				Repository.Init(tempDirectory2.DirectoryName);

				ISourceControl sc = null;
				try
				{
					sc = SourceControlFactory.Instance.GetSourceControl(tempDirectory1.DirectoryName);
					sc = SourceControlFactory.Instance.WithAdditionalRepository(sc, tempDirectory2.DirectoryName);

					var agg = (AggregatedSourceControl)sc;
					AssertContainsExactElementsInExactOrder(new[] { tempDirectory1.DirectoryName, tempDirectory2.DirectoryName }, agg.Children.Select(x => TrimTrailingSeparator(x.Key)));
					AssertContainsExactElementsInExactOrder(new[] { typeof(GitSourceControl), typeof(GitSourceControl) }, agg.Children.Select(x => x.Value.GetType()));
				}
				finally
				{
					sc?.Dispose();
				}
			}
		}

		static string TrimTrailingSeparator(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return path;
			}

			var finalChar = path[path.Length - 1];

			if (finalChar == Path.DirectorySeparatorChar || finalChar == Path.AltDirectorySeparatorChar)
			{
				return path.Substring(0, path.Length - 1);
			}

			return path;
		}
	}
}
