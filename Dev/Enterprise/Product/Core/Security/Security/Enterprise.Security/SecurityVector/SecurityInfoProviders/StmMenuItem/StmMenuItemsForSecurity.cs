using System;
using System.Collections;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Security.Provider
{
	internal class StmMenuItemsForSecurity : ICollection<StmMenuItem>
	{
		//Use wrapper around StmMenuItem, StmMenuItemInfo, to implement desired equality function for duplicate checking.
		//Each entry starts as false, and if a duplicate is found, set to true.
		readonly Dictionary<StmMenuItemInfo, bool> duplicationCheckHashSet = new Dictionary<StmMenuItemInfo, bool>();
		readonly StmMenuItem[] array;

		public StmMenuItemsForSecurity(StmMenuItem[] array)
		{
			this.array = array;
			duplicationCheckHashSet = new Dictionary<StmMenuItemInfo, bool>(array.Length);
			for (int i = 0; i < array.Length; ++i)
			{
				StmMenuItemInfo info = new StmMenuItemInfo(array[i]);
				if (duplicationCheckHashSet.ContainsKey(info))
				{
					duplicationCheckHashSet[info] = true;
				}
				else
				{
					duplicationCheckHashSet[info] = false;
				}
			}
		}

		public int Count
		{
			get
			{
				return array.Length;
			}
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public StmMenuItem GetValue(int key)
		{
			return array[key];
		}

		public bool IsDuplicate(StmMenuItem item)
		{
			return duplicationCheckHashSet[new StmMenuItemInfo(item)];
		}

		public void Add(StmMenuItem item)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(StmMenuItem item)
		{
			return duplicationCheckHashSet.ContainsKey(new StmMenuItemInfo(item));
		}

		public void CopyTo(StmMenuItem[] stmMenuArray, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<StmMenuItem> GetEnumerator()
		{
			foreach (var info in array)
			{
				yield return info;
			}
		}

		public bool Remove(StmMenuItem item)
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		struct StmMenuItemInfo
		{
			public StmMenuItem item;

			public StmMenuItemInfo(StmMenuItem item) { this.item = item; }

			public override bool Equals(object obj)
			{
				var item2 = ((StmMenuItemInfo)obj).item;
				return item.SU_MenuName == item2.SU_MenuName &&
					item.SU_MenuPath == item2.SU_MenuPath &&
					item.SU_BusinessContext == item2.SU_BusinessContext;
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return item.SU_MenuName.GetHashCode()
						+ item.SU_MenuPath.GetHashCode() * 31
						+ item.SU_BusinessContext.GetHashCode() * 86187739;
				}
			}
		}
	}
}
