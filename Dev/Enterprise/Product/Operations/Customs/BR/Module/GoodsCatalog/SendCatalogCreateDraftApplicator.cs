using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.BR.Module
{
	public class SendCatalogCreateDraftApplicator : BaseSendCatalogApplicator
	{
		public SendCatalogCreateDraftApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("E2E55B9E-F8E1-4750-830F-A809D3E32F9F", "Create Draft"), factory)
		{
		}

		protected override BaseGoodsCatalogBatchMessageSender CreateMessageSender(IEnumerable<CusGoodsCatalog> goodsCatalogs, IOperationalActionSectionLog log)
		{
			return new GoodsCatalogCreateDraftBatchMessageSender(goodsCatalogs, log);
		}

		protected override string ConfirmationCaption => Res.GetString("D3A32296-424C-4493-9E66-B194C2B5997B", "Create Draft Confirmation");

		protected override string ConfirmationMessage => Res.GetString("83EBF023-A187-4FBF-87B2-795027A867E8", "You are about to send a message to create a draft for all Catalogs that have not yet been sent to Customs (Customs Status: empty and Message Status: Not Sent). Do you want to proceed?");
	}
}
