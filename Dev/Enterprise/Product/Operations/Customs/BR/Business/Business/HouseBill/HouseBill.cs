using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public partial class Bill : Customs.Business.Bill, Integration.Customs.BR.IBill
	{
		public Bill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
