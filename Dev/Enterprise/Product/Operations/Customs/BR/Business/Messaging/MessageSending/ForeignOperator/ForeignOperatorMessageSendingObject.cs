using CargoWise.Customs.BR.MessageContracts.ProductCatalog.Outgoing;
using CargoWise.Types;
using Enterprise.Customs.BR.Business.ProductCatalog;

namespace Enterprise.Customs.BR.Business
{
	public class ForeignOperatorMessageSendingObject : BaseSingleMessageSendingObject, IMessageSendingObject, IMessageSendingObjectParent
	{
		public ForeignOperatorMessageSendingObject(CusBRForeignOperator foreignOperator) : base(foreignOperator)
		{
		}

		public CusBRForeignOperator ForeignOperator => (CusBRForeignOperator)Parent;

		public ZString Action { get; set; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			MessageType = ForeignOperatorMessageTypesList.Codes.ORI;
		}

		public override ZString GetMessageTypeForEDIMessage() => MessageTypeList.Codes.OPE;

		public override ZString GetMessageText() => new ForeignOperatorMessageBuilder(new ForeignOperatorProvider(this)).GetMessageText();
	}
}
