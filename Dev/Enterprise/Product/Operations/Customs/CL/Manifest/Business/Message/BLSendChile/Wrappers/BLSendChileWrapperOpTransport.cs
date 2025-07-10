using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.CL.Manifest.Business
{
	internal class BLSendChileWrapperOpTransport : IDocumentOpTransport
	{
		internal BLSendChileWrapperOpTransport(AsycudaManifestHeader header)
		{
			this.header = Argument.NotNull(header, nameof(header));
		}
		readonly AsycudaManifestHeader header;

		string IDocumentOpTransport.Nature => header.AMA_Nature == ShipmentTypeList.Codes.Import23 ? WrappersConstants.OperationType.I : header.AMA_Nature == ShipmentTypeList.Codes.Export22 ? WrappersConstants.OperationType.S : header.AMA_Nature == ShipmentTypeList.Codes.Transit24 ? WrappersConstants.OperationType.Tr : WrappersConstants.OperationType.Trb;

		string IDocumentOpTransport.VesselName => header.AMA_VesselName;
	}
}
