using System.Collections.Generic;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.BR.Business
{
	public class GoodsCatalogUpdateDraftBatchMessageSender : BaseGoodsCatalogBatchMessageSender
	{
		public GoodsCatalogUpdateDraftBatchMessageSender(IEnumerable<CusGoodsCatalog> goodsCatalogs, IOperationalActionSectionLog log) : base(goodsCatalogs, log)
		{
		}

		protected override bool IsOKToSendMessage(CusGoodsCatalog catalog)
		{
			return catalog.CGC_AuthorityStatus == GoodsCatalogStatusTypeList.Codes.Draft && catalog.CGC_MessageStatus.IsEmpty;
		}

		protected override string SearchingLogMessage => Res.GetString("D4889A56-C482-4134-A276-E69AB1D76AB4", "Searching for catalogs that attend criteria: “Draft Catalogs that have changes pending (Customs Status: Draft and Message Status: NOT).”");

		protected override string SendAction => ActionList.Codes.UpdateDraft;
	}
}
