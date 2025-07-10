using Enterprise.Licensing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.Business.Test
{
	internal class LegacyLicenceCheckpointTest : TestCase
	{
		public void TestGetDefaultLicenceValue()
		{
			var licences = new LegacyLicence();
			var checkpoint = new LegacyLicenceCheckpoint("Name1", "DisplayName1", licences, "");
			AssertEquals("normal checkpoint", LicenceTypes.Codes.ODM, checkpoint.GetDefaultLicenceValue());

			checkpoint.IsDefaultTransactional = true;
			AssertEquals("auto-enabled transactional checkpoint", LicenceTypes.Codes.CPT, checkpoint.GetDefaultLicenceValue());

			checkpoint = new LegacyLicenceCheckpoint("Name2", "DisplayName2", licences, "", isManuallyEnabled: true);
			checkpoint.IsDefaultTransactional = true;
			AssertEquals("manually-enabled transactional checkpoint", LicenceTypes.Codes.NON, checkpoint.GetDefaultLicenceValue());

			checkpoint = new LegacyLicenceCheckpoint("Name3", "DisplayName3", licences, "", isManuallyEnabled: true);
			AssertEquals("manually-enabled normal checkpoint", LicenceTypes.Codes.NON, checkpoint.GetDefaultLicenceValue());
		}
	}
}