using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.BR.Module
{
	public class SendCatalogUpdateDraftApplicator : BaseSendCatalogApplicator
	{
		public SendCatalogUpdateDraftApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("5711F8B2-B1EB-4024-BFC7-CD357C57BABA", "Update Draft"), factory)
		{
		}

		protected override BaseGoodsCatalogBatchMessageSender CreateMessageSender(IEnumerable<CusGoodsCatalog> goodsCatalogs, IOperationalActionSectionLog log)
		{
			return new GoodsCatalogUpdateDraftBatchMessageSender(goodsCatalogs, log);
		}

		protected override string ConfirmationCaption => Res.GetString("0E563E78-7937-49DB-9C87-C83143A9E885", "Update Draft Confirmation");

		protected override string ConfirmationMessage => Res.GetString("6FA59677-B4E8-4BC7-BEDD-EC0B6B86C060", "You are about to update all draft Catalogs that have changes pending (Customs Status: Draft and Message Status: Not Sent). Do you want to proceed?");
	}
}
