using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface ICharges
	{
		ZDecimal DutyAmount { get; }
		ZDecimal LiquorTaxAmount { get; }
		ZDecimal SpecialConsumptionTaxAmount { get; }
		ZDecimal TransportTaxAmount { get; }
		ZDecimal EducationTaxAmount { get; }
		ZDecimal AgricultureTaxAmount { get; }
		ZDecimal VATAmount { get; }
		ZDecimal ValueForVAT { get; }
		ZDecimal ValueExemptForVAT { get; }
		ZDecimal TotalPenalty { get; }
		ZDecimal LateDeclarationPenalty { get; }
		ZDecimal MissedDeclarationPenalty { get; }
		ZDecimal LatePaymentPenalty { get; }
		ZDecimal NonDutyTaxPayment { get; }
		ZDecimal TotalPaid { get; }
	}
}
