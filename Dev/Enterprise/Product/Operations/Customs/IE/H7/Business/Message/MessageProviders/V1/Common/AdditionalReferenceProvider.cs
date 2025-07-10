using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class AdditionalReferenceProvider : CusSupportingInfoProvider, IAdditionalReference
	{
		public AdditionalReferenceProvider(CusSupportingInfo cusSupportingInfo) : base(cusSupportingInfo)
		{
		}

		public static AdditionalReferenceProvider NewOrNull(CusSupportingInfo supportingInfo) => supportingInfo is null ? null : new AdditionalReferenceProvider(supportingInfo);

		public string Type => cusSupportingInfo.CSI_Code;

		public string Number => cusSupportingInfo.CSI_ReferenceNumber;
	}
}
