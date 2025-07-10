using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	class ConsolDataCalculator : Customs.Business.ConsolDataCalculator
	{
		public ConsolDataCalculator(ForwardingConsol consol, ZString countryCode)
			: base(consol, countryCode)
		{ }

		protected override ZString GetConsolTransportMode()
		{
			return consol.JK_TransportMode;
		}

		protected override bool ShouldClobberFirstCountryDischargeDateWithConsolsDatePortOfFirstArrival(ZDateTime zDateTime)
		{
			return zDateTime.IsValid;
		}

		protected override bool ShouldClobberFirstCountryPortOfDischargeWithConsolsPortOfFirstArrival(MasterFiles.Business.RefUNLOCO port)
		{
			return port != null;
		}
	}
}
