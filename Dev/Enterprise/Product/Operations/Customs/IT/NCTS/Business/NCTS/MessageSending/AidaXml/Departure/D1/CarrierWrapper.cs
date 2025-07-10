using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class CarrierWrapper : CarrierTypeDataProviderAbstractClass, ICarrier
{
	public static ICarrier NewOrNull(OrgHeader carrier, OrgHeader principal) => GetCarrierWrapper(carrier, principal);

	public static CarrierTypeDataProviderAbstractClass NewOrNullCarrierType(OrgHeader carrier, OrgHeader principal) => GetCarrierWrapper(carrier, principal);

	static CarrierWrapper GetCarrierWrapper(OrgHeader carrier, OrgHeader principal)
	{
		var carrierIdNum = carrier != null ? GetIdentificationNumber(carrier.MainAddress) : null;
		var principalIdNum = principal != null ? GetIdentificationNumber(principal.MainAddress) : null;

		if (string.IsNullOrWhiteSpace(carrierIdNum) || carrierIdNum == principalIdNum)
		{
			return null;
		}

		return new CarrierWrapper(carrierIdNum);

		string GetIdentificationNumber(OrgAddress address)
		{
			var traderWrapper = (ITrader)new EoriOrTcuTraderWrapper(address);
			return traderWrapper.IdentificationNumber;
		}
	}

	CarrierWrapper(string identificationNumber)
	{
		IdentificationNumber = identificationNumber;
	}

	public override ContactPersonTypeDataProviderAbstractClass ContactPerson => default;

	public override string IdentificationNumber { get; }
}
