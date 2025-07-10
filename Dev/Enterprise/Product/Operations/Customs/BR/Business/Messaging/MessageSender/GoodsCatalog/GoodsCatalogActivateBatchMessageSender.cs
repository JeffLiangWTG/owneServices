using System.Collections.Generic;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.BR.Business
{
	public class GoodsCatalogActivateBatchMessageSender : BaseGoodsCatalogBatchMessageSender
	{
		public GoodsCatalogActivateBatchMessageSender(IEnumerable<CusGoodsCatalog> goodsCatalogs, IOperationalActionSectionLog log) : base(goodsCatalogs, log)
		{
		}

		protected override bool IsOKToSendMessage(CusGoodsCatalog catalog)
		{
			return (catalog.IsMessageAccepted || catalog.CGC_MessageStatus.IsEmpty) && (catalog.CGC_AuthorityStatus == GoodsCatalogStatusTypeList.Codes.Draft || catalog.CGC_AuthorityStatus.IsEmpty);
		}

		protected override string SearchingLogMessage => Res.GetString("12577667-B51C-456E-8824-B7C3E32E6E11", "Searching for catalogs that attend criteria: “Draft Catalogs or those not yet sent to Customs (Customs Status: Draft or empty and Message Status: ACC or NOT).”");

		protected override string SendAction => ActionList.Codes.Activate;
	}
}
