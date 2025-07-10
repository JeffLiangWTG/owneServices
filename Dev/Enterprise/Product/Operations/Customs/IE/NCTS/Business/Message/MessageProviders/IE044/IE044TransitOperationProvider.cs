using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE044TransitOperationProvider : IIE044TransitOperation
	{
		readonly NctsHeader nctsHeader;

		public IE044TransitOperationProvider(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}

		public string OtherThingsToReport => nctsHeader.ArrivalMovementHeader?.OtherThingsToReport;

		public string MRN => nctsHeader.ArrivalMrnFromUser;
	}
}
