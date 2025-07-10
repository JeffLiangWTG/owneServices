using System;
using System.Data;
using System.Linq;
using Enterprise.Upgrades;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.BeforeAndAfter.DotNetPrerequisiteCheck.Testing
{
	sealed class DotNetCorePrerequisiteViolatingPCsProviderTest : TestCase
	{
		public void TestGetWrongDotNetPcsList()
		{
			var pcsProvider = new DotNetCorePrerequisiteViolatingPCsProvider();

			var fakeTable = new DataTable();
			fakeTable.Columns.Add("SD_Name");
			fakeTable.Columns.Add("value");
			string[] whiteList = Array.Empty<string>();

			AssertEquals("It should not have problems as it has no records yet", false, pcsProvider.GetPCsViolatingDotNetCorePrerequisite(fakeTable.Rows, whiteList).Any());

			var dotNetRow = fakeTable.NewRow();
			dotNetRow["SD_Name"] = PcWithDotNetCoreRuntimesRecord.RegistryPrefix + "|SYDCO-WJDR-1";
			dotNetRow["value"] = "DotNetCoreRuntimes:DotNetRuntime-8.0.3,DotNetDesktopRuntime-8.0.6,AspNetCoreRuntime-8.0.3|IIS:NotInstalled|IISAspNetCoreModuleVersion:|RecordingDate:" + DateTime.UtcNow.ToString("s");
			fakeTable.Rows.Add(dotNetRow);

			AssertEquals("It should not have problems as it has good DotNet Core Runtimes", false, pcsProvider.GetPCsViolatingDotNetCorePrerequisite(fakeTable.Rows, whiteList).Any());

			var dotNetRow2 = fakeTable.NewRow();
			dotNetRow2["SD_Name"] = PcWithDotNetCoreRuntimesRecord.RegistryPrefix + "|OLDPC";
			dotNetRow2["value"] = "DotNetCoreRuntimes:DotNetRuntime-7.0.3,DotNetDesktopRuntime-7.0.6,AspNetCoreRuntime-7.0.3|IIS:NotInstalled|IISAspNetCoreModuleVersion:|RecordingDate:" + DateTime.UtcNow.ToString("s");
			fakeTable.Rows.Add(dotNetRow2);
			AssertEquals("It should have 1 pc with problems because OLDPC doesn't have a supported version and its not in the whitelist", true, pcsProvider.GetPCsViolatingDotNetCorePrerequisite(fakeTable.Rows, whiteList).Any());

			string[] whiteList2 = { "OLDPC" };
			AssertEquals("It should have no pc with problems because OLDPC doesn't have a supported version but its in the whitelist", false, pcsProvider.GetPCsViolatingDotNetCorePrerequisite(fakeTable.Rows, whiteList2).Any());

			string[] whiteList3 = { "OLDP" };
			AssertEquals("It should have 1 pc with problems because OLDPC doesn't have a supported version and its not in the whitelist", true, pcsProvider.GetPCsViolatingDotNetCorePrerequisite(fakeTable.Rows, whiteList3).Any());
		}
	}
}
