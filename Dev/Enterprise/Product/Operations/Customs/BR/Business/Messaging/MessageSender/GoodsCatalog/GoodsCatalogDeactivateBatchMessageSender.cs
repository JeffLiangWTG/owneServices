using System.Collections.Generic;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.BR.Business
{
	public class GoodsCatalogDeactivateBatchMessageSender : BaseGoodsCatalogBatchMessageSender
	{
		public GoodsCatalogDeactivateBatchMessageSender(IEnumerable<CusGoodsCatalog> goodsCatalogs, IOperationalActionSectionLog log) : base(goodsCatalogs, log)
		{
		}

		protected override bool IsOKToSendMessage(CusGoodsCatalog catalog)
		{
			return catalog.IsMessageAccepted && catalog.CGC_AuthorityStatus == GoodsCatalogStatusTypeList.Codes.Active;
		}

		protected override string SearchingLogMessage => Res.GetString("CB1F0D88-54B4-4CC4-BCFB-0F9B256B87C0", "Searching for catalogs that attend criteria: “Active Catalogs (Customs Status: Active and Message Status: ACC).”");

		protected override string SendAction => ActionList.Codes.Deactivate;
	}
}
