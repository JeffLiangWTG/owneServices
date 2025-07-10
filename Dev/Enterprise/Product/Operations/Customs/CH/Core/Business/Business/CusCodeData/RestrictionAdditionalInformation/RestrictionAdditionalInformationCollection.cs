using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class RestrictionAdditionalInformationCollection : CusCodeDataCollection<RestrictionAdditionalInformation>
{
	public RestrictionAdditionalInformationCollection(Restriction parent) : base(parent, CusCodeDataTypeList.Codes.RestrictionAdditionalInformation)
	{
	}
}
