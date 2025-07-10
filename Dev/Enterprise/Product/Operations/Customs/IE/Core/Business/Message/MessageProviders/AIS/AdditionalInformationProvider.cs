using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public class AdditionalInformationProvider : IAdditionalInformation
	{
		public static AdditionalInformationProvider New(CusSupportingInfo supportingInfo)
		{
			AdditionalInformationProvider result = null;
			if (supportingInfo != null)
			{
				result = new AdditionalInformationProvider(supportingInfo.CSI_Code, supportingInfo.CSI_Description);
			}
			return result;
		}

		AdditionalInformationProvider(string code, string text)
		{
			Code = code;
			Text = text;
		}

		public string Code { get; }

		public string Text { get; }
	}
}
