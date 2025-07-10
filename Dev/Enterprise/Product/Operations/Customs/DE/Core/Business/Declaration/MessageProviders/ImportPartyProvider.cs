using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business
{
	public sealed class ImportPartyProvider : IImportParty
	{
		public static ImportPartyProvider NewOrNull(OrgAddress orgAddress) => orgAddress == null ? null : new ImportPartyProvider(orgAddress);

		ImportPartyProvider(OrgAddress orgAddress)
		{
			this.orgAddress = orgAddress;
		}
		readonly OrgAddress orgAddress;
		OrgHeader orgHeader => orgAddress.Header;

		public IPartyID Identification => CachedValueHelper.GetValue(ref identificationCached, () => orgHeader.HasEUEoriNumber() ? ImportPartyIDProvider.NewOrNull(orgAddress) : null);
		CachedValue<IPartyID> identificationCached;

		public IImportPartyIdAddress Address => CachedValueHelper.GetValue(ref addressCached, () => !orgHeader.HasEUEoriNumber() ? ImportPartyIdAddressProvider.NewOrNull(orgAddress) : null);
		CachedValue<IImportPartyIdAddress> addressCached;

		public string TaxNumber => orgHeader.GetVATRegistrationNumberWithCountryCodePrefix(orgAddress.OA_RN_NKCountryCode);
	}
}
