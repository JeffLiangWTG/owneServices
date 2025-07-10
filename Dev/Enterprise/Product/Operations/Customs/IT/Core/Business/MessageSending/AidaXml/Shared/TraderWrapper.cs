using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

public class TraderWrapper : IEoriTrader
{
	public TraderWrapper(JobDocAddress jobDocAddress) : this(jobDocAddress, jobDocAddress?.Organisation, jobDocAddress?.Country)
	{
	}

	public TraderWrapper(OrgAddress orgAddress) : this(orgAddress, orgAddress?.Header, orgAddress?.Country)
	{
	}

	TraderWrapper(IDocAddress docAddress, OrgHeader organisation, RefCountry refCountry)
	{
		this.organisation = organisation;
		this.refCountry = refCountry;

		lazyIdentificationNumber = new Lazy<string>(GetIdentificationNumber);
		lazyAddress = new Lazy<IAddress>(() => GetAddress(docAddress));
		lazyEorNumber = new Lazy<string>(() => organisation?.GetEoriCode(countryCode: true));
	}

	readonly OrgHeader organisation;
	readonly RefCountry refCountry;

	IAddress ITrader.Address => lazyAddress.Value;
	readonly Lazy<IAddress> lazyAddress;

	string ITrader.IdentificationNumber => IdentificationNumber;
	readonly Lazy<string> lazyIdentificationNumber;

	string IEoriTrader.EoriNumber => lazyEorNumber.Value;
	readonly Lazy<string> lazyEorNumber;

	protected string IdentificationNumber => lazyIdentificationNumber.Value.Trim();

	string GetIdentificationNumber()
	{
		var customsCodeInfo = organisation?.GetCustomsCodeInfo(refCountry, zeroIfHasNotValidCustomsCode: false);

		return customsCodeInfo is null
			? ZString.Empty
			: new ZString(customsCodeInfo.IdCountryCode + customsCodeInfo.Id);
	}

	IAddress GetAddress(IDocAddress docAddress)
	{
		return docAddress != null
			? new AddressWrapper(docAddress)
			: null;
	}
}
