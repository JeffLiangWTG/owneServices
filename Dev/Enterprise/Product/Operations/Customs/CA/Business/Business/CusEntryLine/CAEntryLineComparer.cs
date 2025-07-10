using System.Collections;
using System.Collections.Generic;

namespace Enterprise.Customs.CA.Business
{
	public class CAEntryLineComparer : IComparer<CusEntryLine>, IComparer
	{
		public CAEntryLineComparer()
			: this(false) { }

		public CAEntryLineComparer(bool invert)
		{
			this.invert = invert;
		}
		readonly bool invert;

		#region IComparer<CusEntryLine>

		public int Compare(CusEntryLine line1, CusEntryLine line2)
		{
			var result = 0;
			result = line1.CL_GoodsShipmentSequence.CompareTo(line2.CL_GoodsShipmentSequence);
			if (result == 0)
			{
				result = line1.CL_CommoditySequence.CompareTo(line2.CL_CommoditySequence);
			}
			return invert ? -result : result;
		}

		#endregion

		#region IComparer

		public int Compare(object x, object y)
		{
			return Compare((CusEntryLine)x, (CusEntryLine)y);
		}

		#endregion
	}
}
