using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public interface ICimParser
	{
		bool DoAllProcessingBeforePrinting();
		ZString CargoImpCode { get; }
		ICcsukCusAwb Awb { get; }
		ZString MessageInterpretation { get; }
		void DoPrinting();
		ILogger ServiceLogger { get; set; }
	}
}
