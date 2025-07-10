using CargoWise.Customs.BE.MessageContracts.Interfaces;

namespace Enterprise.Customs.BE.Business;

public class AdditionsAndDeductionsProvider : IItemAdditionsAndDeductionsType
{
	public AdditionsAndDeductionsProvider(int sequenceNumber, string code, decimal amount)
	{
		this.Code = code;
		this.Amount = amount;
		this.SequenceNumber = sequenceNumber.ToString();
	}

	public string Code { get; }
	public decimal Amount { get; }
	public string SequenceNumber { get; }
}
