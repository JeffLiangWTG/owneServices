using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.CL.Manifest.Business
{
	internal class AWBSendChileWrapperOpTransport : IDocOpTransport
	{
		internal AWBSendChileWrapperOpTransport(AsycudaManifestHeader header, ZString voyageName)
		{
			this.header = Argument.NotNull(header, nameof(header));
			this.voyageName = voyageName;
		}
		readonly AsycudaManifestHeader header;
		readonly ZString voyageName;

		string IDocOpTransport.Nature => header.AMA_Nature == ShipmentTypeList.Codes.Import23 ? WrappersConstants.OperationType.I : WrappersConstants.OperationType.S;

		string IDocOpTransport.VoyageName => voyageName;

		string IDocOpTransport.TransshipmentType => header.AMA_TransshipmentType;
	}
}
