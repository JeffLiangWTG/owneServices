using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class PartyProvider : IParty
	{
		public static PartyProvider New(JobDocAddress docAddress, string id = "")
		{
			PartyProvider result = null;
			if (docAddress != null)
			{
				result = docAddress.E2_AddressOverride
					? new PartyProvider(docAddress, docAddress.E2_GovRegNum)
					: new PartyProvider(docAddress, new ZString(id).IfEmptyUse(() => docAddress.Address.GetEORI()));
			}
			return result;
		}

		public static PartyProvider New(OrgAddress address)
		{
			PartyProvider result = null;
			if (address != null)
			{
				result = new PartyProvider(address, address.GetEORI());
			}
			return result;
		}

		public static PartyProvider NewWithIDNull(JobDocAddress docAddress) => docAddress != null ? new PartyProvider(docAddress, null) : null;

		protected PartyProvider(IDocAddress address, string id)
		{
			this.address = address;
			this.id = id;
		}
		readonly IDocAddress address;
		readonly string id;

		public CargoWise.Customs.IE.MessageContracts.Interfaces.IAddressWithName Address => CachedValueHelper.GetValue(ref addressCached, () => ID.IsNullOrEmpty() || ID == NoRegNumber ? AddressWithNameProvider.New(address) : null);
		CachedValue<CargoWise.Customs.IE.MessageContracts.Interfaces.IAddressWithName> addressCached;

		public string ID => id;

		public const string NoRegNumber = "NR";
	}
}
