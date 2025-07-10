using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public abstract class NCTSHeaderProvider : INCTSHeader
	{
		protected readonly NctsHeader nctsHeader;

		protected NCTSHeaderProvider(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}
	}
}
