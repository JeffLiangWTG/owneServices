using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class IM432PartiesTypeProvider : IIM432PartiesType
	{
		IM432PartiesTypeProvider(AsycudaManifestHeader header)
		{
			this.header = header;
			address = header.Declarant;
		}

		readonly OrgAddress address;
		readonly AsycudaManifestHeader header;

		public static IM432PartiesTypeProvider NewOrNull(AsycudaManifestHeader header)
		{
			var address = header.Declarant;
			return address == null ? null : new IM432PartiesTypeProvider(header);
		}

		public string PresentationPerson => CachedValueHelper.GetValue(ref presentationPersonCached, () =>
		{
			var id = address.GetEORI();
			return id.IsEmpty ? null : id.ToString();
		});
		CachedValue<string> presentationPersonCached;

		public IRepresentative Representative => CachedValueHelper.GetValue(ref representativeCached, () => MRepresentativeProvider.NewOrNull(header));
		CachedValue<IRepresentative> representativeCached;
	}
}
