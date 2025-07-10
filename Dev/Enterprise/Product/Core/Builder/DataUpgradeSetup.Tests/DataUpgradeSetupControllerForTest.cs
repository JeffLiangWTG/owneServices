using System;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.BuildTools.Testing;
using Enterprise.DbUpgrader.Data;

namespace Enterprise.Builder.DataUpgradeSetup.Testing
{
	public class DataUpgradeSetupControllerForTest : DataUpgradeSetupController
	{
		public DataUpgradeSetupControllerForTest()
			: base(Array.Empty<UpgradeTask>())
		{
			TaskSetupList.Add(new DataTaskSetupForTest(this));
		}

		public DataTaskSetupForTest TestTaskSetup
		{
			get { return (DataTaskSetupForTest)TaskSetupList[0]; }
		}

		public UpgradeTask TestUpgradeTask
		{
			get { return TestTaskSetup.TestUpgradeTask; }
		}

		public DataFile TestDataFile
		{
			get { return TestTaskSetup.TestDataFile; }
		}

		public string DataVersionFileContents
		{
			get { return File.ReadAllText(DataVersionFile); }
			set { File.WriteAllText(DataVersionFile, value); }
		}

		public static string TestDataVersionFile
		{
			get { return Path.Combine(MockSourceControl.MockWorkspacePath, @"DataUpgradeSetup\TestDataVersion.txt"); }
		}

		public override string DataVersionFile
		{
			get { return TestDataVersionFile; }
		}

		protected override void VerifyDbSchemaVersionIsLatest()
		{
		}

		public int MajorVersionInDataVersionFile
		{
			get
			{
				var regMatch = Regex.Match(DataVersionFileContents, AppMajorVersionPattern);
				var versionString = regMatch.ToString();
				return Convert.ToInt32(versionString);
			}
		}

		public int MinorVersionInDataVersionFile
		{
			get
			{
				var regMatch = Regex.Match(DataVersionFileContents, AppMinorVersionPattern);
				var versionString = regMatch.ToString();
				return Convert.ToInt32(versionString);
			}
		}
	}
}
