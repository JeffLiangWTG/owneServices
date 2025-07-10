using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC043CGoodsMeasureProvider
	{
		public CC043CGoodsMeasureProvider(GoodsMeasureType03 goodsMeasure)
		{
			this.goodsMeasure = Argument.NotNull(goodsMeasure, nameof(goodsMeasure));
		}

		readonly GoodsMeasureType03 goodsMeasure;

		public ZDecimal GrossMass => goodsMeasure.GrossMass;
		public ZDecimal NetMassValue => goodsMeasure.NetMassValue;
	}
}
