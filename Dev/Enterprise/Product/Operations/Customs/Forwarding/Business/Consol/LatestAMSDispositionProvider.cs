using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.Forwarding.Business
{
	class LatestAMSDispositionProvider
	{
		public LatestAMSDispositionProvider(ForwardingConsol consol)
		{
			Argument.NotNull(consol, "consol");
			this.consol = consol;
		}

		readonly ForwardingConsol consol;

		public ZString LatestAMSDispositionCode => consol.USAMS?.BH_LatestDispositionCode ?? ZString.Empty;

		public ZString LatestAMSDispositionDesc => consol.USAMS?.BH_LatestDispositionCodeDescription ?? ZString.Empty;
	}
}
