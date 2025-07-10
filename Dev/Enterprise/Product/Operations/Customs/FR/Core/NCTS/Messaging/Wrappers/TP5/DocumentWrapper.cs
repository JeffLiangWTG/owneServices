using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class DocumentWrapper : IDocument
	{
		protected DocumentWrapper(CusSupportingInfo supportingInfo)
		{
			this.supportingInfo = Argument.NotNull(supportingInfo, nameof(supportingInfo));
		}

		protected readonly CusSupportingInfo supportingInfo;

		public static DocumentWrapper New(CusSupportingInfo supportingInfo) => supportingInfo == null ? null : new DocumentWrapper(supportingInfo);

		public virtual string Type => type ?? (type = supportingInfo.CSI_Code);
		string type;

		public virtual string ReferenceNumber => referenceNumber ?? (referenceNumber = supportingInfo.CSI_ReferenceNumber);
		string referenceNumber;
	}
}
