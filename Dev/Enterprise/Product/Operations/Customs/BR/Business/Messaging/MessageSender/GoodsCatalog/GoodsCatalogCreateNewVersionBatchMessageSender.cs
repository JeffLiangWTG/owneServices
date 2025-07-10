using System.Collections.Generic;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.BR.Business
{
	public class GoodsCatalogCreateNewVersionBatchMessageSender : BaseGoodsCatalogBatchMessageSender
	{
		public GoodsCatalogCreateNewVersionBatchMessageSender(IEnumerable<CusGoodsCatalog> goodsCatalogs, IOperationalActionSectionLog log) : base(goodsCatalogs, log)
		{
		}

		protected override bool IsOKToSendMessage(CusGoodsCatalog catalog)
		{
			return catalog.CGC_AuthorityStatus == GoodsCatalogStatusTypeList.Codes.Active && catalog.CGC_MessageStatus.IsEmpty;
		}

		protected override string SearchingLogMessage => Res.GetString("26C6F5A8-19FD-464B-922D-FD5AAB102FE9", "Searching for catalogs that attend criteria: “Active Catalogs (Customs Status: Active and Message Status: NOT).”");

		protected override string SendAction => ActionList.Codes.CreateNewVersion;
	}
}
