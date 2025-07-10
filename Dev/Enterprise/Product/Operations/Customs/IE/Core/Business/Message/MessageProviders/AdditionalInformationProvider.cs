using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IE.Business
{
	public class AdditionalInformationProvider : CusSupportingInfoProvider, IAdditionalInformation
	{
		public static AdditionalInformationProvider New(AdditionalInfo additionalInfo)
		{
			AdditionalInformationProvider result = null;
			if (additionalInfo != null)
			{
				result = new AdditionalInformationProvider(additionalInfo);
			}
			return result;
		}

		public AdditionalInformationProvider(AdditionalInfo additionalInfo) : base(additionalInfo) { }

		public string Code => cusSupportingInfo.CSI_Code;

		public string Text => cusSupportingInfo.CSI_Description;
	}
}
