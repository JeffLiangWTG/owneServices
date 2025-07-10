using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.Business
{
	public partial class CusEntryLineFee : AutoCusEntryLineFee
	{
		public CusEntryLineFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
