using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	using Enterprise.Accounting.Business.Base.Transaction.Testing;
	using NUnit.Framework;

	[TestedType(typeof(InvoicingBaseCollectionForModule))]
	public class InvoicingBaseCollectionForModuleTest : TransactionHeaderCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new InvoicingBaseCollectionForModule(Factory);
		}
	}
}
