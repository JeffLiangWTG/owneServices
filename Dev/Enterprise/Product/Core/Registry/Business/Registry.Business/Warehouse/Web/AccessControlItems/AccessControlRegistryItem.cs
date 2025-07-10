using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Web
{
	public class AccessControlRegistryItem : StronglyTypedRegistryItem<OrgsRoleAccessCollection>
	{
		public AccessControlRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, AccessRulesBase accessRules, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new AccessControlRegistryDataType(accessRules), RegistryStorageFlags.System, options, accessRules.GetDefaultCollection()))
		{
			this.accessRules = accessRules;
		}

		internal readonly AccessRulesBase accessRules;

		public string[] RolesWithAccess()
		{
			List<string> result = new List<string>();

			foreach (OrgsRoleAccess roleAccess in Value)
			{
				for (int i = 1; i <= roleAccess.LastUsedPropertyNum; i++)
				{
					string propertyName = string.Format((NoResString)"Property{0}", i);
					if ((ZBool)roleAccess[propertyName])
					{
						result.Add(roleAccess.Role);
						break;
					}
				}
			}

			return result.ToArray();
		}

		public const string EverythingIsSuppressed = "*EverythingIsSuppressed*";

		public string[] CaptionsToHide(IAccessControlled bizO)
		{
			List<string> result = new List<string>();

			if (bizO is BusinessObject && ((BusinessObject)bizO).IsInDatabase)
			{
				List<OrgsRoleAccess> loggedInOrgsRoles = GetLoggedInOrgsRoles(bizO);

				bool atLeastOneIsVisible = false;

				foreach (string caption in accessRules.GetCaptions())
				{
					if (IsCaptionSuppressed(loggedInOrgsRoles, caption))
					{
						result.Add(caption);
					}
					else
					{
						atLeastOneIsVisible = true;
					}
				}

				if (!atLeastOneIsVisible)
				{
					result.Insert(0, EverythingIsSuppressed);
				}
			}

			return result.ToArray();
		}

		public bool TextNeedsToBeSuppressed(IAccessControlled bizO, string boundProperty)
		{
			if (bizO.IsInTextSuppressionMode && bizO is BusinessObject && ((BusinessObject)bizO).IsInDatabase)
			{
				string caption = bizO.GetRegistryCaption(boundProperty);

				if (string.IsNullOrEmpty(caption))
				{
					if (boundProperty.Contains(".") || boundProperty.Contains("+"))
					{
						string firstExpressionPart = boundProperty.Substring(0, boundProperty.IndexOfAny(new[] { '.', '+' }));
						caption = bizO.GetRegistryCaption(firstExpressionPart);
					}
				}

				bool contains = false;
				foreach (string group in accessRules.GetCaptions())
				{
					if (group == caption)
					{
						contains = true;
						break;
					}
				}

				return contains && IsCaptionSuppressed(GetLoggedInOrgsRoles(bizO), caption);
			}

			return false;
		}

		bool IsCaptionSuppressed(List<OrgsRoleAccess> loggedInOrgsRoles, string captionToCheck)
		{
			foreach (OrgsRoleAccess roleAccess in loggedInOrgsRoles)
			{
				string propertyName = accessRules.GetPropertyName(captionToCheck);
				if ((ZBool)roleAccess[propertyName])
				{
					return false;
				}
			}

			return true;
		}

		List<OrgsRoleAccess> GetLoggedInOrgsRoles(IAccessControlled bizO)
		{
			List<OrgsRoleAccess> result = new List<OrgsRoleAccess>();
			foreach (OrgsRoleAccess roleAccess in Value)
			{
				if (bizO.LoggedInOrgIs(roleAccess.Role))
				{
					result.Add(roleAccess);
				}
			}
			return result;
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.AccessControlRegistryItemEditor, Enterprise.Registry.GUI")]
	public class AccessControlRegistryDataType : NonPersistentBusinessObjectRegistryDataType<OrgsRoleAccessCollection>
	{
		public AccessControlRegistryDataType(AccessRulesBase accessRules)
			: base(new OrgsRoleAccessCollection())
		{
			this.accessRules = accessRules;
		}

		public AccessRulesBase AccessRules
		{
			get { return accessRules; }
		}
		readonly AccessRulesBase accessRules;
	}
}
