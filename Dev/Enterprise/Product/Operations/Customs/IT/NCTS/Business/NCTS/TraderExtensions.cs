using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

static class TraderExtensions
{
	public static string GetEoriOrTcuTraderNumber(this OrgHeader orgHeader) => OrgHeaderExtension.GetEoriOrTcuTraderNumber(orgHeader.MainAddress);

	public static string GetEoriOrTcuTraderNumber(this JobDocAddress jobDocAddress) => OrgHeaderExtension.GetEoriOrTcuTraderNumber(jobDocAddress);

	public static AddressTypeDataProviderAbstractClass GetTraderAddressOrNull(this JobDocAddress jobDocAddress, bool isInPhase5TransitionPeriod)
	{
		var trader = GetTraderFromAddress(jobDocAddress);
		if (trader.Address is null)
		{
			return null;
		}

		var maxStreetAndNumberLen = isInPhase5TransitionPeriod ?
			NctsConstants.CustomsFieldMaxLength.TransitionPeriod.Trader.Address :
			NctsConstants.CustomsFieldMaxLength.Trader.Address;

		return new AddressTypeWrapper(TrimmerHelper.Instance.TrimAddress(
			address: trader.Address,
			maxStreetAndNumberLen: maxStreetAndNumberLen,
			maxCityLen: NctsConstants.CustomsFieldMaxLength.TransitionPeriod.Trader.City,
			maxZipCodeLen: NctsConstants.CustomsFieldMaxLength.TransitionPeriod.Trader.PostCode));
	}

	public static string GetTraderNameOrNull(this JobDocAddress jobDocAddress, bool isInPhase5TransitionPeriod)
	{
		var trader = GetTraderFromAddress(jobDocAddress);
		if (trader.Address is null)
		{
			return null;
		}

		var maxNameLen = isInPhase5TransitionPeriod ?
			NctsConstants.CustomsFieldMaxLength.TransitionPeriod.Trader.Name :
			NctsConstants.CustomsFieldMaxLength.Trader.Name;

		return TrimmerHelper.Instance.TrimAddress(
			address: trader.Address,
			maxNameLen: maxNameLen).Name;
	}

	static ITrader GetTraderFromAddress(JobDocAddress address) => new EoriOrTcuTraderWrapper(address);
}
