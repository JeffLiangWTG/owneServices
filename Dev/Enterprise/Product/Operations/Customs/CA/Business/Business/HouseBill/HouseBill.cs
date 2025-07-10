using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public partial class Bill : AutoBill
	{
		public Bill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
