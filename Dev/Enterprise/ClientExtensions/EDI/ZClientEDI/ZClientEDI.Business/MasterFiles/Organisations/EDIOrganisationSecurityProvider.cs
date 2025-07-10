using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrganisationSecurityProvider : OrganisationSecurityProvider
	{
		public EDIOrganisationSecurityProvider(EDIOrgHeader header) : base(header)
		{
		}

		public bool HasModifyLicenceSecurity
		{
			get { return EDISecurityCheckpoints.OrgLicenceModify.IsAllowed; }
		}

		public bool HasModifyLicenceLicenceKeySecurity
		{
			get { return EDISecurityCheckpoints.OrgLicenceModifyLicenceKey.IsAllowed; }
		}

		public bool HasModifyLicenceInstallationDetailsSecurity
		{
			get { return EDISecurityCheckpoints.OrgLicenceModifyInstallationDetails.IsAllowed; }
		}

		public bool HasModifyLicenceConnectionDetailsSecurity
		{
			get { return EDISecurityCheckpoints.OrgLicenceModifyConnectionDetails.IsAllowed; }
		}

		public bool HasModifyLicenceSendUpgradeSecurity
		{
			get { return EDISecurityCheckpoints.OrgLicenceModifySendUpgrade.IsAllowed; }
		}

		public bool HasModifyLicenceSaveUpgradeToDiskSecurity
		{
			get { return EDISecurityCheckpoints.OrgLicenceModifySaveUpgradeToDisk.IsAllowed; }
		}

		public bool HasModifyLicenceSendUpgradeToHigherRingDBSecurity
		{
			get { return EDISecurityCheckpoints.OrgLicenceModifySendUpgradeToHigherRingDB.IsAllowed; }
		}

		public bool HasModifyLicenceDatabaseConfigurationSecurity
		{
			get { return EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails.IsAllowed; }
		}

		public bool HasModifyLicence3rdPartySoftwareSecurity
		{
			get { return EDISecurityCheckpoints.OrgLicenceModify3rdPartySoftware.IsAllowed; }
		}
	}
}

