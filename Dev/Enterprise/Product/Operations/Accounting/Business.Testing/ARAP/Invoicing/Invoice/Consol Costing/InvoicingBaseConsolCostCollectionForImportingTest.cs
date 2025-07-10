using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoicingBaseConsolCostCollectionForImporting))]
	public class InvoicingBaseConsolCostCollectionForImportingTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new InvoicingBaseConsolCostCollectionForImporting(Factory);
		}
	}
}
