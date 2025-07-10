using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class DeclarantPartyProvider : PartyProvider
	{
		DeclarantPartyProvider(OrgAddress address) : base(address)
		{
		}

		public static DeclarantPartyProvider NewOrNull(AsycudaManifestHeader header) => header?.Declarant == null ? null : new DeclarantPartyProvider(header.Declarant);
	}
}
