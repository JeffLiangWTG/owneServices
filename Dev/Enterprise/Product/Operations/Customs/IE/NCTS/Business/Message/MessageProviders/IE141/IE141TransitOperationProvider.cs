using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE141TransitOperationProvider : IMRN
	{
		public IE141TransitOperationProvider(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}
		readonly NctsHeader nctsHeader;

		public string MRN => nctsHeader.MovementReferenceNumber;
	}
}
