using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class PackingGroup
	{
		public PackingGroup(ZString type, ZInt packCount, ZDecimal itemsCount, ZString marksAndNos, bool isUnpacked)
		{
			Type = type;
			PackCount = packCount;
			ItemsCount = itemsCount;
			MarksAndNos = marksAndNos;
			IsUnpacked = isUnpacked;
		}

		public ZInt PackCount { get; set; }

		public ZDecimal ItemsCount { get; set; }

		public ZString Type { get; set; }

		public ZString MarksAndNos { get; set; }

		public bool IsUnpacked { get; set; }
	}
}
