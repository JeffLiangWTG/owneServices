using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class TaxWrapper : ITax
	{
		TaxWrapper(CusEntryLineFee fee)
		{
			Argument.NotNull(fee, nameof(fee));
			this.Amount = (double)fee.CF_BaseValue;
			this.MeasurementUnitAndQualifier = fee.CF_MethodOfCalculation;
			this.Quantity = (double)fee.CF_Rate;
			this.TaxAmount = (double)fee.CF_ChargeAmount;
			this.TaxRate = (double)fee.CF_Rate;
		}

		TaxWrapper(CusEntryHeaderCharges charge)
		{
			Argument.NotNull(charge, nameof(charge));
			this.Amount = (double)charge.C1_ChargeAmount;
			this.MeasurementUnitAndQualifier = "%";
			this.Quantity = 100d;
			this.TaxAmount = (double)charge.C1_ChargeAmount;
			this.TaxRate = 100d;
		}

		public static TaxWrapper New(CusEntryLineFee fee) => fee == null ? null : new TaxWrapper(fee);

		public static TaxWrapper New(CusEntryHeaderCharges charge) => charge == null ? null : new TaxWrapper(charge);

		public double Amount { get; }

		public string MeasurementUnitAndQualifier { get; }

		public double Quantity { get; }

		public double TaxAmount { get; }

		public double TaxRate { get; }
	}
}
