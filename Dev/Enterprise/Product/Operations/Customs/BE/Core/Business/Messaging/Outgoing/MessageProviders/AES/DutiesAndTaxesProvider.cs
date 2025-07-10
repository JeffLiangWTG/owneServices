using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business;

public class DutiesAndTaxesProvider : IDutiesAndTaxes
{
	public DutiesAndTaxesProvider(CusEntryLineFee entryLineFee, int sequence)
	{
		this.entryLineFee = Argument.NotNull(entryLineFee, nameof(entryLineFee));
		this.sequenceNumber = sequence;
	}

	readonly CusEntryLineFee entryLineFee;
	readonly int sequenceNumber;

	public string SequenceNumber => sequenceNumber.ToString();

	public string TaxType => entryLineFee.CF_ChargeType;

	public decimal PayableTaxAmount => entryLineFee.CF_ChargeAmount;

	public string MethodOfPayment => entryLineFee.EntryLine.Header.Declaration.JE_PaymentMethod;

	public IReadOnlyCollection<ITaxBase> TaxBase => taxBase ?? (taxBase = new List<ITaxBase>() { new TaxBaseProvider(entryLineFee, 1) });
	IReadOnlyCollection<ITaxBase> taxBase;
}
