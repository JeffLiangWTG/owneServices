using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.MX.Business
{
	public partial class CusEntryHeaderCharges : AutoCusEntryHeaderCharges, Integration.Customs.MX.ICusEntryHeaderCharges
	{
		public CusEntryHeaderCharges(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
