using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(PeriodicInvoiceCollection))]
	public class PeriodicInvoiceCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PeriodicInvoiceCollection>
	{
		public void TestAddRemove()
		{
			Assert(!GetCollectionToTest().AllowNew);
			Assert(GetCollectionToTest().AllowRemove);
		}

		public void TestOnAdded()
		{
			PeriodicInvoiceCollection collection = new PeriodicInvoiceCollection(Factory);
			PeriodicInvoice periodicInvoice = (PeriodicInvoice)GetNewElementToAddToTheCollection();
			Assert(!periodicInvoice.IncludeInThePeriodicInvoice);
			collection.Add(periodicInvoice);
			Assert(periodicInvoice.IncludeInThePeriodicInvoice);
		}

		#region Implementation

		protected override PeriodicInvoiceCollection GetCollectionToTest()
		{
			return new PeriodicInvoiceCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PeriodicInvoice(Factory);
		}

		#endregion
	}
}
