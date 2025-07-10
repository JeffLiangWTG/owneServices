using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business;

public class CustomsFee
{
	protected CustomsFee()
	{
	}

	public CustomsFee(ZDecimal percentage)
	{
		MarkCode = FeeMarkTypeList.Codes.Percentage;
		this.percentage = percentage;
	}
	readonly ZDecimal percentage;

	public CustomsFee(Money money)
	{
		MarkCode = FeeMarkTypeList.Codes.TotalPrice;
		this.money = money;
	}
	readonly Money money;

	public ZString MarkCode { get; private set; }

	public ZString MarkDesc => new FeeMarkTypeList().GetDescriptionFromCode(MarkCode);

	public ZDecimal Amount
	{
		get
		{
			switch (MarkCode)
			{
				case FeeMarkTypeList.Codes.TotalPrice:
					return money?.Amount ?? 0;
				case FeeMarkTypeList.Codes.Percentage:
					return percentage;
				default:
					return 0;
			}
		}
	}

	public Money Money => MarkCode == FeeMarkTypeList.Codes.TotalPrice && money != null ? money : Money.Empty;

	public ZString CurrencyCode => Money.IsEmpty ? string.Empty : Money.Currency?.Code;

	public static CustomsFee Empty => new CustomsFee();

	public bool IsEmpty => MarkCode.IsEmpty;
}
