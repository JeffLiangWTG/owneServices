using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.BR.Module
{
	public class SendCatalogDeactivateApplicator : BaseSendCatalogApplicator
	{
		public SendCatalogDeactivateApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("A7DDB1E2-1E61-4704-9FC0-084EDD4B200E", "Deactivate"), factory)
		{
		}

		protected override BaseGoodsCatalogBatchMessageSender CreateMessageSender(IEnumerable<CusGoodsCatalog> goodsCatalogs, IOperationalActionSectionLog log)
		{
			return new GoodsCatalogDeactivateBatchMessageSender(goodsCatalogs, log);
		}

		protected override string ConfirmationCaption => Res.GetString("C10B3C21-23A8-4215-9955-14C4FE3A1C24", "Deactivate Confirmation");

		protected override string ConfirmationMessage => Res.GetString("F3ADA81E-014B-4DF9-99B6-A7FD26D75E6C", "You are about to send a message to deactivate all active Catalogs (Customs Status: Active and Message Status: Accepted). Do you want to proceed?”");
	}
}
