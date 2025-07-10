using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;

namespace Enterprise.Client.EDI.AutoDeploy.Business.Test
{
	internal class UpgradesToClientValidationTest : BusinessObjectValidationTestCase
	{
		public void TestRequestedUpgradeMethod()
		{
			UpgradesToClient upgrade = Factory.New<UpgradesToClient>();

			upgrade.L1_RequestedUpgradeMethodInfo.Value = new ZString("");
			AssertNoErrors("No errors when everything is empty", upgrade.L1_RequestedUpgradeMethodInfo);

			upgrade.L1_RequestedUpgradeMethodInfo.Value = new ZString(UpgradeMethods.Codes.Http);
			AssertNoErrors(upgrade.L1_RequestedUpgradeMethodInfo);

			upgrade.L1_RequestedUpgradeMethodInfo.Value = new ZString(UpgradeMethods.Codes.Blocked);
			AssertNoErrors(upgrade.L1_RequestedUpgradeMethodInfo);

			upgrade.L1_RequestedUpgradeMethodInfo.Value = new ZString("XYZ");
			AssertHasError(upgrade.L1_RequestedUpgradeMethodInfo, "Enter a valid selection.");
		}

		public void TestActualUpgradeMethod()
		{
			EDIOrgHeader testHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.GenerateNewLicenceCode();

			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			database.LD_PublicEmailAddressForUpdate = "test@edi.com.au";

			ReleaseBuild build = Factory.New<ReleaseBuild>();
			HttpDownload httpVersion = new HttpDownload();
			build.HL_MajorVersion = httpVersion.VersionMajorNumber;
			build.HL_MinorVersion = httpVersion.VersionMinorNumber;
			build.HL_Release = httpVersion.VersionReleaseNumber - 1;
			build.HL_Patch = 0;
			database.LD_HL_CurrentRunningVersion = build.PK;

			UpgradesToClient upgrade = Factory.New<UpgradesToClient>();

			upgrade.L1_ActualUpgradeMethodInfo.Value = new ZString("");
			AssertNoErrors("No errors when everything is empty", upgrade.L1_ActualUpgradeMethodInfo);

			upgrade.L1_LD = database.PK;
			AssertNoErrors(upgrade.L1_ActualUpgradeMethodInfo);

			upgrade.L1_ActualUpgradeMethodInfo.Value = new ZString(UpgradeMethods.Codes.Http);
			AssertHasError(upgrade.L1_ActualUpgradeMethodInfo, "Actual Upgrade Method is not supported by the selected server.");

			build.HL_Release++;

			upgrade.L1_ActualUpgradeMethodInfo.Value = new ZString(UpgradeMethods.Codes.Blocked);
			AssertNoErrors(upgrade.L1_ActualUpgradeMethodInfo);

			upgrade.L1_ActualUpgradeMethodInfo.Value = new ZString(UpgradeMethods.Codes.Http);
			AssertNoErrors(upgrade.L1_ActualUpgradeMethodInfo);

			upgrade.L1_ActualUpgradeMethodInfo.Value = new ZString("XYZ");
			AssertHasError(upgrade.L1_ActualUpgradeMethodInfo, "Enter a valid selection.");
		}

		public void TestCurrentStatus()
		{
			UpgradesToClient upgrade = Factory.New<UpgradesToClient>();

			upgrade.L1_CurrentStatusInfo.Value = new ZString("");
			AssertNoErrors("No errors when everything is empty", upgrade.L1_CurrentStatusInfo);

			upgrade.L1_CurrentStatusInfo.Value = new ZString(UpgradesToClientStatus.Codes.Blocked);
			AssertNoErrors(upgrade.L1_CurrentStatusInfo);

			upgrade.L1_CurrentStatusInfo.Value = new ZString(UpgradesToClientStatus.Codes.Failed);
			AssertNoErrors(upgrade.L1_CurrentStatusInfo);

			upgrade.L1_CurrentStatusInfo.Value = new ZString(UpgradesToClientStatus.Codes.Processed);
			AssertNoErrors(upgrade.L1_CurrentStatusInfo);

			upgrade.L1_CurrentStatusInfo.Value = new ZString(UpgradesToClientStatus.Codes.Queued);
			AssertNoErrors(upgrade.L1_CurrentStatusInfo);

			upgrade.L1_CurrentStatusInfo.Value = new ZString(UpgradesToClientStatus.Codes.Received);
			AssertNoErrors(upgrade.L1_CurrentStatusInfo);

			upgrade.L1_CurrentStatusInfo.Value = new ZString(UpgradesToClientStatus.Codes.Sent);
			AssertNoErrors(upgrade.L1_CurrentStatusInfo);

			upgrade.L1_CurrentStatusInfo.Value = new ZString("XYZ");
			AssertHasError(upgrade.L1_CurrentStatusInfo, "Enter a valid selection.");
		}
	}
}