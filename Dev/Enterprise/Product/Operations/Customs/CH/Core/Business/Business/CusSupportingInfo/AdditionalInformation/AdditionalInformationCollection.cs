using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class AdditionalInformationCollection : CusSupportingInfoCollection<AdditionalInformation>
{
	public AdditionalInformationCollection(BusinessObject parent) : base(parent, Common.CH.CusSupportingInfoTypeList.Codes.AdditionalInformation)
	{
	}
}
