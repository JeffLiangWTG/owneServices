using System.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class SortInfo
	{
		public SortInfo(ZString propertyName, ListSortDirection direction)
		{
			this.Direction = direction;
			this.PropertyName = propertyName;
		}

		public override bool Equals(object obj)
		{
			SortInfo rhs = obj as SortInfo;
			if (rhs != null)
			{
				return Direction == rhs.Direction && PropertyName == rhs.PropertyName;
			}
			else
			{
				return base.Equals(obj);
			}
		}

		public override int GetHashCode()
		{
			return PropertyName.GetHashCode();
		}

		public readonly ListSortDirection Direction;
		public readonly ZString PropertyName;
	}
}
