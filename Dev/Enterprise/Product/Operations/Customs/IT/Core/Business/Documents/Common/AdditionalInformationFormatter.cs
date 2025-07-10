using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.DocumentWrappers.Customs.EU;

namespace Enterprise.Customs.IT.Business;

sealed class AdditionalInformationFormatter : IAdditionalInformationFormatter
{
	ZString IAdditionalInformationFormatter.Format(AdditionalInfo additionalInfo)
	{
		Argument.NotNull(additionalInfo, nameof(additionalInfo));

		return new ZStringBuilder()
			.AppendIfNotEmpty(additionalInfo.CSI_Code)
			.AppendIfNotEmpty(additionalInfo.CSI_Description)
			.AppendIfNotEmpty(additionalInfo.CSI_ReferenceNumber)
			.ToStringWithDelimiterBetweenAppends(Enterprise.DocumentWrappers.Customs.EU.DocumentWrapperConstants.Delimiters.Dash);
	}
}
