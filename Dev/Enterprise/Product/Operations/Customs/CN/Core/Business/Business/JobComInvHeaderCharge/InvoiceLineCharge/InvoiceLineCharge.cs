using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public partial class InvoiceLineCharge : AutoInvoiceLineCharge, Integration.Customs.CN.IInvoiceLineCharge
	{
		public InvoiceLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZBool AllowNonWesternEuropeanCharacterForChargeDescription => true;
	}
}
