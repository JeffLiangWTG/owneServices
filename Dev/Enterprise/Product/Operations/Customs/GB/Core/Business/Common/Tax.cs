using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Integration.SadH;

namespace Enterprise.Customs.GB.Business.Messaging
{
	public class Tax : ITax
	{
		public Tax(EU.Business.Declaration.MultiLineAddInfos.TaxStruct tax)
		{
			TaxType = tax.G4_Type;
			TaxBaseAmount = tax.G4_BaseAmount;
			TaxBaseQuantity = tax.G4_BaseQuantity;
			TaxRate = tax.Box47c1;
			TaxOverrideCode = tax.G4_RateOverride;
			MethodOfPayment = tax.G4_MethodOfPayment;
			TaxAmount = tax.G4_Amount;
			TaxAmountValue = ZDecimal.ParseSafe(TaxAmount, 0);
			TaxBaseQuantityUQ = tax.G4_BaseQuantityUQ;
		}

		public Tax(CusEntryLineFee fee)
		{
			TaxAmount = fee.CF_ChargeAmount.ToString();
			TaxAmountValue = ZDecimal.ParseSafe(TaxAmount, 0);
			TaxType = fee.CF_ChargeType;
			TaxBaseAmount = fee.CF_BaseValue;
			TaxRate = fee.CF_Rate.ToString("0.00");
			TaxOverrideCode = fee.CF_RateOverrideReasonCode;
			MethodOfPayment = fee.CF_MethodOfPayment;
			MethodOfCalculation = fee.CF_MethodOfCalculation;
		}

		// 47a
		public ZString TaxType { get; }

		// 47b
		public ZDecimal TaxBaseAmount { get; }

		// 47b
		public ZDecimal TaxBaseQuantity { get; }

		// 47c1
		public ZString TaxRate { get; }

		// 47c2
		public ZString TaxOverrideCode { get; }

		// 47d
		public ZString TaxAmount { get; }

		public ZDecimal TaxAmountValue { get; }

		// 47e
		public ZString MethodOfPayment { get; }

		public ZString TaxBaseQuantityUQ { get; }

		public ZString MethodOfCalculation { get; }
	}
}
