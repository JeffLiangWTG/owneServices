using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class AdditionalInformationWrapper : IAdditionalInformation
	{
		AdditionalInformationWrapper(CusSupportingInfo additionalInfo, string customsOffice)
		{
			this.additionalInfo = Argument.NotNull(additionalInfo, nameof(additionalInfo));
			this.customsOffice = customsOffice;
		}

		readonly CusSupportingInfo additionalInfo;
		readonly string customsOffice;

		public static AdditionalInformationWrapper New(CusSupportingInfo additionalInfo, string customsOffice) => additionalInfo == null ? null : new AdditionalInformationWrapper(additionalInfo, customsOffice);

		public string CcQualifier => ccQualifier ?? (ccQualifier = customsOffice.StartsWith(Core.Constants.CountryCodes.France) ? string.Empty : Core.Constants.CountryCodes.France);
		string ccQualifier;

		public string Code => code ?? (code = additionalInfo.CSI_Code);
		string code;

		public string Text => text ?? (text = additionalInfo.CSI_Description);
		string text;
	}
}
