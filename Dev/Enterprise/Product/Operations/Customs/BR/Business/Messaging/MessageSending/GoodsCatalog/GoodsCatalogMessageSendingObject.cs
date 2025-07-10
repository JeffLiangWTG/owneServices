using CargoWise.Customs.BR.MessageContracts.ProductCatalog.Outgoing;
using CargoWise.Types;
using Enterprise.Customs.BR.Business.ProductCatalog;

namespace Enterprise.Customs.BR.Business
{
	public class GoodsCatalogMessageSendingObject : BaseSingleMessageSendingObject
	{
		public GoodsCatalogMessageSendingObject(CusGoodsCatalog goodsCatalog) : base(goodsCatalog)
		{
		}

		public CusGoodsCatalog GoodsCatalog => Parent as CusGoodsCatalog;

		public ZString Action { get; set; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			MessageType = EDIMessageSubTypeList.Codes.Original;
		}

		public override ZString GetMessageTypeForEDIMessage() => MessageTypeList.Codes.CAT;

		public override ZString GetMessageText() => new ProductMessageBuilder(new ProductCatalogProvider(this)).GetMessageText();
	}
}
