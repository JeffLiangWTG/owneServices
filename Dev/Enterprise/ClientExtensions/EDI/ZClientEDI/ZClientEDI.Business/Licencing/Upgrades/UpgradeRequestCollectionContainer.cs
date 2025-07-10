using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.AutoDeploy;
using Enterprise.Client.EDI.AutoDeploy.Business;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public enum UpgradeStatusType { ViaHttp, Blocked, WithoutNotificationAddress }

	#region Public Status Interfaces

	public interface IUpgradeStatus
	{
		bool IsError { get; }
		string Message { get; }

		EDIOrgHeader[] GetOrganisationsWithoutNotification();
		int OrganisationWithoutNotificationCount { get; }
		bool NotificationEmailsSent { get; }
	}

	public interface IPreUpgradeStatus : IUpgradeStatus
	{
		EDIOrgHeader[] GetOrganisations();
		int OrganisationCount { get; }

		UpgradeRequest[] GetRequestsByStatusType(UpgradeStatusType statusType);
		int GetRequestCountByStatusType(UpgradeStatusType statusType);

		string GetStatusTypeDescription(UpgradeStatusType statusType);

		int ActiveUpgradesTotalCount { get; }
	}

	public interface IPostUpgradeStatus : IUpgradeStatus
	{
		UpgradesToClient[] GetCreatedUpgradesToClients();
		int CreatedUpgradesToClientsCount { get; }

		CustomerServiceEmail[] GetNotificationEmails();
		int NotificationEmailsCount { get; }

		EmailToContactBusinessObject[] GetNotificationEmailsWithoutRecipient();
		int NotificationEmailsWithoutRecipientCount { get; }
		string PackagePath { get; }
	}

	#endregion

	/// <summary>
	/// Summary description for UpgradeArrayContainer.
	/// </summary>
	public class UpgradeRequestCollectionContainer : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region UpgradeStatus struct

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
		protected struct UpgradeStatus : IPreUpgradeStatus, IPostUpgradeStatus
		{
			public UpgradeStatus(bool isError, String message)
			{
				this.isError = isError;
				this.message = message;
				notificationEmailsSent = false;

				organisations = new List<EDIOrgHeader>();
				organisationsWithoutNotification = new List<EDIOrgHeader>();
				upgradesToSendViaHttp = new List<UpgradeRequest>();
				blockedUpgrades = new List<UpgradeRequest>();
				upgradesWithoutNotificationAddress = new List<UpgradeRequest>();

				fCreatedUpgradesToClients = new List<UpgradesToClient>();
				notificationEmails = new List<CustomerServiceEmail>();
				notificationEmailsWithoutRecipient = new List<EmailToContactBusinessObject>();
			}

			#region Common interfaces part

			public bool IsError
			{
				get { return isError; }
				set { isError = value; }
			}

			public string Message
			{
				get { return message; }
				set { message = value; }
			}

			public bool NotificationEmailsSent
			{
				get { return notificationEmailsSent; }
				set { notificationEmailsSent = value; }
			}

			public EDIOrgHeader[] GetOrganisationsWithoutNotification()
			{
				return organisationsWithoutNotification.ToArray();
			}

			public int OrganisationWithoutNotificationCount
			{
				get { return organisationsWithoutNotification.Count; }
			}

			bool isError;
			string message;
			bool notificationEmailsSent;
			public List<EDIOrgHeader> organisationsWithoutNotification;

			#endregion

			#region IPreUpgradeStatus Implementation

			public EDIOrgHeader[] GetOrganisations()
			{
				return organisations.ToArray();
			}

			public int OrganisationCount
			{
				get { return organisations.Count; }
			}

			public UpgradeRequest[] GetRequestsByStatusType(UpgradeStatusType statusType)
			{
				return GetArrayListForStatus(statusType).ToArray();
			}

			public int GetRequestCountByStatusType(UpgradeStatusType statusType)
			{
				return GetArrayListForStatus(statusType).Count;
			}

			public string GetStatusTypeDescription(UpgradeStatusType statusType)
			{
				switch (statusType)
				{
					case UpgradeStatusType.ViaHttp: return "to download via Http";
					case UpgradeStatusType.Blocked: return "blocked for sending upgrades";
					case UpgradeStatusType.WithoutNotificationAddress: return "without client contact to notify about sent upgrade";
					default: return "unknown status";
				}
			}

			public int ActiveUpgradesTotalCount
			{
				get { return upgradesToSendViaHttp.Count; }
			}

			List<UpgradeRequest> GetArrayListForStatus(UpgradeStatusType statusType)
			{
				switch (statusType)
				{
					case UpgradeStatusType.ViaHttp: return upgradesToSendViaHttp;
					case UpgradeStatusType.Blocked: return blockedUpgrades;
					case UpgradeStatusType.WithoutNotificationAddress: return upgradesWithoutNotificationAddress;
					default: return new List<UpgradeRequest>();
				}
			}

			public readonly List<EDIOrgHeader> organisations;
			public readonly List<UpgradeRequest> upgradesToSendViaHttp;
			public readonly List<UpgradeRequest> blockedUpgrades;
			public readonly List<UpgradeRequest> upgradesWithoutNotificationAddress;

			#endregion

			#region IPostUpgradeStatus implementation

			public UpgradesToClient[] GetCreatedUpgradesToClients()
			{
				return fCreatedUpgradesToClients.ToArray();
			}

			public int CreatedUpgradesToClientsCount
			{
				get { return fCreatedUpgradesToClients.Count; }
			}

			public CustomerServiceEmail[] GetNotificationEmails()
			{
				return notificationEmails.ToArray();
			}

			public int NotificationEmailsCount
			{
				get { return notificationEmails.Count; }
			}

			public EmailToContactBusinessObject[] GetNotificationEmailsWithoutRecipient()
			{
				return notificationEmailsWithoutRecipient.ToArray();
			}

			public int NotificationEmailsWithoutRecipientCount
			{
				get { return notificationEmailsWithoutRecipient.Count; }
			}

			public string PackagePath { get; set; }

			public readonly List<UpgradesToClient> fCreatedUpgradesToClients;
			public readonly List<CustomerServiceEmail> notificationEmails;
			public readonly List<EmailToContactBusinessObject> notificationEmailsWithoutRecipient;

			#endregion
		}

		#endregion

		public UpgradeRequestCollectionContainer(BusinessObjectFactory factory, params EDIOrgHeader[] organisationsToUpgrade)
			: this(factory, new UpgradeRequestCollection(factory, organisationsToUpgrade), organisationsToUpgrade.Length < 2)
		{
		}

		public UpgradeRequestCollectionContainer(BusinessObjectFactory factory, UpgradeRequestCollection upgradeRequests)
			: this(factory, upgradeRequests, false)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		UpgradeRequestCollectionContainer(BusinessObjectFactory factory, UpgradeRequestCollection upgradeRequests, bool canSaveToDisk)
			: base(factory)
		{
			upgrades = upgradeRequests;
			RegisterEditableChildObject(upgrades);
			this.canSaveToDisk = canSaveToDisk;
		}

		#region Upgrades

		public UpgradeRequestCollection Upgrades
		{
			get { return upgrades; }
		}

		readonly UpgradeRequestCollection upgrades;

		#endregion

		public ReleaseBuild SelectedReleaseBuild
		{
			get { return Factory.Load<ReleaseBuild>(ReleaseBuildPK); }
		}

		#region Bound Properties

		#region Is Send Via Default

		public ZBool IsSendViaDefault
		{
			get { return fIsSendViaDefault; }
			set
			{
				if (value)
				{
					ClearUpgradeMethodBooleanProperties();
					SetDesiredUpgradeMethod("");
				}

				SetNonPersistentPropertyValue(IsSendViaDefaultInfo, ref fIsSendViaDefault, value);
				DescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsSendViaDefaultInfo
		{
			get { return GetZPropertyInfo(nameof(IsSendViaDefault)); }
		}

		ZBool fIsSendViaDefault;

		#endregion

		#region Is Send Via Http

		public ZBool IsSendViaHttp
		{
			get { return fIsSendViaHttp; }
			set
			{
				if (value)
				{
					ClearUpgradeMethodBooleanProperties();
					SetDesiredUpgradeMethod(UpgradeMethods.Codes.Http);
				}

				SetNonPersistentPropertyValue(IsSendViaHttpInfo, ref fIsSendViaHttp, value);
				DescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsSendViaHttpInfo
		{
			get { return GetZPropertyInfo(nameof(IsSendViaHttp)); }
		}

		ZBool fIsSendViaHttp;

		#endregion

		#region Is Save to Disk

		public ZBool IsSaveToDisk
		{
			get { return fIsSaveToDisk; }
			set
			{
				if (value)
				{
					ClearUpgradeMethodBooleanProperties();
				}

				SetNonPersistentPropertyValue(IsSaveToDiskInfo, ref fIsSaveToDisk, value);
				if (Upgrades != null)
				{
					Upgrades.SetReadOnlyIncludingChildren(value);
				}

				DescriptionInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					ValidateIsSaveToDisk();
					ValidateReleaseBuildPK();
				}
			}
		}

		readonly bool canSaveToDisk;

		public bool IsSaveToDisk_ReadOnly
		{
			get { return !canSaveToDisk; }
		}

		public ZPropertyInfo IsSaveToDiskInfo
		{
			get { return GetZPropertyInfo(nameof(IsSaveToDisk)); }
		}

		public void ValidateIsSaveToDisk()
		{
			IsSaveToDiskInfo.ClearAllNotifications();

			if (fIsSaveToDisk)
			{
				if (!EDIDataRegistry.Instance.AllowSaveUpgradePackageToDisk.Value)
				{
					IsSaveToDiskInfo.AddError(SaveToDiskDisabledMessage + "\r\n\r\n" + SaveToDiskWarningMessage);
				}
				else
				{
					IsSaveToDiskInfo.AddWarning(SaveToDiskWarningMessage);
				}

				SecurityCheckpoint checkpoint = EDISecurityCheckpoints.OrgLicenceModifySaveUpgradeToDisk;
				if (!checkpoint.IsAllowed)
				{
					IsSaveToDiskInfo.AddError(string.Format(CultureInfo.CurrentCulture, "If you need to save upgrade packages to disk, please ask your administrator to change either your Staff or Group Security Rights to allow access to:{0}{0}{1}", System.Environment.NewLine, checkpoint.DisplayTextPathToSecurityRight));
				}
			}
		}

		ZBool fIsSaveToDisk;

		const string SaveToDiskDisabledMessage = @"Save To Disk is disabled. Please use Email or HTTP options only.";

		const string SaveToDiskWarningMessage =
@"Saving upgrade packages to disk allows CargoWise staff or clients to bypass licence delivery controls that results in the wrong upgrade package being delivered and upgraded on a client system. e.g.:
1)	Upgrading to DPR when client only wants to use GPR
2)	Upgrading using the wrong client specific package

In these cases, usually CR1 incidents are raised and would cause significant damage to the client and to our reputation.

Saving to disk function is still accessible to CargoWise staff, but only as a last resort and under limited circumstances. It must be approved and enabled by Richard White, or see Henry Ye as a fall back.";

		#endregion

		#region Description

		public ZString Description
		{
			get
			{
				if (IsSendViaDefault)
				{
					return DefaultDescription;
				}
				if (IsSendViaHttp)
				{
					return HttpDescription;
				}
				if (IsSaveToDisk)
				{
					return SaveToDiskDescription;
				}

				return ZString.Empty;
			}
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(Description)); }
		}

		#endregion

		#region Send Email Notification Automatically

		public ZBool SendEmailNotificationAutomatically
		{
			get { return fSendEmailNotificationAutomatically; }
			set
			{
				SetNonPersistentPropertyValue(SendEmailNotificationAutomaticallyInfo, ref fSendEmailNotificationAutomatically, value);
			}
		}

		public bool SendEmailNotificationAutomatically_ReadOnly
		{
			get { return IsSaveToDisk; }
		}

		public ZPropertyInfo SendEmailNotificationAutomaticallyInfo
		{
			get { return GetZPropertyInfo(nameof(SendEmailNotificationAutomatically)); }
		}

		ZBool fSendEmailNotificationAutomatically;

		#endregion

		#region Release Build PK

		public ZGuid ReleaseBuildPK
		{
			get { return fReleaseBuildPK; }
			set
			{
				if (fReleaseBuildPK != value)
				{
					SetNonPersistentPropertyValue(ReleaseBuildPKInfo, ref fReleaseBuildPK, value);
					if (!IsValidationSuspended)
					{
						ValidateReleaseBuildPK();
					}
					foreach (UpgradeRequest upgrade in Upgrades)
					{
						upgrade.SetSelectedReleaseBuild(SelectedReleaseBuild);
					}
				}
			}
		}

		public void ValidateReleaseBuildPK()
		{
			ReleaseBuildPKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ReleaseBuildPKInfo);
			ListValidation.ErrorIfInvalidPK(ReleaseBuildPKInfo, ReleaseBuilds);
			ErrorIfReleaseBuildIsNotActive();

			if (!ReleaseBuildPKInfo.HasErrors() && !IsSaveToDisk && SelectedReleaseBuild.HL_Superceded)
			{
				if (EDISecurityCheckpoints.OrgLicenceModifySendSupersededUpgrade.IsAllowed)
				{
					ReleaseBuildPKInfo.AddWarning(
@"Superseded builds should not be deployed to clients. Please select this only if:
1. The client has tested a specific version and wants to deploy that version to their production database
2. The client needs an intermediate upgrade to move from a very old version to a newer version");
				}
				else
				{
					ReleaseBuildPKInfo.AddError(string.Format(CultureInfo.CurrentCulture,
@"Superseded builds cannot be deployed to clients. Please select this only if you wish to save the build to disk.
If you do need to send superseded builds, please ask your administrator to change either your Staff or Group Security Rights to allow access to:

{0}", EDISecurityCheckpoints.OrgLicenceModifySendSupersededUpgrade.DisplayTextPathToSecurityRight));
				}
			}
		}

		void ErrorIfReleaseBuildIsNotActive()
		{
			if (!ReleaseBuildPKInfo.HasErrors() && !SelectedReleaseBuild.HL_IsActive)
			{
				ReleaseBuildPKInfo.AddError("This build has not been retained and cannot be deployed to clients.\r\nDeploy the latest patch on the same branch instead.");
			}
		}

		public ZPropertyInfo ReleaseBuildPKInfo
		{
			get { return GetZPropertyInfo(nameof(ReleaseBuildPK)); }
		}

		ZGuid fReleaseBuildPK;

		#endregion

		#region Release Builds

		public ReleaseBuildCollection ReleaseBuilds
		{
			get
			{
				if (fReleaseBuilds == null)
				{
					fReleaseBuilds = new ReleaseBuildCollection(Factory);
				}

				return fReleaseBuilds;
			}
		}

		ReleaseBuildCollection fReleaseBuilds;

		#endregion

		#region ScheduledDateTime

		public ZDateTime ScheduledDateTime
		{
			get { return fScheduledDateTime; }
			set
			{
				SetNonPersistentPropertyValue(ScheduledDateTimeInfo, ref fScheduledDateTime, value);
				if (!IsValidationSuspended)
				{
					ValidateScheduledDateTime();
				}
				SetScheduledDateTime();
			}
		}

		public virtual void ValidateScheduledDateTime()
		{
			ScheduledDateTimeInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(ScheduledDateTimeInfo);
		}

		public bool ScheduledDateTime_ReadOnly
		{
			get { return IsSaveToDisk; }
		}

		public ZPropertyInfo ScheduledDateTimeInfo
		{
			get { return GetZPropertyInfo(nameof(ScheduledDateTime)); }
		}

		ZDateTime fScheduledDateTime;

		#endregion			

		public string NotificationSubjectPrefix { get; set; }

		#region AdditionalNotification

		[MaxLength(2048)]
		public ZString AdditionalNotification
		{
			get { return fAdditionalNotification; }
			set
			{
				if (fAdditionalNotification != value)
				{
					CheckMaximumLength(AdditionalNotificationInfo, value);
					SetNonPersistentPropertyValue(AdditionalNotificationInfo, ref fAdditionalNotification, value);
				}
			}
		}

		public bool AdditionalNotification_ReadOnly
		{
			get { return IsSaveToDisk; }
		}

		public ZPropertyInfo AdditionalNotificationInfo
		{
			get { return GetZPropertyInfo(nameof(AdditionalNotification)); }
		}

		ZString fAdditionalNotification;

		#endregion

		#region Incidents Need To Update EDoc

		public List<SupportIncident> Incidents
		{
			get { return incidents ?? (incidents = new List<SupportIncident>()); }
			set { incidents = value; }
		}
		List<SupportIncident> incidents;

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			IsSendViaDefault = true;
			SendEmailNotificationAutomatically = EDIDataRegistry.Instance.SendUpgradeEmailNotificationAutomatically.Value;
			ScheduledDateTime = ZDateTime.Now;
			AdditionalNotification = EDIDataRegistry.Instance.UpgradeEmailDefaultAdditionalNotification;
		}

		#endregion

		public
#if DEBUG
 virtual
#endif
 IPreUpgradeStatus GetPreUpgradeStatus()
		{
			UpgradeStatus result;

			ValidateReleaseBuildPK();
			if (IsSaveToDisk)
			{
				if (ReleaseBuildPKInfo.HasErrors())
				{
					result = new UpgradeStatus(true, ReleaseBuildPKInfo.GetErrors().GetFirstMessage());
				}
				else if (IsSaveToDiskInfo.HasErrors())
				{
					result = new UpgradeStatus(true, IsSaveToDiskInfo.GetErrors().GetFirstMessage());
				}
				else
				{
					result = new UpgradeStatus(false, "");
				}
			}
			else
			{
				RunPreSaveValidation();

				if (HasErrors)
				{
					result = new UpgradeStatus(true, NotificationsIncludingChildren.GetErrors().ToUniqueMessageListString());
				}
				else
				{
					result = new UpgradeStatus(false, "");

					EDIOrgHeader currentOrganisation = null;
					int upgradesForOrganisationCount = 0;
					bool organisationHasEmailForNotification = false;

					foreach (UpgradeRequest upgrade in Upgrades)
					{
						if (currentOrganisation != null &&
							currentOrganisation.PK != upgrade.OrganisationPk)
						{
							if (upgradesForOrganisationCount > 0)
							{
								result.organisations.Add(currentOrganisation);
								if (!organisationHasEmailForNotification)
								{
									result.organisationsWithoutNotification.Add(currentOrganisation);
								}
							}

							upgradesForOrganisationCount = 0;
							organisationHasEmailForNotification = false;
						}

						currentOrganisation = upgrade.Factory.Load<EDIOrgHeader>(upgrade.OrganisationPk);

						bool blocked = false;
						switch (upgrade.SupportedUpgradeMethod)
						{
							case UpgradeMethods.Codes.Http:
								{
									result.upgradesToSendViaHttp.Add(upgrade);
									break;
								}
							default:
								{
									result.blockedUpgrades.Add(upgrade);
									blocked = true;
									break;
								}
						}

						if (!blocked)
						{
							upgradesForOrganisationCount++;

							if (upgrade.ClientContactEmailAddress.IsEmpty)
							{
								if (SendEmailNotificationAutomatically)
								{
									result.upgradesWithoutNotificationAddress.Add(upgrade);
								}
							}
							else
							{
								organisationHasEmailForNotification = true;
							}
						}
					}

					if (currentOrganisation != null &&
						upgradesForOrganisationCount > 0)
					{
						result.organisations.Add(currentOrganisation);
						if (!organisationHasEmailForNotification)
						{
							result.organisationsWithoutNotification.Add(currentOrganisation);
						}
					}
				}
			}

			return result;
		}

#if DEBUG
		virtual
#endif
 public IPostUpgradeStatus PlaceUpgradesToClients()
		{
			UpgradeStatus result;

			if (ReleaseBuildPK.IsEmpty)
			{
				result = new UpgradeStatus(true, NoBuildSelectedMessage);
			}
			else
			{
				result = new UpgradeStatus(false, "");
				Dictionary<EDIOrgHeader, List<UpgradeRequest>> allUpgrades = new Dictionary<EDIOrgHeader, List<UpgradeRequest>>();

				try
				{
					foreach (UpgradeRequest upgrade in Upgrades)
					{
						List<UpgradeRequest> upgradesForOrganisation;
						EDIOrgHeader organisation = upgrade.Factory.Load<EDIOrgHeader>(upgrade.OrganisationPk);
						if (organisation != null)
						{
							if (!allUpgrades.TryGetValue(organisation, out upgradesForOrganisation))
							{
								allUpgrades[organisation] = upgradesForOrganisation = new List<UpgradeRequest>();
							}
							upgradesForOrganisation.Add(upgrade);
						}
					}

					foreach (EDIOrgHeader organisation in allUpgrades.Keys)
					{
						List<UpgradeRequest> upgradesForOrganisation = allUpgrades[organisation];
						StringBuilder upgradeMethodForServers = new StringBuilder();
						foreach (UpgradeRequest upgrade in upgradesForOrganisation)
						{
							UpgradesToClient request = CreateUpgradesToClient(upgrade);
							if (request != null)
							{
								result.fCreatedUpgradesToClients.Add(request);
								upgradeMethodForServers.Append(string.Format(CultureInfo.CurrentCulture, ", {0} via {1}", request.LicDatabase != null ? request.LicDatabase.LD_ServerCode : new ZString("Unknown"), request.L1_ActualUpgradeMethod));
							}
						}

						SelectedReleaseBuild.Logs.AddNew(Events.Delivered, organisation.OH_Code);
						string logMessage = string.Format(CultureInfo.CurrentCulture, "{0} (v{1}) will be sent to this client{2}", SelectedReleaseBuild.ReleaseDisplayText, SelectedReleaseBuild.ExeVersion, upgradeMethodForServers.ToString());
						organisation.Logs.AddNew(Events.UpgradeSucceeded, logMessage);

						CustomerServiceEmail notificationEmail = CreateNotificationEmailForOrganisation(organisation, upgradesForOrganisation.ToArray());
						AddIncidentContactToEmailRecipient(notificationEmail);
						if (notificationEmail.ToEmailAddress != "")
						{
							result.notificationEmails.Add(notificationEmail);
						}
						else
						{
							result.notificationEmailsWithoutRecipient.Add(notificationEmail);
							result.organisationsWithoutNotification.Add(organisation);
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					result = new UpgradeStatus(true, "An error occurred while creating upgrades:\r\n" + ex.Message);
					ErrorReporter.ReportOnce("An error occurred while creating upgrades", ex.Message, ex);
				}

				try
				{
					Factory.Save();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					result = new UpgradeStatus(true, "An error occurred while saving the upgrade to disk:\r\n" + ex.Message);
					ErrorReporter.ReportOnce("An error occurred while saving the upgrade to disk", ex.Message, ex);
					return result;
				}

				if (SendEmailNotificationAutomatically && result.notificationEmails.Count > 0)
				{
					try
					{
						SendNotificationEmails(result.notificationEmails.ToArray());
						result.NotificationEmailsSent = true;
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						result.IsError = true;
						result.Message = "An error occurred while sending the upgrade notification emails:\r\n" + ex.Message;
						ErrorReporter.ReportOnce("An error occurred while sending the upgrade notification emails", ex.Message, ex);
					}
				}
			}

			return result;
		}

		public
#if DEBUG
 virtual
#endif
 IPostUpgradeStatus SaveToDisk(string saveToDiskDirectory)
		{
			UpgradeStatus result;
			if (ReleaseBuildPK.IsEmpty)
			{
				result = new UpgradeStatus(true, NoBuildSelectedMessage);
			}
			else if (Upgrades.Count == 0 || Upgrades[0].EnterpriseCode.IsEmpty)
			{
				result = new UpgradeStatus(true, "The selected client does not have a LicenceEnterpriseCode.");
			}
			else
			{
				try
				{
					string packagePath = BuildPackage(saveToDiskDirectory, Upgrades[0].EnterpriseCode);
					result = new UpgradeStatus(false, GetSaveToDiskSuccessMessage(packagePath))
					{
						PackagePath = packagePath
					};
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					result = new UpgradeStatus(true, GetSaveToDiskExceptionMessage(ex));
					ErrorReporter.ReportOnce("An error occurred while saving the upgrade to disk", ex.Message, ex);
				}
			}

			return result;
		}

		public CustomerServiceEmail CreateNotificationEmailForOrganisation(EDIOrgHeader organisation, UpgradeRequest[] upgradesForOrganisation)
		{
			var result = new CustomerServiceEmail(organisation);
			result.MarkAsNoNeedSaveToEDocs();
			result.Client = organisation;
			result.Subject = GetSubject(organisation, upgradesForOrganisation);
			result.Body = GetBody(organisation, upgradesForOrganisation);
			result.ToEmailAddress = GetToRecipients(upgradesForOrganisation);
			if (result.ToEmailAddress.IsEmpty)
			{
				result.ToEmailAddress = GetCcRecipients(result.ToEmailAddress, upgradesForOrganisation);
			}
			else
			{
				result.Cc = GetCcRecipients(result.ToEmailAddress, upgradesForOrganisation);
			}

			return result;
		}

		protected
#if DEBUG
 virtual
#endif
 UpgradesToClient CreateUpgradesToClient(UpgradeRequest request)
		{
			return request.CreateUpgradesToClient(SelectedReleaseBuild);
		}

		protected
#if DEBUG
 virtual
#endif
 void SendNotificationEmails(EmailToContactBusinessObject[] emails)
		{
			foreach (EmailToContactBusinessObject email in emails)
			{
				email.SendEmail();
				AddToIncidentEDoc(email);
			}
		}

		void AddToIncidentEDoc(EmailToContactBusinessObject email)
		{
			foreach (var incident in Incidents)
			{
				if (email.Subject.Contains(incident.Client.OH_Code, StringComparison.Ordinal))
				{
					string emailAsString = "To: " + String.Join(";", email.Recipients) + System.Environment.NewLine + System.Environment.NewLine;
					emailAsString += "Subject: " + email.Subject + System.Environment.NewLine + System.Environment.NewLine;
					emailAsString += "Body: " + System.Environment.NewLine + System.Environment.NewLine + email.Body;
					incident.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes(emailAsString), "Upgrade Delivery Notification Email.txt", "COR");
					incident.DocManagerInfo.Save();
				}
			}
		}

		void AddIncidentContactToEmailRecipient(EmailToContactBusinessObject email)
		{
			foreach (var incident in Incidents)
			{
				if (incident.Contact != null && !incident.Contact.OC_Email.IsEmpty && !email.ToEmailAddress.Contains(incident.Contact.OC_Email, StringComparison.Ordinal))
				{
					if (!email.ToEmailAddress.IsEmpty)
					{
						email.ToEmailAddress += ";";
					}
					email.ToEmailAddress += incident.Contact.OC_Email;
				}
			}
		}

		#region Email Subject, Body and Recipients

		public
#if DEBUG
 virtual
#endif
 EDIOrgHeader CurrentOrganisation { get; private set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		[BusinessObjectTestExclude] // suppress TestArrayPropertiesDoNotReturnNull
		public
#if DEBUG
 virtual
#endif
 UpgradeRequest[] UpgradesForCurrentOrganisation { get; private set; }

		UpgradeRequestCollectionContainerParser EmailTemplateParser
		{
			get { return emailTemplateParser ?? (emailTemplateParser = new UpgradeRequestCollectionContainerParser(Factory)); }
		}
		UpgradeRequestCollectionContainerParser emailTemplateParser;

		UpgradeRequestCollectionContainerHtmlParser EmailTemplateHtmlParser
		{
			get { return emailTemplateHtmlParser ?? (emailTemplateHtmlParser = new UpgradeRequestCollectionContainerHtmlParser(Factory)); }
		}
		UpgradeRequestCollectionContainerHtmlParser emailTemplateHtmlParser;

		public string GetSubject(EDIOrgHeader organisation, UpgradeRequest[] upgradesForOrganisation)
		{
			CurrentOrganisation = organisation;
			UpgradesForCurrentOrganisation = upgradesForOrganisation;
			return EmailTemplateParser.Parse(this, EDIDataRegistry.Instance.UpgradePackageDeliveryNotificationEmailTemplateRaw.Value.EmailSubject);
		}

		string GetBody(EDIOrgHeader organisation, UpgradeRequest[] upgradesForOrganisation)
		{
			CurrentOrganisation = organisation;
			UpgradesForCurrentOrganisation = upgradesForOrganisation;
			return EmailTemplateHtmlParser.Parse(this, EDIDataRegistry.Instance.UpgradePackageDeliveryNotificationEmailTemplateRaw.Value.EmailBody);
		}

		string GetToRecipients(UpgradeRequest[] upgradeList)
		{
			List<string> recipientList = new List<string>();
			foreach (UpgradeRequest upgrade in upgradeList)
			{
				OrgContact contact = upgrade.GetClientContact();
				if (contact != null && !contact.OC_Email.IsEmpty && !recipientList.Contains(contact.OC_Email))
				{
					recipientList.Add(contact.OC_Email);
				}
			}

			return string.Join(";", recipientList.ToArray());
		}

		string GetCcRecipients(string toEmailAddress, UpgradeRequest[] upgradeList)
		{
			string result = "";
			List<string> previousRecipients = new List<string>();

			string[] toEmailAddressAsArray = toEmailAddress.Split(';');
			foreach (UpgradeRequest upgrade in upgradeList)
			{
				if (!upgrade.ClientContactEmailAddress.IsEmpty &&
					!previousRecipients.Contains(upgrade.ClientContactEmailAddress) &&
					!((IList<string>)toEmailAddressAsArray).Contains(upgrade.ClientContactEmailAddress))
				{
					result += upgrade.ClientContactEmailAddress + ";";
					previousRecipients.Add(upgrade.ClientContactEmailAddress);
				}
			}

			result = result.TrimEnd(';');

			return result;
		}

		#endregion

		protected
#if DEBUG
 virtual
#endif
 string BuildPackage(string targetDirectory, string licenceEnterpriseCode)
		{
			RuntimePackageBuilder builder = new RuntimePackageBuilder(SelectedReleaseBuild, targetDirectory);
			builder.Build(licenceEnterpriseCode, false); // We could make this smarter about Winzor but this save to disk feature from the Organisation form is deprecated anyway.
			return builder.LastPackagePath;
		}

		protected void SetDesiredUpgradeMethod(ZString desiredUpgradeMethod)
		{
			if (Upgrades != null)
			{
				foreach (UpgradeRequest upgrade in Upgrades)
				{
					upgrade.SetBestSupportedUpgradeMethod(desiredUpgradeMethod);
				}
			}
		}

		protected void SetScheduledDateTime()
		{
			if (Upgrades != null)
			{
				foreach (UpgradeRequest upgrade in Upgrades)
				{
					upgrade.SetDefaultScheduledTime(ScheduledDateTime);
				}
			}
		}

		void ClearUpgradeMethodBooleanProperties()
		{
			IsSendViaDefault = false;
			IsSendViaHttp = false;
			IsSaveToDisk = false;
		}

		public static string GetSaveToDiskSuccessMessage(string path) => $"Package was successfully built and saved as {path}\r\nWARNING: This must not be deployed to WiseCloud as it may not contain all necessary Winzor binaries.";
		public static string GetSaveToDiskExceptionMessage(Exception e) => $"An error occurred while saving the upgrade to disk:\n\n{e.Message}";
		public const string NoBuildSelectedMessage = "Please select a ReleaseBuild to send.";
		public const string DefaultDescription = "This is the recommended method. The upgrade package will be sent to each of the selected client's LicenceDatabases using the upgrade method stored on the licence tab of this organisation.";
		public const string HttpDescription = "The upgrade package will be stored on the web server and the client's batch processor will download it automatically if their LicenceDatabases support this method.";
		public const string SaveToDiskDescription = "The upgrade package will be saved to the ediDeploy folder on your local disk but will not be sent to the client.";
	}
}
