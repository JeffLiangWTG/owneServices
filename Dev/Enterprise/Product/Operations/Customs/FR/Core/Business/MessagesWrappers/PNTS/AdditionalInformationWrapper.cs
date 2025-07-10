using CargoWise.Common;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class AdditionalInformationWrapper : CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces.IAdditionalInformation
	{
		AdditionalInformationWrapper(CusSupportingInfo supportingInfo)
		{
			this.supportingInfo = Argument.NotNull(supportingInfo, nameof(supportingInfo));
		}

		readonly CusSupportingInfo supportingInfo;

		public string Code => code ?? (code = supportingInfo.CSI_Code);
		string code;

		public string Text => text ?? (text = supportingInfo.CSI_Description);
		string text;

		public static AdditionalInformationWrapper New(CusSupportingInfo supportingInfo) => supportingInfo == null ? null : new AdditionalInformationWrapper(supportingInfo);
	}
}
