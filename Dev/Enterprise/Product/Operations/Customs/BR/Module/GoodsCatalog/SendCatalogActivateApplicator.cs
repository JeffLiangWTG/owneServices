using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.BR.Module
{
	public class SendCatalogActivateApplicator : BaseSendCatalogApplicator
	{
		public SendCatalogActivateApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("1D347F75-64D9-47FE-A262-8973B42932A1", "Activate"), factory)
		{
		}

		protected override BaseGoodsCatalogBatchMessageSender CreateMessageSender(IEnumerable<CusGoodsCatalog> goodsCatalogs, IOperationalActionSectionLog log)
		{
			return new GoodsCatalogActivateBatchMessageSender(goodsCatalogs, log);
		}

		protected override string ConfirmationCaption => Res.GetString("BDDC1E07-078F-452D-A0B8-D8E336E72D81", "Activate Confirmation");

		protected override string ConfirmationMessage => Res.GetString("746E8086-CC55-4ECB-8826-E57C7F103F1D", "You are about to send a message to activate all draft Catalogs or those not yet sent to Customs (Customs Status: Draft and Message Status: Not Sent or Customs Status: Draft and Message Status: Not Sent or Accepted). Do you want to proceed?");
	}
}
