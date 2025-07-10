using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CN.Business
{
	public partial class Bill : Customs.Business.Bill, Integration.Customs.CN.IBill
	{
		public Bill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
