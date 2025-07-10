using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETLineDuty
{
	readonly IDutyTaxFee iDutyTaxFee;

	public ETLineDuty(IDutyTaxFee iDutyTaxFee)
	{
		this.iDutyTaxFee = Argument.NotNull(iDutyTaxFee, "iDutyTaxFee");
	}

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 3, true)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString Type => iDutyTaxFee.Type;

	[MessageLayout(Order = 1)]
	[MessageFieldDecimalRepresentation(15, 2, false, true)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZDecimal Base => iDutyTaxFee.Base;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString CalculationFactor1 => iDutyTaxFee.CalculationFactor1;

	[MessageLayout(Order = 3)]
	[MessageFieldDecimalRepresentation(11, 6, false, true)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZDecimal? Rate1 => iDutyTaxFee.Rate1;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	[MessageFieldInternationalRoadTransportsRules("O")]
	public ZString CalculationFactor2 => iDutyTaxFee.CalculationFactor2;

	[MessageLayout(Order = 5)]
	[MessageFieldDecimalRepresentation(11, 6, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	[MessageFieldInternationalRoadTransportsRules("O")]
	public ZDecimal? Rate2 => iDutyTaxFee.Rate2;

	[MessageLayout(Order = 6)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	[MessageFieldInternationalRoadTransportsRules("O")]
	public ZString CalculationFactor3 => iDutyTaxFee.CalculationFactor3;

	[MessageLayout(Order = 7)]
	[MessageFieldDecimalRepresentation(11, 6, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	[MessageFieldInternationalRoadTransportsRules("O")]
	public ZDecimal? Rate3 => iDutyTaxFee.Rate3;

	[MessageLayout(Order = 8)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	[MessageFieldInternationalRoadTransportsRules("O")]
	public ZString CalculationFactor4 => iDutyTaxFee.CalculationFactor4;

	[MessageLayout(Order = 9)]
	[MessageFieldDecimalRepresentation(15, 2, false, true)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZDecimal Amount => iDutyTaxFee.Amount;

	[MessageLayout(Order = 10)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString MethodOfPayment => iDutyTaxFee.MethodOfPayment;
}
