using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.CH.Business;

public class EVVAddressWrapper : DocumentWrapper
{
	internal static EVVAddressWrapper New(IEvvAddress evvAddress) => new EVVAddressWrapper(Argument.NotNull(evvAddress, nameof(evvAddress)));

	EVVAddressWrapper(IEvvAddress evvAddress)
	{
		this.evvAddress = evvAddress;
	}
	readonly IEvvAddress evvAddress;

	public ZString Name => evvAddress.Name;
	public ZString Street => evvAddress.Street;
	public ZString Country => evvAddress.Country;
	public ZString PostalCode => evvAddress.PostalCode;
	public ZString City => evvAddress.City;
}
