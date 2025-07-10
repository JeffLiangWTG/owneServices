using System;
using System.Collections;

namespace Enterprise.Registry.Business
{
	/// <summary>
	/// Used to help store values for RegistryItemTag.
	/// </summary>
	public class RegistryItemTagHashtables
	{
		protected Hashtable IsChangedHashtable;
		protected Hashtable IsInErrorHashtable;

		public RegistryItemTagHashtables()
		{
			IsChangedHashtable = new Hashtable();
			IsInErrorHashtable = new Hashtable();
		}

		#region IsChanged

		public void SetIsChanged(Guid companyPK, Guid branchPK, Guid departmentPK, bool value)
		{
			string key = GetKey(companyPK, branchPK, departmentPK);

			if (IsChangedHashtable.Contains(key))
			{
				IsChangedHashtable[key] = value;
			}
			else
			{
				IsChangedHashtable.Add(key, value);
			}
		}

		public bool GetIsChanged(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			string key = GetKey(companyPK, branchPK, departmentPK);
			bool result = false;

			if (IsChangedHashtable.Contains(key))
			{
				result = (bool)IsChangedHashtable[key];
			}
			return result;
		}

		public ICollection GetIsChangedKeys()
		{
			return IsChangedHashtable.Keys;
		}

		public void ClearIsChangedHashtable()
		{
			IsChangedHashtable.Clear();
		}

		#endregion

		#region IsInError

		public void SetIsInError(Guid companyPK, Guid branchPK, Guid departmentPK, bool value)
		{
			string key = GetKey(companyPK, branchPK, departmentPK);

			if (IsInErrorHashtable.Contains(key))
			{
				IsInErrorHashtable[key] = value;
			}
			else
			{
				IsInErrorHashtable.Add(key, value);
			}
		}

		public bool GetIsInError(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			string key = GetKey(companyPK, branchPK, departmentPK);
			bool result = false;

			if (IsInErrorHashtable.Contains(key))
			{
				result = (bool)IsInErrorHashtable[key];
			}
			return result;
		}

		public bool HasAnyError()
		{
			bool result = false;
			foreach (DictionaryEntry isInError in IsInErrorHashtable)
			{
				if ((bool)isInError.Value)
				{
					result = true;
				}
			}
			return result;
		}

		public void ClearIsInErrorHashtable()
		{
			IsInErrorHashtable.Clear();
		}

		#endregion

		#region Implementation

		protected string GetKey(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return (companyPK.ToString() + "+" + branchPK.ToString() + "+" + departmentPK.ToString());
		}

		#endregion
	}
}
