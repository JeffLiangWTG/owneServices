using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public partial class GroupInvoiceCharge : AutoGroupInvoiceCharge, Integration.Customs.CN.IGroupInvoiceCharge
	{
		public GroupInvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZBool AllowNonWesternEuropeanCharacterForChargeDescription => true;
	}
}
