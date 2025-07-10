using Enterprise.Customs.DE.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.GUI
{
	public class Phase5MessagingMenuProvider : EU.NCTS.GUI.Phase5MessagingMenuProvider
	{
		public Phase5MessagingMenuProvider(NctsHeader header)
			: base(header)
		{
		}

		protected override EU.NCTS.Business.NctsHeaderGenerator GetNctsHeaderGenerator() => new NctsHeaderGenerator();

		protected override EU.NCTS.Business.NctsInventorySelectionHeader GetInventorySelectionHeader(EU.NCTS.Business.NctsBill bill) => new NctsInventorySelectionHeader((NctsBill)bill);
	}
}
