using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class AdditionalInformationWrapper : IAdditionalInformation
	{
		AdditionalInformationWrapper(CusSupportingInfo supportingInfo)
		{
			this.supportingInfo = Argument.NotNull(supportingInfo, nameof(supportingInfo));
		}

		readonly CusSupportingInfo supportingInfo;

		public static AdditionalInformationWrapper New(CusSupportingInfo supportingInfo) => supportingInfo == null ? null : new AdditionalInformationWrapper(supportingInfo);

		public string Code => code ?? (code = supportingInfo.CSI_Code);
		string code;

		public string Text => text ?? (text = supportingInfo.CSI_ReferenceNumber);
		string text;
	}
}
