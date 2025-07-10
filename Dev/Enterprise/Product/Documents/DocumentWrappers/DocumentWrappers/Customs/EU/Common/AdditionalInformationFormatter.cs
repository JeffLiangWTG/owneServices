using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.DocumentWrappers.Customs.EU
{
	sealed class AdditionalInformationFormatter : IAdditionalInformationFormatter
	{
		ZString IAdditionalInformationFormatter.Format(AdditionalInfo additionalInfo)
		{
			Argument.NotNull(additionalInfo, nameof(additionalInfo));

			return additionalInfo.CSI_Code + (additionalInfo.CSI_Description.IsEmpty ? "" : "-" + additionalInfo.CSI_Description);
		}
	}
}
