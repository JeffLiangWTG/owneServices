using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using NUnit.Framework;
using static Enterprise.DocumentWrappers.DocJobInvoicingJob;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(WIPACRMovementsRecognized))]
	sealed class WIPACRMovementsRecognizedTest : NonPersistentBusinessObjectCollectionTestCase<WIPACRMovementsRecognized>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			APInvoiceLine invoice = Factory.New<APInvoiceLine>();
			return WIPACRDummyLine.New(invoice, Factory);
		}

		protected override WIPACRMovementsRecognized GetCollectionToTest()
		{
			return new WIPACRMovementsRecognized(Factory);
		}
	}
}
