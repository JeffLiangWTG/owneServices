using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using CargoWise.ActiveDirectory;
using static Enterprise.Security.ActiveDirectory.DomainCredentialsValidation;

namespace Enterprise.Security.ActiveDirectory
{
	public static class ADAccessRuleChecker
	{
		public static bool HasRequiredPermissionsToSync(OrganisationalUnitType ouType, IDirectorySearcher directorySearcher, string ouPath)
		{
			var rules = GetADAccessRulesOfOU(directorySearcher, ouPath);
			return HasFullControlOnOU(rules) || (CanCreateChildObjects(ouType, rules) && HasFullControlOnDescendentObjects(ouType, rules));
		}

		static IEnumerable<ActiveDirectoryAccessRule> GetADAccessRulesOfOU(IDirectorySearcher directorySearcher, string ouPath)
		{
			var user = directorySearcher.FindUser(directorySearcher.UserName);
			if (user != null)
			{
				var allSid = GetUserAllRelatedSid(user);

				var ou = directorySearcher.FindOrganisationalUnit(ouPath);
				var ouDE = (ou.GetDirectoryEntry() as DirectoryEntryWrapper).DirectoryEntry;
				var ouSecurity = ouDE.ObjectSecurity;
				var rules = ouSecurity.GetAccessRules(includeExplicit: true, includeInherited: true, typeof(SecurityIdentifier));

				return rules.Cast<ActiveDirectoryAccessRule>().Where(r => allSid.Any(s => r.IdentityReference.Equals(s)));
			}
			return Enumerable.Empty<ActiveDirectoryAccessRule>();
		}

		static List<SecurityIdentifier> GetUserAllRelatedSid(IUserDirectoryEntry user)
		{
			var allSid = new List<SecurityIdentifier>
			{
				new SecurityIdentifier((byte[])user["objectSid"], 0)
			};

			user.RefreshCache(new string[] { "tokenGroups", "sIDHistory" });
			var directoryEntry = (user.GetDirectoryEntry() as DirectoryEntryWrapper).DirectoryEntry;
			if (directoryEntry.Properties["sIDHistory"] != null && directoryEntry.Properties["sIDHistory"] is PropertyValueCollection historySid)
			{
				foreach (byte[] sid in historySid)
				{
					allSid.Add(new SecurityIdentifier(sid, 0));
				}
			}

			if (directoryEntry.Properties["tokenGroups"] != null && directoryEntry.Properties["tokenGroups"] is PropertyValueCollection groupSid)
			{
				foreach (byte[] sid in groupSid)
				{
					allSid.Add(new SecurityIdentifier(sid, 0));
				}
			}

			return allSid;
		}

		static bool IsDeniedFullControlOnOU_Explicit(IEnumerable<ActiveDirectoryAccessRule> rules) => HasAccessControlTypeInFullControlOnOU(rules, AccessControlType.Deny, false);
		static bool IsDeniedFullControlOnOU_Inherited(IEnumerable<ActiveDirectoryAccessRule> rules) => HasAccessControlTypeInFullControlOnOU(rules, AccessControlType.Deny, true);
		static bool IsAllowedFullControlOnOU_Explicit(IEnumerable<ActiveDirectoryAccessRule> rules) => HasAccessControlTypeInFullControlOnOU(rules, AccessControlType.Allow, false);
		static bool IsAllowedFullControlOnOU_Inherited(IEnumerable<ActiveDirectoryAccessRule> rules) => HasAccessControlTypeInFullControlOnOU(rules, AccessControlType.Allow, true);
		static bool HasAccessControlTypeInFullControlOnOU(IEnumerable<ActiveDirectoryAccessRule> rules, AccessControlType accessControlType, bool isInherited) =>
			rules.Any(r =>
				r.ObjectType.Equals(Guid.Empty) &&
				r.AccessControlType == accessControlType &&
				r.ActiveDirectoryRights.HasFlag(ActiveDirectoryRights.GenericAll) &&
				r.InheritanceType.HasFlag(ActiveDirectorySecurityInheritance.All) &&
				r.IsInherited == isInherited);

