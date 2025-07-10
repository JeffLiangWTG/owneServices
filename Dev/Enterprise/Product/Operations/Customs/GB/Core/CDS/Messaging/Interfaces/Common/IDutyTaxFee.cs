using CargoWise.Types;
using Enterprise.Customs.EU.Integration.SadH;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IDutyTaxFee
	{
		ZString DutyRegimeCode { get; }
		IPayment Payment { get; }
		ZString QuotaOrderID { get; }
		ZString TypeCode { get; }
		IMeasure SpecificTaxBaseQuantity { get; }
		ZString OverrideCode { get; }
		ZBool OutputTypeCode { get; }
	}

	class DutyTaxFeeWrapper : IDutyTaxFee
	{
		DutyTaxFeeWrapper(ITax tax, ZString dutyRegimeCode, ZString quotaOrderID, ZString currency, IPayment payment, bool outputTypeCode)
		{
			this.tax = tax;
			this.payment = payment;
			this.dutyRegimeCode = dutyRegimeCode;
			quotaOrderId = quotaOrderID;
			this.currency = currency;
			this.outputTypeCode = outputTypeCode;
		}

		public static DutyTaxFeeWrapper New(IImportLine importLine, ITax tax, string currency = "", bool outputTypeCode = false)
		{
			return New(tax, dutyRegimeCode: importLine.PreferenceCode, quotaOrderID: importLine.QuotaOrderNumber.Left(6).PadRight(6), currency: currency, outputTypeCode: outputTypeCode);
		}

		public static DutyTaxFeeWrapper New(ITax tax, IPayment payment = null, string dutyRegimeCode = "", string quotaOrderID = "", string currency = "", bool outputTypeCode = false)
		{
			return new DutyTaxFeeWrapper(tax, dutyRegimeCode, quotaOrderID, currency, payment, outputTypeCode);
		}

		ZString IDutyTaxFee.DutyRegimeCode => dutyRegimeCode;

		IPayment IDutyTaxFee.Payment => payment ?? PaymentWrapper.New(tax.MethodOfPayment
			, AmountAndCurrencyWrapper.New(ZDecimal.ParseSafe(tax.TaxAmount, ZDecimal.Zero), currency)
			, AmountAndCurrencyWrapper.New(ZDecimal.Zero, currency));

		ZString IDutyTaxFee.QuotaOrderID => quotaOrderId;

		ZString IDutyTaxFee.TypeCode => tax.TaxType;

		IMeasure IDutyTaxFee.SpecificTaxBaseQuantity
		{
			get
			{
				var baseAmount = tax.TaxBaseAmount;
				if (!baseAmount.IsEmpty)
				{
					var measureUQ = tax.MethodOfCalculation == (ZString)Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage || tax.MethodOfCalculation == ZString.Empty ? currency : tax.MethodOfCalculation;
					return MeasureWrapper.New(baseAmount, measureUQ);
				}

				return MeasureWrapper.New(ZDecimal.Zero, ZString.Empty);
			}
		}

		ZString IDutyTaxFee.OverrideCode => tax.TaxOverrideCode;

		public ZBool OutputTypeCode => outputTypeCode;

		readonly ITax tax;
		readonly ZString dutyRegimeCode;
		readonly ZString quotaOrderId;
		readonly ZString currency;
		readonly IPayment payment;
		readonly ZBool outputTypeCode;
	}

	class TaxWrapper : ITax
	{
		public TaxWrapper(string taxType = "", decimal taxBaseAmount = 0m, decimal taxBaseQuantity = 0m, string taxRate = "", string taxOverrideCode = "", string taxAmount = "", string methodOfPayment = "", string methodOfCalculation = "")
		{
			TaxType = taxType;
			TaxBaseAmount = taxBaseAmount;
			TaxBaseQuantity = taxBaseQuantity;
			TaxRate = taxRate;
			TaxOverrideCode = taxOverrideCode;
			TaxAmount = taxAmount;
			MethodOfPayment = methodOfPayment;
			MethodOfCalculation = methodOfCalculation;
		}

		public ZString TaxType { get; }

		public ZDecimal TaxBaseAmount { get; }

		public ZDecimal TaxBaseQuantity { get; }

		public ZString TaxRate { get; }

		public ZString TaxOverrideCode { get; }

		public ZString TaxAmount { get; }

		public ZString MethodOfPayment { get; }

		public ZString TaxBaseQuantityUQ { get; }

		public ZString MethodOfCalculation { get; }
	}
}
