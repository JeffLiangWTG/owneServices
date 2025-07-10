using System;
using System.IO;
using CargoWise.Common;
using Enterprise.ZArchitecture.Environment;
using Newtonsoft.Json;

namespace CWNUnit.TestAdapter
{
	public static class TestOptionsManager
	{
		public const string OptionsFileName = "TestAdapter.json";

		public static ITestOptions LoadOptions()
		{
			#region Test stuff
#if DEBUG
			if (Globals.IsTest && overrideTestOptions != null)
			{
				return overrideTestOptions;
			}
#endif
			#endregion

			var path = Path.GetDirectoryName(typeof(TestOptionsManager).Assembly.Location);
			return LoadOptions(path, OptionsFileName);
		}

		public static ITestOptions LoadOptions(string path, string optionsFileName)
		{
			var fileLocation = GetOptionsFileLocation(path, optionsFileName);
			if (!string.IsNullOrEmpty(fileLocation))
			{
				var optionsJson = File.ReadAllText(fileLocation);
				try
				{
					var testOptions = JsonConvert.DeserializeObject<TestOptions>(optionsJson);
					return testOptions;
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error parsing options file {fileLocation}: {ex}");
				}
			}

			return new TestOptions
			{
				Enabled = true,
				DatabaseName = "Odyssey",
				BreakOnPerformanceIssues = false,
				IncludeAmnestyTests = true,
				IncludeDeveloperOnlyTests = true,
				EnableTaskTestListener = false,
			};
		}

		public static string GetOptionsFileLocation(string path, string optionsFileName)
		{
			while (!string.IsNullOrEmpty(path))
			{
				var currentPathFileLocation = Path.Combine(path, optionsFileName);
				if (File.Exists(currentPathFileLocation))
				{
					return currentPathFileLocation;
				}

				path = Path.GetDirectoryName(path);
			}
			return null;
		}

		public static bool IsTestAdapterEnabled => LoadOptions().Enabled;

		#region Test stuff
#if DEBUG

		public static IDisposable OverrideTestOptions(ITestOptions testOptions)
		{
			overrideTestOptions = testOptions;
			return new DisposableAction(() => overrideTestOptions = null);
		}

		[ThreadStatic]
		static ITestOptions overrideTestOptions;

#endif

		#endregion
	}
}
