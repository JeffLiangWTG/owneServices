using CargoWise.EntityFramework.Testing;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.GUI;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.ArchiveManager.Test.Schedule
{
	public class SecurityRightHelperTest : TestCaseWithFactory
	{
		public void TestGetCheckpoint_InvalidCode()
		{
			var invalidCode = "CAT";
			var checkpoint = SecurityCheckpointHelper.GetCheckpoint(invalidCode);

			AssertNull($"'{invalidCode}' isn't a valid code.", checkpoint);
		}

		public void TestGetCheckpoint_ValidCode()
		{
			void TestCase(string code, SecurityCheckpoint securityCheckpoint)
			{
				var valid = code;
				var expected = securityCheckpoint;
				var checkpoint = SecurityCheckpointHelper.GetCheckpoint(valid);

				AssertEquals($"'{valid}' is a valid code.", expected, checkpoint);
			}

			TestCase(ArchiveManagerConstants.Codes.PDR, Env.Security.ArchiveSchedulePurge);
			TestCase(ArchiveManagerConstants.Codes.PAR, Env.Security.ArchiveRecordsPurge);
			TestCase(ArchiveManagerConstants.Codes.PDO, Env.Security.ArchiveSchedulePurge);
			TestCase(ArchiveManagerConstants.Codes.EST, Env.Security.ArchiveSchedulePurge);
			TestCase(ArchiveManagerConstants.Codes.PAL, Env.Security.ArchiveSchedulePurge);
		}
	}
}
