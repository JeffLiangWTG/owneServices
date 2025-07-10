using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	sealed class ShipmentChargeDataComparer : IComparer<ShipmentChargeData>
	{
		public static IReadOnlyList<ShipmentChargeData> SortChargesByTypeCode(IReadOnlyList<ShipmentChargeData> chargesData)
		{
			var sortedList = chargesData.ToList();
			sortedList.Sort(new ShipmentChargeDataComparer());
			return sortedList.AsReadOnly();
		}

		public int Compare(ShipmentChargeData x, ShipmentChargeData y)
		{
			return x.TypeCode.CompareTo(y.TypeCode);
		}
	}
}
