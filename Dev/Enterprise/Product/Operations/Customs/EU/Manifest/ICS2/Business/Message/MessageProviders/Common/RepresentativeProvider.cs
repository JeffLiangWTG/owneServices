using Enterprise.Customs.EU.Manifest.ICS2.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class RepresentativeProvider : PartyProvider
	{
		RepresentativeProvider(AsycudaManifestHeader manifestHeader) : base(manifestHeader.ShippingAgent)
		{
			this.manifestHeader = manifestHeader;
		}

		public static RepresentativeProvider NewOrNull(AsycudaManifestHeader manifestHeader) => manifestHeader?.ShippingAgent == null ? null : new RepresentativeProvider(manifestHeader);

		readonly AsycudaManifestHeader manifestHeader;

		protected override string GetStatus()
		{
			if (manifestHeader.AMA_AgentType == Core.Constants.AgentType.Direct)
			{
				return EUICS2RepresentativeStatus.Codes.CL094_2;
			}

			return EUICS2RepresentativeStatus.Codes.CL094_3;
		}
	}
}
