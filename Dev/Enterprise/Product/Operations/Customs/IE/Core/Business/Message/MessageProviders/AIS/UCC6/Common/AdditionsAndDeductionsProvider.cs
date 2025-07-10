using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class AdditionsAndDeductionsProvider : IAdditionsAndDeductions
	{
		public AdditionsAndDeductionsProvider(string code, decimal amount)
		{
			this.Code = code;
			this.Amount = amount;
		}

		public string Code { get; }
		public decimal Amount { get; }
	}
}
