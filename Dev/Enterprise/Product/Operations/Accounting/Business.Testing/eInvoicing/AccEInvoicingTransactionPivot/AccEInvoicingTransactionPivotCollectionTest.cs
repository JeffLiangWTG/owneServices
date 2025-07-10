using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.EInvoicing.Testing
{
	[TestedType(typeof(AccEInvoicingTransactionPivotCollection))]
	public class AccEInvoicingTransactionPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccEInvoicingTransactionPivotCollection(Factory.New<AccEInvoicingBatch>());
		}
	}
}
