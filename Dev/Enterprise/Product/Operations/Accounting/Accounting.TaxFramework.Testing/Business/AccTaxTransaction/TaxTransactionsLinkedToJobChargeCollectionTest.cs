using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	[TestedType(typeof(TaxTransactionsLinkedToJobChargeCollection))]
	public class TaxTransactionsLinkedToJobChargeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TaxTransactionsLinkedToJobChargeCollection>
	{
		public void TestDefaults()
		{
			var collection = GetCollectionToTest();
			Assert("AllowNew", !collection.AllowNew);
			Assert("AllowRemove", !collection.AllowRemove);
			AssertExceptionThrown<NotSupportedException>(() => collection.AddNew());
		}
		protected override TaxTransactionsLinkedToJobChargeCollection GetCollectionToTest()
		{
			return new TaxTransactionsLinkedToJobChargeCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TaxTransactionsLinkedToJobCharge();
		}
	}
}
