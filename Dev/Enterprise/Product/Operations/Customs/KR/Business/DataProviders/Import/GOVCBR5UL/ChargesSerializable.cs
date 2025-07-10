using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	[XmlRoot("Charges")]
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class ChargesSerializable : ICharges
	{
		public decimal DutyAmount { get; set; }
		public decimal LiquorTaxAmount { get; set; }
		public decimal SpecialConsumptionTaxAmount { get; set; }
		public decimal TransportTaxAmount { get; set; }
		public decimal EducationTaxAmount { get; set; }
		public decimal AgricultureTaxAmount { get; set; }
		public decimal VATAmount { get; set; }
		public decimal ValueForVAT { get; set; }
		public decimal ValueExemptForVAT { get; set; }
		public decimal TotalPenalty { get; set; }
		public decimal LateDeclarationPenalty { get; set; }
		public decimal MissedDeclarationPenalty { get; set; }
		public decimal LatePaymentPenalty { get; set; }
		public decimal NonDutyTaxPayment { get; set; }
		public decimal TotalPaid { get; set; }

		ZDecimal ICharges.DutyAmount => DutyAmount;
		ZDecimal ICharges.LiquorTaxAmount => LiquorTaxAmount;
		ZDecimal ICharges.SpecialConsumptionTaxAmount => SpecialConsumptionTaxAmount;
		ZDecimal ICharges.TransportTaxAmount => TransportTaxAmount;
		ZDecimal ICharges.EducationTaxAmount => EducationTaxAmount;
		ZDecimal ICharges.AgricultureTaxAmount => AgricultureTaxAmount;
		ZDecimal ICharges.VATAmount => VATAmount;
		ZDecimal ICharges.ValueForVAT => ValueForVAT;
		ZDecimal ICharges.ValueExemptForVAT => ValueExemptForVAT;
		ZDecimal ICharges.TotalPenalty => TotalPenalty;
		ZDecimal ICharges.LateDeclarationPenalty => LateDeclarationPenalty;
		ZDecimal ICharges.MissedDeclarationPenalty => MissedDeclarationPenalty;
		ZDecimal ICharges.LatePaymentPenalty => LatePaymentPenalty;
		ZDecimal ICharges.NonDutyTaxPayment => NonDutyTaxPayment;
		ZDecimal ICharges.TotalPaid => TotalPaid;
	}
}
