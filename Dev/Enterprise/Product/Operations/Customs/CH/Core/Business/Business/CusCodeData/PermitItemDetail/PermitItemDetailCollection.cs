using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class PermitItemDetailCollection : CusCodeDataCollection<PermitItemDetail>
{
	public PermitItemDetailCollection(Permit parent) : base(parent, CusCodeDataTypeList.Codes.PermitItemDetails)
	{
	}
}
