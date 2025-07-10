using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.ActiveDirectory;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security.ActiveDirectory.Synchronisation
{
	public class EntitySynchroniser : IEntitySynchroniser
	{
		public EntitySynchroniser(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "factory");
			this.Factory = factory;
		}

		protected BusinessObjectFactory Factory { get; }
		readonly List<IADEntity> synchronisedEntities = new List<IADEntity>();
		readonly HashSet<IADEntity> entitiesWithErrors = new HashSet<IADEntity>();

		#region Synchronise

		public void Synchronise(EntitiesToSync? entitiesToSync = null, SyncMode? preferredSyncMode = null)
		{
			SyncMode? preferredSyncModeForStaff = null;
			if (ActiveDirectoryRegistry.Instance.SyncDirection == SyncDirection.TwoWay)
			{
				preferredSyncModeForStaff = preferredSyncMode;
			}
			SyncMode? preferredSyncModeForGroups = null;
			if (ActiveDirectoryRegistry.Instance.SyncDirectionGroup == SyncDirection.TwoWay)
			{
				preferredSyncModeForGroups = preferredSyncMode;
			}

			if (entitiesToSync == null)
			{
				entitiesToSync = ActiveDirectoryRegistry.Instance.EntitiesToSync;
			}

			ResetDefaultPasswordFailsToMeetDomainPolicy();

			unitsComplete = 0;

			var shouldSyncGroups = entitiesToSync != EntitiesToSync.UsersOnly;
			try
			{
				UpdateProgress(Res.GetString("6a688afc-e2eb-473b-87ef-cf392c6afd20", "Gathering records to sync"));
				var staffToSync = GetStaffToSync(preferredSyncModeForStaff);

				var groupsToSync = shouldSyncGroups ? GetGroupsToSync(preferredSyncModeForGroups) : Array.Empty<GlbGroup>();
				if (ClientHookLoader.Instance.Client == Clients.EDI)
				{
					groupsToSync = GetGroupsMatchWithRegex(groupsToSync, ActiveDirectoryRegistry.Instance.GroupSyncRegex.Value);
				}

				totalUnits = groupsToSync.Count() + staffToSync.Count();
				SyncStaff(staffToSync, preferredSyncModeForStaff);

				if (shouldSyncGroups)
				{
					SyncGroups(groupsToSync, preferredSyncModeForGroups);
				}

				UpdateEntitiesWithErrorsToIncludeEntitiesWithADLinkConflict();
				RethrowPasswordDoesntMatchPolicyExceptionIfAny();
			}
			finally
			{
				OnSyncCompleted();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "These are regex")]
		public static IEnumerable<GlbGroup> GetGroupsMatchWithRegex(IEnumerable<GlbGroup> groups, string userPattern)
		{
			if (string.IsNullOrEmpty(userPattern))
			{
				return groups;
			}

			try
			{
				const string categoryPattern = @"\s*Category\s*=\s*";	// Category=
				var orPattern = $@"\s*\|\|{categoryPattern}";			// ||Category=
				var andPattern = $@"\s*&&{categoryPattern}";			// &&Category=
				var orRegex = new Regex(orPattern, RegexOptions.IgnoreCase);
				var andRegex = new Regex(andPattern, RegexOptions.IgnoreCase);

				userPattern = userPattern.Trim();

				// Check for multiple or simultaneous occurrences of "&&Category=" and "||Category=", or start with these.
				if (orRegex.Matches(userPattern).Count > 1 ||
					andRegex.Matches(userPattern).Count > 1 ||
					((orRegex.IsMatch(userPattern) && andRegex.IsMatch(userPattern))) ||
					IsMatchSafe(userPattern, $@"^{andPattern}") ||
					IsMatchSafe(userPattern, $@"^{orPattern}"))
				{
					// Return an empty result or handle it as needed
					return [];
				}

				if (orRegex.IsMatch(userPattern)) // ||Category=
				{
					// Match by GG_Desc OR GG_Category
					var parts = orRegex.Split(userPattern);
					var descRegex = parts[0];
					var categoryRegex = parts[1];

					return groups.Where(g => IsMatchSafe(g.GG_Desc, descRegex) ||
											 IsMatchSafe(g.GG_Category, categoryRegex));
				}
				else if (andRegex.IsMatch(userPattern)) // &&Category=
				{
					// Match by GG_Desc AND GG_Category
					var parts = andRegex.Split(userPattern);
					var descRegex = parts[0];
					var categoryRegex = parts[1];

					return groups.Where(g => IsMatchSafe(g.GG_Desc, descRegex) &&
											 IsMatchSafe(g.GG_Category, categoryRegex));
				}
				else if (IsMatchSafe(userPattern, $@"^{categoryPattern}")) // Category=
				{
					userPattern = Regex.Replace(userPattern, $@"^{categoryPattern}", "", RegexOptions.IgnoreCase);

					return groups.Where(g => IsMatchSafe(g.GG_Category, userPattern));
				}
				else // Match by GG_Desc only
				{
					 return groups.Where(g => IsMatchSafe(g.GG_Desc, userPattern));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// To be safe, if the regex parsing return exception, we not going to sync any group
				return [];
			}

			bool IsMatchSafe(string input, string pattern)
			{
				try
				{
					return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
				}
				catch (ArgumentException)
				{
					return false;
				}
			}
		}

		protected virtual void ResetDefaultPasswordFailsToMeetDomainPolicy()
		{
			foreach (DomainCredentials dc in ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value)
			{
				dc.DefaultPasswordFailsToMeetDomainPolicy = false;
			}
		}

		void UpdateEntitiesWithErrorsToIncludeEntitiesWithADLinkConflict()
		{
			var entitiesWithADLinkConflict = CheckForADLinkConflict();
			entitiesWithErrors.UnionWith(entitiesWithADLinkConflict);
		}

		void RethrowPasswordDoesntMatchPolicyExceptionIfAny()
		{
			var domainsWithPasswordError = string.Join(", ",
				  ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value.Cast<DomainCredentials>()
					  .Where(d => d.DefaultPasswordFailsToMeetDomainPolicy)
					  .Select(d => d.DomainName));

			if (!string.IsNullOrEmpty(domainsWithPasswordError))
			{
				throw new PasswordDoesNotMatchPolicyException(Res.GetString("26105ed7-aa2e-4e5f-93a8-ca5f40b4c976", @"The registry setting '{0}' is using a password that does not meet the current password policy requirements of domain(s): {1}. Please correct this registry setting.
Synchronization with Active Directory will not work properly until this is fixed.",
						((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.DomainCredentialsCollection).LocationMultilingual,
						domainsWithPasswordError
					));
			}
		}

		HashSet<IADEntity> CheckForADLinkConflict()
		{
			var result = new HashSet<IADEntity>();
			var messageBuilder = new ZStringBuilder();
			var duplications = new Dictionary<Guid, HashSet<string>>();
			CheckForADLinkConflictInSyncedEntities();
			CheckForADLinkConflictWithExistingBizos();

			if (duplications.Count > 0)
			{
				duplications.Keys.ForEach(k =>
				{
					messageBuilder.Append(GetErrorMessage(k, duplications[k]));
				});

				var message = Res.GetString("D4BFF40E-BF9B-4A56-BE29-A631039CB705", "Multiple staff/group match against the same Active Directory record and will not be processed:");
				ADNotification.ShowWarning(message + System.Environment.NewLine + messageBuilder.ToStringWithNewLineBetweenAppends());
			}

			return result;

			string GetErrorMessage(Guid guid, IEnumerable<string> duplicatedEntity)
			{
				return Res.GetString("41B0BC6B-CDD4-4A13-B10F-205936F7513A", "{0} <--> Active Directory record with Guid {1}", string.Join(", ", duplicatedEntity.OrderBy(a => a)), guid);
			}

			void AddToDuplications(Guid guid, string identity)
			{
				if (duplications.ContainsKey(guid))
				{
					duplications[guid].Add(identity);
				}
				else
				{
					duplications.Add(guid, new HashSet<string>() { identity });
				}
			}

			void CheckForADLinkConflictInSyncedEntities()
			{
				foreach (var groupedByGuid in synchronisedEntities.Where(e => e.Guid != default).GroupBy(e => e.Guid).Where(g => g.Count() > 1))
				{
					groupedByGuid.ForEach(e =>
					{
						AddToDuplications(groupedByGuid.Key, e.EnterpriseIdentity);
						e.EnterpriseEntity.HasChanges = false;
						result.Add(e);
					});
				}
			}

			void CheckForADLinkConflictWithExistingBizos()
			{
				foreach (var syncedEntity in synchronisedEntities.Where(e => e.GuidPropertyInfo.HasChanges))
				{
					List<string> duplicationList;
					if (syncedEntity is ADUser)
					{
						duplicationList = AllLinkedStaff.Where(s => (s.GS_ActiveDirectoryObjectGuid == (ZGuid)syncedEntity.GuidPropertyInfo.Value) && (s.PK != ((BusinessObject)syncedEntity.EnterpriseEntity).PK)).Select(s => (string)s.GS_LoginName).ToList();
					}
					else if (syncedEntity is ADGroup)
					{
						duplicationList = AllLinkedGroups.Where(g => (g.GG_ActiveDirectoryObjectGuid == (ZGuid)syncedEntity.GuidPropertyInfo.Value) && (g.PK != ((BusinessObject)syncedEntity.EnterpriseEntity).PK)).Select(g => (string)g.GG_Desc).ToList();
					}
					else
					{
						ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Unsupported object type has been synchronised: " + syncedEntity.GetType().Name));
						continue;
					}

					if (duplicationList.Count > 0)
					{
						duplicationList.ForEach(d =>
						{
							AddToDuplications(syncedEntity.Guid, d);
						});

						AddToDuplications(syncedEntity.Guid, syncedEntity.EnterpriseIdentity);
						syncedEntity.EnterpriseEntity.HasChanges = false;
						result.Add(syncedEntity);
					}
				}
			}
		}

		public IEnumerable<IADEntity> EntitiesWithErrors => entitiesWithErrors;

		void SyncStaff(IEnumerable<GlbStaff> staffToSync, SyncMode? preferredSyncMode)
		{
			UpdateProgress(Res.GetString("5baa72b6-a30f-45c6-9265-6f8495d40aa5", "Synchronizing {0} user(s)", staffToSync.Count()));

			var adUsersToSync = GetADUsers(staffToSync);
			SynchroniseEntities(adUsersToSync, preferredSyncMode);
			synchronisedEntities.AddRange(adUsersToSync);
		}

		bool ShouldSyncFromADToEnterprise(SyncMode? preferredSyncMode, bool isGroup)
		{
			// preferredSyncMode should override registry setting, only use registry setting if preferredSyncMode is not set
			SyncDirection syncDirection;
			var syncMode = ActiveDirectoryRegistry.Instance.SyncMode;
			if (isGroup)
			{
				syncDirection = ActiveDirectoryRegistry.Instance.SyncDirectionGroup;
			}
			else
			{
				syncDirection = ActiveDirectoryRegistry.Instance.SyncDirection;
			}
			return (preferredSyncMode == SyncMode.ADIsMaster) ||
				(!preferredSyncMode.HasValue && (syncDirection == SyncDirection.TwoWay || syncMode == SyncMode.ADIsMaster));
		}

		protected virtual IEnumerable<GlbStaff> GetStaffToSync(SyncMode? preferredSyncMode)
		{
			if (preferredSyncMode.HasValue)
			{
				return Factory.Load<GlbStaff>(GetStaffToSynchroniseQuery(true));
			}
			else
			{
				var staffToSync = Factory.Load<GlbStaff>(GetStaffToSynchroniseQuery(false)).ToList();
				staffToSync.AddRange(GetLastFailedSyncStaff());
				if (ShouldSyncFromADToEnterprise(preferredSyncMode, isGroup: false))
				{
					foreach (IDomainCredentials domainCredentials in ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value)
					{
						var adUsersWithChanges = ObjectFactory.Get<IDirectorySearcherProvider>().GetDirectorySearcher(domainCredentials, false).FindUsersChangedAfter(ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value, domainCredentials.UserOrganisationalUnit);
						foreach (var adUser in adUsersWithChanges)
						{
							var linkedStaff = AllLinkedStaff.FirstOrDefault(e => e.GS_ActiveDirectoryObjectGuid == adUser.Guid);
							if ((linkedStaff != null) && (!staffToSync.Exists(e => e.GS_ActiveDirectoryObjectGuid == linkedStaff.GS_ActiveDirectoryObjectGuid)))
							{
								staffToSync.Add(linkedStaff);
							}
						}
					}
				}
				return staffToSync.GroupBy(s => s.PK).Select(g => g.First());
			}
		}

		protected virtual IEnumerable<GlbGroup> GetGroupsToSync(SyncMode? preferredSyncMode)
		{
			if (preferredSyncMode.HasValue)
			{
				return Factory.Load<GlbGroup>(GetGroupsToSynchroniseQuery(true));
			}
			else
			{
				var groupsToSync = Factory.Load<GlbGroup>(GetGroupsToSynchroniseQuery(false)).ToList();
				groupsToSync.AddRange(GetLastFailedSyncGroups());
				if (ShouldSyncFromADToEnterprise(preferredSyncMode, isGroup: true))
				{
					foreach (IDomainCredentials domainCredentials in ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value)
					{
						var adGroupsWithChanges = ObjectFactory.Get<IDirectorySearcherProvider>().GetDirectorySearcher(domainCredentials, false).FindGroupsChangedAfter(ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value, domainCredentials.UserOrganisationalUnit);
						foreach (var adGroup in adGroupsWithChanges)
						{
							var linkedGroup = AllLinkedGroups.FirstOrDefault(e => e.GG_ActiveDirectoryObjectGuid == adGroup.Guid);
							if ((linkedGroup != null) && (!groupsToSync.Exists(e => e.GG_ActiveDirectoryObjectGuid == linkedGroup.GG_ActiveDirectoryObjectGuid)))
							{
								groupsToSync.Add(linkedGroup);
							}
						}
					}
				}
				return groupsToSync.GroupBy(g => g.PK).Select(g => g.First());
			}
		}

		GlbStaff[] AllLinkedStaff => allLinkedStaff ?? (allLinkedStaff = Factory.Load<GlbStaff>(GetADLinkedStaffQuery()));
		GlbStaff[] allLinkedStaff;

		GlbGroup[] AllLinkedGroups => allLinkedGroups ?? (allLinkedGroups = Factory.Load<GlbGroup>(GetADLinkedGroupsQuery()));
		GlbGroup[] allLinkedGroups;

		GlbStaff[] GetLastFailedSyncStaff() => lastFailedSyncStaff ?? (lastFailedSyncStaff = Factory.Load<GlbStaff>(GetLastFailedSyncStaffQuery(ActiveDirectoryRegistry.Instance.LastFailedSyncStaffPKs.Value)));
		GlbStaff[] lastFailedSyncStaff;

		GlbGroup[] GetLastFailedSyncGroups() => lastFailedSyncGroups ?? (lastFailedSyncGroups = Factory.Load<GlbGroup>(GetLastFailedSyncGroupsQuery(ActiveDirectoryRegistry.Instance.LastFailedSyncGroupPKs.Value)));
		GlbGroup[] lastFailedSyncGroups;

		void SyncGroups(IEnumerable<GlbGroup> groupsToSync, SyncMode? preferredSyncMode)
		{
			UpdateProgress(Res.GetString("9ba4f95c-e600-49ca-8056-a1462ef71f6a", "Synchronizing {0} group(s)", groupsToSync.Count()));

			var adGroupsToSync = GetADGroups(groupsToSync);
			SynchroniseEntities(adGroupsToSync, preferredSyncMode);
			synchronisedEntities.AddRange(adGroupsToSync);
		}

		IEnumerable<ADGroup> GetADGroups(IEnumerable<GlbGroup> enterpriseGroups)
		{
			return enterpriseGroups.Select(g => new ADGroup(g)).ToArray();
		}

		protected virtual IEnumerable<ADUser> GetADUsers(IEnumerable<GlbStaff> enterpriseStaff)
		{
			return enterpriseStaff.Select(s => new ADUser(s)).ToArray();
		}

		void SynchroniseEntities(IEnumerable<IADEntity> entities, SyncMode? preferredSyncMode)
		{
			entities.ForEach(e => SynchroniseEntity(e, preferredSyncMode));
		}

		public event EntitySynchronisedEventHandler EntitySynchronised;

		readonly List<(ISyncEvent syncEvent, IADEntity adEntity)> activationChangedList = new List<(ISyncEvent, IADEntity)>();

		void ReportActivationChanged()
		{
			if (activationChangedList.Count == 0)
			{
				return;
			}

			foreach (var activationChanged in activationChangedList)
			{
				bool? adCurrentActiveStatus = null;
				var adReloadException = string.Empty;
				try
				{
					var directoryEntry = activationChanged.adEntity.GetDirectoryEntry();
					directoryEntry?.RefreshCache();
					adCurrentActiveStatus = directoryEntry?.IsActive;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					adReloadException = ex.GetType().ToString() + ": " + ex.ToString();
				}

				var result = GetActivationChangedMessage(activationChanged.syncEvent, activationChanged.adEntity, adCurrentActiveStatus, adReloadException);
				UpdateProgress(Res.GetString("e8f74083-7a85-4935-b33a-27114aff47b4", $"Activation status changed"), result.Message, result.LogType);

				if (!adCurrentActiveStatus.HasValue || adCurrentActiveStatus.Value != activationChanged.adEntity.EnterpriseEntity.IsActive)
				{
					ReportInconsistentActiveStateAfterADSync(result.Message);
				}
			}

			activationChangedList.Clear();
		}

		(string Message, LogType LogType) GetActivationChangedMessage(ISyncEvent syncEvent, IADEntity entity, bool? adCurrentActiveStatus, string adException)
		{
			var message = new StringBuilder();
			var logType = LogType.Error;

			var syncedStatus = syncEvent.SynchronisedValue.Equals(ZBool.True) ? (NoResString)"activated" : (NoResString)"deactivated";
			var syncedFrom = syncEvent.SynchronisedValue.Equals(syncEvent.ADStartingValue) ? "AD" : "CW1";
			message.Append($"{DataBoundResourceStrings.GetStringForTable(entity.EnterpriseEntity.GetType())} '{entity.EnterpriseIdentity}' has been {syncedStatus} in {syncedFrom}");

			if (!adCurrentActiveStatus.HasValue || !string.IsNullOrEmpty(adException))
			{
				message.Append((NoResString)", and an exception happened when trying to retrieve the new AD value: " + adException);
			}
			else
			{
				var syncedTo = syncEvent.SynchronisedValue.Equals(syncEvent.ADStartingValue) ? "CW1" : "AD";
				if (adCurrentActiveStatus.Value != entity.EnterpriseEntity.IsActive)
				{
					var notSyncedStatus = syncEvent.SynchronisedValue.Equals(ZBool.True) ? (NoResString)"inactive" : (NoResString)"active";
					message.Append($", but it is still {notSyncedStatus} in {syncedTo}.");
				}
				else
				{
					message.Append($" and successfully {syncedStatus} in {syncedTo}.");
					logType = LogType.Warning;
				}
			}

			return (message.ToString(), logType);
		}

		public static void ReportInconsistentActiveStateAfterADSync(string reference)
		{
			if (EnvProxy.IsHostedWithCargowise || ClientHookLoader.Instance.Client == Clients.EDI)
			{
				ErrorReporter.ReportOnce("InconsistentActiveStateAfterADSync", reference + "\r\n\r\n" + "Please assign this Issue to ADI or IS to investigate immediately. Thanks");
			}
		}

		bool IsActiveProperty(ISyncEvent syncEvent) => syncEvent.PropertyName == GlbStaffSchema.GS_IsActive.Name || syncEvent.PropertyName == GlbGroupSchema.GG_IsActive.Name;

		void CheckForActivationChanged(object sender, EntitySynchronisedEventArgs e)
		{
			foreach (var syncEvent in e.SyncEvents)
			{
				if (IsActiveProperty(syncEvent) &&
					syncEvent.EnterpriseStartingValue != null &&
					syncEvent.ADStartingValue != null &&
					!syncEvent.EnterpriseStartingValue.Equals(syncEvent.ADStartingValue))
				{
					activationChangedList.Add((syncEvent, e.Entity));
				}
			}
		}

		void SynchroniseEntity(IADEntity entity, SyncMode? preferredSyncMode)
		{
			entity.Synchronised += EntitySynchronised;
			entity.Synchronised += CheckForActivationChanged;
			if (!entity.Synchronise(preferredSyncMode))
			{
				entitiesWithErrors.Add(entity);
			}
			entity.Synchronised -= EntitySynchronised;
			entity.Synchronised -= CheckForActivationChanged;
			UpdateProgress(++unitsComplete);
		}

		public void Save()
		{
			IgnoreConcurrencyOnSyncedEntities();
			Factory.Save();
			synchronisedEntities.ForEach(e =>
			{
				if (!e.CommitChanges())
				{
					entitiesWithErrors.Add(e);
				}
			});
			SyncUsersToRoboticGroupIfRequired(synchronisedEntities.Where(a => !entitiesWithErrors.Contains(a)));
			ReportActivationChanged();
			synchronisedEntities.Clear();
			allLinkedStaff = null;
			allLinkedGroups = null;
		}

		bool ShouldSyncRoboticGroup(out string roboticGroupName)
		{
			roboticGroupName = ActiveDirectoryRegistry.Instance.RoboticProcessAutomationGroup.Value;
			return DataRegistry.Instance.EnableRPAOnWiseCloud && !roboticGroupName.IsNullOrEmpty();
		}

		public void SyncUsersToRoboticGroupIfRequired(IEnumerable<IADEntity> entities)
		{
			if (ShouldSyncRoboticGroup(out var roboticGroupName))
			{
				var directorySearcher = ObjectFactory.Get<IDirectorySearcherProvider>().GetDirectorySearcher(ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value.DefaultDomainCredentials, true);
				var roboticGroup = directorySearcher.FindGroup(roboticGroupName);

				if (roboticGroup == null)
				{
					var message = Res.GetString("9F2026D1-16D5-4D5A-A0DC-6B48806866C4", "Robotic group '{0}' could not be found.", roboticGroupName);
					throw new NoDomainPrivilegeException(message);
				}
				else
				{
					entities.ForEach(e =>
					{
						if (e is IADUser user)
						{
							if (((GlbStaff)user.EnterpriseEntity).GS_IsRobot)
							{
								roboticGroup.AddMember(user.GetDirectoryEntry());
							}
							else
							{
								roboticGroup.RemoveMember(user.GetDirectoryEntry());
							}
						}
					});
					try
					{
						roboticGroup.CommitChanges();
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						var errorMessage = Res.GetString("16A80394-9DA0-4DA3-8FB0-1FA2E81B2ADD", "Domain user does not have write privileges to the Robotic Group '{0}'.", roboticGroupName);
						throw new NoDomainPrivilegeException(errorMessage, ex);
					}
				}
			}
		}

		void IgnoreConcurrencyOnSyncedEntities()
		{
			foreach (var entity in synchronisedEntities)
			{
				((BusinessObject)entity.EnterpriseEntity).SetConcurrencyPolicyOnProperties(ConcurrencyPolicy.Ignore);
				if (entity.EnterpriseEntity is GlbStaff staff)
				{
					staff.Person?.SetConcurrencyPolicyOnProperties(ConcurrencyPolicy.Ignore);
				}
			}
		}

		#endregion

		#region UpdateProgress

		public event EventHandler<SyncProgressEventArgs> ProgressUpdated;

		int totalUnits;
		int unitsComplete;

		protected void UpdateProgress(string taskName = "", string details = "", LogType logType = LogType.Information)
		{
			UpdateProgress(unitsComplete, taskName, details, logType);
		}

		void UpdateProgress(int unitsComplete, string taskName = "", string details = "", LogType logType = LogType.Information)
		{
			if (ProgressUpdated != null)
			{
				var percent = 0;
				if (totalUnits > 0)
				{
					percent = (int)(unitsComplete / (double)totalUnits * 100);
				}
				var info = new SyncProgressEventArgs { OverallPercentComplete = percent, TaskDetails = details, TaskName = taskName, LogType = logType };
				ProgressUpdated(this, info);
			}
		}

		void OnSyncCompleted()
		{
			if (ProgressUpdated != null)
			{
				ProgressUpdated(this, new SyncProgressEventArgs { OverallPercentComplete = 100, TaskName = Res.GetString("befa0740-5ec8-457a-8a31-b57a0bec9f06", "Complete") });
			}
		}

		#endregion

		#region Queries

		public static ZQuery GetGroupsToSynchroniseQuery(bool getAll)
		{
			var query = new ZQuery(GlbGroupSchema.GG_IsSystemDefined, false);
			query.ReLoadExistingRows = true; // Don't use cached table, we need latest data to sync
			var isActiveQuery = new ZQuery(GlbGroupSchema.GG_IsActive, true);
			var adGuidPresentQuery = new ZQuery(GlbGroupSchema.GG_ActiveDirectoryObjectGuid, SQLComparisonOperator.NotEqual, null);
			query.AddToFilter(new ZQuery(isActiveQuery, JoinCondition.Or, adGuidPresentQuery));
			query.AddToFilter(GlbGroupSchema.GG_Type, GlbGroupTypeList.Codes.Staff);
			if (!getAll)
			{
				query.AddToFilter(GlbGroupSchema.GG_SystemLastEditTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value);
			}
			return query;
		}

		ZQuery GetStaffToSynchroniseQuery(bool getAll)
		{
			var query = new ZQuery(GlbStaffSchema.GS_IsSystemAccount, false);
			query.ReLoadExistingRows = true; // Don't use cached table, we need latest data to sync
			query.AddToFilter(GlbStaffSchema.GS_IsResource, false);

			var adGuidNotPresentQuery = new ZQuery();
			adGuidNotPresentQuery.AddToFilter(new ZQuery(GlbStaffSchema.GS_ActiveDirectoryObjectGuid, SQLComparisonOperator.Equal, ZGuid.Invalid), JoinCondition.Or);
			adGuidNotPresentQuery.AddToFilter(new ZQuery(GlbStaffSchema.GS_ActiveDirectoryObjectGuid, SQLComparisonOperator.Equal, null), JoinCondition.Or);
			var unlinkedStaffQuery = new ZQuery(GlbStaffSchema.GS_IsActive, true);
			unlinkedStaffQuery.AddToFilter(GlbStaffSchema.GS_CanLogin, true);
			unlinkedStaffQuery.AddToFilter(adGuidNotPresentQuery);

			var linkedStaffQuery = GetADLinkedStaffQuery();
			if (!getAll)
			{
				linkedStaffQuery.AddToFilter(GlbStaffSchema.GS_SystemLastEditTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value);
			}
			query.AddToFilter(new ZQuery(unlinkedStaffQuery, JoinCondition.Or, linkedStaffQuery));
			return query;
		}

		static ZQuery GetADLinkedStaffQuery()
		{
			// any staff with GS_ActiveDirectoryObjectGuid set regardless whether it is active
			var query = new ZQuery(GlbStaffSchema.GS_ActiveDirectoryObjectGuid, SQLComparisonOperator.NotEqual, null);
			query.AddToFilter(new ZQuery(GlbStaffSchema.GS_ActiveDirectoryObjectGuid, SQLComparisonOperator.NotEqual, ZGuid.Invalid));
			return query;
		}

		static ZQuery GetADLinkedGroupsQuery()
		{
			// any group with GG_ActiveDirectoryObjectGuid set regardless whether it is active
			var query = new ZQuery(GlbGroupSchema.GG_ActiveDirectoryObjectGuid, SQLComparisonOperator.NotEqual, null);
			query.AddToFilter(new ZQuery(GlbGroupSchema.GG_ActiveDirectoryObjectGuid, SQLComparisonOperator.NotEqual, ZGuid.Invalid));
			return query;
		}

		static ZQuery GetLastFailedSyncStaffQuery(Guid[] guids)
		{
			var query = new ZQuery { AllowTableValuedParameters = true }.AddToFilter(GlbStaffSchema.PK, guids);
			query.AddToFilter(new ZQuery(GlbStaffSchema.GS_ActiveDirectoryObjectGuid, SQLComparisonOperator.NotEqual, null));
			return query;
		}

		static ZQuery GetLastFailedSyncGroupsQuery(Guid[] guids)
		{
			var query = new ZQuery { AllowTableValuedParameters = true }.AddToFilter(GlbGroupSchema.PK, guids);
			query.AddToFilter(new ZQuery(GlbGroupSchema.GG_ActiveDirectoryObjectGuid, SQLComparisonOperator.NotEqual, null));
			return query;
		}

		#endregion
	}
}
