using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES
{
	public class IE615MessageProvider : IE613And615CommonMessageProvider, IIE615Header, IIE615ExportOperation
	{
		public IE615MessageProvider(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		public IIE615ExportOperation ExportOperation => this;

		#region IIE615ExportOperation Members

		public string LRN => AESOutboundEDIMessage.LRNPlaceHolder;

		#endregion

		public bool IsInTransitionPeriod => declaration.IsTransitionPeriodAES30;
	}
}
