using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryLineConfirmedFeeWrapperCollection))]
	sealed class CusEntryLineConfirmedFeeWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CusEntryLineConfirmedFeeWrapperCollection>
	{
		public void TestCollectionCreatedFromConfirmedFeesProperty()
		{
			var cusEntryLineFeeWrapperCollection = new CusEntryLineConfirmedFeeWrapperCollection(entryLine);
			AssertEquals("cusEntryLineFeeWrapperReadonlyCollection.Count", 2, cusEntryLineFeeWrapperCollection.Count);
			CombineAssertions(() =>
			{
				AssertEquals("ConfirmedFees[0].PK", entryLine.ConfirmedFees[0].PK, cusEntryLineFeeWrapperCollection[0].PK);
				AssertEquals("ConfirmedFees[1].PK", entryLine.ConfirmedFees[1].PK, cusEntryLineFeeWrapperCollection[1].PK);
			});
		}

		public void TestCreateNonPersistentBusinessObject()
		{
			var cusEntryLineFeeWrapperCollection = new CusEntryLineConfirmedFeeWrapperCollection(entryLine);
			AssertExceptionThrown<InvalidOperationException>("Cannot AddNew", "It is not possible to create a new object from this collection", () => cusEntryLineFeeWrapperCollection.AddNew());
		}

		public void TestAllowNewCore()
		{
			var cusEntryLineFeeWrapperCollection = new CusEntryLineConfirmedFeeWrapperCollection(entryLine);
			AssertEquals(false, cusEntryLineFeeWrapperCollection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var cusEntryLineFeeWrapperCollection = new CusEntryLineConfirmedFeeWrapperCollection(entryLine);
			AssertEquals(false, cusEntryLineFeeWrapperCollection.AllowRemove);
		}

		protected override void SetUp()
		{
			base.SetUp();
			entryLine = Factory.New<CusEntryLine>();
			var entryLineFee1 = Factory.New<CusEntryLineFee>();
			var entryLineFee2 = Factory.New<CusEntryLineFee>();
			entryLine.ConfirmedFees.Add(entryLineFee1);
			entryLine.ConfirmedFees.Add(entryLineFee2);
		}
		CusEntryLine entryLine;

		protected override CusEntryLineConfirmedFeeWrapperCollection GetCollectionToTest() => new CusEntryLineConfirmedFeeWrapperCollection(entryLine);

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CusEntryLineFee>();
	}
}
