using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.Web
{
	public abstract class AccessRulesBase
	{
		public abstract string[] GetRoles();
		protected string[] roles;

		public abstract string[] GetCaptions();
		protected string[] captions;

		public abstract KeyValuePair<string, string>[] GetDefaultTicked();

		public OrgsRoleAccessCollection GetDefaultCollection()
		{
			OrgsRoleAccessCollection result = new OrgsRoleAccessCollection();

			foreach (string role in GetRoles())
			{
				OrgsRoleAccess member = new OrgsRoleAccess(role, GetCaptions());

				foreach (string caption in GetCaptions())
				{
					string property = GetPropertyName(caption);
					member[property] = GetDefault(role, caption);
				}

				result.Add(member);
			}

			return result;
		}

		public string GetPropertyName(string caption)
		{
			for (int i = 0; i < GetCaptions().Length; i++)
			{
				if (caption == GetCaptions()[i])
				{
					return string.Format((NoResString)"Property{0}", i + 1);
				}
			}
			return null;
		}

		ZBool GetDefault(string role, string caption)
		{
			foreach (KeyValuePair<string, string> pair in GetDefaultTicked())
			{
				if (pair.Key == role && pair.Value == caption)
				{
					return true;
				}
			}
			return false;
		}
	}
}
