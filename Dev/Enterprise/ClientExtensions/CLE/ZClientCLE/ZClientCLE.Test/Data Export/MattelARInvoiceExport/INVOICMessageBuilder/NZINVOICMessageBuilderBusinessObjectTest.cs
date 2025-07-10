using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Client.CLE.MattelARInvoiceExport.Testing
{
	[TestedType(typeof(NZINVOICMessageBuilder))]
	public class NZINVOICMessageBuilderBusinessObjectTest : INVOICMessageBusinessObjectTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new NZINVOICMessageBuilder(base.InvoiceRecord, new BusinessObjectFactory());
		}
	}
}
