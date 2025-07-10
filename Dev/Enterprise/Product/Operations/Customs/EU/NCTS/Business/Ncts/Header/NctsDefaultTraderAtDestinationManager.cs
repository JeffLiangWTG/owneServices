using CargoWise.Common;
using Enterprise.Customs.EU.Registry;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsDefaultTraderAtDestinationManager
	{
		bool IsEnabled();

		void ApplyDefaultingIfEnabled();
	}

	public class NctsDefaultTraderAtDestinationManager : INctsDefaultTraderAtDestinationManager
	{
		public NctsDefaultTraderAtDestinationManager(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}

		readonly NctsHeader nctsHeader;

		public bool IsEnabled()
		{
			var nctsDefaultTraderAtDestination = EUCustomsDataRegistry.Instance.NCTSDefaultTraderAtDestination.Value;
			return nctsDefaultTraderAtDestination.LeaveBlank || !nctsDefaultTraderAtDestination.TraderAtDestination.IsEmpty;
		}

		public void ApplyDefaultingIfEnabled()
		{
			if (IsEnabled())
			{
				nctsHeader.DestinationTrader.OrganisationPK = EUCustomsDataRegistry.Instance.NCTSDefaultTraderAtDestination.Value.TraderAtDestination;
			}
		}
	}
}
