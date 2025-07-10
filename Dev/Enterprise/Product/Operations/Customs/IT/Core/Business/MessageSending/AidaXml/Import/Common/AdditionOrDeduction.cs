using CargoWise.Customs.IT.MessageContracts.Declaration.Import;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

public sealed class AdditionOrDeduction : IAdditionOrDeduction
{
	public AdditionOrDeduction(string code, decimal amount)
	{
		Amount = amount;
		Code = code;
	}

	public decimal Amount { get; }
	public string Code { get; }
}
