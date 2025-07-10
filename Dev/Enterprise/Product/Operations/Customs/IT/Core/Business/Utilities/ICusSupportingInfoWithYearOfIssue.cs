using CargoWise.EntityFramework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.IT.Business;

public interface ICusSupportingInfoWithYearOfIssue : ICusSupportingInfo
{
	ZPropertyInfo CSI_YearOfIssueInfo { get; }
}
