using Enterprise.Freight.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaManifestHeaderTransportSupporter : ASYCUDA.Business.AsycudaManifestHeaderTransportSupporter
	{
		public AsycudaManifestHeaderTransportSupporter(AsycudaManifestHeader parent)
			: base(parent) { }

		public override JobConsolTransportValidation GetNewTransportValidator(Transport transport)
		{
			var asycudaManifestHeader = transport.Parent as AsycudaManifestHeader;
			switch (asycudaManifestHeader?.AMA_TransportMode)
			{
				case Core.Constants.TransportModes.Road:
					return new TransportMeanRoadValidation((TransportMean)transport);
				default:
					return new TransportMeanValidation((TransportMean)transport);
			}
		}
	}
}
