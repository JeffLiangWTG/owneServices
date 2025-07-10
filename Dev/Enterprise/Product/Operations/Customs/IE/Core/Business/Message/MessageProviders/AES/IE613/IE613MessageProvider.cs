using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES
{
	public class IE613MessageProvider : IE613And615CommonMessageProvider, IIE613Header, IIE613ExportOperation
	{
		public IE613MessageProvider(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		public IIE613ExportOperation ExportOperation => this;

		#region IIE613ExportOperation Members

		public string MRN => entryHeader.MovementReferenceNumber;

		#endregion

		public bool IsInTransitionPeriod => declaration.IsTransitionPeriodAES30;
	}
}
