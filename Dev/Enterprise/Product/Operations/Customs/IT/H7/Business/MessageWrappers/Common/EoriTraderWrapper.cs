using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.H7.Business;

public sealed class EoriTraderWrapper : IEoriTrader
{
	public EoriTraderWrapper(string identificationNumber, IAddress address)
	{
		this.identificationNumber = identificationNumber;
		this.address = address;
	}

	readonly string identificationNumber;
	readonly IAddress address;

	public string EoriNumber => null;

	public IAddress Address => address;

	public string IdentificationNumber => identificationNumber;
}
