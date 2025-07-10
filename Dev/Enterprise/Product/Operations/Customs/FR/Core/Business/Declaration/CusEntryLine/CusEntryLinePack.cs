using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusEntryLinePack
	{
		public CusEntryLinePack(ZString type, ZInt count, ZDecimal itemsCount, ZString marksAndNos, bool isUnpacked)
		{
			Type = type;
			Count = count;
			ItemsCount = itemsCount;
			MarksAndNos = marksAndNos;
			IsUnpacked = isUnpacked;
		}

		public ZInt Count { get; set; }

		public ZDecimal ItemsCount { get; set; }

		public ZString Type { get; set; }

		public ZString MarksAndNos { get; set; }

		public bool IsUnpacked { get; set; }
	}
}
