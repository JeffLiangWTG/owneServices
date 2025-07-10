using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ActiveDirectory;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security.ActiveDirectory.Synchronisation
{
	public abstract class SyncStrategyBase<TActiveDirectory, TEnterprise> : ISyncStrategy
		where TActiveDirectory : IADEntity
		where TEnterprise : IADLinkedEntity
	{
		protected SyncStrategyBase(IADEntity adEntity)
		{
			Argument.NotNull(adEntity, "adEntity");

			syncResult = new EntitySynchronisedEventArgs
			{
				SyncEvents = new List<ISyncEvent>(),
				Entity = adEntity,
			};
			this.adEntity = (TActiveDirectory)adEntity;
		}

		readonly EntitySynchronisedEventArgs syncResult;

		protected TEnterprise EnterpriseEntity => (TEnterprise)adEntity.EnterpriseEntity;

		protected readonly TActiveDirectory adEntity;

		protected BusinessObjectFactory Factory => EnterpriseEntity.Factory;

		public EntitySynchronisedEventArgs Synchronise()
		{
			SynchroniseCore(adEntity);
			return syncResult;
		}

		protected void SetDomainName()
		{
			SetValue(e => e.DomainCredentials?.DomainName, EnterpriseEntity.DomainNameInfo.OriginalValue, EnterpriseEntity.DomainNameInfo, e => EnterpriseEntity.DomainName, e => EnterpriseEntity.DomainName, e => EnterpriseEntity.DomainName);
		}

		protected void SetIsLinked()
		{
			SetValue(e => EnterpriseEntity.IsADLinked, EnterpriseEntity.IsADLinked, EnterpriseEntity.IsADLinkedInfo, e => EnterpriseEntity.IsADLinked, e => EnterpriseEntity.IsADLinked, e => EnterpriseEntity.IsADLinked);
		}

		protected abstract void SynchroniseCore(TActiveDirectory adEntity);

		protected void SetValue(Action<TActiveDirectory> valueSetter, Action<TActiveDirectory> adValueSetterIfNullDirectoryEntry = null)
		{
			if (adEntity.GetDirectoryEntry() != null)
			{
				valueSetter(adEntity);
			}
			else if (adValueSetterIfNullDirectoryEntry != null)
			{
				adValueSetterIfNullDirectoryEntry(adEntity);
			}
		}

		protected void SetValue(Func<TActiveDirectory, object> adValueGetter, object enterpriseValue, ZPropertyInfo propertyInfo, Func<TActiveDirectory, object> valueSetter, Func<TActiveDirectory, object> adValueSetterIfNullDirectoryEntry = null, Func<TActiveDirectory, object> adValueGetterIfNullDirectoryEntry = null)
		{
			Argument.NotNull(valueSetter, nameof(valueSetter));
			Argument.NotNull(adValueGetter, nameof(adValueGetter));

			if (propertyInfo == null || IsSynced(propertyInfo.Name)) // propertyInfo == null happens when syncing group members
			{
				object adValue;
				try
				{
					adValue = adEntity.GetDirectoryEntry() != null ? adValueGetter(adEntity) : null;
				}
				catch (InvalidCastException)
				{
					if (propertyInfo != null)
					{
						var adAttribute = AttributeMap.Current.MapItems.Cast<AttributeMapItem>().FirstOrDefault(item => item.EnterpriseColumnName.EqualsIgnoringCase(propertyInfo.Name));
						if (adAttribute != null)
						{
							throw new DirectoryServicesException(string.Format(@"Cannot synchronize {0} with AD attribute {1}. Please review the mappings in the {2} setting in the Registry.",
								propertyInfo.Name,
								adAttribute.ActiveDirectoryAttributeName,
								((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.AttributeMapping).LocationMultilingual));
						}
					}

					throw;
				}

				object synchronisedValue;
				if (adEntity.GetDirectoryEntry() != null)
				{
					synchronisedValue = valueSetter(adEntity);
				}
				else
				{
					synchronisedValue = adValueSetterIfNullDirectoryEntry != null ? adValueSetterIfNullDirectoryEntry(adEntity) : enterpriseValue;
				}

				var history = new SyncEvent { ADStartingValue = adValue, EnterpriseStartingValue = enterpriseValue, SynchronisedValue = synchronisedValue, PropertyName = propertyInfo?.Name };
				syncResult.SyncEvents.Add(history);
			}
			else if (IsForcedToShowInReport(propertyInfo.Name))
			{
				var adValue = adEntity.GetDirectoryEntry() != null ? adValueGetter(adEntity) : adValueGetterIfNullDirectoryEntry?.Invoke(adEntity);
				var history = new SyncEvent { ADStartingValue = adValue, EnterpriseStartingValue = enterpriseValue, SynchronisedValue = enterpriseValue, PropertyName = propertyInfo.Name, IsForcedToShowInReport = true };
				syncResult.SyncEvents.Add(history);
			}
		}

		protected static bool IsSynced(SchemaColumn schemaColumn) => AttributeMap.Current.IsSynced(schemaColumn);

		protected static bool IsSynced(string columnName)
		{
			return AttributeMap.Current.IsSynced(columnName)
				|| columnName == GlbGroupSchema.Constants.GG_IsActive // Groups don't have a concept of active/inactive in AD
				|| columnName == GlbStaffSchema.Constants.GS_DomainName || columnName == GlbGroupSchema.Constants.GG_DomainName; // We don't sync DomainName but need to show in the report
		}

		protected static bool IsForcedToShowInReport(string columnName)
		{
			return columnName == GlbGroupSchema.Constants.GG_Desc
				|| columnName == GlbStaffSchema.Constants.GS_LoginName
				|| columnName == GlbStaff.Schema.IsADLinked
				|| columnName == GlbGroup.Schema.IsADLinked;
		}
	}
}
