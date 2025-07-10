using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsSADLineSpecialMentionInfoAdditionalInformationWrapper : IETLineSpecialMentionInfoAdditionalInformation
{
	public NctsSADLineSpecialMentionInfoAdditionalInformationWrapper(NctsDepartureCargoDesc departureCargoDesc)
	{
		var goodsItem = Argument.NotNull(departureCargoDesc, nameof(departureCargoDesc));
		var nctsAdditionalInfos = Argument.NotNull(goodsItem.AdditionalInfos, nameof(goodsItem.AdditionalInfos));
		nctsAdditionalInfo = nctsAdditionalInfos.FirstOrDefault();
	}

	readonly NctsAdditionalInfo nctsAdditionalInfo;

	public ZString AdditionalInformation => nctsAdditionalInfo?.CSI_Description ?? ZString.Empty;

	public ZString AdditionalInformationCoded => nctsAdditionalInfo?.CSI_Code ?? ZString.Empty;

	public ZBool? IsExportFromCE => nctsAdditionalInfo?.CSI_NctsExportFromEC.GetTrueOrNullIfFalse();

	public ZString ExportCountry => nctsAdditionalInfo?.CSI_RN_NKCountryCode ?? ZString.Empty;
}
