using System;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public partial class EDIOrganisationSecurityContainerControl : OrganisationSecurityContainerControl	// Needs to be not abstract due to designer issues
	{
		public EDIOrganisationSecurityContainerControl()
		{
		}

		protected override Type TypeOfSecurityContainerControl
		{
			get { return typeof(EDIOrganisationSecurityContainerControl); }
		}

		#region IsModifyLicence

		[System.ComponentModel.Description("If this control is dependent on Licencing Security, then set this to true.")]
		public bool IsModifyLicence
		{
			get { return fIsModifyLicence; }
			set { fIsModifyLicence = value; }
		}
		bool fIsModifyLicence;

		#endregion

		#region IsModifyLicenceLicenceKey

		[System.ComponentModel.Description("If this control is dependent on changing Licence Keys, then set this to true.")]
		public bool IsModifyLicenceLicenceKey
		{
			get { return fIsModifyLicenceLicenceKey; }
			set { fIsModifyLicenceLicenceKey = value; }
		}
		bool fIsModifyLicenceLicenceKey;

		#endregion

		#region IsModifyLicenceInstallationDetails

		[System.ComponentModel.Description("If this control is dependent on Installation Details security, then set this to true.")]
		public bool IsModifyLicenceInstallationDetails
		{
			get { return fIsModifyLicenceInstallationDetails; }
			set { fIsModifyLicenceInstallationDetails = value; }
		}
		bool fIsModifyLicenceInstallationDetails;

		#endregion

		#region IsModifyLicenceConnectionDetails

		[System.ComponentModel.Description("If this control is dependent on Connection Details security, then set this to true.")]
		public bool IsModifyLicenceConnectionDetails
		{
			get { return fIsModifyLicenceConnectionDetails; }
			set { fIsModifyLicenceConnectionDetails = value; }
		}
		bool fIsModifyLicenceConnectionDetails;

		#endregion

		#region IsModifyDatabaseConfiguration

		[System.ComponentModel.Description("If this control is dependent on configuring Databases, then set this to true.")]
		public bool IsModifyLicenceDatabaseConfiguration
		{
			get { return fIsModifyLicenceDatabaseConfiguration; }
			set { fIsModifyLicenceDatabaseConfiguration = value; }
		}
		bool fIsModifyLicenceDatabaseConfiguration;

		#endregion

		#region IsModifyLicence3rdPartySoftware

		[System.ComponentModel.Description("If this control is dependent on configuring 3rd Party Software, then set this to true.")]
		public bool IsModifyLicence3rdPartySoftware
		{
			get { return fIsModifyLicence3rdPartySoftware; }
			set { fIsModifyLicence3rdPartySoftware = value; }
		}
		bool fIsModifyLicence3rdPartySoftware;

		#endregion

		#region IsModifyLicenceSendUpgrade

		[System.ComponentModel.Description("If this control is dependent on Send Upgrade Package security, then set this to true.")]
		public bool IsModifyLicenceSendUpgrade
		{
			get { return fIsModifyLicenceSendUpgrade; }
			set { fIsModifyLicenceSendUpgrade = value; }
		}
		bool fIsModifyLicenceSendUpgrade;

		#endregion

		#region IsModifyLicenceSendUpgradeToHigherRingDB

		[System.ComponentModel.Description("If this control is dependent on Send Upgrade Package To Higher DB security, then set this to true.")]
		public bool IsModifyLicenceSendUpgradeToHigherRingDB
		{
			get { return fIsModifyLicenceSendUpgradeToHigherRingDB; }
			set { fIsModifyLicenceSendUpgradeToHigherRingDB = value; }
		}
		bool fIsModifyLicenceSendUpgradeToHigherRingDB;

		#endregion
	}
}
