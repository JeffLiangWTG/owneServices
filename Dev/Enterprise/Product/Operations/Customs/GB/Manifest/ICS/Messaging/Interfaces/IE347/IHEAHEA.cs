using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.GB.ICS.Messaging
{
	[CodeAlive("Will be used in subsequent WI.")]
	public interface IHEAHEA
	{
		ZString TraModAtBorHEA76 { get; }
		ZString InfTypHEA122 { get; }
		ZString ArrivalReferenceNumber { get; }
		ZString UniIdeDivHEA132 { get; }
		ZDateTime ExpDatArrHEA701 { get; }
		ZDateTime ActualDateOfArrival { get; }
		ZString IntendedFirstOffice { get; }
	}
}
