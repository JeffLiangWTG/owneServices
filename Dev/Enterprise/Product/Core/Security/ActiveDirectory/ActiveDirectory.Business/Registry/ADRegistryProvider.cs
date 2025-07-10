using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ActiveDirectory;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

#if DEBUG
[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(CargoWise.ActiveDirectory.TestFramework.TestConstants))]
#endif

namespace Enterprise.Security.ActiveDirectory.Registry
{
#if DEBUG
	using CargoWise.ActiveDirectory.TestFramework;
#endif

	public class ADRegistryProvider : IADRegistry, IDomainCredentialsProvider
	{
		bool IADRegistry.IsIntegrationEnabled
		{
			get { return ActiveDirectoryRegistry.Instance.IsIntegrationEnabled; }
#if DEBUG
			set
			{
				ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = value;

				if (value)
				{
					// To avoid unit tests running by datbackground local account accessing CORP domain without credentials
					var directorySearcher = new DirectorySearcherWrapper(TestConstants.ADTestUserAccount.NameWithDomain, TestConstants.ADTestUserAccount.Password, TestConstants.Domain);
					DirectorySearcherFactory.DirectorySearcherOverride_ForTest = directorySearcher;
				}
				else
				{
					DirectorySearcherFactory.DirectorySearcherOverride_ForTest = null;
				}
			}
#endif
		}

		public EntitiesToSync EntitiesToSync
		{
			get => ActiveDirectoryRegistry.Instance.EntitiesToSync;
#if DEBUG
			set => ActiveDirectoryRegistry.Instance.EntitiesToSync = value;
#endif
		}

		public SyncMode SyncMode
		{
			get => ActiveDirectoryRegistry.Instance.SyncMode;
#if DEBUG
			set => ActiveDirectoryRegistry.Instance.SyncMode = value;
#endif
		}

		public SyncDirection SyncDirection
		{
			get => ActiveDirectoryRegistry.Instance.SyncDirection;
#if DEBUG
			set => ActiveDirectoryRegistry.Instance.SyncDirection = value;
#endif
		}

		bool IADRegistry.IsSingleSignOnEnabled
		{
			get { return ActiveDirectoryRegistry.Instance.IsSingleSignOnEnabled; }
#if DEBUG
			set { ActiveDirectoryRegistry.Instance.IsSingleSignOnEnabled = value; }
#endif
		}

		string IADRegistry.UserLoginPrefix
		{
			get { return ActiveDirectoryRegistry.Instance.UserLoginPrefix.Value; }
#if DEBUG
			set { ActiveDirectoryRegistry.Instance.UserLoginPrefix.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		string IADRegistry.GroupNamePrefix
		{
			get { return ActiveDirectoryRegistry.Instance.GroupNamePrefix.Value; }
#if DEBUG
			set { ActiveDirectoryRegistry.Instance.GroupNamePrefix.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public IEnumerable<IDomainCredentials> DomainCredentialsCollection
		{
			get => ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value.Cast<IDomainCredentials>();
#if DEBUG
			set
			{
				var collection = new DomainCredentialsCollection();
				collection.AddRange(value.ToArray());

				using (ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.DataType.SuspendValidation())
				{
					ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
				}
			}
#endif
		}

		public ICodeDescriptionPairList DomainCredentialsCollectionAsCodeDescriptionPairList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				foreach (var domainCredentials in DomainCredentialsCollection)
				{
					result.AddPair(domainCredentials.DomainName, domainCredentials.DomainName);
				}
				return result;
			}
		}

		public IDomainCredentials DefaultDomainCredentials
		{
			get => ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value.DefaultDomainCredentials;
#if DEBUG
			set
			{
				var collection = new DomainCredentialsCollection();
				collection.Add((BusinessObject)value);

				using (ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.DataType.SuspendValidation())
				{
					ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
				}
			}
#endif
		}

		public string DomainCredentialsCollectionRegistryLocation => ((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.DomainCredentialsCollection).LocationMultilingual;

		public bool HasMultipleDomains => DomainCredentialsCollection.Count() > 1;

		public bool IsColumnSyncedOneWayFromAD(string columnName)
		{
			return ActiveDirectoryRegistry.Instance.SyncMode == SyncMode.ADIsMaster
				&& ActiveDirectoryRegistry.Instance.SyncDirection == SyncDirection.OneWay
				&& ActiveDirectoryRegistry.Instance.AttributeMapping.Value.IsSynced(columnName);
		}

		public bool ShouldUnlinkInactiveStaff
		{
			get => ActiveDirectoryRegistry.Instance.ShouldUnlinkInactiveStaff.Value;
#if DEBUG
			set => ActiveDirectoryRegistry.Instance.ShouldUnlinkInactiveStaff.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}

		public bool DisableADPasswordChange
		{
			get => ActiveDirectoryRegistry.Instance.DisableADPasswordChange.Value;
#if DEBUG
			set => ActiveDirectoryRegistry.Instance.DisableADPasswordChange.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}
	}
}
