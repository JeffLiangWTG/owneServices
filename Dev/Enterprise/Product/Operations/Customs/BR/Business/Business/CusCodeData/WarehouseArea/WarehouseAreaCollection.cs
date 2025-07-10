using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class WarehouseAreaCollection : CusCodeDataCollection<WarehouseArea>
	{
		public WarehouseAreaCollection(JobDeclaration parent)
			: base(parent, CusCodeDataTypeList.Codes.WarehouseArea)
		{
		}
	}
}
