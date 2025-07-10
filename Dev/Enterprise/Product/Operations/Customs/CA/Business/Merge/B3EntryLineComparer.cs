using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	class B3EntryLineComparer : IComparer<Customs.Business.CusEntryLine>
	{
		public bool CompareGoodsShipmentSequenceAndCommoditySequence = true;

		public int Compare(Customs.Business.CusEntryLine x, Customs.Business.CusEntryLine y)
		{
			return CompareCore((CusEntryLine)x, (CusEntryLine)y);
		}

		int CompareCore(CusEntryLine x, CusEntryLine y)
		{
			var result = 0;
			if (CompareGoodsShipmentSequenceAndCommoditySequence)
			{
				var cadSequenceComparer = new SequenceComparer();
				result = cadSequenceComparer.Compare(x.CL_GoodsShipmentSequence, y.CL_GoodsShipmentSequence);
				if (result == 0)
				{
					result = cadSequenceComparer.Compare(x.CL_CommoditySequence, y.CL_CommoditySequence);
				}
			}
			if (result == 0)
			{
				if (x.Declaration.IsLVS || x.Declaration.IsIM2 || !x.Declaration.IsCADEnabled)
				{
					result = x.RandomLine.CA_B3SubHeaderNumber.CompareTo(y.RandomLine.CA_B3SubHeaderNumber);
				}
				else
				{
					result = x.RandomLine.InvoiceHeaderSequence.CompareTo(y.RandomLine.InvoiceHeaderSequence);
				}
			}
			if (result == 0)
			{
				result = x.RandomLine.B3SubHeaderNumberForLVX.CompareTo(y.RandomLine.B3SubHeaderNumberForLVX);
			}
			if (result == 0)
			{
				result = x.RandomLine.JI_LineNo.CompareTo(y.RandomLine.JI_LineNo);
			}
			return result;
		}
	}

	class SequenceComparer : IComparer<ZShort>
	{
		public int Compare(ZShort x, ZShort y)
		{
			if (x == ZShort.Zero && y > ZShort.Zero)
			{
				return 1;
			}
			else if (x > ZShort.Zero && y == ZShort.Zero)
			{
				return -1;
			}
			else
			{
				return x.CompareTo(y);
			}
		}
	}
}
