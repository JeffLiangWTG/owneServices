using System.Collections;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;

namespace Enterprise.Core.Forms
{
	/// <summary>
	/// Collection for ZGridColumnGroup.
	/// </summary>
	public class ZGridColumnGroupCollection : IEnumerable
	{
		public ZGridColumnGroupCollection(ZGridColumns columns)
		{
			Groups = new ArrayList();
			foreach (var column in columns)
			{
				Add(column);
			}
		}

		public ZGridColumnGroup this[ResourceStringData groupName]
		{
			get
			{
				ZGridColumnGroup group = null;
				foreach (ZGridColumnGroup currentGroup in Groups)
				{
					if (currentGroup.GroupName.Equals(groupName))
					{
						group = currentGroup;
						break;
					}
				}

				return group;
			}
		}

		public void Add(ZGridColumn column)
		{
			var isExistingGroup = false;

			foreach (ZGridColumnGroup currentGroup in Groups)
			{
				if (currentGroup.GroupName.Equals(column.GroupName) && !column.GroupName.IsEmpty())
				{
					currentGroup.Add(column);
					isExistingGroup = true;
					break;
				}
			}

			if (!isExistingGroup)
			{
				var group = new ZGridColumnGroup(column);
				Groups.Add(group);
			}
		}

		protected ArrayList Groups;

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Groups.GetEnumerator();
		}

		#endregion
	}
}
