using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

public sealed class EoriOrTcuTraderWrapper : IEoriTrader
{
	public EoriOrTcuTraderWrapper(JobDocAddress jobDocAddress)
		: this(jobDocAddress, jobDocAddress?.Organisation)
	{
	}

	public EoriOrTcuTraderWrapper(OrgAddress orgAddress)
		: this(orgAddress, orgAddress?.Header)
	{
	}

	EoriOrTcuTraderWrapper(IDocAddress docAddress, OrgHeader organisation)
	{
		this.organisation = organisation;

		lazyIdentificationNumber = new Lazy<ZString>(GetIdentificationNumber);
		lazyAddress = new Lazy<IAddress>(() => GetAddress(docAddress));
		lazyEoriNumber = new Lazy<ZString>(() => organisation?.GetEoriCode(countryCode: true) ?? ZString.Empty);
	}

	#region ITrader Members

	IAddress ITrader.Address => lazyAddress.Value;

	string ITrader.IdentificationNumber => IdentificationNumber;

	#endregion
	string IEoriTrader.EoriNumber => EoriCode;

	#region Implementation

	ZString EoriCode => lazyEoriNumber.Value;

	ZString IdentificationNumber => lazyIdentificationNumber.Value;

	ZString GetIdentificationNumber()
	{
		var eoriCode = EoriCode;

		return !eoriCode.IsEmpty
			? eoriCode
			: organisation?.GetTcuCode() ?? ZString.Empty;
	}

	IAddress GetAddress(IDocAddress docAddress)
	{
		return IdentificationNumber.IsEmpty
			? new AddressWrapper(docAddress)
			: null;
	}

	#endregion

	readonly OrgHeader organisation;
	readonly Lazy<ZString> lazyIdentificationNumber;
	readonly Lazy<ZString> lazyEoriNumber;
	readonly Lazy<IAddress> lazyAddress;
}
