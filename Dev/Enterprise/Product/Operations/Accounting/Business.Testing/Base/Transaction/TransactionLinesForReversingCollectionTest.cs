using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.WIPAccrual;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.Base.Transaction
{
	[TestedType(typeof(TransactionLinesForReversingCollection))]
	public class TransactionLinesForReversingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TransactionLinesForReversingCollection>
	{
		public void TestAllowNewCore()
		{
			var testCollection = new TransactionLinesForReversingCollection(Factory);
			Assert("User should not be able to add elements to this collection.", !testCollection.AllowNew);
		}

		public void TestAllowRemoveCore()
		{
			var testCollection = new TransactionLinesForReversingCollection(Factory);
			Assert("User should not be able to remove elements from this collection.", !testCollection.AllowRemove);
		}

		public void TestAddNewCore()
		{
			var testCollection = new TransactionLinesForReversingCollection(Factory);
			AssertExceptionThrown(typeof(NotSupportedException), "Allow new is false so shouldn't get called", () => testCollection.AddNew());
		}

		#region Implementation

		protected override TransactionLinesForReversingCollection GetCollectionToTest()
		{
			return new TransactionLinesForReversingCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var wip = Factory.New<WIP>();
			return new TransactionLineForReversing(wip);
		}

		#endregion

	}
}
