using System;
using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	public class BusinessObjectComparer : IEqualityComparer<BusinessObject>
	{
		#region IEqualityComparer

		public bool Equals(BusinessObject x, BusinessObject y)
		{
			return x.EqualsBase(y);
		}

		public int GetHashCode(BusinessObject obj)
		{
			return obj.GetHashCodeBase();
		}

		#endregion

		#region Instance

		public static BusinessObjectComparer Instance
		{
			get { return instance ?? (instance = new BusinessObjectComparer()); }
		}
		[ThreadStatic]
		static BusinessObjectComparer instance;

		#endregion
	}
}
