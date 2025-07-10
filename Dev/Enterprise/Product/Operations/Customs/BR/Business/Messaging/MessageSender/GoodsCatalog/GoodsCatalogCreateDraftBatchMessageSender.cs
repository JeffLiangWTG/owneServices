using System.Collections.Generic;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.BR.Business
{
	public class GoodsCatalogCreateDraftBatchMessageSender : BaseGoodsCatalogBatchMessageSender
	{
		public GoodsCatalogCreateDraftBatchMessageSender(IEnumerable<CusGoodsCatalog> goodsCatalogs, IOperationalActionSectionLog log) : base(goodsCatalogs, log)
		{
		}

		protected override bool IsOKToSendMessage(CusGoodsCatalog catalog)
		{
			return catalog.CGC_AuthorityStatus.IsEmpty && catalog.CGC_MessageStatus.IsEmpty;
		}

		protected override string SearchingLogMessage => Res.GetString("9F019111-0701-4712-B4F0-2A786E4545F0", "Searching for catalogs that attend criteria: “Catalogs that have not yet been sent to Customs (Customs Status: empty and Message Status: NOT).”");

		protected override string SendAction => ActionList.Codes.CreateDraft;
	}
}
