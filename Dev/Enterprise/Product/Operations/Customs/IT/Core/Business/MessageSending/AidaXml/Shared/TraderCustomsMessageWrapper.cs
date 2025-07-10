using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

sealed class TraderCustomsMessageWrapper : IEoriTrader
{
	public TraderCustomsMessageWrapper(IEoriTrader traderWrapper)
	{
		this.traderWrapper = Argument.NotNull(traderWrapper, nameof(traderWrapper));
	}

	readonly IEoriTrader traderWrapper;

	IAddress ITrader.Address =>
		traderWrapper.IdentificationNumber.IsNullOrEmpty()
		? traderWrapper.Address
		: null;

	string ITrader.IdentificationNumber => traderWrapper.IdentificationNumber;

	string IEoriTrader.EoriNumber => traderWrapper.EoriNumber;
}
