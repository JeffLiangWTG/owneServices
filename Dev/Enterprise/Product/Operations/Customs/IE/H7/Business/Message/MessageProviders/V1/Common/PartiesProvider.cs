using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.AIS;
using IParty = CargoWise.Customs.IE.MessageContracts.AIS.Interfaces.IParty;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class PartiesProvider : IParties
	{
		public PartiesProvider(AsycudaManifestHeader header)
		{
			this.header = header;
		}
		readonly AsycudaManifestHeader header;

		public IRepresentative Representative => CachedValueHelper.GetValue(ref representativeCached, () => MRepresentativeProvider.NewOrNull(header));
		CachedValue<IRepresentative> representativeCached;

		public IParty Declarant => CachedValueHelper.GetValue(ref declarantCached, () => header.Declarant == null ? null : new PartyProvider(header.Declarant));
		CachedValue<IParty> declarantCached;
	}
}
