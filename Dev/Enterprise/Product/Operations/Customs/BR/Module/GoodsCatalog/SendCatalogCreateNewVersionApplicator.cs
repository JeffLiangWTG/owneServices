using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.BR.Module
{
	public class SendCatalogCreateNewVersionApplicator : BaseSendCatalogApplicator
	{
		public SendCatalogCreateNewVersionApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("89F7F496-57C7-44D9-9DF9-83B26BB126B1", "Create New Version"), factory)
		{
		}

		protected override BaseGoodsCatalogBatchMessageSender CreateMessageSender(IEnumerable<CusGoodsCatalog> goodsCatalogs, IOperationalActionSectionLog log)
		{
			return new GoodsCatalogCreateNewVersionBatchMessageSender(goodsCatalogs, log);
		}

		protected override string ConfirmationCaption => Res.GetString("95A0CD25-064D-44AB-9875-1A6CCB2C093D", "Create New Version Confirmation");

		protected override string ConfirmationMessage => Res.GetString("980A6FF7-CE32-4802-BC2A-AE144CAE17BA", "You are about to send a message to create a new version for all active Catalogs (Customs Status: Active and Message Status: Not Sent). Do you want to proceed?”");
	}
}
