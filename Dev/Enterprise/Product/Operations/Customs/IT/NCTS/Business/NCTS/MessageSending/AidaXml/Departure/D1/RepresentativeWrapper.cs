using System;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.MasterFiles.Business;
using IRepresentative = CargoWise.Customs.IT.MessageContracts.NCTS.Departure.IRepresentative;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class RepresentativeWrapper : IRepresentative
{
	public RepresentativeWrapper(OrgHeader representative)
	{
		Argument.NotNull(representative, nameof(representative));
		lazyIdentificationNumber = new Lazy<string>(() => GetIdentificationNumber(representative));
	}

	string IRepresentative.IdentificationNumber => lazyIdentificationNumber.Value;
	readonly Lazy<string> lazyIdentificationNumber;

	int IRepresentative.Status => 2;

	string GetIdentificationNumber(OrgHeader representative)
	{
		ITrader traderCarrier = new EoriOrTcuTraderWrapper(representative.MainAddress);
		return traderCarrier.IdentificationNumber;
	}
}
