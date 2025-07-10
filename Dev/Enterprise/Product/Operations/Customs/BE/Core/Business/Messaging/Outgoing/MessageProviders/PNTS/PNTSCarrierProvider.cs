using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.BE.Business;

public class PNTSCarrierProvider : IPNTSPartyWithName
{
	public PNTSCarrierProvider(OrgAddress address)
	{
		carrier = Argument.NotNull(address, nameof(address));
	}
	readonly OrgAddress carrier;
	OrgHeader orgHeader => carrier.Header;

	public string Name => orgHeader?.OH_FullName ?? string.Empty;

	public string IdentificationNumber => CachedValueHelper.GetValue(ref identificationNumber, () => orgHeader?.GetConcatenatedSingleOrgCusCode(EuropeanUnionSharedCodeTypes.Eori) ?? string.Empty);
	CachedValue<string> identificationNumber;
}
