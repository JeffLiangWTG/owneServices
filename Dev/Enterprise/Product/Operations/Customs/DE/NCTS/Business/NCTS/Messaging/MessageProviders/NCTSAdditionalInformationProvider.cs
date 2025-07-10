using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NCTSAdditionalInformationProvider : INCTSAdditionalInformation
	{
		public static NCTSAdditionalInformationProvider NewOrNull(CusSupportingInfo additionalInfo) => additionalInfo != null ? new NCTSAdditionalInformationProvider(additionalInfo) : null;

		NCTSAdditionalInformationProvider(CusSupportingInfo additionalInfo)
		{
			this.additionalInfo = Argument.NotNull(additionalInfo, nameof(additionalInfo));
		}

		public string Code => additionalInfo.CSI_Code.ValueOrNullIfEmpty();

		public string Text => additionalInfo.CSI_Description.ValueOrNullIfEmpty();

		readonly CusSupportingInfo additionalInfo;
	}
}
