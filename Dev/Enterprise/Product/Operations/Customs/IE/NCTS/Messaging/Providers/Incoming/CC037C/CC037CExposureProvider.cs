using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC037CExposureProvider
	{
		public CC037CExposureProvider(ExposureType exposureType)
		{
			this.exposure = Argument.NotNull(exposureType, nameof(exposureType));
		}
		readonly ExposureType exposure;

		public ZDecimal Exposure => exposure.Exposure;

		public ZString ExposureCounter => exposure.ExposureCounter;

		public ZDecimal Balance => exposure.Balance ?? ZDecimal.Zero;

		public ZString Currency => exposure.Currency;
	}
}
