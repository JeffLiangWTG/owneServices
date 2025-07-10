using Enterprise.Integration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	class PrintP5Provider : PrintFromMessageProvider
	{
		public PrintP5Provider(ICcsukCusAwb awb, EDIMessage baseMessage, ILogger logger)
			: base(baseMessage, logger)
		{
			this.awb = awb;
		}

		public override void DoPrinting()
		{
			ReleasePrintHelper.PrintP5OriginalAndReprint(gbEdiMessage, awb, logger);
		}

		readonly ICcsukCusAwb awb;
	}
}
