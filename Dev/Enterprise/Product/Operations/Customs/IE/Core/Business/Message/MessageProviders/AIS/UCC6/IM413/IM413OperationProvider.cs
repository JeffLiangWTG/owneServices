using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM413OperationProvider : IM413And415OperationProvider, IIM413Operation
	{
		public IM413OperationProvider(CusEntryHeader entryHeader) : base(entryHeader, false) { }

		public string CustomsRegistrationNumber => entryHeader.CRN;

		public string DetailsAmended => entryHeader.CH_CustomsMessageRemarks;

		public string MRN => entryHeader.MovementReferenceNumber;
	}
}
