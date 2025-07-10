using CargoWise.Customs.BR.MessageContracts.ImportSiscomex.Outgoing;
using CargoWise.Types;
using Enterprise.Customs.BR.Business.ImportSiscomex;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public partial class ImportSiscomexMessageSendingObject : DeclarationMessageSendingObject
	{
		public ImportSiscomexMessageSendingObject(CusEntryHeader header) : base(header)
		{
		}

		public override ZString GetMessageOwner() => ZString.Empty;

		public override ZString GetMessageTypeForEDIMessage() => MessageTypeList.Codes.CDI;

		protected override ZString GetDefaultMessageType() => ImportSiscomexActionCodeList.Codes.ANA;

		protected override bool MessageType_ReadOnly => false;

		public override CodeDescriptionPairList MessageTypesList => Factory.GetCachedValue<ImportSiscomexActionCodeList>();

		public override ZString GetMessageText() => new ImportSiscomexMessageBuilder(new ImportSiscomexProvider(this)).GetMessageText();

		#region SetDefaultValuesFromEntry

		protected override void SetMessageSendingObjectDefaultValues()
		{
			base.SetMessageSendingObjectDefaultValues();
			ShouldSend = true;
		}

		#endregion
	}
}
