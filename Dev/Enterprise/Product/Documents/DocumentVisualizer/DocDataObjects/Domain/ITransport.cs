using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface ITransport
	{
		ZInt LegOrder { get; set; }

		ZString VoyageFlightNumber { get; set; }

		ZDateTime ETD { get; set; }
		ZDateTime ETA { get; set; }
		ZDateTime ATD { get; set; }
		ZDateTime ATA { get; set; }
		ZDateTime LCLCutOff { get; set; }
		ZDateTime LCLReceivalCommences { get; set; }

		ICodeDescription Mode { get; }
		ICodeDescription AdditionalTransportMode { get; }
		ICodeDescription Type { get; }
		ICodeDescription Status { get; }

		IVessel Vessel { get; }

		IUnloco PortOfLoading { get; }
		IUnloco PortOfDischarge { get; }

		IAddress Carrier { get; }
	}
}
