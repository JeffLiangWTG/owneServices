using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsContainerItemCollection : CusCodeDataCollection<NctsContainerItem>
	{
		public NctsContainerItemCollection(NctsContainer master)
			: base(master, CusCodeDataTypeList.Codes.ItemNumber)
		{
		}
	}
}
