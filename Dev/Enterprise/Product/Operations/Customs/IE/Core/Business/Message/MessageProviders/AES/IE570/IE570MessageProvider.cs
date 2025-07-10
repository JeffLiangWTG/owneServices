using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES
{
	public class IE570MessageProvider : IE570And573CommonMessageProvider, IIE570Header, IIE570ExportOperation
	{
		public IE570MessageProvider(CusEntryHeader entryHeader) : base(entryHeader)
		{
			Consignment = new IE570And573CommonConsignmentProvider(entryHeaderWrapper);
		}

		public IIE570ExportOperation ExportOperation => this;

		public IIE570And573Consignment Consignment { get; }

		#region IIE570ExportOperation Members

		public string LRN => AESOutboundEDIMessage.LRNPlaceHolder;

		public string StoringFlag => AESFlagCodeList.Codes.No;

		#endregion

	}
}
