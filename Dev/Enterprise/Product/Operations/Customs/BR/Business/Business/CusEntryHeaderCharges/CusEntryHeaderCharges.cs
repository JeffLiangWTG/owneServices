using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public partial class CusEntryHeaderCharges : AutoCusEntryHeaderCharges, Integration.Customs.BR.ICusEntryHeaderCharges
	{
		public CusEntryHeaderCharges(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
