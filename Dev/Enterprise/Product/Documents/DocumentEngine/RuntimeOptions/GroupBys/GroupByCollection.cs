using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	[Serializable]
	public class GroupByCollection : NonPersistentBusinessObject, IEnumerable<IBindableBooleanItem>, IObsoleteValidation
	{
		readonly List<GroupBy> GroupBys = new List<GroupBy>();

		IEnumerator<IBindableBooleanItem> IEnumerable<IBindableBooleanItem>.GetEnumerator()
		{
			foreach (GroupBy element in GroupBys)
			{
				yield return element;
			}
		}

		public GroupBy SelectedGroupBy
		{
			get
			{
				foreach (GroupBy s in this)
				{
					if (s.Selected)
					{
						return s;
					}
				}

				return new GroupBy("", null);
			}

			set
			{
				for (int i = 0; i < Count; i++)
				{
					this[i].Selected = this[i] == value;
				}
			}
		}

		public GroupBy Add(string displayName, string fieldList)
		{
			return Add(new GroupBy(displayName, fieldList));
		}

		public GroupBy Add(GroupBy groupby)
		{
			RegisterEditableChildObject(groupby);
			GroupBys.Add(groupby);
			return groupby;
		}

		public int Count
		{
			get
			{
				return GroupBys.Count;
			}
		}

		public GroupBy this[int index]
		{
			get
			{
				return GroupBys[index];
			}
			internal set
			{
				UnRegisterEditableChildObject(GroupBys[index]);
				GroupBys[index] = value;
				RegisterEditableChildObject(GroupBys[index]);
			}
		}

		public void Clear()
		{
			foreach (GroupBy groupBy in GroupBys)
			{
				UnRegisterEditableChildObject(groupBy);
			}
			GroupBys.Clear();
		}

		public new GroupBy this[string displayName]
		{
			get
			{
				foreach (GroupBy groupBy in this)
				{
					if (groupBy.DisplayName == displayName)
					{
						return groupBy;
					}
				}
				return null;
			}
		}

		public ZBool BreakPageOverride
		{
			get { return breakPageOverride; }
			set
			{
				breakPageOverride = value;
				BreakPageOverrideInfo.RefreshBinding();
			}
		}
		bool breakPageOverride;

		public ZPropertyInfo BreakPageOverrideInfo
		{
			get { return GetZPropertyInfo(nameof(BreakPageOverride)); }
		}

		public GroupBy DefaultGroupBy { get; private set; }

		public void UpdateDefaultGroupBy()
		{
			foreach (GroupBy groupBy in this)
			{
				if (groupBy.Selected)
				{
					DefaultGroupBy = groupBy;
					break;
				}
			}
		}
	}
}
