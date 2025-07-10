using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class HouseConsignmentDifferencesCollection : SingleCusCodeDataCollection<HouseConsignmentDifferences>
{
	public HouseConsignmentDifferencesCollection(BusinessObject parent) : base(parent, CusCodeDataTypeList.Codes.UnloadingRemarks)
	{
	}
}
