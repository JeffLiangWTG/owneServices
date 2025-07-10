using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class CargoManagementNumberBOCollection : CusCodeDataCollection<CargoManagementNumberBO>
	{
		public CargoManagementNumberBOCollection(Bill parent)
			: base(parent, CusCodeDataTypeList.Codes.CargoManagementNumber)
		{
		}
	}
}
