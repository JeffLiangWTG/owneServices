using System;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	public class EDIOrganisationSecurityProviderTest : OrganisationSecurityProviderTest
	{
		protected override OrganisationSecurityProvider GetNewSecurityProvider()
		{
			return new EDIOrganisationSecurityProvider(Factory.New<EDIOrgHeader>());
		}

		protected override Type TypeOfSecurityProvider
		{
			get { return typeof(EDIOrganisationSecurityProvider); }
		}

		EDIOrganisationSecurityProvider testSecurityProvider;

		protected override void SetUp()
		{
			base.SetUp();
			testSecurityProvider = (EDIOrganisationSecurityProvider)GetNewSecurityProvider();
		}

		#region TestHasModifyLicence

		public void TestHasModifyLicenceSecurity()
		{
			bool oldValue = EDISecurityCheckpoints.OrgLicenceModify.IsAllowed;

			try
			{
				EDISecurityCheckpoints.OrgLicenceModify.IsAllowed = false;
				AssertEquals("HasModifyLicence should NOT be allowed", false, testSecurityProvider.HasModifyLicenceSecurity);

				EDISecurityCheckpoints.OrgLicenceModify.IsAllowed = true;
				AssertEquals("HasModifyLicence should be allowed", true, testSecurityProvider.HasModifyLicenceSecurity);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModify.IsAllowed = oldValue;
			}
		}

		#endregion

		#region TestHasModifyLicenceLicenceKey

		public void TestHasModifyLicenceLicenceKeySecurity()
		{
			bool oldValue = EDISecurityCheckpoints.OrgLicenceModifyLicenceKey.IsAllowed;

			try
			{
				EDISecurityCheckpoints.OrgLicenceModifyLicenceKey.IsAllowed = false;
				AssertEquals("HasModifyLicenceKeyConfiguration should NOT be allowed", false, testSecurityProvider.HasModifyLicenceLicenceKeySecurity);

				EDISecurityCheckpoints.OrgLicenceModifyLicenceKey.IsAllowed = true;
				AssertEquals("HasModifyLicenceKeyConfiguration should be allowed", true, testSecurityProvider.HasModifyLicenceLicenceKeySecurity);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModifyLicenceKey.IsAllowed = oldValue;
			}
		}

		#endregion

		#region TestHasModifyLicenceInstallationDetails

		public void TestHasModifyLicenceInstallationDetailsSecurity()
		{
			bool oldValue = EDISecurityCheckpoints.OrgLicenceModifyInstallationDetails.IsAllowed;

			try
			{
				EDISecurityCheckpoints.OrgLicenceModifyInstallationDetails.IsAllowed = false;
				AssertEquals("HasModifyLicenceInstallationDetailsConfiguration should NOT be allowed", false, testSecurityProvider.HasModifyLicenceInstallationDetailsSecurity);

				EDISecurityCheckpoints.OrgLicenceModifyInstallationDetails.IsAllowed = true;
				AssertEquals("HasModifyLicenceInstallationDetailsConfiguration should be allowed", true, testSecurityProvider.HasModifyLicenceInstallationDetailsSecurity);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModifyInstallationDetails.IsAllowed = oldValue;
			}
		}

		#endregion

		#region TestHasModifyLicenceConnectionDetails

		public void TestHasModifyLicenceConnectionDetailsSecurity()
		{
			bool oldValue = EDISecurityCheckpoints.OrgLicenceModifyConnectionDetails.IsAllowed;

			try
			{
				EDISecurityCheckpoints.OrgLicenceModifyConnectionDetails.IsAllowed = false;
				AssertEquals("HasModifyLicenceConnectionDetailsConfiguration should NOT be allowed", false, testSecurityProvider.HasModifyLicenceConnectionDetailsSecurity);

				EDISecurityCheckpoints.OrgLicenceModifyConnectionDetails.IsAllowed = true;
				AssertEquals("HasModifyLicenceConnectionDetailsConfiguration should be allowed", true, testSecurityProvider.HasModifyLicenceConnectionDetailsSecurity);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModifyConnectionDetails.IsAllowed = oldValue;
			}
		}

		#endregion

		#region TestHasModifyLicenceSendUpgrade

		public void TestHasModifyLicenceSendUpgradeSecurity()
		{
			bool oldValue = EDISecurityCheckpoints.OrgLicenceModifySendUpgrade.IsAllowed;

			try
			{
				EDISecurityCheckpoints.OrgLicenceModifySendUpgrade.IsAllowed = false;
				AssertEquals("HasLicenceModifySendUpgrade should NOT be allowed", false, testSecurityProvider.HasModifyLicenceSendUpgradeSecurity);

				EDISecurityCheckpoints.OrgLicenceModifySendUpgrade.IsAllowed = true;
				AssertEquals("HasLicenceModifySendUpgrade should be allowed", true, testSecurityProvider.HasModifyLicenceSendUpgradeSecurity);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModifySendUpgrade.IsAllowed = oldValue;
			}
		}

		#endregion

		#region TestHasModifyLicenceSaveUpgradeToDisk

		public void TestHasModifyLicenceSaveUpgradeToDiskSecurity()
		{
			bool oldValue = EDISecurityCheckpoints.OrgLicenceModifySaveUpgradeToDisk.IsAllowed;

			try
			{
				EDISecurityCheckpoints.OrgLicenceModifySaveUpgradeToDisk.IsAllowed = false;
				AssertEquals("HasLicenceModifySaveUpgradeToDisk should NOT be allowed", false, testSecurityProvider.HasModifyLicenceSaveUpgradeToDiskSecurity);

				EDISecurityCheckpoints.OrgLicenceModifySaveUpgradeToDisk.IsAllowed = true;
				AssertEquals("HasLicenceModifySaveUpgradeToDisk should be allowed", true, testSecurityProvider.HasModifyLicenceSaveUpgradeToDiskSecurity);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModifySaveUpgradeToDisk.IsAllowed = oldValue;
			}
		}

		#endregion

		#region TestHasModifyLicenceSendUpgrade

		public void TestHasModifyLicenceSendUpgradeToHigherRingDBSecurity()
		{
			bool oldValue = EDISecurityCheckpoints.OrgLicenceModifySendUpgradeToHigherRingDB.IsAllowed;

			try
			{
				EDISecurityCheckpoints.OrgLicenceModifySendUpgradeToHigherRingDB.IsAllowed = false;
				AssertEquals("HasModifyLicenceSendUpgradeToHigherRingDBSecurity should NOT be allowed", false, testSecurityProvider.HasModifyLicenceSendUpgradeToHigherRingDBSecurity);

				EDISecurityCheckpoints.OrgLicenceModifySendUpgradeToHigherRingDB.IsAllowed = true;
				AssertEquals("HasModifyLicenceSendUpgradeToHigherRingDBSecurity should be allowed", true, testSecurityProvider.HasModifyLicenceSendUpgradeToHigherRingDBSecurity);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModifySendUpgradeToHigherRingDB.IsAllowed = oldValue;
			}
		}

		#endregion

		#region TestHasModifyLicenceDatabaseConfiguration

		public void TestHasModifyLicenceDatabaseConfigurationSecurity()
		{
			bool oldValue = EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails.IsAllowed;

			try
			{
				EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails.IsAllowed = false;
				AssertEquals("HasModifyLicenceDatabaseConfiguration should NOT be allowed", false, testSecurityProvider.HasModifyLicenceDatabaseConfigurationSecurity);

				EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails.IsAllowed = true;
				AssertEquals("HasModifyLicenceDatabaseConfiguration should be allowed", true, testSecurityProvider.HasModifyLicenceDatabaseConfigurationSecurity);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails.IsAllowed = oldValue;
			}
		}

		#endregion

		#region HasModifyLicence3rdPartySoftware

		public void TestHasModifyLicence3rdPartySoftwareSecurity()
		{
			bool oldValue = EDISecurityCheckpoints.OrgLicenceModify3rdPartySoftware.IsAllowed;

			try
			{
				EDISecurityCheckpoints.OrgLicenceModify3rdPartySoftware.IsAllowed = false;
				AssertEquals("HasModifyLicence3rdPartySoftware should NOT be allowed", false, testSecurityProvider.HasModifyLicence3rdPartySoftwareSecurity);

				EDISecurityCheckpoints.OrgLicenceModify3rdPartySoftware.IsAllowed = true;
				AssertEquals("HasModifyLicence3rdPartySoftware should be allowed", true, testSecurityProvider.HasModifyLicence3rdPartySoftwareSecurity);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModify3rdPartySoftware.IsAllowed = oldValue;
			}
		}

		#endregion
	}
}
