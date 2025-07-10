using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business;

public class TaxBaseProvider : ITaxBase
{
	public TaxBaseProvider(CusEntryLineFee entryLineFee, int sequenceNumber)
	{
		this.entryLineFee = Argument.NotNull(entryLineFee, nameof(entryLineFee));
		this.sequenceNumber = sequenceNumber;
	}

	readonly CusEntryLineFee entryLineFee;
	readonly int sequenceNumber;

	public string SequenceNumber => sequenceNumber.ToString();

	public decimal TaxRate => entryLineFee.CF_Rate;

	public string MeasurementUnitAndQualifier => entryLineFee.CF_MethodOfCalculation;

	public decimal Quantity => entryLineFee.CF_BaseValue;

	public decimal Amount => entryLineFee.CF_BaseValue;

	public decimal TaxAmount => entryLineFee.CF_ChargeAmount;
}
