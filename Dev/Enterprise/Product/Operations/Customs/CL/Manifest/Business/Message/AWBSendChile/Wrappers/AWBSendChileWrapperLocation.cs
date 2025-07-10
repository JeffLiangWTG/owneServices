using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.CL.Manifest.Business
{
	class AWBSendChileWrapperLocation : IDocLocations
	{
		internal AWBSendChileWrapperLocation(AsycudaManifestHeader header, ZString locationName)
		{
			this.header = Argument.NotNull(header, nameof(header));
			Name = locationName;
		}
		readonly AsycudaManifestHeader header;

		public string Name { get; }

		IDocLocations documentLocation => this;

		string IDocLocations.Code => documentLocation.Name == WrappersConstants.LocationName.Pd ? header.AMA_RL_NKPortOfDischarge : header.AMA_RL_NKPortOfLoading;
	}
}
