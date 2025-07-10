using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Client.CLE.MattelARInvoiceExport.Testing
{
	[TestedType(typeof(AUINVOICMessageBuilder))]
	public class AUINVOICMessageBuilderBusinessObjectTest : INVOICMessageBusinessObjectTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new AUINVOICMessageBuilder(base.InvoiceRecord, new BusinessObjectFactory());
		}
	}
}
