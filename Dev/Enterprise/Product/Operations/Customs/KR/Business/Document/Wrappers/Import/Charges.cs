using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("Soon to be used")]
	public class Charges : NonPersistentBusinessObject, ICharges
	{
		public Charges(ChargesSerializable chargesSerializable, BusinessObjectFactory factory) : base(factory)
		{
			this.chargesSerializable = chargesSerializable;
		}

		readonly ICharges chargesSerializable;

		public ZDecimal DutyAmount => chargesSerializable.DutyAmount;

		public ZDecimal LiquorTaxAmount => chargesSerializable.LiquorTaxAmount;

		public ZDecimal SpecialConsumptionTaxAmount => chargesSerializable.SpecialConsumptionTaxAmount;

		public ZDecimal TransportTaxAmount => chargesSerializable.TransportTaxAmount;

		public ZDecimal EducationTaxAmount => chargesSerializable.EducationTaxAmount;

		public ZDecimal AgricultureTaxAmount => chargesSerializable.AgricultureTaxAmount;

		public ZDecimal VATAmount => chargesSerializable.VATAmount;

		public ZDecimal ValueForVAT => chargesSerializable.ValueForVAT;

		public ZDecimal ValueExemptForVAT => chargesSerializable.ValueExemptForVAT;

		public ZDecimal TotalPenalty => chargesSerializable.TotalPenalty;

		public ZDecimal LateDeclarationPenalty => chargesSerializable.LateDeclarationPenalty;

		public ZDecimal MissedDeclarationPenalty => chargesSerializable.MissedDeclarationPenalty;

		public ZDecimal LatePaymentPenalty => chargesSerializable.LatePaymentPenalty;

		public ZDecimal NonDutyTaxPayment => chargesSerializable.NonDutyTaxPayment;

		public ZDecimal TotalPaid => chargesSerializable.TotalPaid;
	}
}
