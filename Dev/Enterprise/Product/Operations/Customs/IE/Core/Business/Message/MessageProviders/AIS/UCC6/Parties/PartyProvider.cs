using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using IParty = CargoWise.Customs.IE.MessageContracts.AIS.Interfaces.IParty;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class PartyProvider : IParty
	{
		public PartyProvider(OrgAddress address)
		{
			this.address = Argument.NotNull(address, nameof(address));
		}
		readonly protected OrgAddress address;

		public IAddress Address => CachedValueHelper.GetValue(ref addressCached, () => Id.IsNullOrEmpty() ? AddressProvider.New(address) : null);
		CachedValue<IAddress> addressCached;

		public string Id => CachedValueHelper.GetValue(ref idCached, () =>
		{
			var id = address.GetEORI();
			return id.IsEmpty ? null : id.ToString();
		});
		CachedValue<string> idCached;

		public string Name => CachedValueHelper.GetValue(ref nameCached, () =>
		{
			if (Id.IsNullOrEmpty())
			{
				var companyNameOverride = address.OA_CompanyNameOverride;
				if (!companyNameOverride.IsEmpty)
				{
					return companyNameOverride;
				}

				var fullName = address.Header.OH_FullName;
				if (!fullName.IsEmpty)
				{
					return fullName;
				}
			}

			return null;
		});
		CachedValue<string> nameCached;
	}
}
