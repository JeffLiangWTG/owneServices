namespace Enterprise.Customs.AE.Manifest.Business;

public interface IMonetaryAmountProvider
{
	string AmountType { get; }

	decimal Amount { get; }

	string Currency { get; }
}
