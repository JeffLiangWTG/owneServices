using System.Collections.Generic;

namespace Enterprise.Customs.BR.Business
{
	public class ImportChargeCodeComparer : IComparer<string>
	{
		public int Compare(string x, string y)
		{
			var xGroupOrder = GetGroupOrder(x);
			var yGroupOrder = GetGroupOrder(y);
			return xGroupOrder == yGroupOrder ? x.CompareTo(y) : xGroupOrder.CompareTo(yGroupOrder);
		}

		int GetGroupOrder(string code) => ImportChargesProvider.IsDeductions(code) ? 2 : ImportChargesProvider.IsAdditions(code) ? 1 : 0;
	}
}
