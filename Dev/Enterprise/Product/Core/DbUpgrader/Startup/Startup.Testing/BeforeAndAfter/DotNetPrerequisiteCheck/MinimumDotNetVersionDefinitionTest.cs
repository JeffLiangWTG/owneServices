using System;
using Enterprise.Upgrades;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.BeforeAndAfter.DotNetPrerequisiteCheck.Testing
{
	sealed class MinimumDotNetVersionDefinitionTest : TestCase
	{
		public void TestIsDotNetSupportedVersion()
		{
			var minimumDotNetVersionDefinition = new MinimumDotNetVersionDefinition();
			AssertEquals("It's a lower version, it should be false", false, minimumDotNetVersionDefinition.IsDotNetSupportedVersion(new DotNetVersion("4.7.2", 0)));
			AssertEquals("It's the same supported version, it should be true", true, minimumDotNetVersionDefinition.IsDotNetSupportedVersion(new DotNetVersion("4.8", 0)));
			AssertEquals("It's a greater version, it should be true", true, minimumDotNetVersionDefinition.IsDotNetSupportedVersion(new DotNetVersion("9.9", 0)));
		}

		public void TestIsAnOldRecord()
		{
			var minimumDotNetVersionDefinition = new MinimumDotNetVersionDefinition();

			var recordDate = DateTime.UtcNow;
			var newPcRecord = new PcWithDotNetVersionRecord("MYPC.DOMAIN", new DotNetVersion("4.6.1", 641654), recordDate);
			AssertEquals("It should be false since the version was recorded less than 2 weeks before the check", false, minimumDotNetVersionDefinition.IsAnOldRecord(newPcRecord));

			recordDate = DateTime.UtcNow.AddDays(-16);
			var oldPcRecord = new PcWithDotNetVersionRecord("MYPC.DOMAIN", new DotNetVersion("4.6.1", 641654), recordDate);
			AssertEquals("It should be true since the version was recorded more than 2 weeks before the check", true, minimumDotNetVersionDefinition.IsAnOldRecord(oldPcRecord));
		}

		public void TestSanityTestForMinimumDotNetVersion()
		{
			var minimumDotNetVersionDefinition = new MinimumDotNetVersionDefinition();
			AssertEquals("Changing the minimum version of .NET can have big impacts on clients upgrades, please use it with care", "4.8", minimumDotNetVersionDefinition.MinimumDotNetVersionRequired);
		}

		public void TestDataFromOldRecorderIsStillValid()
		{
			// An older recorder will still record the true release key, but might not be able to map that to a known .NET version string.
			var minimumDotNetVersionDefinition = new MinimumDotNetVersionDefinition();
			Assert("Minimum version check should use release key as a last resort.", minimumDotNetVersionDefinition.IsDotNetSupportedVersion(new DotNetVersion("4.0.0", 999999)));
		}
	}
}
