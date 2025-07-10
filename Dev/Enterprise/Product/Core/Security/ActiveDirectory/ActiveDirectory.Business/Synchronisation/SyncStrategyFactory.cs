using System;
using System.Globalization;
using Enterprise.Integration;

namespace Enterprise.Security.ActiveDirectory.Synchronisation
{
	static class SyncStrategyFactory
	{
		internal static ISyncStrategy GetSyncStrategy(IADEntity adEntity, SyncMode? preferredSyncMode, bool isInitialSync)
		{
			return isInitialSync ? GetInitialSyncStrategy(adEntity) : GetOngoingSyncStrategy(adEntity, preferredSyncMode);
		}

		#region Initial Sync Strategies

		static ISyncStrategy GetInitialSyncStrategy(IADEntity adEntity)
		{
			var isUser = adEntity is ADUser;
			var isGroup = adEntity is ADGroup;
			var syncMode = adEntity.SyncMode;
			var syncDirection = adEntity.SyncDirection;

			if (syncMode == SyncMode.ADIsMaster)
			{
				if (isUser)
				{
					return new ADMasterUserSyncStrategy(adEntity);
				}
				else if (isGroup)
				{
					if (syncDirection == SyncDirection.TwoWay)
					{
						return new MembershipUnionGroupSyncStrategy(adEntity);
					}
					else if (syncDirection == SyncDirection.OneWay)
					{
						return new ADMasterGroupSyncStrategy(adEntity);
					}
				}
			}
			else if (syncMode == SyncMode.EnterpriseIsMaster)
			{
				if (isUser)
				{
					return new EnterpriseMasterUserSyncStrategy(adEntity);
				}
				else if (isGroup)
				{
					if (syncDirection == SyncDirection.TwoWay)
					{
						return new MembershipUnionGroupSyncStrategy(adEntity, false);
					}
					else if (syncDirection == SyncDirection.OneWay)
					{
						return new EnterpriseMasterGroupSyncStrategy(adEntity);
					}
				}
			}

			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "No initial synchronisation strategy was found for entity type {0} with SyncMode {1} and SyncDirection {2}", adEntity.GetType().Name, syncMode, syncDirection));
		}

		#endregion

		#region Ongoing Sync Strategies

		static ISyncStrategy GetOngoingSyncStrategy(IADEntity adEntity, SyncMode? preferredSyncMode)
		{
			var syncDirection = adEntity.SyncDirection;
			if (syncDirection == SyncDirection.TwoWay)
			{
				return GetOngoingTwoWaySyncStrategy(adEntity, preferredSyncMode);
			}
			else if (syncDirection == SyncDirection.OneWay)
			{
				return GetOngoingOneWaySyncStrategy(adEntity, preferredSyncMode);
			}
			else
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "No ongoing synchronisation strategy was found for entity type {0} with SyncDirection {1}", adEntity.GetType().Name, syncDirection));
			}
		}

		static ISyncStrategy GetOngoingTwoWaySyncStrategy(IADEntity adEntity, SyncMode? preferredSyncMode)
		{
			var isUser = adEntity is ADUser;
			var isGroup = adEntity is ADGroup;

			if (preferredSyncMode.HasValue)
			{
				if (preferredSyncMode.Value == SyncMode.ADIsMaster)
				{
					if (isUser)
					{
						return new ADMasterUserSyncStrategy(adEntity);
					}
					else if (isGroup)
					{
						return new MembershipUnionGroupSyncStrategy(adEntity);
					}
				}
				else if (preferredSyncMode.Value == SyncMode.EnterpriseIsMaster)
				{
					if (isUser)
					{
						return new EnterpriseMasterUserSyncStrategy(adEntity);
					}
					else if (isGroup)
					{
						return new MembershipUnionGroupSyncStrategy(adEntity, false);
					}
				}
			}
			else
			{
				if (isUser)
				{
					return new SyncLatestUserSyncStrategy(adEntity);
				}
				else if (isGroup)
				{
					return new SyncLatestGroupSyncStrategy(adEntity);
				}
			}

			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "No ongoing two-way synchronisation strategy was found for entity type {0} with Sync Mode {1}", adEntity.GetType().Name, preferredSyncMode.HasValue ? preferredSyncMode.Value.ToString() : "null"));
		}

		static ISyncStrategy GetOngoingOneWaySyncStrategy(IADEntity adEntity, SyncMode? preferredSyncMode)
		{
			var isUser = adEntity is ADUser;
			var isGroup = adEntity is ADGroup;
			SyncMode syncMode;
			if (preferredSyncMode.HasValue)
			{
				syncMode = preferredSyncMode.Value;
			}
			else
			{
				syncMode = adEntity.SyncMode;
			}

			if (syncMode == SyncMode.ADIsMaster)
			{
				if (isUser)
				{
					return new ADMasterUserSyncStrategy(adEntity);
				}
				else if (isGroup)
				{
					return new ADMasterGroupSyncStrategy(adEntity);
				}
			}
			else if (syncMode == SyncMode.EnterpriseIsMaster)
			{
				if (isUser)
				{
					return new EnterpriseMasterUserSyncStrategy(adEntity);
				}
				else if (isGroup)
				{
					return new EnterpriseMasterGroupSyncStrategy(adEntity);
				}
			}

			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "No ongoing one-way synchronisation strategy was found for entity type {0} with Sync Mode {1}", adEntity.GetType().Name, preferredSyncMode.HasValue ? preferredSyncMode.Value.ToString() : "null"));
		}

		#endregion
	}
}
