using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	public class AmountFromSnapshotProvider : IAmount
	{
		public static IAmount NewOrNull(Amount amount) => amount != null ? (IAmount)new AmountFromSnapshotProvider(amount) : null;

		AmountFromSnapshotProvider(Amount amount)
		{
			this.amount = Argument.NotNull(amount, nameof(amount));
		}

		readonly Amount amount;

		public decimal Quantity => amount.Quantity;
		public string MeasurementUnit => amount.MeasurementUnit;
		public string Qualifier => amount.Qualifier ?? string.Empty;
	}
}
