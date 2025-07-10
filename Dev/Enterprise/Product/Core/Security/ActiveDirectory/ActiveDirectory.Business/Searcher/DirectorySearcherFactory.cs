using System;
using System.Runtime.Caching;
using System.Runtime.InteropServices;
using CargoWise.ActiveDirectory;
using CargoWise.Common;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Security.ActiveDirectory.DomainCredentialsValidation;

namespace Enterprise.Security.ActiveDirectory
{
	public static class DirectorySearcherFactory
	{
		public static IDirectorySearcher GetDirectorySearcher(IDomainCredentials domainCredentials, bool requireDomainWritePrivilege = false)
		{
			try
			{
#if DEBUG
				if (DirectorySearcherOverride_ForTest != null)
				{
					return DirectorySearcherOverride_ForTest;
				}
#endif
				var cachedSearcher = GetFromCache(domainCredentials?.DomainName, requireDomainWritePrivilege);
				if (cachedSearcher != null)
				{
					return cachedSearcher;
				}

				var directorySearcher = new DirectorySearcherWrapper(
						domainCredentials?.DomainUserName,
						domainCredentials?.DomainUserPassword,
						domainCredentials?.DomainName
						);

				if (AreDomainCredentialsValid(domainCredentials, directorySearcher, requireDomainWritePrivilege, out var innerMessage))
				{
					AddToCache(domainCredentials?.DomainName, directorySearcher, requireDomainWritePrivilege);
					return directorySearcher;
				}
				else
				{
					var errorMessage = new ZStringBuilder();
					errorMessage.Append(Res.GetString("1ca84720-2f95-4afa-b880-d3ccf127a1dd", "Could not perform the action on domain {0}.", domainCredentials?.DomainName));
					if (!string.IsNullOrEmpty(innerMessage))
					{
						errorMessage.Append(innerMessage);
					}
					errorMessage.Append(Res.GetString("2e9abf25-b50a-4228-a879-2610a138effe", "Please contact your system administrator."));

					throw new NoDomainPrivilegeException(errorMessage.ToStringWithNewLineBetweenAppends());
				}
			}
			catch (InvalidOUException ex)
			{
				throw new DirectoryServicesException(Res.GetString("3CDA1810-0B8A-41D1-8193-68F49518AD99",
						"The Organizational Units for domain {0} is invalid, please review the setting in Registry items: {1}.",
						domainCredentials?.DomainName,
						((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.DomainCredentialsCollection).LocationMultilingual
					), ex);
			}
		}

		static bool AreDomainCredentialsValid(IDomainCredentials domainCredentials, IDirectorySearcher directorySearcher, bool requireDomainWritePrivilege, out string innnerMessage)
		{
			innnerMessage = null;
			try
			{
				//to verify username and password
				directorySearcher.FindUser(directorySearcher.UserName);
			}
			catch (COMException e)
			{
				innnerMessage = e.Message;
				return false;
			}

			if (requireDomainWritePrivilege)
			{
				if ((!directorySearcher.CurrentUserHasSecurityOnOU(domainCredentials?.UserOrganisationalUnit)) &&
					(!ADAccessRuleChecker.HasRequiredPermissionsToSync(OrganisationalUnitType.User, directorySearcher, domainCredentials?.UserOrganisationalUnit)))
				{
					innnerMessage = GetNoDomainPrivilegesError(domainCredentials);
					return false;
				}

				if (ActiveDirectoryRegistry.Instance.EntitiesToSync == EntitiesToSync.UsersAndGroups)
				{
					if ((!directorySearcher.CurrentUserHasSecurityOnOU(domainCredentials?.GroupOrganisationalUnit)) &&
						(!ADAccessRuleChecker.HasRequiredPermissionsToSync(OrganisationalUnitType.Group, directorySearcher, domainCredentials?.GroupOrganisationalUnit)))
					{
						innnerMessage = GetNoDomainPrivilegesError(domainCredentials);
						return false;
					}
				}
			}
			return true;
		}

		static string GetNoDomainPrivilegesError(IDomainCredentials domainCredentials) =>
			Res.GetString("029529b0-94e9-4047-bfce-ff367de26db2", "Domain user does not have write privileges to the Organizational Units for domain {0} set in the registry '{1}'",
				domainCredentials?.DomainName,
				((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.DomainCredentialsCollection).LocationMultilingual
			);

#if DEBUG
		static readonly Overridable<IDirectorySearcher> directorySearcherOverride = new Overridable<IDirectorySearcher>();

		public static IDirectorySearcher DirectorySearcherOverride_ForTest
		{
			get { return directorySearcherOverride.Value; }
			set { directorySearcherOverride.Value = value; }
		}
#endif

		public static void ClearCache()
		{
			foreach (var domainCredentials in ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value)
			{
				Cache.Remove(GetDomainCacheKey(domainCredentials.DomainName, false));
				Cache.Remove(GetDomainCacheKey(domainCredentials.DomainName, true));
			}
		}

		#region Implementation

		static string GetDomainCacheKey(string domainName, bool requireDomainWritePrivilege)
		{
			var key = requireDomainWritePrivilege ? ReadWriteCacheKey : ReadCacheKey;
			return key + "-" + domainName;
		}

		static IDirectorySearcher GetFromCache(string domainName, bool requireDomainWritePrivilege)
		{
			return GetFromCache(GetDomainCacheKey(domainName, requireDomainWritePrivilege));
		}

		static IDirectorySearcher GetFromCache(string key)
		{
			if (Cache.Contains(key))
			{
				var cachedSearcher = (DirectorySearcherWrapper)Cache[key];
				if (cachedSearcher != null)
				{
					return cachedSearcher;
				}
				else
				{
					Cache.Remove(ReadCacheKey);
				}
			}
			return null;
		}

		static void AddToCache(string domainName, IDirectorySearcher directorySearcher, bool requireDomainWritePrivilege)
		{
			Cache.Add(GetDomainCacheKey(domainName, requireDomainWritePrivilege), directorySearcher, new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(MinutesToCacheSearcher) });
		}

		const string ReadCacheKey = "DirectorySearcherFactory-Read";
		const string ReadWriteCacheKey = "DirectorySearcherFactory-ReadWrite";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		const int MinutesToCacheSearcher = 10;

		static MemoryCache Cache
		{
			get { return MemoryCache.Default; }
		}

		#endregion
	}
}
