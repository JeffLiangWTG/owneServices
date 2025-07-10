using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Business
{
	class IE014TransitOperationProvider : IIE014TransitOperation
	{
		public IE014TransitOperationProvider(NctsHeader nctsHeader)
		{
			this.nctsHeader = nctsHeader;
		}
		readonly NctsHeader nctsHeader;

		public string MRN => nctsHeader.MovementReferenceNumber;

		public string LRN => nctsHeader.MovementReferenceNumber.IsEmpty ? nctsHeader.MovementHeader.BM_PaperlessInbondNum : ZString.Empty;
	}
}
