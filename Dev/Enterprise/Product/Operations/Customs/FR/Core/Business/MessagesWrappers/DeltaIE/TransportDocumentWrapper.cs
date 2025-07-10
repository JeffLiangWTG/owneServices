using CargoWise.Common;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class TransportDocumentWrapper : CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces.ITransportDocument
	{
		TransportDocumentWrapper(CusSupportingInfo supportingInfo)
		{
			this.supportingInfo = Argument.NotNull(supportingInfo, nameof(supportingInfo));
		}
		readonly CusSupportingInfo supportingInfo;

		public string ReferenceNumber => referenceNumber ?? (referenceNumber = supportingInfo.CSI_ReferenceNumber);
		string referenceNumber;

		public string Type => type ?? (type = supportingInfo.CSI_Code);
		string type;

		public static TransportDocumentWrapper New(CusSupportingInfo supportingInfo) => supportingInfo == null ? null : new TransportDocumentWrapper(supportingInfo);
	}
}
