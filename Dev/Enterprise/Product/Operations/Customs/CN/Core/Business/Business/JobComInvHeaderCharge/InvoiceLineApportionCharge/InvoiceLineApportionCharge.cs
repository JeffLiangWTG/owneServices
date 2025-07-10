using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public partial class InvoiceLineApportionCharge : AutoInvoiceLineApportionCharge, Integration.Customs.CN.IInvoiceLineApportionCharge
	{
		public InvoiceLineApportionCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZBool AllowNonWesternEuropeanCharacterForChargeDescription => true;
	}
}
