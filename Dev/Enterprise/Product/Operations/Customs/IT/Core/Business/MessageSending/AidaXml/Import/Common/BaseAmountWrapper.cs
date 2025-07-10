using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

public sealed class BaseAmountWrapper : IBaseAmount
{
	public BaseAmountWrapper(CusEntryLineFee entryLineFee)
	{
		this.entryLineFee = Argument.NotNull(entryLineFee, nameof(entryLineFee));
	}

	readonly CusEntryLineFee entryLineFee;

	decimal IBaseAmount.Amount => entryLineFee.CF_BaseValue;
}
