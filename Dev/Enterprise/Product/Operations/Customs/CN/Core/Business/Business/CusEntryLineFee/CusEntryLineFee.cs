using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CN.Business
{
	public partial class CusEntryLineFee : AutoCusEntryLineFee, Integration.Customs.CN.ICusEntryLineFee
	{
		public CusEntryLineFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
