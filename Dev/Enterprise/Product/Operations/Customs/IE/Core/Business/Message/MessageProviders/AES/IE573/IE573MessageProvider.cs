using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES
{
	public class IE573MessageProvider : IE570And573CommonMessageProvider, IIE573Header, IIE573ExportOperation
	{
		public IE573MessageProvider(CusEntryHeader entryHeader) : base(entryHeader)
		{
			Consignment = new IE570And573CommonConsignmentProvider(entryHeaderWrapper);
		}

		public IIE573ExportOperation ExportOperation => this;

		public IIE570And573Consignment Consignment { get; }

		#region IIE570ExportOperation Members

		public string StoringFlag => AESFlagCodeList.Codes.No;

		public string MRN => entryHeader.MovementReferenceNumber;

		#endregion
	}
}
