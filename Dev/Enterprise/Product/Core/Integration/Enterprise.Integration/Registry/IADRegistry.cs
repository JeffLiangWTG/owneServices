using System.Collections.Generic;
using CargoWise.Integration;

namespace Enterprise.Integration
{
	public interface IADRegistry
	{
		bool IsIntegrationEnabled
		{
			get;
#if DEBUG
			set;
#endif
		}

		EntitiesToSync EntitiesToSync
		{
			get;
#if DEBUG
			set;
#endif
		}

		SyncMode SyncMode
		{
			get;
#if DEBUG
			set;
#endif
		}

		SyncDirection SyncDirection
		{
			get;
#if DEBUG
			set;
#endif
		}

		bool IsSingleSignOnEnabled
		{
			get;
#if DEBUG
			set;
#endif
		}

		string UserLoginPrefix
		{
			get;
#if DEBUG
			set;
#endif
		}

		string GroupNamePrefix
		{
			get;
#if DEBUG
			set;
#endif
		}

		IEnumerable<IDomainCredentials> DomainCredentialsCollection
		{
			get;
#if DEBUG
			set;
#endif
		}

		ICodeDescriptionPairList DomainCredentialsCollectionAsCodeDescriptionPairList
		{
			get;
		}

		IDomainCredentials DefaultDomainCredentials
		{
			get;
#if DEBUG
			set;
#endif
		}

		string DomainCredentialsCollectionRegistryLocation
		{
			get;
		}

		bool HasMultipleDomains
		{
			get;
		}

		bool IsColumnSyncedOneWayFromAD(string columnName);

		bool ShouldUnlinkInactiveStaff
		{
			get;
#if DEBUG
			set;
#endif
		}

		bool DisableADPasswordChange
		{
			get;
#if DEBUG
			set;
#endif
		}
	}
}
