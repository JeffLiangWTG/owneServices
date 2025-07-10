using System.Linq;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.H7.Business;

public sealed class H7EoriTraderWrapper : IH7EoriTrader
{
	public H7EoriTraderWrapper(OrgAddress orgAddress)
	{
		this.orgAddress = orgAddress;
	}

	readonly OrgAddress orgAddress;

	public string EoriNumber => null;

	public IH7Address Address => CachedValueHelper.GetValue(ref address, () => new H7AddressWrapper(orgAddress));

	CachedValue<IH7Address> address;

	public string IdentificationNumber => CachedValueHelper.GetValue(ref identificationNumber, () => orgAddress?.CustomsCodes
			.Cast<OrgCusCode>()
			.FirstOrDefault(o => o.OK_CodeType.EqualsIgnoringCase(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori))?
			.OK_CustomsRegNo ?? string.Empty);

	CachedValue<string> identificationNumber;

	IAddress ITrader.Address => null;
}
