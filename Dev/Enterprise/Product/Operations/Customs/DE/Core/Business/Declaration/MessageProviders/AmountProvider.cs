using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business
{
	public class AmountProvider : IAmount
	{
		public AmountProvider(ZDecimal quantity, string measurementUnit = "")
		{
			this.measurementUnit = measurementUnit;
			Quantity = quantity;
		}

		readonly ZString measurementUnit;
		public decimal Quantity { get; }
		public string MeasurementUnit => measurementUnit.SubstringSafe(0, 3);
		public string Qualifier => measurementUnit.SubstringSafe(3, 1);
	}
}
