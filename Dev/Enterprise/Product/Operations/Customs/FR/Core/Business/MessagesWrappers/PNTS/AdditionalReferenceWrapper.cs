using CargoWise.Common;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class AdditionalReferenceWrapper : CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces.IAdditionalReference
	{
		AdditionalReferenceWrapper(CusSupportingInfo supportingInfo)
		{
			this.supportingInfo = Argument.NotNull(supportingInfo, nameof(supportingInfo));
		}

		readonly CusSupportingInfo supportingInfo;

		public string ReferenceNumber => referenceNumber ?? (referenceNumber = supportingInfo.CSI_ReferenceNumber);
		string referenceNumber;

		public string Type => type ?? (type = supportingInfo.CSI_Code);
		string type;

		public static AdditionalReferenceWrapper New(CusSupportingInfo supportingInfo) => supportingInfo == null ? null : new AdditionalReferenceWrapper(supportingInfo);
	}
}
