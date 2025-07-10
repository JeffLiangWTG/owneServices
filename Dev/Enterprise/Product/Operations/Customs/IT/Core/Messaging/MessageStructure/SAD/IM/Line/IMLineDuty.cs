using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMLineDuty
{
	public IMLineDuty(IDutyTaxFee dutyTaxFee)
	{
		this.dutyTaxFee = Argument.NotNull(dutyTaxFee, "dutyTaxFee");
	}

	readonly IDutyTaxFee dutyTaxFee;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 3, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString Type => dutyTaxFee.Type;

	[MessageLayout(Order = 2)]
	[MessageFieldDecimalRepresentation(17, 2, false, isDecimalPartFixedLength: true)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZDecimal Base => dutyTaxFee.Base;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, true)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public ZString CalculationFactor1 => dutyTaxFee.CalculationFactor1;

	[MessageLayout(Order = 4)]
	[MessageFieldDecimalRepresentation(11, 6, false, isDecimalPartFixedLength: true)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public ZDecimal? Rate1 => dutyTaxFee.Rate1;

	[MessageLayout(Order = 5)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, false)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public ZString CalculationFactor2 => dutyTaxFee.CalculationFactor2;

	[MessageLayout(Order = 6)]
	[MessageFieldDecimalRepresentation(11, 6, false, isDecimalPartFixedLength: true)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public ZDecimal? Rate2 => dutyTaxFee.Rate2;

	[MessageLayout(Order = 7)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, true)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public ZString CalculationFactor3 => dutyTaxFee.CalculationFactor3;

	[MessageLayout(Order = 8)]
	[MessageFieldDecimalRepresentation(11, 6, false, isDecimalPartFixedLength: true)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public ZDecimal? Rate3 => dutyTaxFee.Rate3;

	[MessageLayout(Order = 9)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, true)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public ZString CalculationFactor4 => dutyTaxFee.CalculationFactor4;

	[MessageLayout(Order = 10)]
	[MessageFieldDecimalRepresentation(17, 2, false, isDecimalPartFixedLength: true)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZDecimal Amount => dutyTaxFee.Amount;

	[MessageLayout(Order = 11)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, true)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString MethodOfPayment => dutyTaxFee.MethodOfPayment;
}
