using CargoWise.Types;

namespace Enterprise.Customs.EU.Integration.SadH
{
	public interface ITax
	{
		ZString TaxType { get; } // TTY-CODE
		ZDecimal TaxBaseAmount { get; } // ITLN-BASE-AMT-DC
		ZDecimal TaxBaseQuantity { get; } // ITLN-BASE-QTY
		ZString TaxBaseQuantityUQ { get; } // ITLN-BASE-QTYUQ
		ZString TaxRate { get; } // TAX-RATE-ID
		ZString TaxOverrideCode { get; } // TTY-OVR-CODE
		ZString TaxAmount { get; } // ITLN-DECL-TAX-DC
		ZString MethodOfPayment { get; } // MOP-CODE
		ZString MethodOfCalculation { get; }
	}
}