		static bool IsDeniedFullControlOnDescendentObjects_Explicit(OrganisationalUnitType objectType, IEnumerable<ActiveDirectoryAccessRule> rules) => HasAccessControlTypeInFullControlOnDescendentObjects(objectType, rules, AccessControlType.Deny, false);
		static bool IsDeniedFullControlOnDescendentObjects_Inherited(OrganisationalUnitType objectType, IEnumerable<ActiveDirectoryAccessRule> rules) => HasAccessControlTypeInFullControlOnDescendentObjects(objectType, rules, AccessControlType.Deny, true);
		static bool IsAllowedFullControlOnDescendentObjects_Explicit(OrganisationalUnitType objectType, IEnumerable<ActiveDirectoryAccessRule> rules) => HasAccessControlTypeInFullControlOnDescendentObjects(objectType, rules, AccessControlType.Allow, false);
		static bool IsAllowedFullControlOnDescendentObjects_Inherited(OrganisationalUnitType objectType, IEnumerable<ActiveDirectoryAccessRule> rules) => HasAccessControlTypeInFullControlOnDescendentObjects(objectType, rules, AccessControlType.Allow, true);
		static bool HasAccessControlTypeInFullControlOnDescendentObjects(OrganisationalUnitType objectType, IEnumerable<ActiveDirectoryAccessRule> rules, AccessControlType accessControlType, bool isInherited) =>
			rules.Any(r =>
				r.AccessControlType == accessControlType &&
				r.ActiveDirectoryRights.HasFlag(ActiveDirectoryRights.GenericAll) &&
				r.InheritedObjectType.Equals(GetObjectSchemaIdGuid(objectType)) &&
				r.InheritanceType.HasFlag(ActiveDirectorySecurityInheritance.Descendents) &&
				r.IsInherited == isInherited);

		static bool IsDeniedCreateChildObjects_Explicit(OrganisationalUnitType objectType, IEnumerable<ActiveDirectoryAccessRule> rules) => HasAccessControlTypeCreateChildObjects(objectType, rules, AccessControlType.Deny, false);
		static bool IsDeniedCreateChildObjects_Inherited(OrganisationalUnitType objectType, IEnumerable<ActiveDirectoryAccessRule> rules) => HasAccessControlTypeCreateChildObjects(objectType, rules, AccessControlType.Deny, true);
		static bool IsAllowedCreateChildObjects_Explicit(OrganisationalUnitType objectType, IEnumerable<ActiveDirectoryAccessRule> rules) => HasAccessControlTypeCreateChildObjects(objectType, rules, AccessControlType.Allow, false);
		static bool IsAllowedCreateChildObjects_Inherited(OrganisationalUnitType objectType, IEnumerable<ActiveDirectoryAccessRule> rules) => HasAccessControlTypeCreateChildObjects(objectType, rules, AccessControlType.Allow, true);
		static bool HasAccessControlTypeCreateChildObjects(OrganisationalUnitType objectType, IEnumerable<ActiveDirectoryAccessRule> rules, AccessControlType accessControlType, bool isInherited) =>
			rules.Any(r =>
				r.ObjectType.Equals(GetObjectSchemaIdGuid(objectType)) &&
				r.AccessControlType == accessControlType &&
				r.ActiveDirectoryRights.HasFlag(ActiveDirectoryRights.CreateChild) &&
				r.InheritanceType.HasFlag(ActiveDirectorySecurityInheritance.All) &&
				r.IsInherited == isInherited);

		static bool HasFullControlOnOU(IEnumerable<ActiveDirectoryAccessRule> rules)
		{
			if (IsDeniedFullControlOnOU_Explicit(rules))
			{
				return false;
			}

			if (IsAllowedFullControlOnOU_Explicit(rules))
			{
				return true;
			}

			if (IsDeniedFullControlOnOU_Inherited(rules))
			{
				return false;
			}

			if (IsAllowedFullControlOnOU_Inherited(rules))
			{
				return true;
			}

			return false;
		}

		static bool CanCreateChildObjects(OrganisationalUnitType objectType, IEnumerable<ActiveDirectoryAccessRule> rules)
		{
			if (IsDeniedCreateChildObjects_Explicit(objectType, rules))
			{
				return false;
			}

			if (IsAllowedCreateChildObjects_Explicit(objectType, rules))
			{
				return true;
			}

			if (IsDeniedCreateChildObjects_Inherited(objectType, rules))
			{
				return false;
			}

			if (IsAllowedCreateChildObjects_Inherited(objectType, rules))
			{
				return true;
			}

			return false;
		}

		static bool HasFullControlOnDescendentObjects(OrganisationalUnitType objectType, IEnumerable<ActiveDirectoryAccessRule> rules)
		{
			if (IsDeniedFullControlOnDescendentObjects_Explicit(objectType, rules))
			{
				return false;
			}

			if (IsAllowedFullControlOnDescendentObjects_Explicit(objectType, rules))
			{
				return true;
			}

			if (IsDeniedFullControlOnDescendentObjects_Inherited(objectType, rules))
			{
				return false;
			}

			if (IsAllowedFullControlOnDescendentObjects_Inherited(objectType, rules))
			{
				return true;
			}

			return false;
		}

		// https://learn.microsoft.com/en-us/windows/win32/adschema/c-user
		// https://learn.microsoft.com/en-us/windows/win32/adschema/c-group
		static Guid GetObjectSchemaIdGuid(OrganisationalUnitType objectType) =>
			objectType == OrganisationalUnitType.User ? new Guid("bf967aba-0de6-11d0-a285-00aa003049e2") : new Guid("bf967a9c-0de6-11d0-a285-00aa003049e2");
	}
}
