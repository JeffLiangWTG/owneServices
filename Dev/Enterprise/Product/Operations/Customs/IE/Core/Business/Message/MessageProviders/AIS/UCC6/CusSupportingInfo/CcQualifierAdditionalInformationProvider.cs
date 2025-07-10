using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class CcQualifierAdditionalInformationProvider : CusSupportingInfoProvider, ICcQualifierAdditionalInformation
	{
		public CcQualifierAdditionalInformationProvider(CusSupportingInfo additionalInfo) : base(additionalInfo) { }

		public string CcQualifier => null;

		public string Code => cusSupportingInfo.CSI_Code;

		public string Text => cusSupportingInfo.CSI_Description;
	}
}
