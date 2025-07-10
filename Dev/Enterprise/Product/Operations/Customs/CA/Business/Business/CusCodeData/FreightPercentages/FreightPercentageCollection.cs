using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public class FreightPercentageCollection : CusCodeDataCollection<FreightPercentage>
	{
		public FreightPercentageCollection(BusinessObject master)
			: base(master, CusCodeDataTypeList.Codes.FreightPercentage)
		{
		}
	}
}
