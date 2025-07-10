using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class InvoiceLineApportionCharge : EU.Business.Declaration.InvoiceLineApportionCharge, Integration.Customs.ES.IInvoiceLineApportionCharge
	{
		public InvoiceLineApportionCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
