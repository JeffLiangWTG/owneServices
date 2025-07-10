using System;
using System.Data;
using System.Linq;
using Enterprise.Upgrades;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.BeforeAndAfter.DotNetPrerequisiteCheck.Testing
{
	sealed class DotNetPrerequisiteViolatingPCsProviderTest : TestCase
	{
		public void TestGetWrongDotNetPcsList()
		{
			var versionDefinition = new MinimumDotNetVersionDefinition();
			var pcsProvider = new DotNetPrerequisiteViolatingPCsProvider(versionDefinition);

			var fakeTable = new DataTable();
			fakeTable.Columns.Add("SD_Name");
			fakeTable.Columns.Add("value");
			string[] whiteList = Array.Empty<string>();

			AssertEquals("It should not have problems as it has not records yet", false, pcsProvider.GetPCsViolatingDotNetPrerequisite(fakeTable.Rows, whiteList).Any());

			var dotNetRow = fakeTable.NewRow();
			dotNetRow["SD_Name"] = PcWithDotNetVersionRecord.RegistryPrefix + "|SYDCO-WJDR-1";
			dotNetRow["value"] = "DotNetVersion:5.0|ReleaseNumber:500000|RecordingDate:" + DateTime.UtcNow.ToString("s");
			fakeTable.Rows.Add(dotNetRow);

			AssertEquals("It should not have problems as it has a good DotNetVersion", false, pcsProvider.GetPCsViolatingDotNetPrerequisite(fakeTable.Rows, whiteList).Any());

			var dotNetRow2 = fakeTable.NewRow();
			dotNetRow2["SD_Name"] = PcWithDotNetVersionRecord.RegistryPrefix + "|OLDPC";
			dotNetRow2["value"] = "DotNetVersion:3.5|ReleaseNumber:0|RecordingDate:" + DateTime.UtcNow.ToString("s");
			fakeTable.Rows.Add(dotNetRow2);
			AssertEquals("It should have 1 pc with problems because OLDPC doesnt have a supported version and its not in the whitelist", true, pcsProvider.GetPCsViolatingDotNetPrerequisite(fakeTable.Rows, whiteList).Any());

			string[] whiteList2 = { "OLDPC" };
			AssertEquals("It should have not pc with problems because OLDPC doesnt have a supported version but its in the whitelist", false, pcsProvider.GetPCsViolatingDotNetPrerequisite(fakeTable.Rows, whiteList2).Any());

			string[] whiteList3 = { "OLDP" };
			AssertEquals("It should have 1 pc with problems because OLDPC doesnt have a supported version and its not in the whitelist", true, pcsProvider.GetPCsViolatingDotNetPrerequisite(fakeTable.Rows, whiteList3).Any());
		}
	}
}
