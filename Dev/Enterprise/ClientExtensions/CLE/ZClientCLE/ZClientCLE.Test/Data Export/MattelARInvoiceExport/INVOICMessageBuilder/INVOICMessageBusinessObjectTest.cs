using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.CLE.MattelARInvoiceExport.Testing
{
	public abstract class INVOICMessageBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected JobInvoiceRecord InvoiceRecord
		{
			get
			{
				return new JobInvoiceRecord(Factory.New<ForwardingShipment>(), Factory.New<BaseJobDeclaration>(), new InvoicingBase[1]);
			}
		}
	}
}
